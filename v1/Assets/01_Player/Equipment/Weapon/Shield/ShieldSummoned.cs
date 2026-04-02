using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace PupSurvivors.Equipment
{
    public class ShieldSummoned : SummonedWeaponBase<ShieldSummoned, Shield>
    {
        private ProjectilePool<ShieldProjectile> _projectilePool;
        private List<ShieldProjectile> _currentProjectiles;

        private float _angleInterval;

        public override void InitializeAfter()
        {
            _projectilePool = Weapon.ProjectilePool;
            _currentProjectiles = new List<ShieldProjectile>();
        }

        public override void Attack()
        {
            RotateShield().Forget();
        }


        private async UniTaskVoid RotateShield()
        {
            var timer = 0f;
            var currentAngle = Random.Range(0f, 360f);

            for (var i = 0; i < ModifiedStats.amount; i++)
            {
                var projectile = _projectilePool.Pop();
                projectile.Appear().Forget();
                _currentProjectiles.Add(projectile);
            }

            // 지속 시간동안 회전시킴
            while (timer < ModifiedStats.duration)
            {
                // 위치 조정
                for (var i = 0; i < _currentProjectiles.Count; i++)
                {
                    var deg = currentAngle + _angleInterval * i;
                    _currentProjectiles[i].Set(deg);
                }

                currentAngle += Time.deltaTime * ModifiedStats.speed;
                timer += Time.deltaTime;
                await UniTask.Yield(DisableCts.Token);
            }

            foreach (var shield in _currentProjectiles)
            {
                shield.Disappear().Forget();
            }

            _currentProjectiles.Clear();
            Weapon.SummonedStack.Push(this);
        }

        public override void UpdateStats()
        {
            if (ModifiedStats.amount != _currentProjectiles.Count)
            {
                _angleInterval = 360f / ModifiedStats.amount;

                // 활성화 된 상태면
                if (_currentProjectiles.Count != 0)
                {
                    _currentProjectiles.Add(_projectilePool.Pop());
                }
            }
        }
    }
}