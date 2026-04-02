using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using PupSurvivors.Enemy;
using PupSurvivors.Weapon;
using UnityEngine;

namespace PupSurvivors.Equipment
{
    public class DeathSentenceProjectile : WeaponProjectileBase<DeathSentenceProjectile>
    {
        private HealthSystemBase _target;
        
        public void SetTarget(HealthSystemBase target)
        {
            _target = target;
        }

        private void Update()
        {
            if (_target.IsLive)
            {
                transform.position = _target.transform.position;
            }
        }

        public void AnimationEvent_OnAnimationFinished()
        {
            if (_target.IsLive)
            {
                _target.Kill();
            }
            ManagedPool.Push(this);
        }

        public override void OnStatsChanged()
        {
        }
    }
}