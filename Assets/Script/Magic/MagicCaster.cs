using System.Collections.Generic;
using UnityEngine;

public class MagicCaster : MonoBehaviour
{
    [Header("발사 위치")]
    public Transform firePoint;

    [Header("마법 슬롯 (QWER)")]
    public int[] selectedMagicIds = new int[4]; // 카드 ID 기반

    [Header("카드 데이터베이스")]
    public CardDatabase cardDB;

    // 서버에서 받은 런타임 데이터 저장소
    private Dictionary<int, CardRuntimeData> runtimeDict = new();
    private int currentSlotIndex = -1; // 현재 선택된 슬롯 (-1 = 없음)

    void Update()
    {
        HandleMagicSelection();
        HandleMagicCasting();
    }

    void HandleMagicSelection()
    {
        if (Input.GetKeyDown(KeyCode.Q)) { currentSlotIndex = 0; Debug.Log($"[Q] 선택됨: {selectedMagicIds[0]}"); }
        else if (Input.GetKeyDown(KeyCode.W)) { currentSlotIndex = 1; Debug.Log($"[W] 선택됨: {selectedMagicIds[1]}"); }
        else if (Input.GetKeyDown(KeyCode.E)) { currentSlotIndex = 2; Debug.Log($"[E] 선택됨: {selectedMagicIds[2]}"); }
        else if (Input.GetKeyDown(KeyCode.R)) { currentSlotIndex = 3; Debug.Log($"[R] 선택됨: {selectedMagicIds[3]}"); }
    }

    void HandleMagicCasting()
    {
        if (Input.GetMouseButtonDown(0) && currentSlotIndex >= 0)
        {
            int cardId = selectedMagicIds[currentSlotIndex];
            if (cardId >= 0)
            {
                CastMagic(cardId);
            }
        }
    }

    public void SetRuntimeData(int id, CardRuntimeData data, int slotIndex)
    {
        runtimeDict[id] = data; // 덮어쓰기 or 추가

        if (slotIndex >= 0 && slotIndex < selectedMagicIds.Length)
            selectedMagicIds[slotIndex] = id;

        Debug.Log($"[MagicCaster] 슬롯 {slotIndex} → 카드 {id} 등록 완료 (공격력={data.attack}, 쿨타임={data.cooldown})");
    }

    public void CastMagic(int cardId)
    {
        var staticData = cardDB.GetCard(cardId);
        if (staticData == null || !runtimeDict.ContainsKey(cardId))
        {
            Debug.LogWarning($"[MagicCaster] 카드 {cardId} 시전 실패 (데이터 없음)");
            return;
        }

        var runtimeData = runtimeDict[cardId];

        // 마우스 위치 → 방향 계산
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;
        Vector3 direction = (mouseWorldPos - firePoint.position).normalized;

        // 발사체 생성
        GameObject prefabToUse = CompareTag("Player") ? staticData.playerPrefab : staticData.enemyPrefab;
        if (prefabToUse == null)
        {
            Debug.LogWarning($"[MagicCaster] 카드 {cardId} 프리팹이 없음");
            return;
        }

        GameObject magic = Instantiate(prefabToUse, firePoint.position, Quaternion.identity);
        magic.transform.position = new Vector3(magic.transform.position.x, magic.transform.position.y, -1f);

        // 속도 적용
        Rigidbody2D rb = magic.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.velocity = direction * runtimeData.projectileSpeed;

        // 투사체 인터페이스 적용
        var projectile = magic.GetComponent<IMagicProjectile>();
        if (projectile != null)
        {
            projectile.shooterTag = gameObject.tag;
            projectile.shooterId = SessionData.UserID;   // 내가 쏜 유저 ID 기록
            projectile.SetDamage(runtimeData.attack);

            if (staticData.applySlow)
                projectile.SetSlowEffect(0.5f, runtimeData.duration);
        }

        // 서버로 발사체 패킷 전송
        if (NetworkConnector.Instance != null)
        {
            string userId = SessionData.UserID;
            string payload = $"{userId},{cardId},{firePoint.position.x:F2},{firePoint.position.y:F2},{direction.x:F2},{direction.y:F2}";
            NetworkConnector.Instance.SendPacket(PacketType.CastMagic, payload);
        }

        Debug.Log($"[MagicCaster] {staticData.cardName} 발사됨! (Attack={runtimeData.attack}, 쿨타임={runtimeData.cooldown})");
    }
}
