using UnityEngine;

public interface IDamagable
{
    // ReSharper disable once InconsistentNaming
    public Transform transform { get; }
    
    public void Damage(float damage, bool isCritical);
    public void Knockback(float power);
    public void Kill();
}
