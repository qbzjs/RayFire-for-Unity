using HeroFishing.Battle;
using HeroFishing.Main;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class CircleAreaSpell : SpellBase {
    protected override int VariableCount => 4;
    private float _radius;
    private float _scale;
    private float _lifeTime;
    private float _delay;
    private float _collisionDelay;
    private float _collisionDuration;

    public override SpellIndicator.IndicatorType SpellIndicatorType => SpellIndicator.IndicatorType.Circle;

    private HeroSpellJsonData _data;

    public CircleAreaSpell(HeroSpellJsonData data) {
        _data = data;

        var values = _data.SpellTypeValues;
        _radius = float.Parse(values[0]);
        _scale = float.Parse(values[1]);
        _lifeTime = float.Parse(values[2]);
        _delay = float.Parse(values[3]);
        _collisionDelay = float.Parse(values[4]);
        _collisionDuration = float.Parse(values[5]);
    }

    public override void Play(SpellPlayData playData) {
        base.Play(playData);
        var spawnBulletInfo = new SpawnBulletInfo {
            PrefabID = _data.PrefabID,
            ProjectileScale = _scale,
            ProjectileDelay = _delay,
            FirePosition = playData.heroPos,
            InitPosition = playData.attackPos,
            InitDirection = playData.direction,
            IgnoreFireModel = false,
            LifeTime = _lifeTime,
            IsDrop = playData.IsDrop,
        };
        if (!BulletSpawner.Spawn(spawnBulletInfo, out Bullet bullet)) return;
        var collisionInfo = new AreaCollisionInfo {
            HeroIndex = playData.heroIndex,
            AttackID = playData.attackID,
            SpellID = _data.ID,
            Delay = _collisionDelay - _delay,
            Duration = _collisionDuration,
            Waves = _data.Waves,
            Position = playData.attackPos,
            Direction = playData.direction,
            Radius = _radius,
        };
        BulletSpawner.AddCollisionComponent(collisionInfo, bullet);
        //base.Play(position, heroPosition, direction);
        //var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        //var entity = entityManager.CreateEntity();
        //var strIndex_SpellID = ECSStrManager.AddStr(_data.ID);

        //var spawnData = new SpellSpawnData {
        //    AttackID = playData.attackID,
        //    SpellPrefabID = _data.PrefabID,
        //    ProjectileScale = _scale,
        //    ProjectileDelay = _delay,
        //    FirePosition = playData.heroPos,
        //    InitPosition = playData.attackPos,
        //    InitDirection = playData.direction,
        //    IgnoreFireModel = false
        //};

        //entityManager.AddComponentData(entity, new SpellAreaData {
        //    HeroIndex = playData.heroIndex,
        //    StrIndex_SpellID = strIndex_SpellID,
        //    SpawnData = spawnData,
        //    CollisionDelay = _collisionDelay,
        //    CollisionTime = _collisionTime,
        //    LifeTime = _lifeTime,
        //    Radius = _radius,
        //    Waves = _data.Waves
        //});
    }

    public override void IndicatorCallback(GameObject go) {
        var renderer = go.GetComponentInChildren<Renderer>();
        renderer.transform.localScale = Vector3.one * _radius * 2;
    }
}
