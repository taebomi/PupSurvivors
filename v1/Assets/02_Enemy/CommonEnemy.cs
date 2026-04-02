using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using PupSurvivors.Enemy;
using UnityEngine;
using UnityEngine.Pool;

namespace PupSurvivors.Enemy
{
    public class CommonEnemy : EnemyBase
    {
        private IObjectPool<CommonEnemy> _managedPool;

        private CancellationTokenSource _despawnCts;
        private const float DespawnTime = 5f;


        protected override void InitializeAfter()
        {
            base.InitializeAfter();
            StartDespawnTimer().Forget();
        }

        public void SetManagedPool(IObjectPool<CommonEnemy> pool)
        {
            _managedPool = pool;
        }

        public override void OnDie()
        {
            base.OnDie();
            ani.SetTrigger(TaeBoMiCache.DieTrigger);
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        public override void AnimationEvent_DieAnimationFinished()
        {
            gameObject.SetActive(false);
            _managedPool.Release(this);
        }


        protected void OnBecameVisible()
        {
            StopDespawnTimer();
        }

        protected void OnBecameInvisible()
        {
            StartDespawnTimer().Forget();
        }

        private void StopDespawnTimer()
        {
            _despawnCts.Cancel();
        }

        private async UniTaskVoid StartDespawnTimer()
        {
            _despawnCts?.CancelAndDispose();
            _despawnCts = new CancellationTokenSource();
            var cts = CancellationTokenSource.CreateLinkedTokenSource(_despawnCts.Token, DisableCts.Token);
            await UniTask.Delay(TimeSpan.FromSeconds(DespawnTime), cancellationToken: cts.Token);
            gameObject.SetActive(false);
            _managedPool.Release(this);
        }

        #region IDamagable

        public override void Damage(float damage, bool isCritical)
        {
            HealthSystem.Damage(damage, isCritical);
        }

        public override void Knockback(float power)
        {
         HealthSystem.Knockback(power);
        }

        public override void Kill()
        {
        }

        #endregion
    }
}