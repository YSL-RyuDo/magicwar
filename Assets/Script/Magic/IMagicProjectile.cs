using UnityEngine;

public interface IMagicProjectile
{
    string shooterTag { get; set; }
    void SetDamage(int damage);
    void SetSlowEffect(float factor, float duration);
}
