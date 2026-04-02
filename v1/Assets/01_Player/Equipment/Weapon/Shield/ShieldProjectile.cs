using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using PupSurvivors.Enemy;
using UnityEngine;
using UnityEngine.Serialization;

namespace PupSurvivors.Equipment
{
    public class ShieldProjectile : WeaponProjectileBase<ShieldProjectile>
    {
        [SerializeField] private SpriteRenderer mainSr, shadowSr;
        [SerializeField] private SpriteContainer sprites;

        private bool _isScaleChanging;

        private float _radius;
        private Transform _targetTr;

        public void Init(Transform targetTr)
        {
            _targetTr = targetTr;
        }

        public override void OnStatsChanged()
        {
            if (!_isScaleChanging)
            {
                transform.localScale = Vector3.one * ModifiedStats.size;
            }

            const float defaultRadius = 1.5f;
            const float radiusMultiplier = 1.5f;

            _radius = defaultRadius + radiusMultiplier * ModifiedStats.size;
        }

        public async UniTaskVoid Appear()
        {
            _isScaleChanging = true;
            transform.localScale = Vector3.zero;
            var timer = 0f;
            const float duration = 0.25f;
            while (timer < duration)
            {
                transform.localScale = Vector3.one * Easing.OutBack(timer, duration) * ModifiedStats.size;
                timer += Time.deltaTime;
                await UniTask.Yield(DisableCts.Token);
            }

            transform.localScale = Vector3.one * ModifiedStats.size;
            _isScaleChanging = false;
        }

        public async UniTaskVoid Disappear()
        {
            _isScaleChanging = true;
            var timer = 0f;
            const float duration = 0.25f;
            while (timer < duration)
            {
                transform.localScale = Vector3.one * (1f - Easing.InBack(timer, duration)) * ModifiedStats.size;
                timer += Time.deltaTime;
                await UniTask.Yield(DisableCts.Token);
            }

            _isScaleChanging = false;
            ManagedPool.Push(this);
        }

        public void Set(float deg)
        {
            // 각도에 따른 위치 설정
            var rad = Mathf.Deg2Rad * deg;
            transform.position = _targetTr.position + new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * _radius;

            // 각도에 따른 스프라이트 설정
            const int spriteNum = 8;
            const float spriteInterval = 360f / spriteNum;
            const float spriteIntervalInverse = 1 / spriteInterval;

            var spriteIdx = (int)((deg * spriteIntervalInverse + 0.5f) % spriteNum);
            mainSr.sprite = sprites.data[spriteIdx];
            shadowSr.sprite = sprites.data[spriteIdx + spriteNum];
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            DefaultProjectileTriggerEnter(col);
        }
    }
}