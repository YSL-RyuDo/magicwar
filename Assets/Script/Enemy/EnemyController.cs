using UnityEngine;
using System.Collections;

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

    // 상태
    private Coroutine invisRoutine;

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

        transform.position = Vector3.Lerp(
            transform.position,
            targetPos,
            Time.deltaTime * smoothSpeed
        );

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

    public void SetPosition(float localX, float localY)
    {
        if (isDead)
            return;

        Vector3 local = new Vector3(localX, localY, 0f);
        Vector3 worldPos;

        if (enemySide == TeamSide.Top)
        {
            local.x = -local.x;
            local.y = -local.y;
            worldPos = topAnchor.position + local;
        }
        else
        {
            worldPos = bottomAnchor.position + local;
        }

        targetPos = worldPos;
        hasTarget = true;
    }


    private void SetRun(bool isRun)
    {
        if (animator != null)
            animator.SetBool("isRun", isRun);
    }

    public void PlayAttack()
    {
        if (isDead)
            return;

        if (animator != null)
        {
            animator.SetBool("isRun", false);
            animator.ResetTrigger("attack");
            animator.SetTrigger("attack");
        }
    }

    public void PlayHurt()
    {
        if (isDead)
            return;

        if (animator != null)
        {
            animator.SetBool("isRun", false);
            animator.ResetTrigger("hurt");
            animator.SetTrigger("hurt");
        }
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
        }
    }

    public void ApplyInvisibility(float duration)
    {
        if (invisRoutine != null)
            StopCoroutine(invisRoutine);

        invisRoutine = StartCoroutine(InvisibilityRoutine(duration));
    }

    private IEnumerator InvisibilityRoutine(float duration)
    {
        SetInvisible(true);

        if (duration > 0f)
            yield return new WaitForSeconds(duration);

        SetInvisible(false);
        invisRoutine = null;
    }

    private void SetInvisible(bool invisible)
    {
        if (visualRoot == null)
            return;

        foreach (var sr in visualRoot.GetComponentsInChildren<SpriteRenderer>())
            sr.enabled = !invisible;
    }

    public void Teleport(float localX, float localY)
    {
        if (isDead)
            return;

        Vector3 local = new Vector3(localX, localY, 0f);
        Vector3 worldPos;

        if (enemySide == TeamSide.Top)
        {
            local.x = -local.x;
            local.y = -local.y;
            worldPos = topAnchor.position + local;
        }
        else
        {
            worldPos = bottomAnchor.position + local;
        }

        hasTarget = false;
        targetPos = worldPos;
        prevWorldPos = worldPos;

        transform.position = worldPos;
        SetRun(false);
    }

    public void ForceSetPosition(float localX, float localY)
    {
        Vector3 local = new Vector3(localX, localY, 0f);
        Vector3 worldPos;

        if (enemySide == TeamSide.Top)
        {
            local.x = -local.x;
            local.y = -local.y;
            worldPos = topAnchor.position + local;
        }
        else
        {
            worldPos = bottomAnchor.position + local;
        }

        transform.position = worldPos;
        targetPos = worldPos;
        prevWorldPos = worldPos;
        hasTarget = false;

        SetRun(false);
    }

}
