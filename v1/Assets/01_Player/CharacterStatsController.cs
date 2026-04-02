using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CharacterStatsController : MonoBehaviour
{
    [field:SerializeField] public CharacterStats OriginalStats { get; private set; }
    [field:SerializeField] public CharacterStats ModifiedStats { get; private set; }
    
    [field:SerializeField] public UnityEvent<CharacterStats> StatsUpdatedEvent { get; private set; }


    public void Initialize(CharacterData characterData)
    {
        OriginalStats = characterData.GetStats();
    }

    private void OnEquipmentChanged()
    {
        UpdateModifiedStats();
    }

    public void UpdateModifiedStats()
    {
        ModifiedStats.Reset();
        
    }
}
