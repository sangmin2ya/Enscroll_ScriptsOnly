using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.Board
{
    public class PlaceDescUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("설명 UI")]
        [Tooltip("지명에 대한 설명 UI 지정 ㄱㄱ.")]
        public GameObject hoverDescriptionUI;

        private void Start()
        {
            if (hoverDescriptionUI != null)
            {
                hoverDescriptionUI.SetActive(false); // 초기에는 비활성화
            }
            else
            {
                Debug.LogWarning($"{gameObject.name}에 설명 UI가 할당되지 않았습니다!");
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (hoverDescriptionUI != null)
            {
                hoverDescriptionUI.SetActive(true); // 설명 UI 활성화
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (hoverDescriptionUI != null)
            {
                hoverDescriptionUI.SetActive(false); // 설명 UI 비활성화
            }
        }
    }
}