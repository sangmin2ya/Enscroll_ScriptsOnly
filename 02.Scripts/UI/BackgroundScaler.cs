using UnityEngine;

public class BackgroundScaler : MonoBehaviour
{
    [Header("배경 스프라이트")]
    [SerializeField]
    private SpriteRenderer backgroundSprite;
    [Header("타겟 비율")]
    [SerializeField]
    private float targetAspectWidth = 16f;
    [SerializeField]
    private float targetAspectHeight = 9f;

    void Start()
    {
        AdjustBackground();
    }

    void AdjustBackground()
    {
        float screenAspect = (float)Screen.width / Screen.height;
        float targetAspect = targetAspectWidth / targetAspectHeight;

        if (screenAspect > targetAspect)
        {
            // 화면이 더 넓은 경우
            backgroundSprite.transform.localScale = new Vector3(screenAspect / targetAspect, 1, 1);
        }
        else
        {
            // 화면이 더 높은 경우
            backgroundSprite.transform.localScale = new Vector3(1, targetAspect / screenAspect, 1);
        }
    }
}
