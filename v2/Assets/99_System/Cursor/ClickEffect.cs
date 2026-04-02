using PupSurvivors.Pool;
using UnityEngine;

public class ClickEffect : MonoBehaviour
{
    private IObjectPool<ClickEffect> _pool;
    
    public void Initialize(IObjectPool<ClickEffect> pool)
    {
        _pool = pool;
    }
    public void AnimationEvent_PushToPool()
    {
        gameObject.SetActive(false);
        _pool.Push(this);
    }
}
