using Cysharp.Threading.Tasks;

namespace PupSurvivors.Equipment
{
    public class Haki : WeaponBase
    {
        private HakiSummoned _summoned;

        protected override async UniTask InitializeAfter()
        {
            await SummonedWeaponBase<HakiSummoned,Haki>.Initialize(this);
            _summoned = SummonedWeaponBase<HakiSummoned, Haki>.CreateInstance();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Destroy(_summoned.gameObject);
            SummonedWeaponBase<HakiSummoned,Haki>.Dispose();
        }
    }
}