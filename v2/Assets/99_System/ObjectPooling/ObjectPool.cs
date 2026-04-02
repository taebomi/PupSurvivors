using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PupSurvivors.Pool
{
    public class ObjectPool<T> : IObjectPool<T> where T : MonoBehaviour
    {
        public int UsedCount { get; private set; }

        private readonly Stack<T> _pool;
        private readonly Func<T> _createFunc;
        private readonly Action<T> _actionOnGet, _actionOnPush, _actionOnDestroy;

        private bool _isDestroyed;

        public ObjectPool(Func<T> createFunc, int capacity, Action<T> actionOnGet = null, Action<T> actionOnPush = null,
            Action<T> actionOnDestroy = null)
        {
            _createFunc = createFunc;
            _actionOnDestroy = actionOnDestroy;
            _actionOnGet = actionOnGet;
            _actionOnPush = actionOnPush;
            _pool = new Stack<T>(capacity);
            _isDestroyed = false;
        }

        public T Get()
        {
            if (_isDestroyed)
            {
                Debug.LogError("파괴된 오브젝트 풀 사용 시도");
                return null;
            }

            if (!_pool.TryPop(out var pooled))
            {
                pooled = _createFunc();
            }

            UsedCount++;
            _actionOnGet?.Invoke(pooled);
            return pooled;
        }

        public void Push(T willPooled)
        {
            UsedCount--;

            if (_isDestroyed)
            {
                Object.Destroy(willPooled.gameObject);
                if (UsedCount == 0)
                {
                    _actionOnDestroy?.Invoke(willPooled);
                }

                return;
            }
            _actionOnPush?.Invoke(willPooled);
            _pool.Push(willPooled);
        }

        public void Destroy()
        {
            _isDestroyed = true;
            while (_pool.TryPop(out var obj))
            {
                Object.Destroy(obj);
            }
        }
    }
}