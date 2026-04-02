using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PupSurvivors.ObjectPool
{
    public interface IObjectPool<T> where T : MonoBehaviour
    {
        T Get();

        void Push(T willPooled);

        void Clear();
    }
}