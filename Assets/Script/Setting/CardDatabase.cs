using UnityEngine;

public class CardRuntimeData
{
    public int ID;

    public int attack;
    public float cooldown;
    public int cost;

    public float speed;
    public float radius;
    public float duration;
    public int amount;
}


[CreateAssetMenu(fileName = "CardDatabase", menuName = "ScriptableObjects/CardDatabase")]
public class CardDatabase : ScriptableObject
{
    public CardData[] cards;

    public CardData GetCard(int id)
    {
        foreach (var card in cards)
        {
            if (card.ID == id)
                return card;
        }

        Debug.LogWarning($"[CardDatabase] ID {id} 카드 없음");
        return null;
    }
}
