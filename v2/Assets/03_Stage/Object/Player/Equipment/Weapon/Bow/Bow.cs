using Cysharp.Threading.Tasks;
using UnityEngine;

namespace PupSurvivors.Equipment
{
    public class Bow : WeaponBase
    {
        public WeaponProjectilePool<BowProjectile> ProjectilePool { get; private set; }

        private BowSummoned _summoned;

        protected override async UniTask InitAfter()
        {
            ProjectilePool = new WeaponProjectilePool<BowProjectile>(this);
            await ProjectilePool.Init();
            _summoned = await WeaponSummonedBase<BowSummoned, Bow>.CreateInstance(this);
            
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Destroy(_summoned);
            ProjectilePool.Dispose();
        }
    }
}