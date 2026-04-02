using System.Threading;
using Cysharp.Threading.Tasks;
using PupSurvivors.Enemy;
using PupSurvivors.Equipment;
using PupSurvivors.Weapon;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace PupSurvivors.Weapon
{
    public class FailNot : WeaponBase
    {
        private GameObject _arrowSummonedAsset;
        private FailNotSummoned _summoned;

        public ProjectilePool<FailNotProjectile> ArrowPool { get; private set; }

        protected override async UniTask InitializeAfter()
        {
            ArrowPool = new ProjectilePool<FailNotProjectile>(this);
            await ArrowPool.Initialize();
             await SummonedWeaponBase<FailNotSummoned, FailNot>.Initialize(this);
             _summoned = SummonedWeaponBase<FailNotSummoned, FailNot>.CreateInstance();
        }
    }
}