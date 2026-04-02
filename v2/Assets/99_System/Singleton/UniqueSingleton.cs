using UnityEngine;

namespace PupSurvivors.System
{
    public abstract class UniqueSingleton<T> : MonoBehaviour where T : UniqueSingleton<T>
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

                if (_isQuit)
                {
                    return null;
                }

                _instance = FindObjectOfType<T>();
                if (_instance == null)
                {
                    _instance = new GameObject($"{nameof(T)}").GetComponent<T>();
                }

                return _instance;
            }
        }

        // ReSharper disable once StaticMemberInGenericType
        private static bool _isQuit;

        private bool IsUnique()
        {
            if (_instance == null)
            {
                _instance = this as T;
                return true;
            }
            else
            {
                Destroy(gameObject);
                return false;
            }
        }

        private void Awake()
        {
            if (!IsUnique())
            {
                return;
            }
            DontDestroyOnLoad(this);
            Initialize();
        }

        protected virtual void OnApplicationQuit()
        {
            _isQuit = true;
        }

        protected abstract void Initialize();
    }
}