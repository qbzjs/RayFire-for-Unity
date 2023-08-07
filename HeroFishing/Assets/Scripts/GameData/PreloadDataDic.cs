using UnityEngine;
using System.Collections.Generic;
using HeroFishing.Main;
using HeroFishing.Battle;
using UnityEngine.AddressableAssets;

namespace Scoz.Func {

    public partial class GameDictionary : MonoBehaviour {


        [HeaderAttribute("==============直接引用的資源==============")]

        [SerializeField] public Monster MonsterPrefab;

        //[HeaderAttribute("==============AssetReference引用的資源==============")]

        //static Dictionary<string, GameObject> BulletPrefabs = new Dictionary<string, GameObject>();

        //public static void PreLoadBulletPrefabs() {
        //    WriteLog.Log("預載BulletPrefabs");
        //    var monsterDatas = GetIntKeyJsonDic<MonsterData>("HeroSkill");
        //    foreach (var data in monsterDatas.Values) {
        //        var tmData = data;
        //        if (string.IsNullOrEmpty(tmData.Ref)) continue;
        //        string path = string.Format("Monster/{0}", tmData.Ref);
        //        AddressablesLoader.GetPrefab(path, (go, handle) => {

        //            WriteLog.LogFormat("載入MonsterPrefab {0} 完成", tmData.Ref);
        //        });
        //    }
        //}


    }
}