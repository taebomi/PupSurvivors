using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using PupSurvivors.Enemy;
using PupSurvivors.ObjectPool;
using PupSurvivors.Stage.Item;
using UnityEngine;
using ItemName = ItemDB.ItemName;
using Random = UnityEngine.Random;

public partial class StageManager
{
    public Dictionary<int, ItemObjectBase> ItemColliderDict { get; private set; }

    [SerializeField] private ItemDB itemDB;
    private int _totalWeight;

    private LimitedObjectPool<RefinedExp> _refinedExpPool;
    private const int RefinedExpMaxNum = 50;
    private LimitedObjectPool<Nyan> _nyanPool;
    private const int NyanMaxNum = 50;

    private Transform _itemContainer;


    private LimitedObjectPool<DestructibleObject> _destructibleObjectPool;
    [SerializeField] private DestructibleObject destructibleObjectPrefab;
    private Transform _destructibleObjectsContainer;

    private const int MaxDestructibleNum = 25;
    private const float DestructibleObjectCreationDist = 3f;

    private void InitializeItem()
    {
        ItemColliderDict = new Dictionary<int, ItemObjectBase>(MaxExpObjectNumber);
        _itemContainer = new GameObject("Item Container").transform;
        _itemContainer.SetParent(ObjectPoolContainer);

        _refinedExpPool =
            new LimitedObjectPool<RefinedExp>(
                () => Instantiate(itemDB.itemDataDict[ItemName.RefinedExp].prefab) as RefinedExp, RefinedExpMaxNum);
        RefinedExp.SetManagedPool(_refinedExpPool);
        _nyanPool = new LimitedObjectPool<Nyan>(
            () => Instantiate(itemDB.itemDataDict[ItemName.Nyan].prefab) as Nyan, NyanMaxNum);
        Nyan.SetManagedPool(_nyanPool);

        _totalWeight = 0;
        foreach (var (itemName, itemData) in itemDB.itemDataDict)
        {
            if (itemName is ItemName.RefinedExp or ItemName.Nyan)
            {
                continue;
            }

            _totalWeight += itemData.weight;
        }
    }

    private void InitializeDestructibleObject()
    {
        _destructibleObjectsContainer = new GameObject("Destructible Object Container").transform;
        _destructibleObjectsContainer.SetParent(ObjectPoolContainer);

        _destructibleObjectPool = new LimitedObjectPool<DestructibleObject>(
            () => Instantiate(destructibleObjectPrefab, _destructibleObjectsContainer), MaxDestructibleNum);
        DestructibleObject.SetManagedPool(_destructibleObjectPool);
    }

    private async UniTask CheckPlayerMoveDistance(PlayerController player)
    {
        var tracker = player.GetComponentInChildren<DistanceTracker>();
        var desiredDist = DestructibleObjectCreationDist * Random.Range(0.5f, 1.5f);
        while (true)
        {
            if (tracker.MoveDistance > desiredDist)
            {
                // todo 플레이어 근처에 생성
                Vector3 pos;
                do
                {
                    pos = CameraManager.Instance.GetCameraOutsideRandomPosition();
                } while (TempPathFinder.Instance.CanSpawn(pos, true));

                var destructibleObject = _destructibleObjectPool.Get();
                destructibleObject.Set(pos, player.CurrentStats.luck);

                desiredDist += DestructibleObjectCreationDist * Random.Range(0.5f, 1.5f);
            }

            await UniTask.Delay(3000, cancellationToken: _destroyCts.Token);
        }
    }


    public ItemObjectBase GetRandomItem(int luck)
    {
        // itemDB로부터 랜덤한 아이템 뽑기
        // todo 언락된 아이템만 리스트에 모아둔 뒤 그걸로 뽑기로 변경하기

        var random = Random.value;
        if (random < 0.5f) // 냥
        {
            var nyan = _nyanPool.Get();
            // todo 스테이지에 따라 확률 스테이지 정보로부터 가져오기
            switch (Random.value)
            {
                case <= 0.75f:
                    nyan.SetNyan(Random.Range(1, 51));
                    break;
                case <= 0.95f:
                    nyan.SetNyan(Random.Range(51, 251));
                    break;
                default:
                    nyan.SetNyan(Random.Range(251, 751));
                    break;
            }
        }
        else if (random < 0.75f) // 경험치
        {
            var refinedExp = _refinedExpPool.Get();
            switch (Random.value)
            {
                case <= 0.1f:
                    refinedExp.SetExp(RefinedExp.Type.Lowest);
                    break;
                case <= 0.5f:
                    refinedExp.SetExp(RefinedExp.Type.Lower);
                    break;
                case <= 0.85f:
                    refinedExp.SetExp(RefinedExp.Type.Intermediate);
                    break;
                case <= 0.95f:
                    refinedExp.SetExp(RefinedExp.Type.Higher);
                    break;
                case <= 1f:
                    refinedExp.SetExp(RefinedExp.Type.Highest);
                    break;
            }
        }
        else // 아이템
        {
            var roll = Random.Range(0, _totalWeight + luck * (itemDB.itemDataDict.Count - 2));

            foreach (var (itemName, itemData) in itemDB.itemDataDict)
            {
                if (itemName is ItemName.RefinedExp or ItemName.Nyan)
                {
                    continue;
                }

                if (roll <= itemData.weight) // 당첨
                {
                    return Instantiate(itemData.prefab, _itemContainer);
                }
            }
        }

        return null;

        // 골드 / 경험치 확률 75% 나머지 아이템 확률 25%
        // 아이템이 3번 이상 안나왔을 경우 다음은 무조건 아이템 나오기

        // 1. 경험치, 골드일 경우
        // 1 - 1 - 1. 경험치는 등급에 따라 고정값 아닌 현재 필요 경험치 비율만큼 증가시켜줌.
        // 1 - 1 - 2. 골드는 고정값의 랜덤 - 25% ~ + 25%
    }
}