using Cysharp.Threading.Tasks;
using HeroFishing.Battle;
using HeroFishing.Main;
using HeroFishing.Socket.Matchgame;
using LitJson;
using Scoz.Func;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HeroFishing.Socket {
    public partial class HeroFishingSocket {
        TcpClient TCP_MatchgameClient;
        UdpSocket UDP_MatchgameClient;
        ServerTimeSyncer TimeSyncer;
        string UDP_MatchgameConnToken;// 連Matchgame需要的Token，由AUTH_REPLY時取得

        Dictionary<Tuple<string, int>, Action<string>> CMDCallback = new Dictionary<Tuple<string, int>, Action<string>>();

        readonly Subject<Unit> JoinRoomSubject = new Subject<Unit>();
        public IObservable<Unit> JoinRoomObservable => JoinRoomSubject;

        public int GetMatchgamePing() {
            if (TimeSyncer)
                return Mathf.RoundToInt(TimeSyncer.GetLantency() * 1000);
            return 0;
        }
        void RegisterMatchgameCommandCB(string _command, int _packetID, Action<string> _ac) {
            var cmdID = new Tuple<string, int>(_command, _packetID);
            if (CMDCallback.ContainsKey(cmdID)) {
                WriteLog.LogError("Command remain here should not happen.");
                CMDCallback.Remove(cmdID);
            }
            CMDCallback.Add(cmdID, _ac);
        }
        public void JoinMatchgame(Action _onDisconnected, string _realmToken, string _tcpIP, string _udpIP, int _port) {
            CreateClientObject(ref TCP_MatchgameClient, _tcpIP, _port, "JoinMatchgame", "TCP_MatchgameClient");
            CreateClientObject(ref UDP_MatchgameClient, _udpIP, _port, "JoinMatchgame", "UDP_MatchgameClient");
            TCP_MatchgameClient.OnReceiveMsg += OnRecieveMatchgameTCPMsg;

            TCP_MatchgameClient.StartConnect((bool connected) => OnMatchgameTCPConnect(connected, _realmToken));
            TCP_MatchgameClient.RegistOnDisconnect(_onDisconnected);
        }

        public void OnMatchgameUDPDisconnect() {
            UDP_MatchgameClient.OnReceiveMsg -= OnRecieveMatchgameUDPMsg;
            if (UDP_MatchgameClient != null) {
                this.MatchgameUDPEndWithDisconnection();
                return;
            }

            UDP_MatchgameClient.Close();
            UDP_MatchgameClient = new GameObject("GameUdpSocket").AddComponent<UdpSocket>();
            UniTask.Void(async () => {
                try {
                    var dbMatchgame = await GamePlayer.Instance.GetMatchGame();
                    if (dbMatchgame == null) {
                        WriteLog.LogError("OnMatchgameUDPDisconnect時重連失敗，dbMatchgame is null");
                        this.MatchgameUDPEndWithDisconnection();
                        return;
                    } else UDP_MatchgameClient.Init(dbMatchgame.IP, dbMatchgame.Port ?? 0);
                } catch (Exception _e) {
                    WriteLog.LogError("OnMatchgameUDPDisconnect時嘗試重連失敗: " + _e);
                    OnMatchgameUDPDisconnect();
                    return;
                }
                UDP_MatchgameClient.StartConnect(UDP_MatchgameConnToken, (bool connected) => {
                    WriteLog.LogColor("OnMatchgameUDPDisconnect後重連結果 :" + connected, WriteLog.LogType.Connection);
                    if (connected) {
                        UDP_MatchgameClient.OnReceiveMsg += OnRecieveMatchgameUDPMsg;
                    } else {
                        this.MatchgameUDPEndWithDisconnection();
                        return;
                    }
                });
                UDP_MatchgameClient.RegistOnDisconnect(OnMatchgameUDPDisconnect);
            });
        }
        public void MatchgameUDPEndWithDisconnection() {
            WriteLog.LogColor("MatchgameUDPEndWithDisconnection", WriteLog.LogType.Connection);
        }
        public int UDPSend<T>(SocketCMD<T> cmd) where T : SocketContent {
            cmd.SetConnToken(UDP_MatchgameConnToken);//設定封包ConnToken
            if (string.IsNullOrEmpty(cmd.ConnToken)) {
                WriteLog.LogError("UDP封包的ConnToken不可為空");
                return -1;
            }
            return UDP_MatchgameClient.Send(cmd);
        }
        public int TCPSend<T>(SocketCMD<T> cmd) where T : SocketContent {
            return TCP_MatchgameClient.Send(cmd);
        }


        private void OnMatchgameTCPConnect(bool connected, string realmToken) {
            WriteLog.LogColor($"TCP_MatchgameClient connection: {connected}", WriteLog.LogType.Connection);
            if (!connected) {
                JoinRoomSubject?.OnError(new Exception("client connection error"));
                return;
            }
            // Close Matchmaker client
            if (TCP_MatchmakerClient != null) {
                TCP_MatchmakerClient.OnReceiveMsg -= OnRecieveMatchmakerTCPMsg;
                TCP_MatchmakerClient.Close();
                WriteLog.LogColor($"JoinMatchgame成功後 TCP_MatchmakerClient不需要了關閉 {TCP_MatchmakerClient}", WriteLog.LogType.Connection);
            }
            SocketCMD<AUTH> cmd = new SocketCMD<AUTH>(new AUTH(realmToken));

            int id = TCP_MatchgameClient.Send(cmd);
            if (id < 0) {
                JoinRoomSubject?.OnError(new Exception("packet id < 0"));
                return;
            }
            RegisterMatchgameCommandCB(SocketContent.MatchgameCMD_TCP.AUTH_TOCLIENT.ToString(), id, TCPClientCmdCallback);
        }

        private void TCPClientCmdCallback(string msg) {
            SocketCMD<AUTH_TOCLIENT> packet = LitJson.JsonMapper.ToObject<SocketCMD<AUTH_TOCLIENT>>(msg);
            if (packet.Content.IsAuth) {
                try {
                    ConnectUDPMatchgame(packet.Content.ConnToken, packet.Content.Index);

                    JoinRoomSubject?.OnNext(Unit.Default);
                } catch (Exception e) {
                    WriteLog.LogError($"UDP error: " + e);
                    JoinRoomSubject?.OnError(e);
                }
                if (TimeSyncer == null)
                    TimeSyncer = new GameObject("TimeSyncer").AddComponent<ServerTimeSyncer>();
                TimeSyncer.StartCountTime();
            } else {
                JoinRoomSubject?.OnError(new Exception("auth is invalid"));
            }
        }

        private void ConnectUDPMatchgame(string connToken, int index) {
            //取得Matchgame Auth的回傳結果 UDP socket的ConnToken與遊戲房間的座位索引
            WriteLog.LogColor($"Matchgame auth success! UDP_MatchgameConnToken: {UDP_MatchgameConnToken}", WriteLog.LogType.Connection);
            UDP_MatchgameConnToken = connToken;
            AllocatedRoom.Instance.SetPlayerIndex(index);

            //取得ConnToken後就能進行UDP socket連線
            UDP_MatchgameClient.StartConnect(UDP_MatchgameConnToken, (bool connected) => {
                WriteLog.LogColor($"UDP Is connected: {connected}", WriteLog.LogType.Connection);
                if (connected) {
                    UDP_MatchgameClient.OnReceiveMsg += OnRecieveMatchgameUDPMsg;
                } else {
                    WriteLog.LogError("UDP連線失敗");
                    OnMatchgameUDPDisconnect();
                }
                UDP_MatchgameClient.RegistOnDisconnect(OnMatchgameUDPDisconnect);
            });

        }

        public void OnRecieveMatchgameUDPMsg(string _msg) {
            try {
                SocketCMD<SocketContent> data = JsonMapper.ToObject<SocketCMD<SocketContent>>(_msg);
                SocketContent.MatchgameCMD_UDP cmdType;
                if (!MyEnum.TryParseEnum(data.CMD, out cmdType)) {
                    WriteLog.LogErrorFormat("收到錯誤的命令類型: {0}", cmdType);
                    return;
                }
                WriteLog.LogColorFormat("(UDP)接收: {0}", WriteLog.LogType.Connection, _msg);
                switch (cmdType) {
                    // Attack改使用TCP傳
                    //case SocketContent.MatchgameCMD_UDP.ATTACK_TOCLIENT:
                    //    var attackPacket = LitJson.JsonMapper.ToObject<SocketCMD<ATTACK_TOCLIENT>>(_msg);
                    //    HandleAttack(attackPacket);
                    //    break;
                    case SocketContent.MatchgameCMD_UDP.UPDATEGAME_TOCLIENT:
                        var updateGamePacket = LitJson.JsonMapper.ToObject<SocketCMD<UPDATEGAME_TOCLIENT>>(_msg);
                        HandleUpdateGame(updateGamePacket);
                        break;
                    case SocketContent.MatchgameCMD_UDP.UPDATEPLAYER_TOCLIENT:
                        var updatePlayerPacket = LitJson.JsonMapper.ToObject<SocketCMD<UPDATEPLAYER_TOCLIENT>>(_msg);
                        HandleUpdatePlayer(updatePlayerPacket);
                        break;
                    case SocketContent.MatchgameCMD_UDP.UPDATESCENE_TOCLIENT:
                        var updateScenePacket = LitJson.JsonMapper.ToObject<SocketCMD<UPDATESCENE_TOCLIENT>>(_msg);
                        HandleUpdateScene(updateScenePacket);
                        break;
                    default:
                        WriteLog.LogErrorFormat("收到尚未定義的命令類型: {0}", cmdType);
                        break;
                }
            } catch (Exception e) {
                WriteLog.LogError("Parse UDP Message with Error : " + e.ToString());
            }
        }

        private void OnRecieveMatchgameTCPMsg(string _msg) {
            try {

                WriteLog.LogColorFormat("(TCP)接收: {0}", WriteLog.LogType.Connection, _msg);
                SocketCMD<SocketContent> data = JsonMapper.ToObject<SocketCMD<SocketContent>>(_msg);
                Tuple<string, int> cmdID = new Tuple<string, int>(data.CMD, data.PackID);
                SocketContent.MatchgameCMD_TCP cmdType;
                if (!MyEnum.TryParseEnum(data.CMD, out cmdType)) {
                    WriteLog.LogErrorFormat("收到錯誤的命令類型: {0}", cmdType);
                    return;
                }
                if (CMDCallback.TryGetValue(cmdID, out Action<string> _cb)) {
                    CMDCallback.Remove(cmdID);
                    _cb?.Invoke(_msg);
                }
                switch (cmdType) {
                    case SocketContent.MatchgameCMD_TCP.AUTH_TOCLIENT:
                        var authPacket = LitJson.JsonMapper.ToObject<SocketCMD<AUTH_TOCLIENT>>(_msg);
                        HandleAuth(authPacket);
                        break;
                    case SocketContent.MatchgameCMD_TCP.ATTACK_TOCLIENT:
                        var attackPacket = LitJson.JsonMapper.ToObject<SocketCMD<ATTACK_TOCLIENT>>(_msg);
                        HandleAttack(attackPacket);
                        break;
                    case SocketContent.MatchgameCMD_TCP.SPAWN_TOCLIENT:
                        var spawnPacket = LitJson.JsonMapper.ToObject<SocketCMD<SPAWN_TOCLIENT>>(_msg);
                        HandleSPAWN(spawnPacket);
                        break;
                    case SocketContent.MatchgameCMD_TCP.SETHERO_TOCLIENT:
                        var setHeroPacket = LitJson.JsonMapper.ToObject<SocketCMD<SETHERO_TOCLIENT>>(_msg);
                        HandleSETHERO(setHeroPacket);
                        break;
                    case SocketContent.MatchgameCMD_TCP.UPDATEPLAYER_TOCLIENT:
                        var updatePlaeyrPacket = LitJson.JsonMapper.ToObject<SocketCMD<UPDATEPLAYER_TOCLIENT>>(_msg);
                        HandleUpdatePlayer(updatePlaeyrPacket);
                        break;
                    case SocketContent.MatchgameCMD_TCP.HIT_TOCLIENT:
                        //WriteLog.LogColorFormat("(TCP)接收: {0}", WriteLog.LogType.Connection, _msg);
                        var hitPacket = LitJson.JsonMapper.ToObject<SocketCMD<HIT_TOCLIENT>>(_msg);
                        HandleHit(hitPacket);
                        break;
                    case SocketContent.MatchgameCMD_TCP.UPDATESCENE_TOCLIENT:
                        //WriteLog.LogColorFormat("(TCP)接收: {0}", WriteLog.LogType.Connection, _msg);
                        var updateScenePacket = LitJson.JsonMapper.ToObject<SocketCMD<UPDATESCENE_TOCLIENT>>(_msg);
                        HandleUpdateScene(updateScenePacket);
                        break;
                    case SocketContent.MatchgameCMD_TCP.MONSTERDIE_TOCLIENT:
                        //WriteLog.LogColorFormat("(TCP)接收: {0}", WriteLog.LogType.Connection, _msg);
                        var monsterDiePacket = LitJson.JsonMapper.ToObject<SocketCMD<MONSTERDIE_TOCLIENT>>(_msg);
                        HandleMonsterDie(monsterDiePacket);
                        break;
                    case SocketContent.MatchgameCMD_TCP.AUTO_TOCLIENT:
                        var autoPacket = JsonMapper.ToObject<SocketCMD<AUTO_TOCLIENT>>(_msg);
                        HandleAuto(autoPacket);
                        break;
                    case SocketContent.MatchgameCMD_TCP.LEAVE_TOCLIENT:
                        var leavePacket = JsonMapper.ToObject<SocketCMD<LEAVE_TOCLIENT>>(_msg);
                        HandleLeave(leavePacket);
                        break;
                    case SocketContent.MatchgameCMD_TCP.LVUPSPELL_TOCLIENT:
                        var lvupSpellPacket = JsonMapper.ToObject<SocketCMD<LVUPSPELL_TOCLIENT>>(_msg);
                        HandleLvUpSpell(lvupSpellPacket);
                        break;
                    case SocketContent.MatchgameCMD_TCP.DROPSPELL_TOCLIENT:
                        var dropSpellPacket = JsonMapper.ToObject<SocketCMD<DROPSPELL_TOCLIENT>>(_msg);
                        HandleDropSepll(dropSpellPacket);
                        break;
                    case SocketContent.MatchgameCMD_TCP.ADDBOT_TOCLIENT:
                        var addBotPacket = JsonMapper.ToObject<SocketCMD<ADDBOT_TOCLIENT>>(_msg);
                        HandleAddBot(addBotPacket);
                        break;
                    default:
                        WriteLog.LogErrorFormat("收到尚未定義的命令類型: {0}", cmdType);
                        break;
                }
            } catch (Exception e) {
                WriteLog.LogError("Parse收到的封包時出錯 : " + e.ToString());
                if (SceneManager.GetActiveScene().name != MyScene.BattleScene.ToString()) {
                    WriteLog.LogErrorFormat("不在{0}就釋放資源: ", MyScene.BattleScene, e.ToString());
                    Release();
                }
            }
        }

        void HandleSPAWN(SocketCMD<SPAWN_TOCLIENT> _packet) {
            if (SceneManager.GetActiveScene().name != MyScene.BattleScene.ToString()) return;
            if (BattleManager.Instance == null || BattleManager.Instance.MyMonsterScheduler == null) return;
            BattleManager.Instance.MyMonsterScheduler.EnqueueMonster(_packet.Content.MonsterIDs, _packet.Content.MonsterIdxs, _packet.Content.RouteID, _packet.Content.IsBoss, (float)_packet.Content.SpawnTime, BattleManager.Instance.Index);
        }

        void HandleSETHERO(SocketCMD<SETHERO_TOCLIENT> _packet) {
            if (SceneManager.GetActiveScene().name != MyScene.BattleScene.ToString()) return;
            AllocatedRoom.Instance.SetHeros(_packet.Content.HeroIDs, _packet.Content.HeroSkinIDs);
            BattleManager.Instance?.UpdateHeros();
        }

        void HandleHit(SocketCMD<HIT_TOCLIENT> _packet) {
            if (SceneManager.GetActiveScene().name != MyScene.BattleScene.ToString()) return;
            if (!string.IsNullOrEmpty(_packet.ErrMsg))
                WriteLog.LogError("Hit Error: " + _packet.ErrMsg);
            if (_packet.Content.KillMonsterIdxs == null || _packet.Content.KillMonsterIdxs.Length == 0) return;
            BattleManager.Instance?.SetMonsterDead(_packet.Content.PlayerIdx, _packet.Content.KillMonsterIdxs, _packet.Content.GainPoints, _packet.Content.GainHeroExps, _packet.Content.GainSpellCharges, _packet.Content.GainDrops);
            //WriteLog.Log(DebugUtils.EnumerableToStr(_packet.Content.KillMonsterIdxs));
            //WriteLog.Log(DebugUtils.EnumerableToStr(_packet.Content.GainGolds));
        }
        void HandleAuth(SocketCMD<AUTH_TOCLIENT> _packet) {
            //if (SceneManager.GetActiveScene().name != MyScene.BattleScene.ToString()) return;
            if (_packet.Content == null || !_packet.Content.IsAuth) {
                WriteLog.LogError("Auth錯誤 遊戲無法開始");
                return;
            }
            AllocatedRoom.Instance.SetGameState(AllocatedRoom.GameState.Playing);
            BattleManager.Instance?.StartGame();
        }
        void HandleAttack(SocketCMD<ATTACK_TOCLIENT> _packet) {
            if (SceneManager.GetActiveScene().name != MyScene.BattleScene.ToString()) return;
            //WriteLog.Log(DebugUtils.ObjToStr(_packet.Content));
            BattleManager.Instance?.Attack(_packet.Content);
        }
        void HandleUpdateGame(SocketCMD<UPDATEGAME_TOCLIENT> _packet) {
            if (SceneManager.GetActiveScene().name != MyScene.BattleScene.ToString()) return;
            //WriteLog.Log("GameTime=" + _packet.Content.GameTime);
            GameTime.Override((float)_packet.Content.GameTime);
            var cmdContent = new UPDATEGAME();//建立封包內容
            var cmd = new SocketCMD<UPDATEGAME>(cmdContent);//建立封包
            UDPSend(cmd);
        }
        void HandleUpdatePlayer(SocketCMD<UPDATEPLAYER_TOCLIENT> _packet) {
            if (SceneManager.GetActiveScene().name != MyScene.BattleScene.ToString()) return;
            List<Player> players = new List<Player>();
            for (int i = 0; i < _packet.Content.Players.Length; i++) {
                var player = _packet.Content.Players[i];
                if (player == null) continue;
                players.Add(player);
            }
            players.Sort((a, b) => {
                if (b.GainPoints == a.GainPoints) {
                    return a.Idx - b.Idx;
                }
                return (int)(b.GainPoints - a.GainPoints);
            });

            var rankUI = BaseUI.GetInstance<RankUI>();
            if (rankUI != null)
                rankUI.SetRank(players.Select(p => p.Idx).ToArray());
        }
        void HandleUpdateScene(SocketCMD<UPDATESCENE_TOCLIENT> _packet) {
            if (SceneManager.GetActiveScene().name != MyScene.BattleScene.ToString()) return;
            if (CDChecker.DoneCD("UpdateScene", 1f))
                BattleManager.Instance?.UpdateScene(_packet.Content.Spawns, _packet.Content.SceneEffects);

            if (UpdateSceneGizmos.Instance != null)
                UpdateSceneGizmos.Instance.SetUpdateScene(_packet.Content);
        }
        void HandleMonsterDie(SocketCMD<MONSTERDIE_TOCLIENT> _packet) {
            if (SceneManager.GetActiveScene().name != MyScene.BattleScene.ToString()) return;
        }

        void HandleAuto(SocketCMD<AUTO_TOCLIENT> _packet) {
            if (SceneManager.GetActiveScene().name != MyScene.BattleScene.ToString()) return;
            Debug.Log(_packet.Content.IsAuto);
        }

        void HandleLeave(SocketCMD<LEAVE_TOCLIENT> _packet) {
            if (SceneManager.GetActiveScene().name != MyScene.BattleScene.ToString()) return;
            //Debug.Log(_packet.Content.PlayerIdx);
            AllocatedRoom.Instance.SetHero(_packet.Content.PlayerIdx, 0, string.Empty);
            BattleManager.Instance?.UpdateHeros();
        }
        void HandleLvUpSpell(SocketCMD<LVUPSPELL_TOCLIENT> _packet) {
            if (SceneManager.GetActiveScene().name != MyScene.BattleScene.ToString()) return;
            WriteLog.Log("技能升級結果:" + _packet.Content.Success);
        }
        void HandleDropSepll(SocketCMD<DROPSPELL_TOCLIENT> _packet) {
            if (SceneManager.GetActiveScene().name != MyScene.BattleScene.ToString()) return;
            WriteLog.Log("施放掉落技能:" + _packet.Content.Success);
            var playerIdx = _packet.Content.PlayerIdx;
            var heroIndex = BattleManager.Instance.GetHeroIndex(playerIdx);
            DropManager.Instance.PlayDrop(heroIndex, _packet.Content.DropSpellJsonID);
        }
        void HandleAddBot(SocketCMD<ADDBOT_TOCLIENT> _packet) {
            if (SceneManager.GetActiveScene().name != MyScene.BattleScene.ToString()) return;
        }

    }
}
