using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingSystem : MonoBehaviour
{
    private string roomId;
    private string myId;
    private string opponentId;

    [SerializeField] private Button cancelButton;
    private NetworkConnector network;

    private void Start()
    {
        network = FindFirstObjectByType<NetworkConnector>();

        if (cancelButton != null)
            cancelButton.onClick.AddListener(OnClickCancelButton);

        if (network != null)
            network.Dispatcher.Register(PacketType.ResponseMatching, OnMatchResponse);
    }

    private void OnClickCancelButton()
    {
        Debug.Log("[Loading] 매칭 취소 → 로비로 복귀");
        string currentUserId = SessionData.UserID;
        network.SendPacket(PacketType.CancelMatch, currentUserId);

        SceneManager.LoadScene("LobbyScene");
    }

    private void OnMatchResponse(string msg)
    {
        Debug.Log("[Loading] 매칭 응답: " + msg);

        if (msg.StartsWith("OK"))
        {
            // OK,Room123,qwe,asd
            string[] tokens = msg.Split(',');
            if (tokens.Length >= 4)
            {
                string roomId = tokens[1];
                string userA = tokens[2];
                string userB = tokens[3];

                string myId = SessionData.UserID; // 이미 로그인 시점에 세팅됨
                string opponentId = (userA == myId) ? userB : userA;

                // 세션 저장
                SessionData.RoomID = roomId;
                SessionData.OpponentID = opponentId;

                Debug.Log($"[Loading] 방 ID={roomId}, 내 ID={myId}, 상대 ID={opponentId}");
            }

            // 매칭 성공 → GameScene으로 이동
            SceneManager.LoadScene("GameScene");
        }
        else if (msg == "WAIT")
        {
            Debug.Log("[Loading] 매칭 대기 중...");
        }
    }

}
