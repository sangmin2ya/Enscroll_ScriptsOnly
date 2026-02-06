using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Game.Element;
using Game.Desk;
using DG.Tweening;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace Game.Magic
{
    public class MagicDragController : MonoBehaviour, IDragHandler, IEndDragHandler, IBeginDragHandler, IPointerDownHandler
    {
        [Header("잉크 정보")]
        [SerializeField]
        private ElementType _inkType;
        private RectTransform _inkRect;
        private Image _inkImage;

        [Header("드래그 효과")]
        [SerializeField]
        private GameObject dragEffect;
        [SerializeField]
        private Gradient inkGradient;
        [SerializeField]
        private DrawerHoverCheck drawerHoverCheck;
        private RectTransform scrollRect => MagicController.Instance.ScrollArea;
        [Header("애니메이션")]
        private Vector2 initialPosition;
        private Tween _inkTween;
        private float scaleAmount = 0.03f;  // Scale 변화량
        private float scaleDuration = 1.5f;  // Scale 변화 시간
        private float circleRadius = 5f;  // 원형 궤적 반지름
        private float circleDuration = 2f; // 원형 궤적 회전 시간

        void Start()
        {
            _inkRect = GetComponent<RectTransform>();
            _inkImage = GetComponent<Image>();
            _inkImage.alphaHitTestMinimumThreshold = 0.1f;
            initialPosition = _inkRect.anchoredPosition;

            dragEffect.SetActive(false);
        }
        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return; // 좌클릭이 아니면 함수 종료
            }
            SoundManager.Instance.PlaySFX("Ink1");
            transform.DOShakePosition(0.3f, 3, 10, 90, false, true).SetEase(Ease.OutBack);
        }
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return; // 좌클릭이 아니면 함수 종료
            }
            var mousePosition = Camera.main.ScreenToWorldPoint(eventData.position);
            drawerHoverCheck.IsDragging = true;

            // 1. Scale 애니메이션 (크기 변화)
            _inkRect.DOScale(Vector3.one * (1 + scaleAmount), scaleDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);

            // 2. 원형 궤적 애니메이션
            _inkTween = DOTween.To(() => 0f, angle =>
            {
                float x = Mathf.Cos(angle) * circleRadius;
                float y = Mathf.Sin(angle) * circleRadius;
                _inkRect.anchoredPosition = initialPosition + new Vector2(x, y);
            }, Mathf.PI * 2, circleDuration)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Restart);
        }
        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return; // 좌클릭이 아니면 함수 종료
            }
            var mousePosition = Camera.main.ScreenToWorldPoint(eventData.position);
            dragEffect.transform.position = new Vector3(mousePosition.x, mousePosition.y, 0);

            if (dragEffect.activeSelf == false)
            {
                dragEffect.SetActive(true);
                TrailRenderer trail = dragEffect.GetComponent<TrailRenderer>();
                trail.Clear();
                trail.colorGradient = inkGradient;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return; // 좌클릭이 아니면 함수 종료
            }
            drawerHoverCheck.IsDragging = false;
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

            dragEffect.SetActive(false);
            DOTween.Kill(_inkRect);
            _inkTween.Kill();
            _inkRect.DOAnchorPos(initialPosition, 0.2f).SetEase(Ease.OutBack);
            _inkRect.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);
            if (_inkType == ElementType.None || scrollRect == null)
            {
                return;
            }
            var mousePosition = Camera.main.ScreenToWorldPoint(eventData.position);
            if (RectTransformUtility.RectangleContainsScreenPoint(scrollRect, mousePosition))
            {
                MagicController.Instance.CreateSymbol(_inkType, new Vector2(mousePosition.x, mousePosition.y));
            }
        }
    }
}