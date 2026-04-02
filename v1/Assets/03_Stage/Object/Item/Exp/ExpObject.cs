using System.Threading;
using Cysharp.Threading.Tasks;
using PupSurvivors.ObjectPool;
using UnityEngine;
using UnityEngine.Serialization;

public class ExpObject : ItemObjectBase
{
    [field: SerializeField] private AudioClip se;

    public float ExpValue { get; private set; }

    public static int VisibleCount { get; private set; }


    [SerializeField] private SpriteContainer expSprites;

    private LimitedObjectPool<ExpObject> _managedPool;


    public override void OnVisible()
    {
        base.OnVisible();
        VisibleCount++;
    }

    public override void OnInvisible()
    {
        base.OnInvisible();
        VisibleCount--;
    }

    public void SetManagedPool(LimitedObjectPool<ExpObject> pool)
    {
        _managedPool = pool;
    }

    public void SetExp(float expValue)
    {
        ExpValue = expValue;
        MainSr.sprite = expValue switch
        {
            < 5 => expSprites.data[0],
            < 15 => expSprites.data[1],
            < 50 => expSprites.data[2],
            < 200 => expSprites.data[3],
            _ => expSprites.data[4]
        };
    }

    public void AddExp(float expValue)
    {
        ExpValue += expValue;
        MainSr.sprite = ExpValue switch
        {
            < 5 => expSprites.data[0],
            < 15 => expSprites.data[1],
            < 50 => expSprites.data[2],
            < 200 => expSprites.data[3],
            _ => expSprites.data[4]
        };
    }

    protected override void ApplyItem(PlayerController target)
    {
        StageManager.Instance.AddExp(ExpValue * target.CurrentStats.expMultiplier);
        ExpValue = 0f;
        _managedPool.Push(this);
    }
}