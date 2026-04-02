using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using PupSurvivors.Enemy;
using PupSurvivors.Equipment;
using PupSurvivors.Stage;
using PupSurvivors.Weapon;
using UnityEngine;
using Random = UnityEngine.Random;

public class MeteorProjectile : WeaponProjectileBase<MeteorProjectile>
{
    [SerializeField] private SpriteRenderer meteorSr;
    [SerializeField] private ParticleSystem meteorPc;
    [SerializeField] private SpriteRenderer shadowSr;
    [SerializeField] private ParticleSystem explosionParticleSystem;
    [SerializeField] private ParticleSystem fireParticleSystem;

    private const float FallingDuration = 0.5f;
    private const float FallingDistance = CameraManager.CameraHalfHeight * 2 + 5f;
    private const float TickInterval = 0.5f;
    private const float FireSize = 2f;

    private float _currentRadius;


    protected override void OnEnable()
    {
        base.OnEnable();
        Attack().Forget();
    }

    public override MeteorProjectile SetPosition(Vector3 destPos)
    {
        transform.position = destPos;
        var randomXPos = Random.Range(-7.5f, 7.5f);
        var meteorTr = meteorSr.transform;
        meteorTr.position = destPos + new Vector3(randomXPos, FallingDistance);
        shadowSr.transform.position = destPos + new Vector3(randomXPos, Random.Range(-3f, 3f));
        meteorTr.right = destPos - meteorTr.position;
        return this;
    }

    private async UniTaskVoid Attack()
    {
        await Fall(); // 낙하
        Explosion(); // 폭파
        await Fire(); // 장판
        await UniTask.Delay(1000);
        ManagedPool.Push(this);
    }

    private async UniTask Fall()
    {
        meteorSr.enabled = true;
        shadowSr.enabled = true;
        meteorSr.transform.DOLocalMove(Vector3.zero, FallingDuration).Play();
        shadowSr.transform.DOLocalMove(Vector3.zero, FallingDuration).Play();
        await UniTask.Delay(TimeSpan.FromSeconds(FallingDuration), cancellationToken: DisableCts.Token);
        meteorSr.enabled = false;
        shadowSr.enabled = false;
        meteorPc.Stop();

        // while (timer < FallingDuration)
        // {
        //     meteorTr.position =
        //         Vector3.MoveTowards(meteorTr.position, transform.position, FallingSpeed * Time.deltaTime);
        //     timer += Time.deltaTime;
        //     await UniTask.Yield(DisableCts.Token);
        // }
    }

    private void Explosion()
    {
        explosionParticleSystem.Play(true);
        Physics2D.OverlapCircle(transform.position, ModifiedStats.size, EnemyManager.Instance.ContactFilter2D,
            TaeBoMiCache.TempColliderList);
        DoDamage(TaeBoMiCache.TempColliderList);
    }

    private async UniTask Fire()
    {
        fireParticleSystem.Play(true);
        var timer = 0f;
        var tick = 0f;
        while (timer < ModifiedStats.duration)
        {
            if (tick > TickInterval)
            {
                Physics2D.OverlapCircle(transform.position, ModifiedStats.size * FireSize,
                    EnemyManager.Instance.ContactFilter2D, TaeBoMiCache.TempColliderList);
                while (tick > TickInterval)
                {
                    tick -= TickInterval;
                    DoDamage(TaeBoMiCache.TempColliderList);
                }
            }

            tick += Time.deltaTime;
            timer += Time.deltaTime;
            await UniTask.Yield(DisableCts.Token);
        }
        fireParticleSystem.Stop(true);
    }

    public override void OnStatsChanged()
    {
        transform.localScale = Vector3.one * ModifiedStats.size;
    }

}