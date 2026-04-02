using UnityEngine;


namespace PupSurvivors.Equipment
{
    [CreateAssetMenu(menuName = "TaeBoMi/Accessory Data", fileName = "AccessoryData", order = 1002)]
    public class AccessoryData : EquipmentData
    {
        public CharacterStats[] levelData;
        
        public override int GetMaxLevel()
        {
            return levelData.Length;
        }
    }
}