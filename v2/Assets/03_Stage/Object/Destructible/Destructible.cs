using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using PupSurvivors.Pool;
using PupSurvivors.Stage.UI;
using UnityEngine;
using UnityEngine.Serialization;

namespace PupSurvivors.Stage
{
    public class Destructible : MonoBehaviour
    {
        [FormerlySerializedAs("visibleDamagableSO")] [SerializeField] private DamagableSO damagableSo;
        
        [SerializeField] private SpriteRenderer bodySr, shadowSr;
        [field:SerializeField] public CircleCollider2D CircleColl { get; private set; }
        
        [SerializeField] private VisibilityChecker visibilityChecker;
        [SerializeField] private DestructibleHealthSystem healthSystemBase;
        [SerializeField] private HitEffect hitEffect;

        private LimitedObjectPool<Destructible> _managedPool;
        private IObjectPool<Effect> _destroyEffectPool;

        private CancellationTokenSource _damagedCts;

        private void Awake()
        {
            damagableSo.CollDict.Add(CircleColl, healthSystemBase);
            // StageManager.Instance.DamagableDict.Add(CircleColl.GetInstanceID(), healthSystemBase);
        }

        private void OnDestroy()
        {
            
            damagableSo.CollDict.Remove(CircleColl);
        }

        private void Start()
        {
            visibilityChecker.VisibleChangedEvent.AddListener(OnVisibleChanged);
        }
        
        public void Initialize(LimitedObjectPool<Destructible> pool,
            IObjectPool<Effect> destroyEffectPool)
        {
            _managedPool = pool;
            _destroyEffectPool = destroyEffectPool;
        }

        public void Set(in DestructibleDB.DestructibleData data, Vector3 pos)
        {
            bodySr.sprite = data.bodySprite;
            shadowSr.sprite = data.shadowSprite;
            CircleColl.offset = data.offset;
            CircleColl.radius = data.radius;
            
            healthSystemBase.Set(10, data.floatingDamageYPos);

            transform.position = pos;

            gameObject.SetActive(true);
        }

        public void OnDamaged()
        {
            if (_damagedCts != null)
            {
                _damagedCts.Cancel();
                _damagedCts.Dispose();
            }
            _damagedCts = new CancellationTokenSource();
            hitEffect.Play(_damagedCts.Token).Forget();
        }

        public void OnDied()
        {
            gameObject.SetActive(false);
            _managedPool.Push(this);

            var curPos = transform.position;
            var effect = _destroyEffectPool.Get();
            effect.transform.position = curPos + (Vector3)CircleColl.offset;
            effect.gameObject.SetActive(true);
            StageManager.Instance.CreateRandomItem(curPos);
        }


        public void OnVisibleChanged(bool isVisible)
        {
            if (isVisible)
            {
                damagableSo.HashSet.Add(healthSystemBase);
                // StageManager.Instance.VisibleDamagableSet.Add(healthSystemBase);
            }
            else
            {
                damagableSo.HashSet.Remove(healthSystemBase);
                // StageManager.Instance.VisibleDamagableSet.Remove(healthSystemBase);
            }
        }
    }
}