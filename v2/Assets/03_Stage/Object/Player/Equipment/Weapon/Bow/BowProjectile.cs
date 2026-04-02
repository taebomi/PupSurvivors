using UnityEngine;

namespace PupSurvivors.Equipment
{
    public class BowProjectile : WeaponProjectileBase<BowProjectile>
    {
        [SerializeField] private Rigidbody2D rb;

        public override void OnStatsChanged()
        {
            transform.localScale = Vector3.one * CurStats.size;
        }

        private void OnBecameInvisible()
        {
            Pool.Push(this);
        }

        public void Set(Vector3 pos, Vector2 dir)
        {
            var tr = transform;
            tr.position = pos;
            tr.right = dir;
            rb.linearVelocity = dir * CurStats.speed;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            DefaultAttack(other);
            RemainedPierce--;
            if (RemainedPierce <= 0)
            {
                Pool.Push(this);
            }
        }
    }
}