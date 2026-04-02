using PupSurvivors.Stage.UI;
using UnityEngine;

namespace PupSurvivors.Stage
{
    public class DestructibleHealthSystem : DamagableHealthSystemBase
    {
        [SerializeField] private Destructible destructible;

        private float _floatingDamageYPos;

        public void Set(int hp, float floatingDamageYPos)
        {
            CurHp = hp;
            _floatingDamageYPos = floatingDamageYPos;
        }

        public override void Damage((float, bool) damage)
        {
            if (!IsLive) return;

            if (damage.Item2)
            {
                CurHp -= 1;
                StageManager.Instance.ShowFloatingDamage(1, true,
                    transform.position + new Vector3(0f, _floatingDamageYPos));
            }
            else
            {
                CurHp -= 2;
                StageManager.Instance.ShowFloatingDamage(2, false,
                    transform.position + new Vector3(0f, _floatingDamageYPos));
            }

            if (!IsLive)
            {
                destructible.OnDied();
            }
            else
            {
                destructible.OnDamaged();
            }
        }

        public override void Knockback(float power)
        {
        }

        public override void Kill()
        {
            if (!IsLive) return;

            CurHp = 0f;
            destructible.OnDied();
        }
    }
}