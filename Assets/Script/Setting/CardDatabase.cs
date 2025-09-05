using UnityEngine;

[System.Serializable]
public class CardData
{
    public int ID;
    public string cardName;
    public Sprite cardImage;
    [TextArea]
    public string description;
}

[CreateAssetMenu(fileName = "CardData", menuName = "ScriptableObjects/CardData")]
public class CardDatabase : ScriptableObject
{
    public CardData[] cards;
}