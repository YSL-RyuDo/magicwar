using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Runtime.InteropServices;

public class LoadingSystem : MonoBehaviour
{
    private NetworkConnector network;

    [SerializeField] private Button cancelButton;

    private void Start()
    {
        network = FindFirstObjectByType<NetworkConnector>();

        if (cancelButton != null)
            cancelButton.onClick.AddListener(OnClickCancelButton);

        if (network != null)
            network.Dispatcher.Register(PacketType.ResponseMatching, OnMatchResponseBinary);
    }

    private void OnClickCancelButton()
    {
        Debug.Log("[Loading] 衙蘆 鏃模 ⊥ 煎綠煎 犒敝");

        string currentUserId = SessionData.UserID;
        network.SendPacket(PacketType.CancelMatch, currentUserId);

        SceneManager.LoadScene("LobbyScene");
    }

    // ------------------------------
    //   夥檜傘葬 衙蘆 擬港 籀葬
    // ------------------------------
    private void OnMatchResponseBinary(byte[] body)
    {
        if (body.Length < Marshal.SizeOf<MatchResponsePacket>())
        {
            Debug.LogError("[Loading] 衙蘆 ぬ韃 觼晦 螃盟");
            return;
        }

        MatchResponsePacket pkt =
            NetworkConnector.ByteArrayToStructure<MatchResponsePacket>(body);

        Debug.Log($"[Loading] 衙蘆 擬港 熱褐: result={pkt.result}, room={pkt.roomId}, A={pkt.userA}, B={pkt.userB}");

        // 式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式
        //  1. WAIT
        // 式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式
        if (pkt.result == 0)
        {
            Debug.Log("[Loading] 衙蘆 渠晦(WAIT) 醞...");
            return;
        }

        // 式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式
        //  2. 衙蘆 撩奢 OK
        // 式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式
        if (pkt.result == 1)
        {
            string myId = SessionData.UserID;
            string opponent = (pkt.userA == myId) ? pkt.userB : pkt.userA;

            SessionData.RoomID = pkt.roomId;
            SessionData.OpponentID = opponent;

            Debug.Log($"[Loading] 衙蘆 諫猿 ⊥ Room={pkt.roomId}, My={myId}, Opponent={opponent}");

            SceneManager.LoadScene("GameScene");
            return;
        }

        // 式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式
        //  3. 衙蘆 褒ぬ 籀葬
        // 式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式式
        if (pkt.result == 2)
        {
            Debug.LogWarning("[Loading] 衙蘆 褒ぬ");
            SceneManager.LoadScene("LobbyScene");
        }
    }
}
