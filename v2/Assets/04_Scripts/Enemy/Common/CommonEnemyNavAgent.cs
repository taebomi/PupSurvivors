using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using PupSurvivors.Enemy;
using PupSurvivors.Stage;
using PupSurvivors.Stage.PathFinding;
using Unity.Mathematics;
using UnityEngine;

public class CommonEnemyNavAgent : MonoBehaviour
{
    public Vector2 CurDir { get; private set; }

    private MobilityType _mobilityType;
    private float _maxSpeed;

    private PathFinder _pathFinder;
    private FlowField _curFlowField;
    private List<FlowField> _flowFields;

    private CancellationTokenSource _cts;

    public const int MaxNearestCheckingDist = 8;

    public enum MoveType
    {
        KeepLastDir,
        DirectToPlayer,
        FollowFlowField,
    }

    private void OnDisable()
    {
        _cts?.CancelAndDispose();
    }

    public void Set(MoveType moveType)
    {
    }

    private async UniTaskVoid UpdateDirToPlayer()
    {
        // 1인일 경우
        var stageManager = StageManager.Instance;
        if (stageManager.IsSoloPlay)
        {
            _curFlowField = _pathFinder.FlowFields[0];
            while (_cts.IsCancellationRequested is false)
            {
                CurDir = _curFlowField.GetCell(transform.position, _mobilityType).Dir;
                await UniTask.Delay(TimeSpan.FromSeconds(0.25), cancellationToken: _cts.Token);
            }
        }
        // 다인일 경우
        else
        {
            _curFlowField = _pathFinder.FlowFields[0];
            while (_cts.IsCancellationRequested is false)
            {
                var curPos = transform.position;
                var curCell = _curFlowField.GetCell(curPos, _mobilityType);
                if (curCell.Dist <= MaxNearestCheckingDist)
                {
                    CurDir = curCell.Dir;
                }
                else
                {
                    _curFlowField = _pathFinder.GetNearestFlowField(curPos);
                    CurDir = _curFlowField.GetCell(curPos, _mobilityType).Dir;
                }

                await UniTask.Delay(TimeSpan.FromSeconds(0.25), cancellationToken: _cts.Token);
            }
        }
    }
}