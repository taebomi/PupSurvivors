using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;

public partial class StageManager : Singleton<StageManager>
{
    [field: SerializeField] public EquipmentDB EquipmentDB { get; private set; }

    public enum StageState
    {
        Init,
        Start,
        GameOver,
    }

    [field: SerializeField] public UnityEvent<StageState> StateChangedEvent { get; private set; }
    public StageState CurrentStageState { get; private set; }
    

    public float CurrentTimer { get; private set; }

    private CharacterData _characterData;

    private CancellationTokenSource _destroyCts;

    private List<UniTask> _initializationList;

    protected override void AwakeAfter()
    {
        Application.targetFrameRate = 60;
        _destroyCts = new CancellationTokenSource();
        _initializationList = new List<UniTask>();
        InitializePool();
        InitializeItem();
        InitializeDamagable();
        // InitializeEnemy();
        InitializeDestructibleObject();
        InitializeExp();
    }

    private async UniTask Start()
    {
        await UniTask.WhenAll(_initializationList);
        CurrentStageState = StageState.Start;
        StateChangedEvent.Invoke(CurrentStageState);
        CheckPlayerMoveDistance(PlayerController.Instance).Forget();
    }

    public void AddInitQueue(UniTask task)
    {
        _initializationList.Add(task);
    }

    private void OnDestroy()
    {
        _destroyCts.CancelAndDispose();
    }
}