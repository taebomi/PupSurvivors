using PupSurvivors.Equipment;
using UnityEngine;

namespace PupSurvivors.Weapon
{
    public class ClawProjectile : WeaponProjectileBase<ClawProjectile>
    {
        private void OnTriggerEnter2D(Collider2D col)
        {
            DefaultProjectileTriggerEnter(col);
        }

        public void AnimationEvent_AttackFinished()
        {
            ManagedPool.Push(this);
        }

        public override void OnStatsChanged()
        {
            SetScale(ModifiedStats.size);
        }

    }
}