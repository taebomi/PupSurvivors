using System;
using UnityEngine;

namespace PupSurvivors.Equipment
{
    public abstract class EquipmentData : ScriptableObject
    {
        public string equipmentName;
        public Sprite icon;
        public EquipmentType type;
        public EquipmentRarity rarity;
        public int weight;

        public abstract int GetMaxLevel();
    }

    public enum EquipmentRarity
    {
        Normal,
        Rare,
    }

    public enum EquipmentType
    {
        Weapon,
        Accessory,
    }
}