using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

using Game.Scroll;
using Game.Symbols;
using Game.Element;
using Game.Attribute;

using DG.Tweening;
using UnityEngine.EventSystems;
using Game.Magic;
using TMPro;
using Game.Tutorial;

namespace Game.Desk
{
    public class MagicController : Singleton<MagicController>
    {
        [Header("임시 힌트")]
        private int hintCount = 0;
        private int currentHintCount = 0;
        [SerializeField] private TextMeshProUGUI scrollBookhintText;

        [Header("버튼")]
        [SerializeField]
        private Button enchantActionButton;
        public Button EnchantActionButton
        {
            get { return enchantActionButton; }
            set { enchantActionButton = value; }
        }

        [Header("서랍 뚜껑")]
        [SerializeField]
        private GameObject drawerCover;
        [SerializeField]
        private GameObject runeDrawer;
        [Header("강화와 확장 제어")]
        public List<Button> controlButtons = new List<Button>();
        public bool _isEnchanting = true;
        public bool IsEnchanting
        {
            get => _isEnchanting;
        }

        [Header("패널")]
        [SerializeField]
        private RectTransform scrollArea;
        public RectTransform ScrollArea
        {
            get { return scrollArea; }
            set { scrollArea = value; }
        }
        [SerializeField]
        private GameObject enchantPanel;
        public GameObject EnchantPanel
        {
            get { return enchantPanel; }
            set { enchantPanel = value; }
        }
        private GameObject enchantSubPanel;
        public GameObject EnchantSubPanel
        {
            get { return enchantSubPanel; }
            set { enchantSubPanel = value; }
        }

        [Header("프리팹")]
        [SerializeField]
        private MagicSpell magicSpellSymbolPrefab;
        public bool ableToEnchant = true;
        private ScrollCreationManager _scrollCreationManager;

        void Start()
        {
            _scrollCreationManager = ScrollCreationManager.Instance;
            ResetHint();
        }

        #region 행위 마법 메서드
        private void EnchantMode()
        {
            foreach (var symbolGroup in _scrollCreationManager.ConfirmCircleSymbols)
            {
                symbolGroup.isEnchant = true;
            }
            enchantPanel.SetActive(true);
        }
        /// <summary>
        /// 마법 키워드를 생성하고 상징과 잇는 함수
        /// </summary>
        /// <param name="id">0 : 불, 1 : 물, 2 : 공기, 3 : 흙</param>
        public void CreateSymbol(ElementType elementType, Vector2 mousePosition)
        {
            if (_scrollCreationManager.ableToEnchant)
            {
                var grid = enchantSubPanel.transform.GetChild(0);
                enchantPanel.SetActive(true);
                int id = 0;
                switch (elementType)
                {
                    case ElementType.Fire:
                        id = 0;
                        break;
                    case ElementType.Water:
                        id = 1;
                        break;
                    case ElementType.Wind:
                        id = 2;
                        break;
                    case ElementType.Earth:
                        id = 3;
                        break;
                }

                Instantiate(magicSpellSymbolPrefab, grid.transform).GetComponent<MagicSpell>()
                    .SetSpell(_scrollCreationManager.AddSymbol(new SymbolData(ElementManager.Instance.ElementTable[id].id, SymbolType.Preview), mousePosition));

                //근형 추가 2024-12-04
                switch (elementType)
                {
                    case ElementType.Water:
                        SoundManager.Instance.PlaySFX("Aqua01");
                        break;
                    case ElementType.Fire:
                        SoundManager.Instance.PlaySFX("Ignis01");
                        break;
                    case ElementType.Wind:
                        SoundManager.Instance.PlaySFX("Ventus01");
                        break;
                    case ElementType.Earth:
                        SoundManager.Instance.PlaySFX("Terra01");
                        break;
                    case ElementType.None:
                        break;
                }
            }
        }
        /// <summary>
        /// 새로운 원이 생성됨에 따라 상징을 결합하는 함수
        /// </summary>
        public void CombineSymbol(Symbol symbol)
        {
            var grid = enchantSubPanel.transform.GetChild(0);
            //첫 상징이라면
            Instantiate(magicSpellSymbolPrefab, grid.transform).GetComponent<MagicSpell>()
                .SetSpell(symbol);
        }
        public void DisableEnchant()
        {
            foreach (var symbolGroup in _scrollCreationManager.ConfirmCircleSymbols)
            {
                symbolGroup.isEnchant = false;
            }
            drawerCover.GetComponent<RectTransform>().DOAnchorPosY(240, 0.5f).SetEase(Ease.OutQuad);
            _isEnchanting = false;
        }
        public void EnableEnchant()
        {
            //하드코딩. 나중에 수치 수정요
            drawerCover.GetComponent<RectTransform>().DOAnchorPosY(650, 0.5f).SetEase(Ease.OutQuad);
            _isEnchanting = true;
        }
        public int EnsureMode()
        {
            //미리보기 상징이 없어야함
            if (_scrollCreationManager.PreviewCircleSymbols.Count != 0)
            {
                Debug.LogWarning("부여중입니다. 부여를 완료해주세요.");
                return -1;
            }
            //활성화된 상징이 하나여야 함
            if (!_scrollCreationManager.ableToEnsure)
            {
                Debug.LogWarning("활성화 된 상징이 하나여야 합니다.");
                return -1;
            }

            AttributeManager.Instance.ObtainAttributeById(_scrollCreationManager.SearchTargetSymbols[0].SymbolData.Id);
            _scrollCreationManager.SetScrollEffect();

            // 모든 심볼 확정 상태로 (이미 확정)
            _scrollCreationManager.SetIntensityPolygon();
            _scrollCreationManager.SetRangePolygon();
            _scrollCreationManager.SetEnchantSymobolZoom();

            DisableEnchant();
            return 0;
        }
        /// <summary>
        /// 스크롤 확정
        /// </summary>
        public void Ensure()
        {
            _scrollCreationManager.OffAllMode();
            // 스크롤 생성
            _scrollCreationManager.FinishScroll();

            turnOffOtherControlButtons(4);

            //SoundManager.Instance.PlaySFX("Scroll");
        }

        public void Erase()
        {
            _scrollCreationManager.ClearAllSymbols();
            if (runeDrawer.gameObject.activeSelf)
            {
                turnOnOtherControlButtons();
                ResetControlButtons();
            }

            // turnOnOtherControlButtons();
            DeleteAllElement();
            //enchantPanel.SetActive(false);
            EnableEnchant();
        }
        public void DeleteAllElement()
        {   // 인챈트 패널의 모든 엘레멘트 스펠 오브젝트 삭제.
            if (enchantSubPanel != null)
            {
                var grid = enchantSubPanel.transform.GetChild(0);
                // 자식 오브젝트의 수만큼 반복
                for (int i = grid.transform.childCount - 1; i >= 0; i--)
                {
                    // 자식 오브젝트 가져오기
                    Transform elementSpell = grid.transform.GetChild(i);
                    // 자식 오브젝트 삭제
                    Destroy(elementSpell.gameObject);
                }
            }
        }
        public void DeleteCirtainElement(Symbol symbol)
        {
            if (symbol.MagicSpell != null)
                Destroy(symbol.MagicSpell.gameObject);
        }
        //나중에 위에 상위 속성으로 덮어주는 거 추가
        public void DisableAllElement()
        {
            if (enchantSubPanel != null)
            {
                var grid = enchantSubPanel.transform.GetChild(0);
                // 자식 오브젝트의 수만큼 반복
                for (int i = grid.transform.childCount - 1; i >= 0; i--)
                {
                    Transform elementSpell = grid.transform.GetChild(i);
                    elementSpell.GetComponent<Button>().interactable = false;
                    elementSpell.GetComponent<MagicSpell>().ConfrimSymbol();
                }
            }
        }
        #endregion
        #region 확장, 강화 마법 메서드
        public void OnReinforceClicked(bool weaking)
        {
            ScrollCreationManager.Instance.IntensityPolygonDrawer?.StartTransition(weaking);

            if (TutorialManager.Instance != null)
            {
                if (TutorialManager.Instance.TutorialComplete == false && TutorialManager.Instance.CurrentStepIndex == 29 && weaking == false)
                {   // 강화 약화 실습 단계에서 강화 누를 때 다음 스텝으로
                    TutorialManager.Instance.NextStep();
                }
                if (TutorialManager.Instance.TutorialComplete == false && TutorialManager.Instance.CurrentStepIndex == 30 && weaking == true)
                {   // 강화 약화 실습 단계에서 약화 누를 때 다음 스텝으로
                    TutorialManager.Instance.NextStep();
                }
            }
        }
        public void OnRangeClicked(bool weaking)
        {
            ScrollCreationManager.Instance.RangePolygonDrawer?.StartTransition(weaking);

            if (TutorialManager.Instance != null)
            {
                if (TutorialManager.Instance.TutorialComplete == false && TutorialManager.Instance.CurrentStepIndex == 32 && weaking == false)
                {   // 확장 축소 실습 단계에서 확장 누를 때 다음 스텝으로
                    TutorialManager.Instance.NextStep();
                }
                if (TutorialManager.Instance.TutorialComplete == false && TutorialManager.Instance.CurrentStepIndex == 33 && weaking == true)
                {   // 확장 축소 실습 단계에서 축소 누를 때 다음 스텝으로
                    TutorialManager.Instance.NextStep();
                }
            }
        }

        public void turnOffOtherControlButtons(int mode)
        {
            if (_scrollCreationManager.IntensityPolygonDrawer != null)
            {
                if (_scrollCreationManager.IntensityPolygonDrawer.CurrentDegree == 0 && mode == 1)
                    return;
                else if (_scrollCreationManager.IntensityPolygonDrawer.CurrentDegree == 2 && mode == 0)
                    return;
            }
            if (_scrollCreationManager.RangePolygonDrawer != null)
            {
                if (_scrollCreationManager.RangePolygonDrawer.CurrentDegree == 0 && mode == 3)
                    return;
                else if (_scrollCreationManager.RangePolygonDrawer.CurrentDegree == 2 && mode == 2)
                    return;
            }

            if (DeskManager.Instance.CurrentScroll != null)
            {
                DeskManager.Instance.CurrentScroll.GetComponent<Scroll>().IsAbleToComplete = false;
            }
            for (int i = 0; i < controlButtons.Count; i++)
            {
                if (i != mode)
                {
                    controlButtons[i].interactable = false;
                }
            }
        }

        public void ResetControlButtons()
        {
            controlButtons[0].interactable = true;
            controlButtons[1].interactable = false;
            controlButtons[2].interactable = true;
            controlButtons[3].interactable = false;
        }
        public void turnOnOtherControlButtons()
        {
            for (int i = 0; i < controlButtons.Count; i++)
            {
                controlButtons[i].interactable = true;
                controlButtons[i].GetComponent<RuneStoneEffect>()?.EndRuneStoneEffect();
            }
            if (DeskManager.Instance.CurrentScroll != null)
            {
                DeskManager.Instance.CurrentScroll.GetComponent<Scroll>().IsAbleToComplete = true;
                if (_scrollCreationManager.IntensityPolygonDrawer != null)
                {
                    if (_scrollCreationManager.IntensityPolygonDrawer.CurrentDegree == 0)
                    {
                        controlButtons[1].interactable = false;
                    }
                    else if (_scrollCreationManager.IntensityPolygonDrawer.CurrentDegree == 2)
                    {
                        controlButtons[0].interactable = false;
                    }
                }
                if (_scrollCreationManager.RangePolygonDrawer != null)
                {
                    if (_scrollCreationManager.RangePolygonDrawer.CurrentDegree == 0)
                    {
                        controlButtons[3].interactable = false;
                    }
                    else if (_scrollCreationManager.RangePolygonDrawer.CurrentDegree == 2)
                    {
                        controlButtons[2].interactable = false;
                    }
                }
            }
        }

        #endregion

        #region 초기화
        public void ResetElementSpell()
        {
            var grid = enchantSubPanel.transform.GetChild(0);
            for (int i = 0; i < grid.transform.childCount; i++)
            {
                Destroy(grid.transform.GetChild(i).gameObject);
            }
            //enchantPanel.SetActive(false);
        }
        #endregion

        #region 힌트 관련
        public void DecideHintCount()
        {
            hintCount = DayManager.Instance.todayRequestCount * 2;
            currentHintCount = hintCount;
        }

        public void SetHintText()
        {
            scrollBookhintText.text = currentHintCount.ToString();
        }

        public bool UseHint()
        {
            if (currentHintCount > 0)
            {
                currentHintCount--;
                SetHintText();
                return true;
            }
            else return false;
        }

        public void ResetHint()
        {
            DecideHintCount();
            SetHintText();
        }
        #endregion
    }
}