using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using PupSurvivors.Equipment;
using UnityEngine;
using UnityEngine.Events;

public partial class PlayerController
{
    [field: Header("스텟")]
    [field: SerializeField]
    public CharacterStats OriginalStats { get; private set; }
    [field: SerializeField] public CharacterStats CurrentStats { get; private set; }
    
    [field:SerializeField] public UnityEvent<CharacterStats> StatUpdatedEvent { get; private set; }


    private void InitializeStats(CharacterData characterData)
    {
        OriginalStats = characterData.GetStats();
        CurrentStats = OriginalStats with { };
        StatUpdatedEvent.Invoke(CurrentStats);
    }

    public void OnEquipmentUpdated(EquipmentType updatedType, Dictionary<string, EquipmentBase> equippedDict)
    {
        // 무기 변동 시에는 할 것 없음
        if (updatedType is EquipmentType.Weapon)
        {
            return;
        }
        
        // 악세사리 변동 시 
        var newStats = new CharacterStats();
        foreach (var (_, value) in equippedDict)
        {
            if (value is AccessoryBase accessoryBase)
            {
                newStats.AddStats(accessoryBase.GetCurrentLevelData());
            }   
        }

        CurrentStats = OriginalStats.Calculate(newStats);
        StatUpdatedEvent.Invoke(CurrentStats);
    }

    public void OnStatsUpdated(CharacterStats _)
    {
        if (CurrentStats.sp != _maxSp)
        {
            MaxSpChangedEvent.Invoke(CurrentStats.sp);
        }

        accelerationSpeed = CurrentStats.movementSpeed * 6f;
        deaccelerationSpeed = CurrentStats.movementSpeed * 4f;
    }
    
    
}