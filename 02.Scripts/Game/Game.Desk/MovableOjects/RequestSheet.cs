using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Game.Request;
using Game.Tutorial;

namespace Game.Desk
{
    public class RequestSheet : MonoBehaviour, IPointerClickHandler, IDeskItem
    {
        [SerializeField]
        private Scroll currentScroll;
        [SerializeField]
        private GameObject _scrollSavePointPrefab;
        [SerializeField]
        private GameObject _scrollSavePoint;
        public GameObject ScrollSavePoint
        {
            get { return _scrollSavePoint; }
        }
        private bool _isPlacedOnDesk = true;
        // IDeskItem: DragableObject가 제출/서랍 복귀 시 호출
        bool IDeskItem.CanSubmit => true;
        void IDeskItem.OnSubmitted() { SendScroll(); }
        void IDeskItem.OnReturnedToDrawer() { }

        public bool IsPlacedOnDesk
        {
            get { return _isPlacedOnDesk; }
            set { _isPlacedOnDesk = value; }
        }
        private Canvas _canvas;
        private RectTransform _rectTransform;
        private RectTransform _parentRectTransform;
        private Vector2 _previousPosition;

        public bool isOnFile = false;

        void Awake()
        {
            _canvas = GetComponentInParent<Canvas>();
            _rectTransform = GetComponent<RectTransform>();
            _parentRectTransform = _rectTransform.parent as RectTransform;
            //Sheet 오른쪽 하단에 위치한 저장 지점 생성
            _scrollSavePoint = Instantiate(_scrollSavePointPrefab, _rectTransform.Find("내부컨텐츠"));
            _scrollSavePoint.GetComponent<RectTransform>().offsetMin = new Vector2(-15, -15); // Left, Bottom
            _scrollSavePoint.GetComponent<RectTransform>().offsetMax = new Vector2(15, 15); // Right, Top
            _scrollSavePoint.SetActive(false);
        }

        public void Start()
        {
            var dragableObject = GetComponent<DragableObject>();
            if (dragableObject != null)
            {
                dragableObject.IsDraggable = false; // 초기에는 드래그 불가능
            }

            // 부모가 의뢰서 부착 위치 오브젝트이면 아래 동작 안하게. 그러면 DoTween 애니메이션도 안나오고, _isPlacedOnDesk도 true로 계속 되니까 드래그 관련 코드에서 그냥 바로 return 됨.
            var detailPanel = _rectTransform.GetComponentInParent<DetailRequestPanel>();
            if (detailPanel != null)
            {
                _isPlacedOnDesk = true; // 상세 의뢰 정보 패널에 있는 경우 드래그 제한
                _rectTransform.localPosition = Vector3.zero; // 위치 초기화
                _rectTransform.localRotation = Quaternion.Euler(Vector3.zero);
                if (dragableObject != null)
                {
                    dragableObject.IsDraggable = true;
                }
                return; // 아래 애니메이션 코드 실행 안 함
            }

            if (!isOnFile)
            {
                _rectTransform.DOLocalMoveY(0, 1f).SetEase(Ease.OutQuad);
                _rectTransform.DORotate(new Vector3(0, 0, 0), 1f).SetEase(Ease.OutQuad).OnComplete(() =>
                {
                    _isPlacedOnDesk = true;
                    if (TutorialManager.Instance != null && TutorialManager.Instance.TutorialComplete == false && TutorialManager.Instance.CurrentStepIndex == 14)
                    {   // 교수님 첫 편지 왔을 때 바로 줌인
                        ZoomInOrder();
                    }
                    else if (TutorialManager.Instance != null && TutorialManager.Instance.TutorialComplete == false && TutorialManager.Instance.CurrentStepIndex == 24 && DayManager.Instance.day == 1)
                    {   // 교수님 두 번째 편지 왔을 때 바로 줌인
                        ZoomInOrder();
                    }
                    else
                    {
                        if (dragableObject != null)
                        {
                            dragableObject.IsDraggable = true; // 애니메이션 완료 후 드래그 가능
                        }
                    }
                });
            }
        }


        public void SendScroll()
        {
            _isPlacedOnDesk = false;
            RequestManager.Instance.SendRequest(currentScroll);
            // Destroy(gameObject);
        }
        //미리보기로 안에 넣어주는 함수
        public void SetScrollPreview(GameObject preview)
        {
            //뺐을때
            if (preview == null || !_isPlacedOnDesk)
            {
                return;
            }
            RectTransform previewRectTransform = preview.GetComponent<RectTransform>();
            previewRectTransform.transform.SetParent(transform.Find("내부컨텐츠"));
            // 부모의 RectTransform의 가장 아래 위치 계산
            float targetY = _rectTransform.rect.height / 2;
            Vector2 targetPosition = new Vector2(0, targetY);
            previewRectTransform.transform.DOLocalMove(targetPosition, 0.3f).SetEase(Ease.OutQuad);
            // previewRectTransform.transform.DORotate(new Vector3(0, 0, 0), 0.3f).SetEase(Ease.OutQuad);
            previewRectTransform.transform.DOScale(new Vector3(0.8f, 0.8f, 0.8f), 0.3f).SetEase(Ease.OutQuad);
        }
        public void SetScroll(Scroll scroll)
        {
            currentScroll = scroll;
            ScrollSavePoint.SetActive(false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (GetComponent<DragableObject>().IsDragging)
            {
                GetComponent<DragableObject>().IsDragging = false;
                return;
            }
            if (_isPlacedOnDesk && TutorialManager.Instance != null
                && !(TutorialManager.Instance.TutorialComplete == false && TutorialManager.Instance.CurrentStepIndex == 14)
                && !(TutorialManager.Instance.TutorialComplete == false && TutorialManager.Instance.CurrentStepIndex == 24 && DayManager.Instance.day == 1)
                && GetComponentInParent<DetailRequestPanel>() != null)
            {   // 튜토리얼 특정 단계에서는 줌인 수동으로 안되게 함
                // 영수증에 있는 애들은 되야 함
                ZoomInOrder();
            }
            else if (_isPlacedOnDesk)
            {   // 그 외의 경우에는 줌 수동으로 할 수 있음
                ZoomInOrder();
            }
        }
        public void ZoomInOrder()
        {
            if (!_isPlacedOnDesk)
            {
                return;
            }
            _rectTransform.DOKill();
            DeskManager.Instance.BlurPanel.gameObject.SetActive(true);
            DeskManager.Instance.BlurPanel.GetComponent<Image>().DOFade(0.93f, 0.3f);
            GetComponent<DragableObject>().IsDraggable = false;
            _isPlacedOnDesk = false;
            _scrollSavePoint.SetActive(false);

            _rectTransform.SetParent(DeskManager.Instance.BlurPanel);
            _previousPosition = _rectTransform.position;
            _rectTransform.SetAsLastSibling();
            transform.parent.SetAsLastSibling();
            _rectTransform.DOScale(new Vector3(1.8f, 1.8f, 1f), 0.4f).SetEase(Ease.OutQuad);
            _rectTransform.DOLocalMove(new Vector3(0, 0, 0), 0.4f).SetEase(Ease.OutQuad).onComplete = () =>
            {
                DeskManager.Instance.BlurPanel.GetComponent<Button>().onClick.AddListener(() =>
                {
                    ZoomOutOrder();
                    DeskManager.Instance.BlurPanel.GetComponent<Button>().onClick.RemoveAllListeners();
                });
            };
            if (currentScroll != null)
            {
                currentScroll.gameObject.SetActive(false);
            }
        }
        public void ZoomOutOrder()
        {
            if (_isPlacedOnDesk)
            {
                return;
            }
            _rectTransform.DOKill();
            DeskManager.Instance.BlurPanel.GetComponent<Image>().DOFade(0, 0.4f);
            _rectTransform.DOScale(new Vector3(1f, 1f, 1f), 0.4f).SetEase(Ease.OutQuad);
            _rectTransform.DOMove(_previousPosition, 0.4f).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                _rectTransform.SetParent(_parentRectTransform);
                DeskManager.Instance.BlurPanel.gameObject.SetActive(false);
                GetComponent<DragableObject>().IsDraggable = true;
                _isPlacedOnDesk = true;
                if (currentScroll != null)
                {
                    //_scrollSavePoint.SetActive(true);
                    currentScroll.gameObject.SetActive(true);
                }
                if (TutorialManager.Instance.TutorialComplete == false && TutorialManager.Instance.CurrentStepIndex == 14)
                {   // 교수님 첫 편지 왔을 때 확대 해제 시 다음 스텝
                    TutorialManager.Instance.ShowTutorialStep(TutorialManager.Instance.CurrentStepIndex);
                }
                // 영수증 상세 보기의 줌인 아웃도 판정되어서 예외 처리 해줘야 함. 부모가 영수증이면 안된다는 조건 추가하면 될듯.
                if (TutorialManager.Instance.TutorialComplete == false
                    && TutorialManager.Instance.CurrentStepIndex == 24
                    && DayManager.Instance.day == 1
                    && GetComponentInParent<DetailRequestPanel>() == null)
                {   // 교수님 두 번째 편지 확대 해제 시 다음 스텝
                    TutorialManager.Instance.ShowTutorialStep(TutorialManager.Instance.CurrentStepIndex);
                }
            });

        }
    }
}