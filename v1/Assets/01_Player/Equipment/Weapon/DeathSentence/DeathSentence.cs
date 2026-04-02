using Cysharp.Threading.Tasks;
using UnityEngine;

namespace PupSurvivors.Equipment
{
    public class DeathSentence : WeaponBase
    {
        private GameObject _summonedAsset;
        private DeathSentenceSummoned _summoned;
    
        public ProjectilePool<DeathSentenceProjectile> ProjectilePool { get; private set; }
        protected override async UniTask InitializeAfter()
        {
            ProjectilePool = new ProjectilePool<DeathSentenceProjectile>(this);
            await ProjectilePool.Initialize();
            await SummonedWeaponBase<DeathSentenceSummoned, DeathSentence>.Initialize(this);
            _summoned = SummonedWeaponBase<DeathSentenceSummoned, DeathSentence>.CreateInstance();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Destroy(_summoned);
            ProjectilePool.Dispose();
            SummonedWeaponBase<DeathSentenceSummoned, DeathSentence>.Dispose();
        }
    }
}