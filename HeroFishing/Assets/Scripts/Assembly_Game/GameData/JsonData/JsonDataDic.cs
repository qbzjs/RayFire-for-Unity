using UnityEngine;
using System.Collections.Generic;
using System;
using HeroFishing.Main;
using System.Linq;

namespace Scoz.Func {

    public partial class GameDictionary : MonoBehaviour {



        //字典
        public static Dictionary<ItemType, Dictionary<int, MyJsonData>> ItemJsonDic = new Dictionary<ItemType, Dictionary<int, MyJsonData>>();
        public static Dictionary<string, Dictionary<int, MyJsonData>> IntKeyJsonDic = new Dictionary<string, Dictionary<int, MyJsonData>>();
        public static Dictionary<string, Dictionary<string, MyJsonData>> StrKeyJsonDic = new Dictionary<string, Dictionary<string, MyJsonData>>();


        //String
        public static Dictionary<string, StringJsonData> StringDic = new Dictionary<string, StringJsonData>();
        static LoadingProgress MyLoadingProgress;//載入JsonData進度
        public static bool IsFinishLoadAddressableJson {
            get {
                if (MyLoadingProgress == null) return false;
                return MyLoadingProgress.IsFinished;
            }
        }

        ///// <summary>
        ///// 將Json資料寫入字典裡
        ///// </summary>
        public static void LoadJsonDataToDic(Action _action) {
            //初始化讀取進度並設定讀取完要執行的程式
            MyLoadingProgress = new LoadingProgress(() => {
#if UNITY_EDITOR
                ExcelDataValidation.CheckDatas();//檢查excel資料有沒有填錯
#endif
                _action?.Invoke();
            });

            //Addressables版本
            StringJsonData.GetStringDic_Remote("String", dic => {
                StringDic = dic;
                //完成MyLoadingProgress進度，全部都載完就會回傳LoadJsonDataToDic傳入的Action
                MyLoadingProgress.FinishProgress("String");
            });

            GameSettingJsonData.DataName = "GameSetting";
            MyJsonData.SetDataStringKey_Remote<GameSettingJsonData>(GameSettingJsonData.DataName, SetDic);

            SceneTransitionJsonData.DataName = "SceneTransition";
            MyJsonData.SetData_Remote<SceneTransitionJsonData>(SceneTransitionJsonData.DataName, SetDic);

            MapJsonData.DataName = "Map";
            MyJsonData.SetData_Remote<MapJsonData>(MapJsonData.DataName, SetDic);

            MonsterSpawnerJsonData.DataName = "MonsterSpawner";
            MyJsonData.SetData_Remote<MonsterSpawnerJsonData>(MonsterSpawnerJsonData.DataName, SetDic);

            RouteJsonData.DataName = "Route";
            MyJsonData.SetData_Remote<RouteJsonData>(RouteJsonData.DataName, SetDic);

            MonsterJsonData.DataName = "Monster";
            MyJsonData.SetData_Remote<MonsterJsonData>(MonsterJsonData.DataName, SetDic);

            HeroJsonData.DataName = "Hero";
            MyJsonData.SetData_Remote<HeroJsonData>(HeroJsonData.DataName, SetDic);

            HeroSkinJsonData.DataName = "HeroSkin";
            MyJsonData.SetDataStringKey_Remote<HeroSkinJsonData>(HeroSkinJsonData.DataName, SetDic);

            HeroSpellJsonData.DataName = "HeroSpell";
            MyJsonData.SetDataStringKey_Remote<HeroSpellJsonData>(HeroSpellJsonData.DataName, SetDic);

            HeroEXPJsonData.DataName = "HeroEXP";
            MyJsonData.SetData_Remote<HeroEXPJsonData>(HeroEXPJsonData.DataName, SetDic);

            DropSpellJsonData.DataName = "Drop";
            MyJsonData.SetData_Remote<DropJsonData>(DropSpellJsonData.DataName, SetDic);

            DropSpellJsonData.DataName = "DropSpell";
            MyJsonData.SetData_Remote<DropSpellJsonData>(DropSpellJsonData.DataName, SetDic);

            //設定X秒會顯示尚未載入的JsonData
            CoroutineJob.Instance.StartNewAction(ShowUnLoadedJsondata, 5);

        }
        /// <summary>
        /// 將要載入的json加到進度中，等全部json都載完才會透過MyLoadingProgress回傳LoadJsonDataToDic傳入的Action
        /// </summary>
        public static void AddLoadingKey(string _key) {
            MyLoadingProgress.AddLoadingProgress(_key);
        }
        /// <summary>
        /// 開始載json後過3秒會顯示尚未載入的JsonData
        /// </summary>
        static void ShowUnLoadedJsondata() {
            List<string> notFinishedKeys = MyLoadingProgress.GetNotFinishedKeys();
            for (int i = 0; i < notFinishedKeys.Count; i++)
                WriteLog.LogErrorFormat("{0}Json尚未載入", notFinishedKeys[i]);
        }
        /// <summary>
        /// 取得T類型的JsonData Dic
        /// </summary>
        public static Dictionary<int, T> GetIntKeyJsonDic<T>(string _name) where T : MyJsonData {
            if (IntKeyJsonDic.ContainsKey(_name)) {
                return IntKeyJsonDic[_name].ToDictionary(a => a.Key, a => a.Value as T);
            } else {
                string log = string.Format("{0}表不存IntKeyJsonDic中", _name);
                PopupUI.ShowClickCancel(log, null);
                WriteLog.LogErrorFormat(log);
                return null;
            }
        }
        /// <summary>
        /// 取得T類型的JsonData Dic
        /// </summary>
        public static Dictionary<string, T> GetStrKeyJsonDic<T>(string _name) where T : MyJsonData {
            if (StrKeyJsonDic.ContainsKey(_name)) {
                return StrKeyJsonDic[_name].ToDictionary(a => a.Key, a => a.Value as T);
            } else {
                string log = string.Format("{0}表不存StrKeyJsonDic中", _name);
                PopupUI.ShowClickCancel(log, null);
                WriteLog.LogErrorFormat(log);
                return null;
            }
        }
        /// <summary>
        /// 取得T類型的JsonData
        /// </summary>
        public static T GetJsonData<T>(string _name, int _id, bool showErrorMsg = true) where T : MyJsonData {
            if (IntKeyJsonDic == null)
                return null;
            if (IntKeyJsonDic.ContainsKey(_name) && IntKeyJsonDic[_name] != null && IntKeyJsonDic[_name].ContainsKey(_id))
                return IntKeyJsonDic[_name][_id] as T;
            else {
                string log = string.Format("{0}表不存在ID:{1}的資料", _name, _id);
                if (showErrorMsg) {
                    PopupUI.ShowClickCancel(log, null);
                }
                WriteLog.LogErrorFormat(log);
                return null;
            }
        }
        /// <summary>
        /// 取得T類型的JsonData
        /// </summary>
        public static T GetJsonData<T>(string _name, string _id) where T : MyJsonData {
            if (StrKeyJsonDic == null)
                return null;
            if (StrKeyJsonDic.ContainsKey(_name) && StrKeyJsonDic[_name] != null && StrKeyJsonDic[_name].ContainsKey(_id))
                return StrKeyJsonDic[_name][_id] as T;
            else {
                string log = string.Format("{0}表不存在ID:{1}的資料", _name, _id);
                PopupUI.ShowClickCancel(log, null);
                WriteLog.LogErrorFormat(log);
                return null;
            }
        }
        /// <summary>
        /// 取得IItemJsonData Dic
        /// </summary>
        public static Dictionary<int, IItemJsonData> GetIItemJsonDic(ItemType _itemType) {
            if (ItemJsonDic.ContainsKey(_itemType)) {
                return ItemJsonDic[_itemType].ToDictionary(a => a.Key, a => a.Value as IItemJsonData);
            } else {
                string log = string.Format("{0}表不存ItemJsonDic中", _itemType);
                PopupUI.ShowClickCancel(log, null);
                WriteLog.LogErrorFormat(log);
                return null;
            }
        }
        /// <summary>
        /// 取得ItemJsonData
        /// </summary>
        public static IItemJsonData GetItemJsonData(ItemType _itemType, int _id) {
            if (ItemJsonDic.ContainsKey(_itemType) && ItemJsonDic[_itemType].ContainsKey(_id)) {
                IItemJsonData iItemJsonData = ItemJsonDic[_itemType][_id] as IItemJsonData;
                if (iItemJsonData != null)
                    return iItemJsonData;
                string log = string.Format("{0}表的資料不為IItemJsonData", _itemType);
                PopupUI.ShowClickCancel(log, null);
                WriteLog.LogErrorFormat("{0}表的資料不為IItemJsonData", _itemType);
                return null;
            } else {
                string log = string.Format("{0}表不存在ID:{1}的資料", _itemType, _id);
                PopupUI.ShowClickCancel(log, null);
                WriteLog.LogErrorFormat(log);
                return null;
            }
        }

        /// <summary>
        /// 設定已int作為Key值得 JsonData Dictionary
        /// </summary>
        static void SetDic(string _name, Dictionary<int, MyJsonData> _dic) {

            if (_dic != null && _dic.Values.Count > 0) {
                //將JsonDataDic加到字典中
                IntKeyJsonDic[_name] = _dic;
                //如果是IItemType類的Json資料(會繼承IItemJsonData)也加到ItemJsonDic中
                if (_dic.Values.First() is IItemJsonData) {
                    ItemType itemType;
                    if (MyEnum.TryParseEnum(_name, out itemType)) {
                        ItemJsonDic[itemType] = _dic;
                    }
                }
            }
            //完成MyLoadingProgress進度，全部都載完就會回傳LoadJsonDataToDic傳入的Action
            MyLoadingProgress.FinishProgress(_name);
        }

        /// <summary>
        /// 設定已string作為Key值得 JsonData Dictionary
        /// </summary>
        static void SetDic(string _name, Dictionary<string, MyJsonData> _dic) {

            if (_dic != null && _dic.Values.Count > 0) {
                StrKeyJsonDic[_name] = _dic;
            }

            //完成MyLoadingProgress進度，全部都載完就會回傳LoadJsonDataToDic傳入的Action
            MyLoadingProgress.FinishProgress(_name);
        }

    }
}
