using System;
using Cysharp.Threading.Tasks;
using PupSurvivors.Stage;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PupSurvivors.Equipment
{
    public class SwordSummoned : WeaponSummonedBase<SwordSummoned, Sword>
    {
        // ReSharper disable once InconsistentNaming
        [SerializeField] private DamagableSO damagableSO;
        [SerializeField] private AudioEventChannelSO seEventChannelSO;

        [SerializeField] private Transform bodyTr;
        [SerializeField] private TrailRenderer trailRenderer;

        [SerializeField] private AudioClip[] slashSE;

        private enum State
        {
            Idle,
            Wait,
            Attack,
        }

        private State _state;
        private float _lastDuration;
        private DamagableHealthSystemBase _lastDamagable;

        private float _cooldownTimer;

        private float _maxRange, _moveDuration, _rotateDuration;


        protected override void InitAfter()
        {
            _lastDuration = 0f;
            _cooldownTimer = 0f;
            _state = State.Idle;
            Weapon.ActionOnLevelUp = () => _cooldownTimer = CurStats.cooldown;
            Weapon.Player.Follower.AddFollower(transform);
            trailRenderer.emitting = false;
            CheckCanAttack().Forget();
            Flow().Forget();
        }

        private void OnDestroy()
        {
            if (_state is State.Idle)
            {
                Weapon.Player.Follower.RemoveFollower(transform);
            }

            Weapon.ActionOnLevelUp = null;
        }

        protected override void OnStatsUpdated()
        {
            _maxRange = CurStats.floatOptionDict[WeaponStats.FloatOption.MaxRange];
            _moveDuration = CurStats.floatOptionDict[WeaponStats.FloatOption.AttackDelay];
            _rotateDuration = CurStats.floatOptionDict[WeaponStats.FloatOption.AttackDuration];
            transform.localScale = Vector3.one * CurStats.size;
        }

        private void ChangeState(State state)
        {
            _state = state;
            switch (state)
            {
                case State.Idle:
                    Weapon.Player.Follower.AddFollower(transform, false);
                    trailRenderer.emitting = false;
                    CheckCanAttack().Forget();
                    Flow().Forget();
                    break;
                case State.Wait:
                    break;
                case State.Attack:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        private async UniTaskVoid Flow()
        {
            bodyTr.rotation = Quaternion.Euler(0, 0, -225f);
            const float halfFlowingHeight = 0.25f;
            while (_state is State.Idle && DisableCts.IsCancellationRequested is false)
            {
                var currentHeight = halfFlowingHeight * Mathf.Sin(Time.time * 3f);
                bodyTr.localPosition = new Vector3(0f, currentHeight);
                await UniTask.Yield(DisableCts.Token);
            }
        }

        private async UniTaskVoid CheckCanAttack()
        {
            var cooldown = (1 - _lastDuration / CurStats.duration) * CurStats.cooldown;
            _cooldownTimer = 0f;
            while (_cooldownTimer < cooldown && DisableCts.IsCancellationRequested is false)
            {
                _cooldownTimer += Time.deltaTime;
                await UniTask.Yield(DisableCts.Token);
            }

            var nearestDamagableDist = Weapon.Player.EnemyFinder.NearestDamagableDist;
            while (nearestDamagableDist > _maxRange && DisableCts.IsCancellationRequested is false)
            {
                nearestDamagableDist = Weapon.Player.EnemyFinder.NearestDamagableDist;
                await UniTask.Yield(DisableCts.Token);
            }

            Attack().Forget();
        }

        private async UniTaskVoid Attack()
        {
            trailRenderer.emitting = true;
            Weapon.Player.Follower.RemoveFollower(transform);

            var timer = Time.time;
            var destTime = timer + CurStats.duration;
            while (timer < destTime)
            {
                if (_state is not State.Attack)
                {
                    _lastDamagable = Weapon.Player.EnemyFinder.NearestDamagable;
                    if (_lastDamagable)
                    {
                        await Slash(_lastDamagable);
                    }
                    else
                    {
                        _lastDuration = Mathf.Clamp(destTime - timer, 0f, CurStats.duration);
                        ChangeState(State.Idle);
                        break;
                    }
                }

                timer = Time.time;
                await UniTask.Yield(DisableCts.Token);
            }

            _lastDuration = 0f;
            ChangeState(State.Idle);
        }

        private async UniTask Slash(Component damagable)
        {
            _state = State.Attack;

            seEventChannelSO.RaiseEvent(slashSE[Random.Range(0, slashSE.Length)]);

            var startPos = transform.position;
            var moveDir = (damagable.transform.position - startPos);
            var destPos = startPos + moveDir +
                          moveDir.normalized * _maxRange;

            var startDir = bodyTr.up;
            var destDir = Quaternion.Euler(0, 0, -45) * moveDir;

            await RotateToDest();
            await MoveToDest();
            DamageAllFromStartToDest();

            _state = State.Wait;
            return;

            async UniTask RotateToDest()
            {
                var timer = 0f;
                while (timer < _rotateDuration)
                {
                    bodyTr.up = Vector3.Lerp(startDir, destDir, timer / _rotateDuration);
                    timer += Time.deltaTime;
                    await UniTask.Yield(DisableCts.Token);
                }
            }

            async UniTask MoveToDest()
            {
                var timer = 0f;
                while (timer < _moveDuration)
                {
                    transform.position = Vector3.Lerp(startPos, destPos, timer / _moveDuration);
                    timer += Time.deltaTime;
                    await UniTask.Yield(DisableCts.Token);
                }

                transform.position = destPos;
            }

            void DamageAllFromStartToDest()
            {
                var damage = Weapon.GetRandomDamage();
                var num = Physics2D.CircleCast(startPos, CurStats.size * 0.5f, moveDir,
                    TaeBoMiCache.DamagableFilter, TaeBoMiCache.RaycastHitArr, moveDir.magnitude + _maxRange);
   
                for (var i = 0; i < num; i++)
                {
                    if (damagableSO.CollDict.TryGetValue(TaeBoMiCache.RaycastHitArr[i].collider, out var healthSystem))
                    {
                        healthSystem.Damage(damage);
                        healthSystem.Knockback(CurStats.knockbackPower);
                    }
                    else
                    {
                        Debug.LogWarning($"{TaeBoMiCache.RaycastHitArr[i].transform.name} - damagable Dict에 존재하지 않음.");
                    }
                }
            }
        }
    }
}