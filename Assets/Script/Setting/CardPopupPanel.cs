using UnityEngine;
using UnityEngine.UI;

public class CardPopupPanel : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Button useButton;
    [SerializeField] private Button infoButton;

    private CardData currentCard;
    private SettingSystem settingSystem;

    private void Awake()
    {
        Hide();
        settingSystem = FindFirstObjectByType<SettingSystem>();
    }

    public void Show(CardData card, Vector3 worldPos)
    {
        // 같은 카드 다시 클릭 시 → 닫기
        if (panel.activeSelf && currentCard == card)
        {
            Hide();
            return;
        }

        currentCard = card;
        panel.SetActive(true);

        panel.transform.position = worldPos;

        useButton.onClick.RemoveAllListeners();
        infoButton.onClick.RemoveAllListeners();

        //  Use 버튼 → 슬롯 교체 모드 활성화
        useButton.onClick.AddListener(() =>
        {
            if (settingSystem != null)
                settingSystem.EnterReplaceMode(currentCard.ID);

            Debug.Log($"[Popup] Use {currentCard.cardName} (ID={currentCard.ID}) → 교체 모드 진입");
            Hide();
        });

        //  Info 버튼 → 서버에 카드 상세 요청
        infoButton.onClick.AddListener(() =>
        {
            var network = FindFirstObjectByType<NetworkConnector>();
            if (network != null && network.IsConnected)
            {
                network.SendPacket(PacketType.RequestSetUserMagic, currentCard.ID.ToString());
                Debug.Log($"[Popup] Info {currentCard.cardName} (ID={currentCard.ID}) → 서버 전송");
            }
            Hide();
        });
    }

    public void Hide()
    {
        panel.SetActive(false);
        currentCard = null;
    }
}
