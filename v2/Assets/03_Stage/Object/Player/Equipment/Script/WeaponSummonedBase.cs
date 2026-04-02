using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using PupSurvivors.Stage;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace PupSurvivors.Equipment
{
    public abstract class WeaponSummonedBase<T, U> : MonoBehaviour
        where T : WeaponSummonedBase<T, U>
        where U : WeaponBase
    {
        protected U Weapon;
        protected WeaponStats CurStats;

        protected CancellationTokenSource DisableCts;

        public static async UniTask<T> CreateInstance(U weapon)
        {
            var handle =
                Addressables.LoadAssetAsync<GameObject>($"Equipment/Weapon/{weapon.WeaponData.equipmentName}_Summoned");
            var prefab = await handle;
            Addressables.Release(handle);
            var summoned = Instantiate(prefab, weapon.Container).GetComponent<T>();
            summoned.Init(weapon);
            return summoned;
        }

        private void Init(U weapon)
        {
            Weapon = weapon;
            CurStats = weapon.CurWeaponStats;
            Weapon.StatsChangedEvent.AddListener(OnStatsUpdated);
            InitAfter();
            OnStatsUpdated();
        }

        protected abstract void InitAfter();
        protected abstract void OnStatsUpdated();

        protected virtual void OnEnable()
        {
            DisableCts?.Dispose();
            DisableCts = new CancellationTokenSource();
        }

        protected virtual void OnDisable()
        {
            DisableCts.Cancel();
        }
    }
}