using System.Collections;
using System.Collections.Generic;
using PupSurvivors.Equipment;
using UnityEngine;
using UnityEngine.Events;

namespace PupSurvivors.Stage
{
    public partial class Player
    {
        [Header("Stats"), SerializeField] private CharacterStats oriStats;
        [field: SerializeField] public CharacterStats CurStats { get; private set; }
        [field: SerializeField] public UnityEvent<CharacterStats> StatsUpdatedEvent { get; private set; }

        private void InitializeStats()
        {
            oriStats = new CharacterStats(CurCharacterData.stats);
            CurStats = new CharacterStats(CurCharacterData.stats);
            StatsUpdatedEvent.Invoke(CurStats);
        }

        public void OnEquipmentChanged(Dictionary<string, EquipmentBase> equippedDict)
        {
            var newStats = new CharacterStats();
            foreach (var (_, equipment) in equippedDict)
            {
                if (equipment is AccessoryBase accessory)
                {
                    newStats.Add(accessory.GetCurLevelData());
                }
            }

            CurStats = new CharacterStats(oriStats);
            CurStats.Calculate(newStats);
            StatsUpdatedEvent.Invoke(CurStats);
        }

        public void OnStatsUpdated(CharacterStats _)
        {
            // todo sp 변경 시 연결
        }
    }
}