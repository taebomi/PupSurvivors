using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using PupSurvivors.Enemy;
using PupSurvivors.Equipment;
using UnityEngine;

public class DivineFootprintsSummoned : SummonedWeaponBase<DivineFootprintsSummoned, DivineFootprints>
{
    public override void InitializeAfter()
    {
        transform.SetParent(Weapon.transform);
        Damage().Forget();
    }

    private async UniTaskVoid Damage()
    {
        var timer = 0f;
        while (true)
        {
            while (timer > ModifiedStats.interval)
            {
                timer -= ModifiedStats.interval;
                Physics2D.OverlapCircle(transform.position, ModifiedStats.size, EnemyManager.Instance.ContactFilter2D,
                    TaeBoMiCache.TempColliderList);
                DoDamage(TaeBoMiCache.TempColliderList);
            }

            timer += Time.deltaTime;
            await UniTask.Yield(DisableCts.Token);
        }
    }

    public override void Attack()
    {
        var footPrints = Weapon.ProjectilePool.Pop();
        footPrints.SetPosition(transform.position);
    }

    public override void UpdateStats()
    {
    }
}