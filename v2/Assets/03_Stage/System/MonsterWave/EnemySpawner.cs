using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using PupSurvivors.Enemy;
using UnityEngine;

namespace PupSurvivors.Stage
{
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private EnemyDB enemyDB;
        [SerializeField] private StageWaveData stageWaveData;

        private MovementTracker _movementTracker;

        private Transform _enemyContainerTr,
            _commonContainerTr,
            _eliteContainerTr,
            _miniBossContainerTr,
            _bossContainerTr;

        [SerializeField] private CommonEnemy commonEnemyPrefab;
        private Pool.ObjectPool<CommonEnemy> _commonEnemyPool;


        [SerializeField] private MiniBossHealthBar miniBossHealthBar;
        
        private float _timer;

        private CancellationTokenSource _waveCts, _periodicCts;

        public const int MaxCommonEnemyNum = 1000;

        protected void Awake()
        {
            _enemyContainerTr = new GameObject("Enemy Container").transform;
            _commonContainerTr = new GameObject("Common").transform;
            _eliteContainerTr = new GameObject("Elite").transform;
            _miniBossContainerTr = new GameObject("MiniBoss").transform;
            _bossContainerTr = new GameObject("Boss").transform;
            _commonContainerTr.SetParent(_enemyContainerTr);
            _eliteContainerTr.SetParent(_enemyContainerTr);
            _miniBossContainerTr.SetParent(_enemyContainerTr);
            _bossContainerTr.SetParent(_enemyContainerTr);


            _commonEnemyPool = new Pool. ObjectPool<CommonEnemy>(() =>
            {
                var pooled = Instantiate(commonEnemyPrefab, _commonContainerTr);
                pooled.Initialize(_commonEnemyPool);
                return pooled;
            }, MaxCommonEnemyNum);

            _timer = 0f;
        }

        private void OnDisable()
        {
            StopWave();
        }

        protected void OnDestroy()
        {
            _waveCts?.CancelAndDispose();
            _periodicCts?.CancelAndDispose();
        }

        public void Initialize(MovementTracker movementTracker)
        {
            _movementTracker = movementTracker;
        }

        public Transform GetContainer(EnemyType type)
        {
            return type switch
            {
                EnemyType.Common => _commonContainerTr,
                EnemyType.Elite => _eliteContainerTr,
                EnemyType.MiniBoss => _miniBossContainerTr,
                EnemyType.Boss => _bossContainerTr,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }

        public async UniTaskVoid StartWave()
        {
            if (_waveCts is not null)
            {
                _waveCts.Cancel();
                _waveCts.Dispose();
            }

            _waveCts = new CancellationTokenSource();

            CheckCommon().Forget();
            CheckUncommon().Forget();

            while (_waveCts.IsCancellationRequested is false)
            {
                _timer += Time.deltaTime;
                await UniTask.Yield(_waveCts.Token);
            }
        }

        public void StopWave() => _waveCts?.Cancel();

        //todo waveCts 변경하기, timer만 멈춰주면 됨.

        private async UniTaskVoid CheckCommon()
        {
            var periodicCts = new CancellationTokenSource();
            foreach (var waveData in stageWaveData.commonWaveData)
            {
                while (_timer < waveData.time)
                {
                    await UniTask.Yield(_waveCts.Token);
                }

                switch (waveData.option)
                {
                    case CommonWaveData.SpawnOption.StartPeriodic:
                        periodicCts.CancelAndDispose();
                        periodicCts = new CancellationTokenSource();
                        var cts = CancellationTokenSource.CreateLinkedTokenSource(periodicCts.Token, _waveCts.Token);
                        foreach (var enemyData in waveData.data)
                        {
                            CreateCommonPeriodic(enemyData, cts.Token).Forget();
                        }

                        break;
                    case CommonWaveData.SpawnOption.StopPeriodic:
                        periodicCts.Cancel();
                        break;
                    case CommonWaveData.SpawnOption.Once:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private async UniTaskVoid CreateCommonPeriodic(CommonWaveData.Data waveData, CancellationToken ct)
        {
            var timer = 0f;
            var interval = 1f / waveData.num;
            var enemyData = waveData.commonEnemyData;
            while (ct.IsCancellationRequested is false)
            {
                if (timer > interval)
                {
                    timer -= interval;
                    if (_commonEnemyPool.UsedCount < MaxCommonEnemyNum)
                    {
                        if (_movementTracker.GetOutsideRandomPos(out var pos, enemyData.mobilityType))
                        {
                            var common = _commonEnemyPool.Get();
                            common.Set(enemyData, pos);
                            common.ChasePlayer().Forget();
                        }
                    }
                }

                timer += Time.deltaTime;
                await UniTask.Yield(ct);
            }
        }

        private async UniTaskVoid CheckUncommon()
        {
            foreach (var uncommonWaveData in stageWaveData.uncommonWaveData)
            {
                while (_timer < uncommonWaveData.time)
                {
                    await UniTask.Yield(_waveCts.Token);
                }

                var enemyData = uncommonWaveData.enemyData;

                if (_movementTracker.GetOutsideRandomPos(out var pos, enemyData.mobilityType))
                {
                    var uncommon = Instantiate(enemyData.prefab, pos, Quaternion.identity,
                        enemyData.type is EnemyType.Elite ? _eliteContainerTr : _miniBossContainerTr);
                    if (enemyData.type is EnemyType.MiniBoss) // 체력 바 추가
                    {
                        var healthBar = Instantiate(miniBossHealthBar, uncommon.transform);
                        healthBar.Initialize(uncommon.HealthSystem);
                    }

                    uncommon.SetData(enemyData);
                    miniBossHealthBar.transform.localPosition = uncommon.HpBarPos;
                }
            }
        }
    }
}