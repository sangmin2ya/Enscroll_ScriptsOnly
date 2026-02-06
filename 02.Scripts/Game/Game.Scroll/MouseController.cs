using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using Game.Symbols;
using Game.Desk;
using Game.Magic;

namespace Game.Scroll
{
    /// <summary>
    /// 스크롤위의 마우스 컨트롤러 - 최적화가 안좋아서 나중에 수정해야함.
    /// </summary>
    public class MouseController : MonoBehaviour
    {
        private List<GameObject> uiObjects = new List<GameObject>();
        private GameObject currentMouseOverObject = null;
        private GameObject previousMouseOverObject = null;
        [Header("레이캐스트")]
        private PointerEventData pointerEventData;
        private EventSystem eventSystem;
        [Header("참조용 오브젝트")]
        [SerializeField] private DrawerHoverCheck drawerHoverCheck;
        private void Start()
        {
            eventSystem = EventSystem.current;
        }
        void Update()
        {
            if (drawerHoverCheck.IsDragging)
            {
                return;
            }
            if (IsMouseOverUI(out GameObject topmostUIElement))
            {
                if (topmostUIElement != null && topmostUIElement.layer == LayerMask.NameToLayer("OverableUI"))
                {
                    if (previousMouseOverObject != null)
                    {
                        previousMouseOverObject.GetComponent<SymbolDrawer>()?.MouseExit();
                        previousMouseOverObject.GetComponent<OuterCircleDrawer>()?.MouseExit();
                        previousMouseOverObject = null;
                    }
                    return;
                }
            }
            // 2. 2D 콜라이더 감지 (마우스 위치를 월드 좌표로 변환)
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Collider2D[] colliders = Physics2D.OverlapPointAll(mousePosition);
            if (colliders.Length == 0 || (colliders[colliders.Length - 1].GetComponent<SymbolDrawer>() == null && colliders[colliders.Length - 1].GetComponent<OuterCircleDrawer>() == null))
            {
                if (previousMouseOverObject != null)
                {
                    previousMouseOverObject.GetComponent<SymbolDrawer>()?.MouseExit();
                    previousMouseOverObject.GetComponent<OuterCircleDrawer>()?.MouseExit();
                    previousMouseOverObject = null;
                }
                return;
            }
            // 감지된 UI 요소 확인
            foreach (Collider2D result in colliders)
            {
                // 외접원이 아니거나 미리보기 상태가 아닌 경우
                if ((result.gameObject.GetComponent<OuterCircleDrawer>() != null && !result.gameObject.GetComponent<OuterCircleDrawer>().isPreview)
                 || result.gameObject.GetComponent<SymbolDrawer>() != null)
                {
                    uiObjects.Add(result.gameObject);
                }
            }
            SignalMouseOverObject();
        }
        public void SignalMouseOverObject()
        {
            foreach (GameObject obj in uiObjects)
            {
                if (obj.GetComponent<OuterCircleDrawer>() != null)
                {
                    continue;
                }
                if (obj.GetComponent<SymbolDrawer>() != null)
                {
                    if (currentMouseOverObject == null)
                    {
                        currentMouseOverObject = obj;
                    }
                    else
                    {
                        if (obj.transform.GetSiblingIndex() > currentMouseOverObject.transform.GetSiblingIndex())
                        {
                            currentMouseOverObject = obj;
                        }
                    }
                }
            }
            if (currentMouseOverObject != null)
            {
                if (previousMouseOverObject != null && currentMouseOverObject != previousMouseOverObject)
                {
                    previousMouseOverObject.GetComponent<SymbolDrawer>()?.MouseExit();
                    previousMouseOverObject.GetComponent<OuterCircleDrawer>()?.MouseExit();
                }
                currentMouseOverObject.GetComponent<SymbolDrawer>()?.MouseEnter();
                previousMouseOverObject = currentMouseOverObject;
                currentMouseOverObject = null;
                uiObjects.Clear();
                return;
            }
            foreach (GameObject obj in uiObjects)
            {
                if (obj.GetComponent<OuterCircleDrawer>() != null)
                {
                    currentMouseOverObject = obj;
                    if (previousMouseOverObject != null && currentMouseOverObject != previousMouseOverObject)
                    {
                        previousMouseOverObject.GetComponent<SymbolDrawer>()?.MouseExit();
                        previousMouseOverObject.GetComponent<OuterCircleDrawer>()?.MouseExit();
                    }
                    currentMouseOverObject.GetComponent<OuterCircleDrawer>()?.MouseEnter();
                    previousMouseOverObject = currentMouseOverObject;
                    currentMouseOverObject = null;
                    uiObjects.Clear();
                    return;
                }
            }
            if (previousMouseOverObject != null)
            {
                previousMouseOverObject.GetComponent<SymbolDrawer>()?.MouseExit();
                previousMouseOverObject.GetComponent<OuterCircleDrawer>()?.MouseExit();
                previousMouseOverObject = null;
            }
            uiObjects.Clear();
        }
        private bool IsMouseOverPolygonCollider()
        {
            // 마우스 좌표를 월드 좌표로 변환
            Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPosition.z = 0; // 2D 좌표로 맞추기 위해 Z값 제거

            // Physics2D.Raycast로 Polygon Collider 확인
            RaycastHit2D hit = Physics2D.Raycast(mouseWorldPosition, Vector2.zero);

            if (hit.collider != null && hit.collider is PolygonCollider2D)
            {
                return true;
            }

            return false;
        }
        private bool IsMouseOverUI(out GameObject topmostElement)
        {
            topmostElement = null;

            // PointerEventData 생성
            pointerEventData = new PointerEventData(eventSystem)
            {
                position = Input.mousePosition
            };

            // 모든 UI 요소에 대한 레이캐스트 결과 저장
            List<RaycastResult> results = new List<RaycastResult>();
            eventSystem.RaycastAll(pointerEventData, results);

            if (results.Count > 0)
            {
                // 첫 번째 (최상단) 요소 반환
                topmostElement = results[0].gameObject;
                return true;
            }

            return false;
        }
    }
}