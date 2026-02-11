using UnityEngine;
using System.Runtime.InteropServices;

public class EnemyService : MonoBehaviour
{
    private NetworkConnector network;
    private EnemyController enemy;

    private void Awake()
    {
        Debug.Log("[EnemyService] Awake");

        network = NetworkConnector.Instance;
        Debug.Log("[EnemyService] NetworkConnector = " + (network != null));

        enemy = FindFirstObjectByType<EnemyController>();
        Debug.Log("[EnemyService] EnemyController = " + (enemy != null));
    }

    private void Start()
    {
        Debug.Log("[EnemyService] Start - Register Move Packet");
        network.Dispatcher.Register(PacketType.Move, OnEnemyMoveReceived);
        network.Dispatcher.Register(PacketType.CastBlink, OnEnemyBlinkReceived);
    }

    private void OnDestroy()
    {
        Debug.Log("[EnemyService] OnDestroy - Unregister Move Packet");
        if (network != null)
        {
            network.Dispatcher.Unregister(PacketType.Move, OnEnemyMoveReceived);
            network.Dispatcher.Unregister(PacketType.CastBlink, OnEnemyBlinkReceived);
        }
    }

    // 서버 → 상대 이동 패킷 수신
    private void OnEnemyMoveReceived(byte[] body)
    {
        Debug.Log($"[EnemyService] Move Packet Received. body.Length = {body.Length}");

        int expectedSize = Marshal.SizeOf<PKT_MOVE_STRUCT>();
        Debug.Log($"[EnemyService] Expected PKT_MOVE_STRUCT size = {expectedSize}");

        if (body.Length != expectedSize)
        {
            Debug.LogError($"[EnemyMove] size mismatch {body.Length}/{expectedSize}");
            return;
        }

        PKT_MOVE_STRUCT pkt =
            NetworkConnector.ByteArrayToStructure<PKT_MOVE_STRUCT>(body);

        if (enemy == null)
        {
            Debug.LogError("[EnemyService] EnemyController is NULL");
            return;
        }

        Debug.Log("[EnemyService] Calling EnemyController.SetPosition()");
        enemy.SetPosition(pkt.x, pkt.y);
    }

    private void OnEnemyBlinkReceived(byte[] body)
    {
        int expectedSize = Marshal.SizeOf<PKT_CAST_BLINK>();
        if (body.Length != expectedSize)
            return;

        PKT_CAST_BLINK pkt =
            NetworkConnector.ByteArrayToStructure<PKT_CAST_BLINK>(body);

        if (enemy == null)
            return;

        enemy.ForceSetPosition(pkt.x, pkt.y);
    }
}
