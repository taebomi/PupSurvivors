using Cysharp.Threading.Tasks;
using PupSurvivors.Equipment;

public class DivineFootprints : WeaponBase
{
    private DivineFootprintsSummoned _summoned;
    public ProjectilePool<DivineFootprintsProjectile> ProjectilePool { get; private set; }

    protected override async UniTask InitializeAfter()
    {
        await SummonedWeaponBase<DivineFootprintsSummoned, DivineFootprints>.Initialize(this);
        _summoned = SummonedWeaponBase<DivineFootprintsSummoned, DivineFootprints>.CreateInstance();
        ProjectilePool = new ProjectilePool<DivineFootprintsProjectile>(this);
        await ProjectilePool.Initialize();
        CheckMovedDistance().Forget();
    }

    private async UniTaskVoid CheckMovedDistance()
    {
        var movedDistance = 0f;
        var lastPosition = transform.position;
        while (true)
        {
            movedDistance += (lastPosition - transform.position).magnitude;
            if (movedDistance > ModifiedWeaponStats.cooldown)
            {
                movedDistance -= ModifiedWeaponStats.cooldown;
                _summoned.Attack();
            }

            lastPosition = transform.position;
            await UniTask.Yield(DestroyCts.Token);
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        Destroy(_summoned.gameObject);
        ProjectilePool.Dispose();
        SummonedWeaponBase<DivineFootprintsSummoned, DivineFootprints>.Dispose();
    }
}