using System;
using System.Collections;
using System.Collections.Generic;
using PupSurvivors.Stage.UI;
using UnityEngine;
using UnityEngine.Events;

namespace PupSurvivors.Stage
{
    public class UncommonEnemyHealthSystem : DamagableHealthSystemBase, IHpNotifier
    {
        public float MaxHp { get; protected set; }

        public event Action<float> ActionOnCurHpChanged;
        public event Action<float> ActionOnMaxHpChanged;
        
        public void SetMaxHp(float maxHp)
        {
            MaxHp = maxHp;
            CurHp = maxHp;
            ActionOnMaxHpChanged?.Invoke(maxHp);
        }

        public override void Damage((float, bool) damage)
        {
            if (!IsLive) return;

            CurHp -= damage.Item1;
            if (CurHp < 0f)
            {
                CurHp = 0f;
            }
            ActionOnCurHpChanged?.Invoke(CurHp);
            StageManager.Instance.ShowFloatingDamage((int)damage.Item1, damage.Item2, Damagable.FloatingDamageWorldPos);
            if (!IsLive)
            {
                Damagable.OnDied();
            }
            else
            {
                Damagable.OnDamaged();
            }
        }

        public override void Knockback(float power)
        {
            if (!IsLive)
            {
                return;
            }

            Knockbackable?.OnKnockbacked(power);
        }

        public override void Kill()
        {
            if (!IsLive) return;

            CurHp = 0f;
            Damagable.OnDied();
        }

    }
}