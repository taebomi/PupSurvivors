using UnityEngine;

public class EmoticonBubble : PoolableObject<EmoticonBubble>
{
    [SerializeField] private Animator animator;

    public enum EmoticonType
    {
        Dialogue = 0,
    }

    public void Set(EmoticonType emoticonType, Transform container)
    {
        transform.SetParent(container, false);
        animator.SetInteger(TaeBoMiCache.Type, (int)emoticonType);
        // todo !사운드 - 추가
    }

    public void Remove()
    {
        animator.SetInteger(TaeBoMiCache.Type, -1);
    }

    public void AnimationEvent_ReleaseToPool()
    {
        ManagedPool.Release(this);
    }
}
