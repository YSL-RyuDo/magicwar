using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUIController : MonoBehaviour
{
    public static GameUIController Instance { get; private set; }

    [Header("Magic Buttons (Q W E R)")]
    [SerializeField] private Button[] magicButtons = new Button[4];
    

    [Header("Magic Icons")]
    [SerializeField] private Image[] magicIcons = new Image[4];
    [SerializeField] private Image[] cooldownMasks;

    [Header("Card Database")]
    [SerializeField] private CardDatabase cardDB;

    [Header("HP UI")]
    [SerializeField] private Slider hpSlider;
    [SerializeField] private TextMeshProUGUI hpText;

    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private GameObject defeatPanel;
    private int lastHp = -1;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        for (int i = 0; i < magicButtons.Length; i++)
        {
            int idx = i;
            magicButtons[i].onClick.AddListener(
                () => GameManager.Instance.OnMagicButtonPressed(idx)
            );
        }
    }

    private void Update()
    {
        UpdateCooldownUI();
    }

    private void UpdateCooldownUI()
    {
        for (int i = 0; i < 4; i++)
        {
            float remain =
                GameManager.Instance.GetCooldownRemain(i);

            float total =
                GameManager.Instance.GetCooldownTotal(i);

            cooldownMasks[i].fillAmount =
                total > 0f ? remain / total : 0f;
        }
    }

    public void RefreshMagicSlots()
    {
        var slots = GameManager.Instance.EquippedMagicIds;

        for (int i = 0; i < 4; i++)
        {
            CardData card = FindCard(slots[i]);

            if (card != null)
            {
                // 아이콘 설정
                magicIcons[i].sprite = card.cardImage;
                magicIcons[i].enabled = true;

                // 쿨다운 마스크에도 같은 이미지 넣기
                cooldownMasks[i].sprite = card.cardImage;
                cooldownMasks[i].enabled = true;

                // 초기엔 쿨타임 없음
                cooldownMasks[i].fillAmount = 0f;
            }
            else
            {
                magicIcons[i].enabled = false;
                cooldownMasks[i].enabled = false;
            }
        }

        Debug.Log("[GameUI] 마법 슬롯 + 쿨다운 마스크 갱신 완료");
    }


    private CardData FindCard(int id)
    {
        if (cardDB == null) return null;

        foreach (var card in cardDB.cards)
        {
            if (card != null && card.ID == id)
                return card;
        }
        return null;
    }

    public void UpdateHP(int current, int max)
    {
        // 최초 호출
        if (lastHp < 0)
            lastHp = current;

        if (current < lastHp)
        {
            CharacterController2D controller =
                FindFirstObjectByType<CharacterController2D>();

            //controller?.PlayHurt();
        }

        // UI 갱신
        hpSlider.maxValue = max;
        hpSlider.value = current;
        hpText.text = $"{current} / {max}";

        lastHp = current;
    }


    public void ShowVictoryUI()
    {
        victoryPanel.SetActive(true);
    }

    public void ShowDefeatUI()
    {
        defeatPanel.SetActive(true);
    }

    public void OnClickReturnToLobby()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene("LobbyScene");
    }
}
