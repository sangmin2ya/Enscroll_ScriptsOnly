using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using TMPro;
using Game.Scroll;
using UnityEngine.UI;
using ntw.CurvedTextMeshPro;
using Game.Symbols;
using System.Collections;
using Game.Magic;
using Game.Tutorial;
using System;
namespace Game.Desk
{
    [Serializable]
    public struct EffectSet
    {
        public UIDissolve enchantEffect;
        public UIDissolve completeEffect;
        public UIDissolve backgroundEffect;
    }
    public class Scroll : MonoBehaviour, IDragable
    {
        [Header("스크롤 정보")]
        [SerializeField]
        ScrollInfo scrollInfo;
        public ScrollInfo ScrollInfo
        {
            get { return scrollInfo; }
            set { scrollInfo = value; }
        }
        [Header("제출 의뢰지 정보 - 자동초기화")]
        [SerializeField]
        private RectTransform requestSheetPlace;
        public RectTransform RequestSheetPlace
        {
            get { return requestSheetPlace; }
            set { requestSheetPlace = value; }
        }
        [Header("스크롤 내부 데이터")]
        [SerializeField]
        private RectTransform enchantArea;
        public RectTransform EnchantArea
        {
            get { return enchantArea; }
        }
        [SerializeField]
        private Image runeImage;
        public Image RuneImage
        {
            get { return runeImage; }
        }
        [SerializeField]
        private TextProOnACircle runeText;
        public TextProOnACircle RuneText
        {
            get { return runeText; }
        }
        [SerializeField]
        private FinalCircle finalCircle;
        public FinalCircle FinalCircle
        {
            get { return finalCircle; }
        }
        [SerializeField]
        private TextMeshProUGUI scrollText;
        public TextMeshProUGUI ScrollText
        {
            get { return scrollText; }
            set { scrollText = value; }
        }
        [SerializeField]
        private Button deleteButton;

        [SerializeField]
        private TextMeshProUGUI scrollDurationInfoText;
        public TextMeshProUGUI ScrollDurationInfoText
        {
            get { return scrollDurationInfoText; }
            set { scrollDurationInfoText = value; }
        }
        [SerializeField]
        private TextMeshProUGUI scrollDurationText;
        public TextMeshProUGUI ScrollDurationText
        {
            get { return scrollDurationText; }
            set { scrollDurationText = value; }
        }
        [Header("스탬프 찍히는 곳")]
        [SerializeField]
        private RectTransform stampArea;
        [SerializeField]
        private GameObject stamp;
        [Header("왼쪽 활성 속성 태그")]
        // [SerializeField]
        // private GameObject activatedAttributePanel;
        // [SerializeField]
        // GameObject[] scrollActivatedAttribute = new GameObject[3];
        // private Symbol[] activatedAttributeInfos = new Symbol[3];

        private bool isAbleToComplete = false;
        public bool IsAbleToComplete
        {
            get { return isAbleToComplete; }
            set { isAbleToComplete = value; }
        }
        // [SerializeField]
        // private GameObject completeButtonTag;
        // [SerializeField]
        // private Button completeButton;
        [Header("스크롤 단계 구분용 이미지")]
        [SerializeField]
        private EffectSet[] effectSet = new EffectSet[4]; //레벨단위로 세트
        [SerializeField]
        private int _scrollProcess = 0;
        [SerializeField]
        private Color normalColor;
        public Color NormalColor
        {
            get { return normalColor; }
        }
        [SerializeField]
        private Color pressedColor;
        [Header("스크롤 부여 버튼")]
        [SerializeField]
        private GameObject enchantButtonTag;
        private Button scrollTextButton;
        [SerializeField]
        private Button enchantButton;
        [Header("지속시간 툴팁")]
        [SerializeField]
        private GameObject durationTooltip;
        [Header("스크롤 롤 버튼")]
        [SerializeField]
        private Button scrollButton;
        [Header("스크롤 활성 문양")]
        [SerializeField]
        private GameObject activeElementPanel;
        [SerializeField]
        private GameObject[] activeElementGrids = new GameObject[3];
        [SerializeField]
        private Image[] activeElementImages = new Image[8];
        [SerializeField]
        private Sprite emptyElementSprite;
        [SerializeField]
        private Sprite filledElementSprite;

        [Header("스크롤 안내 UI")]
        [SerializeField]
        private GameObject scrollRollGuideUI;
        [SerializeField]
        private GameObject scrollTextGuideUI;
        private Animator _scrollAnimator;
        private RectTransform _rectTransform;
        private RectTransform _parentRectTransform;
        private Canvas _canvas;
        private bool _isRolled = false;
        private bool _whildAnim = false;
        private Camera _mainCamera;
        private Vector3 _dragOffset;
        private bool _isFocused = false;
        private bool _justPullOut = false;
        private ScrollCreationManager _scrollCreationManager;
        public void Start()
        {
            _canvas = GetComponentInParent<Canvas>();
            _rectTransform = GetComponent<RectTransform>();
            _parentRectTransform = _rectTransform.parent as RectTransform;
            _scrollAnimator = GetComponent<Animator>();
            _scrollAnimator.keepAnimatorStateOnDisable = true;
            _scrollCreationManager = ScrollCreationManager.Instance;
            //스크롤 가이드 비활성화
            scrollRollGuideUI.SetActive(false);
            scrollTextGuideUI.SetActive(false);
            _scrollProcess = 0;
            foreach (var img in effectSet)
            {
                img.enchantEffect.gameObject.SetActive(false);
                img.completeEffect.gameObject.SetActive(false);
                img.backgroundEffect.gameObject.SetActive(false);

                img.enchantEffect.location = 1;
                img.completeEffect.location = 1;
                img.backgroundEffect.location = 1;
            }

            scrollButton.interactable = false;
            DeskManager.Instance.SetStampArea(stampArea);
            _mainCamera = Camera.main;
            //속성 보여주는 패널 등록
            MagicController.Instance.EnchantPanel = activeElementPanel;
            activeElementGrids[_scrollCreationManager.CurrentScrollLevel].SetActive(true);
            foreach (var image in activeElementImages)
            {
                image.gameObject.SetActive(false);
            }
            for (int i = 0; i < _scrollCreationManager.currentSymbolLimit; i++)
            {
                activeElementImages[i].gameObject.SetActive(true);
                activeElementImages[i].sprite = emptyElementSprite;
            }
            MagicController.Instance.EnchantSubPanel = activeElementGrids[_scrollCreationManager.CurrentScrollLevel];
            MagicController.Instance.EnchantActionButton = enchantButton;
            MagicController.Instance.ScrollArea = enchantArea;
            //스크롤이 나중에 생성시(업그레이드시) 의뢰 등록
            if (DeskManager.Instance.CurrentOrderSheet != null)
            {
                requestSheetPlace = DeskManager.Instance.CurrentOrderSheet.GetComponent<RectTransform>();
            }

            scrollTextButton = scrollText.GetComponent<Button>();
            scrollTextButton.onClick.AddListener(() =>
            {
                if (scrollText.text != "")
                {
                    if (MagicController.Instance.EnsureMode() != -1)
                    {
                        if (TutorialManager.Instance != null)
                        {
                            if (TutorialManager.Instance.TutorialComplete == false && TutorialManager.Instance.CurrentStepIndex == 17)
                            {   // 부여 설명 단계에서 마법 효과 클릭하면 다음 단계
                                TutorialManager.Instance.NextStep();
                            }
                            if (TutorialManager.Instance.TutorialComplete == false && TutorialManager.Instance.CurrentStepIndex == 26)
                            {   // 강화 확장 설명 단계에서 마법 효과 클릭하면 다음 스텝
                                TutorialManager.Instance.NextStep();
                            }
                        }
                        SoundManager.Instance.PlaySFX("Element"); // 사운드 
                        ShowScrollProcessEffect();
                        ShowTextGuide(false);
                        //부여 버튼 제거 및 완성 버튼 팝업
                        MagicController.Instance.DisableAllElement();
                        enchantButtonTag.GetComponent<RectTransform>().DOAnchorPosX(-100, 0.5f).SetEase(Ease.OutQuad).onComplete = () =>
                        {
                            isAbleToComplete = true;
                            scrollText.color = pressedColor;
                        };
                        scrollText.GetComponent<Button>().enabled = false;

                    }
                }
            });

            // completeButton.onClick.AddListener(() =>
            // {
            //     activeElementPanel.SetActive(false);
            //     activatedAttributePanel.SetActive(false);
            //     enchantButtonTag.SetActive(false);
            //     completeButton.interactable = false;
            //     MagicController.Instance.Ensure();
            //     completeButtonTag.GetComponent<RectTransform>().DOAnchorPosX(-100, 0.5f).SetEase(Ease.OutQuad).onComplete = () =>
            //     {
            //         completeButtonTag.SetActive(false);
            //         scrollButton.interactable = true;
            //     };
            // });

            deleteButton.onClick.AddListener(() =>
            {
                SoundManager.Instance.PlaySFX("Trashbin");
                MagicController.Instance.Erase();
                activeElementPanel.SetActive(true);
                // activatedAttributePanel.SetActive(true);
                ResetAttributeCount();
                //enchantButtonTag.SetActive(true);
                // completeButtonTag.SetActive(true);
                // completeButtonTag.GetComponent<RectTransform>().DOAnchorPosX(-100, 0.5f).SetEase(Ease.OutQuad).onComplete = () =>
                // {
                //     completeButton.interactable = true;
                // };
                isAbleToComplete = false;
                stamp.GetComponent<Image>().DOFade(0, 0.5f).SetEase(Ease.OutQuad);
                //enchantButton.interactable = true;
                scrollButton.interactable = false;
                ShowRollGuide(false);
                ShowTextGuide(false);
                HideScrollProcessEffect();
                //enchantButtonTag.GetComponent<RectTransform>().DOAnchorPosX(0, 0.5f).SetEase(Ease.OutQuad);
            });

            scrollButton.onClick.AddListener(() =>
            {
                ToggleScroll();
                ShowRollGuide(false);
            });

            MagicController.Instance.EnableEnchant();
            _rectTransform.DOLocalMove(new Vector3(-20, -10, 0), 1f).SetEase(Ease.OutQuad);
        }

        public void Update()
        {
            // 스크롤 생성됐을 때 15 스텝 이전에서 부여 버튼 클릭 못하게 막기
            if (TutorialManager.Instance != null)
            {
                if (TutorialManager.Instance.TutorialComplete)
                {
                    scrollTextButton.interactable = true;
                    return;
                }
                else
                {
                    if (TutorialManager.Instance.CurrentStepIndex <= 15)
                    {

                        scrollTextButton.interactable = false;
                    }
                    else
                    {
                        scrollTextButton.interactable = true;
                    }
                }
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!_isRolled || _whildAnim || eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }
            Debug.Log("드래그시작");
            _dragOffset = _rectTransform.position - _mainCamera.ScreenToWorldPoint(eventData.position);
            _rectTransform.SetAsLastSibling();
            // 의뢰지 등록 해제 후 원래 위치로
            if (DeskManager.Instance.CurrentOrderSheet != null)
            {
                DeskManager.Instance.CurrentOrderSheet.SetScroll(null);
            }
            if (_rectTransform.parent != _parentRectTransform)
            {
                Debug.Log("부모로 이동");
                _rectTransform.SetParent(_parentRectTransform);
                _rectTransform.DOScale(new Vector3(1f, 1f, 1f), 0.1f).SetEase(Ease.OutQuad);
                // _rectTransform.DORotate(new Vector3(0, 0, 0), 0.1f).SetEase(Ease.OutQuad);
                _dragOffset = (_rectTransform.position - scrollButton.transform.position);
                _justPullOut = true;
            }
            else
            {
                if (DeskManager.Instance.CurrentOrderSheet != null)
                {
                    DeskManager.Instance.CurrentOrderSheet.GetComponent<RequestSheet>().ScrollSavePoint.SetActive(true);
                }
            }
            _dragOffset.z = 0;
        }
        public void OnDrag(PointerEventData eventData)
        {
            if (!_isRolled || _whildAnim || eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }
            DOTween.Kill(_rectTransform.position);
            if (_rectTransform.GetSiblingIndex() != _parentRectTransform.childCount - 1)
            {
                _rectTransform.SetAsLastSibling();
            }
            //마우스 위치를 월드 좌표로 변환
            var mouseWorldPosition = _mainCamera.ScreenToWorldPoint(eventData.position);
            //드래그 중인 스크롤 위치 변경
            mouseWorldPosition.z = 0;
            //스크롤이 제출 미리보기중이 아니면 위치 같이 바꿔줌
            if (!_isFocused)
            {
                _rectTransform.position = mouseWorldPosition + _dragOffset;
            }
            //스크롤이 의뢰지 위에 있으면 확대
            if (IsMouseNearRequestSheet(requestSheetPlace, mouseWorldPosition, 1) && !_justPullOut && DeskManager.Instance.CurrentOrderSheet.IsPlacedOnDesk)
            {
                if (!_isFocused)
                {
                    DeskManager.Instance.CurrentOrderSheet.SetScrollPreview(this.gameObject);
                    scrollButton.interactable = false;
                    _isFocused = true;
                }
            }
            else
            {
                if (_isFocused)
                {
                    //마우스위치로 바로 이동
                    _rectTransform.DOKill();
                    _rectTransform.SetParent(_parentRectTransform);

                    _rectTransform.SetAsLastSibling();
                    //원상 복구
                    _rectTransform.DOScale(new Vector3(1f, 1f, 1f), 0.3f).SetEase(Ease.OutQuad);
                    _rectTransform.DORotate(new Vector3(0, 0, 0), 0.3f).SetEase(Ease.OutQuad);
                    _isFocused = false;
                    scrollButton.interactable = true;
                }
            }
        }
        // Helper Function
        private bool IsMouseNearRequestSheet(RectTransform rectTransform, Vector2 mousePosition, float margin)
        {
            Rect rect = rectTransform.rect;
            rect.x -= margin; // Expand left
            rect.y -= margin; // Expand down
            rect.width += margin * 2; // Expand width on both sides
            rect.height += margin * 2; // Expand height on both sides

            // Convert rectTransform's local bounds to screen-space
            Vector3[] worldCorners = new Vector3[4];
            rectTransform.GetWorldCorners(worldCorners);

            // Transform into the expanded bounds
            Rect expandedRect = new Rect(
                worldCorners[0].x - margin,
                worldCorners[0].y - margin,
                (worldCorners[2].x - worldCorners[0].x) + margin * 2,
                (worldCorners[2].y - worldCorners[0].y) + margin * 2
            );

            return expandedRect.Contains(mousePosition);
        }
        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_isRolled || _whildAnim || eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }
            //마우스 위치를 월드 좌표로 변환
            var mouseWorldPosition = Camera.main.ScreenToWorldPoint(eventData.position);
            // 의뢰지 위로 올리면 제출 => 의뢰지에 등록하기
            if (_isFocused && DeskManager.Instance.CurrentOrderSheet != null && DeskManager.Instance.CurrentOrderSheet.IsPlacedOnDesk)
            {
                // 의뢰지에 등록
                var rs = DeskManager.Instance.CurrentOrderSheet;

                rs.SetScroll(this);
                _isFocused = false;
            }
            else
            {
                scrollButton.interactable = true;
                _justPullOut = false;
                if (DeskManager.Instance.CurrentOrderSheet != null)
                {
                    DeskManager.Instance.CurrentOrderSheet.ScrollSavePoint.SetActive(false);
                }
                _rectTransform.DOScale(new Vector3(1f, 1f, 1f), 0.3f).SetEase(Ease.OutQuad);
                _rectTransform.DOLocalMove(new Vector3(-20, -30, 0), 0.5f).SetEase(Ease.OutQuad);
                _rectTransform.DORotate(new Vector3(0, 0, 0), 0.3f).SetEase(Ease.OutQuad);
            }
        }

        public void ClampToParentBounds()
        {
            Vector3[] parentCorners = new Vector3[4];
            _parentRectTransform.GetWorldCorners(parentCorners);

            Vector3[] rectCorners = new Vector3[4];
            _rectTransform.GetWorldCorners(rectCorners);

            Vector2 anchoredPosition = _rectTransform.anchoredPosition;

            // 부모의 경계를 기준으로 클램프 수행
            for (int i = 0; i < 4; i++)
            {
                Vector3 localCorner = _parentRectTransform.InverseTransformPoint(rectCorners[i]);

                // 왼쪽 경계 클램프
                if (localCorner.x < _parentRectTransform.rect.xMin)
                    anchoredPosition.x += _parentRectTransform.rect.xMin - localCorner.x;

                // 오른쪽 경계 클램프
                if (localCorner.x > _parentRectTransform.rect.xMax)
                    anchoredPosition.x += _parentRectTransform.rect.xMax - localCorner.x;

                // 아래쪽 경계 클램프
                if (localCorner.y < _parentRectTransform.rect.yMin)
                    anchoredPosition.y += _parentRectTransform.rect.yMin - localCorner.y;

                // 상단 경계 클램프
                if (localCorner.y > _parentRectTransform.rect.yMax)
                    anchoredPosition.y += _parentRectTransform.rect.yMax - localCorner.y;
            }

            _rectTransform.anchoredPosition = anchoredPosition;
        }
        /// <summary>
        /// 스크롤 완성 효과
        /// </summary>
        public void ShowScrollProcessEffect()
        {
            StopAllCoroutines();
            if (_scrollProcess == 0)
            {
                effectSet[_scrollCreationManager.CurrentScrollLevel - 1].enchantEffect.gameObject.SetActive(true);
                effectSet[_scrollCreationManager.CurrentScrollLevel - 1].enchantEffect.location = 1;
            }
            else if (_scrollProcess == 1)
            {
                effectSet[_scrollCreationManager.CurrentScrollLevel - 1].backgroundEffect.gameObject.SetActive(true);
                effectSet[_scrollCreationManager.CurrentScrollLevel - 1].backgroundEffect.location = 1;
                effectSet[_scrollCreationManager.CurrentScrollLevel - 1].completeEffect.gameObject.SetActive(true);
                effectSet[_scrollCreationManager.CurrentScrollLevel - 1].completeEffect.location = 1;
            }
            StartCoroutine(ChangeLocation(_scrollProcess));
            _scrollProcess++;
        }
        public void HideScrollProcessEffect()
        {
            StopAllCoroutines();
            StartCoroutine(ResetLocation());
            _scrollProcess = 0;
        }
        public IEnumerator ChangeLocation(int index)
        {
            float time = 0;
            var set = effectSet[_scrollCreationManager.CurrentScrollLevel - 1];
            while (time < 1f)
            {
                time += Time.deltaTime;
                if (index == 0)
                {
                    set.enchantEffect.location = 1 - time / 1f;
                }
                else if (index == 1)
                {
                    set.backgroundEffect.location = 1 - time / 1f;
                    set.completeEffect.location = 1 - time / 1f;
                }
                yield return null;
            }
        }

        public IEnumerator ResetLocation()
        {
            float time = 0;
            var set = effectSet[_scrollCreationManager.CurrentScrollLevel - 1];
            while (time < 1f)
            {
                time += Time.deltaTime;
                set.enchantEffect.location = time / 1f;
                set.completeEffect.location = time / 1f;
                set.backgroundEffect.location = time / 1f;
                yield return null;
            }
            set.enchantEffect.gameObject.SetActive(false);
            set.completeEffect.gameObject.SetActive(false);
            set.backgroundEffect.gameObject.SetActive(false);
        }
        /// <summary>
        /// 카운트 올리고 실행할것
        /// </summary>
        public void FillAttributeCount()
        {
            activeElementImages[_scrollCreationManager.CurrentSymbolCount - 1].sprite = filledElementSprite;
        }
        /// <summary>
        /// 카운트 내리고 실행할것
        /// </summary>
        public void DeleteAttributeCount()
        {
            activeElementImages[_scrollCreationManager.CurrentSymbolCount].sprite = emptyElementSprite;
        }
        public void ResetAttributeCount()
        {
            foreach (var img in activeElementImages)
            {
                img.sprite = emptyElementSprite;
            }
        }
        /// <summary>
        /// 스크롤 효과 가이드 표시 메서드
        /// </summary>
        public void ShowTextGuide(bool show)
        {
            Image img = scrollTextGuideUI.GetComponent<Image>();
            RectTransform rt = scrollTextGuideUI.GetComponent<RectTransform>();
            if (show)
            {
                if (scrollText.GetComponent<Button>().enabled == false)
                {
                    return;
                }
                img.DOKill();
                rt.DOKill();
                rt.localScale = new Vector3(1, 1, 1);
                scrollTextGuideUI.SetActive(true);
                img.DOFade(1, 0.3f).SetEase(Ease.OutQuad).OnComplete(() =>
                {
                    rt.DOScale(1.01f, 0.5f).SetLoops(-1, LoopType.Yoyo);
                });

                // 마법 이름 다 나오고 나서야 다음 스텝으로 넘어가기
                if (TutorialManager.Instance != null && TutorialManager.Instance.TutorialComplete == false && TutorialManager.Instance.CurrentStepIndex == 14)
                {
                    TutorialManager.Instance.NextStep();
                }
            }
            else
            {
                img.DOKill();
                rt.DOKill();
                img.DOFade(0, 0.3f).SetEase(Ease.OutQuad).onComplete = () =>
                {
                    scrollTextGuideUI.SetActive(false);
                    rt.localScale = new Vector3(1, 1, 1);
                };
            }
        }
        /// <summary>
        /// 스크롤 접기 가이드 표시 메서드
        /// </summary>
        public void ShowRollGuide(bool show)
        {
            Image img = scrollRollGuideUI.GetComponent<Image>();
            RectTransform rt = scrollRollGuideUI.GetComponent<RectTransform>();
            if (show)
            {
                if (_isRolled || _whildAnim || scrollButton.interactable == false)
                {
                    return;
                }
                img.DOKill();
                rt.DOKill();
                rt.localScale = new Vector3(1, 1, 1);
                scrollRollGuideUI.SetActive(true);
                img.DOFade(1, 0.3f).SetEase(Ease.OutQuad).OnComplete(() =>
                {
                    rt.DOScale(1.01f, 0.5f).SetLoops(-1, LoopType.Yoyo);
                });
            }
            else
            {
                img.DOKill();
                rt.DOKill();
                img.DOFade(0, 0.3f).SetEase(Ease.OutQuad).onComplete = () =>
                {
                    scrollRollGuideUI.SetActive(false);
                    rt.localScale = new Vector3(1, 1, 1);
                };
            }
        }
        public void ToggleScroll()
        {
            if (_whildAnim)
            {
                return;
            }
            if (_isRolled)
            {
                _whildAnim = true;
                _scrollAnimator.SetTrigger("Open");
                StartCoroutine(ToggleRollFlag(false));
                SoundManager.Instance.PlaySFX("Scrollup");
                transform.SetAsFirstSibling();
            }
            else
            {
                _whildAnim = true;
                _scrollAnimator.SetTrigger("Close");
                deleteButton.gameObject.SetActive(false);
                durationTooltip.SetActive(false);
                StartCoroutine(ToggleRollFlag(true));
                SoundManager.Instance.PlaySFX("Scrollup");
            }
        }

        private IEnumerator ToggleRollFlag(bool flag)
        {
            yield return new WaitForSeconds(0.7f);
            _isRolled = flag;
            _whildAnim = false; // 애니메이션 중복 방지
            durationTooltip.SetActive(!flag);
            deleteButton.gameObject.SetActive(!flag);
        }

        // public void UpdateAttribute(List<Symbol> attributeInfo)
        // {
        //     for (int i = 0; i < 3; i++)
        //     {
        //         if (attributeInfo.Count > i)
        //         {
        //             if (attributeInfo[i] is SymbolDrawer)
        //             {
        //                 activatedAttributeInfos[i] = null;
        //             }
        //             else
        //             {
        //                 activatedAttributeInfos[i] = attributeInfo[i];
        //             }
        //         }
        //         else
        //         {
        //             activatedAttributeInfos[i] = null;
        //         }
        //     }
        //     PopUpAttribute();
        // }
        public void CompleteScroll(Vector3 stampPos)
        {
            activeElementPanel.SetActive(false);
            // activatedAttributePanel.SetActive(false);
            enchantButtonTag.SetActive(false);
            MagicController.Instance.Ensure();
            isAbleToComplete = false;
            stamp.GetComponent<RectTransform>().position = stampPos;
            stamp.GetComponent<Image>().DOFade(1, 0.01f);
            scrollButton.interactable = true;
            ShowRollGuide(true);
            ShowScrollProcessEffect();
            if (TutorialManager.Instance != null && TutorialManager.Instance.TutorialComplete == false && TutorialManager.Instance.CurrentStepIndex == 19)
            {   // 속성 확정 설명 단계에서 버튼 누르면 다음 스텝
                TutorialManager.Instance.NextStep();
            }
        }
        /// <summary>
        /// 스크롤 상단 사이드바에 속성을 표시하는 메서드
        /// </summary>
        // public void PopUpAttribute()
        // {
        //     for (int i = 0; i < 3; i++)
        //     {
        //         TextMeshProUGUI text = scrollActivatedAttribute[i].GetComponentInChildren<TextMeshProUGUI>();
        //         if (activatedAttributeInfos[i] == null)
        //         {
        //             text.text = "";
        //             scrollActivatedAttribute[i].transform.DOKill();
        //             scrollActivatedAttribute[i].transform.localPosition = new Vector3(150, 0, 0);
        //         }
        //         else
        //         {
        //             string attributeName = AttributeManager.Instance.FindNameById(activatedAttributeInfos[i].SymbolData.Id);
        //             if (!AttributeManager.Instance.IsNameUnlocked(activatedAttributeInfos[i].SymbolData.Id))
        //             {
        //                 attributeName = "???";
        //             }
        //             if (attributeName != text.text)
        //             {
        //                 scrollActivatedAttribute[i].transform.DOKill();
        //                 scrollActivatedAttribute[i].transform.localPosition = new Vector3(150, 0, 0);
        //                 text.text = attributeName;
        //                 scrollActivatedAttribute[i].transform.DOLocalMove(new Vector3(0, 0, 0), 0.5f).SetEase(Ease.OutQuad);
        //             }
        //         }
        //     }
        // }
    }
}