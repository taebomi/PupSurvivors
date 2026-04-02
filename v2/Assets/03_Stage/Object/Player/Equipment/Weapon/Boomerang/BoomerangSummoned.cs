using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PupSurvivors.Equipment
{
    public class BoomerangSummoned : WeaponSummonedBase<BoomerangSummoned, Boomerang>
    {
        [SerializeField] private DamagableSO damagableSO;
        
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private Transform bodyTr;
        [SerializeField] private SpriteRenderer bodySr;
        [SerializeField] private SpriteContainer bodySprites;
        [SerializeField] private Collider2D bodyColl;
        [SerializeField] private AudioSource audioSource;

        [SerializeField] private AudioClip throwSE, returnSE, catchSE;
        [SerializeField] private AudioClip[] hitSE;

        private enum State
        {
            Throw,
            Return
        }

        private State _state;

        private (float, bool) _curDamage;

        private float _maxRange;

        private const float RotateSpeed = 1540f;


        protected override void InitAfter()
        {
        }

        protected override void OnStatsUpdated()
        {
            _maxRange = CurStats.floatOptionDict[WeaponStats.FloatOption.MaxRange];
            transform.localScale = Vector3.one * CurStats.size;
        }

        private void Update()
        {
            bodyTr.Rotate(0f, 0f, RotateSpeed * Time.deltaTime);
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            // audioSource.PlayOneShot(hitSE[Random.Range(0, hitSE.Length)]);
            if (!damagableSO.CollDict.TryGetValue(col, out var damagable))
            {
#if UNITY_EDITOR
                Debug.Log($"{transform.name} - {col.name}이 EnemyColliderDict에 존재하지 않음.");
#endif
                return;
            }

            damagable.Damage(_curDamage);
            switch (_state)
            {
                case State.Throw:
                    damagable.Knockback(CurStats.knockbackPower * 1f);
                    break;
                case State.Return:
                    damagable.Knockback(CurStats.knockbackPower * 0.25f);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public async UniTaskVoid Throw(Vector2 dir)
        {
            // 날아가기
            _state = State.Throw;
            audioSource.clip = throwSE;
            audioSource.Play();
            bodyColl.enabled = false;
            bodyColl.enabled = true;
            _curDamage = Weapon.GetRandomDamage();
            bodySr.sprite = bodySprites.data[0];
            

            dir.Normalize();

            var a = CurStats.speed * CurStats.speed / (_maxRange * 2);
            var firstSpeed = CurStats.speed;
            var duration = CurStats.speed / a;
            var timer = 0f;
            while (timer < duration && DisableCts.IsCancellationRequested is false)
            {
                var speed = Mathf.LerpUnclamped(firstSpeed, 0f, timer / duration);
                rb.linearVelocity = dir * speed;
                timer += Time.fixedDeltaTime;
                await UniTask.Yield(PlayerLoopTiming.FixedUpdate, DisableCts.Token);
            }

            Return().Forget();
            CheckPlayerCatch().Forget();
        }
        

        private async UniTaskVoid Return()
        {
            _state = State.Return;
            audioSource.clip = returnSE;
            audioSource.Play();
            bodyColl.enabled = false;
            bodyColl.enabled = true;
            _curDamage = (_curDamage.Item1 * 4, _curDamage.Item2);
            bodySr.sprite = bodySprites.data[1];
            
            var playerTr = Weapon.Player.transform;
            
            var a = CurStats.speed * CurStats.speed * 1.5f * 1.5f / (_maxRange * 2);
            var returnSpeed = CurStats.speed * 1.5f;
            var duration = returnSpeed / a;
            var timer = 0f;
            while (_state is State.Return && DisableCts.IsCancellationRequested is false)
            {
                Vector2 returnDir = playerTr.position - transform.position;
                returnDir.Normalize();
                var speed = timer < duration
                    ? Mathf.LerpUnclamped(0f, returnSpeed, timer / duration)
                    : returnSpeed;
                rb.linearVelocity = returnDir * speed;
                timer += Time.fixedDeltaTime;
                await UniTask.Yield(PlayerLoopTiming.FixedUpdate, DisableCts.Token);
            }
        }

        private async UniTaskVoid CheckPlayerCatch()
        {
            var playerTr = Weapon.Player.transform;
            while (_state is State.Return && DisableCts.IsCancellationRequested is false)
            {
                var sqrDist = (playerTr.position - transform.position).sqrMagnitude;
                const float catchSqrDist = 1f;
                if (sqrDist < catchSqrDist)
                {
                    Catch();
                    return;
                }

                await UniTask.Yield(DisableCts.Token);
            }
        }

        private void Catch()
        {
            audioSource.PlayOneShot(catchSE);
            Weapon.ThrowBoomerang();
        }
    }
}