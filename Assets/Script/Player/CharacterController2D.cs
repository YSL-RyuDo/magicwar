using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class CharacterController2D : MonoBehaviour
{
    private NetworkConnector network;

    [Header("Movement")]
    public float moveSpeed = 5f;
    private float originalMoveSpeed;

    [Header("Field Anchor (필수)")]
    [SerializeField] private Transform myFieldAnchor;

    [Header("Tags")]
    public string obstacleTag = "Obstacle";

    [Header("Visual (Sprite Root)")]
    [SerializeField] private Transform visualRoot;

    private Vector2 targetPosition;
    private bool isMoving;
    private bool isDead = false;
    private bool isStunned = false;

    private Rigidbody2D rb;
    private Animator anim;

    // ───── 네트워크 전송 제어 ─────
    private float sendInterval = 0.1f;
    private float sendTimer = 0f;
    private Vector2 lastSentLocalPos;

    // 슬로우 코루틴
    private Coroutine slowRoutine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.gravityScale = 0;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        anim = GetComponentInChildren<Animator>();

        // 원래 이동속도 저장
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
        {
            StopMoving();
        }
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

    public void Die()
    {
        if (isDead) return;

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

    // ─────────────────────────────
    // 슬로우 처리 (핵심)
    // ─────────────────────────────
    public void ApplySlow(float ratio, int durationMs)
    {
        if (slowRoutine != null)
            StopCoroutine(slowRoutine);

        slowRoutine = StartCoroutine(SlowRoutine(ratio, durationMs));
    }

    private IEnumerator SlowRoutine(float ratio, int durationMs)
    {
        moveSpeed = originalMoveSpeed * ratio;
        Debug.Log($"[SLOW APPLY] moveSpeed={moveSpeed}");

        yield return new WaitForSeconds(durationMs / 1000f);

        moveSpeed = originalMoveSpeed;
        Debug.Log($"[SLOW END] moveSpeed restored={moveSpeed}");

        slowRoutine = null;
    }

    // ─────────────────────────────
    // 스턴 처리
    // ─────────────────────────────
    private void ApplyStun(int durationMs)
    {
        StartCoroutine(StunRoutine(durationMs));
    }

    private IEnumerator StunRoutine(int durationMs)
    {
        isStunned = true;
        yield return new WaitForSeconds(durationMs / 1000f);
        isStunned = false;
    }

    // ─────────────────────────────
    // 네트워크 수신
    // ─────────────────────────────
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
}
