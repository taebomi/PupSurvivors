using System;
using System.Collections;
using System.Collections.Generic;
using PupSurvivors.Equipment;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentIcon : MonoBehaviour
{
    // todo 이후 클릭 시 장비 정보도 띄워주기
    [SerializeField] private Image container, icon;
    [SerializeField] private TMP_Text level;
    [SerializeField] private SpriteContainer containerSprites;


    public void Set(EquipmentData data, int currentLevel)
    {
        container.sprite = data.rarity switch
        {
            EquipmentRarity.Normal => containerSprites.data[1],
            EquipmentRarity.Rare => containerSprites.data[2],
            _ => throw new ArgumentOutOfRangeException()
        };
        
        var maxLevel = data.type switch
        {
            EquipmentType.Weapon => data.rarity switch
            {
                EquipmentRarity.Normal => EquipmentData.NormalWeaponMaxLevel,
                EquipmentRarity.Rare => EquipmentData.RareWeaponMaxLevel,
                _ => throw new ArgumentOutOfRangeException()
            },
            EquipmentType.Accessory => data.rarity switch
            {
                EquipmentRarity.Normal => EquipmentData.NormalAccessoryMaxLevel,
                EquipmentRarity.Rare => EquipmentData.RareAccessoryMaxLevel,
                _ => throw new ArgumentOutOfRangeException()
            },
            _ => throw new ArgumentOutOfRangeException()
        };

        icon.sprite = data.icon;
        level.text = currentLevel == maxLevel ? "MAX" : $"Lv. {currentLevel}";
        
        icon.enabled = true;
        level.enabled = true;
    }

    public void Clear()
    {
        container.sprite = containerSprites.data[0];
        level.enabled = false;
        icon.enabled = false;
    }
}