using UnityEngine;

public class CardGridInitializer : MonoBehaviour
{
    [SerializeField] private CardDatabase database;
    [SerializeField] private CardButton[] cardButtons;
    [SerializeField] private CardPopupPanel popupPanel; // Inspector에서 연결

    private void Start()
    {
        int count = Mathf.Min(cardButtons.Length, database.cards.Length);

        for (int i = 0; i < count; i++)
        {
            var button = cardButtons[i];
            button.Init(database.cards[i]);

            button.onCardClicked.RemoveAllListeners();
            button.onCardClicked.AddListener(card =>
            {
                popupPanel.Show(card, button.transform.position);
            });
        }
    }
}
