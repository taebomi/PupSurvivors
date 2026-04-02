using System.Collections;
using System.Collections.Generic;
using PupSurvivors.ObjectPool;
using UnityEngine;

namespace PupSurvivors.Stage.Item
{
    public class Nyan : ItemObjectBase
    {
        private static LimitedObjectPool<Nyan> _managedPool;
        private int _nyan;

        [SerializeField] private SpriteContainer sprites;

        public static void SetManagedPool(LimitedObjectPool<Nyan> pool)
        {
            _managedPool = pool;
        }

        public void SetNyan(int nyan)
        {
            _nyan = nyan;
            // 골드 1 ~ 50, 주머니 51 ~ 250, 금괴 251 ~ 
            MainSr.sprite = nyan switch
            {
                <= 50 => sprites.data[0],
                <= 250 => sprites.data[1],
                _ => sprites.data[2]
            };
        }

        protected override void ApplyItem(PlayerController target)
        {
            Debug.Log($"{_nyan}냥 획득 !");
            _managedPool.Push(this);
        }
    }
}