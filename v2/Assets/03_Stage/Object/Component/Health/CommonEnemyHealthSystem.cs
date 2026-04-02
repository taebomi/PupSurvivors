using PupSurvivors.Enemy;
using PupSurvivors.Stage.UI;
using UnityEngine;

namespace PupSurvivors.Stage
{
    public class CommonEnemyHealthSystem : DamagableHealthSystemBase
    {
        public void SetHp(int hp)
        {
            CurHp = hp;
        }
        
        public override void Damage((float, bool) damage)
        {
            if (!IsLive) return;

            CurHp -= damage.Item1;
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