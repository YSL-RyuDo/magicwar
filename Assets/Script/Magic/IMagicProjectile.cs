using UnityEngine;

public interface IMagicProjectile
{
    string shooterTag { get; set; }
    string shooterId { get; set; }
    void SetDamage(int damage);
    void SetSlowEffect(float factor, float duration);
}
