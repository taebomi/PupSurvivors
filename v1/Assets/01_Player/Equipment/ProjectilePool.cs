using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

namespace PupSurvivors.Equipment
{
    public class ProjectilePool<T> where T : WeaponProjectileBase<T>
    {
        private readonly WeaponBase _weapon;

        private readonly Stack<T> _pool;
        private GameObject _addressableAsset; // 풀링될 어드레서블 에셋
        private T _projectileComponent; // 풀링될 어드레서블 에셋의 스크립트 컴포넌트

        private bool _canUse; // 풀 파괴 후 동작 방지용도 (pop 시에도 체크해야하나?)

        private readonly Action<T> _actionOnCreate;

        
        // 풀 생성 시 원본 무기로부터 세팅, Initialize 함수에서 반드시 초기화 해줄 것 
        public ProjectilePool(WeaponBase weaponBase, Action<T> actionOnCreate = null)
        {
            _weapon = weaponBase;
            _pool = new Stack<T>(weaponBase.WeaponData.projectileCapacity);
            WeaponProjectileBase<T>.Initialize(this, weaponBase);
            _actionOnCreate = actionOnCreate;
            _canUse = true;
        }

        ~ProjectilePool()
        {
            Dispose();
        }

        // 사용 전 반드시 초기화
        public async UniTask Initialize()
        {
            var key = $"{_weapon.WeaponData.equipmentName}_Projectile";
            _addressableAsset = await Addressables.LoadAssetAsync<GameObject>(key);
            _projectileComponent = _addressableAsset.GetComponent<T>();
        }

        public T Pop()
        {
            if (!_pool.TryPop(out var projectile))
            {
                projectile = Object.Instantiate(_projectileComponent,StageManager.Instance.ObjectPoolContainer);
                _actionOnCreate?.Invoke(projectile);
                projectile.OnStatsChanged();
                _weapon.StatChangedEvent.AddListener(projectile.OnStatsChanged);
            }
            else
            {
                projectile.gameObject.SetActive(true);
            }

            return projectile;
        }

        public void Push(T projectile)
        {
            if (!_canUse)
            {
                Destroy(projectile);
                return;
            }

            projectile.gameObject.SetActive(false);
            _pool.Push(projectile);
        }

        private void Destroy(T projectile)
        {
            _weapon.StatChangedEvent.RemoveListener(projectile.OnStatsChanged);
            Object.Destroy(projectile);
        }

        public void Dispose()
        {
            if (!_canUse)
            {
                return;
            }

            _canUse = false;

            foreach (var projectile in _pool)
            {
                Destroy(projectile);
            }

            Addressables.Release(_addressableAsset);
            _pool.Clear();
        }
    }
}