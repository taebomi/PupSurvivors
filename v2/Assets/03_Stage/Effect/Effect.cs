using PupSurvivors.Pool;
using UnityEngine;

namespace PupSurvivors.Stage
{
    public class Effect : MonoBehaviour
    {
        private IObjectPool<Effect> _pool;

        public void SetPool(IObjectPool<Effect> pool)
        {
            _pool = pool;
        }

        public void AnimationEvent_OnEffectFinished()
        {
            _pool.Push(this);
            gameObject.SetActive(false);
        }
    }
}