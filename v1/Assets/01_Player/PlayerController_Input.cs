using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerController
{
    public Vector2 CurrentInputDir { get; private set; }
    public Vector2 LastInputDir { get; private set; }
    public bool IsRightLastInputXDir { get; private set; } = true;
    
    
    private const float DashButtonBuffer = 0.15f;
    private float _dashButtonCounter;
    public bool IsDashButtonPressed => _dashButtonCounter > 0f;

    private void InitializeInput()
    {
        CurrentInputDir = Vector2.zero;
        LastInputDir = Vector2.right;

        InputManager.MovePerformedEvent.AddListener(OnMovePerformed);
        InputManager.MoveCanceledEvent.AddListener(() => CurrentInputDir = Vector2.zero);
        InputManager.DashButtonDownedEvent.AddListener(() => _dashButtonCounter = DashButtonBuffer);
    }

    private void OnMovePerformed(Vector2 dir)
    {
        LastInputDir = CurrentInputDir = dir;
        IsRightLastInputXDir = dir.x switch
        {
            < 0 => false,
            > 0 => true,
            _ => IsRightLastInputXDir
        };
    }

    private void UpdateDashBuffer()
    {
        _dashButtonCounter -= Time.deltaTime;
        if (_dashButtonCounter > 0f)
        {
            CurrentState.OnDashButtonDown?.Invoke();
        }
    }
    
    public void ResetDashButton() => _dashButtonCounter = 0f;
}