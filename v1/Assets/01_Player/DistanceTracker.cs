using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class DistanceTracker : MonoBehaviour
{
    public float MoveDistance { get; private set; } 
    
    private IMovable _movable;
    
    private CancellationTokenSource _trackingCts;

    private void Awake()
    {
        _movable = GetComponent<IMovable>();
        StartMoveDistanceTracking();
    }

    private void OnDisable()
    {
        _trackingCts?.CancelAndDispose();
    }

    public void StartMoveDistanceTracking()
    {
        _trackingCts?.CancelAndDispose();
        _trackingCts = new CancellationTokenSource();
        TrackMoveDistance().Forget();
    }

    public void StopMoveDistanceTracking()
    {
        _trackingCts.Cancel();
    }

    private async UniTaskVoid TrackMoveDistance()
    {
        var tr = transform;
        var lastPos = tr.position;
        while (true)
        {
            // 이전 위치와 현재 위치를 비교하여 거리 계산
            var currentPos = tr.position;
            var distance = (currentPos - lastPos).magnitude;
            // 계산된 거리가 최대 속도보다 느릴 경우에만
            if (distance <= _movable.CurrentSpeed * Time.deltaTime)
            {
                MoveDistance += distance;
            }

            lastPos = currentPos;
            await UniTask.Yield(_trackingCts.Token);
        }
    }
}
