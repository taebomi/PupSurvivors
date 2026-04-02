using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamagable
{
    Vector3 FloatingDamageWorldPos { get; }
    void OnDamaged();
    void OnDied();
}
