using UnityEngine;

[System.Serializable]
public class CardData
{
    [Header("기본 정보")]
    public int ID;                 // 카드 고유 번호
    public string cardName;        // 이름
    public Sprite cardImage;       // 카드 아이콘
    [TextArea] public string description; // 설명

    [Header("리소스")]
    public GameObject playerPrefab;   // 플레이어 발사체 프리팹
    public GameObject enemyPrefab;    // 적 발사체 프리팹

    [Header("효과 옵션")]
    public bool applySlow = false;    // 슬로우 효과 여부
}


public class CardRuntimeData
{
    public int ID;
    public int attack;
    public int cooldown;
    public float projectileSpeed;
    public float radius;
    public float duration;
    public int amount;
}

[CreateAssetMenu(fileName = "CardDatabase", menuName = "ScriptableObjects/CardDatabase")]
public class CardDatabase : ScriptableObject
{
    public CardData[] cards;

    // 편의 메서드: ID로 카드 찾기
    public CardData GetCard(int id)
    {
        foreach (var card in cards)
        {
            if (card.ID == id) return card;
        }
        Debug.LogWarning($"CardDatabase: ID {id}를 찾을 수 없습니다.");
        return null;
    }
}
