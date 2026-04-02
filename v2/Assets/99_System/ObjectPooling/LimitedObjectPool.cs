using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PupSurvivors.Pool
{
    public class LimitedObjectPool<T> : IObjectPool<T> where T : MonoBehaviour
    {
        private readonly Stack<T> _pool;
        private readonly LinkedList<T> _usingList;

        private readonly Func<T> _createFunc;
        private readonly int _maxNum;

        public LimitedObjectPool(Func<T> createFunc, int maxNum)
        {
            _createFunc = createFunc;
            _maxNum = maxNum;

            _pool = new Stack<T>(maxNum);
            _usingList = new LinkedList<T>();
        }

        public void Push(T willPooled)
        {
            _usingList.Remove(willPooled);
            _pool.Push(willPooled);
        }

        public T Get()
        {
            if (_pool.TryPop(out var pooled))
            {
                _usingList.AddLast(pooled);
                return pooled;
            }
            
            if (_usingList.Count >= _maxNum)
            {
                pooled = _usingList.First.Value;
                _usingList.RemoveFirst();
                _usingList.AddLast(pooled);
                return pooled;
            }

            pooled = _createFunc();
            _usingList.AddLast(pooled);
            return pooled;
        }

        public void Destroy()
        {
            while (_pool.TryPop(out var obj))
            {
                GameObject.Destroy(obj);
            }

            if (_usingList.Count != 0)
            {
                Debug.LogWarning($"{nameof(T)} 풀에서 사용중인 오브젝트가 {_usingList.Count} 개 존재함.");
            }
        }
    }
}