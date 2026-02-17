using UnityEngine;

public class CreditScroller : MonoBehaviour
{
    public float scrollSpeed = 50f;     // 올라가는 속도
    public float startPositionY = -600f; // 시작 위치 (화면 아래)
    public float endPositionY = 2000f;  // 종료 위치 (화면 위 끝)

    public GameObject wizard;
    public GameObject creditPanel;

    private bool isScrolling = false;
    private RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    // 버튼을 눌렀을 때 호출될 함수
    public void StartCredits()
    {
        // 위치 초기화 및 재생 시작
        rectTransform.anchoredPosition = new Vector2(0, startPositionY);
        isScrolling = true;
    }

    void Update()
    {
        if (isScrolling)
        {
            // 위로 이동
            rectTransform.anchoredPosition += Vector2.up * scrollSpeed * Time.deltaTime;

            // 끝 지점에 도달하면 정지
            if (rectTransform.anchoredPosition.y >= endPositionY)
            {
                wizard.SetActive(true);
                isScrolling = false;
                creditPanel.SetActive(false);
            }
        }
    }

    // 필요 시 정지 함수
    public void StopCredits()
    {
        isScrolling = false;
    }
}