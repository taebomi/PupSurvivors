using System;
using Cysharp.Threading.Tasks;
using PupSurvivors.Weapon;
using UnityEngine;

namespace PupSurvivors.Equipment
{
    public class Claw : WeaponBase
    {
        private ProjectilePool<ClawProjectile> _projectilePool;

        private float _angleInterval;
        private float _cooldownTimer;

        protected override async UniTask InitializeAfter()
        {
            _projectilePool = new ProjectilePool<ClawProjectile>(this);
            await _projectilePool.Initialize();
            AttackAction += () => Attack().Forget();
            StatChangedEvent.AddListener(OnPlayerStatsChanged);
            CheckCooldown().Forget();
        }

        private void OnPlayerStatsChanged()
        {
            _angleInterval = 360f / ModifiedWeaponStats.amount;
        }


        private async UniTaskVoid CheckCooldown()
        {
            while (true)
            {
                if (_cooldownTimer > ModifiedWeaponStats.cooldown)
                {
                    Attack().Forget();
                    _cooldownTimer -= ModifiedWeaponStats.cooldown;
                }

                _cooldownTimer += Time.deltaTime;
                await UniTask.Yield(DestroyCts.Token);
            }
        }


        private async UniTaskVoid Attack()
        {
            var position = Player.transform.position;
            for (var i = 0; i < ModifiedWeaponStats.amount; i++)
            {
                var clawProjectile = _projectilePool.Pop();
                var direction = (Vector3)Player.LastInputDir;
                clawProjectile.SetPosition(position)
                    .SetDirection(Quaternion.Euler(0, 0, _angleInterval * i) * direction);
                await UniTask.Delay(TimeSpan.FromSeconds(ModifiedWeaponStats.interval),
                    cancellationToken: DestroyCts.Token);
            }
        }


        protected override void OnDestroy()
        {
            _projectilePool.Dispose();
            base.OnDestroy();
        }
    }
}