using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using PupSurvivors.Stage;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace PupSurvivors.Enemy
{
    public class EnemyManager : Singleton<EnemyManager>
    {
        [SerializeField] private StageWaveData currentStageWaveData;
        [SerializeField] private EnemyDB enemyDB;


        [Header("Common Enemy"), SerializeField]
        private CommonEnemy commonEnemyPrefab;

        [SerializeField] private Transform commonEnemyContainer;
        private ObjectPool<CommonEnemy> _commonEnemyPool;

        public HashSet<EnemyBase> VisibleEnemySet { get; private set; }
        private List<EnemyBase> _nearEnemyList;
        private List<Collider2D> _nearEnemyColliders;
        public IDamagable NearestDamagableEnemy { get; private set; }
        private Collider2D _nearestEnemyCollider;

        public ContactFilter2D ContactFilter2D { get; private set; }

        private Transform _playerTr;

        public const int MaxEnemyNum = 750;


        protected override void AwakeAfter()
        {
            _playerTr = PlayerController.Instance.transform;

            VisibleEnemySet = new HashSet<EnemyBase>(750);

            _commonEnemyPool = new ObjectPool<CommonEnemy>(CreateCommonEnemy, defaultCapacity: 750,
                collectionCheck: false, maxSize: 500);

            _nearEnemyColliders = new List<Collider2D>(750);
            ContactFilter2D = new ContactFilter2D
            {
                layerMask = LayerMask.GetMask("GroundEnemy", "AirEnemy"), useLayerMask = true
            };
            StartWave().Forget();
            FindNearestEnemy().Forget();
        }

        public EnemyBase GetVisibleEnemy()
        {
            var visibleEnemyNum = VisibleEnemySet.Count;
            return visibleEnemyNum > 0 ? VisibleEnemySet.ElementAt(Random.Range(0, visibleEnemyNum)) : null;
        }

        public int GetVisibleEnemy(EnemyBase[] enemyBases)
        {
            var visibleEnemyCount = VisibleEnemySet.Count;
            var enemyBasesLength = enemyBases.Length;

            if (visibleEnemyCount > enemyBasesLength)
            {
                for (var i = 0; i < enemyBases.Length; i++)
                {
                    EnemyBase temp;
                    do
                    {
                        temp = GetVisibleEnemy();
                    } while (enemyBases.Contains(temp));

                    enemyBases[i] = temp;
                }

                return enemyBases.Length;
            }
            else
            {
                var num = 0;
                foreach (var enemyBase in VisibleEnemySet)
                {
                    enemyBases[num] = enemyBase;
                    num++;
                }

                for (var j = num; j < enemyBases.Length; j++)
                {
                    enemyBases[num] = null;
                }

                return num;
            }
        }

        private async UniTaskVoid FindNearestEnemy()
        {
            while (true)
            {
                var nearestDist = Mathf.Infinity;
                var enemyCount = 0;
                var checkingDist = 3;
                while (enemyCount == 0)
                {
                    enemyCount = Physics2D.OverlapCircle(_playerTr.position, checkingDist, ContactFilter2D,
                        _nearEnemyColliders);
                    checkingDist += 2;
                    if (checkingDist > 9)
                    {
                        break;
                    }
                }

                if (enemyCount == 0)
                {
                    NearestDamagableEnemy = GetVisibleEnemy();
                }
                else
                {
                    var nearestEnemyCollider2D = _nearEnemyColliders[0];
                    for (var i = 1; i < _nearEnemyColliders.Count; i++)
                    {
                        var distanceSqr = (_playerTr.position - _nearEnemyColliders[i].transform.position).sqrMagnitude;
                        if (distanceSqr > nearestDist) continue;
                        nearestDist = distanceSqr;
                        nearestEnemyCollider2D = _nearEnemyColliders[i];
                    }

                    NearestDamagableEnemy = StageManager.Instance.DamagableDict[nearestEnemyCollider2D.GetInstanceID()];
                }

                await UniTask.Yield();
            }
        }

        public async UniTaskVoid StartWave()
        {
            var timer = 0f;
            var periodicalSpawnCts = new CancellationTokenSource();

            foreach (var waveData in currentStageWaveData.waveData)
            {
                while (timer < waveData.time)
                {
                    await UniTask.Yield();
                    timer += Time.deltaTime;
                }

                switch (waveData.waveType)
                {
                    case WaveType.OutsidePeriodic:
                        periodicalSpawnCts.CancelAndDispose();
                        periodicalSpawnCts = new CancellationTokenSource();
                        foreach (var waveEnemyData in waveData.enemyDataArr)
                        {
                            SpawnCommonEnemyOutsidePeriodically(waveEnemyData, periodicalSpawnCts).Forget();
                        }

                        break;
                    case WaveType.OutsideOnce:
                        foreach (var waveEnemyData in waveData.enemyDataArr)
                        {
                            SpawnCommonEnemyOutsideOnce(waveEnemyData);
                        }

                        break;
                }
            }
        }

        private async UniTaskVoid SpawnCommonEnemyOutsidePeriodically(WaveEnemyData waveEnemyData,
            CancellationTokenSource cts)
        {
            if (waveEnemyData.num == 0)
            {
                return;
            }

            var interval = 1f / waveEnemyData.num;
            var timer = 0f;
            var enemyData = enemyDB.enemyDict[waveEnemyData.enemyName];
            while (true)
            {
                while (timer > interval)
                {
                    timer -= interval;
                    var enemy = _commonEnemyPool.Get();
                    enemy.Initialize(enemyData);

                    var pos = CameraManager.Instance.GetCameraOutsideRandomPosition();
                    while (TempPathFinder.Instance.CanSpawn(pos,enemyData.stats.type == EnemyType.Ground))
                    {
                        pos = CameraManager.Instance.GetCameraOutsideRandomPosition();
                    }


                    enemy.transform.position = pos;
                    enemy.gameObject.SetActive(true);
                    enemy.ChasePlayer().Forget();
                }

                await UniTask.Yield(cts.Token);
                timer += Time.deltaTime;
            }
        }

        private void SpawnCommonEnemyOutsideOnce(WaveEnemyData waveEnemyData)
        {
            var enemyData = enemyDB.enemyDict[waveEnemyData.enemyName];
            var pos = CameraManager.Instance.GetCameraOutsideRandomPosition();
            while (TempPathFinder.Instance.CanSpawn(pos,enemyData.stats.type == EnemyType.Ground))
            {
                pos = CameraManager.Instance.GetCameraOutsideRandomPosition();
            }

            var oneDir = (Vector2)(PlayerController.Instance.transform.position - pos);
            for (var i = 0; i < waveEnemyData.num; i++)
            {
                var enemy = _commonEnemyPool.Get();
                enemy.Initialize(enemyData);
                var delta = 0.04f * i;
                enemy.SetPosition(pos + new Vector3(Mathf.Cos(delta), Mathf.Sin(delta), 0) * delta);
                if (waveEnemyData.movementType == EnemyMovementType.OneDirection)
                {
                    enemy.SetDirection(oneDir);
                }
                else if (waveEnemyData.movementType == EnemyMovementType.ChasePlayer)
                {
                    enemy.ChasePlayer().Forget();
                }

                enemy.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// 풀 최초 생성용
        /// </summary>
        private CommonEnemy CreateCommonEnemy()
        {
            var newCommonEnemy = Instantiate(commonEnemyPrefab, commonEnemyContainer);
            StageManager.Instance.DamagableDict.Add(newCommonEnemy.MainCollider.GetInstanceID(), newCommonEnemy);
            newCommonEnemy.SetManagedPool(_commonEnemyPool);
            return newCommonEnemy;
        }
    }
}