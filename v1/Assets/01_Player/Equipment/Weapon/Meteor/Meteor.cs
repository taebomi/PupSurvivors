using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace PupSurvivors.Equipment
{
    public class Meteor : WeaponBase
    {
        private ProjectilePool<MeteorProjectile> _projectilePool;

        private GameObject _meteorEffectAsset;

        private float _timer;

        protected override async UniTask InitializeAfter()
        {
            _projectilePool = new ProjectilePool<MeteorProjectile>(this);
            await _projectilePool.Initialize();
            AttackAction += () => Attack().Forget();
        }

        private void Update()
        {
            while (_timer>ModifiedWeaponStats.cooldown)
            {
                _timer -= ModifiedWeaponStats.cooldown;
                Attack().Forget();
            }
            _timer += Time.deltaTime;
        }

        private async UniTaskVoid Attack()
        {
            for (var i = 0; i < ModifiedWeaponStats.amount; i++)
            {
                var projectile = _projectilePool.Pop();
                projectile.SetPosition(CameraManager.Instance.GetCameraInsideRandomPosition());
                await UniTask.Delay(TimeSpan.FromSeconds(ModifiedWeaponStats.interval));
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _projectilePool.Dispose();
        }
    }
}