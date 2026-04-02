using System;
using PupSurvivors.Enemy;
using UnityEngine;

namespace PupSurvivors.Equipment
{
    public class SharangaSummoned : SummonedWeaponBase<SharangaSummoned, Sharanga>
    {
        [SerializeField] private Animator ani;
        [SerializeField] private Transform bow;
        [SerializeField] private ParticleSystem muzzlePS;
    
        private IDamagable _currentTarget;

        private float _attackTimer;

        private ProjectilePool<SharangaProjectile> _projectilePool;

        [SerializeField] private AudioClip se;

        public override void InitializeAfter()
        {
            _projectilePool = Weapon.ProjectilePool;
            PlayerController.Instance.Follower.AddFollower(transform);
            Weapon.AttackAction += Attack;
        }

        private void OnDestroy()
        {
            PlayerController.Instance.Follower.RemoveFollower(transform);
        }

        private void Update()
        {
            AimEnemy();
            VerticalFlow();
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
        }

        public override void Attack()
        {
            _currentTarget = EnemyManager.Instance.NearestDamagableEnemy;
            ani.SetTrigger(TaeBoMiCache.AttackTrigger);
        }

        public void AnimationEvent_OnAttack()
        {
            muzzlePS.Play(true);
            SoundManager.Instance.PlaySoundEffect(se).Forget();
            var arrow = _projectilePool.Pop();
            arrow.SetPosition(bow.position)
                .SetDirection(bow.right);
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