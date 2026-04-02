using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using PupSurvivors.Stage;
using UnityEngine;

namespace PupSurvivors.Enemy
{
    public abstract class UncommonEnemyBase : MonoBehaviour, IKnockbackable, IDamagable
    {
        [SerializeField] private DamagableSO damagableSo;

        protected UncommonEnemyData OriData;
        protected UncommonEnemyStats OriStats;

        [field: SerializeField] public UncommonEnemyHealthSystem HealthSystem { get; private set; }
        [SerializeField] protected HitEffect hitEffect;
        [SerializeField] private VisibilityChecker visibilityChecker;

        [SerializeField] protected SpriteRenderer bodySr;
        [SerializeField] protected Animator ani;
        [SerializeField] protected Rigidbody2D rb;
        [SerializeField] protected CircleCollider2D bodyColl;

        protected enum State
        {
            Appear,
            Chase,
            Idle,
            Attack,
            Died,
        }

        protected State CurState, LastState;

        protected Vector2 MoveDir;
        protected float CurSpeed;

        public Vector3 FloatingDamageWorldPos => transform.position + OriData.floatingDamagePos;
        public Vector3 HpBarPos => new Vector3(0f, -bodyColl.radius - 0.5f);

        // ReSharper disable once InconsistentNaming
        [HideInInspector] public float AniVar_MoveSpeed = 1f;

        protected CancellationTokenSource StateCts, DestroyCts, OnDamagedCts;

        private bool _hasSeen;
        protected Action ActionOnRelocated;

        private const float MassMultiplier = 5f;


        protected virtual void Awake()
        {
            DestroyCts = new CancellationTokenSource();
            HealthSystem.Initialize(this, this);
            damagableSo.CollDict.Add(bodyColl, HealthSystem);
        }

        protected virtual void OnDestroy()
        {
            StateCts?.Cancel();
            OnDamagedCts?.Cancel();
            DestroyCts.Cancel();
            DestroyCts.Dispose();
        }

        public virtual void SetData(UncommonEnemyData data)
        {
            OriData = data;
            OriStats = data.stats;

            switch (data.type)
            {
                case EnemyType.MiniBoss:
                    bodySr.material.EnableKeyword("_ISMINIBOSS");
                    break;
                case EnemyType.Elite:
                    bodySr.material.DisableKeyword("_ISMINIBOSS");
                    break;
                case EnemyType.Common:
                case EnemyType.Boss:
                default:
                    throw new ArgumentOutOfRangeException();
            }

            gameObject.layer = data.mobilityType is MobilityType.Ground
                ? TaeBoMiCache.GetNameToLayer(TaeBoMiCache.LayerName.GroundEnemy)
                : TaeBoMiCache.GetNameToLayer(TaeBoMiCache.LayerName.AirEnemy);

            if (data.aniController)
            {
                ani.runtimeAnimatorController = data.aniController;
            }

            rb.mass = OriStats.mass * MassMultiplier;

            HealthSystem.SetMaxHp(OriStats.hp);

            CheckLocation().Forget();
            ChangeState(State.Appear);
        }

        // 기본적인 방향 설정 (가까운 플레이어 최단경로)

        protected async UniTaskVoid DefaultUpdateDirection(CancellationToken ct)
        {
            var pathFindingSystem = StageManager.Instance.PathFinder;
            while (ct.IsCancellationRequested is false)
            {
                var flowField = pathFindingSystem.GetNearestFlowField(transform.position);
                var dir = flowField.GetDir(transform.position, OriData.mobilityType);

                if (dir != Vector2.zero)
                {
                    SetDir(dir);
                }

                await UniTask.Delay(TimeSpan.FromSeconds(0.25), cancellationToken: ct);
            }
        }

        // 멈춘 상태에서의 이동 제어

        protected async UniTaskVoid DefaultUpdateMovementStop(CancellationToken ct)
        {
            while (ct.IsCancellationRequested is false)
            {
                var maxSpeed = 0f;
                if (CurSpeed < maxSpeed)
                {
                    CurSpeed += OriStats.acceleration * Time.deltaTime;
                }

                if (CurSpeed > maxSpeed)
                {
                    CurSpeed = maxSpeed;
                }

                rb.linearVelocity = MoveDir * CurSpeed;
                await UniTask.Yield(ct);
            }
        }

        // 움직이는 상태에서의 이동 제어

        protected async UniTaskVoid DefaultUpdateMovementMove(CancellationToken ct)
        {
            while (ct.IsCancellationRequested is false)
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


                await UniTask.Yield(ct);
            }
        }


        protected void ChangeState(State state)
        {
            LastState = CurState;
            CurState = state;

            StateCts?.CancelAndDispose();
            switch (LastState)
            {
                case State.Appear:
                    AppearExit();
                    break;
                case State.Chase:
                    ChaseExit();
                    break;
                case State.Idle:
                    IdleExit();
                    break;
                case State.Attack:
                    AttackExit();
                    break;
                case State.Died:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            StateCts = new CancellationTokenSource();
            switch (state)
            {
                case State.Appear:
                    AppearEnter().Forget();
                    break;
                case State.Chase:
                    ChaseEnter();
                    break;
                case State.Idle:
                    IdleEnter();
                    break;
                case State.Attack:
                    AttackEnter();
                    break;
                case State.Died:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        private async UniTaskVoid AppearEnter()
        {
            DefaultUpdateDirection(StateCts.Token).Forget();
            DefaultUpdateMovementMove(StateCts.Token).Forget();
            await UniTask.WaitUntil(() => _hasSeen, cancellationToken: DestroyCts.Token);
            ChangeState(State.Chase);
        }

        protected abstract void AppearExit();

        protected abstract void ChaseEnter();

        protected abstract void ChaseExit();

        protected abstract void IdleEnter();

        protected abstract void IdleExit();

        protected abstract void AttackEnter();

        protected abstract void AttackExit();

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

        public virtual void OnVisibleChanged(bool isVisible)
        {
            if (isVisible)
            {
                if (!_hasSeen)
                {
                    _hasSeen = true;
                }

                damagableSo.HashSet.Add(HealthSystem);
            }
            else
            {
                damagableSo.HashSet.Remove(HealthSystem);
            }
        }

        private async UniTaskVoid CheckLocation()
        {
            var moveDirTracker = StageManager.Instance.MovementTracker;
            while (DestroyCts.IsCancellationRequested is false)
            {
                if (moveDirTracker.IsOutside(transform.position))
                {
                    if (moveDirTracker.GetMovingDirectionRandomPos(out var randomPos, OriData.mobilityType))
                    {
                        transform.position = randomPos;
                        ActionOnRelocated?.Invoke();
                    }
                }

                await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: DestroyCts.Token);
            }
        }

        public abstract void OnDamaged();
        public abstract void OnKnockbacked(float power);

        public virtual void OnDied()
        {
            ChangeState(State.Died);
            damagableSo.HashSet.Remove(HealthSystem);
            damagableSo.CollDict.Remove(bodyColl);

            ani.SetTrigger(TaeBoMiCache.DieTrigger);

            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;

            var centerPos = transform.position + OriData.spriteCenterPos;

            switch (OriData.type)
            {
                case EnemyType.Elite:
                    StageManager.Instance.CreateExpCrystal(OriStats.addExp, centerPos);
                    StageManager.Instance.CreateNyan(OriStats.nyan, centerPos);
                    break;
                case EnemyType.MiniBoss:
                    StageManager.Instance.CreateMiniBossChest(centerPos, OriStats.ratioExp, OriStats.nyan);
                    break;
                case EnemyType.Common:
                case EnemyType.Boss:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public virtual void AniEvent_OnDieAnimationFinished()
        {
            Destroy(gameObject);
        }

        protected void DefaultOnDamaged()
        {
            OnDamagedCts?.CancelAndDispose();
            OnDamagedCts = new CancellationTokenSource();

            hitEffect.Play(OnDamagedCts.Token).Forget();
        }

        protected void DefaultOnKnockbacked(float power)
        {
            var velocity = power * MassMultiplier / rb.mass;
            CurSpeed -= velocity;
        }
    }
}