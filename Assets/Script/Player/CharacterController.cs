using System.Collections;
using UnityEngine;

public class CharacterController2D : MonoBehaviour
{
    public float moveSpeed = 5f;
    public string mapTag = "Map";
    public string obstacleTag = "Obstacle";

    private float originalMoveSpeed; // 원래 속도 저장용
    private Vector2 targetPosition;
    private bool isMoving = false;
    private Rigidbody2D rb;
    private Animator anim;
    private int direction = 1;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        targetPosition = rb.position;

        originalMoveSpeed = moveSpeed;

        anim = GetComponent<Animator>();
        if (anim == null)
            Debug.LogWarning("Animator 컴포넌트를 찾을 수 없습니다!");
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mouseWorldPos.x, mouseWorldPos.y);

            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
            if (hit.collider != null && hit.collider.CompareTag(mapTag))
            {
                targetPosition = mousePos2D;
                isMoving = true;

                direction = (mouseWorldPos.x < transform.position.x) ? -1 : 1;
                transform.localScale = new Vector3(direction, 1, 1);

                if (anim != null && !anim.GetBool("isJump"))
                    anim.SetBool("isRun", true);
            }
        }

        if (!isMoving && anim != null)
            anim.SetBool("isRun", false);
    }

    private void FixedUpdate()
    {
        if (isMoving)
        {
            Vector2 newPosition = Vector2.MoveTowards(rb.position, targetPosition, moveSpeed * Time.fixedDeltaTime);
            rb.MovePosition(newPosition);

            if (Vector2.Distance(rb.position, targetPosition) < 0.05f)
            {
                isMoving = false;
                if (anim != null)
                    anim.SetBool("isRun", false);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag(obstacleTag))
        {
            isMoving = false;
            if (anim != null)
                anim.SetBool("isRun", false);
        }
    }

    // 슬로우 처리 (IMovable)
    public void ApplySlow(float factor, float duration)
    {
        StopAllCoroutines(); // 중복 슬로우 방지
        StartCoroutine(SlowRoutine(factor, duration));
    }

    private IEnumerator SlowRoutine(float factor, float duration)
    {
        moveSpeed *= factor;
        yield return new WaitForSeconds(duration);
        moveSpeed = originalMoveSpeed;
    }
}
