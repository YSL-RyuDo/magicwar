using UnityEngine;
using UnityEngine.UI;
using TMPro; // 텍스트를 TextMeshPro로 쓸 경우

public class CharacterHealthUI : MonoBehaviour
{
    [Header("대상 캐릭터")]
    public CharacterHealth target;

    [Header("UI 구성")]
    public Slider healthSlider;
    public TextMeshProUGUI healthText; // 일반 Text 쓸 거면 Text로 변경

    private void Start()
    {
        if (target == null)
        {
            Debug.LogError("CharacterHealthUI: 타겟이 설정되지 않았습니다.");
            enabled = false;
            return;
        }

        UpdateUI();
    }

    private void OnEnable()
    {
        if (target != null)
        {
            target.onTakeDamage.AddListener(UpdateUI);
            target.onHeal.AddListener(UpdateUI);
            target.onDeath.AddListener(UpdateUI);
        }
    }

    private void OnDisable()
    {
        if (target != null)
        {
            target.onTakeDamage.RemoveListener(UpdateUI);
            target.onHeal.RemoveListener(UpdateUI);
            target.onDeath.RemoveListener(UpdateUI);
        }
    }

    public void UpdateUI()
    {
        if (target == null || healthSlider == null) return;

        float ratio = (float)target.CurrentHealth / target.MaxHealth;
        healthSlider.value = ratio;

        if (healthText != null)
            healthText.text = $"{target.CurrentHealth} / {target.MaxHealth}";
    }

}
