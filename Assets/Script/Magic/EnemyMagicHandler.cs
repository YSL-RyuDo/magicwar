using System.Runtime.InteropServices;
using UnityEngine;

public class EnemyMagicHandler : MonoBehaviour
{
    [Header("카드 DB")]
    [SerializeField] private CardDatabase cardDB;

    private NetworkConnector network;

    void Start()
    {
        network = FindFirstObjectByType<NetworkConnector>();

        if (network != null)
        {
            network.Dispatcher.Register(PacketType.SpawnMagic, OnSpawnMagic);
        }
    }

    private void OnSpawnMagic(byte[] body)
    {
        if (body.Length < Marshal.SizeOf<SpawnMagicPacket>())
        {
            Debug.LogError("[EnemyMagicHandler] SpawnMagic 패킷 사이즈 오류");
            return;
        }

        SpawnMagicPacket pkt = NetworkConnector.ByteArrayToStructure<SpawnMagicPacket>(body, 0);

        string userId = pkt.userId.TrimEnd('\0');
        int cardId = pkt.cardId;

        float x = pkt.posX;
        float y = pkt.posY;
        float dirX = pkt.dirX;
        float dirY = pkt.dirY;

        int attack = pkt.attack;
        float speed = pkt.speed;

        // 카드 정보 가져오기
        var card = cardDB.GetCard(cardId);
        if (card == null)
        {
            Debug.LogWarning($"SpawnMagic: 카드 {cardId} 없음");
            return;
        }

        GameObject prefab = card.enemyPrefab;
        if (prefab == null)
        {
            Debug.LogWarning($"SpawnMagic: 카드 {cardId} enemyPrefab 없음");
            return;
        }

        // 발사체 생성
        Vector3 spawnPos = new Vector3(x, y, -1f);
        GameObject magic = Instantiate(prefab, spawnPos, Quaternion.identity);

        // 방향 적용
        Vector3 dir = new Vector3(dirX, dirY, 0f).normalized;

        Rigidbody2D rb = magic.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.velocity = dir * speed;

        // 공격력 적용
        var projectile = magic.GetComponent<IMagicProjectile>();
        if (projectile != null)
        {
            projectile.shooterTag = "Enemy";
            projectile.SetDamage(attack);
        }

        Debug.Log($"[EnemyMagicHandler] SpawnMagic → user={userId}, card={cardId}, pos=({x},{y}), atk={attack}, spd={speed}");
    }

}
