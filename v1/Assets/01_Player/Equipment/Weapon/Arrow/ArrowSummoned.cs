using System;
using Cysharp.Threading.Tasks;
using PupSurvivors.Enemy;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PupSurvivors.Equipment
{
    public class ArrowSummoned : SummonedWeaponBase<ArrowSummoned, Arrow>
    {
        [SerializeField] private Animator ani;
        [SerializeField] private Transform bow;

        [SerializeField] private AudioClip[] se;

        private ProjectilePool<ArrowProjectile> _projectilePool;
        private IDamagable _currentTarget;

        private float _attackTimer;

        public override void InitializeAfter()
        {
            _projectilePool = Weapon.ArrowPool;
            PlayerController.Instance.Follower.AddFollower(transform);
            Weapon.AttackAction += Attack;
        }

        private void OnDestroy()
        {
            if (PlayerController.Instance)
            {
                PlayerController.Instance.Follower.RemoveFollower(transform);
            }
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

        public async UniTaskVoid AnimationEvent_OnAttack()
        {
            for (var i = 0; i < ModifiedStats.amount; i++)
            {
                var arrow = _projectilePool.Pop();
                arrow.SetPosition(bow.position)
                    .SetDirection((Vector2)bow.right + Vector2.up * Random.Range(-0.075f, 0.075f));
                SoundManager.Instance.PlaySoundEffect(se[Random.Range(0, se.Length)]).Forget();
                await UniTask.Delay(TimeSpan.FromSeconds(ModifiedStats.interval), cancellationToken: DisableCts.Token);
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