using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Pool;

public class PlayerStateDash : PlayerStateBase
{
    public override PlayerState ThisEnum => PlayerState.Dash;

    private Vector2 _dashDir;

    private bool _dashStatusChanged;

    private const float DashDuration = 0.3f;
    private const float DashControlableTime = 0.1f;

    private CancellationTokenSource _dashCts;
    private CancellationTokenSource _stateCts;

    public PlayerStateDash()
    {
        InitializeGhostEffect();

        OnStateEnter = StateEnter;
        OnFixedUpdate = FixedUpdate;
        OnStateExit = StateExit;
    }

    private void StateEnter()
    {
        _stateCts?.CancelAndDispose();
        _stateCts = new CancellationTokenSource();

        Player.SpUsedEvent.Invoke();
        Player.SetFaceAnimation(TaeBoMiCache.FaceStateType.Cute);
        SetDashDir(Player.LastInputDir);
        StartDash().Forget();
        ActivateGhostEffect().Forget();
    }

    private void StateExit()
    {
        _stateCts.Cancel();
    }

    private void FixedUpdate()
    {
        Player.Rb.linearVelocity = Player.velocity = _dashDir * Player.CurrentSpeed;
    }

    private void SetDashDir(Vector2 dir)
    {
        _dashDir = dir.normalized;
        Player.SetMovementAnimation(Player.IsRightLastInputXDir, true);
    }

    private async UniTaskVoid StartDash()
    {
        _dashCts?.CancelAndDispose();
        _dashCts = new CancellationTokenSource();

        ControlDashDirection().Forget();
        // todo - 플레이어 이동속도 변경 시에 이것도 바꿔줘야함.
        await DOTween.To(x => Player.CurrentSpeed = x,
                Player.CurrentStats.movementSpeed * 7.5f, Player.CurrentStats.movementSpeed,
                DashDuration)
            .SetEase(Ease.OutQuad).Play().WithCancellation(_dashCts.Token);

        Player.ChangeState(Player.LastState);
    }

    private async UniTaskVoid ControlDashDirection()
    {
        var timer = DashControlableTime;
        while (timer > 0f)
        {
            timer -= Time.deltaTime;

            var inputDir = Player.CurrentInputDir;
            if (inputDir != Vector2.zero)
            {
                SetDashDir(inputDir);
            }

            await UniTask.Yield(_dashCts.Token);
        }
    }

    #region 고스트 이펙트

    private Transform _ghostEffectContainerTr;
    private IObjectPool<GhostEffect> _ghostEffectPool;

    private async UniTaskVoid ActivateGhostEffect()
    {
        while (true)
        {
            var ghostEffect = _ghostEffectPool.Get();
            ghostEffect.Set(Player.transform.position, Player.Sr.sprite).Forget();
            await UniTask.Delay(60, cancellationToken: _stateCts.Token);
        }
    }

    private void InitializeGhostEffect()
    {
        _ghostEffectContainerTr = new GameObject("Player GhostEffect Container").transform;
        _ghostEffectContainerTr.SetParent(StageManager.Instance.transform);
        _ghostEffectPool = new ObjectPool<GhostEffect>(CreateGhostEffect, GetGhostEffect, ReleaseGhostEffect);
    }

    private GhostEffect CreateGhostEffect()
    {
        var ghostEffect = new GameObject("Player Ghost Effect", typeof(SpriteRenderer), typeof(GhostEffect))
            .GetComponent<GhostEffect>();
        ghostEffect.transform.SetParent(_ghostEffectContainerTr);
        ghostEffect.SetManagedPool(_ghostEffectPool);
        return ghostEffect;
    }

    private static void GetGhostEffect(GhostEffect ghostEffect)
    {
        ghostEffect.gameObject.SetActive(true);
    }

    private static void ReleaseGhostEffect(GhostEffect ghostEffect)
    {
        ghostEffect.gameObject.SetActive(false);
    }

    #endregion
}