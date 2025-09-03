using UnityEngine;
using UnityEngine.UI;

public class MagicSlotUI : MonoBehaviour
{
    [Header("UI 슬롯들 (Q, W, E, R 순서)")]
    public Image[] magicIcons = new Image[4];

    [Header("마법 정보")]
    public MagicLibrary magicLibrary; // 인스펙터에서 할당
    public string[] selectedMagics = new string[4]; // QWER 슬롯에 설정된 마법 이름

    void Start()
    {
        UpdateMagicIcons();
    }

    public void UpdateMagicIcons()
    {
        for (int i = 0; i < selectedMagics.Length && i < magicIcons.Length; i++)
        {
            string magicName = selectedMagics[i];
            MagicData data = magicLibrary.GetMagic(magicName);

            if (data != null)
            {
                Sprite icon = MagicImageManager.Instance.GetMagicSprite(data.magicType);

                if (icon != null)
                    magicIcons[i].sprite = icon;
                else
                    Debug.LogWarning($"스프라이트 없음: {data.magicType}");
            }
            else
            {
                Debug.LogWarning($"마법 정보 없음: {magicName}");
            }
        }
    }
}
