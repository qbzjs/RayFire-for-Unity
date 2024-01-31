using HeroFishing.Battle;
using HeroFishing.Socket;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct AreaCollisionInfo {
    public int HeroIndex;
    public int AttackID;
    public string SpellID;
    public Vector3 Position;
    public Vector3 Direction;
    public float Angle;
    public float Radius;
    public int IgnoreMonsterIdx;
    public int Waves;
    public float Delay;
    public float Duration;
}

public class AreaCollision : CollisionBase {

    private AreaCollisionInfo _info;
    private float _timer;
    private int _waveIndex;
    private Monster[] _hitMonsters;
    private Bullet _bullet;
    public void Init(AreaCollisionInfo info) {
        _info = info;
        Debug.Log(_info.Angle);
        _hitMonsters = new Monster[8];
        _timer = 0;
        _waveIndex = 0;
#if UNITY_EDITOR
        if (_bullet == null)
            _bullet = GetComponent<Bullet>();
        var gizmoData = new BulletGizmoData {
            Position = _info.Position,
            Direction = _info.Direction,
            Angle = _info.Angle,
            Radius = _info.Radius,
        };
        _bullet.SetGizmoData(gizmoData);
#endif
    }

    public override void OnUpdate(float deltaTime) {
        _timer += deltaTime;
        if (_timer < _info.Delay || _timer > _info.Delay + _info.Duration) return;

        float checkTime = _info.Waves > 0 ? _info.Duration / _info.Waves : 0.5f;
        if (_waveIndex >= _info.Waves || _timer < _info.Delay + checkTime * _waveIndex)
            return;
        _waveIndex++;

        int count;
        Monster.TryGetMonsterByIdx(_info.IgnoreMonsterIdx, out Monster ignoreMonster);
        if (_info.Angle > 0) {
            count = Monster.GetMonstersInRangeWithAngle(_info.Position, _info.Radius, _info.Direction, _info.Angle, _hitMonsters, ignoreMonster);
        }
        else {
            count = Monster.GetMonstersInRange(_info.Position, _info.Radius, _hitMonsters);
        }

        int[] idxs = new int[count];
        for (int i = 0; i < count; i++) {
            var monster = _hitMonsters[i];
            monster.OnHit(_info.SpellID, _info.Direction);

            var monsterPos = monster.transform.position;
            monsterPos.y = _info.Position.y;

            SpawnHitParticleInfo hitParticleInfo = new SpawnHitParticleInfo {
                SpellID = _info.SpellID,
                HitPos = monsterPos,
                HitRot = Quaternion.identity,
                Monster = monster,
            };
            HitParticleSpawner.Spawn(hitParticleInfo);
            idxs[i] = monster.MonsterIdx;

            if (!GameConnector.Connected) {
                float value = Random.value;
                if (value < BattleManager.Instance.LocalDieThreshold) {
                    monster.Die(_info.HeroIndex);
                }
            }
        }

        if (GameConnector.Connected)
            GameConnector.Instance.Hit(_info.AttackID, idxs, _info.SpellID);
    }
}
