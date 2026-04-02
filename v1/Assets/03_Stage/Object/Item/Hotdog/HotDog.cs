using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HotDog : ItemObjectBase
{
    private const float HealValue = 25f;
    protected override void ApplyItem(PlayerController target)
    {
        target.HealthSystem.Heal(HealValue);
        Destroy(gameObject);
    }
}
