using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.UI;
using Game.Request;
using Unity.Burst.Intrinsics;
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

        void Awake()
        {
            mainDeskUI = CanvasManager.Instance.MainDeskUI;
            rightUI = CanvasManager.Instance.RightUI;
            submitUI = CanvasManager.Instance.SubmitUI;

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
                // 제출 영역에 있는 경우
                // 스케일과 스프라이트 복원
                // rectTransform.DOScale(rightUIScale, 0.2f).SetEase(Ease.OutQuad);
                // GetComponent<RequestSheet>()?.SendScroll();
                // GetComponent<Payment>()?.SendPayment();
                // GetComponent<EndingLetter>()?.SendLetter();
            }
            else if (currentParent == rightUI)
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
                if (objectType == InteractablePositionType.스크랩북)
                {
                    GetComponent<ScrollBook>().CloseFile();
                }
                if (objectType == InteractablePositionType.인장)
                {
                    rectTransform.DORotate(new Vector3(0, 0, 0), 0.3f).SetEase(Ease.OutQuad);
                }

                if (objectType != InteractablePositionType.답장)
                {
                    rectTransform.DOMove(rightUIPosition.position, 0.5f).SetEase(Ease.OutQuad);
                    if (objectType == InteractablePositionType.의뢰)
                    {
                        GetComponent<RequestSheet>().IsPlacedOnDesk = false;
                    }
                    if (objectType == InteractablePositionType.납부서)
                    {
                        GetComponent<Payment>().IsPlacedOnDesk = false;
                    }
                    if (objectType == InteractablePositionType.엔딩)
                    {
                        GetComponent<EndingLetter>().IsPlacedOnDesk = false;
                    }
                }
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
            GetComponent<Image>().sprite = deskSprite;
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
                    GetComponent<Image>().alphaHitTestMinimumThreshold = 0.1f;
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
                            GetComponent<Image>().sprite = deskSprite;
                        });
                    });
                });
            }
            if (currentParent == submitUI)
            {
                // 제출 영역에 있는 경우
                // 스케일과 스프라이트 복원
                rectTransform.DOScale(rightUIScale, 0.2f).SetEase(Ease.OutQuad);
                GetComponent<RequestSheet>()?.SendScroll();
                GetComponent<Payment>()?.SendPayment();
                GetComponent<EndingLetter>()?.SendLetter();
            }
            else if (currentParent == rightUI)
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
                if (objectType == InteractablePositionType.스크랩북)
                {
                    GetComponent<ScrollBook>().CloseFile();
                }
                if (objectType == InteractablePositionType.인장)
                {
                    rectTransform.DORotate(new Vector3(0, 0, 0), 0.3f).SetEase(Ease.OutQuad);
                    GetComponent<Image>().alphaHitTestMinimumThreshold = 0;
                }
                if (objectType != InteractablePositionType.답장)
                {
                    rectTransform.DOMove(rightUIPosition.position, 0.5f).SetEase(Ease.OutQuad);
                    if (objectType == InteractablePositionType.의뢰)
                    {
                        GetComponent<RequestSheet>().IsPlacedOnDesk = false;
                    }
                    if (objectType == InteractablePositionType.납부서)
                    {
                        GetComponent<Payment>().IsPlacedOnDesk = false;
                    }
                    if (objectType == InteractablePositionType.엔딩)
                    {
                        GetComponent<EndingLetter>().IsPlacedOnDesk = false;
                    }
                }
            }
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
            // 의뢰, 납부서면서 제출 영역 위에 있는 경우
            if (objectType != InteractablePositionType.기타 && IsWithinBounds(mousePosition, submitAreaCorners) && (objectType == InteractablePositionType.의뢰 || (objectType == InteractablePositionType.납부서 && RequestManager.Instance.isPaymentDay)
                || objectType == InteractablePositionType.엔딩 && !(!GetComponent<EndingLetter>().IsStamped && GetComponent<EndingLetter>().EndingType == endingType.BAD)))
            {
                if (objectType == InteractablePositionType.의뢰)
                {
                    if (currentParent != submitUI)
                    {
                        if (GetComponent<RequestSheet>() != null)
                        {
                            GetComponent<RequestSheet>().IsPlacedOnDesk = false;
                        }
                        if (GetComponent<EndingLetter>() != null)
                        {
                            GetComponent<EndingLetter>().IsPlacedOnDesk = false;
                        }
                        if (GetComponent<Payment>() != null)
                        {
                            GetComponent<Payment>().IsPlacedOnDesk = false;
                        }
                        isOnSubmitArea = true;
                        currentParent = submitUI;
                        rectTransform.SetParent(RequestManager.Instance.SubmitArea.GetComponent<RectTransform>(), true);
                        rectTransform.localScale = rightUIScale * onClickScaler;
                        rectTransform.DOLocalMove(new Vector3(0, 0, 0), 0.3f).SetEase(Ease.OutQuad);

                        // 마우스 위치로 바로 이동
                        dragOffset = Vector3.zero;
                        GetComponent<Image>().sprite = rightUISprite;
                        transform.Find("내부컨텐츠").gameObject.SetActive(false);
                    }
                }
                else if (objectType == InteractablePositionType.납부서 && RequestManager.Instance.isPaymentDay)
                {
                    if (currentParent != submitUI)
                    {
                        if (GetComponent<Payment>() != null)
                        {
                            GetComponent<Payment>().IsPlacedOnDesk = false;
                        }
                        isOnSubmitArea = true;
                        currentParent = submitUI;
                        rectTransform.SetParent(RequestManager.Instance.SubmitArea.GetComponent<RectTransform>(), true);
                        rectTransform.localScale = rightUIScale * onClickScaler;
                        rectTransform.DOLocalMove(new Vector3(0, 0, 0), 0.3f).SetEase(Ease.OutQuad);

                        // 마우스 위치로 바로 이동
                        dragOffset = Vector3.zero;
                        GetComponent<Image>().sprite = rightUISprite;
                        transform.Find("내부컨텐츠").gameObject.SetActive(false);
                    }
                }
                else if (objectType == InteractablePositionType.엔딩)
                {
                    if (currentParent != submitUI)
                    {
                        if (GetComponent<EndingLetter>() != null)
                        {
                            GetComponent<EndingLetter>().IsPlacedOnDesk = false;
                        }
                        isOnSubmitArea = true;
                        currentParent = submitUI;
                        rectTransform.SetParent(RequestManager.Instance.SubmitArea.GetComponent<RectTransform>(), true);
                        rectTransform.localScale = rightUIScale * onClickScaler;
                        rectTransform.DOLocalMove(new Vector3(0, 0, 0), 0.3f).SetEase(Ease.OutQuad);

                        // 마우스 위치로 바로 이동
                        dragOffset = Vector3.zero;
                        GetComponent<Image>().sprite = rightUISprite;
                        transform.Find("내부컨텐츠").gameObject.SetActive(false);
                    }
                }
            }
            // 우측 UI 영역 위에 있는 경우
            else if (objectType != InteractablePositionType.기타 && IsWithinBounds(mousePosition, rightUICorners))
            {
                if (currentParent != rightUI)
                {
                    if (GetComponent<RequestSheet>() != null)
                    {
                        GetComponent<RequestSheet>().IsPlacedOnDesk = false;
                    }
                    if (GetComponent<FileController>() != null)
                    {
                        GetComponent<FileController>().IsPlacedOnDesk = false;
                    }
                    if (GetComponent<ScrollBook>() != null)
                    {
                        GetComponent<ScrollBook>().IsPlacedOnDesk = false;
                    }
                    if (GetComponent<EndingLetter>() != null)
                    {
                        GetComponent<EndingLetter>().IsPlacedOnDesk = false;
                    }
                    if (GetComponent<Payment>() != null)
                    {
                        GetComponent<Payment>().IsPlacedOnDesk = false;
                    }
                    rectTransform.DOKill();
                    isOnSubmitArea = false;
                    currentParent = rightUI;
                    rectTransform.SetParent(rightUI, true);
                    // 마우스 위치로 바로 이동
                    dragOffset = Vector3.zero;
                    rectTransform.position = mousePosition + dragOffset;
                    if (objectType != InteractablePositionType.납부서)
                    {
                        GetComponent<Image>().sprite = rightUISprite;
                        rectTransform.localScale = rightUIScale * onClickScaler;
                    }
                    else
                    {
                        GetComponent<Image>().sprite = deskSprite;
                        transform.Find("내부컨텐츠").gameObject.SetActive(true);
                        rectTransform.localScale = new Vector3(rightUIScale.x, rightUIScale.x, rightUIScale.z) * onClickScaler;
                    }

                    if (objectType == InteractablePositionType.의뢰 || objectType == InteractablePositionType.엔딩)
                    {
                        transform.Find("내부컨텐츠").gameObject.SetActive(false);
                    }

                }
            }
            // 메인 책상 영역 위에 있는 경우
            if (IsWithinBounds(mousePosition, mainDeskCorners))
            {
                if (currentParent != mainDeskUI)
                {
                    if (GetComponent<RequestSheet>() != null)
                    {
                        GetComponent<RequestSheet>().IsPlacedOnDesk = true;
                    }
                    if (GetComponent<FileController>() != null)
                    {
                        GetComponent<FileController>().IsPlacedOnDesk = true;
                    }
                    if (GetComponent<ScrollBook>() != null)
                    {
                        GetComponent<ScrollBook>().IsPlacedOnDesk = true;
                    }
                    if (GetComponent<EndingLetter>() != null)
                    {
                        GetComponent<EndingLetter>().IsPlacedOnDesk = true;
                    }
                    if (GetComponent<Payment>() != null)
                    {
                        GetComponent<Payment>().IsPlacedOnDesk = true;
                    }
                    rectTransform.DOKill();
                    isOnSubmitArea = false;
                    currentParent = mainDeskUI;
                    rectTransform.SetParent(mainDeskUI, true);
                    rectTransform.localScale = deskScale * onClickScaler;
                    rectTransform.position = mousePosition + dragOffset;
                    GetComponent<Image>().sprite = deskSprite;
                    if (objectType == InteractablePositionType.의뢰 || objectType == InteractablePositionType.납부서 || objectType == InteractablePositionType.엔딩)
                    {
                        transform.Find("내부컨텐츠").gameObject.SetActive(true);
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
                            GetComponent<Image>().sprite = stampSprite;
                            dragOffset = Vector3.zero;
                            rectTransform.position = mousePosition + dragOffset;
                            isStampOnScroll = true;
                        }
                        else
                        {
                            rectTransform.DORotate(new Vector3(0, 0, 0), 0.3f).SetEase(Ease.OutQuad);
                            GetComponent<Image>().sprite = deskSprite;
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
                        GetComponent<Image>().sprite = stampSprite;
                        dragOffset = Vector3.zero;
                        rectTransform.position = mousePosition + dragOffset;
                        isStampOnLetter = true;
                    }
                    else
                    {
                        rectTransform.DORotate(new Vector3(0, 0, 0), 0.3f).SetEase(Ease.OutQuad);
                        GetComponent<Image>().sprite = deskSprite;
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
                // GetComponent<Image>().sprite = rightUISprite;
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
                // GetComponent<Image>().sprite = rightUISprite;
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