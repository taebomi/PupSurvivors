using System;
using Cysharp.Threading.Tasks;
using PupSurvivors.Enemy;
using UnityEngine;
using Random = UnityEngine.Random;

// 미니보스와 네임드의 차이점
// 미니보스 - 스테이지 진행 시간에 따른

public class GoblinSlinger : UncommonEnemyBase
{
    private int _curAttackCount;
    private int _attackCount = 1;
    // todo 공격 타이머 발생
    public override void OnDamaged()
    {
        throw new NotImplementedException();
    }

    public override void OnKnockbacked(float power)
    {
        throw new NotImplementedException();
    }

    protected override void AppearExit()
    {
    }


    // 쿨타임 동안 쫓기
    protected override void ChaseEnter()
    {
        ani.SetBool(TaeBoMiCache.IsMoving, true);
        DefaultUpdateDirection(StateCts.Token).Forget();
        DefaultUpdateMovementMove(StateCts.Token).Forget();
        Chase().Forget();
    }

    private async UniTaskVoid Chase()
    {
        // await UniTask.Delay(
        //     TimeSpan.FromSeconds(Random.Range(oriStats.attackCooldown * 0.5f, oriStats.attackCooldown * 2.5f))
        //     , cancellationToken: DestroyCts.Token);
        ChangeState(State.Attack);
    }

    protected override void ChaseExit()
    {
        ani.SetBool(TaeBoMiCache.IsMoving, false);
    }

    protected override void IdleEnter()
    {
        rb.linearVelocity = Vector2.zero;
        _attackCount++;
        Idle().Forget();
    }

    private async UniTaskVoid Idle()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(Random.Range(2f, 4f)));
        ChangeState(State.Chase);
    }

    protected override void IdleExit()
    {
    }

    protected override void AttackEnter()
    {
        rb.linearVelocity = Vector2.zero;
        _curAttackCount = _attackCount;
        ani.SetInteger(TaeBoMiCache.AttackCount, _curAttackCount);
    }

    protected override void AttackExit()
    {
    }


    // 처음 생성 (화면 바깥)
    // 플레이어를 향해 이동 (가장 가까운 플레이어 )
    // 화면 밖 타이머 실행

    // 화면 바깥으로 나갔을 경우 타이머 작동, 화면 내에 보일 시 타이머 취소
    //      타이머 완료 시 - 카메라가 이동중인 방향으로 재배치

    // 처음 화면 내로 들어온 이후부터 랜덤 주기로 투사체 공격하기
    // 이후에는 다가오며 랜덤 딜레이로 투사체 공격


    public void AniEvent_OnAttackFinished()
    {
        _curAttackCount--;
        ani.SetInteger(TaeBoMiCache.AttackCount, _curAttackCount);
        //todo 발사체 생성
        if (_curAttackCount <= 0)
        {
            if (Random.value < 0.3f)
            {
                ChangeState(State.Chase);
            }
            else
            {
                ChangeState(State.Idle);
            }
        }
    }
}