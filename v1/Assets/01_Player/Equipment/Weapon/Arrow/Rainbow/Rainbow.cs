using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using PupSurvivors.Enemy;
using PupSurvivors.Equipment;
using PupSurvivors.Weapon;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace PupSurvivors.Weapon
{
    public class Rainbow : WeaponBase
    {
        private RainbowSummoned _rainbowSummoned;

        public ProjectilePool<RainbowProjectile> ArrowPool { get; private set; }

        protected override async UniTask InitializeAfter()
        {
            ArrowPool = new ProjectilePool<RainbowProjectile>(this);
            await ArrowPool.Initialize();
            await SummonedWeaponBase<RainbowSummoned, Rainbow>.Initialize(this);
            _rainbowSummoned = SummonedWeaponBase<RainbowSummoned, Rainbow>.CreateInstance();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Destroy(_rainbowSummoned.gameObject);
            ArrowPool.Dispose();
            SummonedWeaponBase<RainbowSummoned, Rainbow>.Dispose();
        }
    }
}