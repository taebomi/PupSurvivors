using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using PupSurvivors.Enemy;
using PupSurvivors.Pool;
using PupSurvivors.Stage.PathFinding;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PupSurvivors.Stage
{
    public class DestructibleCreator : MonoBehaviour
    {
        [SerializeField] private DestructibleDB destructibleDB;
        [SerializeField] private Destructible destructiblePrefab;
        [SerializeField] private Effect destructibleDestroyEffect;

        private MovementTracker _moveDirTracker;
        private List<MoveDistTracker> _moveDistTrackerList;

        private LimitedObjectPool<Destructible> _destructiblePool;
        private IObjectPool<Effect> _destroyEffectPool;
        private CancellationTokenSource _creatingCts;

        private const int MaxDestructibleNum = 25;
        private const float DefaultCreatingDist = 1f;

        private float _creatingDist;

        private void Awake()
        {
            var destructibleContainer = new GameObject("Destructible").transform;
            destructibleContainer.SetParent(transform);
            var destroyEffectContainer = new GameObject("Destroy Effect").transform;
            destroyEffectContainer.SetParent(transform);
            _destroyEffectPool = new ObjectPool<Effect>(() =>
                {
                    var pooled = Instantiate(destructibleDestroyEffect, destroyEffectContainer);
                    pooled.SetPool(_destroyEffectPool);
                    return pooled;
                }, 3);
            _destructiblePool = new LimitedObjectPool<Destructible>(
                () =>
                {
                    var pooled = Instantiate(destructiblePrefab, destructibleContainer);
                    pooled.Initialize(_destructiblePool, _destroyEffectPool);
                    return pooled;
                }, MaxDestructibleNum);
        }

        public void Initialize(MovementTracker movementTracker, List<MoveDistTracker> trackers)
        {
            _moveDirTracker = movementTracker;
            _moveDistTrackerList = trackers;
            _creatingDist = trackers.Count switch
            {
                // todo 밸런스 - 인원수에 따른 거리량 
                1 => DefaultCreatingDist,
                2 => DefaultCreatingDist * 1.85f,
                3 => DefaultCreatingDist * 2.65f,
                4 => DefaultCreatingDist * 3.25f,
                _ => throw new Exception("인원 수가 올바르지 않음")
            };
        }

        public async UniTaskVoid StartCreation(CancellationTokenSource cancellationTokenSource)
        {
            if (_creatingCts != null)
            {
                _creatingCts.Cancel();
                _creatingCts.Dispose();
            }

            _creatingCts = new CancellationTokenSource();
            var cts = CancellationTokenSource.CreateLinkedTokenSource(_creatingCts.Token,
                cancellationTokenSource.Token);

            var desiredDist = _moveDistTrackerList.Sum(distanceTracker => distanceTracker.MoveDist);
            desiredDist += _creatingDist * Random.Range(0.5f, 1.5f);

            while (cts.IsCancellationRequested is false)
            {
                var curMoveDistSum = _moveDistTrackerList.Sum(distanceTracker => distanceTracker.MoveDist);

                if (curMoveDistSum > desiredDist)
                {
                    CreateDestructible();
                    desiredDist += _creatingDist * Random.Range(0.5f, 1.5f);
                }

                await UniTask.Delay(1000, cancellationToken: cts.Token);
            }
        }

        public void StopCreation() => _creatingCts?.Cancel();


        private void CreateDestructible()
        {
            if (!_moveDirTracker.GetOutsideRandomPos(out var pos, MobilityType.Ground))
            {
                return;
            }


            var destructible = _destructiblePool.Get();
            destructible.Set(destructibleDB.db["Emiya Field"], pos);
        }

    }
}