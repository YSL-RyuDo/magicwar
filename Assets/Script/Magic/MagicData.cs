using UnityEngine;

[System.Serializable]
public class MagicData
{
    public string magicName;

    [Header("프리팹")]
    public GameObject playerPrefab;
    public GameObject enemyPrefab;

    [Header("마법 속성")]
    public float projectileSpeed;
    public float cooldown;
    public int damage;

    [Header("특수효과")]
    public bool applySlow;

    [Header("UI용 타입 분류")]
    public MagicType magicType;
}
