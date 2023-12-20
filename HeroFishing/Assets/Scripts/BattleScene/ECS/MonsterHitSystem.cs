using HeroFishing.Main;
using Scoz.Func;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace HeroFishing.Battle {
    public partial struct MonsterHitSystem : ISystem {
        EndSimulationEntityCommandBufferSystem.Singleton ECBSingleton;
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<MonsterInstance>();
            state.RequireForUpdate<MonsterHitTag>();
        }

        public void OnDestroy(ref SystemState state) {
        }

        public void OnUpdate(ref SystemState state) {
            ECBSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecbWriter = ECBSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            //確認是否是本地端處理死亡
            bool localTest = SystemAPI.HasSingleton<LocalTestSys>();
            uint seed = (uint)(SystemAPI.Time.DeltaTime * 1000000f);
            var random = new Random(seed);
            foreach (var (monsterInstance, hitTag, entity) in SystemAPI.Query<MonsterInstance, MonsterHitTag>().WithEntityAccess()) {
                var path = ECSStrManager.GetStr(hitTag.StrIndex_SpellID);
                string spellID = new string(path.ToArray());
                monsterInstance.MyMonster.OnHit(spellID, hitTag.HitDirection);

                //本地端處理死亡，取得隨機值與基準值來判斷死亡
                if (localTest) {
                    float threshold = SystemAPI.GetSingleton<LocalTestSys>().DeadThreshold;
                    float value = random.NextFloat();
                    if (value < threshold)
                        ecbWriter.AddComponent<MonsterDieTag>(entity.Index, entity);
                }
                ecbWriter.RemoveComponent<MonsterHitTag>(entity.Index, entity);
            }
        }
    }
}