using UnityEngine;

public class Fireball : MonoBehaviour, IMagicProjectile
{
    public string shooterTag { get; set; }
    public string shooterId { get; set; }
    private int damage;
    private bool isInMap = false;
    private bool isInObstacle = false;

    public void SetDamage(int dmg) => damage = dmg;

    public void SetSlowEffect(float factor, float duration)
    {
        // Fireball은 슬로우 효과 없음 → 무시
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Map")) { isInMap = true; return; }
        if (other.CompareTag("Obstacle")) { isInObstacle = true; return; }

        string targetId = null;

        if (other.CompareTag("Player"))
        {
            if (shooterTag == "Player") return; // 아군 판정 제외
            targetId = SessionData.UserID;
        }
        else if (other.CompareTag("Enemy"))
        {
            if (shooterTag == "Enemy") return; // 아군 판정 제외
            targetId = SessionData.OpponentID;
        }

        if (!string.IsNullOrEmpty(targetId))
        {
            // 내가 쏜 발사체일 때만 서버에 패킷 전송
            if (shooterId == SessionData.UserID)
            {
                SendHitPacket(targetId, damage);
            }

            Destroy(gameObject);
        }
    }


    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Map")) isInMap = false;
        if (other.CompareTag("Obstacle")) isInObstacle = false;

        if (!isInMap && !isInObstacle)
        {
            Debug.Log("Map과 Obstacle 모두 벗어남 → 파괴");
            Destroy(gameObject);
        }
    }

    private void SendHitPacket(string targetUserId, int dmg)
    {
        var network = NetworkConnector.Instance;
        if (network != null)
        {
            string payload = $"{targetUserId},{dmg}";
            network.SendPacket(PacketType.Hit, payload);
            Debug.Log($"[Fireball] 서버로 Hit 패킷 전송: {payload}");
        }
    }
}
