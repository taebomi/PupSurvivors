using System.Collections;
using System.Collections.Generic;
using PupSurvivors.ObjectPool;
using UnityEngine;

public class DestructibleObject : MonoBehaviour, IDamagable
{
    private int _savedLuck;

    private static LimitedObjectPool<DestructibleObject> _pool;

    public static void SetManagedPool(LimitedObjectPool<DestructibleObject> pool)
    {
        _pool = pool;
    }

    // 배치 용도 함수
    public void Set(Vector3 pos, int luck)
    {
        transform.position = pos;
        _savedLuck = luck;
        gameObject.SetActive(true);
    }
    
    // 데미지 받을 시
    public void OnDamaged()
    {
        // 반짝임 이펙트
        // 사운드 재생
    }

    // 데미지 받은 횟수 만족하여 파괴될 시
    public void OnDestroyed()
    {
        var item = StageManager.Instance.GetRandomItem(_savedLuck);
        item.Set(transform.position);
            
        gameObject.SetActive(false);
    }

    public void Damage(float damage, bool isCritical)
    {
        // 크리티컬과 데미지 양에 관계 없이 히트횟수만 체크
        Debug.Log("공격당함");
    }

    public void Knockback(float power)
    {
        // 넉백당하지 않음
    }

    public void Kill()
    {
        // 즉사당하지 않음
    }
}
