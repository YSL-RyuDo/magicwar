using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LobbySystem : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text nicknameText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private Image rankImage;
    [SerializeField] private TMP_Text rankScoreText;
    [SerializeField] private Slider rankScoreSlider;

    [Header("Rank Icons")]
    [SerializeField] private Sprite[] rankIcons;

    [Header("Buttons")]
    [SerializeField] private Button settingButton;

    private NetworkConnector network;

    private void Start()
    {
        network = FindFirstObjectByType<NetworkConnector>();

        if (network != null && network.IsConnected)
        {
            string currentUserId = PlayerPrefs.GetString("UserID", "guest");

            Debug.Log("[Lobby] 서버에 프로필 요청 패킷 전송");
            network.SendPacket(PacketType.RequestProfile, currentUserId);
            network.Dispatcher.Register(PacketType.ResponseProfile, OnProfileReceived);
        }
        else
        {
            Debug.LogWarning("[Lobby] 네트워크 연결이 없거나 NetworkConnector를 찾지 못했습니다.");
        }

        if (settingButton != null)
            settingButton.onClick.AddListener(OnClickSettingButton);
    }

    private void OnProfileReceived(string msg)
    {
        // 서버 응답: "닉네임,레벨,랭크ID,랭크점수,랭크MIN,랭크MAX"
        string[] parts = msg.Split(',');
        if (parts.Length < 6)
        {
            Debug.LogError("[Lobby] 프로필 데이터 파싱 실패: " + msg);
            return;
        }

        string nickname = parts[0];
        string level = parts[1];
        int rankId = int.Parse(parts[2]);
        int rankScore = int.Parse(parts[3]);
        int rankMin = int.Parse(parts[4]);
        int rankMax = int.Parse(parts[5]);

        // UI 업데이트
        nicknameText.text = nickname;
        levelText.text = $"Lv. {level}";
        rankScoreText.text = $"{rankScore}";

        if (rankId >= 0 && rankId < rankIcons.Length)
            rankImage.sprite = rankIcons[rankId];

        // 슬라이더 설정
        rankScoreSlider.minValue = rankMin;
        rankScoreSlider.maxValue = rankMax;
        rankScoreSlider.value = rankScore;

        Debug.Log($"[Lobby] 프로필 적용 완료 → {nickname}, Lv.{level}, RankID:{rankId}, Score:{rankScore}/{rankMin}-{rankMax}");
    }

    private void OnClickSettingButton()
    {
        Debug.Log("[Lobby] 세팅 버튼 클릭됨 → Setting 씬 로드");
        SceneManager.LoadScene("SettingScene");
    }
}
