using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using LitJson;
using Unity.Collections;
using Unity.Burst;
using System.Linq;

namespace HeroFishing.Main {
    public enum SpellName {
        attack = 0,
        spell1 = 1,
        spell2 = 2,
        spell3 = 3,
    }
    public class HeroSpellJsonData : MyJsonData {
        public SpellBase Spell;

        public enum SpellType {
            LineShot,//(直線飛射)： [指示物長度, 子彈寬度, 子彈速度, 生命週期]
            SpreadLineShot,//(錐形直線飛射)：[指示物長度, 子彈寬度, 子彈速度, 散射間隔角度, 散射數量, 生命週期]
            CircleArea,
            SectorArea,
            //LineRange,//(直線飛射穿透)：[指示物長度, 子彈寬度, 子彈速度, 生命週期]
            //LineRangeInstant,//(直線立即範圍)：[指示物長度, 子彈寬度, 生命週期]
        }

        public enum DragType {
            Rot, Mov
        }

        public enum HitType {
            None,
            Chain,
            Explode
        }

        public enum MoveType {
            None,
            Target
        }
        public static string DataName { get; set; }
        public new string ID { get; private set; }
        public int HeroID { get; private set; }
        public string Name {
            get {
                return StringJsonData.GetString_static(DataName + "_" + ID, "Name");
            }
        }
        public string Description {
            get {
                return StringJsonData.GetString_static(DataName + "_" + ID, "Description");
            }
        }
        public string Ref { get; private set; }
        public SpellName SpellName { get; private set; }
        public float[] RTP { get; private set; }
        public float CD { get; private set; }
        public int Cost { get; private set; }
        public int Waves { get; private set; }
        public int WaveHits { get; private set; }
        public int MaxHits { get; private set; }
        public DragType MyDragType { get; private set; }
        public bool DestroyOnCollision { get; private set; }
        public SpellType MySpellType { get; private set; }
        public string[] SpellTypeValues { get; private set; }
        public HitType MyHitType { get; private set; }
        public string[] HitTypeValues { get; private set; }
        public MoveType MyMoveType { get; private set; }
        public string[] MoveTypeValues { get; private set; }
        public int[] Threshold { get; private set; }
        public string[] Motions { get; private set; }
        public string Voice { get; private set; }
        public int PrefabID { get; private set; }
        public int SubPrefabID { get; private set; }
        public float[] HitMonsterShaderSetting { get; private set; }
        public float[] CameraShakeSettings { get; private set; }
        public float[] HeroShaderSettings { get; private set; }
        static Dictionary<int, Dictionary<SpellName, HeroSpellJsonData>> SpellDic = new Dictionary<int, Dictionary<SpellName, HeroSpellJsonData>>();//使用英雄ID與技能名稱取資料的字典

        protected override void GetDataFromJson(JsonData _item, string _dataName) {
            DataName = _dataName;
            JsonData item = _item;
            foreach (string key in item.Keys) {
                switch (key) {
                    case "ID":
                        ID = item[key].ToString();
                        break;
                    case "Ref":
                        Ref = item[key].ToString();
                        break;
                    case "RTP":
                        RTP = TextManager.StringSplitToFloatArray(item[key].ToString(), ',');
                        break;
                    case "CD":
                        CD = float.Parse(item[key].ToString());
                        break;
                    case "Cost":
                        Cost = int.Parse(item[key].ToString());
                        break;
                    case "Waves":
                        Waves = int.Parse(item[key].ToString());
                        break;
                    case "WaveHits":
                        WaveHits = int.Parse(item[key].ToString());
                        break;
                    case "MaxHits":
                        MaxHits = int.Parse(item[key].ToString());
                        break;
                    case "DragType":
                        MyDragType = MyEnum.ParseEnum<DragType>(item[key].ToString());
                        break;
                    case "DestroyOnCollision":
                        DestroyOnCollision = bool.Parse(item[key].ToString());
                        break;
                    case "SpellType":
                        MySpellType = MyEnum.ParseEnum<SpellType>(item[key].ToString());
                        break;
                    case "SpellTypeValues":
                        SpellTypeValues = item[key].ToString().Split(',');
                        if (SpellTypeValues == null || SpellTypeValues.Length == 0) WriteLog.LogErrorFormat("Hero");
                        break;
                    case "HitEffectType":
                        MyHitType = MyEnum.ParseEnum<HitType>(item[key].ToString());
                        break;
                    case "HitEffectValues":
                        HitTypeValues = item[key].ToString().Split(',');
                        break;
                    case "MoveType":
                        MyMoveType = MyEnum.ParseEnum<MoveType>(item[key].ToString());
                        break;
                    case "MoveTypeValues":
                        MoveTypeValues = item[key].ToString().Split(',');
                        if (MoveTypeValues == null || MoveTypeValues.Length == 0) WriteLog.LogErrorFormat("Move");
                        break;
                    case "Threshold":
                        Threshold = TextManager.StringSplitToIntArray(item[key].ToString(), ',');
                        break;
                    case "Motions":
                        Motions = item[key].ToString().Split(',');
                        break;
                    case "Voice":
                        Voice = item[key].ToString();
                        break;
                    case "PrefabID":
                        if (int.TryParse(item[key].ToString(), out int _id))
                            PrefabID = _id;
                        else
                            WriteLog.LogErrorFormat("{0}表ID為{1}的PrefabID必須為數字 {2}", DataName, ID, item[key]);
                        break;
                    case "SubPrefabID":
                        if (int.TryParse(item[key].ToString(), out _id))
                            SubPrefabID = _id;
                        else
                            WriteLog.LogErrorFormat("{0}表ID為{1}的SubPrefabID必須為數字 {2}", DataName, ID, item[key]);
                        break;
                    case "HitMonsterShaderSetting":
                        HitMonsterShaderSetting = TextManager.StringSplitToFloatArray(item[key].ToString(), ',');
                        break;
                    case "CameraShakeSettings":
                        var content = item[key].ToString();
                        if (!string.IsNullOrEmpty(content))
                            CameraShakeSettings = TextManager.StringSplitToFloatArray(content, ',');
                        break;
                    case "HeroShaderSettings":
                        HeroShaderSettings = TextManager.StringSplitToFloatArray(item[key].ToString(), ',');
                        break;
                    default:
                        WriteLog.LogWarning(string.Format("{0}表有不明屬性:{1}", DataName, key));
                        break;
                }
            }
            AddToSpellDic(ID, this);
            BuildSpell();
        }

        private void BuildSpell() {
            var builder = new SpellBuilder(this);
            Spell = builder.Build();
        }

        static void AddToSpellDic(string _id, HeroSpellJsonData _data) {
            string[] strs = _id.Split('_');
            if (strs.Length != 2) return;
            if (string.IsNullOrEmpty(strs[1])) return;

            SpellName spellName;
            if (!MyEnum.TryParseEnum(strs[1], out spellName)) {
                WriteLog.LogErrorFormat("HeroSpell ID為 {0} 的格式應該為 HeroID_SpellName", _id);
                return;
            }
            _data.SpellName = spellName;
            if (int.TryParse(strs[0], out int _heroID)) {
                if (SpellDic.ContainsKey(_heroID)) {
                    if (SpellDic[_heroID].ContainsKey(spellName))
                        WriteLog.LogErrorFormat("重複的SpellName: {0}", strs[1]);
                    SpellDic[_heroID][spellName] = _data;
                }
                else {
                    SpellDic.Add(_heroID, new Dictionary<SpellName, HeroSpellJsonData>() { { spellName, _data } });
                }
                _data.HeroID = _heroID;
            }

        }

        public static HeroSpellJsonData GetData(string _id) {
            return GameDictionary.GetJsonData<HeroSpellJsonData>(DataName, _id);
        }

        public static Dictionary<SpellName, HeroSpellJsonData> GetSpellDic(int _heroID) {
            if (!SpellDic.ContainsKey(_heroID)) return null;
            return SpellDic[_heroID];
        }
        public static HeroSpellJsonData GetSpell(int _heroID, SpellName _spellName) {
            if (!SpellDic.ContainsKey(_heroID)) { WriteLog.LogErrorFormat("GetSpell失敗,  SpellDic不包含此英雄ID: {0}", _heroID); return null; }
            if (!SpellDic[_heroID].ContainsKey(_spellName)) { WriteLog.LogErrorFormat("GetSpell失敗,  SpellDic不包含此技能ID: {0}", _spellName); return null; }
            return SpellDic[_heroID][_spellName];
        }

        protected override void ResetStaticData() {
            SpellDic.Clear();
        }
    }

}
