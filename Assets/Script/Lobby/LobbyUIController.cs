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

    [Header("Logout Emotion Image")]
    [SerializeField] private Image logoutEmotionImage;
    [SerializeField] private Sprite defaultSprite;
    [SerializeField] private Sprite cryingSprite;
    [SerializeField] private Sprite smilingSprite;

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

        AddHoverEvent(confirmButton, cryingSprite);
        AddHoverEvent(cancelButton, smilingSprite);
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

        if (logoutEmotionImage != null)
            logoutEmotionImage.sprite = defaultSprite;

        if (playerObject)
            playerObject.SetActive(false);
    }


    private void OnClickConfirm()
    {
        NetworkConnector.Instance.ResetToken();
        NetworkConnector.Instance.SendPacket(PacketType.Logout, System.Array.Empty<byte>());

        Application.Quit();
    }

    private void OnClickCancel()
    {
        logoutPanel.SetActive(false);
        if (playerObject) playerObject.SetActive(true);
    }

    private void AddHoverEvent(Button btn, Sprite hoverSprite)
    {
        EventTrigger trigger = btn.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = btn.gameObject.AddComponent<EventTrigger>();

        trigger.triggers ??= new System.Collections.Generic.List<EventTrigger.Entry>();

        // Mouse Enter
        EventTrigger.Entry enter = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerEnter
        };
        enter.callback.AddListener(_ =>
        {
            if (logoutEmotionImage != null)
                logoutEmotionImage.sprite = hoverSprite;
        });

        // Mouse Exit
        EventTrigger.Entry exit = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerExit
        };
        exit.callback.AddListener(_ =>
        {
            if (logoutEmotionImage != null)
                logoutEmotionImage.sprite = defaultSprite;
        });

        trigger.triggers.Add(enter);
        trigger.triggers.Add(exit);
    }

}
