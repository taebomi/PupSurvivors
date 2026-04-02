using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

// ReSharper disable StaticMemberInGenericType

namespace PupSurvivors.Equipment
{
    public abstract class SummonedWeaponBase<T, U> : MonoBehaviour
        where T : SummonedWeaponBase<T, U>
        where U : WeaponBase
    {
        protected static U Weapon;
        protected static WeaponStats ModifiedStats;
        private static GameObject _asset;

        protected CancellationTokenSource DisableCts;


        public static async UniTask Initialize(U weapon)
        {
            Weapon = weapon;
            ModifiedStats = weapon.ModifiedWeaponStats;
            _asset = await Addressables.LoadAssetAsync<GameObject>($"{weapon.WeaponData.equipmentName}_Summoned");
        }

        public static void Dispose()
        {
            Addressables.Release(_asset);
        }

        public static T CreateInstance()
        {
            var summoned = Instantiate(_asset, StageManager.Instance.PlayerPoolContainer).GetComponent<T>();
            Weapon.StatChangedEvent.AddListener(summoned.UpdateStats);
            summoned.InitializeAfter();
            summoned.UpdateStats();
            return summoned;
        }

        public abstract void InitializeAfter();
        public abstract void Attack();

        public abstract void UpdateStats();

        protected void OnEnable()
        {
            DisableCts = new CancellationTokenSource();
        }

        protected void OnDisable()
        {
            DisableCts.CancelAndDispose();
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
    }
}

// ReSharper disable StaticMemberInGenericType