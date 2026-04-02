using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using PupSurvivors.Enemy;
using PupSurvivors.Stage;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;
// ReSharper disable StaticMemberInGenericType

namespace PupSurvivors.Equipment
{
    public abstract class WeaponProjectileBase<T> : MonoBehaviour where T : WeaponProjectileBase<T>
    {
        // 상속받은 클래스 별로 static 변수 존재.
        protected static WeaponBase Weapon;
        protected static WeaponStats ModifiedStats; // 무기 스탯 참조
        protected static ProjectilePool<T> ManagedPool; // 담겨질 풀

        protected CancellationTokenSource DisableCts;

        // 현재 스텟 관련
        protected float CurrentDamage;
        protected bool IsCritical;
        protected int RemainedPierce;


        public abstract void OnStatsChanged(); // 캐릭터 스텟 변동 시 맞추어 업데이트

        public static void Initialize(ProjectilePool<T> pool, WeaponBase weaponBase)
        {
            ManagedPool = pool;
            Weapon = weaponBase;
            ModifiedStats = weaponBase.ModifiedWeaponStats;
        }

        protected virtual void OnEnable()
        {
            DisableCts = new CancellationTokenSource();
            ResetProjectile();
        }

        protected virtual void OnDisable()
        {
            DisableCts.CancelAndDispose();
        }

        protected async UniTask DefaultPushToPoolAfterDuration()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(ModifiedStats.duration), cancellationToken: DisableCts.Token);
            ManagedPool.Push((T)this);
        }


        // 투사체 재생성 시 필요한 값들 초기화
        private void ResetProjectile()
        {
            (CurrentDamage, IsCritical) = Weapon.GetRandomDamage();
            RemainedPierce = ModifiedStats.pierce;
        }

        // 기본 투사체 공격
        protected void DefaultProjectileTriggerEnter(Collider2D col)
        {
            if (!StageManager.Instance.DamagableDict.TryGetValue(col.GetInstanceID(), out var damagable))
            {
#if UNITY_EDITOR
                Debug.Log($"{transform.name} - {col.name}이 EnemyColliderDict에 존재하지 않음.");
#endif
                return;
            }

            damagable.Damage(CurrentDamage, IsCritical);
            damagable.Knockback(ModifiedStats.knockbackPower);
        }
        
        // 기본 관통형 투사체 공격
        protected void DefaultPierceProjectileTriggerEnter(Collider2D col)
        {
            DefaultProjectileTriggerEnter(col);
            RemainedPierce--;
            if (RemainedPierce <= 0)
            {
                ManagedPool.Push((T)this);
            }
        }
        protected void DoDamage(IDamagable target)
        {
            var (damage, isCritical) = Weapon.GetRandomDamage();
            target.Damage(damage, isCritical);
            target.Knockback(ModifiedStats.knockbackPower);
        }

        protected void DoDamage(IEnumerable<IDamagable> targets)
        {
            var (damage, isCritical) = Weapon.GetRandomDamage();
            foreach (var target in targets)
            {
                target.Damage(damage, isCritical);
                target.Knockback(ModifiedStats.knockbackPower);
            }
        }

        protected void DoDamage(IEnumerable<Collider2D> targets)
        {
            var (damage, isCritical) = Weapon.GetRandomDamage();
            var damagableDict = StageManager.Instance.DamagableDict;
            foreach (var targetCollider in targets)
            {
                var target = damagableDict[targetCollider.GetInstanceID()];
                target.Damage(damage, isCritical);
                target.Knockback(ModifiedStats.knockbackPower);
            }
        }

        #region 세팅용

        public virtual T SetPosition(Vector3 pos)
        {
            transform.position = pos;
            return (T)this;
        }

        public T SetLocalPosition(Vector3 pos)
        {
            transform.localPosition = pos;
            return (T)this;
        }

        public T SetScale(float size)
        {
            transform.localScale = new Vector3(size * Mathf.Sign(transform.localScale.x), size, 1f);
            return (T)this;
        }

        public WeaponProjectileBase<T> SetScale(bool isRight, float size)
        {
            transform.localScale = isRight
                ? new Vector3(size, size, 1f)
                : new Vector3(-size, size, 1f);
            return this;
        }

        public virtual void SetDirection(Vector2 dir)
        {
            transform.right = dir;
        }

        #endregion
    }
}