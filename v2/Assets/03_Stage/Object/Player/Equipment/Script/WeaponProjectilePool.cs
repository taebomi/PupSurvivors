using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using PupSurvivors.Stage;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace PupSurvivors.Equipment
{
    public class WeaponProjectilePool<T> where T : WeaponProjectileBase<T>
    {
        private readonly WeaponBase _weapon;
        private readonly Stack<T> _pool;
        
        private T _projectilePrefab;

        private readonly Transform _container;

        private bool _isDisposed;
        private readonly Action<T> _actionOnPop;
        private readonly Action<T> _actionOnPush;


        public WeaponProjectilePool(WeaponBase weaponBase, Action<T> actionOnPop = null, Action<T> actionOnPush = null)
        {
            _weapon = weaponBase;
            _pool = new Stack<T>(weaponBase.WeaponData.projectileCapacity);

            _actionOnPop = actionOnPop;
            _actionOnPush = actionOnPush;
            
            _container = new GameObject($"Projectile Pool").transform;
            _container.SetParent(_weapon.Container);
        }

        public async UniTask Init()
        {
            var key = $"Equipment/Weapon/{_weapon.WeaponData.equipmentName}_Projectile";
            var handle = Addressables.LoadAssetAsync<GameObject>(key);
            _projectilePrefab = (await handle).GetComponent<T>();
            Addressables.Release(handle);
        }

        public T Pop()
        {
            if (!_pool.TryPop(out var projectile))
            {
                projectile = Object.Instantiate(_projectilePrefab, _container);
                projectile.Init(this, _weapon);
                projectile.OnStatsChanged();
                _weapon.StatsChangedEvent.AddListener(projectile.OnStatsChanged);
            }

            projectile.SetActive();
            _actionOnPop?.Invoke(projectile);
            return projectile;
        }

        public void Push(T projectile)
        {
            if (_isDisposed)
            {
                Object.Destroy(projectile.gameObject);
            }
            projectile.SetDeactive();
            _actionOnPush?.Invoke(projectile);
            _pool.Push(projectile);
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }
            
            _isDisposed = true;

            _projectilePrefab = null;
            foreach (var projectileBase in _pool)
            {
                Object.Destroy(projectileBase.gameObject);
            }
            _pool.Clear();
        }
    }
}