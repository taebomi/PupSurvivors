using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class InputManager : Singleton<InputManager>
{
    private PlayerInputAction _inputAction;

    public static readonly UnityEvent<Vector2> MovePerformedEvent = new();
    public static readonly UnityEvent MoveCanceledEvent = new();

    public static readonly UnityEvent DashButtonDownedEvent = new();

    protected override void AwakeAfter()
    {
        _inputAction = new PlayerInputAction();
        _inputAction.Control.Enable();

        _inputAction.Control.Move.performed += context => MovePerformedEvent.Invoke(context.ReadValue<Vector2>());
        _inputAction.Control.Move.canceled += _ => MoveCanceledEvent.Invoke();

        _inputAction.Control.Dash.started += _ => DashButtonDownedEvent.Invoke();
    }
}