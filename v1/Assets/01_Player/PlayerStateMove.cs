using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateMove : PlayerStateBase
{
    public override PlayerState ThisEnum => PlayerState.Move;

    public PlayerStateMove()
    {
        OnStateEnter = StateEnter;
        OnUpdate = Update;
        OnFixedUpdate = FixedUpdate;
        OnDashButtonDown = DashButtonDown;
    }

    private void StateEnter()
    {
        Player.SetFaceAnimation(TaeBoMiCache.FaceStateType.Normal);
    }

    private void Update()
    {
        CalculateAcceleration();
        Player.SetMovementAnimation(Player.IsRightLastInputXDir,
            Player.CurrentInputDir != Vector2.zero);
    }

    private void FixedUpdate()
    {
        Player.Rb.linearVelocity = Player.velocity =
            Player.LastInputDir * Player.CurrentSpeed;
    }

    private void CalculateAcceleration()
    {
        var currentMovementSpeed = Player.CurrentSpeed;
        // 가속
        if (Player.CurrentInputDir != Vector2.zero)
        {
            currentMovementSpeed += Player.accelerationSpeed * Time.deltaTime;
            if (currentMovementSpeed > Player.CurrentStats.movementSpeed)
            {
                currentMovementSpeed = Player.CurrentStats.movementSpeed;
            }
        }
        // 감속
        else
        {
            currentMovementSpeed -= Player.deaccelerationSpeed * Time.deltaTime;
            if (currentMovementSpeed < 0f)
            {
                currentMovementSpeed = 0f;
            }
        }

        Player.CurrentSpeed = currentMovementSpeed;
    }

    private void DashButtonDown()
    {
        var currentInteractable = Player.InteractableChecker.CurrentClosestInteractable;
        if (currentInteractable)
        {
            Player.ResetDashButton();
            currentInteractable.Interact();
            return;
        }

        if (Player.CanUseSkill)
        {
            Player.ResetDashButton();
            Player.ChangeState(PlayerState.Dash);
            return;
        }
    }
}