using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class HealthSystemBase : MonoBehaviour
{
    public float MaxHp { get; protected set; }

    private float _currentHp;
    public float CurrentHp
    {
        get => _currentHp;
        protected set => _currentHp = Mathf.Clamp(value, 0f, MaxHp);
    }

    public bool IsLive => CurrentHp > 0f;

    public UnityEvent DieEvent;
    public UnityEvent<float> DamagedEvent;

    public virtual void SetMaxHp(float maxHp)
    {
        MaxHp = maxHp;
        CurrentHp = maxHp;
    }
    
    public abstract void Damage(float damage, bool isCritical);
    public abstract void Damage(float damage, bool isCritical, float knockbackPower);
    public abstract void Knockback(float knockbackPower);

    public abstract void Kill();

}
