using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using PupSurvivors.Enemy;
using PupSurvivors.Stage;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class EnemyHealthSystem : HealthSystemBase
{
    [SerializeField] private EnemyBase enemyBase;

    [SerializeField] private AudioClip se;

    public override void Damage(float damage, bool isCritical)
    {
        CurrentHp -= damage;
        StageUIManager.Instance.CreateFloatingDamage((int)damage, isCritical,
            transform.position + enemyBase.FloatingDamagePosition);
        if (!IsLive)
        {
            DieEvent.Invoke();
        }
    }

    public override void Damage(float damage, bool isCritical, float knockbackPower)
    {
        SoundManager.Instance.PlaySoundEffect(se).Forget();
        CurrentHp -= damage;
        StageUIManager.Instance.CreateFloatingDamage((int)damage, isCritical,
            transform.position + enemyBase.FloatingDamagePosition);
        DamagedEvent?.Invoke(knockbackPower);
        if (!IsLive)
        {
            DieEvent?.Invoke();
        }
    }

    public override void Knockback(float knockbackPower)
    {
        DamagedEvent?.Invoke(knockbackPower);
    }

    public override void Kill()
    {
        if (!IsLive)
            return;
        CurrentHp -= CurrentHp;
        DieEvent?.Invoke();
    }
}