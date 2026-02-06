using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Game.Desk;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggablePuzzle : MonoBehaviour, IDragable
{
    private Image image;
    private RectTransform rectTransform;
    private Vector3 dragOffset;
    private Canvas canvas;
    private Camera mainCamera;
    private bool isDraggable = true;
    public bool IsDraggable
    {
        get { return isDraggable; }
        set { isDraggable = value; }
    }
    private bool isDragging = false;
    public bool IsDragging
    {
        get { return isDragging; }
        set { isDragging = value; }
    }
    private RectTransform mainDeskUI;


    void Awake()
    {
        image = GetComponent<Image>();
        image.alphaHitTestMinimumThreshold = 0.5f;
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        mainCamera = Camera.main;
        mainDeskUI = CanvasManager.Instance.MainDeskUI;
    }

    void Start()
    {
        Vector3 randomTargetPos = new Vector3(Random.Range(-50, 50), Random.Range(-50, 50), 0);
        transform.DOLocalMove(transform.localPosition + randomTargetPos, 1f).SetEase(Ease.OutExpo);
    }


    public void ClampToParentBounds()
    {
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!IsDraggable || eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }
        // 마우스 위치를 월드 좌표로 변환하여 RectTransform 이동
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
            rectTransform,
            eventData.position,
            mainCamera,
            out Vector3 worldPoint))
        {
            DOTween.Kill(rectTransform.position);
            rectTransform.position = worldPoint + dragOffset;
        }
        isDragging = true;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!IsDraggable || eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }
        DOTween.Kill(rectTransform);
        // 클릭한 위치와 오브젝트 중심 간의 오프셋 계산
        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            rectTransform,
            eventData.position,
            mainCamera,
            out Vector3 worldPoint);
        dragOffset = rectTransform.position - worldPoint;
        rectTransform.DORotate(Vector3.zero, 0.3f).SetEase(Ease.OutQuad);
        rectTransform.SetAsLastSibling();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // mainDeskUI 안에서 부모 경계 제한 추가
        ConstrainToParentBounds(mainDeskUI);
    }

    private void ConstrainToParentBounds(RectTransform parent)
    {
        // 부모 RectTransform의 경계를 가져오기
        Vector3[] parentCorners = new Vector3[4];
        parent.GetWorldCorners(parentCorners);

        // 자식 RectTransform의 경계를 가져오기
        Vector3[] myCorners = new Vector3[4];
        rectTransform.GetWorldCorners(myCorners);

        Vector3 clampedPosition = rectTransform.position;

        float halfWidth = (myCorners[2].x - myCorners[0].x) / 2; // 자식 가로의 절반 크기
        float height = (myCorners[1].y - myCorners[0].y);         // 자식 전체 높이

        // 좌우 제한
        clampedPosition.x = Mathf.Clamp(
            clampedPosition.x,
            parentCorners[0].x - (halfWidth / 4),
            parentCorners[2].x - (halfWidth)
        );

        // 하단 제한: 이미지의 전체 하단이 부모 내부에 있어야 함
        if (myCorners[0].y < parentCorners[0].y)
        {
            clampedPosition.y += parentCorners[0].y - myCorners[0].y;
        }
        // 상단 제한: 부모 상단 밖으로 이미지의 중심이 나가지 않도록 제한
        clampedPosition.y = Mathf.Min(clampedPosition.y, parentCorners[1].y + height * 0.25f);


        // DoTween으로 부드럽게 이동
        if (clampedPosition != rectTransform.position)
        {
            rectTransform.DOMove(clampedPosition, 0.3f).SetEase(Ease.OutQuad);
        }
    }

    public void DestroyObject()
    {
        image.DOColor(new Color(image.color.r, image.color.g, image.color.b, 0), 0.7f).OnComplete(() => { Destroy(gameObject); });
    }
}
