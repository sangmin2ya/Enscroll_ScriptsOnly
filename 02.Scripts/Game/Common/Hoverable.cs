using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class Hoverable : MonoBehaviour, IUIHoverAnimatable
{
    RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        rectTransform.DOScale(Vector3.one * 1.05f, 0.2f).SetEase(Ease.OutQuad);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        rectTransform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutQuad);
    }
}
