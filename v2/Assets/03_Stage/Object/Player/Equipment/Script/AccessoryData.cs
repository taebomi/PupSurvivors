using UnityEngine;

namespace PupSurvivors.Equipment
{
    [CreateAssetMenu(menuName = "PupSurvivors/Accessory Data", fileName = "AccessoryData", order = 1001)]
    public class AccessoryData : EquipmentData
    {
        public CharacterStats[] levelData;

        public override int GetMaxLevel()
        {
            return levelData.Length;
        }
    }
}