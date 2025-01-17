using HeroFishing.Main;
using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
namespace Scoz.Func {
    [Serializable]
    public struct BloomSetting {
        public float Threshold;
        public float Intensity;
        public Color TintColor;
    }

    public class PostProcessingManager : MonoBehaviour {

        [Serializable] public class BloomSettingDicClass : SerializableDictionary<MyScene, BloomSetting> { }
        [SerializeField] BloomSettingDicClass MyBloomSettingDic;//字典攝影機設定字典
        [SerializeField] bool AutoDisableVolume;
        Volume MyVolume;
        public static PostProcessingManager Instance;
        private float _timer;

        public void Init() {
            Instance = this;
            SceneManager.sceneLoaded += OnLevelFinishedLoading;
            DontDestroyOnLoad(gameObject);
            MyVolume = GetComponent<Volume>();

            //初始化時先執行一次
            OnLevelFinishedLoading(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }

        private void Update() {
            if (!AutoDisableVolume) return;
            if (MyVolume == null || !MyVolume.enabled) return;
            var fps = FPSChecker.GetFPS();
            if (fps < 30) {
                _timer += Time.deltaTime;
                if (_timer > 3) {
                    MyVolume.enabled = false;
                }
            }
            else {
                _timer = 0;
            }
        }

        private void OnDestroy() {
            SceneManager.sceneLoaded -= OnLevelFinishedLoading;
        }
        void OnLevelFinishedLoading(Scene _scene, LoadSceneMode _mode) {
            if (MyBloomSettingDic == null || MyBloomSettingDic.Count == 0) return;
            MyVolume.enabled = GamePlayer.Instance.PostProcessing;
            if (!GamePlayer.Instance.PostProcessing)//沒開後製效果就不用處理後續
                return;
            SetVolume(_scene);
        }
        public void RefreshSetting() {
            if (!Instance) return;
            MyVolume.enabled = GamePlayer.Instance.PostProcessing;
            if (GamePlayer.Instance.PostProcessing)
                SetVolume(SceneManager.GetActiveScene());
        }

        /// <summary>
        /// 根據場景設定Volume
        /// </summary>
        void SetVolume(Scene _scene) {
            if (MyVolume == null) return;
            Bloom bloom;
            MyVolume.profile.TryGet(out bloom);
            if (bloom == null) return;
            MyScene myScene;
            if (Enum.TryParse(_scene.name, out myScene)) {
                if (!MyBloomSettingDic.ContainsKey(myScene)) {
                    MyVolume.enabled = false;
                    return;
                }
                bloom.intensity.value = MyBloomSettingDic[myScene].Intensity;
                bloom.threshold.value = MyBloomSettingDic[myScene].Threshold;
                bloom.tint.value = MyBloomSettingDic[myScene].TintColor;
                MyVolume.enabled = true;
            }
            else {
                MyVolume.enabled = false;
            }
        }
    }
}