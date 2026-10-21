using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.UI;
using Game.Request;
using Game.Scroll;
using System.Collections;

namespace Game.Desk
{
    public enum InteractableSoundType
    {
        종이, 도장, 책
    }
    public class DragableObject : MonoBehaviour, IDragable, IPointerDownHandler, IPointerUpHandler
    {
        [Header("인터렉터블 오브젝트 타입")]
        [SerializeField] private InteractablePositionType objectType; // 위치 타입
        [SerializeField] private InteractableSoundType soundType;
        private RectTransform mainDeskUI; // 메인 책상 UI
        private RectTransform rightUI;    // 우측 UI
        private RectTransform submitUI; // 제출 영역
        private RectTransform rightUIPosition;  // 우측 UI 초기 위치
        private RectTransform scrollUIPosition;  // 우측 UI 초기 위치
        public RectTransform ScrollUIPosition // 스탬프 위치
        {
            get { return scrollUIPosition; }
            set { scrollUIPosition = value; }
        }
        private RectTransform endingLetterUI;
        public RectTransform EndingLetterUI
        {
            get { return endingLetterUI; }
            set { endingLetterUI = value; }
        }

        [Header("변화 스프라이트 및 스케일")]
        [SerializeField] private Sprite deskSprite;        // 메인 책상 스프라이트
        public Sprite DeskSprite
        {
            get { return deskSprite; }
            set { deskSprite = value; }
        }
        [SerializeField] private Sprite rightUISprite;     // 우측 UI 스프라이트
        [SerializeField] private Sprite stampSprite;       // 스탬프 스프라이트
        [SerializeField] private Vector3 deskScale = Vector3.one;      // 메인 책상 크기
        [SerializeField] private Vector3 rightUIScale = new Vector3(0.3f, 0.3f, 0.3f); // 우측 UI 크기
        private float onClickScaler = 1.03f; // 클릭 시 스케일러

        private RectTransform rectTransform;
        private Camera mainCamera;
        private Vector3 dragOffset;
        private RectTransform currentParent; // 현재 부모를 추적
        //자세히 보기 용 플래그
        private bool isDragging = false;
        public bool IsDragging
        {
            get { return isDragging; }
            set { isDragging = value; }
        }
        private bool isDraggable = true;
        public bool IsDraggable
        {
            get { return isDraggable; }
            set { isDraggable = value; }
        }
        private bool isOnSubmitArea = false;
        private bool isStampOnScroll = false;
        private bool isStampOnLetter = false;

        // 공통 드래그 외의 오브젝트별 동작 (의뢰서, 납부서, 엔딩 편지, 스크랩북, 파일)
        // 오브젝트마다 IDeskItem 구현 컴포넌트는 objectType에 맞는 하나만 붙어 있다고 가정
        private IDeskItem[] deskItems;
        private Image image;
        private GameObject innerContent;
        // 처음 사용할 때 한 번만 찾아서 캐싱
        private GameObject InnerContent
        {
            get
            {
                if (innerContent == null)
                {
                    innerContent = transform.Find("내부컨텐츠").gameObject;
                }
                return innerContent;
            }
        }

        void Awake()
        {
            mainDeskUI = CanvasManager.Instance.MainDeskUI;
            rightUI = CanvasManager.Instance.RightUI;
            submitUI = CanvasManager.Instance.SubmitUI;

            deskItems = GetComponents<IDeskItem>();
            image = GetComponent<Image>();
            rectTransform = GetComponent<RectTransform>();
            mainCamera = Camera.main;
            currentParent = mainDeskUI; // 초기 부모는 메인 책상
            if (objectType == InteractablePositionType.스크랩북 || objectType == InteractablePositionType.인장)
            {
                currentParent = rightUI;
            }
            if (objectType != InteractablePositionType.물약)
            {
                rightUIPosition = CanvasManager.Instance.GetUIPositionOfObject(objectType, this);
            }
        }
        public void OnPointerUp(PointerEventData eventData)
        {
            if (!IsDraggable || eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }
            // 드래그 종료 후 위치 설정
            if (currentParent == mainDeskUI)
            {
                // mainDeskUI 안에서 부모 경계 제한 추가
                ConstrainToParentBounds(mainDeskUI);

                // 스케일과 스프라이트 복원
                rectTransform.DOScale(deskScale, 0.2f).SetEase(Ease.OutQuad);
            }
            if (currentParent == submitUI)
            {
                // 제출 영역에 있는 경우: 제출은 OnEndDrag에서 처리
            }
            else if (currentParent == rightUI)
            {
                ReturnToDrawer(resetStampHitTest: false);
            }
        }
        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left || !isDraggable)
            {
                return;
            }
            PlaySFX();
            rectTransform.DOScale(onClickScaler * transform.localScale, 0.2f).SetEase(Ease.OutQuad);
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
            // 물약이면 우측 UI카운트 제거
            if (objectType == InteractablePositionType.물약 && currentParent == rightUI)
            {
                CanvasManager.Instance.RemovePotionFromDrawer(this);
            }
            rectTransform.DORotate(Vector3.zero, 0.3f).SetEase(Ease.OutQuad);
            rectTransform.SetAsLastSibling();
        }
        private void PlaySFX()
        {
            switch (soundType)
            {
                case InteractableSoundType.종이:
                    SoundManager.Instance.PlaySFX("Papergrab");
                    break;
                case InteractableSoundType.도장:
                    SoundManager.Instance.PlaySFX("Stampgrab");
                    break;
                case InteractableSoundType.책:
                    SoundManager.Instance.PlaySFX("Bookgrab");
                    break;
                default:
                    break;
            }
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
                CheckCurrentParent(worldPoint);
                if (isOnSubmitArea)
                {
                    return;
                }
                DOTween.Kill(rectTransform.position);
                rectTransform.position = worldPoint + dragOffset;
                // 드래그 중 현재 위치에 따라 부모 전환
            }
            isDragging = true;
        }
        private IEnumerator ReturnStampSprite()
        {
            yield return new WaitForSeconds(0.6f);
            image.sprite = deskSprite;
        }
        public void OnEndDrag(PointerEventData eventData)
        {
            if (!IsDraggable || eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }
            // 드래그 종료 후 위치 설정
            if (currentParent == mainDeskUI)
            {
                // mainDeskUI 안에서 부모 경계 제한 추가
                ConstrainToParentBounds(mainDeskUI);

                // 스케일과 스프라이트 복원
                rectTransform.DOScale(deskScale, 0.2f).SetEase(Ease.OutQuad);
                if (objectType == InteractablePositionType.인장)
                {
                    image.alphaHitTestMinimumThreshold = 0.1f;
                }
            }
            if (isStampOnScroll)
            {
                isStampOnScroll = false;
                rectTransform.DOKill();
                rectTransform.DOScale(0.9f, 0.1f).SetEase(Ease.OutQuad).OnComplete(() =>
                {
                    SoundManager.Instance.PlaySFX("Stamp");
                    DeskManager.Instance.CurrentScroll.GetComponent<Scroll>().CompleteScroll(rectTransform.position);
                    rectTransform.DOScale(0.9f, 0.4f).OnComplete(() =>
                    {
                        rectTransform.DOScale(1, 0.2f).SetEase(Ease.OutQuad);
                    });
                    StartCoroutine(ReturnStampSprite());
                });

            }
            if (isStampOnLetter)
            {
                isStampOnLetter = false;
                rectTransform.DOKill();
                rectTransform.DOScale(0.9f, 0.1f).SetEase(Ease.OutQuad).OnComplete(() =>
                {
                    SoundManager.Instance.PlaySFX("Stamp");
                    endingLetterUI.GetComponent<EndingLetter>().Stamp(rectTransform.position);
                    rectTransform.DOScale(0.9f, 0.4f).OnComplete(() =>
                    {
                        rectTransform.DOScale(1, 0.2f).SetEase(Ease.OutQuad).OnComplete(() =>
                        {
                            image.sprite = deskSprite;
                        });
                    });
                });
            }
            if (currentParent == submitUI)
            {
                // 제출 영역에 있는 경우
                // 스케일과 스프라이트 복원
                rectTransform.DOScale(rightUIScale, 0.2f).SetEase(Ease.OutQuad);
                foreach (IDeskItem item in deskItems)
                {
                    item.OnSubmitted();
                }
            }
            else if (currentParent == rightUI)
            {
                ReturnToDrawer(resetStampHitTest: true);
            }
        }

        /// <summary>
        /// 드래그를 놓은 곳이 우측 서랍이면 크기를 줄이고 서랍 자리로 되돌림
        /// </summary>
        private void ReturnToDrawer(bool resetStampHitTest)
        {
            if (objectType == InteractablePositionType.납부서)
            {
                rectTransform.DOScale(new Vector3(rightUIScale.x, rightUIScale.x, 1), 0.2f).SetEase(Ease.OutQuad);
            }
            else
            {
                rectTransform.DOScale(rightUIScale, 0.2f).SetEase(Ease.OutQuad);
            }
            // 우측 UI로 되돌리기
            // 물약이면 이 시점에 물약 Index에 따라 자리 설정
            if (objectType == InteractablePositionType.물약)
            {
                rightUIPosition = CanvasManager.Instance.GetUIPositionOfObject(objectType, this);
            }
            foreach (IDeskItem item in deskItems)
            {
                item.OnReturnedToDrawer();
            }
            if (objectType == InteractablePositionType.인장)
            {
                rectTransform.DORotate(new Vector3(0, 0, 0), 0.3f).SetEase(Ease.OutQuad);
                if (resetStampHitTest)
                {
                    image.alphaHitTestMinimumThreshold = 0;
                }
            }
            if (objectType != InteractablePositionType.답장)
            {
                rectTransform.DOMove(rightUIPosition.position, 0.5f).SetEase(Ease.OutQuad);
                if (objectType == InteractablePositionType.의뢰 || objectType == InteractablePositionType.납부서 || objectType == InteractablePositionType.엔딩)
                {
                    SetPlacedOnDesk(false);
                }
            }
        }

        // 제출 가능 조건은 오브젝트별 IDeskItem이 판단 (의뢰서: 항상, 납부서: 납부일, 엔딩 편지: 배드 엔딩이면 도장 후)
        private bool CanSubmit()
        {
            foreach (IDeskItem item in deskItems)
            {
                if (item.CanSubmit)
                {
                    return true;
                }
            }
            return false;
        }

        private void SetPlacedOnDesk(bool isPlaced)
        {
            foreach (IDeskItem item in deskItems)
            {
                item.IsPlacedOnDesk = isPlaced;
            }
        }

        /// <summary>
        /// 제출 영역으로 옮김 (의뢰서, 납부서, 엔딩 편지 공통)
        /// </summary>
        private void MoveToSubmitArea()
        {
            SetPlacedOnDesk(false);
            isOnSubmitArea = true;
            currentParent = submitUI;
            rectTransform.SetParent(RequestManager.Instance.SubmitArea.GetComponent<RectTransform>(), true);
            rectTransform.localScale = rightUIScale * onClickScaler;
            rectTransform.DOLocalMove(new Vector3(0, 0, 0), 0.3f).SetEase(Ease.OutQuad);

            // 마우스 위치로 바로 이동
            dragOffset = Vector3.zero;
            image.sprite = rightUISprite;
            InnerContent.SetActive(false);
        }
        /// <summary>
        /// 추후에 날 바뀌면 모든 오브젝트 서럽 안으로 ㄴ ㅓㅎ어주는 메소드.
        /// </summary>
        public void OrganizeObject()
        {
            if (!gameObject.activeSelf)
            {
                return;
            }
            if (objectType == InteractablePositionType.영수증)
            {
                currentParent = mainDeskUI;
                rectTransform.SetParent(currentParent, true);
                rectTransform.localScale = deskScale;
                rectTransform.localPosition = Vector3.zero;
                return;
            }
            // if (currentParent == mainDeskUI)
            // {
            //     isOnSubmitArea = false;
            //     currentParent = rightUI;
            //     rectTransform.SetParent(rightUI, true);
            //     rectTransform.localScale = rightUIScale;
            //     rectTransform.position = rightUIPosition.position;
            // }

        }
        private void CheckCurrentParent(Vector3 mousePosition)
        {
            // 현재 위치가 어느 UI 위에 있는지 확인
            Vector3[] mainDeskCorners = new Vector3[4];
            mainDeskUI.GetWorldCorners(mainDeskCorners);

            Vector3[] rightUICorners = new Vector3[4];
            rightUI.GetWorldCorners(rightUICorners);

            Vector3[] submitAreaCorners = new Vector3[4];
            submitUI.GetWorldCorners(submitAreaCorners);

            Vector3[] scrollUICorners = new Vector3[4];
            if (scrollUIPosition != null)
            {
                scrollUIPosition.GetWorldCorners(scrollUICorners);
            }
            Vector3[] endingLetterCorners = new Vector3[4];
            if (endingLetterUI != null)
            {
                endingLetterUI.GetWorldCorners(endingLetterCorners);
            }
            // 제출 가능한 오브젝트(IDeskItem.CanSubmit)가 제출 영역 위에 있는 경우
            if (objectType != InteractablePositionType.기타 && IsWithinBounds(mousePosition, submitAreaCorners) && CanSubmit())
            {
                if (currentParent != submitUI)
                {
                    MoveToSubmitArea();
                }
            }
            // 우측 UI 영역 위에 있는 경우
            else if (objectType != InteractablePositionType.기타 && IsWithinBounds(mousePosition, rightUICorners))
            {
                if (currentParent != rightUI)
                {
                    SetPlacedOnDesk(false);
                    rectTransform.DOKill();
                    isOnSubmitArea = false;
                    currentParent = rightUI;
                    rectTransform.SetParent(rightUI, true);
                    // 마우스 위치로 바로 이동
                    dragOffset = Vector3.zero;
                    rectTransform.position = mousePosition + dragOffset;
                    if (objectType != InteractablePositionType.납부서)
                    {
                        image.sprite = rightUISprite;
                        rectTransform.localScale = rightUIScale * onClickScaler;
                    }
                    else
                    {
                        image.sprite = deskSprite;
                        InnerContent.SetActive(true);
                        rectTransform.localScale = new Vector3(rightUIScale.x, rightUIScale.x, rightUIScale.z) * onClickScaler;
                    }

                    if (objectType == InteractablePositionType.의뢰 || objectType == InteractablePositionType.엔딩)
                    {
                        InnerContent.SetActive(false);
                    }

                }
            }
            // 메인 책상 영역 위에 있는 경우
            if (IsWithinBounds(mousePosition, mainDeskCorners))
            {
                if (currentParent != mainDeskUI)
                {
                    SetPlacedOnDesk(true);
                    rectTransform.DOKill();
                    isOnSubmitArea = false;
                    currentParent = mainDeskUI;
                    rectTransform.SetParent(mainDeskUI, true);
                    rectTransform.localScale = deskScale * onClickScaler;
                    rectTransform.position = mousePosition + dragOffset;
                    image.sprite = deskSprite;
                    if (objectType == InteractablePositionType.의뢰 || objectType == InteractablePositionType.납부서 || objectType == InteractablePositionType.엔딩)
                    {
                        InnerContent.SetActive(true);
                    }
                }
                if (objectType == InteractablePositionType.인장 && DeskManager.Instance.CurrentScroll != null)
                {
                    if (DeskManager.Instance.CurrentScroll.GetComponent<Scroll>()?.IsAbleToComplete == true)
                    {
                        //스크롤 위에 있으면
                        if (IsWithinBounds(mousePosition, scrollUICorners))
                        {
                            //세움
                            image.sprite = stampSprite;
                            dragOffset = Vector3.zero;
                            rectTransform.position = mousePosition + dragOffset;
                            isStampOnScroll = true;
                        }
                        else
                        {
                            rectTransform.DORotate(new Vector3(0, 0, 0), 0.3f).SetEase(Ease.OutQuad);
                            image.sprite = deskSprite;
                            isStampOnScroll = false;
                        }
                    }
                }
                if (objectType == InteractablePositionType.인장 && endingLetterUI != null && endingLetterUI.GetComponent<EndingLetter>()?.IsStamped == false)
                {
                    //엔딩 편지 위에 있으면
                    if (IsWithinBounds(mousePosition, endingLetterCorners))
                    {
                        //세움
                        image.sprite = stampSprite;
                        dragOffset = Vector3.zero;
                        rectTransform.position = mousePosition + dragOffset;
                        isStampOnLetter = true;
                    }
                    else
                    {
                        rectTransform.DORotate(new Vector3(0, 0, 0), 0.3f).SetEase(Ease.OutQuad);
                        image.sprite = deskSprite;
                        isStampOnLetter = false;
                    }
                }
            }
        }

        private bool IsWithinBounds(Vector3 position, Vector3[] corners)
        {
            return position.x > corners[0].x && position.x < corners[2].x &&
                   position.y > corners[0].y && position.y < corners[1].y;
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

            if (objectType == InteractablePositionType.인장)
            {
                // 좌우 제한
                clampedPosition.x = Mathf.Clamp(
                    clampedPosition.x,
                    parentCorners[0].x + halfWidth,
                    parentCorners[2].x - halfWidth);
                // 하단 제한: 이미지의 전체 하단이 부모 내부에 있어야 함
                // if (myCorners[0].y < parentCorners[0].y)
                // {
                //     clampedPosition.y += parentCorners[0].y - myCorners[0].y;
                // }
                // // 상단 제한: 부모 상단 밖으로 이미지의 중심이 나가지 않도록 제한
                // clampedPosition.y = Mathf.Min(clampedPosition.y, parentCorners[1].y - height / 2);
                clampedPosition.y = Mathf.Clamp(
                    clampedPosition.y,
                    parentCorners[0].y + height / 3,
                    parentCorners[1].y - height / 2);
            }
            else
            {
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
            }

            // DoTween으로 부드럽게 이동
            if (clampedPosition != rectTransform.position)
            {
                rectTransform.DOMove(clampedPosition, 0.3f).SetEase(Ease.OutQuad);
            }
        }
        public void ClampToParentBounds()
        {

        }

        public void LocatePayment()
        {
            if (objectType == InteractablePositionType.납부서)
            {
                // GetComponent<Payment>().IsPlacedOnDesk = false;
                currentParent = rightUI;
                rectTransform.SetParent(currentParent, true);
                // image.sprite = rightUISprite;
                rectTransform.localScale = new Vector3(rightUIScale.x, rightUIScale.x, 1);
                rectTransform.position = CanvasManager.Instance.GetUIPositionOfObject(objectType, this).position;
                rectTransform.localRotation = Quaternion.identity;
            }
        }

        // 도장을 인벤에 넣어줌
        public void LocateStamp()
        {
            if (objectType == InteractablePositionType.인장)
            {
                // GetComponent<Payment>().IsPlacedOnDesk = false;
                currentParent = rightUI;
                rectTransform.SetParent(currentParent, true);
                // image.sprite = rightUISprite;
                rectTransform.localScale = new Vector3(rightUIScale.x, rightUIScale.x, 1);
                rectTransform.position = CanvasManager.Instance.GetUIPositionOfObject(objectType, this).position;
                rectTransform.localRotation = Quaternion.identity;
            }
        }

        public void DisableDragScript()
        {
            this.enabled = false;
        }

        public void EnableDragScript()
        {
            this.enabled = true;
        }
    }
}