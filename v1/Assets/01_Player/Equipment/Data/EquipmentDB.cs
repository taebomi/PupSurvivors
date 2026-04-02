using System;
using System.Collections.Generic;
using System.Linq;
using PupSurvivors.Equipment;
using UnityEngine;
using AYellowpaper.SerializedCollections;
using Unity.VisualScripting;

// [CreateAssetMenu(menuName = "TaeBoMi/EquipmentDB", fileName = "EquipmentDB")]
public class EquipmentDB : ScriptableObject
{
    [SerializedDictionary("Equipment Name", "Equipment Data")]
    public SerializedDictionary<string, EquipmentData> equipmentDict;

    [SerializedDictionary("Result Equipment Name", "Crafting Recipe")]
    public SerializedDictionary<string, CraftingRecipe> recipeDict;

    public (int, HashSet<EquipmentData>) GetAvailableEquipmentList(Dictionary<string, EquipmentBase> equippedBaseDict)
    {
        var totalRarity = 0;
        var availableSet = new HashSet<EquipmentData>(equipmentDict.Count);

        var equippedWeaponNum = 0;
        var equippedAccessoryNum = 0;
        foreach (var (key, _) in equippedBaseDict)
        {
            switch (equipmentDict[key].type)
            {
                case EquipmentType.Weapon:
                    equippedWeaponNum++;
                    break;
                case EquipmentType.Accessory:
                    equippedAccessoryNum++;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        foreach (var (_, value) in equipmentDict)
        {
            // todo 세이브 데이터를 읽어서 해당 장비가 해금되었는지 체크, 아닐 경우 continue

            // 보유 중인 장비인지 체크
            if (equippedBaseDict.TryGetValue(value.equipmentName, out var equipmentBase))
            {
                // 만렙이 아닐 경우에만 목록에 추가
                if (value.GetMaxLevel() == equipmentBase.CurrentLevel)
                {
                    continue;
                }
            }
            else // 보유 중이 아닐 경우
            {
                // 해당 타입 장착 개수가 최대치일 경우 패스
                if (value.type == EquipmentType.Weapon &&
                    equippedWeaponNum == PlayerEquipment.MaxEquipmentNum)
                    continue;

                if (value.type == EquipmentType.Accessory &&
                    equippedAccessoryNum == PlayerEquipment.MaxEquipmentNum)
                    continue;
                
                // 일반 등급이 아니면 패스
                if (value.rarity != EquipmentRarity.Normal)
                {
                    continue;
                }

            }

            // 사용 가능 리스트에 추가
            availableSet.Add(value);
            totalRarity += value.weight;
        }

        return (totalRarity, availableSet);
    }
    public (int, List<EquipmentData>) GetCraftableEquipment(Dictionary<string, EquipmentBase> equippedBaseDict)
    {
        var craftableEquipmentDataList = new List<EquipmentData>(5);
        var totalWeight = 0;
        
        foreach (var (resultEquipmentName, recipe) in recipeDict)
        {
            // todo 해당 조합 장비 해금 체크
            
            // 결과물을 이미 보유중일 경우에는 조합 불가 ( 중복템 방지 )
            if (equippedBaseDict.ContainsKey(resultEquipmentName))
            {
                continue;
            }

            var hasAllRequired = true;
            foreach (var requiredData in recipe.requiredEquipmentDataList)
            {
                // 재료 장비 미착용 시 패스
                if (!equippedBaseDict.TryGetValue(requiredData.equipmentName, out var equippedRequiredBase))
                {
                    hasAllRequired = false;
                    break;
                }
                
                // 조합 무기일 경우  
                if (recipe.resultEquipmentData.type is EquipmentType.Weapon)
                {
                    if (requiredData.type is EquipmentType.Weapon &&
                        equippedRequiredBase.CurrentLevel != requiredData.GetMaxLevel())
                    {
                        hasAllRequired = false;
                        break;
                    }
                }
                else if (recipe.resultEquipmentData.type is EquipmentType.Accessory)
                {
                    if (requiredData.type is EquipmentType.Accessory &&
                        equippedRequiredBase.CurrentLevel != requiredData.GetMaxLevel())
                    {
                        hasAllRequired = false;
                        break;
                    }
                }
            }

            if (hasAllRequired)
            {
                craftableEquipmentDataList.Add(recipe.resultEquipmentData);
                totalWeight += recipe.resultEquipmentData.weight;
            }
        }

        return (totalWeight, craftableEquipmentDataList);
    }

    [Serializable]
    public class CraftingRecipe
    {
        public EquipmentData[] requiredEquipmentDataList;
        public EquipmentData resultEquipmentData;
    }
}