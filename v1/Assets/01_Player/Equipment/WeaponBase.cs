using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.Pool;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace PupSurvivors.Equipment
{
    public abstract class WeaponBase : EquipmentBase
    {
        public WeaponData WeaponData { get; private set; }
        public WeaponStats ModifiedWeaponStats { get; private set; }
        
        public Action AttackAction;
        public UnityEvent StatChangedEvent { get; private set; }
        
        protected abstract UniTask InitializeAfter();

        protected override async UniTaskVoid Initialize()
        {
            WeaponData = StageManager.Instance.EquipmentDB.equipmentDict[GetType().Name] as WeaponData;
            Player.StatUpdatedEvent.AddListener(OnPlayerStatsUpdated);
            ModifiedWeaponStats = new WeaponStats();
            StatChangedEvent = new UnityEvent();
            await InitializeAfter();
            SetLevel(1);
            AttackAction?.Invoke();
        }

        public override EquipmentData GetEquipmentData()
        {
            return WeaponData;
        }

        public (float, bool) GetRandomDamage()
        {
            var randomDamage = ModifiedWeaponStats.defaultDamage *
                               Random.Range(WeaponStats.RandomMinDamage, WeaponStats.RandomMaxDamage);
            bool isCritical;
            if (Random.value < Player.CurrentStats.criticalRate)
            {
                isCritical = true;
                randomDamage *= Player.CurrentStats.criticalMultiplier;
            }
            else
            {
                isCritical = false;
            }

            return (randomDamage, isCritical);
        }

        public override void LevelUp()
        {
            CurrentLevel++;
            SetLevel(CurrentLevel);
            AttackAction?.Invoke();
        }

        private void SetLevel(int level)
        {
            CurrentLevel = Mathf.Clamp(level, 1, WeaponData.levelData.Length);
            OnPlayerStatsUpdated(Player.CurrentStats);
        }

        protected void OnPlayerStatsUpdated(CharacterStats stats)
        {
            ModifiedWeaponStats.CalculateStats(WeaponData.levelData[CurrentLevel - 1], Player.CurrentStats);
            StatChangedEvent.Invoke();
        }
    }
}