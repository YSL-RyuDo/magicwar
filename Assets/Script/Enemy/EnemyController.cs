using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Team")]
    public TeamSide enemySide;

    [Header("Field Anchors")]
    [SerializeField] private Transform bottomAnchor;
    [SerializeField] private Transform topAnchor;

    [Header("Visual")]
    [SerializeField] private Transform visualRoot;   // 스프라이트 루트
    [SerializeField] private Animator animator;      // Animator

    [Header("Movement")]
    public float smoothSpeed = 10f;

    private Vector3 targetPos;
    private Vector3 prevWorldPos;
    private bool hasTarget = false;
    private bool isDead = false;

    private void Start()
    {
        prevWorldPos = transform.position;
    }

    private void Update()
    {
        if (isDead)
            return;

        if (!hasTarget)
        {
            SetRun(false);
            return;
        }

        // 이동
        transform.position = Vector3.Lerp(
            transform.position,
            targetPos,
            Time.deltaTime * smoothSpeed
        );

        // ───────── 방향 판단 (월드 이동 기준) ─────────
        float dx = transform.position.x - prevWorldPos.x;

        if (Mathf.Abs(dx) > 0.001f)
        {
            float dir = dx < 0 ? -1f : 1f;
            if (visualRoot != null)
                visualRoot.localScale = new Vector3(dir, 1f, 1f);

            SetRun(true);
        }
        else
        {
            SetRun(false);
        }

        prevWorldPos = transform.position;
    }

    // 서버에서 받은 "필드 기준 로컬 좌표"
    public void SetPosition(float localX, float localY)
    {
        if (isDead)
            return;

        Vector3 local = new Vector3(localX, localY, 0f);

        if (enemySide == TeamSide.Top)
        {
            local.x = -local.x;
            local.y = -local.y;
            targetPos = topAnchor.position + local;
        }
        else
        {
            targetPos = bottomAnchor.position + local;
        }

        hasTarget = true;
    }

    private void SetRun(bool isRun)
    {
        if (animator != null)
            animator.SetBool("isRun", isRun);
    }

    public void Die()
    {
        if (isDead)
            return;

        isDead = true;
        hasTarget = false;

        if (animator != null)
        {
            animator.SetBool("isRun", false);
            animator.SetTrigger("die");
            OnDieAnimationEnd();
            Time.timeScale = 0f;
        }
    }

    public void OnDieAnimationEnd()
    {
        Destroy(gameObject);
    }
}
