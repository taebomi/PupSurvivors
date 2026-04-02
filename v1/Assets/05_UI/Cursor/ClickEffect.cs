using System.Collections;
using System.Collections.Generic;
using PupSurvivors.ObjectPool;
using UnityEngine;

public class ClickEffect : LimitedPoolableObject<ClickEffect>
{
    public void AnimationEvent_PushToPool()
    {
        gameObject.SetActive(false);
        ManagedPool.Push(this);
    }
}
