using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using PupSurvivors.Enemy;
using PupSurvivors.Weapon;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

namespace PupSurvivors.Equipment
{
    public class DivineFootprintsProjectile : WeaponProjectileBase<DivineFootprintsProjectile>
    {
        [SerializeField] private ParticleSystem effectParticle;
    
        protected override void OnEnable()
        {
            base.OnEnable();
            StopEffectAfterDuration().Forget();
            CheckInterval().Forget();
        }

        private async UniTaskVoid CheckInterval()
        {
            var timer = 0f;
            while (true)
            {
                while (timer > ModifiedStats.interval)
                {
                    timer -= ModifiedStats.interval;
                    Attack();
                }
                timer += Time.deltaTime;
                await UniTask.Yield(DisableCts.Token);
            }
        }

        private void Attack()
        {        
            Physics2D.OverlapCircle(transform.position, ModifiedStats.size, EnemyManager.Instance.ContactFilter2D,
                TaeBoMiCache.TempColliderList);
            DoDamage(TaeBoMiCache.TempColliderList);
        }

        private async UniTaskVoid StopEffectAfterDuration()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(ModifiedStats.duration), cancellationToken: DisableCts.Token);
            effectParticle.Stop();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            ManagedPool.Push(this);
        }

        public override void OnStatsChanged()
        {
        }

    }
}