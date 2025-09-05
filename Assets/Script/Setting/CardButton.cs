using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[RequireComponent(typeof(Button))]
public class CardButton : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image iconImage;

    [System.NonSerialized] private CardData data;   // 외부에서만 Init
    public UnityEvent<CardData> onCardClicked = new UnityEvent<CardData>();

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(HandleClick);
    }

    /// <summary>
    /// 외부 매니저에서 카드 데이터를 주입해 초기화
    /// </summary>
    public void Init(CardData cardData)
    {
        data = cardData;
        if (iconImage != null)
            iconImage.sprite = data.cardImage;
    }

    private void HandleClick()
    {
        if (data == null)
        {
            Debug.LogWarning($"{name} 클릭됨 - CardData가 초기화되지 않았음!");
            return;
        }

        Debug.Log($"[CardButton] 카드 클릭됨: {data.cardName} (ID={data.ID})");
        onCardClicked?.Invoke(data);
    }
}
