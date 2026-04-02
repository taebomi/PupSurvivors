using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using PupSurvivors.Equipment;
using PupSurvivors.Weapon;
using UnityEngine;

namespace PupSurvivors.Weapon
{
    public class RainbowProjectile : WeaponProjectileBase<RainbowProjectile>
    {
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private SpriteRenderer sr;
        [SerializeField] private SpriteContainer spriteContainer;

        public enum Color
        {
            Red,
            Yellow,
            Green,
            Blue,
            Purple,
        }

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

        public void SetColor(Color color)
        {
            sr.sprite = spriteContainer.data[(int)color];
        }
    }
}