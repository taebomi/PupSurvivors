using System.Collections;
using System.Collections.Generic;
using PupSurvivors.Pool;
using TMPro;
using UnityEngine;

namespace PupSurvivors.Stage
{
    public class FloatingDamage : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private TMP_Text tmp;

        [SerializeField] private MaterialContainer materials;
    
        private LimitedObjectPool<FloatingDamage> _pool;

        public const float Spacing = 0.5f;
    
        public void Initialize(LimitedObjectPool<FloatingDamage> pool)
        {
            _pool = pool;
        }
    
        public void Set(int damage, bool isCritical, Vector3 pos)
        {
            transform.position = pos;
            tmp.SetIntText(damage);

            if (!isCritical)
            {
                animator.SetTrigger(TaeBoMiCache.AttackTrigger);
                tmp.fontMaterial = materials.data[0];
                tmp.fontStyle = FontStyles.Normal;
            }
            else
            {
                animator.SetTrigger(TaeBoMiCache.CriticalTrigger);
                tmp.fontMaterial = materials.data[1];
                tmp.fontStyle = FontStyles.Bold;
            }

            meshRenderer.enabled = true;
        }

        public void AnimationEvent_Finished()
        {
            meshRenderer.enabled = false;
            _pool.Push(this);
        }
    }
}