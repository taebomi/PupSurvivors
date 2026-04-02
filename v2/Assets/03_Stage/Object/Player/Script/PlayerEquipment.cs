using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PupSurvivors.Equipment;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace PupSurvivors.Stage
{
    public class PlayerEquipment : MonoBehaviour
    {
        [SerializeField] private Player player;
        [SerializeField] private EquipmentDB equipmentDB;
        public Transform EquipmentContainer { get; private set; } // 생성된 장비나 투사체 보관

        public Dictionary<string, EquipmentBase> EquippedDict { get; private set; }
        public Dictionary<string, WeaponBase> EquippedWeaponDict { get; private set; }
        public Dictionary<string, AccessoryBase> EquippedAccessoryDict { get; private set; }

        public int PlayerLuck => player.CurStats.luck;
        
        public int CurWeaponNum => EquippedWeaponDict.Count;
        public int CurAccessoryNum => EquippedAccessoryDict.Count;
        public int CurMaxWeaponNum { get; private set; }
        public int CurMaxAccessoryNum { get; private set; }

        public bool IsInitialized { get; private set; }
        public bool IsAvailable => CraftableTable.DataSet.Count != 0 || AddableTable.DataSet.Count != 0;

        public EquipmentTable CraftableTable { get; private set; }
        public EquipmentTable AddableTable { get; private set; }

        public class EquipmentTable
        {
            public readonly HashSet<EquipmentData> DataSet;
            public int TotalWeight;

            public EquipmentTable()
            {
                DataSet = new HashSet<EquipmentData>();
                TotalWeight = 0;
            }

            public EquipmentTable(IEnumerable<EquipmentData> dataEnumerable)
            {
                DataSet = new HashSet<EquipmentData>(dataEnumerable);
            }

            public EquipmentTable(EquipmentTable oriTable)
            {
                DataSet = new HashSet<EquipmentData>(oriTable.DataSet);
                TotalWeight = oriTable.TotalWeight;
            }

            public void Remove(EquipmentData equipmentData)
            {
                if (!DataSet.Remove(equipmentData))
                {
                    throw new Exception($"{equipmentData}가 존재하지 않음.");
                }

                TotalWeight -= equipmentData.weight;
            }

            public EquipmentData PickRandomItem(int playerLuck)
            {
                var roll = Random.Range(0, CalibratedWeight(playerLuck));
                foreach (var equipmentData in DataSet)
                {
                    var curCalibratedWeight = equipmentData.weight + playerLuck;
                    if (roll <= curCalibratedWeight)
                    {
                        return equipmentData;
                    }

                    roll -= curCalibratedWeight;
                }

                return null;
            }

            public int CalibratedWeight(int playerLuck) => TotalWeight + DataSet.Count * playerLuck;
        }

        private EquipmentChangedFlag _equipmentChangedFlag;

        [Flags]
        private enum EquipmentChangedFlag
        {
            None = 0,
            Weapon = 1 << 0,
            Accessory = 1 << 1,
        }

        [field: SerializeField]
        public UnityEvent<IEnumerable<WeaponBase>> EquippedWeaponChangedEvent { get; private set; }

        [field: SerializeField]
        public UnityEvent<IEnumerable<AccessoryBase>> EquippedAccessoryChangedEvent { get; private set; }

        private int _notHaveSelectedCount;
        private const int MaxNotHaveSelectedCount = 5;

        public const int MaxWeaponNum = 6;
        public const int MaxAccessoryNum = 6;
        public const int MaxEquipmentNum = MaxWeaponNum + MaxAccessoryNum;

        public void Initialize()
        {
            var playerTr = player.transform;
            EquipmentContainer = new GameObject($"{playerTr.name} Equipment").transform;
            EquipmentContainer.SetParent(playerTr.parent);

            EquippedDict = new Dictionary<string, EquipmentBase>(MaxEquipmentNum);
            EquippedWeaponDict = new Dictionary<string, WeaponBase>(MaxWeaponNum);
            EquippedAccessoryDict = new Dictionary<string, AccessoryBase>(MaxAccessoryNum);

            _notHaveSelectedCount = 0;

            CurMaxWeaponNum = GameManager.Instance.SaveData.maxWeaponNum;
            CurMaxAccessoryNum = GameManager.Instance.SaveData.maxAccessoryNum;

            AddableTable = new EquipmentTable();
            CraftableTable = new EquipmentTable();

            AddEquipment(player.CurCharacterData.defaultEquipmentName);
            RaiseEquipmentChangedEvent();
            IsInitialized = true;
        }

        public void LevelUpReward(string equipmentName)
        {
            if (IsEquipped(equipmentName, out var equipment))
            {
                LevelUpEquipment(equipment);
                RaiseEquipmentChangedEvent();
                return;
            }

            if (IsCraftable(equipmentName, out var recipe))
            {
                CraftEquipment(recipe);
                RaiseEquipmentChangedEvent();
                return;
            }

            AddEquipment(equipmentName);
            RaiseEquipmentChangedEvent();
        }

        private void AddEquipment(string equipmentName)
        {
            var equipment =
                gameObject.AddComponent(Type.GetType($"PupSurvivors.Equipment.{equipmentName}")) as EquipmentBase;
            if (!equipment)
            {
                throw new Exception($"{player.transform.name} - PupSurvivors.Equipment.{equipmentName}가 존재하지 않음.");
            }

            equipment.Initialize(player);
            EquippedDict.Add(equipmentName, equipment);

            switch (equipment.EquipmentData.type)
            {
                case EquipmentType.Weapon:
                    EquippedWeaponDict.Add(equipmentName, equipment as WeaponBase);
                    _equipmentChangedFlag |= EquipmentChangedFlag.Weapon;
                    break;
                case EquipmentType.Accessory:
                    EquippedAccessoryDict.Add(equipmentName, equipment as AccessoryBase);
                    _equipmentChangedFlag |= EquipmentChangedFlag.Accessory;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// 착용중인 장비 제거
        /// </summary>
        /// <param name="equipmentName"></param>
        /// <returns>제거 성공 시 true 반환</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private bool RemoveEquipment(string equipmentName)
        {
            if (!equipmentDB.equipmentDict.ContainsKey(equipmentName))
            {
                throw new Exception($"{player.transform.name} - {equipmentName}이 DB에 존재하지 않음.");
            }

            if (!EquippedDict.Remove(equipmentName, out var equipmentBase))
            {
                return false;
            }

            Destroy(equipmentBase);
            switch (equipmentBase.EquipmentData.type)
            {
                case EquipmentType.Weapon:
                    EquippedWeaponDict.Remove(equipmentName);
                    _equipmentChangedFlag |= EquipmentChangedFlag.Weapon;
                    break;
                case EquipmentType.Accessory:
                    EquippedAccessoryDict.Remove(equipmentName);
                    _equipmentChangedFlag |= EquipmentChangedFlag.Accessory;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return true;
        }

        private void LevelUpEquipment(EquipmentBase equipment)
        {
            equipment.LevelUp();
            SetEquipmentChangedFlag(equipment.EquipmentData.type);
        }

        private void CraftEquipment(EquipmentDB.Recipe recipe)
        {
            foreach (var ingredient in recipe.ingredients)
            {
                if (ingredient.willRemoved)
                {
                    RemoveEquipment(ingredient.equipmentData.equipmentName);
                }
            }

            AddEquipment(recipe.result.equipmentName);
        }

        private void SetEquipmentChangedFlag(EquipmentType type)
        {
            _equipmentChangedFlag |= type switch
            {
                EquipmentType.Weapon => EquipmentChangedFlag.Weapon,
                EquipmentType.Accessory => EquipmentChangedFlag.Accessory,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private void RaiseEquipmentChangedEvent()
        {
            if (_equipmentChangedFlag.HasFlag(EquipmentChangedFlag.Weapon))
            {
                EquippedWeaponChangedEvent.Invoke(EquippedWeaponDict.Values);
            }

            if (_equipmentChangedFlag.HasFlag(EquipmentChangedFlag.Accessory))
            {
                EquippedAccessoryChangedEvent.Invoke(EquippedAccessoryDict.Values);
            }

            UpdateCraftableTable();
            UpdateAddableTable();

            _equipmentChangedFlag = EquipmentChangedFlag.None;
        }

        private void UpdateCraftableTable()
        {
            CraftableTable.DataSet.Clear();
            CraftableTable.TotalWeight = 0;

            foreach (var (resultEquipmentName, recipe) in equipmentDB.recipeDict)
            {
                // todo 결과물 장비 해금되어있는지 체크, 안된 경우 continue

                // 결과물 보유중이면 패스
                if (EquippedDict.ContainsKey(resultEquipmentName))
                {
                    continue;
                }

                var satisfyCondition = true;
                foreach (var ingredient in recipe.ingredients)
                {
                    // 보유중이지 않으면 패스
                    if (!IsEquipped(ingredient.equipmentData.equipmentName, out var equipment))
                    {
                        satisfyCondition = false;
                        break;
                    }

                    // 조건 만족 못하면 패스
                    if (ingredient.needMaxLevel && equipment.IsMaxLevel())
                    {
                        satisfyCondition = false;
                        break;
                    }
                }

                if (!satisfyCondition)
                {
                    continue;
                }

                CraftableTable.DataSet.Add(recipe.result);
                CraftableTable.TotalWeight += recipe.result.weight;
            }
        }

        private void UpdateAddableTable()
        {
            AddableTable.DataSet.Clear();
            AddableTable.TotalWeight = 0;

            foreach (var (equipmentName, equipmentData) in equipmentDB.equipmentDict)
            {
                // todo 장비 해금 여부 체크, 안된 경우 continue

                if (IsEquipped(equipmentName, out var equipment))
                {
                    // 보유중일 때 만렙이면 
                    if (equipment.IsMaxLevel())
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
                            if (CurWeaponNum >= CurMaxWeaponNum)
                            {
                                continue;
                            }

                            break;
                        case EquipmentType.Accessory:
                            if (CurAccessoryNum >= CurMaxAccessoryNum)
                            {
                                continue;
                            }

                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                AddableTable.DataSet.Add(equipmentData);
                AddableTable.TotalWeight += equipmentData.weight;
            }
        }

        public bool IsEquipped(string equipmentName) => EquippedDict.ContainsKey(equipmentName);

        public bool IsEquipped(string equipmentName, out EquipmentBase equipment) =>
            EquippedDict.TryGetValue(equipmentName, out equipment);

        private bool IsCraftable(string equipmentName, out EquipmentDB.Recipe recipe)
        {
            if (!equipmentDB.recipeDict.TryGetValue(equipmentName, out recipe))
            {
                return false;
            }

            // 재료 아이템 확인
            var satisfyCondition = true;
            foreach (var ingredient in recipe.ingredients)
            {
                if (ingredient.willRemoved && !IsEquipped(ingredient.equipmentData.equipmentName))
                {
                    satisfyCondition = false;
                    break;
                }
            }

            return satisfyCondition;
        }


        /*public List<EquipmentData> GetLevelUpRewardList()
        {
            var rewardList = new List<EquipmentData>();
            var playerLuck = player.CurStats.luck;

            var rewardCount = Mathf.Clamp(CraftableTable.DataSet.Count, 0, rewardList.Length) ;
            if (rewardCount != 0)
            {
                _notHaveSelectedCount = 0;
            }

            var craftableTable = new EquipmentTable(CraftableTable);

            for (var i = 0; i < rewardCount; i++)
            {
                var randomEquipmentData = craftableTable.PickRandomItem(playerLuck);
                rewardList[i] = randomEquipmentData;
                craftableTable.Remove(randomEquipmentData);
            }

            if (rewardCount == rewardList.Length)
            {
                return rewardCount;
            }

            var equippedSet =
                new HashSet<EquipmentData>(EquippedDict.Values.Select(equipment => equipment.EquipmentData));

            var equippedAddableTable = new EquipmentTable(AddableTable.DataSet);
            equippedAddableTable.DataSet.IntersectWith(equippedSet);
            equippedAddableTable.TotalWeight = equippedAddableTable.DataSet.Sum(equipmentData => equipmentData.weight);
            var unequippedAddableTable = new EquipmentTable(AddableTable.DataSet);
            unequippedAddableTable.DataSet.ExceptWith(equippedSet);
            unequippedAddableTable.TotalWeight = AddableTable.TotalWeight - equippedAddableTable.TotalWeight;

            for (var i = 2 - rewardCount; i >= 0; i--)
            {
                const float equippedSelectProbability = 0.2f;
                if (equippedAddableTable.DataSet.Count > 0 &&
                    (_notHaveSelectedCount > MaxNotHaveSelectedCount || Random.value < equippedSelectProbability))
                {
                    _notHaveSelectedCount = 0;
                    var randomEquipmentData = equippedAddableTable.PickRandomItem(player.CurStats.luck);
                    if (randomEquipmentData)
                    {
                        rewardList[i] = randomEquipmentData;
                        equippedAddableTable.Remove(randomEquipmentData);
                    }
                }
                else
                {
                    _notHaveSelectedCount++;
                    var randomEquipmentData = unequippedAddableTable.PickRandomItem(player.CurStats.luck);
                    if (randomEquipmentData)
                    {
                        rewardList[i] = randomEquipmentData;
                        unequippedAddableTable.Remove(randomEquipmentData);
                    }
                }
            }
        }

        public List<EquipmentData> Trash()
        {
            var availableList = new List<EquipmentData>();

            // 업그레이드 가능한 장비 우선
            var (totalWeight, tempList) = equipmentDB.GetCraftableEquipment(EquippedDict);
            var playerLuck = player.CurStats.luck;
            totalWeight += playerLuck * tempList.Count;

            var count = Mathf.Clamp(tempList.Count, 0, 3);
            for (var i = 0; i < count; i++)
            {
                var roll = Random.Range(0, totalWeight);
                foreach (var equipmentData in tempList)
                {
                    var weight = equipmentData.weight + playerLuck;
                    if (roll < weight)
                    {
                        tempList.Remove(equipmentData);
                        availableList.Add(equipmentData);
                        totalWeight -= weight;
                        break;
                    }

                    roll -= weight;
                }
            }

            if (count == 3)
            {
                return availableList;
            }

            if (count != 0)
            {
                _notHaveSelectedCount = 0;
            }

            HashSet<EquipmentData> availableSet;
            (totalWeight, availableSet) = equipmentDB.GetAvailableEquipmentList(EquippedDict);
            totalWeight += playerLuck * availableSet.Count;

            var equippedSet = new HashSet<EquipmentData>(
                EquippedDict.Values.Select(equipmentBase => equipmentBase.EquipmentData));
            var unequippedAvailableSet = new HashSet<EquipmentData>(availableSet);
            unequippedAvailableSet.ExceptWith(equippedSet);
            var equippedAvailableSet = new HashSet<EquipmentData>(equippedSet);
            equippedAvailableSet.IntersectWith(availableSet);

            var totalEquippedAvailableWeight = equippedAvailableSet.Sum(data => data.weight) +
                                               playerLuck * equippedAvailableSet.Count;
            var totalUnequippedAvailableWeight = totalWeight - totalEquippedAvailableWeight;

            for (var i = 3 - count; i > 0; i--)
            {
                if (equippedAvailableSet.Count > 0 &&
                    (_notHaveSelectedCount > MaxNotHaveSelectedCount || Random.value < 0.2f))
                {
                    totalWeight = totalEquippedAvailableWeight;
                    availableSet = equippedAvailableSet;
                    _notHaveSelectedCount = 0;
                }
                else
                {
                    totalWeight = totalUnequippedAvailableWeight;
                    availableSet = unequippedAvailableSet;
                    _notHaveSelectedCount++;
                }

                var roll = Random.Range(0, totalWeight);
                foreach (var data in availableSet)
                {
                    var weight = data.weight + playerLuck;
                    if (roll <= weight)
                    {
                        availableSet.Remove(data);
                        availableList.Add(data);

                        if (availableSet == equippedAvailableSet)
                        {
                            totalEquippedAvailableWeight -= weight;
                        }
                        else if (availableSet == unequippedAvailableSet)
                        {
                            totalUnequippedAvailableWeight -= weight;
                        }

                        break;
                    }

                    roll -= weight;
                }
            }

            return availableList;
        }*/
    }
}