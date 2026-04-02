using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using PupSurvivors.Weapon;
using UnityEngine;

namespace PupSurvivors.Equipment
{
    public class ArrowProjectile : WeaponProjectileBase<ArrowProjectile>
    {
        [SerializeField] private Rigidbody2D rb;
        
        public override void OnStatsChanged()
        {
            transform.localScale = Vector3.one * ModifiedStats.size;
        }
        
        protected override void OnEnable()
        {
            base.OnEnable();
            DefaultPushToPoolAfterDuration().Forget();
        }

        public void OnTriggerEnter2D(Collider2D col)
        {
            DefaultPierceProjectileTriggerEnter(col);
        }

        public override void SetDirection(Vector2 dir)
        {
            transform.right = dir;
            rb.linearVelocity = dir * ModifiedStats.speed;
        }
    }
}