using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class LobbyUIController : MonoBehaviour
{
    [Header("Profile UI")]
    [SerializeField] private TMP_Text nicknameText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text rankNameText;
    [SerializeField] private TMP_Text rankScoreText;
    [SerializeField] private Image rankImage;
    [SerializeField] private Slider rankScoreSlider;
    [SerializeField] private Sprite[] rankIcons;

    [Header("Chat UI")]
    [SerializeField] private TMP_InputField chatInput;
    [SerializeField] private Button chatSendButton;
    [SerializeField] private Transform chatContent;
    [SerializeField] private GameObject chatTextPrefab;
    [SerializeField] private ScrollRect chatScroll;

    [Header("Logout UI")]
    [SerializeField] private GameObject logoutPanel;
    [SerializeField] private Button logoutButton;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private GameObject playerObject;

    [Header("Other Buttons")]
    [SerializeField] private Button settingButton;
    [SerializeField] private Button startButton;

    private LobbyService service;

    private void Awake()
    {
        service = FindFirstObjectByType<LobbyService>();
    }

    private void Start()
    {
        logoutPanel.SetActive(false);

        chatSendButton.onClick.AddListener(OnClickSendChat);
        logoutButton.onClick.AddListener(OnClickLogout);
        confirmButton.onClick.AddListener(OnClickConfirm);
        cancelButton.onClick.AddListener(OnClickCancel);

        settingButton?.onClick.AddListener(() =>
            SceneManager.LoadScene("SettingScene"));

        startButton?.onClick.AddListener(() =>
            SceneManager.LoadScene("LoadingScene"));
    }

    private void Update()
    {
        if (chatInput.isFocused && Input.GetKeyDown(KeyCode.Return))
            OnClickSendChat();
    }

    // ----------------------------
    // Profile UI
    // ----------------------------
    public void ApplyProfile(PKT_RES_PROFILE_DATA profile)
    {
        nicknameText.text = profile.nickname;
        levelText.text = $"Lv. {profile.level}";
        rankNameText.text = profile.rankName;
        rankScoreText.text = profile.rankScore.ToString();

        if (profile.rank < rankIcons.Length)
            rankImage.sprite = rankIcons[profile.rank];

        rankScoreSlider.minValue = profile.minScore;
        rankScoreSlider.maxValue = profile.maxScore;
        rankScoreSlider.value = profile.rankScore;
    }

    // ----------------------------
    // Chat UI
    // ----------------------------
    private void OnClickSendChat()
    {
        string msg = chatInput.text.Trim();
        chatInput.text = "";

        service.SendChat(msg);
    }

    public void AddChat(PKT_CHAT_LOBBY_DATA chat)
    {
        GameObject go = Instantiate(chatTextPrefab, chatContent);
        TMP_Text txt = go.GetComponent<TMP_Text>();
        txt.text = $"{chat.nickname} : {chat.message}";

        Canvas.ForceUpdateCanvases();
        chatScroll.verticalNormalizedPosition = 0f;
    }

    // ----------------------------
    // Logout
    // ----------------------------
    private void OnClickLogout()
    {
        logoutPanel.SetActive(true);
        if (playerObject) playerObject.SetActive(false);
    }

    private void OnClickConfirm()
    {
        NetworkConnector.Instance.SendPacket(PacketType.Logout, System.Array.Empty<byte>());
        SceneManager.LoadScene("LoginScene");
    }

    private void OnClickCancel()
    {
        logoutPanel.SetActive(false);
        if (playerObject) playerObject.SetActive(true);
    }
}
