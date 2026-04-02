using System;
using Cysharp.Threading.Tasks;
using PupSurvivors.Enemy;
using PupSurvivors.Equipment;
using PupSurvivors.Weapon;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PupSurvivors.Weapon
{
    // ReSharper disable once InconsistentNaming
    public class FailNotSummoned : SummonedWeaponBase<FailNotSummoned, FailNot>
    {
        [SerializeField] private Animator ani;
        [SerializeField] private Transform bow;
        [SerializeField] private ParticleSystem muzzlePC;

        private IDamagable _currentTarget;

        private float _attackTimer;

        private ProjectilePool<FailNotProjectile> _projectilePool;

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
            muzzlePC.Play(true);
            var dir = Quaternion.Euler(0, 0, -50f) * bow.right;
            for (int i = 0; i < 5; i++)
            {
                var arrow = _projectilePool.Pop();
                arrow.SetPosition(bow.position + bow.right * 1.5f)
                    .SetDirection(Quaternion.Euler(0, 0, 25f * i) * dir);
            }
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