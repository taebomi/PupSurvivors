using PupSurvivors.ObjectPool;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace PupSurvivors.Stage
{
    public class FloatingDamage : LimitedPoolableObject<FloatingDamage>
    {
        [SerializeField] private Animator animator;
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private TextMeshPro tmp;
        [FormerlySerializedAs("fontMaterialContainer")] [SerializeField] private MaterialContainer materialContainer;

        public void Set(int damage, bool isCritical, Vector3 pos)
        {
            transform.position = pos;
            tmp.SetIntText(damage);
            if (!isCritical)
            {
                animator.SetTrigger(TaeBoMiCache.Attack);
                tmp.fontMaterial = materialContainer.materials[0];
            }
            else
            {
                animator.SetTrigger(TaeBoMiCache.Critical);
                tmp.fontMaterial = materialContainer.materials[1];
            }

            meshRenderer.enabled = true;
        }

        public void AnimationEvent_ReleaseThis()
        {
            meshRenderer.enabled = false;
            ManagedPool.Push(this);
        }
    }

// 애니메이션이 아닌 시퀀스로 사용했던 것들.
// _floatingSequence = DOTween.Sequence()
//     .Append(tmp.transform.DOLocalMoveY(0.5f, 0.25f).From(0f).SetEase(Ease.OutSine))
//     .Join(tmp.DOFontSize(5f, 0.25f).From(0f).SetEase(Ease.OutBounce))
//     .Insert(0.5f, tmp.DOFade(0f, 0.25f))
//     .SetAutoKill(false);
//
// _criticalSequence = DOTween.Sequence()
//     // 위 -> 아래 방향 이동, 큰 글자 -> 작은 글자
//     .Append(tmp.transform.DOLocalMoveY(0.5f, 0.1f).From(1f).SetEase(Ease.OutSine))
//     .Join(tmp.DOFontSize(7.5f, 0.25f).From(10f).SetEase(Ease.OutBounce))
//     // 투명해지며 사라짐
//     .Insert(0.5f, tmp.DOFade(0f, 0.25f))
//     .SetAutoKill(false);
}