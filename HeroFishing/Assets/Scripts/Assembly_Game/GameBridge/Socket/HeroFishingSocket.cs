using DG.Tweening;
using HeroFishing.Main;
using LitJson;
using Scoz.Func;
using Service.Realms;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HeroFishing.Socket {
    public partial class HeroFishingSocket {

        protected static HeroFishingSocket Instance = null;
        public static HeroFishingSocket GetInstance() {
            if (Instance == null) {
                Instance = new HeroFishingSocket();
                Instance.Init();
            }
            return Instance;
        }


        public void Init() {
        }
        public void Release() {
            if (Instance != null)
                Instance.Dispose();
            Instance = null;
        }
        public void Dispose() {
            if (TCP_MatchmakerClient != null)
                TCP_MatchmakerClient.Close();
            if (TCP_MatchgameClient != null)
                TCP_MatchgameClient.Close();
            if (UDP_MatchgameClient != null)
                UDP_MatchgameClient.Close();
            if (TimeSyncer != null)
                GameObject.Destroy(TimeSyncer.gameObject);
            if (LogInSubject != null)
                LogInSubject.Dispose();
            if (CreateRoomSubject != null)
                CreateRoomSubject.Dispose();
        }

        private void CreateClientObject<T>(ref T client, string ip, int port, string progress, string name) where T : MonoBehaviour, INetworkClient {
            if (client != null) {
                WriteLog.LogColor($"{progress}時 {name}不為null, 關閉 {name}", WriteLog.LogType.Connection);
                client.Close();
            }
            client = new GameObject(name).AddComponent<T>();
            client.Init(ip, port);
        }

    }
}
