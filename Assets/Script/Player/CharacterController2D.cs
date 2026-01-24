using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class CharacterController2D : MonoBehaviour
{
    private NetworkConnector network;

    [Header("Movement")]
    public float moveSpeed = 5f;
    private float originalMoveSpeed;

    [Header("Field Anchor")]
    [SerializeField] private Transform myFieldAnchor;

    [Header("Tags")]
    public string obstacleTag = "Obstacle";

    [Header("Visual (Sprite Root)")]
    [SerializeField] private Transform visualRoot;

    private Vector2 targetPosition;
    private bool isMoving;
    private bool isDead = false;
    private bool isStunned = false;
    private bool isInvisible = false;

    private Rigidbody2D rb;
    private Animator anim;

    // 네트워크 이동 전송
    private float sendInterval = 0.1f;
    private float sendTimer = 0f;
    private Vector2 lastSentLocalPos;

    // 상태 코루틴
    private Coroutine slowRoutine;
    private Coroutine stunRoutine;
    private Coroutine castLockRoutine;
    private Coroutine invisRoutine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.gravityScale = 0;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        anim = GetComponentInChildren<Animator>();

        originalMoveSpeed = moveSpeed;
    }

    private void Start()
    {
        network = FindFirstObjectByType<NetworkConnector>();

        if (network != null)
        {
            network.Dispatcher.Register(PacketType.PKT_STATUS_SLOW, OnSlowReceived);
            network.Dispatcher.Register(PacketType.PKT_STATUS_STUN, OnStunReceived);
        }
    }

    private void OnDestroy()
    {
        if (network != null)
        {
            network.Dispatcher.Unregister(PacketType.PKT_STATUS_SLOW, OnSlowReceived);
            network.Dispatcher.Unregister(PacketType.PKT_STATUS_STUN, OnStunReceived);
        }
    }

    private void Update()
    {
        if (isDead || isStunned)
            return;

        if (Input.GetMouseButtonDown(1))
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0f;

            targetPosition = mouseWorld;
            isMoving = true;

            if (visualRoot != null)
            {
                float dir = (targetPosition.x < transform.position.x) ? -1f : 1f;
                visualRoot.localScale = new Vector3(dir, 1f, 1f);
            }

            if (anim != null)
                anim.SetBool("isRun", true);
        }
    }

    private void FixedUpdate()
    {
        if (isDead || isStunned || !isMoving)
            return;

        Vector2 nextWorldPos = Vector2.MoveTowards(
            rb.position,
            targetPosition,
            moveSpeed * Time.fixedDeltaTime
        );

        rb.MovePosition(nextWorldPos);

        if (myFieldAnchor == null)
            return;

        Vector2 localPos = nextWorldPos - (Vector2)myFieldAnchor.position;

        sendTimer += Time.fixedDeltaTime;

        if (sendTimer >= sendInterval &&
            Vector2.Distance(localPos, lastSentLocalPos) > 0.05f)
        {
            SendMove(localPos);
            lastSentLocalPos = localPos;
            sendTimer = 0f;
        }

        if (Vector2.Distance(rb.position, targetPosition) < 0.05f)
            StopMoving();
    }

    private void SendMove(Vector2 localPos)
    {
        if (network == null)
            return;

        PKT_MOVE_STRUCT pkt = new PKT_MOVE_STRUCT
        {
            userId = SessionData.UserID,
            x = localPos.x,
            y = localPos.y
        };

        network.SendPacket(
            PacketType.Move,
            NetworkConnector.StructToBytes(pkt)
        );
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag(obstacleTag))
            StopMoving();
    }

    private void StopMoving()
    {
        if (!isMoving)
            return;

        isMoving = false;

        if (anim != null)
            anim.SetBool("isRun", false);
    }

    // =====================
    // Combat / States
    // =====================
    public void PlayAttack()
    {
        if (isDead)
            return;

        StopMoving();
        rb.velocity = Vector2.zero;

        if (anim != null)
        {
            anim.SetBool("isRun", false);
            anim.ResetTrigger("attack");
            anim.SetTrigger("attack");
        }
    }

    public void PlayHurt()
    {
        if (isDead)
            return;

        StopMoving();

        if (anim != null)
        {
            anim.SetBool("isRun", false);
            anim.ResetTrigger("hurt");
            anim.SetTrigger("hurt");
        }
    }

    public void Die()
    {
        if (isDead)
            return;

        isDead = true;
        isMoving = false;

        rb.velocity = Vector2.zero;
        rb.simulated = false;

        if (anim != null)
        {
            anim.SetBool("isRun", false);
            anim.SetTrigger("die");
        }
    }

    // =====================
    // Slow / Stun
    // =====================
    public void ApplySlow(float ratio, int durationMs)
    {
        if (slowRoutine != null)
            StopCoroutine(slowRoutine);

        slowRoutine = StartCoroutine(SlowRoutine(ratio, durationMs));
    }

    private IEnumerator SlowRoutine(float ratio, int durationMs)
    {
        moveSpeed = originalMoveSpeed * ratio;
        yield return new WaitForSeconds(durationMs / 1000f);
        moveSpeed = originalMoveSpeed;
        slowRoutine = null;
    }

    private void ApplyStun(int durationMs)
    {
        if (stunRoutine != null)
            StopCoroutine(stunRoutine);

        stunRoutine = StartCoroutine(StunRoutine(durationMs));
    }

    private IEnumerator StunRoutine(int durationMs)
    {
        isStunned = true;
        StopMoving();

        if (anim != null)
            anim.SetBool("isRun", false);

        yield return new WaitForSeconds(durationMs / 1000f);

        isStunned = false;
        stunRoutine = null;
    }

    private void OnSlowReceived(byte[] body)
    {
        PKT_STATUS_SLOW_STRUCT pkt =
            NetworkConnector.ByteArrayToStructure<PKT_STATUS_SLOW_STRUCT>(body);

        string targetId = pkt.targetId.TrimEnd('\0', ' ');

        if (SessionData.UserID != targetId)
            return;

        ApplySlow(pkt.slowRatio, pkt.durationMs);
    }

    private void OnStunReceived(byte[] body)
    {
        PKT_STATUS_STUN_STRUCT pkt =
            NetworkConnector.ByteArrayToStructure<PKT_STATUS_STUN_STRUCT>(body);

        string targetId = pkt.targetId.TrimEnd('\0', ' ');

        if (SessionData.UserID != targetId)
            return;

        ApplyStun(pkt.durationMs);
    }

    // =====================
    // Cast Lock
    // =====================
    public void ApplyCastLock(int durationMs)
    {
        if (isDead)
            return;

        if (castLockRoutine != null)
            StopCoroutine(castLockRoutine);

        castLockRoutine = StartCoroutine(CastLockRoutine(durationMs));
    }

    private IEnumerator CastLockRoutine(int durationMs)
    {
        isStunned = true;
        StopMoving();

        yield return new WaitForSeconds(durationMs / 1000f);

        isStunned = false;
        castLockRoutine = null;
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

    private void SetInvisible(bool value)
    {
        if (visualRoot == null)
            return;

        float alpha = value ? 0.3f : 1f;

        foreach (var sr in visualRoot.GetComponentsInChildren<SpriteRenderer>())
        {
            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
        }
    }
}
