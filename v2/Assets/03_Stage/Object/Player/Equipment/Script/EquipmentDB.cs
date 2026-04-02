using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using AYellowpaper.SerializedCollections;
using PupSurvivors.Stage;
using UnityEngine;
using UnityEngine.Serialization;

namespace PupSurvivors.Equipment
{
    [CreateAssetMenu(menuName = "PupSurvivors/Equipment DB", fileName = "EquipmentDB", order = 1001)]
    public class EquipmentDB : ScriptableObject
    {
        [SerializedDictionary("Equipment Name", "Equipment Data")]
        public SerializedDictionary<string, EquipmentData> equipmentDict;

        [SerializedDictionary("Result Equipment Name", "Crafting Recipe")]
        public SerializedDictionary<string, Recipe> recipeDict;

        public void UpdateAddableTable(PlayerEquipment playerEquipment)
        {
            var equippedDict = playerEquipment.EquippedDict;

            playerEquipment.AddableTable.TotalWeight = 0;
            playerEquipment.AddableTable.DataSet.Clear();

            foreach (var (equipmentName, equipmentData) in equipmentDict)
            {
                // todo 장비 해금 여부 체크, 해금 안될 시 continue

                if (equippedDict.TryGetValue(equipmentName, out var equipmentBase))
                {
                    if (equipmentBase.CurLevel == equipmentData.GetMaxLevel())
                    {
                        continue;
                    }
                }
                else
                {
                    if (equipmentData.rarity is not EquipmentRarity.Normal)
                    {
                        continue;
                    }

                    switch (equipmentData.type)
                    {
                        case EquipmentType.Weapon:
                            if (playerEquipment.CurWeaponNum >= playerEquipment.CurMaxWeaponNum)
                            {
                                continue;
                            }

                            break;
                        case EquipmentType.Accessory:
                            if (playerEquipment.CurAccessoryNum >= playerEquipment.CurMaxAccessoryNum)
                            {
                                continue;
                            }

                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                playerEquipment.AddableTable.DataSet.Add(equipmentData);
                playerEquipment.AddableTable.TotalWeight += equipmentData.weight;
            }
        }

        public (int, HashSet<EquipmentData>) GetAvailableEquipmentList(Dictionary<string, EquipmentBase> equippedDict)
        {
            var totalRarity = 0;
            var availableSet = new HashSet<EquipmentData>(equipmentDict.Count);

            var equippedWeaponCount = 0;
            var equippedAccessoryCount = 0;
            foreach (var (equipmentName, _) in equippedDict)
            {
                switch (equipmentDict[equipmentName].type)
                {
                    case EquipmentType.Weapon:
                        equippedWeaponCount++;
                        break;
                    case EquipmentType.Accessory:
                        equippedAccessoryCount++;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            foreach (var (_, equipmentData) in equipmentDict)
            {
                // todo 장비 해금 여부 체크하기


                if (equippedDict.TryGetValue(equipmentData.equipmentName, out var equipmentBase))
                {
                    if (equipmentBase.CurLevel == equipmentData.GetMaxLevel())
                    {
                        continue;
                    }
                }
                else
                {
                    if (equipmentData.type is EquipmentType.Weapon &&
                        equippedWeaponCount > PlayerEquipment.MaxWeaponNum)
                    {
                        continue;
                    }

                    if (equipmentData.type is EquipmentType.Accessory &&
                        equippedAccessoryCount > PlayerEquipment.MaxAccessoryNum)
                    {
                        continue;
                    }

                    if (equipmentData.rarity is not EquipmentRarity.Normal)
                    {
                        continue;
                    }
                }

                availableSet.Add(equipmentData);
                totalRarity += equipmentData.weight;
            }

            return (totalRarity, availableSet);
        }

        public (int, List<EquipmentData>) GetCraftableEquipment(Dictionary<string, EquipmentBase> equippedDict)
        {
            var craftableEquipmentDataList = new List<EquipmentData>(PlayerEquipment.MaxWeaponNum);
            var totalWeight = 0;

            foreach (var (result, recipe) in recipeDict)
            {
                // todo 결과물 장비 해금 체크

                // 이미 가지고 있으면 패스
                if (equippedDict.ContainsKey(result))
                {
                    continue;
                }

                var hasAllRequired = true;
                foreach (var ingredient in recipe.ingredients)
                {
                    // 재료가 미착용 중이면 패스
                    if (!equippedDict.TryGetValue(ingredient.equipmentData.equipmentName, out var equipped))
                    {
                        hasAllRequired = false;
                        break;
                    }

                    // 재료 장비가 조건을 만족하지 못하면 패스
                    if (ingredient.needMaxLevel && equipped.CurLevel != ingredient.equipmentData.GetMaxLevel())
                    {
                        hasAllRequired = false;
                        break;
                    }
                }

                if (hasAllRequired)
                {
                    craftableEquipmentDataList.Add(recipe.result);
                    totalWeight += recipe.result.weight;
                }
            }

            return (totalWeight, craftableEquipmentDataList);
        }


        [Serializable]
        public class Recipe
        {
            public EquipmentData result;
            public Ingredient[] ingredients;
        }

        [Serializable]
        public class Ingredient
        {
            public EquipmentData equipmentData;
            public bool willRemoved;
            public bool needMaxLevel;
        }
    }
}