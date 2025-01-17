using DG.Tweening;
using HeroFishing.Main;
using HeroFishing.Socket;
using Scoz.Func;
using System;
using UniRx;
using UnityEngine;

namespace HeroFishing.Battle {
    public struct BulletInfo {
        public Vector3 FirePosition;
        public int PrefabID;
        public int SubPrefabID;
        public bool IgnoreFireModel;
        public bool IsDrop;
        public float Delay;
    }

#if UNITY_EDITOR
    public struct BulletGizmoData {
        public Vector3 Position;
        public Vector3 Direction;
        public float Radius;
        public float Angle;
    }
#endif
    public class Bullet : MonoBehaviour {
        private BulletInfo _info;

#if UNITY_EDITOR
        BulletGizmoData gizmoData;
#endif

        public bool IsLoaded { get; private set; }
        public GameObject Projectile { get; private set; }

        private void OnDestroy() {
        }

        //public void HitParticleEffect() {
        //    //載入Hit模型
        //    var bulletPos = transform.position - new Vector3(0, GameSettingJsonData.GetFloat(GameSetting.Bullet_PositionY), 0);
        //    var bulletRot = transform.rotation;
        //    string hitPath = string.Format("Bullet/BulletHit{0}", SpellPrefabID);
        //    GameObjSpawner.SpawnParticleObjByPath(hitPath, bulletPos, bulletRot, null, (go, handle) => {
        //        AddressableManage.SetToChangeSceneRelease(handle);//切場景再釋放資源
        //    });
        //}
        public void Create(BulletInfo bulletInfo) {
            _info = bulletInfo;
            gameObject.SetActive(false);
            if (!bulletInfo.IgnoreFireModel)
                LoadFireModel();
            if (_info.Delay > 0) {
                Observable.Timer(TimeSpan.FromSeconds(_info.Delay)).Subscribe(_ => {
                    LoadProjetileModel();
                });
            }
            else
                LoadProjetileModel();
        }

        void LoadFireModel() {
            //載入Fire模型
            var bulletPos = _info.FirePosition;
            var bulletRot = transform.rotation;
            //string firePath = string.Format($"Bullet/{HeroName}/BulletFire{SpellPrefabID}");
            var pool = PoolManager.Instance;
            //var path = string.Format("Assets/AddressableAssets/Particles/{0}.prefab", firePath);
            pool.Pop(_info.PrefabID, 0, PoolManager.PopType.Fire, bulletPos, bulletRot, null, go => {
                // 讓AutoBackPool.cs自己控制返回物件池的時間
            });
        }

        void LoadProjetileModel() {
            if (Projectile != null) {
                LoadDone();
                return;
            }

            //string projectilePath;
            ////載入Projectile模型
            //if (SubSpellPrefabID == 0)
            //    projectilePath = string.Format($"Bullet/{HeroName}/BulletProjectile{SpellPrefabID}");
            //else
            //    projectilePath = string.Format($"Bullet/{HeroName}/BulletProjectile{SpellPrefabID}_{SubSpellPrefabID}");

            var pool = PoolManager.Instance;
            //var path = string.Format("Assets/AddressableAssets/Particles/{0}.prefab", projectilePath);
            //Debug.Log("is drop " + _info.IsDrop);
            PoolManager.PopType popType = _info.IsDrop ? PoolManager.PopType.Empty : PoolManager.PopType.Projectile;
            pool.Pop(_info.PrefabID, _info.SubPrefabID, popType, Vector3.zero, Quaternion.identity, transform, go => {
                go.transform.localScale = Vector3.one;
                Projectile = go;
                LoadDone();
            });
        }

        /// <summary>
        /// 模型都載入完才呼叫並顯示物件
        /// </summary>
        protected virtual void LoadDone() {
            IsLoaded = true;
            gameObject.SetActive(true);
        }

#if UNITY_EDITOR
        public void SetGizmoData(BulletGizmoData gizmoData) {
            this.gizmoData = gizmoData;
        }

        private void OnDrawGizmos() {
            Gizmos.color = Color.yellow;
            UnityEditor.Handles.color = Color.yellow;
            if (gizmoData.Angle > 0 && gizmoData.Direction != Vector3.zero) {
                Vector3 fromDir = Quaternion.AngleAxis(-gizmoData.Angle / 2, Vector3.up) * gizmoData.Direction;
                gizmoData.Position.y = 0.1f;
                UnityEditor.Handles.DrawWireArc(gizmoData.Position, Vector3.up, fromDir, gizmoData.Angle, gizmoData.Radius);
                Gizmos.DrawRay(gizmoData.Position, fromDir * gizmoData.Radius);
                Gizmos.DrawRay(gizmoData.Position, Quaternion.AngleAxis(gizmoData.Angle, Vector3.up) * fromDir * gizmoData.Radius);
            }
            else {
                Gizmos.DrawWireSphere(gizmoData.Position, gizmoData.Radius);
            }
        }
#endif
    }

}