using Cysharp.Threading.Tasks;

namespace PupSurvivors.Equipment
{
    public class Sharanga : WeaponBase
    {
        private SharangaSummoned _summoned;
        public ProjectilePool<SharangaProjectile> ProjectilePool { get; private set; }

        protected override async UniTask InitializeAfter()
        {
            ProjectilePool = new ProjectilePool<SharangaProjectile>(this);
            await ProjectilePool.Initialize();
            await SummonedWeaponBase<SharangaSummoned, Sharanga>.Initialize(this);
            _summoned = SummonedWeaponBase<SharangaSummoned, Sharanga>.CreateInstance();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Destroy(_summoned.gameObject);
            ProjectilePool.Dispose();
            SummonedWeaponBase<SharangaSummoned, Sharanga>.Dispose();
        }
    }
}