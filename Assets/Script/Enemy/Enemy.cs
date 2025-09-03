using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;

    public EnemyHealthUI healthUI;

    public float moveSpeed = 3f;
    private float originalSpeed;

    private Coroutine slowCoroutine;

    private Animator animator;

    void Start()
    {
        currentHealth = maxHealth;
        originalSpeed = moveSpeed;
        healthUI?.SetHealthRatio(1f);

        animator = GetComponent<Animator>();
        if (animator == null)
            Debug.LogWarning("Enemy에 Animator가 없습니다.");
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        healthUI?.SetHealthRatio((float)currentHealth / maxHealth);

        // 피격 애니메이션
        animator?.SetTrigger("hurt");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void ApplySlow(float slowRatio, float duration)
    {
        if (slowCoroutine != null)
            StopCoroutine(slowCoroutine);

        slowCoroutine = StartCoroutine(SlowRoutine(slowRatio, duration));
    }

    private System.Collections.IEnumerator SlowRoutine(float ratio, float duration)
    {
        moveSpeed *= ratio;
        yield return new WaitForSeconds(duration);
        moveSpeed = originalSpeed;
        slowCoroutine = null;
    }

    void Die()
    {
        // 사망 애니메이션
        animator?.SetTrigger("die");

        // 애니메이션 재생 후 제거하고 싶다면 Delay 필요
        Destroy(gameObject, 0.5f); // 예: 0.5초 후 제거
    }
}