using System;
using Cysharp.Threading.Tasks;
using PupSurvivors.Stage;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PupSurvivors.Equipment
{
    public class BowSummoned : WeaponSummonedBase<BowSummoned, Bow>
    {
        [SerializeField] private AudioEventChannelSO seEventChannelSO;

        [SerializeField] private Animator ani;
        [SerializeField] private Transform bowTr;
        [SerializeField] private AudioClip[] se;

        private WeaponProjectilePool<BowProjectile> _projectilePool;
        private DamagableHealthSystemBase _curTarget;

        private EnemyFinder _enemyFinder;

        protected override void InitAfter()
        {
            _projectilePool = Weapon.ProjectilePool;
            Weapon.Player.Follower.AddFollower(transform);
            Weapon.ActionOnLevelUp += Attack;

            _enemyFinder = Weapon.Player.EnemyFinder;

            CheckCooldown().Forget();
        }

        private void OnDestroy()
        {
            Weapon.ActionOnLevelUp -= Attack;
            Weapon.Player.Follower.RemoveFollower(transform);
        }

        protected override void OnStatsUpdated()
        {
            var attackRatio = 1 / CurStats.cooldown;
            ani.SetFloat(TaeBoMiCache.AttackRatio, attackRatio);
            bowTr.localScale = new Vector3(CurStats.size, CurStats.size, 1f);
        }

        private async UniTaskVoid CheckCooldown()
        {
            var timer = 0f;
            while (true)
            {
                if (timer > CurStats.cooldown && _enemyFinder.IsDamagableVisible())
                {
                    timer = 0f;
                    Attack();
                }

                timer += Time.deltaTime;
                await UniTask.Yield(DisableCts.Token);
            }
        }

        private void Update()
        {
            Aim();
            Flow();
        }

        private void Attack()
        {
            _curTarget = _enemyFinder.NearestDamagable;
            if (_curTarget is not null)
            {
                ani.SetTrigger(TaeBoMiCache.AttackTrigger);
            }
        }

        public async UniTaskVoid AnimationEvent_OnShot()
        {
            for (var i = 0; i < CurStats.amount; i++)
            {
                var arrow = _projectilePool.Pop();
                seEventChannelSO.RaiseEvent(se[Random.Range(0, se.Length)]);
                arrow.Set(bowTr.position, (Vector2)bowTr.right + Vector2.up * Random.Range(-0.075f, 0.075f));
                await UniTask.Delay(TimeSpan.FromSeconds(CurStats.interval), cancellationToken: DisableCts.Token);
            }
        }

        private void Aim()
        {
            if (_curTarget is not null)
            {
                bowTr.right = _curTarget.transform.position - bowTr.position;
            }
        }

        private void Flow()
        {
            bowTr.localPosition = new Vector3(0f, Mathf.Sin(Time.time * 2f) * 0.25f, 0f);
        }
    }
}