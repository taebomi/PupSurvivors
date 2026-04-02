using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using PupSurvivors.Stage.PathFinding;
using PupSurvivors.Stage.UI;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PupSurvivors.Stage
{
    public class EnemyFinder
    {
        public DamagableHealthSystemBase NearestDamagable { get; private set; }
        public float NearestDamagableDist { get; private set; }

        private readonly HashSet<DamagableHealthSystemBase> _visibleDamagableSet;
        private readonly Dictionary<Collider2D, DamagableHealthSystemBase> _damagableDict;

        private Collider2D _nearEnemyCollider;

        private Transform _playerTr;

        public EnemyFinder(Player player)
        {
            var stageManager = StageManager.Instance;
            _damagableDict = stageManager.DamagableSo.CollDict;
            _visibleDamagableSet = stageManager.DamagableSo.HashSet;

            _playerTr = player.transform;


            FindNearestEnemy().Forget();
        }


        private async UniTaskVoid FindNearestEnemy()
        {
            while (true)
            {
                _nearEnemyCollider = null;

                for (var dist = 1; dist < 10; dist++)
                {
                    _nearEnemyCollider =
                        Physics2D.OverlapCircle(_playerTr.position, dist, TaeBoMiCache.DamagableLayerMask);
                    if (_nearEnemyCollider != null)
                    {
                        break;
                    }
                }

                NearestDamagable = _nearEnemyCollider == null
                    ? GetRandomVisibleDamagable()
                    : _damagableDict[_nearEnemyCollider];
                NearestDamagableDist = NearestDamagable
                    ? Vector3.Distance(NearestDamagable.transform.position, _playerTr.position)
                    : float.MaxValue;

                await UniTask.Yield();
            }
        }


        // 2. 화면에 보이는 적 랜덤 추출

        public bool IsDamagableVisible()
        {
            return _visibleDamagableSet.Count != 0;
        }

        public DamagableHealthSystemBase GetRandomVisibleDamagable()
        {
            if (_visibleDamagableSet.Count == 0)
            {
                return null;
            }

            DamagableHealthSystemBase result = null;
            var count = 0;

            foreach (var damagable in _visibleDamagableSet)
            {
                count++;
                if (Random.Range(0, count) == 0)
                {
                    result = damagable;
                }
            }

            return result;
        }

        public int GetRandomVisibleDamagableArr(DamagableHealthSystemBase[] results)
        {
            var size = results.Length;
            var currentIdx = 0;

            foreach (var damagable in _visibleDamagableSet)
            {
                if (currentIdx < size)
                {
                    results[currentIdx] = damagable;
                }
                else
                {
                    var rand = Random.Range(0, currentIdx + 1);
                    if (rand < size)
                    {
                        results[rand] = damagable;
                    }
                }

                currentIdx++;
            }

            return currentIdx;
        }
    }
}