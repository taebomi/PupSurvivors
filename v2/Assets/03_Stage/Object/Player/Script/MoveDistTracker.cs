using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace PupSurvivors.Stage
{
    public class MoveDistTracker : MonoBehaviour
    {
        public float MoveDist { get; private set; }

        private CancellationTokenSource _trackingCts;

        private void Awake()
        {
            MoveDist = 0f;
        }

        private void OnDestroy()
        {
            _trackingCts?.Cancel();
        }

        public async UniTaskVoid StartTracking()
        {
            if (_trackingCts != null)
            {
                _trackingCts.Cancel();
                _trackingCts.Dispose();
            }
            _trackingCts = new CancellationTokenSource();

            var tr = transform;
            var lastPos = tr.position;
            while (_trackingCts.IsCancellationRequested is false)
            {
                var curPos = tr.position;
                MoveDist += (curPos - lastPos).magnitude;
                lastPos = curPos;
                await UniTask.Yield(_trackingCts.Token);
            }
        }

        public void StopTracking()
        {
            _trackingCts?.Cancel();
        }
    }
}