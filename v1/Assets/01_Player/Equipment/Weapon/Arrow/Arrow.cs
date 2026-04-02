using Cysharp.Threading.Tasks;

namespace PupSurvivors.Equipment
{
    public class Arrow : WeaponBase
    {
        private ArrowSummoned _arrowSummoned;
        public ProjectilePool<ArrowProjectile> ArrowPool { get; private set; }
        
        protected override async UniTask InitializeAfter()
        {
            ArrowPool = new ProjectilePool<ArrowProjectile>(this);
            await ArrowPool.Initialize();
            await SummonedWeaponBase<ArrowSummoned, Arrow>.Initialize(this);
            _arrowSummoned = SummonedWeaponBase<ArrowSummoned, Arrow>.CreateInstance();
        }


        protected override void OnDestroy()
        {
            base.OnDestroy();
            Destroy(_arrowSummoned.gameObject);
            ArrowPool.Dispose();
            SummonedWeaponBase<ArrowSummoned,Arrow>.Dispose();
        }
    }
}