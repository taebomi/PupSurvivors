using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance != null)
            {
                return _instance;
            }

            _instance = FindObjectOfType<T>();
            if (_instance != null)
            {
                return _instance;
            }

            var container = new GameObject($"{nameof(T)}");
            _instance = container.AddComponent<T>();
            return _instance;
        }
    }

    protected void Awake()
    {
        if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this as T;
        AwakeAfter();
    }

    protected abstract void AwakeAfter();
}