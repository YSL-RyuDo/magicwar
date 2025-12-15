//using UnityEngine;

//public class Enemy : MonoBehaviour
//{

//    [Header("스탯")]
//    public int maxHealth = 100;
//    private int currentHealth;

//    [Header("UI")]
//    public EnemyHealthUI healthUI;

//    [Header("이동")]
//    public float moveSpeed = 3f;
//    private float originalSpeed;
//    private Vector2 targetPosition;
//    private bool isMoving = false;

//    private Coroutine slowCoroutine;
//    private Animator animator;
//    private Rigidbody2D rb;
//    public string obstacleTag = "Obstacle";
//    void Start()
//    {
//        currentHealth = maxHealth;
//        originalSpeed = moveSpeed;
//        healthUI?.SetHealthRatio(1f);

//        animator = GetComponent<Animator>();
//        rb = GetComponent<Rigidbody2D>();
//        if (rb != null)
//            rb.freezeRotation = true;

//        if (animator == null)
//            Debug.LogWarning("Enemy에 Animator가 없습니다.");

//        targetPosition = transform.position;
//    }

//    void FixedUpdate()
//    {
//        if (isMoving)
//        {
//            Vector2 newPosition = Vector2.MoveTowards(rb.position, targetPosition, moveSpeed * Time.fixedDeltaTime);
//            rb.MovePosition(newPosition);

//            if (Vector2.Distance(rb.position, targetPosition) < 0.05f)
//            {
//                isMoving = false;
//                animator?.SetBool("isRun", false);
//            }
//        }
//    }

//    // 서버에서 받은 좌표 적용
//    public void SetTargetPosition(Vector2 pos)
//    {
//        targetPosition = pos;
//        isMoving = true;
//        animator?.SetBool("isRun", true);
//    }

//    public void TakeDamage(int damage)
//    {
//        currentHealth -= damage;
//        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
//        healthUI?.SetHealthRatio((float)currentHealth / maxHealth);

//        animator?.SetTrigger("hurt");

//        if (currentHealth <= 0)
//            Die();
//    }

//    public void ApplySlow(float slowRatio, float duration)
//    {
//        if (slowCoroutine != null)
//            StopCoroutine(slowCoroutine);

//        slowCoroutine = StartCoroutine(SlowRoutine(slowRatio, duration));
//    }

//    private System.Collections.IEnumerator SlowRoutine(float ratio, float duration)
//    {
//        moveSpeed *= ratio;
//        yield return new WaitForSeconds(duration);
//        moveSpeed = originalSpeed;
//        slowCoroutine = null;
//    }

//    void Die()
//    {
//        animator?.SetTrigger("die");
//        Destroy(gameObject, 0.5f);
//    }

//    private void OnCollisionEnter2D(Collision2D collision)
//    {
//        if (collision.collider.CompareTag(obstacleTag))
//        {
//            isMoving = false;
//            if (animator != null)
//                animator.SetBool("isRun", false);
//        }
//    }

//}
