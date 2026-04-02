
using UnityEngine;
using UnityEngine.Pool;

namespace PupSurvivors.ObjectPool
{
    public class LimitedPoolableObject<T> : MonoBehaviour where T : LimitedPoolableObject<T>
    {
        protected LimitedObjectPool<T> ManagedPool;

        public void SetManagedPool(LimitedObjectPool<T> pool)
        {
            ManagedPool = pool;
        }
    }
}