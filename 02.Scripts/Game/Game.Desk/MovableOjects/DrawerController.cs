using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Game.Desk
{
    public class DrawerController : MonoBehaviour, IPointerClickHandler
    {
        [Header("서랍 열리고 닫히는 위치")]
        [SerializeField]
        private Vector2 openPosition;
        [SerializeField]
        private Vector2 closePosition;
        private bool _isOpened = true;
        private RectTransform _rectTransform;
        public void Start()
        {
            _rectTransform = GetComponent<RectTransform>();
        }
        public void OnPointerClick(PointerEventData eventData)
        {
            var mousePosition = Camera.main.ScreenToWorldPoint(eventData.position);
            if (RectTransformUtility.RectangleContainsScreenPoint(_rectTransform, mousePosition))
            {
                ToggleDrawer();
            }
        }
        public void ToggleDrawer()
        {
            if (_isOpened)
            {
                CloseDrawer();
            }
            else
            {
                OpenDrawer();
            }
        }
        public void OpenDrawer()
        {
            _rectTransform.DOAnchorPos(openPosition, 1.0f).SetEase(Ease.OutQuad);
            _isOpened = true;
            SoundManager.Instance.PlaySFX("DrawerOpen");
        }

        public void CloseDrawer()
        {
            _rectTransform.DOAnchorPos(closePosition, 0.6f).SetEase(Ease.InQuad);
            _isOpened = false;
            SoundManager.Instance.PlaySFX("DrawerClose");
        }
    }
}