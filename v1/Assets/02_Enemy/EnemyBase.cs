using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;


namespace PupSurvivors.Enemy
{
    public abstract class EnemyBase : MonoBehaviour, IDamagable
    {
        [SerializeField] protected SpriteRenderer mainSr;
        [SerializeField] protected Animator ani;
        [SerializeField] protected Rigidbody2D rb;
        [SerializeField] protected VisibilityChecker visibilityChecker;
        [field: SerializeField] public CircleCollider2D MainCollider { get; private set; }
        [field: SerializeField] public EnemyHealthSystem HealthSystem { get; private set; }

        public EnemyStats Stats { get; protected set; }

        protected Vector2 MovementDir;
        protected float CurrentSpeed;
        [HideInInspector] public float movementSpeedAnimationMultiplier = 1f;

        public Vector3 FloatingDamagePosition { get; private set; }

        protected CancellationTokenSource DisableCts;
        protected CancellationTokenSource DamagedCts;

        public void Initialize(EnemyData enemyData)
        {
            DisableCts = new CancellationTokenSource();
            movementSpeedAnimationMultiplier = 1f;

            // 스텟 세팅
            Stats = enemyData.stats;
            HealthSystem.SetMaxHp(Stats.hp);
            switch (Stats.type)
            {
                case EnemyType.Ground:
                    gameObject.layer = TaeBoMiCache.GetNameToLayer(TaeBoMiCache.LayerName.GroundEnemy);
                    break;
                case EnemyType.Air:
                    gameObject.layer = TaeBoMiCache.GetNameToLayer(TaeBoMiCache.LayerName.AirEnemy);
                    break;
                default:
                    Debug.LogAssertion("적 레이어 추가해줘.");
                    break;
            }

            // 스프라이트 세팅
            mainSr.color = Color.white;
            ani.runtimeAnimatorController = enemyData.aniController;
            // 물리 세팅
            MainCollider.offset = new Vector2(0f, Stats.offsetY);
            MainCollider.radius = Stats.radius;
            rb.mass = Stats.mass;
            rb.simulated = true;
            FloatingDamagePosition = new Vector3(0f, MainCollider.offset.y + Stats.radius);
            InitializeAfter();
        }

        protected virtual void InitializeAfter()
        {
        }


        public void SetVisibleEnemyDetector(bool value)
        {
            if (value)
            {
                DamagableDetector.AddVisible(this);
            }
            else
            {
                DamagableDetector.RemoveInvisible(this);
                
            }
        }

        private void OnDisable()
        {
            DisableCts.CancelAndDispose();
        }

        protected void Update()
        {
            if (CurrentSpeed < Stats.speed)
            {
                CurrentSpeed += Time.deltaTime * Stats.speed * 8f;
            }
            else if (CurrentSpeed > Stats.speed)
            {
                CurrentSpeed = Stats.speed;
            }

            if (CurrentSpeed < 0f)
            {
                rb.linearVelocity = MovementDir * CurrentSpeed;
            }
            else
            {
                rb.linearVelocity = MovementDir * (CurrentSpeed * movementSpeedAnimationMultiplier);
            }
        }

        public void SetPosition(Vector2 pos)
        {
            transform.position = pos;
        }

        public void SetDirection(Vector2 dir)
        {
            MovementDir = dir.normalized;
            ani.SetBool(TaeBoMiCache.IsRight, dir.x >= 0);
        }

        public async UniTaskVoid ChasePlayer()
        {
            while (HealthSystem.IsLive)
            {
                var direction = TempPathFinder.Instance.GetDirection(
                    transform.position + new Vector3(0f, Stats.offsetY, 0f), Stats.type == EnemyType.Ground);

                if (direction != Vector2.zero)
                {
                    SetDirection(direction);
                }

                await UniTask.Delay(250, cancellationToken: DisableCts.Token);
            }
        }

        public virtual void OnDie()
        {
            SetVisibleEnemyDetector(false);
            StageManager.Instance.CreateExpObject(Stats.exp, transform.position);
        }


        public virtual void OnDamaged(float knockbackPower)
        {
            DamagedCts?.CancelAndDispose();
            DamagedCts = new CancellationTokenSource();
            var cts = CancellationTokenSource.CreateLinkedTokenSource(DamagedCts.Token, DisableCts.Token);
            if (knockbackPower != 0f)
            {
                DefaultKnockback(knockbackPower);
            }

            DefaultFlashEffect(cts).Forget();
        }

        protected async UniTaskVoid DefaultFlashEffect(CancellationTokenSource cts)
        {
            // todo - 셰이더 작성해서 컬러값으로 하얗게 만들어주도록 하기
            var timer = 0f;
            while (timer < 0.1f)
            {
                timer += Time.deltaTime;
                var value = 1 - Easing.OutSine(timer, 0.1f);
                mainSr.color = new Color(value, value, value, 1f);
                await UniTask.Yield(cts.Token);
            }

            while (timer > 0f)
            {
                timer -= Time.deltaTime;
                var value = 1 - Easing.OutSine(timer, 0.1f);
                mainSr.color = new Color(value, value, value, 1f);
                await UniTask.Yield(cts.Token);
            }

            mainSr.color = Color.white;
        }


        protected void DefaultKnockback(float knockbackPower)
        {
            var knockbackAmount = knockbackPower / rb.mass;

            var newSpeed = Stats.speed - knockbackAmount;
            if (CurrentSpeed >= newSpeed)
            {
                CurrentSpeed = newSpeed;
            }
        }

        public abstract void AnimationEvent_DieAnimationFinished();

        public abstract void Damage(float damage, bool isCritical);
        public abstract void Knockback(float power);
        public abstract void Kill();
    }
}