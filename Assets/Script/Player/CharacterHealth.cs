using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class CharacterHealth : MonoBehaviour
{
    [Header("체력 설정")]
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;

    [Header("이벤트")]
    public UnityEvent onTakeDamage;
    public UnityEvent onHeal;
    public UnityEvent onDeath;

    private Animator animator;
    private bool isSlowed = false;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
        animator = GetComponentInChildren<Animator>();
    }

    /// <summary>
    /// 순수 데미지만 처리
    /// </summary>
    public void TakeDamage(int damage)
    {
        if (damage <= 0 || currentHealth <= 0) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        if (animator != null)
            animator.SetTrigger("hurt");

        onTakeDamage?.Invoke();

        if (currentHealth == 0)
            Die();
    }

    /// <summary>
    /// 이동속도 감소 처리 (예: 아이스볼 맞음)
    /// </summary>
    public void ApplySlow(float factor, float duration)
    {
        if (isSlowed) return;

        CharacterController2D controller = GetComponent<CharacterController2D>();
        if (controller != null)
        {
            StartCoroutine(SlowRoutine(controller, factor, duration));
        }
    }

    private IEnumerator SlowRoutine(CharacterController2D controller, float factor, float duration)
    {
        isSlowed = true;

        float originalSpeed = controller.moveSpeed;
        controller.moveSpeed *= factor;

        yield return new WaitForSeconds(duration);

        controller.moveSpeed = originalSpeed;
        isSlowed = false;
    }

    public void Heal(int amount)
    {
        if (amount <= 0 || currentHealth <= 0) return;

        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);

        onHeal?.Invoke();
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name}이(가) 죽었습니다.");

        if (animator != null)
            animator.SetTrigger("die");

        onDeath?.Invoke();
    }

    public void SetMaxHealth(int newMaxHealth)
    {
        maxHealth = Mathf.Max(1, newMaxHealth);
        currentHealth = maxHealth;
    }
}
