using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace PupSurvivors.Stage
{
    public partial class StageManager
    {
        public float CurTime { get; private set; }


        private CancellationTokenSource _timerCts;
        
        private void InitializeTime()
        {
            CurTime = 0f;
        }

        public async UniTaskVoid StartTimer()
        {
            if (_timerCts != null)
            {
                _timerCts.Cancel();
                _timerCts.Dispose();
            }
            _timerCts = new CancellationTokenSource();
            var cts = CancellationTokenSource.CreateLinkedTokenSource(_timerCts.Token, _gameFinishedCts.Token);
            
            while (cts.IsCancellationRequested)
            {
                CurTime += Time.deltaTime;
                await UniTask.Yield(cts.Token);
            }
        }

        public void StopTimer()
        {
            _timerCts?.Cancel();
        }
    }
}