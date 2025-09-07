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

    private void OnSpawnMagic(string msg)
    {
        // msg: "userId,cardId,x,y,dirX,dirY,attack,speed"
        Debug.Log("[EnemyMagicHandler] SpawnMagic 수신: " + msg);

        string[] tokens = msg.Split(',');
        if (tokens.Length < 8) return;

        string userId = tokens[0];
        int cardId = int.Parse(tokens[1]);
        float x = float.Parse(tokens[2]);
        float y = float.Parse(tokens[3]);
        float dirX = float.Parse(tokens[4]);
        float dirY = float.Parse(tokens[5]);
        int attack = int.Parse(tokens[6]);
        float speed = float.Parse(tokens[7]);

        // 카드 정보 가져오기
        var card = cardDB.GetCard(cardId);
        if (card == null)
        {
            Debug.LogWarning($"SpawnMagic: 카드 {cardId} 없음");
            return;
        }

        // 발사체 프리팹 선택 (상대방 → enemyPrefab 사용)
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
        {
            rb.velocity = dir * speed; // 서버에서 받은 속도 사용
        }

        // 공격력 적용
        var projectile = magic.GetComponent<IMagicProjectile>();
        if (projectile != null)
        {
            projectile.shooterTag = "Enemy"; // 상대방이 쏜 거니까 Enemy
            projectile.SetDamage(attack);    // 서버에서 받은 공격력 사용
        }

        Debug.Log($"[EnemyMagicHandler] {userId} → 카드 {cardId} 발사 (pos={spawnPos}, dir={dir}, atk={attack}, spd={speed})");
    }

}
