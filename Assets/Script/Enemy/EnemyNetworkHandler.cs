using UnityEngine;

public class EnemyNetworkHandler : MonoBehaviour
{
    [Header("적 캐릭터 (상대방 표시용)")]
    [SerializeField] private Enemy enemy; // Enemy_Wizard 오브젝트 연결

    private NetworkConnector network;

    void Start()
    {
        network = FindFirstObjectByType<NetworkConnector>();

        if (network != null)
        {
            // 서버에서 상대 이동 패킷 받기 (예: PacketType.EnemyMove)
            network.Dispatcher.Register(PacketType.Move, OnReceiveEnemyMove);
        }
    }

    // 서버에서 이동 좌표 수신
    private void OnReceiveEnemyMove(string msg)
    {
        // 예: "asd,-1.11,1.10"
        Debug.Log("[EnemyNetworkHandler] 받은 데이터 = " + msg);

        string[] tokens = msg.Split(',');
        if (tokens.Length >= 3)
        {
            string enemyId = tokens[0];
            if (float.TryParse(tokens[1], out float x) && float.TryParse(tokens[2], out float y))
            {
                Vector2 pos = new Vector2(x, y);

                if (enemy != null)
                {
                    enemy.SetTargetPosition(pos);
                    Debug.Log($"[EnemyNetworkHandler] {enemyId} 이동 → {pos}");
                }
            }
        }
    }
}
