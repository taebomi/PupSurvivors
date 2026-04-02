using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PupSurvivors.Pool
{
    public interface IObjectPool<T> where T : MonoBehaviour
    {
        void Push(T willPooled);
        T Get();

        void Destroy();
    }
}