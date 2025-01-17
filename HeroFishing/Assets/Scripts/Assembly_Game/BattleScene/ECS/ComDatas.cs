//using Cysharp.Threading.Tasks;
//using HeroFishing.Battle;
//using Scoz.Func;
//using System;
//using Unity.Collections;
//using Unity.Entities;
//using Unity.Mathematics;
//using UnityEngine;
//public struct BulletValue : IComponentData {
//    public float Speed;
//    public float Radius;
//    public float3 Position;
//    public float3 Direction;
//    public uint StrIndex_SpellID;//紀錄子彈的技能ID
//    public int SpellPrefabID;//技能Prefab名稱
//}

///// <summary>
///// 子彈參照元件，用於參照GameObject實例用
///// </summary>
//public class BulletInstance : IComponentData, IDisposable {
//    public GameObject GO;
//    public Transform Trans;
//    public Bullet MyBullet;
//    public async void Dispose() {
//        await UniTask.WaitUntil(() => MyBullet.IsLoaded);
//        if (GO != null) {
//            var pool = PoolManager.Instance;
//            pool.Push(GO);
//        }
//    }
//}

//public struct SpellSpawnData {
//    public int SpellPrefabID;
//    public int AttackID;
//    public int SubSpellPrefabID;
//    public float3 InitPosition;
//    public float3 FirePosition;
//    public float3 InitDirection;
//    public float ProjectileScale;
//    public float ProjectileDelay;
//    public bool IgnoreFireModel;
//}

//public struct SpellBulletData : IComponentData {
//    public int HeroIndex;
//    public uint StrIndex_SpellID;
//    public SpellSpawnData SpawnData;
//    public float Speed;
//    public float Radius;
//    public float LifeTime;
//    public bool DestroyOnCollision;
//    public bool IsSub;
//    public MonsterValue TargetMonster;
//}

//public struct SpellAreaData : IComponentData {
//    public int HeroIndex;
//    public uint StrIndex_SpellID;
//    public SpellSpawnData SpawnData;
//    public float Radius;
//    public float LifeTime;
//    public float CollisionDelay;
//    public float CollisionTime;
//    public float CollisionAngle;
//    public int Waves;
//    public MonsterValue IgnoreMonster;
//}

//public struct SpellHitNetworkData : IComponentData {
//    public int AttackID;
//    public uint StrIndex_SpellID;
//}

//public struct RefreshSceneTag : IComponentData { }
//public struct SpawnTag : IComponentData { }
//public struct AlreadyUpdateTag : IComponentData { }

////public struct MonsterUpdateData : IComponentData {
////    public int RouteID;
////    public float SpawnTime;
////}

//public struct SpawnData : IComponentData, IDisposable {
//    public NativeArray<MonsterData> Monsters;
//    public int RouteID;
//    public float SpawnTime;
//    public bool IsBoss;
//    public int PlayerIndex;

//    public void Dispose() {
//        if (Monsters.IsCreated)
//            Monsters.Dispose();
//    }
//}

//public struct MonsterData : IComponentData {
//    public int ID;
//    public int Idx;
//}

//[InternalBufferCapacity(16)]
//public struct MonsterHitNetworkData : IBufferElementData {
//    public MonsterValue Monster;
//}

//public struct MonsterDieNetworkData : IComponentData, IDisposable {
//    public NativeArray<KillMonsterData> KillMonsters;

//    public void Dispose() {
//        if (KillMonsters.IsCreated)
//            KillMonsters.Dispose();
//    }
//}

//public struct KillMonsterData : IComponentData {
//    public int HeroIndex;
//    public int KillMonsterIdx;
//    public long GainPoints;
//    public int GainHeroExp;
//    public int GainSpellCharge;
//    public int GainDrop;
//}

//public struct SpellHitTag : IComponentData {
//    public int HeroIndex;
//    public int AttackID;
//    public uint StrIndex_SpellID;
//    public MonsterValue Monster;
//    public float3 HitPosition;
//    public float3 HitDirection;
//    public float3 BulletPosition;
//}

//public struct ChainHitData : IComponentData {
//    public int HeroIndex;
//    public int AttackID;
//    public uint StrIndex_SpellID;
//    public MonsterValue OnHitMonster;
//    public MonsterValue NearestMonster;
//    public float3 HitPosition;
//    public float3 HitDirection;
//    public int MaxChainCount;
//    public float TriggerRange;
//    public float Angle;
//    public float Radius;
//    public int SpellPrefabID;
//    public int SubSpellPrefabID;
//    public float Speed;
//    public float LifeTime;
//}

//public struct MoveData : IComponentData {
//    public MonsterValue TargetMonster;
//    public float Speed;
//    public float3 Position;
//    public float3 Direction;
//}

//public struct BulletCollisionData : IComponentData {
//    public int HeroIndex;
//    public int AttackID;
//    public uint StrIndex_SpellID;
//    public int SpellPrefabID;
//    public float Radius;
//    public float Delay;
//    public float Timer;
//    public bool Destroy;
//    public bool IsSub;
//}

//public struct AreaCollisionData : IComponentData {
//    public int HeroIndex;
//    public int AttackID;
//    public uint StrIndex_SpellID;
//    public int SpellPrefabID;
//    public float3 Position;
//    public float3 Direction;
//    public float Radius;
//    public float Delay;
//    public float CollisionTime;
//    public float Timer;
//    public float Angle;
//    public int Waves;
//    public int WaveIndex;
//    public MonsterValue IgnoreMonster;
//}

//public struct LockMonsterData : IComponentData {
//    public int MonsterIdx;
//    public SpellBulletData BulletData;
//}

//[InternalBufferCapacity(16)]
//public struct MonsterBuffer : IBufferElementData, IComparable<MonsterBuffer> {
//    public MonsterValue Monster;
//    public float Distance;

//    public int CompareTo(MonsterBuffer other) {
//        return (int)(Distance - other.Distance);
//    }
//}

//[InternalBufferCapacity(16)]
//public struct HitInfoBuffer : IBufferElementData {
//    public Entity MonsterEntity;
//    public double HitTime;
//    public int HitCount;
//}

///// <summary>
///// 怪物參照元件，用於參照GameObject實例用
///// </summary>
//public class MonsterInstance : IComponentData, IDisposable {
//    public GameObject GO;
//    public Transform Trans;
//    public Monster MyMonster;
//    public Vector3 Dir;
//    public void Dispose() {
//        UnityEngine.Object.DestroyImmediate(GO);
//    }
//}
///// <summary>
///// 怪物資料元件
///// </summary>
//public struct MonsterValue : IComponentData {
//    public int MonsterID;
//    public int MonsterIdx;
//    public Entity MyEntity;//把自己的Enity記錄起來，之後取的時候較快
//    public float Radius;
//    public float3 Pos;
//    public bool InField;//是否進入戰場，進入之後會改為true，並在怪物離開區域後會將InField為true的怪物移除
//}
///// <summary>
///// 擊中標籤元件
///// </summary>
//public struct MonsterHitTag : IComponentData {
//    public int HeroIndex;
//    public int MonsterID;//受擊怪物ID 
//    public uint StrIndex_SpellID;//ECSStrManager的技能ID字串索引
//    public float3 HitDirection;
//}
///// <summary>
///// 死亡標籤元件
///// </summary>
//public struct MonsterDieTag : IComponentData {
//    public int HeroIndex;
//}
//public struct AutoDestroyTag : IComponentData {
//    public float LifeTime;//生命週期
//    public float ExistTime;//目前存活秒數(預設為0)
//}
///// <summary>
///// 特效元件
///// </summary>
//public struct ParticleSpawnTag : IComponentData {
//    public uint StrIndex_ParticlePath;//ECSStrManager的特效位置字串索引
//    public float3 Pos;
//    public float4 Rot;
//}
///// <summary>
///// 技能特效元件
///// </summary>
//public struct HitParticleSpawnTag : IComponentData {
//    public int SpellPrefabID;
//    public MonsterValue Monster;
//    public float3 HitPos;
//    public float3 HitDir;
//}