using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class SettingSystem : MonoBehaviour
{
    [Header("Database & UI")]
    [SerializeField] private CardDatabase cardDatabase;
    [SerializeField] private Image[] slotImages;
    [SerializeField] private Button[] infoButtons;

    [Header("Magic Panel")]
    [SerializeField] private GameObject magicPanel;
    [SerializeField] private Image magicImage;
    [SerializeField] private TMP_Text magicNameText;
    [SerializeField] private TMP_Text magicAttackText;
    [SerializeField] private TMP_Text magicCooldownText;
    [SerializeField] private TMP_Text magicCostText;
    [SerializeField] private TMP_Text magicEffectTypeText;
    [SerializeField] private TMP_Text magicSpeedText;
    [SerializeField] private TMP_Text magicRadiusText;
    [SerializeField] private TMP_Text magicDurationText;
    [SerializeField] private TMP_Text magicAmountText;
    [SerializeField] private TMP_Text magicDescriptionText;
    [SerializeField] private Button closeButton;

    [Header("Navigation")]
    [SerializeField] private Button lobbyButton;

    private bool isReplaceMode = false;
    private int pendingCardId = -1;
    private NetworkConnector network;


    private void Start()
    {
        network = FindFirstObjectByType<NetworkConnector>();

        if (network != null && network.IsConnected)
        {
            Debug.Log("[Setting] 서버에 유저 슬롯 요청");

            network.SendPacket(PacketType.RequestUserMagic, SessionData.UserID);

            // 서버 응답 등록
            network.Dispatcher.Register(PacketType.ResponseUserMagic, OnUserMagicReceived);
            network.Dispatcher.Register(PacketType.ResponseSetUserMagic, OnUserMagicDetailReceived);
            network.Dispatcher.Register(PacketType.ResponseReplaceUserMagic, OnReplaceResult);
        }
        else
        {
            Debug.LogWarning("[Setting] NetworkConnector 없음 or 연결 안됨");
        }

        // 슬롯 클릭 이벤트 등록
        for (int i = 0; i < slotImages.Length; i++)
        {
            int index = i;
            Button btn = slotImages[i].GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => OnSlotClicked(index));
            }
        }

        // 초기에는 패널 비활성화
        magicPanel?.SetActive(false);

        closeButton?.onClick.AddListener(CloseMagicPanel);
        lobbyButton?.onClick.AddListener(GoToLobby);
    }


    // -------------------------------
    //  슬롯 정보 수신
    // -------------------------------
    private void OnUserMagicReceived(byte[] data)
    {
        string msg = System.Text.Encoding.UTF8.GetString(data);

        // 예: "0,1,4,7"
        var slots = msg.Split(',');
        if (slots.Length < slotImages.Length)
        {
            Debug.LogError("[Setting] 잘못된 슬롯 수신: " + msg);
            return;
        }

        for (int i = 0; i < slotImages.Length; i++)
        {
            if (int.TryParse(slots[i], out int cardId))
                UpdateSlot(i, cardId);
            else
                ClearSlot(i);
        }
    }


    private void UpdateSlot(int index, int cardId)
    {
        CardData card = System.Array.Find(cardDatabase.cards, c => c.ID == cardId);

        if (card != null)
        {
            slotImages[index].enabled = true;
            slotImages[index].sprite = card.cardImage;

            infoButtons[index].gameObject.SetActive(true);
            infoButtons[index].onClick.RemoveAllListeners();
            infoButtons[index].onClick.AddListener(() =>
            {
                network.SendPacket(PacketType.RequestSetUserMagic, cardId.ToString());
            });
        }
        else
        {
            ClearSlot(index);
        }
    }

    private void ClearSlot(int index)
    {
        slotImages[index].sprite = null;
        slotImages[index].enabled = false;

        if (index < infoButtons.Length)
            infoButtons[index].gameObject.SetActive(false);
    }


    // -------------------------------
    //  카드 상세 보기
    // -------------------------------
    private void OnUserMagicDetailReceived(byte[] data)
    {
        // byte[] → string 변환
        string msg = System.Text.Encoding.UTF8.GetString(data);

        // 서버 예시: "11,MagicWall,0,8,5,Wall,0,4,8,200"
        var p = msg.Split(',');
        if (p.Length < 10)
        {
            Debug.LogError("[Setting] 잘못된 상세 응답: " + msg);
            return;
        }

        int id = int.Parse(p[0]);
        CardData card = System.Array.Find(cardDatabase.cards, c => c.ID == id);

        magicNameText.text = p[1];
        magicAttackText.text = $"Attack: {p[2]}";
        magicCooldownText.text = $"Cooldown: {p[3]}";
        magicCostText.text = $"Cost: {p[4]}";
        magicEffectTypeText.text = p[5];
        magicSpeedText.text = $"Speed: {p[6]}";
        magicRadiusText.text = $"Radius: {p[7]}";
        magicDurationText.text = $"Duration: {p[8]}";
        magicAmountText.text = $"Amount: {p[9]}";

        if (card != null)
        {
            magicImage.sprite = card.cardImage;
            magicDescriptionText.text = card.description;
        }

        magicPanel.SetActive(true);
    }



    // -------------------------------
    //  교체 모드
    // -------------------------------
    public void EnterReplaceMode(int cardId)
    {
        isReplaceMode = true;
        pendingCardId = cardId;

        Debug.Log($"[Setting] 교체 모드 ON → ID={cardId}");
    }

    public void OnSlotClicked(int slotIndex)
    {
        if (!isReplaceMode) return;

        CardData oldCard = null;
        if (slotImages[slotIndex].sprite != null)
            oldCard = System.Array.Find(cardDatabase.cards, c => c.cardImage == slotImages[slotIndex].sprite);

        int oldCardId = oldCard != null ? oldCard.ID : -1;

        string payload = $"{SessionData.UserID},{oldCardId},{pendingCardId}";
        network.SendPacket(PacketType.RequestReplaceUserMagic, payload);

        isReplaceMode = false;
        pendingCardId = -1;
    }


    // -------------------------------
    //  서버 교체 응답
    // -------------------------------
    private void OnReplaceResult(byte[] data)
    {
        string msg = System.Text.Encoding.UTF8.GetString(data);

        Debug.Log("[Setting] 교체 응답 수신: " + msg);

        // 필요하면 여기서 교체 성공/실패 처리 가능
        // 예: if (msg == "OK") { ... }
    }



    public void CloseMagicPanel()
    {
        magicPanel.SetActive(false);
    }

    public void GoToLobby()
    {
        SceneManager.LoadScene("LobbyScene");
    }

    private void OnDestroy()
    {
        if (network != null)
        {
            network.Dispatcher.Unregister(PacketType.ResponseUserMagic, OnUserMagicReceived);
            network.Dispatcher.Unregister(PacketType.ResponseSetUserMagic, OnUserMagicDetailReceived);
            network.Dispatcher.Unregister(PacketType.ResponseReplaceUserMagic, OnReplaceResult);
        }
    }
}
