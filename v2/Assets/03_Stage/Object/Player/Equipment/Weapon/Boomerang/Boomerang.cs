using Cysharp.Threading.Tasks;

namespace PupSurvivors.Equipment
{
    public class Boomerang : WeaponBase
    {
        private BoomerangSummoned _summoned;

        protected override async UniTask InitAfter()
        {
            _summoned = await WeaponSummonedBase<BoomerangSummoned, Boomerang>.CreateInstance(this);
            _summoned.transform.position = Player.transform.position;
            ThrowBoomerang();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Destroy(_summoned);
        }

        public void ThrowBoomerang()
        {
            _summoned.Throw(Player.LastInputDir).Forget();
        }
    }
}