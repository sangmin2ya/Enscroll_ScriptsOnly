using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using Game.Symbols;
using Game.Attribute;
using Game.Element;
using Game.Desk;
using DG.Tweening;
using ntw.CurvedTextMeshPro;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using Game.Tutorial;
using Unity.VisualScripting;
using UnityEngine.Localization.Settings;


namespace Game.Scroll
{
    public class ScrollCreationManager : Singleton<ScrollCreationManager>
    {
        #region 스크롤 심볼 관리
        [Header("조합 소요 시간")]
        [SerializeField]
        private float combinationDuration = 1f;
        public float CombinationDuration => combinationDuration;

        [Header("심볼 리스트")]
        [SerializeField]
        private List<Symbol> entireSymbols = new List<Symbol>(); // 전체 심볼 리스트
        [SerializeField]
        private List<Symbol> searchTargetSymbols = new List<Symbol>(); // 탐색 대상 심볼 리스트 (미리보기 원이 생성되고 그 자식들은 제외)
        public List<Symbol> SearchTargetSymbols => searchTargetSymbols;
        [SerializeField]
        private List<List<Symbol>> connectedSymbolGroups = new List<List<Symbol>>(); // 연결된 심볼 그룹 리스트 이를 대상으로 외접원을 그림
        [SerializeField]
        private List<Symbol> previewCircleSymbols = new List<Symbol>(); // 미리보기 심볼 리스트
        public List<Symbol> PreviewCircleSymbols => previewCircleSymbols;
        [SerializeField]
        private List<Symbol> confirmCircleSymbols = new List<Symbol>(); // 확정 심볼 리스트
        public List<Symbol> ConfirmCircleSymbols => confirmCircleSymbols;
        [SerializeField]
        [Range(0, 8)]
        private int currentSymbolCount = 0; // 현재 심볼 개수\\
        public int CurrentSymbolCount => currentSymbolCount;
        private int currentScrollLevel = 1; // 현재 스크롤 레벨
        public int CurrentScrollLevel => currentScrollLevel;
        public int currentSymbolLimit
        {
            get
            {
                switch (currentScrollLevel)
                {
                    case 1:
                        return 2;
                    case 2:
                        return 3;
                    case 3:
                        return 5;
                    case 4:
                        return 8;
                    default:
                        return -1;
                }
            }
        }
        public bool ableToEnchant => currentSymbolCount < currentSymbolLimit; // 8개 이하일 때만 생성 가능
        public bool ableToEnsure => searchTargetSymbols.Count == 1; // 1개일 때만 확정 가능

        #endregion

        #region 강화, 확장
        [Header("강화, 확장")]
        [SerializeField]
        private float transitionDuration = 2f;
        public float TransitionDuration => transitionDuration;
        private IntensityPolygonDrawer intensityDrawer = null;
        public IntensityPolygonDrawer IntensityPolygonDrawer { get { return intensityDrawer; } }
        private RangePolygonDrawer rangeDrawer = null;
        public RangePolygonDrawer RangePolygonDrawer { get { return rangeDrawer; } }
        [SerializeField]
        private string scrollEffect;
        [SerializeField]
        private string scrollDuration;
        #endregion

        #region 스크롤 완성 꾸미기
        [SerializeField]
        private Image runeImage;
        [SerializeField]
        private TextProOnACircle runeText;
        [SerializeField] FinalCircle finalCircle;
        private float ratio = 1;
        #endregion

        #region 참조

        [Header("참조")]
        [SerializeField]
        private Canvas canvas; // 캔버스 참조
        public Canvas Canvas => canvas;
        [SerializeField]
        private Transform scrollTransform; // 문양 생성 위치
        [SerializeField]
        public Transform ScrollTransform { get { return scrollTransform; } }
        private Desk.Scroll scroll; // 현재 스크롤
        [SerializeField]
        private SymbolDrawer SymbolPrefab; // 외접원 프리팹
        [SerializeField]
        private OuterCircleDrawer outerCirclePrefab; // 외접원 프리팹
        [SerializeField]
        private IntensityPolygonDrawer intensityPolygonDrawerPrefab; // 강화 다각형 프리팹
        [SerializeField]
        private RangePolygonDrawer rangePolygonDrawerPrefab; // 확장 다각형 프리팹  

        [Header("코루틴")]
        private Coroutine rotateRuneCoroutine;
        private Coroutine textingRuneCoroutine;

        void Start()
        {
            #region 저장 데이터
            currentScrollLevel = GameData.SaveInfo?.parchmentLevel ?? currentScrollLevel;
            GameManager.Instance.UpgradeScroll();
            #endregion
        }

        /// <summary>
        /// 캔버스 생성될때마다 호출되는 메서드
        /// </summary>
        public void RegisterNewScroll(Desk.Scroll scroll, Transform scrollTransform)
        {
            this.scroll = scroll;
            this.scrollTransform = scrollTransform;
            this.runeImage = scroll.RuneImage;
            this.runeText = scroll.RuneText;
            this.finalCircle = scroll.FinalCircle;
            MagicController.Instance.ResetControlButtons();
        }
        #endregion

        #region 스크롤 초기화
        public void ResetScroll()
        {
            entireSymbols.Clear();
            searchTargetSymbols.Clear();
            connectedSymbolGroups.Clear();
            previewCircleSymbols.Clear();
            confirmCircleSymbols.Clear();
            ClearAllSymbols();
            if (DeskManager.Instance.CurrentScroll != null)
            {
                Destroy(DeskManager.Instance.CurrentScroll.gameObject);
            }
        }

        public void UpgradeScroll(int level, bool popupOpen = true)
        {
            currentScrollLevel = Math.Max(level, currentScrollLevel);
            if (currentScrollLevel == 4)
            {
                SteamAchievementManager.Instance.UnlockAchievement("NEW_ACHIEVEMENT_1_3");
            }
            if (popupOpen)
                PopupManager.Instance.OpenParchmentPopup();
            ResetScroll();
            DeskManager.Instance.CreateNewScroll();
        }
        #endregion

        #region 심볼 생성 및 제거
        /// <summary>
        /// 심볼 추가 생성
        /// </summary>
        /// /// <param name="symbolData">생성할 심볼의 데이터</param>
        public Symbol AddSymbol(SymbolData symbolData, Vector2 position)
        {
            SymbolDrawer symbol = Instantiate(SymbolPrefab, scrollTransform);
            symbol.transform.position = position;
            symbol.Init(AssignUniqueId(), symbolData);

            entireSymbols.Add(symbol);

            // if (TutorialManager.Instance != null && TutorialManager.Instance.TutorialComplete == false && TutorialManager.Instance.CurrentStepIndex == 14)
            // {   // 교수님의 첫 번째 편지를 받고 심볼 2개를 양피지에 그린 순간.
            //     if (entireSymbols.Count >= 2)
            //     {
            //         TutorialManager.Instance.NextStep();
            //     }
            // }

            searchTargetSymbols.Add(symbol);
            currentSymbolCount++;
            scroll.FillAttributeCount();
            SetScrollEffect();

            return symbol;
        }
        /// <summary>
        /// 심볼 제거
        /// </summary>
        /// <param name="symbol">제거할 심볼</param>
        public void RemoveSymbol(Symbol symbol)
        {
            if (!entireSymbols.Contains(symbol))
            {
                Debug.LogWarning("해당 심볼이 존재하지 않습니다.");
                return;
            }
            // 외접원일 경우 하위 심볼들도 제거 ( 속성 단위 제거를 위해서 )
            if (symbol is OuterCircleDrawer)
            {
                var allChildren = symbol.GetEveryChildSymbols();
                foreach (Symbol child in allChildren)
                {
                    entireSymbols.Remove(child);
                    confirmCircleSymbols.Remove(child);
                    ReclaimUniqueId(symbol.UniqueId);
                    if (child is SymbolDrawer)
                    {
                        currentSymbolCount--;
                        scroll.DeleteAttributeCount();
                    }
                    child.DestroyObject();
                }
                entireSymbols.Remove(symbol);
                searchTargetSymbols.Remove(symbol);
                confirmCircleSymbols.Remove(symbol);
                ReclaimUniqueId(symbol.UniqueId);
            }
            // 미리보기 상태가 아닐경우
            else if (!searchTargetSymbols.Contains(symbol))
            {
                Debug.Log("하위 심볼 제거" + symbol.UniqueId);
                foreach (Symbol child in searchTargetSymbols)
                {
                    var everyChildren = child.GetEveryChildSymbols();
                    if (everyChildren.Contains(symbol))
                    {
                        //하위 자손들 전부 삭제
                        foreach (var everyone in everyChildren)
                        {
                            entireSymbols.Remove(everyone);
                            confirmCircleSymbols.Remove(everyone);
                            ReclaimUniqueId(everyone.UniqueId);
                            everyone.DestroyObject();
                            if (everyone is SymbolDrawer)
                            {
                                currentSymbolCount--;
                                scroll.DeleteAttributeCount();
                                MagicController.Instance.DeleteCirtainElement(everyone);
                            }
                        }
                        entireSymbols.Remove(child);
                        previewCircleSymbols.Remove(child);
                        confirmCircleSymbols.Remove(child);
                        searchTargetSymbols.Remove(child);
                        ReclaimUniqueId(child.UniqueId);
                        child.DestroyObject();
                        break;
                    }
                }
            }
            else
            {
                entireSymbols.Remove(symbol);
                searchTargetSymbols.Remove(symbol);
                currentSymbolCount--;
                scroll.DeleteAttributeCount();
                ReclaimUniqueId(symbol.UniqueId);
            }
            // 제거 후 심볼들을 다시 그룹화해서 외접원 생성
            GeneratePreviewCircleAtOnce(symbol); //=> 이렇게 하면 제거할 때마다 전체 심볼을 대상으로 외접원을 생성하므로 비효율적. 생성시에도 참고하려면 이걸로
            //GeneratePreviewCircleOnCollision();
            // SetScrollEffect();
            // SetScrollDuration();
            symbol.DestroyObject();
            SetScrollEffect();
        }
        /// <summary>
        /// 모든 심볼 제거
        /// </summary>
        public void ClearAllSymbols()
        {
            List<Symbol> symbols = FindObjectsOfType<Symbol>().ToList();
            foreach (Symbol symbol in symbols)
            {
                symbol.DestroyObject();
            }

            if (intensityDrawer != null)
            {
                intensityDrawer.DestroyObject();
            }
            if (rangeDrawer != null)
            {
                rangeDrawer.DestroyObject();
            }
            if (textingRuneCoroutine != null)
                StopCoroutine(textingRuneCoroutine);
            if (rotateRuneCoroutine != null)
                StopCoroutine(rotateRuneCoroutine);

            ResetData();
        }
        public void ResetData()
        {
            if (DeskManager.Instance.CurrentScroll == null)
            {
                return;
            }
            if (runeImage != null)
                runeImage.fillAmount = 0;

            finalCircle?.ClearCirclePoints();

            entireSymbols.Clear();
            searchTargetSymbols.Clear();
            connectedSymbolGroups.Clear();
            previewCircleSymbols.Clear();
            ConfirmCircleSymbols.Clear();
            currentSymbolCount = 0;
            intensityDrawer = null;
            rangeDrawer = null;
            assignedIds.Clear();

            if (scroll != null)
            {
                scroll.ScrollText.GetComponent<Button>().enabled = true;
                scroll.ScrollText.color = scroll.NormalColor;
                SetScrollEffect();
                SetScrollDuration();
            }
        }
        public void DragSymbol(Symbol symbol)
        {
            searchTargetSymbols.Remove(symbol);
            // if (searchTargetSymbols.Contains(symbol))
            // {
            //     GeneratePreviewCircleOnCollision();
            // }
            // else
            // {
            GeneratePreviewCircleAtOnce(symbol);
            // }

        }
        public void DropSymbol(Symbol symbol)
        {
            if (!searchTargetSymbols.Contains(symbol))
            {
                searchTargetSymbols.Add(symbol);
            }
            // if (searchTargetSymbols.Contains(symbol))
            // {
            //     GeneratePreviewCircleOnCollision();
            // }
            // else
            // {
            GeneratePreviewCircleAtOnce(symbol);
            SetScrollEffect();
            // }
        }

        #endregion

        #region 미리보기 절차적 생성

        /// <summary>
        /// 현재 모든 심볼들을 대상으로 겹친 심볼들을 찾아서 그룹화
        /// </summary>
        private void GroupLinkedSymbols()
        {
            connectedSymbolGroups.Clear();
            var visited = new HashSet<int>(); // 방문한 심볼의 ID를 저장할 해시셋
            foreach (var symbol in searchTargetSymbols)
            {
                if (symbol is OuterCircleDrawer)
                {
                    if (!visited.Contains(symbol.UniqueId))
                    {
                        var group = new List<Symbol>();
                        FindConnectedSymbols(symbol, group, visited);
                        if (IsGroupCombinationValid(group) != null)
                        {
                            connectedSymbolGroups.Add(group); // 조합이 있을 경우에만 그룹으로 추가
                        }
                        else
                        {
                            // 조합이 없으면 개별 심볼로 그룹화하여 추가
                            foreach (var singleSymbol in group)
                            {
                                connectedSymbolGroups.Add(new List<Symbol> { singleSymbol });
                            }
                        }
                    }
                }
            }
            foreach (var symbol in searchTargetSymbols)
            {
                if (symbol is SymbolDrawer)
                {
                    if (!visited.Contains(symbol.UniqueId))
                    {
                        var group = new List<Symbol>();
                        FindConnectedSymbols(symbol, group, visited);
                        if (IsGroupCombinationValid(group) != null)
                        {
                            connectedSymbolGroups.Add(group); // 조합이 있을 경우에만 그룹으로 추가
                        }
                        else
                        {
                            // 조합이 없으면 개별 심볼로 그룹화하여 추가
                            foreach (var singleSymbol in group)
                            {
                                connectedSymbolGroups.Add(new List<Symbol> { singleSymbol });
                            }
                        }

                    }
                }
            }
        }
        /// <summary>
        /// 재귀를 통해 심볼을 기준으로 연결된 심볼들을 찾아서 그룹화
        /// </summary>
        private void FindConnectedSymbols(Symbol symbol, List<Symbol> group, HashSet<int> visited)
        {
            visited.Add(symbol.UniqueId);
            group.Add(symbol);

            foreach (var linkedSymbol in symbol.LinkedSymbols)
            {
                if (!visited.Contains(linkedSymbol.UniqueId) && searchTargetSymbols.Contains(linkedSymbol))
                {
                    FindConnectedSymbols(linkedSymbol, group, visited);
                }
            }
        }
        /// <summary>
        /// 그룹의 조합이 유효한지 확인하는 메서드
        /// </summary>
        private AttributeInfo IsGroupCombinationValid(List<Symbol> group)
        {
            // 그룹의 모든 심볼의 UniqueId를 문자열 리스트로 변환하여 조합 검사
            // 2개의 심볼이 아니면 null 반환
            if (group.Count != 2)
            {
                return null;
            }
            List<string> spellIds = new List<string>();
            foreach (var symbol in group)
            {
                spellIds.Add(symbol.SymbolData.Id);
            }
            // FindCombinedAttributeSpell 함수를 이용해 조합이 존재하는지 확인
            return FindCombinedAttributeSpell(spellIds);
        }
        /// <summary>
        /// 연결된 심볼들을 기반으로 조합된 속성을 찾아 반환
        /// </summary>
        public AttributeInfo FindCombinedAttributeSpell(List<string> inputSpellIds)
        {
            List<string> spellNames = new List<string>();
            foreach (string id in inputSpellIds)
            {
                string spellName = ElementManager.Instance.FindKoreanNameById(id);
                if (spellName != "")
                {
                    spellNames.Add(spellName);
                }
                else
                {
                    spellName = AttributeManager.Instance.FindKoreanNameById(id);
                    spellNames.Add(spellName);
                }
            }
            spellNames.Sort();

            foreach (AttributeInfo attribute in AttributeManager.Instance.attributeTable)
            {
                if (attribute.isMatch(spellNames))
                {
                    return attribute;
                }
            }
            return null;
        }

        /// <summary>
        /// 외접원을 그리고 탐색 대상에 추가하고 하위 심볼들은 탐색대상에서 제외
        /// </summary>
        private void UpdateSearchTargetSymbols()
        {
            foreach (var group in connectedSymbolGroups)
            {
                bool skipGroup = false;
                foreach (var irrelateSymbol in previewCircleSymbols)
                {
                    if (group.Contains(irrelateSymbol.PreviewChildren[0]) || group.Contains(irrelateSymbol.PreviewChildren[1]))
                    {
                        skipGroup = true;
                    }
                }
                if (skipGroup)
                {
                    continue;
                }
                // 그룹에 속한 심볼들 제외하고 외접원 심볼로 대체
                // 그룹에 속한 심볼이 하나라면 해당 심볼을 그대로 추가
                if (group.Count > 0)
                {
                    Symbol symbol = CreateOuterCircleSymbol(group);
                    if (group.Count > 1)
                        previewCircleSymbols.Add(symbol); //저장용
                }
            }
            //searchTargetSymbols = updatedTargets;
        }

        /// <summary>
        /// 주어진 심볼 그룹을 기반으로 외접원을 표현하는 Symbol 생성 (임시 메서드, 실제 구현 필요)
        /// </summary>
        private Symbol CreateOuterCircleSymbol(List<Symbol> group)
        {
            // 그룹에 속한 심볼이 하나라면 해당 심볼을 반환
            if (group.Count == 1)
            {
                return group[0];
            }
            // 그룹에 속한 심볼들의 중심점을 기준으로 외접원을 그리고 반환
            // 이때 속성의 ID를 조합하여 외접원의 속성을 결정하고 저장은 다시 ID로 바꿔서 저장
            string combinedAttributeId = AttributeManager.Instance.FindIdByName(IsGroupCombinationValid(group).GetKoreanName());
            OuterCircleDrawer outerCircle = Instantiate(outerCirclePrefab, scrollTransform);
            // 외접원의 IncludedSymbols에 하위 심볼들을 추가
            outerCircle.SetIncludedSymbols(group);
            // 외접원의 데이터 초기화
            outerCircle.Init(AssignUniqueId(), new SymbolData(combinedAttributeId, SymbolType.Preview));
            // 다음 탐색을 위해 충돌처리 가능하도록 설정
            outerCircle.ableToCollisionWithOther = true;

            return outerCircle;
        }
        /// <summary>
        /// 모든 타겟 심볼들이 그룹화될 때까지 반복하는 메소드.
        /// --------------------실행 조건--------------------
        /// 1. 마우스 클릭에는 해당 심볼제외하고 실행
        /// 2. 드롭에는 모든 심볼을 대상으로 실행
        /// 3. 심볼 추가에는 실행하지 않도록.
        /// 4. 하지만 제거에는 실행되어야 한다.
        /// </summary>
        public void GeneratePreviewCircleAtOnce(Symbol focusSymbol)
        {
            // 모든 심볼 대상으로, 미리보기는 해당 안됨. 전부 삭제해준다.
            // searchTargetSymbols = new List<Symbol>(entireSymbols);
            // Debug.Log("미리보기 원 생성 및 제거" + focusSymbol.UniqueId);
            for (int i = 0; i < previewCircleSymbols.Count; i++)
            {
                foreach (Symbol child in previewCircleSymbols[i].PreviewChildren)
                {
                    if (previewCircleSymbols[i].GetEveryChildSymbols().Contains(focusSymbol) || child.LinkedSymbols.Contains(focusSymbol))
                    {
                        if (previewCircleSymbols[i].PreviewChildren == null)
                        {
                            continue;
                        }
                        child.ResetPreviewGroup();
                        ReclaimUniqueId(previewCircleSymbols[i].UniqueId);
                        Destroy(previewCircleSymbols[i].gameObject);
                        previewCircleSymbols.RemoveAt(i);
                        i--;
                        break;
                    }
                }
            }
            // if (previewCircleSymbols.Count > 0)
            // {
            //     foreach (Symbol symbol in previewCircleSymbols)
            //     {
            //         foreach (Symbol child in symbol.PreviewChildren)
            //         {
            //             if (symbol.GetEveryChildSymbols().Contains(focusSymbol) || child.LinkedSymbols.Contains(focusSymbol))
            //             {
            //                 ReclaimUniqueId(child.UniqueId);
            //                 Destroy(child.gameObject);
            //             }
            //         }
            //     }
            //     previewCircleSymbols.
            // }

            GroupLinkedSymbols(); // 겹친 심볼들을 그룹화
            UpdateSearchTargetSymbols(); // 외접원을 탐색 대상에 추가하고 하위 심볼들 제외

            // 모든 외접원이 그려질 때 소리 재생하게 ㅇㅇ
            // if (previewCircleSymbols.Count > 0)
            // {
            //     Invoke("PlayCircleSound", combinationDuration);
            // }
        }
        private void PlayCircleSound()
        {
            SoundManager.Instance.PlaySFX("Circlesounds01");
        }

        public void GeneratePreviewCircleOnCollision()
        {
            GroupLinkedSymbols(); // 겹친 심볼들을 그룹화
            UpdateSearchTargetSymbols(); // 외접원을 탐색 대상에 추가하고 하위 심볼들 제외

        }

        #endregion

        #region 스크롤 효과 출력

        /// <summary>
        /// 스크롤의 현재 효과를 띄워주는 메서드.
        /// 현재는 하나의 속성만 존재할 경우에만 효과를 띄워줌. 나머지는 공백으로 표시
        /// </summary>
        public void SetScrollEffect()
        {
            if (searchTargetSymbols.Count == 1)
            {
                // 여기서 알고있는 속성일 경우에만 효과를 띄워준다.
                // 모르는 속성은 ??? 으로 표시, 알고 있는 속성은 이름으로 표시 한번 만들면 아는 것이라고 판단
                if (intensityDrawer == null)
                {
                    scrollEffect = AttributeManager.Instance.FindAttributeById(searchTargetSymbols[0].SymbolData.Id)?.powers[0];
                }
                else
                {
                    scrollEffect = AttributeManager.Instance.FindAttributeById(searchTargetSymbols[0].SymbolData.Id)?.powers[intensityDrawer.CurrentDegree];
                    if (searchTargetSymbols[0].SymbolData.Id == "Attribute_6" && intensityDrawer.CurrentDegree == 2)
                    {
                        SteamAchievementManager.Instance.UnlockAchievement("NEW_ACHIEVEMENT_1_2");
                    }
                }

                // 효과 지속시간
                if (rangeDrawer == null)
                {
                    scrollDuration = "|";
                }
                else
                {
                    scrollDuration = rangeDrawer.CurrentDegree.ToString();
                    switch (scrollDuration)
                    {
                        case "0":
                            scrollDuration = "|";
                            break;
                        case "1":
                            scrollDuration = "||";
                            break;
                        case "2":
                            scrollDuration = "|||";
                            break;
                    }
                }
                SetScrollEffect(scrollEffect == null ? "" : scrollEffect);
                // SetScrollDuration(scrollEffect == null ? "" : "스크롤 지속레벨", scrollEffect == null ? "" : scrollDuration);
            }
            else
            {
                SetScrollEffect("");
                //SetScrollDuration("", "");
            }
            // scroll.UpdateAttribute(searchTargetSymbols);
        }
        public void SetScrollDuration()
        {
            if (rangeDrawer == null)
            {
                scrollDuration = "|";
            }
            else
            {
                scrollDuration = rangeDrawer.CurrentDegree.ToString();
                switch (scrollDuration)
                {
                    case "0":
                        scrollDuration = "|";
                        break;
                    case "1":
                        scrollDuration = "||";
                        break;
                    case "2":
                        scrollDuration = "|||";
                        break;
                }
            }
            SetScrollDuration(scrollEffect == null ? "" : "지속시간", scrollEffect == null ? "" : scrollDuration);
        }
        private void SetScrollEffect(string effectName)
        {
            var dissolve = scroll.ScrollText.transform.parent.GetComponent<UIDissolve>();
            DOTween.Kill(dissolve.location);
            //투명도 조절과 흐물거리는 효과
            DOTween.To(() => dissolve.location, x => dissolve.location = x, 1, 0.5f).OnComplete(() =>
            {
                scroll.ScrollText.text = effectName;
                DOTween.To(() => dissolve.location, x => dissolve.location = x, 0, 0.5f).OnComplete(() =>
                {
                    scroll.ShowTextGuide(effectName == "" ? false : true);
                });
            });
        }
        private void SetScrollDuration(string effectDurationInfoText, string effectDuration)
        {
            if (!effectDurationInfoText.Equals(""))
            {
                effectDurationInfoText = LocalizationSettings.StringDatabase.GetLocalizedString("Localizing", "ScrollDuration");
            }

            DOTween.Kill(scroll.ScrollDurationInfoText);
            DOTween.Kill(scroll.ScrollDurationText);
            DOTween.To(() => scroll.ScrollDurationInfoText.color, x => scroll.ScrollDurationInfoText.color = x, new Color(scroll.ScrollDurationInfoText.color.r, scroll.ScrollDurationInfoText.color.g, scroll.ScrollDurationInfoText.color.b, 0), 0.2f).OnComplete(() =>
            {
                scroll.ScrollDurationInfoText.text = effectDurationInfoText;
                DOTween.To(() => scroll.ScrollDurationInfoText.color, x => scroll.ScrollDurationInfoText.color = x, new Color(scroll.ScrollDurationInfoText.color.r, scroll.ScrollDurationInfoText.color.g, scroll.ScrollDurationInfoText.color.b, 1), 0.3f);
            });
            DOTween.To(() => scroll.ScrollDurationText.color, x => scroll.ScrollDurationText.color = x, new Color(scroll.ScrollDurationText.color.r, scroll.ScrollDurationText.color.g, scroll.ScrollDurationText.color.b, 0), 0.2f).OnComplete(() =>
            {
                scroll.ScrollDurationText.text = effectDuration;
                DOTween.To(() => scroll.ScrollDurationText.color, x => scroll.ScrollDurationText.color = x, new Color(scroll.ScrollDurationText.color.r, scroll.ScrollDurationText.color.g, scroll.ScrollDurationText.color.b, 1), 0.3f);
            });
        }
        #endregion

        #region 확정 문양 생성

        public int confirmCertainSymbol(Symbol symbol)
        {
            if (symbol.GetComponent<SymbolDrawer>() != null)
            {
                Debug.LogWarning("외접원만 확정가능합니다.");
            }
            // 확정문양으로 
            entireSymbols.Add(symbol);
            confirmCircleSymbols.Add(symbol);
            searchTargetSymbols.Add(symbol);
            previewCircleSymbols.Remove(symbol);
            //하단 목록에서 지워주기
            MagicController.Instance.CombineSymbol(symbol);
            foreach (var child in symbol.PreviewChildren)
            {
                MagicController.Instance.DeleteCirtainElement(child);
            }
            //SoundManager.Instance.PlaySFX("Circlesounds01");
            SoundManager.Instance.PlaySFX("OutCircle");


            foreach (Symbol child in symbol.GetEveryChildSymbols())
            {
                searchTargetSymbols.Remove(child);
            }
            symbol.GetComponent<OuterCircleDrawer>().ConfirmOuterCircle();

            SetScrollEffect();

            // if (TutorialManager.Instance != null && TutorialManager.Instance.TutorialComplete == false && TutorialManager.Instance.CurrentStepIndex == 14)
            // {
            //     TutorialManager.Instance.NextStep();
            // }

            return 0;
        }
        #endregion

        #region 스크롤 완성 (강화, 확장, 꾸미기)
        public void SetRuneText(MinimumEnclosingCircle.Circle circle)
        {
            runeText.transform.position = confirmCircleSymbols[0].transform.position;

            float radius = circle.radius;
            runeText.Radius = radius;

            TextMeshProUGUI textMeshPro = runeText.GetComponent<TextMeshProUGUI>();
            textMeshPro.characterSpacing = 200f;

            // string text = "helloweareintheboxfromkraftonjunglegamelabsecondenscrollwillbeoverwhelmingpositiveplease";
            string text = "helloweareintheboxfromkraftonjunglegamelab";
            if (radius <= 60)
            {
                text = "helloweareintheboxfromkraf";
            }
            else if (radius >= 200)
            {
                text = "helloweareintheboxfromkraftonjunglegamelabsecondenscrollwillbeoverwhelmingpositive";
                textMeshPro.characterSpacing = 100f;
            }

            textMeshPro.SetText(text);
        }

        IEnumerator AnimateRuneText()
        {
            float duration = 1f;
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                runeImage.fillAmount = elapsedTime / duration;
                yield return null;
            }

            runeImage.fillAmount = 1;

            rotateRuneCoroutine = StartCoroutine(RotateRuneText());
        }

        IEnumerator RotateRuneText()
        {
            while (!runeImage.gameObject.IsDestroyed())
            {
                runeText.AngularOffset = Mathf.Repeat(runeText.AngularOffset + Time.deltaTime * 10f, 360f);
                yield return null;
            }
        }

        public OutShapeType GetShapeTypeOfIntensityDrawer()
        {
            return intensityDrawer.currentShape;
        }

        public void SetIntensityPolygon()
        {
            IntensityPolygonDrawer intensityPolygon = Instantiate(intensityPolygonDrawerPrefab, scrollTransform);
            if (intensityDrawer != null)
            {
                Destroy(intensityDrawer.gameObject);
            }
            intensityDrawer = intensityPolygon;
            intensityDrawer.SetInscribedCircle(SearchTargetSymbols[0] as OuterCircleDrawer);
        }

        public void SetRangePolygon()
        {
            RangePolygonDrawer rangePolygon = Instantiate(rangePolygonDrawerPrefab, ScrollTransform);
            if (rangeDrawer != null)
            {
                Destroy(rangeDrawer.gameObject);
            }

            rangeDrawer = rangePolygon;
            intensityDrawer.RangePolygonDrawer = rangeDrawer;
            rangeDrawer.SetInscribedCircle(SearchTargetSymbols[0] as OuterCircleDrawer);
        }
        public void IntenseModeOn()
        {
            RangePolygonDrawer.FinishRange();

            scroll.ScrollDurationText.transform.DOKill();
            scroll.ScrollDurationInfoText.transform.DOKill();
            scroll.ScrollDurationText.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.InQuad);
            scroll.ScrollDurationInfoText.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.InQuad).onComplete = () =>
            {
                scroll.ScrollText.transform.DOScale(Vector3.one * 1.02f, 0.5f).SetLoops(-1, LoopType.Yoyo);
                // intensityDrawer.StartTransition();
            };
        }
        public void RangeModeOn()
        {
            intensityDrawer.FinishIntensity();

            scroll.ScrollText.transform.DOKill();
            scroll.ScrollText.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.InQuad).onComplete = () =>
            {
                scroll.ScrollDurationText.transform.DOScale(Vector3.one * 1.05f, 0.5f).SetLoops(-1, LoopType.Yoyo);
                scroll.ScrollDurationInfoText.transform.DOScale(Vector3.one * 1.05f, 0.5f).SetLoops(-1, LoopType.Yoyo);
                // rangeDrawer.StartTransition();
            };
        }
        public void OffAllMode()
        {
            scroll.ScrollDurationText.transform.DOKill();
            scroll.ScrollDurationInfoText.transform.DOKill();
            scroll.ScrollText.transform.DOKill();
            scroll.ScrollText.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.InQuad);
            scroll.ScrollDurationText.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.InQuad);
            scroll.ScrollDurationInfoText.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.InQuad);

            if (intensityDrawer != null)
            {
                intensityDrawer.FinishIntensity();
            }
            if (rangeDrawer != null)
            {
                rangeDrawer.FinishRange();
            }

        }

        #endregion

        #region 스크롤 완성!!!!
        public string FinishScroll()
        {
            SetFinishSymbolZoom();
            // 스크롤 정보 설정 (제출시 판단을 위해)
            scroll.ScrollInfo = new ScrollInfo(
                searchTargetSymbols[0].SymbolData.Id,
                intensityDrawer.CurrentDegree,
                rangeDrawer.CurrentDegree + 1,
                MoneyManager.Instance.GetPaymentValue(DayManager.Instance.day),
                string.Format("{0}{1}", AttributeManager.Instance.FindKoreanNameById(searchTargetSymbols[0].SymbolData.Id), intensityDrawer.CurrentDegree)
                );
            return scroll.ScrollInfo.AttributeId;
        }

        public void SetFinishSymbolZoom()
        {
            List<Vector2> lastPoints = new List<Vector2>();

            if (intensityDrawer != null)
            {
                if (intensityDrawer.CurrentDegree != 0)
                    lastPoints = intensityDrawer.points;
            }
            if (rangeDrawer != null)
            {
                if (rangeDrawer.CurrentDegree != 0)
                    lastPoints = RangePolygonDrawer.lastPoints;
            }

            if (lastPoints.Count > 0)
            {
                finalCircle.SetCircle(lastPoints);
                SetRuneText(finalCircle.GetCircle());
            }
            else
            {
                SetRuneText((confirmCircleSymbols[confirmCircleSymbols.Count - 1] as OuterCircleDrawer).Circle);
            }

            float scrollWidth = 200;
            float width;
            width = runeText.GetComponent<TextProOnACircle>().Radius;
            ratio = MathF.Min(MathF.Max(scrollWidth / width, 0.45f), ratio);

            Transform lastCircle = SearchTargetSymbols[0].transform;

            finalCircle.transform.localScale = Vector3.one * ratio;
            runeText.transform.localPosition = Vector3.zero;
            // runeText.Radius = MathF.Min(200, runeText.Radius);
            runeText.Radius *= ratio;

            if (textingRuneCoroutine != null)
                StopCoroutine(textingRuneCoroutine);
            if (rotateRuneCoroutine != null)
                StopCoroutine(rotateRuneCoroutine);

            DG.Tweening.Sequence sequence = DOTween.Sequence();

            if (ratio < 1f)
            {
                sequence.Append(lastCircle.DOScale(Vector3.one * ratio, 0.5f).SetEase(Ease.InQuad))
                        .Join(intensityDrawer.transform?.DOScale(Vector3.one * ratio, 0.5f).SetEase(Ease.InQuad))
                        .Join(rangeDrawer.transform?.DOScale(Vector3.one * ratio, 0.5f).SetEase(Ease.InQuad))
                        .AppendCallback(() => { if (rangeDrawer?.currentShape != OutShapeType.Circle) finalCircle.DrawCircle(); })
                        .JoinCallback(() => { textingRuneCoroutine = StartCoroutine(AnimateRuneText()); });
            }
            else
            {
                sequence.AppendCallback(() => { if (rangeDrawer?.currentShape != OutShapeType.Circle) finalCircle.DrawCircle(); })
                        .JoinCallback(() => { textingRuneCoroutine = StartCoroutine(AnimateRuneText()); });
            }

            sequence.Play();
        }

        public void SetEnchantSymobolZoom()
        {
            //220은 스크롤의 크기에 따라 다를듯... 추후 수정해야함. 동적으로 하려면 스크롤 전체크기랑 부모의 부모의 부모 다 계산해야함
            float scrollWidth = 200;
            float width;

            width = (SearchTargetSymbols[0] as OuterCircleDrawer).Circle.radius;

            int expandRatio = (int)width / 10;
            width += expandRatio * 5;
            ratio = MathF.Min(MathF.Max(scrollWidth / width, 0.5f), 1f);

            foreach (Symbol everySymbol in entireSymbols)
            {
                Color newColor;
                UnityEngine.ColorUtility.TryParseHtmlString("#734228", out newColor);
                everySymbol.DOColor(newColor, 0.5f);
            }

            SearchTargetSymbols[0].transform.DOLocalMove(Vector2.zero, 0.5f).SetEase(Ease.OutQuad);
            SearchTargetSymbols[0].transform.DOScale(Vector3.one * ratio, 0.5f).SetEase(Ease.OutQuad);

            if (intensityDrawer != null)
            {
                intensityDrawer.transform.DOLocalMove(Vector2.zero, 0.5f).SetEase(Ease.OutQuad);
                intensityDrawer.transform.DOScale(Vector3.one * ratio, 0.5f).SetEase(Ease.OutQuad);
            }
            if (rangeDrawer != null)
            {
                rangeDrawer.transform.DOLocalMove(Vector2.zero, 0.5f).SetEase(Ease.OutQuad);
                rangeDrawer.transform.DOScale(Vector3.one * ratio, 0.5f).SetEase(Ease.OutQuad);
            }

            runeText.transform.localPosition = Vector3.zero;
            // DOTween.To(() => runeText.Radius, x => runeText.Radius = x, MathF.Min(200, runeText.Radius), 0.1f).SetEase(Ease.OutQuad);
        }


        #endregion

        #region 고유 ID 할당
        private List<int> assignedIds = new List<int>(); // 할당된 고유 ID 리스트
        /// <summary>
        /// 고유 id(uniqueId) 할당해주는 메서드.
        /// </summary>
        public int AssignUniqueId()
        {
            for (int i = 0; i <= 100; i++)
            {
                if (!assignedIds.Contains(i))
                {
                    assignedIds.Add(i);
                    return i;
                }
            }
            throw new Exception("No available unique IDs");
        }

        /// <summary>
        /// 할당된 id 회수하는 메서드.
        /// </summary>
        public void ReclaimUniqueId(int id)
        {
            assignedIds.Remove(id);
        }
        #endregion

        #region 저장 데이터 로드
        public void LoadParchmentLevel(int level)
        {
            currentScrollLevel = level;
        }
        #endregion

        #region QA용 임시
        public void SetTransitionDuration(float value) { transitionDuration = value; }
        public void SetCombinationDuration(float value) { combinationDuration = value; }
        #endregion
    }
}