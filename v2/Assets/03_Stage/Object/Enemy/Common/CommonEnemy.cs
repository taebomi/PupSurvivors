using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using PupSurvivors.Pool;
using PupSurvivors.Stage;
using PupSurvivors.Stage.UI;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace PupSurvivors.Enemy
{
    public class CommonEnemy : MonoBehaviour, IDamagable, IKnockbackable
    {
        [SerializeField] public DamagableSO damagableSo;
        [SerializeField] public AudioEventChannelSO seEventChannelSO;
        
        [SerializeField] protected SpriteRenderer bodySr;
        [SerializeField] protected Transform shadowTr;
        [SerializeField] protected Animator ani;
        [SerializeField] protected Rigidbody2D rb;
        [field: SerializeField] public CircleCollider2D BodyColl { get; private set; }
        
        [SerializeField] protected CommonEnemyHealthSystem healthSystem;
        [SerializeField] protected HitEffect hitEffect;

        public CommonEnemyData OriData { get; protected set; }
        public EnemyStats OriStats { get; protected set; }

        protected MobilityType MobilityType;
        protected Vector2 MoveDir;
        protected float CurSpeed;

        protected CancellationTokenSource DisableCts;
        protected CancellationTokenSource OnDamagedCts;
        protected CancellationTokenSource DespawnCts;

        // ReSharper disable once InconsistentNaming 애니메이션 전용 변수
        [HideInInspector] public float AniVar_MoveSpeed = 1f;

        private IObjectPool<CommonEnemy> _managedPool;

        #region 이벤트 함수

        protected void Awake()
        {
            healthSystem.Initialize(this, this);
            damagableSo.CollDict.Add(BodyColl, healthSystem);
        }

        private void OnDestroy()
        {
            damagableSo.CollDict.Remove(BodyColl);
        }

        private void Update()
        {
            var maxSpeed = OriStats.speed;
            if (CurSpeed < maxSpeed)
            {
                CurSpeed += OriStats.acceleration * Time.deltaTime;
            }

            if (CurSpeed > maxSpeed)
            {
                CurSpeed = maxSpeed;
            }

            if (CurSpeed < 0f)
            {
                rb.linearVelocity = MoveDir * CurSpeed;
            }
            else
            {
                rb.linearVelocity = MoveDir * (CurSpeed * AniVar_MoveSpeed);
            }
        }

        private void OnDisable()
        {
            DisableCts.Cancel();
            OnDamagedCts?.Cancel();
            DespawnCts?.Cancel();
        }

        #endregion

        #region 외부

        public void AniEvent_OnDieAnimationFinished()
        {
            gameObject.SetActive(false);
            _managedPool.Push(this);
        }

        public void OnVisibleChanged(bool isVisible)
        {
            if (isVisible)
            {
                damagableSo.HashSet.Add(healthSystem);
                DespawnCts?.Cancel();
            }
            else
            {
                damagableSo.HashSet.Remove(healthSystem);
                StartDespawnTimer().Forget();
            }
        }

        public Vector3 FloatingDamageWorldPos => transform.position + _floatingDamageLocalPos;
        private Vector3 _floatingDamageLocalPos;

        public void OnDamaged()
        {
            OnDamagedCts?.CancelAndDispose();
            OnDamagedCts = new CancellationTokenSource();
            seEventChannelSO.RaiseEvent(OriData.hitSE[Random.Range(0,OriData.hitSE.Length)]);
            
            hitEffect.Play(OnDamagedCts.Token).Forget();
        }

        public void OnKnockbacked(float power)
        {
            var velocity = power / rb.mass;
            CurSpeed -= velocity;
        }

        public void OnDied()
        {
            seEventChannelSO.RaiseEvent(OriData.hitSE[Random.Range(0,OriData.hitSE.Length)]);
            damagableSo.HashSet.Remove(healthSystem);
            StageManager.Instance.CreateExpCrystal(OriStats.exp, transform.position);
            ani.SetTrigger(TaeBoMiCache.DieTrigger);
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        #endregion

        public void Initialize(IObjectPool<CommonEnemy> pool)
        {
            _managedPool = pool;
        }

        public void Set(CommonEnemyData commonEnemyData, Vector2 pos)
        {
            DisableCts?.Dispose();
            DisableCts = new CancellationTokenSource();

            OriData = commonEnemyData;
            OriStats = commonEnemyData.stats;

            
            //todo 적 별로 위치 세팅
            _floatingDamageLocalPos = new Vector3(0f,commonEnemyData.collRadius + FloatingDamage.Spacing);
            healthSystem.SetHp(OriStats.hp);

            gameObject.layer = commonEnemyData.mobilityType == MobilityType.Ground
                ? TaeBoMiCache.GetNameToLayer(TaeBoMiCache.LayerName.GroundEnemy)
                : TaeBoMiCache.GetNameToLayer(TaeBoMiCache.LayerName.AirEnemy);

            bodySr.color = Color.white;
            ani.runtimeAnimatorController = commonEnemyData.aniController;

            MobilityType = commonEnemyData.mobilityType;

            BodyColl.radius = commonEnemyData.collRadius;
            rb.mass = commonEnemyData.mass;
            rb.simulated = true;

            shadowTr.localPosition = bodySr.transform.localPosition = new Vector3(0f, commonEnemyData.spriteYPos);


            AniVar_MoveSpeed = 1f;

            transform.position = pos;
            gameObject.SetActive(true);
            StartDespawnTimer().Forget();
        }

        private async UniTaskVoid StartDespawnTimer()
        {
            DespawnCts?.CancelAndDispose();
            DespawnCts = new CancellationTokenSource();
            await UniTask.Delay(TimeSpan.FromSeconds(5), cancellationToken: DespawnCts.Token);
            gameObject.SetActive(false);
            _managedPool.Push(this);
        }

        public async UniTaskVoid ChasePlayer()
        {
            // var pathFindingSystem = StageManager.Instance.PathFindingSystem;
            // while (DisableCts.IsCancellationRequested is false)
            // {
            //     var dir = pathFindingSystem.GetDirection(transform.position,
            //         MobilityType is MobilityType.Ground);
            //
            //     if (dir != Vector2.zero)
            //     {
            //         SetDir(dir);
            //     }
            //
            //     await UniTask.Delay(250, cancellationToken: DisableCts.Token);
            // }
        }

        public void SetDir(Vector2 dir)
        {
            MoveDir = dir.normalized;
            switch (MoveDir.x)
            {
                case > 0:
                    ani.SetBool(TaeBoMiCache.IsRight, true);
                    break;
                case < 0:
                    ani.SetBool(TaeBoMiCache.IsRight, false);
                    break;
            }
        }
    }
}