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

        SendMatchRequest();
    }

    private void OnClickCancelButton()
    {
        Debug.Log("[Loading] 매칭 취소 → 로비로 복귀");

        string currentUserId = SessionData.UserID;
        network.SendPacket(PacketType.CancelMatch, currentUserId);

        SceneManager.LoadScene("LobbyScene");
    }

    private void SendMatchRequest()
    {
        Debug.Log("[Loading] 매칭 요청 전송");

        string currentUserId = SessionData.UserID;
        network.SendPacket(PacketType.RequestMatching, currentUserId);
    }


    // ------------------------------
    //   바이너리 매칭 응답 처리
    // ------------------------------
    private void OnMatchResponseBinary(byte[] body)
    {
        if (body.Length < Marshal.SizeOf<PKT_MATCH_RESPONSE_STRUCT>())
            return;

        PKT_MATCH_RESPONSE_STRUCT pkt =
            NetworkConnector.ByteArrayToStructure<PKT_MATCH_RESPONSE_STRUCT>(body);

        if (pkt.result == 0)
        {
            Debug.Log("[Loading] 매칭 대기 중");
            return;
        }

        if (pkt.result == 1)
        {
            SessionData.RoomID = pkt.roomId;
            SessionData.OpponentID =
                (pkt.userA == SessionData.UserID) ? pkt.userB : pkt.userA;

            SceneManager.LoadScene("GameScene");
        }

        if (pkt.result == 2)
        {
            SceneManager.LoadScene("LobbyScene");
        }
    }

}
