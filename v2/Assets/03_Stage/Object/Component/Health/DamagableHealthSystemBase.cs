using UnityEngine;

namespace PupSurvivors.Stage
{
    public abstract class DamagableHealthSystemBase : MonoBehaviour
    {
        public float CurHp { get; protected set; }
        public bool IsLive => CurHp > 0f;

        public abstract void Damage((float, bool) damage);
        public abstract void Knockback(float power);
        public abstract void Kill();

        protected IDamagable Damagable;
        protected IKnockbackable Knockbackable;

        public void Initialize(IDamagable damagable, IKnockbackable knockbackable = null)
        {
            Damagable = damagable;
            Knockbackable = knockbackable;
        }
    }
}