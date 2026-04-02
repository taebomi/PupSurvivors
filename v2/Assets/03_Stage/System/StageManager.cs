using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using PathFinder = PupSurvivors.Stage.PathFinding.PathFinder;

namespace PupSurvivors.Stage
{
    public partial class StageManager : MonoBehaviour
    {
        #region Singletone

        public static StageManager Instance { get; private set; }

        #endregion

        [SerializeField] private StageStateChannelSO stageStateChannelSO;

        [SerializeField] private StageInfo stageInfo;

        [SerializeField] private DestructibleCreator destructibleCreator;
        [field: SerializeField] public PathFinder PathFinder { get; private set; }
        public MovementTracker MovementTracker { get; private set; }


        public StageState State { get; private set; }

        private List<UniTask> _initTaskList;

        private CancellationTokenSource _gameFinishedCts;
        public CancellationTokenSource DestroyCts { get; private set; }


        protected virtual void Awake()
        {
            Instance = this;

            DestroyCts = new CancellationTokenSource();

            _initTaskList = new List<UniTask>();
            InitializeTime();
            InitializeChest();
            InitPlayer();
            CameraManager.Instance.Initialize(CurPlayers);
            InitEnemy();
            InitializeItem();
            InitializeUI();
        }


        protected virtual async UniTask Start()
        {
            await UniTask.WhenAll(_initTaskList);

            ChangeState(StageState.Init);
            
            PathFinder.Initialize(CameraManager.Instance.MainCam, CurPlayers);
            PathFinder.StartPathFind().Forget();
            MovementTracker = new MovementTracker(PathFinder);
            MovementTracker.StartTracker().Forget();

            // destructibleCreator.Initialize(PathFindingSystem,
            //     new List<MoveDistTracker>() { player.MoveDistTracker });
            // destructibleCreator.StartCreation(_gameFinishedCts).Forget();
            // MonsterWaveManager.StartWave().Forget();

            ChangeState(StageState.Start);

            StartTimer().Forget();
        }

        private void ChangeState(StageState state)
        {
            State = state;
            stageStateChannelSO.RaiseEvent(state);
            switch (state)
            {
                case StageState.None:
                    break;
                case StageState.Init:
                    _gameFinishedCts = new CancellationTokenSource();
                    break;
                case StageState.Start:
                    break;
                case StageState.GameOver:
                    break;
                case StageState.GameFinished:
                    _gameFinishedCts?.Cancel();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        private void OnDestroy()
        {
            DestroyCts.CancelAndDispose();
            ChangeState(StageState.GameFinished);
            Instance = null;
        }
    }
}