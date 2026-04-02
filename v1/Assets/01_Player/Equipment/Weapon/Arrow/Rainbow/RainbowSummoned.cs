using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using PupSurvivors.Enemy;
using PupSurvivors.Equipment;
using PupSurvivors.Weapon;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PupSurvivors.Weapon
{
    // ReSharper disable once InconsistentNaming
    public class RainbowSummoned : SummonedWeaponBase<RainbowSummoned, Rainbow>
    {
        [SerializeField] private Animator ani;
        [SerializeField] private Transform bow;
        [SerializeField] private ParticleSystem muzzleParticle, glowParticle;

        private IDamagable _currentTarget;

        private float _attackTimer;

        private ProjectilePool<RainbowProjectile> _projectilePool;

        private static readonly Color[] effectColors =
        {
            Color.red,
            Color.yellow,
            Color.green,
            Color.blue,
            new(0.7169523f, 0f, 1f)
        };

        public override void InitializeAfter()
        {
            _projectilePool = Weapon.ArrowPool;
            PlayerController.Instance.Follower.AddFollower(transform);
        }

        private void Update()
        {
            while (_attackTimer > ModifiedStats.cooldown)
            {
                if (EnemyManager.Instance.VisibleEnemySet.Count == 0)
                {
                    return;
                }

                _attackTimer -= ModifiedStats.cooldown;
                Attack();
            }

            _attackTimer += Time.deltaTime;
            AimEnemy();
            VerticalFlow();
        }

        public override void Attack()
        {
            _currentTarget = EnemyManager.Instance.NearestDamagableEnemy;
            ani.SetTrigger(TaeBoMiCache.AttackTrigger);
        }

        public async UniTaskVoid AnimationEvent_OnAttack()
        {
            for (var i = 0; i < ModifiedStats.amount; i++)
            {
                var arrow = _projectilePool.Pop();
                arrow.SetPosition(bow.position + bow.right * 1.5f)
                    .SetDirection((Vector2)bow.right + Vector2.up * Random.Range(-0.075f, 0.075f));
                var color = (RainbowProjectile.Color)(Random.Range(0, 5));
                PlayEffect(color);
                arrow.SetColor(color);
                await UniTask.Delay(TimeSpan.FromSeconds(ModifiedStats.interval));
            }
        }

        private void PlayEffect(RainbowProjectile.Color color)
        {
            var muzzleParticleMain = muzzleParticle.main;
            muzzleParticleMain.startColor = effectColors[(int)color];
            var glowParticleMain = glowParticle.main;
            glowParticleMain.startColor = effectColors[(int)color];
            muzzleParticle.Play();
            glowParticle.Play();
        }

        public override void UpdateStats()
        {
            var attackRatio = 1 / ModifiedStats.cooldown;
            ani.SetFloat(TaeBoMiCache.AttackRatio, attackRatio);
            bow.localScale = Vector3.one * ModifiedStats.size;
        }

        private void VerticalFlow()
        {
            bow.localPosition = new Vector3(0f, Mathf.Sin(Time.time * 2) * 0.25f, 0f);
        }

        private void AimEnemy()
        {
            if (_currentTarget is not null)
            {
                bow.right = _currentTarget.transform.position - bow.position;
            }
        }
    }
}