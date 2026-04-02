using System;
using PupSurvivors.Enemy;
using PupSurvivors.Weapon;
using UnityEngine;

namespace PupSurvivors.Equipment
{
    public class HakiSummoned : SummonedWeaponBase<HakiSummoned, Haki>
    {
        private float _size, _timer;
        private const float SizeMultiplier = 2.5f;
        private Transform _playerTransform;
        
        public override void InitializeAfter()
        {
            _playerTransform = PlayerController.Instance.transform;
            Weapon.AttackAction += Attack;
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

        public override void Attack()
        {
            Physics2D.OverlapCircle(transform.position,_size , EnemyManager.Instance.ContactFilter2D,
                TaeBoMiCache.TempColliderList);
            
            DoDamage(TaeBoMiCache.TempColliderList);
        }

        public override void UpdateStats()
        {
            _size = ModifiedStats.size * SizeMultiplier;
            transform.localScale = ModifiedStats.size * Vector3.one;
        }

    }
}