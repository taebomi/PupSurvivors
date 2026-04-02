using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using PupSurvivors.Weapon;
using UnityEngine;
using UnityEngine.Serialization;

namespace PupSurvivors.Equipment
{
    public class SharangaProjectile : WeaponProjectileBase<SharangaProjectile>
    {
        [SerializeField] private LineRenderer lineRenderer;

        public override void OnStatsChanged()
        {
            transform.localScale = Vector3.one * ModifiedStats.size;
        }


        protected override void OnEnable()
        {
            base.OnEnable();
            Play().Forget();
        }

        private async UniTaskVoid Play()
        {

            var timer = 0f;
            const float maxWidth = 1f;
            const float endDuration = 0.2f;
            while (timer < endDuration)
            {
                lineRenderer.startWidth = maxWidth * (1 - Easing.InSine(timer, endDuration));
                timer += Time.deltaTime;
                await UniTask.Yield(DisableCts.Token);
            }

            lineRenderer.startWidth = 0f;

            ManagedPool.Push(this);
        }

        public void OnTriggerEnter2D(Collider2D col)
        {
            DefaultProjectileTriggerEnter(col);
        }

        public override void SetDirection(Vector2 dir)
        {
            transform.right = dir;
        }
    }
}