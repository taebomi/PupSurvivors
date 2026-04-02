using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PupSurvivors.Stage
{
    public partial class Player
    {
        public PlayerStateBase CurState { get; private set; }
        public PlayerStateBase LastState { get; private set; }

        private PlayerStateBase[] _playerStates;

        private void InitializeStateMachine()
        {
            // todo 플레이어 상태 추가할 때 추가하기
            var numOfStates = Enum.GetValues(typeof(PlayerState)).Length;
            _playerStates = new PlayerStateBase[numOfStates];
            _playerStates[0] = new PlayerStateNormal(this);

            LastState = CurState = _playerStates[0];
            CurState.OnStateEnter?.Invoke();
        }

        private void UpdateStateMachine() => CurState.OnUpdate?.Invoke();
        private void FixedUpdateStateMachine() => CurState.OnFixedUpdate?.Invoke();
        private void LateUpdateStateMachine() => CurState.OnLateUpdate?.Invoke();

        public void ChangeState(PlayerState desiredState)
        {
            if (CurState.ThisEnum == desiredState)
            {
#if UNITY_EDITOR
                Debug.LogAssertion($"{transform.name} - {desiredState}에서 동일 상태로 변경 시도");
#endif
                return;
            }

            LastState = CurState;
            LastState.OnStateExit?.Invoke();

#if UNITY_EDITOR
            Debug.Log($"{transform.name} - 상태 변경 : {LastState.ThisEnum} -> {desiredState}");
#endif

            CurState = _playerStates[(int)desiredState];
            CurState.OnStateEnter?.Invoke();
        }

        public void ChangeState(PlayerStateBase desiredState) => ChangeState(desiredState.ThisEnum);
    }
}