using UnityEngine;

public class CloudMovement : MonoBehaviour
{
    [SerializeField] private RectTransform cloudImage1; // 첫 번째 구름 이미지
    [SerializeField] private RectTransform cloudImage2; // 두 번째 구름 이미지
    [SerializeField] private float scrollSpeed = 100f; // 구름 스크롤 속도
    [SerializeField] private float resetPosition = 1920f; // 이미지가 리셋되는 위치 (이미지 너비)

    void Update()
    {
        // 구름 이미지 이동
        ScrollCloud(cloudImage1);
        ScrollCloud(cloudImage2);
    }

    private void ScrollCloud(RectTransform cloudImage)
    {
        // 구름 이미지를 왼쪽으로 이동
        cloudImage.anchoredPosition += Vector2.left * scrollSpeed * Time.deltaTime;

        // 이미지가 화면 밖으로 벗어나면 위치를 초기화
        if (cloudImage.anchoredPosition.x <= -resetPosition)
        {
            cloudImage.anchoredPosition += new Vector2(resetPosition * 2, 0);
        }
    }
}
