using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using PupSurvivors.Enemy;
using PupSurvivors.Stage;
using PupSurvivors.Stage.PathFinding;
using Unity.Mathematics;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEngine;
using Direction = TaeBoMiCache.Direction;
using Random = UnityEngine.Random;

public class MovementTracker
{
    private readonly PathFinder _pathFinder;
    public Direction CurDir { get; private set; }

    private readonly Transform _targetTr;
    private Vector3 _lastPos;

    private readonly Queue<int2> _int2DirQueue;
    private int2 _int2DirSum;

    private CancellationTokenSource _trackingCts;

    // 옵션
    private const double TrackInterval = 0.25;
    private const int TrackCount = 12;
    private const int MaxRetryNum = 1000;
    //

    private const float OutMinHalfWidth = CameraManager.HalfWidth + 2;
    private const float OutMaxHalfWidth = CameraManager.HalfWidth + PathFinder.OutSize;
    private const float OutMinHalfHeight = CameraManager.HalfHeight + 2;
    private const float OutMaxHalfHeight = CameraManager.HalfHeight + PathFinder.OutSize;
    
    private static readonly int2 None = int2.zero;
    private static readonly int2 Up = new(0, 1);
    private static readonly int2 Down = new(0, -1);
    private static readonly int2 Left = new(-1, 0);
    private static readonly int2 Right = new(1, 0);

    public MovementTracker(PathFinder pathFinder)
    {
        _pathFinder = pathFinder;
        _targetTr = _pathFinder.CostField.TargetTr;
        _lastPos = _targetTr.position;

        _int2DirQueue = new Queue<int2>(TrackCount);
    }

    public async UniTaskVoid StartTracker()
    {
        _trackingCts?.CancelAndDispose();
        _trackingCts = new CancellationTokenSource();
        
        _int2DirQueue.Clear();
        for (var i = 0; i < TrackCount; i++)
        {
            _int2DirQueue.Enqueue(None);
        }
        _int2DirSum = int2.zero;

        while (_trackingCts.IsCancellationRequested is false)
        {
            var curPos = _targetTr.position;

            var curInt2Dir = curPos == _lastPos ? None : Vector3ToDirectionInt2(curPos - _lastPos);

            _int2DirQueue.Enqueue(curInt2Dir);
            var lastInt2Dir = _int2DirQueue.Dequeue();
            _int2DirSum -= lastInt2Dir;
            _int2DirSum += curInt2Dir;

            CurDir = Int2ToDirection(_int2DirSum);

            _lastPos = curPos;
            await UniTask.Delay(TimeSpan.FromSeconds(TrackInterval), cancellationToken: _trackingCts.Token);
        }
    }

    public void StopTracker()
    {
        _trackingCts?.Cancel();
        CurDir = Direction.None;
    }

    public bool IsOutside(Vector3 pos)
    {
        var diff = pos - _targetTr.position;
        return math.abs(diff.x) >= OutMinHalfWidth || math.abs(diff.y) >= OutMinHalfHeight;
    }

    public Vector3 GetOutsideRandomPos()
    {
        var randomDir = Random.Range(0, 5);
        return GetOutsidePos((Direction)randomDir);
    }

    public bool GetOutsideRandomPos(out Vector3 randomPos, MobilityType mobilityType)
    {
        for (var i = 0; i < MaxRetryNum; i++)
        {
            randomPos = GetOutsideRandomPos();
            if (_pathFinder.CanSpawn(randomPos, mobilityType))
            {
                return true;
            }
        }
        randomPos = Vector3.zero;
        return false;
    }

    public bool GetMovingDirectionRandomPos(out Vector3 pos, MobilityType mobilityType)
    {
        if (CurDir is Direction.None)
        {
            return GetOutsideRandomPos(out pos, mobilityType);
        }

        for (int i = 0; i < MaxRetryNum; i++)
        {
            pos = GetOutsidePos(CurDir);
            if (_pathFinder.CanSpawn(pos, mobilityType))
            {
                return true;
            }
        }

        // 실패했을 경우 전체 랜덤으로 재시도
        return GetOutsideRandomPos(out pos, mobilityType);
    }

    public Vector3 GetOutsidePos(Direction dir)
    {
        var randomPos = _targetTr.position;
        randomPos.z = 0f;
        switch (dir)
        {
            case Direction.Up:
                randomPos.x += Random.Range(-OutMinHalfWidth, OutMaxHalfWidth);
                randomPos.y += OutMinHalfHeight;
                break;
            case Direction.Down:
                randomPos.x += Random.Range(-OutMaxHalfWidth, OutMinHalfWidth);
                randomPos.y -= OutMinHalfHeight;
                break;
            case Direction.Left:
                randomPos.x -= OutMinHalfWidth;
                randomPos.y += Random.Range(-OutMinHalfHeight, OutMaxHalfHeight);
                break;
            case Direction.Right:
                randomPos.x += OutMinHalfWidth;
                randomPos.y += Random.Range(-OutMaxHalfHeight, OutMinHalfHeight);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(dir), dir, null);
        }

        return randomPos;
    }

    private static Direction Int2ToDirection(int2 dir)
    {
        if (dir.x >= dir.y)
        {
            return dir.x >= -dir.y ? Direction.Right : Direction.Down;
        }
        else
        {
            return dir.x >= -dir.y ? Direction.Left : Direction.Up;
        }
    }
    private static int2 Vector3ToDirectionInt2(Vector3 dir)
    {
        if (dir.x >= dir.y)
        {
            return dir.x >= -dir.y ? Right : Down;
        }
        else
        {
            return dir.x >= -dir.y ? Left : Up;
        }
    }
}