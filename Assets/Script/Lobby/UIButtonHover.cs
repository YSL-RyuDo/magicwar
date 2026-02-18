using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public float hoverScale = 1.2f;

    public float hoverRotation = 0f;   // 0이면 회전 안함
    public float rotationSpeed = 10f;  // 부드러운 회전 속도

    private Vector3 originalScale;
    private Quaternion originalRotation;
    private Quaternion targetRotation;

    void Start()
    {
        originalScale = transform.localScale;
        originalRotation = transform.localRotation;
        targetRotation = originalRotation;
    }

    void Update()
    {
        // hoverRotation이 0이 아닐 때만 회전 처리
        if (hoverRotation != 0f)
        {
            transform.localRotation = Quaternion.Lerp(
                transform.localRotation,
                targetRotation,
                Time.deltaTime * rotationSpeed
            );
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localScale = originalScale * hoverScale;

        if (hoverRotation != 0f)
        {
            targetRotation = Quaternion.Euler(0, 0, hoverRotation);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = originalScale;

        if (hoverRotation != 0f)
        {
            targetRotation = originalRotation;
        }
    }
}
