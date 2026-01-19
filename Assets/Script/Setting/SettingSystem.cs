using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Runtime.InteropServices;
using System;

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

    void Awake()
    {
        Screen.SetResolution(576, 864, false);
    }
    private void Start()
    {
        network = FindFirstObjectByType<NetworkConnector>();

        if (network != null && network.IsConnected)
        {
            Debug.Log("[Setting] 서버에 유저 슬롯 요청");

            network.SendPacket(PacketType.RequestUserMagic, SessionData.UserID);

            // 서버 응답 등록
            network.Dispatcher.Register(PacketType.ResponseUserMagic, OnUserMagicReceived);
            network.Dispatcher.Register(PacketType.ResponseMagicDetail, OnUserMagicDetailReceived);
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
        PKT_RES_USER_MAGIC pkt =
            NetworkConnector.ByteArrayToStructure<PKT_RES_USER_MAGIC>(data);

        for (int i = 0; i < pkt.slotCount; i++)
        {
            UpdateSlot(i, pkt.cardIds[i]);
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
                // 바이너리 요청 구조체 생성
                PKT_REQ_MAGIC_DETAIL req = new PKT_REQ_MAGIC_DETAIL
                {
                    cardId = cardId
                };

                // 구조체 → byte[]
                byte[] body = NetworkConnector.StructToBytes(req);

                // 전송
                network.SendPacket(PacketType.RequestMagicDetail, body);

                Debug.Log($"[Setting] MagicDetail 요청 cardId={cardId}");
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
        PKT_RES_MAGIC_DETAIL pkt =
            NetworkConnector.ByteArrayToStructure<PKT_RES_MAGIC_DETAIL>(data);

        magicNameText.text = pkt.name;
        magicAttackText.text = $"Attack: {pkt.attack}";
        magicCooldownText.text = $"Cooldown: {pkt.cooldown}";
        magicCostText.text = $"Cost: {pkt.cost}";
        magicEffectTypeText.text = pkt.effectType;
        magicSpeedText.text = $"Speed: {pkt.speed}";
        magicRadiusText.text = $"Radius: {pkt.radius}";
        magicDurationText.text = $"Duration: {pkt.duration}";
        magicAmountText.text = $"Amount: {pkt.amount}";

        CardData card = System.Array.Find(cardDatabase.cards, c => c.ID == pkt.id);
        if (card != null)
        {
            magicImage.sprite = card.cardImage;
            magicDescriptionText.text = card.description;
        }

        magicPanel.SetActive(true);
    }

    //  교체 모드
    public void EnterReplaceMode(int cardId)
    {
        isReplaceMode = true;
        pendingCardId = cardId;

        Debug.Log($"[Setting] 교체 모드 ON → ID={cardId}");
    }

    public void OnSlotClicked(int slotIndex)
    {
        if (!isReplaceMode) return;

        int oldCardId = -1;
        if (slotImages[slotIndex].sprite != null)
        {
            CardData oldCard =
                System.Array.Find(cardDatabase.cards,
                    c => c.cardImage == slotImages[slotIndex].sprite);

            if (oldCard != null)
                oldCardId = oldCard.ID;
        }

        PKT_REQ_REPLACE_MAGIC req = new PKT_REQ_REPLACE_MAGIC
        {
            oldCardId = oldCardId,
            newCardId = pendingCardId
        };

        network.SendPacket(PacketType.RequestReplaceUserMagic, NetworkConnector.StructToBytes(req));


        isReplaceMode = false;
        pendingCardId = -1;
    }

    //  서버 교체 응답
    private void OnReplaceResult(byte[] data)
    {
        if (data.Length < Marshal.SizeOf<PKT_RES_REPLACE_USER_MAGIC>())
        {
            Debug.LogError("[Setting] 교체 응답 패킷 크기 오류");
            return;
        }

        PKT_RES_REPLACE_USER_MAGIC res =
            NetworkConnector.ByteArrayToStructure<PKT_RES_REPLACE_USER_MAGIC>(data);

        Debug.Log("[Setting] 교체 결과 = " + res.result);

        if (res.result == 1)
        {
            Debug.Log("[Setting] 교체 성공 → 슬롯 재요청");

            // 여기서 다시 슬롯 요청
            network.SendPacket(PacketType.RequestUserMagic, Array.Empty<byte>());
        }
        else
        {
            Debug.LogWarning("[Setting] 교체 실패");
        }
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
            network.Dispatcher.Unregister(PacketType.ResponseMagicDetail, OnUserMagicDetailReceived);
            network.Dispatcher.Unregister(PacketType.ResponseReplaceUserMagic, OnReplaceResult);
        }
    }
}
