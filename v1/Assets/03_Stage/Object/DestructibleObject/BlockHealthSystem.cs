using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockHealthSystem : HealthSystemBase
{
    public override void Damage(float damage, bool isCritical)
    {
        throw new System.NotImplementedException();
    }

    public override void Damage(float damage, bool isCritical, float knockbackPower)
    {
        throw new System.NotImplementedException();
    }

    public override void Knockback(float knockbackPower)
    {
        throw new System.NotImplementedException();
    }

    public override void Kill()
    {
        throw new System.NotImplementedException();
    }
}
