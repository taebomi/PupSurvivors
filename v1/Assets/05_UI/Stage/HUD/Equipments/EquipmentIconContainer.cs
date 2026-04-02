using System;
using System.Collections;
using System.Collections.Generic;
using PupSurvivors.Equipment;
using UnityEngine;
using UnityEngine.UI;


public class EquipmentIconContainer : MonoBehaviour
{
    [SerializeField] private EquipmentIcon[] weaponIcons, accessoryIcons;

    private void Awake()
    {
        foreach (var equipmentIcon in weaponIcons)
        {
            equipmentIcon.Clear();
        }

        foreach (var accessoryIcon in accessoryIcons)
        {
            accessoryIcon.Clear();
        }
    }

    public void OnEquipmentChanged(EquipmentType type, Dictionary<string, EquipmentBase> equippedBaseDict)
    {
        var equipmentIcons = type switch
        {
            EquipmentType.Weapon => weaponIcons,
            EquipmentType.Accessory => accessoryIcons,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        var idx = 0;
        foreach (var (_, value) in equippedBaseDict)
        {
            var data = value.GetEquipmentData();
            if (data.type != type)
            {
                continue;
            }

            equipmentIcons[idx].Set(data, value.CurrentLevel);
            idx++;
        }

        for (var i = idx; i < equipmentIcons.Length; i++)
        {
            equipmentIcons[i].Clear();
        }
    }
}