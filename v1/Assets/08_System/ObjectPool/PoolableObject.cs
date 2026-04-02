using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class PoolableObject<T> : MonoBehaviour where T :  PoolableObject<T>
{
    protected IObjectPool<T> ManagedPool;

    public void SetManagedPool(IObjectPool<T> pool)
    {
        ManagedPool = pool;
    }
}
