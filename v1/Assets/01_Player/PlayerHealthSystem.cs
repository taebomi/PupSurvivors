using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class PlayerHealthSystem : HealthSystemBase
{
    [field: SerializeField] public UnityEvent<float> MaxHpChangedEvent { get; private set; } = new();
    [field: SerializeField] public UnityEvent<float> HpUpdateEvent { get; private set; } = new();

    private float _recoveryTimer;
    private float _recovery;

    private bool _isInvincible;

    public const float InvincibleDuration = 0.25f;

    private CancellationTokenSource _disableCts;

    private void OnEnable()
    {
        _disableCts = new CancellationTokenSource();
    }

    private void OnDisable()
    {
        _disableCts.CancelAndDispose();
    }

    private void Update()
    {
        RegenerateHp();
    }

    public void SetRecovery(float rate)
    {
        _recovery = rate;
    }

    public void OnStatsUpdated(CharacterStats stats)
    {
        if (Math.Abs(stats.hp - MaxHp) > 1f)
        {
            SetMaxHp(stats.hp);
        }

        if (Mathf.Abs(stats.hpRecovery - _recovery) > 0.1f)
        {
            SetRecovery(stats.hpRecovery);
        }
    }

    private void RegenerateHp()
    {
        _recoveryTimer += Time.deltaTime;
        while (_recoveryTimer > 1f)
        {
            CurrentHp += _recovery;
            _recoveryTimer -= 1f;
            HpUpdateEvent.Invoke(CurrentHp);
        }

    }

    public override void SetMaxHp(float maxHp)
    {
        var diff = maxHp - MaxHp;
        MaxHp = maxHp;
        CurrentHp += diff;
        MaxHpChangedEvent.Invoke(maxHp);
        HpUpdateEvent.Invoke(CurrentHp);
    }

    public void Heal(float value)
    {
        CurrentHp += value;
        HpUpdateEvent.Invoke(CurrentHp);
    }

    public void Damage(int damage)
    {
        if (_isInvincible)
        {
            return;
        }

        CurrentHp -= damage;
        HpUpdateEvent.Invoke(CurrentHp);
        DamagedEvent.Invoke(0f);
    }

    public void OnDamaged() => SetInvincible().Forget();

    private async UniTaskVoid SetInvincible()
    {
        _isInvincible = true;
        var timer = InvincibleDuration;
        while (timer > 0f)
        {
            timer -= Time.fixedDeltaTime;
            await UniTask.Yield(PlayerLoopTiming.FixedUpdate, _disableCts.Token);
        }

        _isInvincible = false;
    }

    public override void Damage(float damage, bool isCritical)
    {
        throw new System.NotImplementedException();
    }

    public override void Damage(float damage, bool isCritical, float knockbackPower)
    {
        throw new System.NotImplementedException();
    }

    public override void Knockback(float knockbackPower)
    {
        throw new NotImplementedException();
    }

    public override void Kill()
    {
        throw new System.NotImplementedException();
    }
}