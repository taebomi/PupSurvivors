using System;
using PupSurvivors.Enemy;
using Unity.VisualScripting;
using UnityEngine;

namespace PupSurvivors.Equipment
{
    public class DeathSentenceSummoned : SummonedWeaponBase<DeathSentenceSummoned, DeathSentence>
    {
        [SerializeField] private Animator ani;


        private ProjectilePool<DeathSentenceProjectile> _projectilePool;

        private EnemyBase[] _targets;

        private Transform _playerTransform;

        private float _timer;
        private int _attackNum;

        public override void InitializeAfter()
        {
            _projectilePool = Weapon.ProjectilePool;
            _targets = new EnemyBase[ModifiedStats.amount];
            _playerTransform = PlayerController.Instance.transform;
            Weapon.AttackAction += Attack;
        }

        public override void Attack()
        {
            _attackNum++;
            ani.SetInteger(TaeBoMiCache.Attack, _attackNum);
        }

        private void Update()
        {
            while (_timer > ModifiedStats.cooldown)
            {
                _timer -= ModifiedStats.cooldown;
                Attack();
            }

            _timer += Time.deltaTime;
        }

        private void LateUpdate()
        {
            transform.position = _playerTransform.position;
        }


        public void AnimationEvent_OnAttack()
        {
            for (var i = 0; i < _attackNum; i++)
            {
                var targetNum = EnemyManager.Instance.GetVisibleEnemy(_targets);
                for (var j = 0; j < targetNum; j++)
                {
                    var projectile = _projectilePool.Pop();
                    projectile.SetTarget(_targets[j].HealthSystem);
                }
            }

            _attackNum = 0;
            ani.SetInteger(TaeBoMiCache.Attack, 0);
        }

        public override void UpdateStats()
        {
            if (_targets.Length != ModifiedStats.amount)
            {
                _targets = new EnemyBase[ModifiedStats.amount];
            }
        }
    }
}