using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PupSurvivors.ObjectPool
{
    public class LimitedObjectPool<T> : IDisposable, IObjectPool<T> where T : MonoBehaviour
    {
        private readonly Stack<T> _pool;
        private readonly LinkedList<T> _usingObjects; // 사용중인 풀링된 오브젝트를 담아둠.

        private readonly Func<T> _createFunc;

        private readonly int _maxNumber;
        private bool _canUse;

        public LimitedObjectPool(Func<T> createFunc, int maxNumber)
        {
            _canUse = true;
            _createFunc = createFunc;
            _maxNumber = maxNumber;
            _pool = new Stack<T>(maxNumber);
            _usingObjects = new LinkedList<T>();
        }

        public T Get()
        {
            if (_pool.TryPop(out var poolableObject))
            {
                _usingObjects.AddLast(poolableObject);
                return poolableObject;
            }

            // 사용가능한 것이 없을 때
            if (_usingObjects.Count >= _maxNumber) // 제한된 개수 모두를 사용중인 경우 가장 오래된 것을 재사용.
            {
                poolableObject = _usingObjects.First.Value;
                _usingObjects.RemoveFirst();
                _usingObjects.AddLast(poolableObject);
                return poolableObject;
            }

            // 제한된 개수에 도달하지 않았을 경우 새로 생성
            poolableObject = _createFunc();
            _usingObjects.AddLast(poolableObject);
            return poolableObject;
        }
        
        public void Push(T willPooledObject)
        {
            if (!_canUse)
            {
                Object.Destroy(willPooledObject);
                return;
            }

            _usingObjects.Remove(willPooledObject);
            _pool.Push(willPooledObject);
        }

        public void Clear()
        {
            if (!_canUse) return;
            _canUse = false;

            foreach (var poolableObject in _pool)
            {
                Object.Destroy(poolableObject.gameObject);
            }

            _pool.Clear();
            _usingObjects.Clear();
        }


        public void Dispose() => Clear();
    }
}