using UnityEngine;

public class Fireball : MonoBehaviour, IMagicProjectile
{
    public string shooterTag { get; set; }

    private int damage;

    private bool isInMap = false;
    private bool isInObstacle = false;

    public void SetDamage(int dmg)
    {
        damage = dmg;
    }

    public void SetSlowEffect(float factor, float duration)
    {
        // Fireball은 슬로우 효과 없음 → 무시
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
            }

            Destroy(gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Map")) isInMap = false;
        if (other.CompareTag("Obstacle")) isInObstacle = false;

        if (!isInMap && !isInObstacle)
        {
            Debug.Log("Map과 Obstacle 모두 벗어남 → 파괴");
            Destroy(gameObject);
        }
    }
}
