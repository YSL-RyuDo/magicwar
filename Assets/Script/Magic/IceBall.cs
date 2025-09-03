using UnityEngine;

public class Iceball : MonoBehaviour, IMagicProjectile
{
    public string shooterTag { get; set; }

    private int damage;
    private float slowDuration;
    private float slowFactor;

    private bool isInMap = false;
    private bool isInObstacle = false;

    public void SetDamage(int dmg)
    {
        damage = dmg;
    }

    public void SetSlowEffect(float factor, float duration)
    {
        slowFactor = factor;
        slowDuration = duration;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Map"))
        {
            isInMap = true;
            return;
        }

        if (other.CompareTag("Obstacle"))
        {
            isInObstacle = true;
            return;
        }

        if (other.CompareTag("Player"))
        {
            if (shooterTag == "Player") return;

            CharacterHealth health = other.GetComponent<CharacterHealth>();
            if (health != null)
            {
                health.TakeDamage(damage);
                health.ApplySlow(slowFactor, slowDuration);
            }

            Destroy(gameObject);
        }
        else if (other.CompareTag("Enemy"))
        {
            if (shooterTag == "Enemy") return;

            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                enemy.ApplySlow(slowFactor, slowDuration);
            }

            Destroy(gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Map")) isInMap = false;
        if (other.CompareTag("Obstacle")) isInObstacle = false;

        if (!isInMap && !isInObstacle)
            Destroy(gameObject);
    }
}
