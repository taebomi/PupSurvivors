using System;
using System.Threading;
using PupSurvivors.Stage;
using UnityEngine;
using UnityEngine.Pool;

namespace PupSurvivors.Equipment
{
    public abstract class WeaponProjectileBase<T> : MonoBehaviour where T : WeaponProjectileBase<T>
    {
        [SerializeField] protected DamagableSO damagableSO;
        
        protected WeaponBase Weapon;
        protected WeaponStats OriStats, CurStats;
        protected WeaponProjectilePool<T> Pool;

        protected CancellationTokenSource DisableCts;

        protected (float, bool) CurDamage;
        protected int RemainedPierce;

        public abstract void OnStatsChanged();


        public void Init(WeaponProjectilePool<T> pool, WeaponBase weaponBase)
        {
            Pool = pool;
            Weapon = weaponBase;
            OriStats = Weapon.OriWeaponStats;
            CurStats = Weapon.CurWeaponStats;
        }

        private void OnEnable()
        {
            DisableCts = new CancellationTokenSource();
        }

        private void OnDisable()
        {
            DisableCts.CancelAndDispose();
        }

        public void SetActive()
        {
            gameObject.SetActive(true);
            ResetProjectile();
        }

        public void SetDeactive()
        {
            gameObject.SetActive(false);
        }

        protected void ResetProjectile()
        {
            CurDamage = Weapon.GetRandomDamage();
            RemainedPierce = CurStats.pierce;
        }

        protected void DefaultAttack(Collider2D col)
        {
            if (!damagableSO.CollDict.TryGetValue(col, out var damagable))
            {
#if UNITY_EDITOR
                Debug.Log($"{transform.name} - {col.name}이 EnemyColliderDict에 존재하지 않음.");
#endif
                return;
            }

            damagable.Damage(CurDamage);
            damagable.Knockback(CurStats.knockbackPower);
        }
    }
}