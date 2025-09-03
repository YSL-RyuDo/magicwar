using UnityEngine;
using System.Collections.Generic;

public class MagicImageManager : MonoBehaviour
{
    public static MagicImageManager Instance { get; private set; }

    [System.Serializable]
    public struct MagicSpriteEntry
    {
        public MagicType magicType;
        public Sprite magicSprite;
    }

    [Header("마법 이미지 목록")]
    public List<MagicSpriteEntry> magicSprites;

    private Dictionary<MagicType, Sprite> spriteDictionary;

    private void Awake()
    {
        // 싱글턴 초기화
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeDictionary();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeDictionary()
    {
        spriteDictionary = new Dictionary<MagicType, Sprite>();
        foreach (var entry in magicSprites)
        {
            if (!spriteDictionary.ContainsKey(entry.magicType))
            {
                spriteDictionary.Add(entry.magicType, entry.magicSprite);
            }
            else
            {
                Debug.LogWarning($"중복된 MagicType: {entry.magicType}");
            }
        }
    }

    public Sprite GetMagicSprite(MagicType type)
    {
        if (spriteDictionary.TryGetValue(type, out Sprite sprite))
        {
            return sprite;
        }

        Debug.LogWarning($"MagicType {type}에 해당하는 스프라이트가 없습니다.");
        return null;
    }
}
