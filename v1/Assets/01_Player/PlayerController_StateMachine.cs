using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public partial class PlayerController
{
    public PlayerStateBase CurrentState { get; private set; }
    public PlayerStateBase LastState { get; private set; }

    private PlayerStateBase[] _playerStates;

    private void InitializeStateMachine()
    {
        PlayerStateBase.Initialize(this);
        var numOfStates = Enum.GetValues(typeof(PlayerStateBase.PlayerState)).Length;
        _playerStates = new PlayerStateBase[numOfStates];
        _playerStates[0] = new PlayerStateMove();
        _playerStates[1] = new PlayerStateDash();

        LastState = CurrentState = _playerStates[0];
    }

    public void ChangeState(PlayerStateBase.PlayerState desiredState)
    {
        if (CurrentState.ThisEnum == desiredState)
        {
            
#if UNITY_EDITOR
            Debug.LogAssertion($"플레이어 - {desiredState}에서 동일 상태로 변경 시도");
#endif
            
            return;
        }

        LastState = CurrentState;
        LastState.OnStateExit?.Invoke();
        
#if UNITY_EDITOR
        Debug.Log($"플레이어 - {LastState.ThisEnum} -> {desiredState} 상태 변경");
#endif
        CurrentState = _playerStates[(int)desiredState];
        CurrentState.OnStateEnter?.Invoke();
    }

    public void ChangeState(PlayerStateBase desiredState)
    {
        ChangeState(desiredState.ThisEnum);
    }

    private void UpdateStateMachine()
    {
        CurrentState.OnUpdate?.Invoke();
    }

    private void FixedUpdateStateMachine()
    {
        CurrentState.OnFixedUpdate?.Invoke();
    }
}