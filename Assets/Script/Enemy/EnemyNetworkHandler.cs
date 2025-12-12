//using System.Runtime.InteropServices;
//using UnityEngine;

//public class EnemyNetworkHandler : MonoBehaviour
//{
//    [Header("적 캐릭터 (상대방 표시용)")]
//    [SerializeField] private Enemy enemy; // Enemy_Wizard 오브젝트 연결

//    private NetworkConnector network;

//    void Start()
//    {
//        network = FindFirstObjectByType<NetworkConnector>();

//        if (network != null)
//        {
//            // 서버에서 상대 이동 패킷 받기 (예: PacketType.EnemyMove)
//            network.Dispatcher.Register(PacketType.Move, OnReceiveEnemyMove);
//        }
//    }

//    // 서버에서 이동 좌표 수신
//    private void OnReceiveEnemyMove(byte[] body)
//    {
//        if (body.Length < Marshal.SizeOf<EnemyMovePacket>())
//        {
//            Debug.LogError("[EnemyNetworkHandler] EnemyMove 패킷 사이즈 오류");
//            return;
//        }

//        EnemyMovePacket pkt = NetworkConnector.ByteArrayToStructure<EnemyMovePacket>(body, 0);

//        string enemyId = pkt.enemyId.TrimEnd('\0');
//        float x = pkt.posX;
//        float y = pkt.posY;

//        Vector2 pos = new Vector2(x, y);

//        if (enemy != null)
//        {
//            enemy.SetTargetPosition(pos);
//            Debug.Log($"[EnemyNetworkHandler] {enemyId} 이동 → {pos}");
//        }
//    }

//}
