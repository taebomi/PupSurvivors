using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PupSurvivors.Equipment;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class PlayerEquipment : MonoBehaviour
{
    public Dictionary<string, EquipmentBase> EquippedDict { get; private set; }

    private HashSet<EquipmentData> _equippedSet;

    [field: SerializeField]
    public UnityEvent<EquipmentType, Dictionary<string, EquipmentBase>> EquipmentChangedEvent { get; private set; }

    public const int MaxEquipmentNum = 6;

    
    
    private EquipmentDB _equipmentDB;
    private int _playerLuck;
    private int _notSelectedEquippedCount;

    private void Awake()
    {
        _equipmentDB = StageManager.Instance.EquipmentDB;
        
        _equippedSet = new HashSet<EquipmentData>(12);
        EquippedDict = new Dictionary<string, EquipmentBase>(12);
        _playerLuck = 0;
        _notSelectedEquippedCount = 0;
    }

    public void OnStatsUpdated(CharacterStats stats)
    {
        _playerLuck = stats.luck;
    }

    public void AddEquipment(string equipmentName)
    {
        if (!_equipmentDB.equipmentDict.TryGetValue(equipmentName, out var equipmentData))
        {
            Debug.LogError($"{equipmentName} 장비가 존재하지 않음");
            return;
        }

        // # 이미 보유중일 시
        if (EquippedDict.TryGetValue(equipmentName, out var equipmentBase))
        {
            equipmentBase.LevelUp();
            EquipmentChangedEvent.Invoke(equipmentData.type, EquippedDict);
            return;
        }

        // 첫 획득일 시, 상위 등급일 시 타입에 따라 재료 장비 제거
        var equipmentType = equipmentData.type;
        switch (equipmentType)
        {
            case EquipmentType.Weapon: // 하위 무기 제거
                if (_equipmentDB.recipeDict.TryGetValue(equipmentName, out var recipe))
                {
                    foreach (var requiredEquipmentData in recipe.requiredEquipmentDataList)
                    {
                        if (requiredEquipmentData.type is EquipmentType.Weapon)
                        {
                            RemoveEquipment(requiredEquipmentData.equipmentName);
                        }
                    }
                }

                break;
            case EquipmentType.Accessory: // 하위 악세사리 제거
                if (_equipmentDB.recipeDict.TryGetValue(equipmentName, out recipe))
                {
                    foreach (var requiredEquipmentData in recipe.requiredEquipmentDataList)
                    {
                        if (requiredEquipmentData.type is EquipmentType.Accessory)
                        {
                            RemoveEquipment(requiredEquipmentData.equipmentName);
                        }
                    }
                }

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        // 장비 추가
        equipmentBase =
            gameObject.AddComponent(Type.GetType($"PupSurvivors.Equipment.{equipmentName}")) as EquipmentBase;
        EquippedDict.Add(equipmentName, equipmentBase);
        _equippedSet.Add(equipmentData);
        EquipmentChangedEvent.Invoke(equipmentData.type, EquippedDict);
    }

    public void RemoveEquipment(string equipmentName)
    {
        if (!EquippedDict.Remove(equipmentName, out var equipmentBase))
        {
            Debug.LogError($"{equipmentName} 장비가 존재하지 않음");
            return;
        }

        var equipmentData = equipmentBase.GetEquipmentData();
        _equippedSet.Remove(equipmentData);
        Destroy(equipmentBase);
        EquipmentChangedEvent.Invoke(equipmentData.type, EquippedDict);
    }

    public void GetLevelUpEquipmentDataList(List<EquipmentData> availableEquipmentDataList)
    {
        // 초기화
        availableEquipmentDataList.Clear();

        // 업그레이드 가능한 장비들을 체크
        var (totalWeight, tempEquipmentDataList) = _equipmentDB.GetCraftableEquipment(EquippedDict);
        totalWeight += _playerLuck * tempEquipmentDataList.Count;
        var selectableCount = Mathf.Clamp(tempEquipmentDataList.Count, 0, 3);
        for (var i = 0; i < selectableCount; i++)
        {
            var roll = Random.Range(0, totalWeight);
            foreach (var equipmentData in tempEquipmentDataList)
            {
                var weight = equipmentData.weight + _playerLuck;
                if (roll < weight)
                {
                    tempEquipmentDataList.Remove(equipmentData);
                    availableEquipmentDataList.Add(equipmentData);
                    totalWeight -= weight;
                    break;
                }

                roll -= weight;
            }
        }


        if (selectableCount == 3)
        {
            return;
        }

        if (selectableCount != 0)
        {
            _notSelectedEquippedCount = 0;
        }

        // # 남은 개수만큼 획득가능한 무기 선택

        HashSet<EquipmentData> availableSet;
        (totalWeight, availableSet) = _equipmentDB.GetAvailableEquipmentList(EquippedDict);
        totalWeight += _playerLuck * availableSet.Count;

        // 업그레이드 가능한 착용/미착용 장비로 분리
        var notEquippedAvailableSet = new HashSet<EquipmentData>(availableSet);
        notEquippedAvailableSet.ExceptWith(_equippedSet);
        var equippedAvailableSet = new HashSet<EquipmentData>(_equippedSet);
        equippedAvailableSet.IntersectWith(availableSet);

        // 각 weight 합 계산
        var totalEquippedAvailableWeight = equippedAvailableSet.Sum(equipmentData => equipmentData.weight)
                                           + _playerLuck * equippedAvailableSet.Count;
        var totalNotEquippedWeight = totalWeight - totalEquippedAvailableWeight;

        for (var i = 3 - selectableCount; i > 0; i--)
        {
            if (equippedAvailableSet.Count > 0 && // 착용중인 장비 중 업그레이드 가능한 것 중에서
                (_notSelectedEquippedCount > 5 || Random.value < 0.2f)) // 착용중인 것 못뽑은 횟수 많거나 확률에 따라 선택
            {
                totalWeight = totalEquippedAvailableWeight;
                availableSet = equippedAvailableSet;
                _notSelectedEquippedCount = 0;
            }
            else // 미보유 장비 중에서 뽑기
            {
                totalWeight = totalNotEquippedWeight;
                availableSet = notEquippedAvailableSet;
                _notSelectedEquippedCount++;
            }

            var roll = Random.Range(0, totalWeight);
            foreach (var equipmentData in availableSet)
            {
                  var weight = equipmentData.weight + _playerLuck;
                if (roll <= weight) // 당첨
                {
                    availableSet.Remove(equipmentData);
                    availableEquipmentDataList.Add(equipmentData);

                    if (availableSet == _equippedSet)
                    {
                        totalEquippedAvailableWeight -= weight;
                    }
                    else
                    {
                        totalNotEquippedWeight -= weight;
                    }

                    break;
                }

                roll -= weight;
            }
        }
    }
}