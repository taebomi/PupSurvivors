using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using PupSurvivors.ObjectPool;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class ItemObjectBase : MonoBehaviour
{
    [field: SerializeField] public SpriteRenderer MainSr { get; protected set; }
    [SerializeField] protected SpriteRenderer shadowSr;

    [SerializeField] protected Rigidbody2D rb;
    [field: SerializeField] public CircleCollider2D MainCollider { get; private set; }

    [SerializeField] private AudioClip drawSe, useSe;

    public State CurrentState { get; private set; }
    public bool IsVisible { get; private set; }


    private Transform _mainSrTr;

    public enum State
    {
        Init,
        CanUse,
        Drawing,
        Used,
    }

    private CancellationTokenSource _disableCts, _flowCts;

    private void Awake()
    {
        _mainSrTr = MainSr.transform;
        ChangeState(State.Init);
    }

    private void OnEnable()
    {
        _disableCts = new CancellationTokenSource();
    }

    private void OnDisable()
    {
        _disableCts.CancelAndDispose();
    }

    public virtual void OnVisible()
    {
        IsVisible = true;
        Flow().Forget();
    }

    public virtual void OnInvisible()
    {
        IsVisible = false;
    }

    public void ChangeState(State state)
    {
        CurrentState = state;
        switch (state)
        {
            case State.Init:
                rb.simulated = false;
                MainSr.enabled = false;
                shadowSr.enabled = false;
                break;
            case State.CanUse:
                rb.simulated = true;
                MainSr.enabled = true;
                MainSr.sortingOrder = -1;
                shadowSr.enabled = true;
                break;
            case State.Drawing:
                SoundManager.Instance.PlaySoundEffect(drawSe).Forget();
                rb.simulated = false;
                MainSr.sortingOrder = 1;
                break;
            case State.Used:
                SoundManager.Instance.PlaySoundEffect(useSe).Forget();
                MainSr.enabled = false;
                shadowSr.enabled = false;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }
    }

    public void Set()
    {
        ChangeState(State.CanUse);
    }

    public void Set(Vector3 pos)
    {
        transform.position = pos;
        ChangeState(State.CanUse);
    }

    private async UniTaskVoid Flow()
    {
        if (CurrentState is not State.CanUse)
        {
            return;
        }

        _flowCts?.CancelAndDispose();
        _flowCts = new CancellationTokenSource();
        var cts = CancellationTokenSource.CreateLinkedTokenSource(_disableCts.Token, _flowCts.Token);

        const float halfFlowingHeight = 0.125f;
        var timer = 0f;
        while (IsVisible && CurrentState is State.CanUse)
        {
            var currentHeight = halfFlowingHeight + halfFlowingHeight * Mathf.Sin(timer);
            _mainSrTr.localPosition = new Vector3(0f, currentHeight);
            timer += Time.deltaTime;
            await UniTask.Yield(cts.Token);
        }
    }

    public void Use(PlayerController target)
    {
        ChangeState(State.Used);
        ApplyItem(target);
    }

    protected abstract void ApplyItem(PlayerController target);
}