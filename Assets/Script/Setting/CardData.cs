using UnityEngine;

[System.Serializable]

[CreateAssetMenu(fileName = "CardData", menuName = "Card/CardData")]
public class CardData : ScriptableObject
{
    [Header("기본 정보")]
    public int ID;
    public string cardName;
    public Sprite cardImage;
    [TextArea] public string description;

    [Header("발사체 프리팹")]
    public GameObject playerPrefab;
    public GameObject enemyPrefab;

    [Header("효과 목록")]
    public MagicEffect[] effects;

    [Header("효과음")]
    public AudioClip audioClip;
}