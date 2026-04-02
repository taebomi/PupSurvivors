using Cysharp.Threading.Tasks;
using UnityEngine;

namespace PupSurvivors.Equipment
{
    public class Shield : WeaponBase
    {
        private WeaponProjectilePool<ShieldProjectile> _pool;

        private float _cooldownTimer;
        private float _defaultRange, _addRange;
        private float _radius;

        protected override async UniTask InitAfter()
        {
            _pool = new WeaponProjectilePool<ShieldProjectile>(this);
            await _pool.Init();
            _cooldownTimer = 0;
            OnStatsUpdated();
            StatsChangedEvent.AddListener(OnStatsUpdated);
            ActionOnLevelUp += () =>
            {
                _cooldownTimer = 0f;
                Attack().Forget();
            };
            CheckCooldown().Forget();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _pool.Dispose();
            ActionOnLevelUp = null;
            StatsChangedEvent.RemoveListener(OnStatsUpdated);
        }

        private void OnStatsUpdated()
        {
            _radius = CurWeaponStats.floatOptionDict[WeaponStats.FloatOption.DefaultRange] +
                      CurWeaponStats.floatOptionDict[WeaponStats.FloatOption.AddRange] * CurWeaponStats.size;
        }


        private async UniTaskVoid CheckCooldown()
        {
            while (DestroyCts.IsCancellationRequested is false)
            {
                if (_cooldownTimer > CurWeaponStats.cooldown)
                {
                    _cooldownTimer = 0f;
                    Attack().Forget();
                }

                _cooldownTimer += Time.deltaTime;
                await UniTask.Yield(DestroyCts.Token);
            }
        }

        private async UniTaskVoid Attack()
        {
            var shieldArr = new ShieldProjectile[CurWeaponStats.amount];
            CreateShields();
            await Rotate();
            RemoveShields();
            shieldArr = null;

            void CreateShields()
            {
                for (int i = 0; i < CurWeaponStats.amount; i++)
                {
                    var projectile = _pool.Pop();
                    projectile.Appear().Forget();
                    shieldArr[i] = projectile;
                }
            }

            async UniTask Rotate()
            {
                var timer = 0f;
                var duration = CurWeaponStats.duration;
                var curAngle = 0f;
                var angleInterval = 360f / CurWeaponStats.amount;
                while (timer < duration)
                {
                    for (int i = 0; i < shieldArr.Length; i++)
                    {
                        var deg = curAngle + angleInterval * i;
                        var rad = deg * Mathf.Deg2Rad;
                        shieldArr[i].transform.position = transform.position +
                                                          new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) *
                                                          _radius;
                    }

                    curAngle += Time.deltaTime * CurWeaponStats.speed;
                    timer += Time.deltaTime;
                    await UniTask.Yield(DestroyCts.Token);
                }
            }

            void RemoveShields()
            {
                foreach (var shieldProjectile in shieldArr)
                {
                    shieldProjectile.Disappear().Forget();
                }
            }
        }
    }
}