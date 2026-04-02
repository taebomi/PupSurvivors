using UnityEngine;
// ReSharper disable NonReadonlyMemberInGetHashCode


namespace PupSurvivors.Equipment
{
    public abstract class EquipmentData : ScriptableObject
    {
        public const int NormalWeaponMaxLevel = 7;
        public const int RareWeaponMaxLevel = 5;
        public const int NormalAccessoryMaxLevel = 5;
        public const int RareAccessoryMaxLevel = 3;
        
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