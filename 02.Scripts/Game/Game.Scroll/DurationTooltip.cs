using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace scroll
{
    public class DurationTooltip : MonoBehaviour, IHoverable
    {
        [SerializeField]
        private GameObject tooltip;
        private RectTransform tooltipRect;
        public void Start()
        {
            tooltipRect = tooltip.GetComponent<RectTransform>();
        }
        public void OnHover()
        {
            throw new System.NotImplementedException();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (GetComponent<TextMeshProUGUI>().text == "")
            {
                return;
            }
            tooltipRect.DOKill();
            tooltipRect.DOLocalMoveX(50, 0.3f);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            tooltipRect.DOKill();
            tooltipRect.DOLocalMoveX(-100, 0.3f);
        }
    }
}