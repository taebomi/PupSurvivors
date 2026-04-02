using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using PupSurvivors.Equipment;
using UnityEngine;

public class AccessoryBase : EquipmentBase
{
    public AccessoryData AccessoryData { get; private set; }
    
    protected override UniTaskVoid Initialize()
    {
        AccessoryData = StageManager.Instance.EquipmentDB.equipmentDict[GetType().Name] as AccessoryData;
        return default;
    }

    public override EquipmentData GetEquipmentData()
    {
        return AccessoryData;
    }

    public override void LevelUp()
    {
        CurrentLevel++;
    }

    public CharacterStats GetCurrentLevelData()
    {
        return AccessoryData.levelData[CurrentLevel - 1];
    }
}