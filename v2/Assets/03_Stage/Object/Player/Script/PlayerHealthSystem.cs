using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace PupSurvivors.Stage
{
    public class PlayerHealthSystem : MonoBehaviour
    {
        public float MaxHp { get; private set; }

        public float CurHp
        {
            get => _curHp;
            private set => _curHp = Mathf.Clamp(value, 0f, MaxHp);
        }

        private float _curHp;

        public bool IsLive => CurHp > 0f;

        [field: SerializeField] public UnityEvent<float> MaxHpChangedEvent { get; private set; }
        [field: SerializeField] public UnityEvent<float> HpUpdatedEvent { get; private set; }

        private float _recovery;
        private CancellationTokenSource _recoveryCts;

        public void Initialize(float maxHp)
        {
            MaxHp = CurHp = maxHp;
            RegenerateHp().Forget();
        }
        
        private void OnDisable()
        {
            _recoveryCts?.Cancel();
        }

        public async UniTaskVoid RegenerateHp()
        {
            _recoveryCts?.CancelAndDispose();
            _recoveryCts = new CancellationTokenSource();

            var timer = 0f;
            while (true)
            {
                const float recoveryTick = 1f;
                while (timer > recoveryTick)
                {
                    timer -= recoveryTick;
                    CurHp += _recovery;
                    HpUpdatedEvent.Invoke(CurHp);
                }

                timer += Time.deltaTime;
                await UniTask.Yield(_recoveryCts.Token);
            }
        }

        public void StopRegenerateHp()
        {
            _recoveryCts?.Cancel();
        }

        public void OnStatsUpdated(CharacterStats stats)
        {
            MaxHp = stats.hp;
            MaxHpChangedEvent.Invoke(MaxHp);
            HpUpdatedEvent.Invoke(_curHp);
            _recovery = stats.hpRecovery;
        }
    }
}