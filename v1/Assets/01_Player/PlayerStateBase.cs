using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public abstract class PlayerStateBase
{
    public enum PlayerState
    {
        Move,
        Dash,
        Died,
        Event
    }

    protected static PlayerController Player;
    
    public abstract PlayerState ThisEnum { get; }

    public UnityAction OnStateEnter;
    public UnityAction OnUpdate;
    public UnityAction OnFixedUpdate;
    public UnityAction OnStateExit;

    public UnityAction OnDashButtonDown;

    public static void Initialize(PlayerController playerController)
    {
        Player = playerController;
    }
}
