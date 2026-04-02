using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace PupSurvivors.Equipment
{
    public class ShieldProjectile : WeaponProjectileBase<ShieldProjectile>
    {
        [SerializeField] private AudioEventChannelSO seEventChannelSO;
        [SerializeField] private AudioClip appearSE;
        
        private bool _isScaleChanging;

        private LinkedList<Collider2D> _colliderList;

        private float _damageTimer;

        private void Awake()
        {
            _colliderList = new LinkedList<Collider2D>();
        }

        private void Update()
        {
            const float damageInterval = 0.125f;
            if (_damageTimer > damageInterval)
            {
                foreach (var coll in _colliderList)
                {
                    DefaultAttack(coll);
                }

                _damageTimer = 0f;
            }
            
            _damageTimer += Time.deltaTime;
        }

        public override void OnStatsChanged()
        {
            if (!_isScaleChanging)
            {
                transform.localScale = Vector3.one * CurStats.size;
            }
        }

        public async UniTaskVoid Appear()
        {
            _isScaleChanging = true;

            var timer = 0f;
            const float appearDuration = 0.25f;
            while (timer < appearDuration)
            {
                transform.localScale = Vector3.one * (Easing.OutBack(timer, appearDuration) * CurStats.size);
                timer += Time.deltaTime;
                await UniTask.Yield(DisableCts.Token);
            }

            transform.localScale = Vector3.one * CurStats.size;
            _isScaleChanging = false;
        }

        public async UniTaskVoid Disappear()
        {
            _isScaleChanging = true;
            var timer = 0f;
            const float disappearDuration = 0.25f;
            while (timer < disappearDuration)
            {
                transform.localScale = Vector3.one * (1f - Easing.InBack(timer, disappearDuration)) * CurStats.size;
                timer += Time.deltaTime;
                await UniTask.Yield(DisableCts.Token);
            }
            
            _isScaleChanging = false;
            Pool.Push(this);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            _colliderList.AddLast(other);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            _colliderList.Remove(other);
        }
    }
}