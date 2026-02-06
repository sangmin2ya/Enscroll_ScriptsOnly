using UnityEngine;

public class ViewPortController : MonoBehaviour
{
    [Header("타겟 비율")]
    [SerializeField]
    private float targetAspectWidth = 16f;
    [SerializeField]
    private float targetAspectHeight = 9f;
    void Update()
    {
        float targetAspect = targetAspectWidth / targetAspectHeight; // 타겟 비율
        float windowAspect = (float)Screen.width / (float)Screen.height; // 현재 화면 비율
        float scaleHeight = windowAspect / targetAspect;

        Camera camera = Camera.main;

        if (scaleHeight < 1.0f)
        {
            Rect rect = camera.rect;

            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;

            camera.rect = rect;
        }
        else
        {
            float scaleWidth = 1.0f / scaleHeight;

            Rect rect = camera.rect;

            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;

            camera.rect = rect;
        }
    }
}