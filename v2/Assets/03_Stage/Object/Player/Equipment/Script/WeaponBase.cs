using System;
using Cysharp.Threading.Tasks;
using PupSurvivors.Stage;
using PupSurvivors.Stage.UI;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace PupSurvivors.Equipment
{
    public abstract class 
        WeaponBase : EquipmentBase
    {
        public WeaponData WeaponData { get; private set; }
        public override EquipmentData EquipmentData => WeaponData;
        
        public WeaponStats OriWeaponStats { get; private set; }
        public WeaponStats CurWeaponStats { get; private set; }
        public UnityEvent StatsChangedEvent { get; private set; }

        public Action ActionOnLevelUp;

        public Transform Container { get; protected set; }
        
        protected override async UniTaskVoid Initialize()
        {
            var weaponName = GetType().Name;
            Container = new GameObject($"{weaponName}").transform;
            Container.SetParent(Player.Equipment.EquipmentContainer);
            WeaponData = StageManager.Instance.EquipmentDB.equipmentDict[weaponName] as WeaponData;
            
            await UniTask.WaitUntil(() => StageManager.Instance.State == StageState.Start,
                cancellationToken: DestroyCts.Token);


            Player.StatsUpdatedEvent.AddListener(OnPlayerStatsUpdated);

            CurWeaponStats = new WeaponStats();
            StatsChangedEvent = new UnityEvent();
            SetLevel(1);
            await InitAfter();
            ActionOnLevelUp?.Invoke();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Player.StatsUpdatedEvent.RemoveListener(OnPlayerStatsUpdated);
        }

        protected abstract UniTask InitAfter();

        public override void LevelUp()
        {
            SetLevel(CurLevel + 1);
            ActionOnLevelUp?.Invoke();
        }

        public void SetLevel(int level)
        {
            CurLevel = Mathf.Clamp(level, 1, WeaponData.levelData.Length);
            OriWeaponStats = WeaponData.levelData[CurLevel - 1];
            ApplyStats();
        }

        public (float, bool) GetRandomDamage()
        {
            var randomDamage = CurWeaponStats.defaultDamage *
                               Random.Range(WeaponStats.RandomMinDamage, WeaponStats.RandomMaxDamage);
            bool isCritical;
            if (Random.value < Player.CurStats.criticalRate)
            {
                isCritical = true;
                randomDamage *= Player.CurStats.criticalMultiplier;
            }
            else
            {
                isCritical = false;
            }

            return (randomDamage, isCritical);
        }

        private void OnPlayerStatsUpdated(CharacterStats stats) => ApplyStats();

        private void ApplyStats()
        {
            CurWeaponStats.CalculateStats(OriWeaponStats, Player.CurStats);
            StatsChangedEvent.Invoke();
        }
    }
}