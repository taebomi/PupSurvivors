using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Cysharp.Threading.Tasks;
using PupSurvivors.Pool;
using PupSurvivors.Stage.Item;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PupSurvivors.Stage
{
    public partial class StageManager
    {
        [Header("Item"), SerializeField] private ItemDB itemDB;

        private List<ItemDB.ItemData> _availableItemList;
        private int _availableItemTotalWeight;

        private Transform _itemContainer;

        private LimitedObjectPool<ExpCrystal> _expCrystalPool;
        private ObjectPool<ExpGem> _expGemPool;
        private ObjectPool<Nyan> _nyanPool;

        private const int MaxExpCrystalNum = 1000;
        private const int MaxVisibleExpCrystalNum = 500;

        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        private void InitializeItem()
        {
            _availableItemList = new List<ItemDB.ItemData>();
            _availableItemTotalWeight = 0;
            foreach (var (_, itemData) in itemDB.db)
            {
                _availableItemList.Add(itemData); // todo 해금 정보에 따라 추가하기
                _availableItemTotalWeight += itemData.weight;
            }

            var itemContainer = new GameObject("Item").transform;
            var expCrystalContainer = new GameObject("Exp Crystal").transform;
            expCrystalContainer.SetParent(itemContainer);
            var expGemContainer = new GameObject("Exp Gem").transform;
            expGemContainer.SetParent(itemContainer);
            var nyanContainer = new GameObject("Nyan").transform;
            nyanContainer.SetParent(itemContainer);

            _expCrystalPool = new LimitedObjectPool<ExpCrystal>(
                () =>
                {
                    var expCrystal = Instantiate(itemDB.expCrystalPrefab, expCrystalContainer);
                    expCrystal.Initialize(_expCrystalPool);
                    return expCrystal;
                }, MaxExpCrystalNum);
            _expGemPool = new ObjectPool<ExpGem>(
                () =>
                {
                    var expGem = Instantiate(itemDB.expGemPrefab, expGemContainer);
                    expGem.Initialize(_expGemPool);
                    return expGem;
                }, 100
            );
            _nyanPool = new ObjectPool<Nyan>(
                () =>
                {
                    var nyan = Instantiate(itemDB.nyanPrefab, nyanContainer);
                    nyan.Initialize(_nyanPool);
                    return nyan;
                }, 100
            );
        }

        public void CreateExpCrystal(float expValue, Vector3 pos)
        {
            if (ExpCrystal.VisibleCount > MaxVisibleExpCrystalNum)
            {
                return;
            }

            var expCrystal = _expCrystalPool.Get();
            while (expCrystal.IsVisible)
            {
                expCrystal = _expCrystalPool.Get();
            }

            if (expCrystal.ExpValue > 0f)
            {
                var mergeExp = _expCrystalPool.Get();
                mergeExp.AddExp(expCrystal.ExpValue);
            }

            expCrystal.Set(pos);
            expCrystal.SetExp(expValue);
        }

        // 파괴 가능한 오브젝트 파괴 시 랜덤으로 아이템 1개 생성

        // 일반 몬스터 죽일 경우 경험치 1개 생성

        // 엘리트 죽일 경우 경험치랑 골드 생성

        // 미니보스 죽일 경우 랜덤 상자 드롭
        //      상자에서는 등급에 따라 보상량 결정
        //      빨리 죽일수록 골드가 많고 늦게 죽일수록 경험치가 많아짐

        // 보스를 죽일 경우 상자


        public void CreateRandomItem(Vector3 pos)
        {
            var random = Random.value;
            if (random < 0.5f) // 50% 확률로 냥
            {
                var nyan = _nyanPool.Get();
                nyan.Set(pos);
                var nyanValue = stageInfo.GetCurrentGoodsValue(CurTime).destructibleNyan * Random.Range(0.5f, 1.5f);
                nyan.SetNyan((int)nyanValue);
            }
            else if (random < 0.75f) // 25% 확률로 정제 경험치
            {
                var refinedExp = _expGemPool.Get();
                refinedExp.Set(pos);

                random = Random.value;
                var grade = random switch
                {
                    < 0.05f => ExpGem.Grade.Lowest, // 10 %
                    < 0.3f => ExpGem.Grade.Lower, // 25%
                    < 0.8f => ExpGem.Grade.Intermediate, // 50%
                    < 0.95f => ExpGem.Grade.Higher, // 10 %
                    < 1f => ExpGem.Grade.Highest, // 5 %
                    _ => throw new ArgumentOutOfRangeException()
                };

                refinedExp.SetGrade(grade);
            }
            else // 랜덤 아이템
            {
                var randomWeight = Random.Range(0, _availableItemTotalWeight);
                foreach (var itemData in _availableItemList)
                {
                    var weight = itemData.weight;
                    if (randomWeight < weight) // 당첨
                    {
                        var item = Instantiate(itemData.prefab, pos, Quaternion.identity, _itemContainer);
                        item.Set(pos);
                        break;
                    }
                }
            }
        }

        #region Nyan

        public int CurNyan { get; private set; }

        private void InitializeGoods()
        {
            CurNyan = 0;
        }

        public void AddNyan(int nyan)
        {
            CurNyan += nyan;
        }

        public void CreateNyan(int nyanValue, Vector3 pos)
        {
            var nyan = _nyanPool.Get();
            nyan.SetNyan(nyanValue);
            nyan.Set(pos);
        }
        public void CreateNyan(int nyanValue, Vector3 spawnPos, Vector3 destPos)
        {
            var nyan = _nyanPool.Get();
            nyan.SetNyan(nyanValue);
            nyan.Set(spawnPos, destPos).Forget();
        }

        #endregion

        #region ExpGem

        public void CreateExpGem(Vector3 pos, ExpGem.Grade grade)
        {
            var gem = _expGemPool.Get();
            gem.SetGrade(grade);
            gem.Set(pos);
        }

        public void CreateExpGem(ExpGem.Grade grade, Vector3 spawnPos, Vector3 destPos)
        {
            var gem = _expGemPool.Get();
            gem.SetGrade(grade);
            gem.Set(spawnPos, destPos).Forget();
        }

        #endregion
    }
}