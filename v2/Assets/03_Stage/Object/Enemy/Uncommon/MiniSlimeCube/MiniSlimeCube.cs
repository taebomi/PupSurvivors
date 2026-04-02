using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using FloatOption = PupSurvivors.Enemy.UncommonEnemyStats.FloatOption;

namespace PupSurvivors.Enemy
{
    public class MiniSlimeCube : UncommonEnemyBase
    {
        [SerializeField] private GameObject stunEffect;

        private float _phase1Duration,
            _phase1CurDuration,
            _phase1AddDuration,
            _phase1MaxDuration,
            _phase2Duration,
            _attackCooldown,
            _attackHpRatioCondition,
            _groggyDuration,
            _attackChargingDuration,
            _phase1Speed;

        [SerializeField] private 독장판Data 독장판Data;
        private 독장판생성기 _독장판생성기;
        
        private TaeBoMiCache.Direction _lastScreenExitDir;

        public override void OnDamaged()
        {
            DefaultOnDamaged();
        }

        public override void OnKnockbacked(float power)
        {
            if (CurState is not State.Attack)
            {
                DefaultOnKnockbacked(power);
            }
        }

        public override void OnDied()
        {
            base.OnDied();
            stunEffect.SetActive(false);
            
            //todo 아이템 생성
        }

        protected override void Awake()
        {
            base.Awake();

            ActionOnRelocated += ResetExitDirection;


        }

        public override void SetData(UncommonEnemyData data)
        {
            base.SetData(data);
            
            var floatOptionDict = data.stats.floatOptionDict;
            _phase1AddDuration = floatOptionDict[FloatOption.AttackDurationChange];
            _phase1Duration = floatOptionDict[FloatOption.AttackDuration1];
            _phase1CurDuration = _phase1Duration - _phase1AddDuration;
            _phase1MaxDuration = floatOptionDict[FloatOption.AttackMaxDuration];
            _phase1Speed = floatOptionDict[FloatOption.MaxSpeed];
            _phase2Duration = floatOptionDict[FloatOption.AttackDuration2];

            _attackCooldown = floatOptionDict[FloatOption.AttackCooldown];
            _attackHpRatioCondition = floatOptionDict[FloatOption.Option1];
            _groggyDuration = floatOptionDict[FloatOption.GroggyDuration];
            _attackChargingDuration = floatOptionDict[FloatOption.ChargingDuration];
            
            
            _독장판생성기 = 독장판Data.Create독장판생성기(50);

            CreatePoison().Forget();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            ActionOnRelocated -= ResetExitDirection;
            _독장판생성기.Destroy();
        }

        protected override void AppearExit()
        {
        }

        /// <summary>
        /// 이동 거리 or 시간 간격에 따라 독 장판 생성 반복 
        /// </summary>
        private async UniTaskVoid CreatePoison()
        {
            var movedDist = 0f;
            var timer = 0f;
            while (DestroyCts.IsCancellationRequested is false)
            {
                movedDist += Mathf.Abs(CurSpeed) * Time.deltaTime; // s = vt

                const float poisonSpaces = 1f;
                const float poisonInterval = 1f;
                if (movedDist > poisonSpaces || timer > poisonInterval)
                {
                    _독장판생성기.CreatePoison(transform.position + OriData.spriteCenterPos);
                    movedDist = 0f;
                    timer = 0f;
                }
                else
                {
                    timer += Time.deltaTime;
                }

                await UniTask.Yield(DestroyCts.Token);
            }
        }

        #region 추적

        protected override void ChaseEnter()
        {
            ani.SetInteger(TaeBoMiCache.AniType, (int)AniType.Move);
            DefaultUpdateDirection(StateCts.Token).Forget();
            DefaultUpdateMovementMove(StateCts.Token).Forget();
            Chase().Forget();
        }

        private async UniTaskVoid Chase()
        {
            var firstHpRatio = HealthSystem.CurHp / OriStats.hp;
            var attackCooldown = _attackCooldown;
            var desiredHpRatio = firstHpRatio - _attackHpRatioCondition;

            var timer = 0f;
            while (StateCts.IsCancellationRequested is false)
            {
                var curHpRatio = HealthSystem.CurHp / OriStats.hp;
                if (curHpRatio < desiredHpRatio || timer > attackCooldown)
                {
                    ChangeState(State.Idle);
                    break;
                }

                const float checkInterval = 1f;
                timer += checkInterval;
                await UniTask.Delay(TimeSpan.FromSeconds(checkInterval), cancellationToken: StateCts.Token);
            }
        }

        protected override void ChaseExit()
        {
        }

        #endregion

        #region 차징 / 그로기

        protected override void IdleEnter()
        {
            CurSpeed = 0f;
            rb.linearVelocity = Vector2.zero;
            ani.SetInteger(TaeBoMiCache.AniType, (int)AniType.Idle);
            DefaultUpdateMovementStop(StateCts.Token).Forget();
            Idle().Forget();
        }

        private async UniTaskVoid Idle()
        {
            if (LastState is State.Attack) // 공격 끝난거면 딜타임 및 다시 추적
            {
                stunEffect.SetActive(true);
                await UniTask.Delay(
                    TimeSpan.FromSeconds(_groggyDuration),
                    cancellationToken: StateCts.Token);
                stunEffect.SetActive(false);
                ChangeState(State.Chase);
            }
            else // 추적 상태였으면 잠시 후 공격
            {
                await UniTask.Delay(TimeSpan.FromSeconds(_attackChargingDuration),
                    cancellationToken: StateCts.Token);
                ChangeState(State.Attack);
            }
        }

        protected override void IdleExit()
        {
        }

        #endregion

        #region 공격

        protected override void AttackEnter()
        {
            _phase1CurDuration += _phase1AddDuration;
            _phase1CurDuration = Mathf.Clamp(_phase1CurDuration, _phase1Duration, _phase1MaxDuration);
            ani.SetInteger(TaeBoMiCache.AniType, (int)AniType.Attack);
            Attack().Forget();
            CheckReflect().Forget();
        }

        /// <summary>
        /// 마지막으로 바라본 방향을 향해 돌진(1페이즈), 천천히 속도 감소(2페이즈)
        /// 벽이나 화면 가장자리에 도달 시 방향 전환
        /// 공격 횟수 증가할 수록 지속시간 증가
        /// </summary>
        private async UniTaskVoid Attack()
        {
            CurSpeed = _phase1Speed;
            // todo 가장가까운 플레이어 구하기
            // 페이즈 1 - 등속 돌진
            var timer = _phase1Duration;
            while (timer > 0f && StateCts.IsCancellationRequested is false)
            {
                RestrictDirection();
                rb.linearVelocity = CurSpeed * MoveDir;

                timer -= Time.deltaTime;
                await UniTask.Yield(StateCts.Token);
            }

            // 페이즈 2 - 서서히 정지
            timer = _phase2Duration;
            while (timer > 0f && StateCts.IsCancellationRequested is false)
            {
                RestrictDirection();
                CurSpeed = Mathf.Lerp(0f, _phase1Speed, timer / _phase2Duration);
                rb.linearVelocity = CurSpeed * MoveDir;

                timer -= Time.deltaTime;
                await UniTask.Yield(StateCts.Token);
            }

            ChangeState(State.Idle);
        }


        private async UniTaskVoid CheckReflect()
        {
            while (StateCts.IsCancellationRequested is false)
            {
                // todo - (고민) 나중에 RayCast로 변경할지 체크해보기
                var num = bodyColl.Cast(MoveDir, TaeBoMiCache.GroundFilter, TaeBoMiCache.RaycastHitArr, 0.1f);
                if (num != 0)
                {
                    var reflectDir = Vector2.Reflect(MoveDir, TaeBoMiCache.RaycastHitArr[0].normal);
                    SetDir(reflectDir);
                    rb.linearVelocity = MoveDir * CurSpeed;
                }

                await UniTask.Yield(PlayerLoopTiming.FixedUpdate, StateCts.Token);
            }
        }

        /// <summary>
        /// 화면 테두리 나갈 시 방향 전환.
        /// 다시 안으로 들어오기 전에는 전환하지 않음. ( 화면 밖에서 벽 충돌 시 또 방향 전환하는 것 방지 )
        /// </summary>
        private void RestrictDirection()
        {
            // ReSharper disable once PossibleNullReferenceException
            var worldPos = Camera.main.WorldToViewportPoint(transform.position);
            switch (worldPos.x)
            {
                case < 0f:
                {
                    if (!_lastScreenExitDir.HasFlag(TaeBoMiCache.Direction.Left) && MoveDir.x < 0f)
                    {
                        MoveDir.x *= -1f;
                        _lastScreenExitDir |= TaeBoMiCache.Direction.Left;
                    }

                    break;
                }
                case > 1f:
                {
                    if (!_lastScreenExitDir.HasFlag(TaeBoMiCache.Direction.Right) && MoveDir.x > 0f)
                    {
                        MoveDir.x *= -1f;
                        _lastScreenExitDir |= TaeBoMiCache.Direction.Right;
                    }

                    break;
                }
                // 화면 
                default:
                    _lastScreenExitDir &= ~(TaeBoMiCache.Direction.Left | TaeBoMiCache.Direction.Right);
                    break;
            }

            switch (worldPos.y)
            {
                case < 0f:
                {
                    if (!_lastScreenExitDir.HasFlag(TaeBoMiCache.Direction.Down) && MoveDir.y < 0f)
                    {
                        MoveDir.y *= -1f;
                        _lastScreenExitDir |= TaeBoMiCache.Direction.Down;
                    }

                    break;
                }
                case > 1f:
                {
                    if (!_lastScreenExitDir.HasFlag(TaeBoMiCache.Direction.Up) && MoveDir.y > 0f)
                    {
                        MoveDir.y *= -1f;
                        _lastScreenExitDir |= TaeBoMiCache.Direction.Up;
                    }

                    break;
                }
                default:
                    _lastScreenExitDir &= ~(TaeBoMiCache.Direction.Up | TaeBoMiCache.Direction.Down);
                    break;
            }
        }

        /// <summary>
        /// 위치 재조정 시 방향 초기화 
        /// </summary>
        private void ResetExitDirection() => _lastScreenExitDir = TaeBoMiCache.Direction.None;

        protected override void AttackExit()
        {
        }

        #endregion

    }
}