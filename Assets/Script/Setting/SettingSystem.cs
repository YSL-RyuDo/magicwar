using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class SettingSystem : MonoBehaviour
{
    [Header("Database & UI")]
    [SerializeField] private CardDatabase cardDatabase;
    [SerializeField] private Image[] slotImages;   // 슬롯별 아이콘
    [SerializeField] private Button[] infoButtons; // 슬롯별 InfoButton

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
            Debug.Log("[Setting] 서버에 유저 마법 슬롯 요청");
            string currentUserId = SessionData.UserID;
            network.SendPacket(PacketType.RequestUserMagic, currentUserId);

            // 서버 응답 이벤트 등록
            network.Dispatcher.Register(PacketType.ResponseUserMagic, OnUserMagicReceived);
            network.Dispatcher.Register(PacketType.ResponseSetUserMagic, OnUserMagicDetailReceived);
        }
        else
        {
            Debug.LogWarning("[Setting] 네트워크 연결이 없거나 NetworkConnector를 찾지 못했습니다.");
        }

        for (int i = 0; i < slotImages.Length; i++)
        {
            int index = i;
            Button slotBtn = slotImages[i].GetComponent<Button>();
            if (slotBtn != null)
            {
                slotBtn.onClick.RemoveAllListeners();
                slotBtn.onClick.AddListener(() => OnSlotClicked(index));
            }
        }

        // 패널은 처음에 꺼두기
        if (magicPanel != null)
            magicPanel.SetActive(false);

        if (closeButton != null)
            closeButton.onClick.AddListener(CloseMagicPanel);

        if (lobbyButton != null)
            lobbyButton.onClick.AddListener(GoToLobby);
    }

    private void OnUserMagicReceived(string msg)
    {
        // 서버 응답 예: "0,1,2,3"
        string[] slots = msg.Split(',');
        if (slots.Length < 4)
        {
            Debug.LogError("[Setting] 잘못된 마법 슬롯 응답: " + msg);
            return;
        }

        Debug.Log($"[Setting] 슬롯 수신 완료: {string.Join(", ", slots)}");

        for (int i = 0; i < slotImages.Length; i++)
        {
            if (i >= slots.Length) break;

            if (int.TryParse(slots[i], out int cardId))
                UpdateSlot(i, cardId);
            else
                ClearSlot(i);
        }
    }

    /// 슬롯에 카드 적용
    private void UpdateSlot(int index, int cardId)
    {
        CardData card = System.Array.Find(cardDatabase.cards, c => c.ID == cardId);

        if (card != null && card.cardImage != null)
        {
            slotImages[index].sprite = card.cardImage;
            slotImages[index].enabled = true;

            Debug.Log($"[Setting] 슬롯 {index} → {card.cardName} (ID={card.ID})");

            if (index < infoButtons.Length && infoButtons[index] != null)
            {
                infoButtons[index].gameObject.SetActive(true);

                infoButtons[index].onClick.RemoveAllListeners();
                infoButtons[index].onClick.AddListener(() =>
                {
                    OnInfoButtonClicked(card.ID);
                });
            }
        }
        else
        {
            ClearSlot(index);
        }
    }

    /// 슬롯 초기화
    private void ClearSlot(int index)
    {
        if (index < slotImages.Length && slotImages[index] != null)
            slotImages[index].enabled = false;

        if (index < infoButtons.Length && infoButtons[index] != null)
            infoButtons[index].gameObject.SetActive(false);
    }

    /// InfoButton → 서버 요청
    private void OnInfoButtonClicked(int cardId)
    {
        Debug.Log($"[Setting] InfoButton 클릭 → 서버 전송: CardID={cardId}");
        network.SendPacket(PacketType.RequestSetUserMagic, cardId.ToString());
    }

    /// 서버 응답으로 카드 상세 정보 받았을 때
    /// 예: "11,MagicWall,0,8,5,Wall,0,4,8,200"
    private void OnUserMagicDetailReceived(string msg)
    {
        string[] parts = msg.Split(',');
        if (parts.Length < 10)
        {
            Debug.LogError("[Setting] 잘못된 마법 상세 응답: " + msg);
            return;
        }

        int cardId = int.Parse(parts[0]);
        string name = parts[1];

        // UI 업데이트
        magicNameText.text = $"{name}";
        magicAttackText.text = $"Attack: {parts[2]}";
        magicCooldownText.text = $"Cooldown: {parts[3]}";
        magicCostText.text = $"Cost: {parts[4]}";
        magicEffectTypeText.text = $"{parts[5]}";
        magicSpeedText.text = $"Speed: {parts[6]}";
        magicRadiusText.text = $"Radius: {parts[7]}";
        magicDurationText.text = $"Duration: {parts[8]}";
        magicAmountText.text = $"Amount: {parts[9]}";


        // 카드 데이터베이스에서 이미지/설명 가져오기
        CardData card = System.Array.Find(cardDatabase.cards, c => c.ID == cardId);
        if (card != null)
        {
            magicImage.sprite = card.cardImage;
            magicDescriptionText.text = card.description;
        }

        // 패널 열기
        magicPanel.SetActive(true);

        Debug.Log($"[Setting] MagicPanel 열림 → {msg}");
    }

    /// Close 버튼 눌렀을 때
    public void CloseMagicPanel()
    {
        if (magicPanel != null)
        {
            magicPanel.SetActive(false);

            // 텍스트 기본값으로 초기화
            magicNameText.text = "-";
            magicAttackText.text = "Attack: -";
            magicCooldownText.text = "Cooldown: -";
            magicCostText.text = "Cost: -";
            magicEffectTypeText.text = "-";
            magicSpeedText.text = "Speed: -";
            magicRadiusText.text = "Radius: -";
            magicDurationText.text = "Duration: -";
            magicAmountText.text = "Amount: -";
            magicDescriptionText.text = "-";

            magicImage.sprite = null;

            Debug.Log("[Setting] MagicPanel 닫힘");
        }
    }

    public void EnterReplaceMode(int cardId)
    {
        // 이미 슬롯에 존재하는 카드라면 교체 불가
        foreach (var img in slotImages)
        {
            if (img.sprite != null)
            {
                CardData card = System.Array.Find(cardDatabase.cards, c => c.cardImage == img.sprite);
                if (card != null && card.ID == cardId)
                {
                    Debug.LogWarning("[Setting] 이미 슬롯에 존재하는 카드입니다. 교체 불가.");
                    return;
                }
            }
        }

        isReplaceMode = true;
        pendingCardId = cardId;
        Debug.Log($"[Setting] 교체 모드 진입 → 카드 ID={cardId}");
    }

    public void OnSlotClicked(int slotIndex)
    {
        if (!isReplaceMode || pendingCardId < 0)
        {
            Debug.Log("[Setting] 교체 모드가 아님 → 무시");
            return;
        }

        // 슬롯의 기존 카드 찾기
        CardData oldCard = null;
        if (slotImages[slotIndex].sprite != null)
        {
            oldCard = System.Array.Find(cardDatabase.cards, c => c.cardImage == slotImages[slotIndex].sprite);
        }

        int oldCardId = oldCard != null ? oldCard.ID : -1;

        // 같은 카드로 교체 시 무시
        if (oldCardId == pendingCardId)
        {
            Debug.Log("[Setting] 동일 카드로 교체 불가");
            isReplaceMode = false;
            pendingCardId = -1;
            return;
        }

        // 유저 ID 가져오기
        string currentUserId = SessionData.UserID;

        Debug.Log($"[Setting] 슬롯 {slotIndex} 교체 요청 → 기존:{oldCardId}, 새:{pendingCardId}");

        // 서버에 패킷 전송 (UserID, 기존카드ID, 새카드ID)
        string payload = $"{currentUserId},{oldCardId},{pendingCardId}";
        network.SendPacket(PacketType.RequestReplaceUserMagic, payload);

        // 클라이언트 UI 즉시 반영 (서버 응답으로 최종 확인 가능)
        CardData newCard = System.Array.Find(cardDatabase.cards, c => c.ID == pendingCardId);
        if (newCard != null)
        {
            slotImages[slotIndex].sprite = newCard.cardImage;
            slotImages[slotIndex].enabled = true;
        }

        // 교체 모드 종료
        isReplaceMode = false;
        pendingCardId = -1;
        Debug.Log("[Setting] 교체 모드 종료");
    }

    public void GoToLobby()
    {
        Debug.Log("[Setting] 로비 버튼 클릭 → LobbyScene 이동");
        UnityEngine.SceneManagement.SceneManager.LoadScene("LobbyScene");
    }

    private void OnDestroy()
    {
        if (network != null)
        {
            network.Dispatcher.Unregister(PacketType.ResponseUserMagic, OnUserMagicReceived);
            network.Dispatcher.Unregister(PacketType.ResponseSetUserMagic, OnUserMagicDetailReceived);
        }
    }
}
