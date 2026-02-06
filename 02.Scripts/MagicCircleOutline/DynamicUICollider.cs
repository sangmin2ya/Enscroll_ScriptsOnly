using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[RequireComponent(typeof(RectTransform))]
public class DynamicUICollider : MonoBehaviour
{
    public Image uiImage;  // UI Image 컴포넌트
    public GameObject colliderObject;  // Polygon Collider가 포함된 GameObject

    private PolygonCollider2D polygonCollider;

    private void Start()
    {
        if (colliderObject == null)
        {
            Debug.LogError("Collider Object가 설정되지 않았습니다.");
            return;
        }

        polygonCollider = colliderObject.GetComponent<PolygonCollider2D>();
        UpdateCollider();
    }
    private void Update()
    {
        ChangeSprite(uiImage.sprite);
    }
    // UI Image의 스프라이트가 변경될 때 호출
    public void ChangeSprite(Sprite newSprite)
    {
        uiImage.sprite = newSprite;
        UpdateCollider();
    }

    private void UpdateCollider()
    {
        if (uiImage.sprite == null || polygonCollider == null)
            return;

        // 콜라이더 초기화
        polygonCollider.pathCount = 0;
        
        // 스프라이트의 물리적 모양을 가져와 콜라이더에 적용
        for (int i = 0; i < uiImage.sprite.GetPhysicsShapeCount(); i++)
        {
            List<Vector2> path = new List<Vector2>();
            uiImage.sprite.GetPhysicsShape(i, path);
            polygonCollider.SetPath(i, path.ToArray());
        }

        // Collider Object를 UI Image 위치에 맞춤
        RectTransform rectTransform = uiImage.GetComponent<RectTransform>();
        colliderObject.transform.position = rectTransform.position;
        colliderObject.transform.rotation = rectTransform.rotation;
        colliderObject.transform.localScale = rectTransform.lossyScale;
    }
}
