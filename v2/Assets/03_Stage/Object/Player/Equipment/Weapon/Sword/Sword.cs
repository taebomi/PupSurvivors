using Cysharp.Threading.Tasks;

namespace PupSurvivors.Equipment
{
    public class Sword : WeaponBase
    {
        private SwordSummoned _summoned;
        
        protected override async UniTask InitAfter()
        {
            _summoned = await WeaponSummonedBase<SwordSummoned, Sword>.CreateInstance(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Destroy(_summoned);
        }
    }
}