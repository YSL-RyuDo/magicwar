using UnityEngine;
using UnityEngine.UI;

public class MagicHotkeyInput : MonoBehaviour
{
    [Header("마법 버튼들 (순서: Q, W, E, R)")]
    public Button[] magicButtons = new Button[4];

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            TriggerMagic(0, "Q");
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            TriggerMagic(1, "W");
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            TriggerMagic(2, "E");
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            TriggerMagic(3, "R");
        }
    }

    private void TriggerMagic(int index, string key)
    {
        if (index < 0 || index >= magicButtons.Length)
        {
            Debug.LogWarning($"잘못된 인덱스 {index}");
            return;
        }

        var button = magicButtons[index];
        if (button != null)
        {
            Debug.Log($"{key} 키 누름 → {button.name} 클릭됨");
            button.onClick.Invoke(); // 버튼 클릭된 것처럼 동작
        }
        else
        {
            Debug.LogWarning($"{key} 키에 해당하는 버튼이 설정되지 않음");
        }
    }
}
