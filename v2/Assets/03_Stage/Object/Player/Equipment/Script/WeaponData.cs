using UnityEngine;
using NotImplementedException = System.NotImplementedException;

namespace PupSurvivors.Equipment
{
    
    [CreateAssetMenu(menuName = "PupSurvivors/Weapon Data", fileName = "Weapon Data", order = 1001)]
    public class WeaponData : EquipmentData
    {
        public int projectileCapacity;
        public WeaponStats[] levelData;

        public override int GetMaxLevel()
        {
            return levelData.Length;
        }
    }
}