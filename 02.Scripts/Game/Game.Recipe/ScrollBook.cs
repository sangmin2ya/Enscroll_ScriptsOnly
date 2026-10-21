using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game.Attribute;
using Game.Desk;
using Game.Scroll;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScrollBook : MonoBehaviour, IPointerClickHandler, IDeskItem
{
    [Header("페이지 정보")]
    [SerializeField] private int maxLevelPage = 0;
    [SerializeField] private int currentPageOfLevel = 0;
    [SerializeField] private int page = 0; // 인덱스 - 1 - 2 - 3
    [SerializeField] private int nowMaxPage = 1;
    private AttributeInfo currentAttribute;
    [SerializeField]
    private Sprite emptySprite;

    [Header("UI")]
    [SerializeField] private Button closeButton;
    [SerializeField] private Button backButton;
    [SerializeField] private Button indexButton;
    [SerializeField] private GameObject pageButtons;
    [SerializeField] private List<GameObject> attributeDetailLayouts;
    [SerializeField] private List<GameObject> indexLayouts;
    [SerializeField] private List<RectTransform> levelIndicies;
    [SerializeField] private List<GameObject> levels;

    [Header("속성 상세정보 UI")]
    [SerializeField] private TextMeshProUGUI attributeName;
    [SerializeField] private TextMeshProUGUI attributeLevel;
    [SerializeField] private TextMeshProUGUI attributeDesc;
    [SerializeField] private Image leftIcon;
    [SerializeField] private Image rightIcon;
    [SerializeField] private Image combination1Icon;
    [SerializeField] private Image combination2Icon;

    [Header("Index 표시")]
    List<AttributeIndexText> indicies = new List<AttributeIndexText>();

    [Header("Tag")]
    [SerializeField] List<Button> levelTags = new List<Button>();

    [Header("프리팹")]
    [SerializeField] AttributeIndexText attributeIndexPfb;

    [Header("레벨별 속성 페이지 순서")]
    List<List<string>> levelPageOrders = new List<List<string>>() { new List<string>(), new List<string>(), new List<string>(), new List<string>(), new List<string>() };

    private Stack<AttributeInfo> stackPages = new Stack<AttributeInfo>();
    private Animator _fileAnimator;
    //서랍안에 넣어둘거면 false로
    private bool _isPlacedOnDesk = false;
    private bool _isOpen = false;
    public bool IsOpen => _isOpen;
    // IDeskItem: DragableObject가 제출/서랍 복귀 시 호출
    bool IDeskItem.CanSubmit => false;
    void IDeskItem.OnSubmitted() { }
    void IDeskItem.OnReturnedToDrawer() { CloseFile(); }

    public bool IsPlacedOnDesk
    {
        get { return _isPlacedOnDesk; }
        set { _isPlacedOnDesk = value; }
    }

    private SymbolLoader symbolLoader;

    void Awake()
    {
        symbolLoader = FindObjectOfType<SymbolLoader>();
        _fileAnimator = GetComponent<Animator>();
        closeButton.GetComponent<Button>().onClick.AddListener(CloseFile);
    }

    void Start()
    {
        Init();
    }

    void Update()
    {
        if (stackPages.Count > 0)
            backButton.gameObject.SetActive(true);
        else backButton.gameObject.SetActive(false);
    }

    private void Init()
    {
        LoadAttributeIndcies();

        int limit = 0;
        if (GameData.SaveInfo != null)
        {
            limit = GameData.SaveInfo.parchmentLevel - 1;
        }

        for (int i = 0; i < levelTags.Count; i++)
        {
            if (i <= limit)
            {
                levelTags[i].gameObject.SetActive(true);
                levels[i].SetActive(true);

            }
            else
            {
                levelTags[i].gameObject.SetActive(false);
                levels[i].gameObject.SetActive(false);
            }
        }

        indexButton.onClick.AddListener(() =>
        {
            OpenFile();
            LoadIndexPage();
        });
        levelTags[0].onClick.AddListener(() =>
        {
            OpenFile();
            LoadAttributePage(1, 0, AttributeManager.Instance.attributeTable.Find(e => e.id == levelPageOrders[1][0]));
        });
        levelTags[1].onClick.AddListener(() =>
        {
            OpenFile();
            LoadAttributePage(2, 0, AttributeManager.Instance.attributeTable.Find(e => e.id == levelPageOrders[2][0]));
        });
        levelTags[2].onClick.AddListener(() =>
        {
            OpenFile();
            LoadAttributePage(3, 0, AttributeManager.Instance.attributeTable.Find(e => e.id == levelPageOrders[3][0]));
        });
        levelTags[3].onClick.AddListener(() =>
        {
            OpenFile();
            LoadAttributePage(4, 0, AttributeManager.Instance.attributeTable.Find(e => e.id == levelPageOrders[4][0]));
        });

        nowMaxPage = GameData.SaveInfo?.parchmentLevel ?? nowMaxPage;
        maxLevelPage = levelPageOrders[nowMaxPage].Count - 1;
    }

    public void ShowCheckOfAttribute(string attributeId)
    {
        indicies.Find(e => e.AttributeInfo.id == attributeId).ShowCheck();
    }

    private void LoadAttributeIndcies()
    {
        for (int i = 0; i < AttributeManager.Instance.attributeTable.Count; i++)
        {
            AttributeInfo attributeInfo = AttributeManager.Instance.attributeTable[i];
            if (attributeInfo.level <= 1)
            {
                levelPageOrders[1].Add(attributeInfo.id);
                AttributeIndexText indexText = Instantiate(attributeIndexPfb, levelIndicies[0].transform);
                indicies.Add(indexText);
                indexText.SetData(attributeInfo, 1, levelPageOrders[1].Count - 1);
            }
            else if (attributeInfo.level <= 2)
            {
                levelPageOrders[2].Add(attributeInfo.id);
                AttributeIndexText indexText = Instantiate(attributeIndexPfb, levelIndicies[1].transform);
                indicies.Add(indexText);
                indexText.SetData(attributeInfo, 2, levelPageOrders[2].Count - 1);
            }
            else if (attributeInfo.level <= 4)
            {
                levelPageOrders[3].Add(attributeInfo.id);
                AttributeIndexText indexText = Instantiate(attributeIndexPfb, levelIndicies[2].transform);
                indicies.Add(indexText);
                indexText.SetData(attributeInfo, 3, levelPageOrders[3].Count - 1);
            }
            else
            {
                levelPageOrders[4].Add(attributeInfo.id);
                AttributeIndexText indexText = Instantiate(attributeIndexPfb, levelIndicies[3].transform);
                indicies.Add(indexText);
                indexText.SetData(attributeInfo, 4, levelPageOrders[4].Count - 1);
            }
        }
    }

    public void LoadAttributePage(int parchmentLevel, int currentPage, AttributeInfo attributeInfo)
    {
        if (_isOpen)
        {
            indicies.Find(e => e.AttributeInfo == attributeInfo)?.HideCheck();

            foreach (GameObject layout in attributeDetailLayouts)
            {
                layout.SetActive(true);
            }

            foreach (GameObject layout in indexLayouts)
            {
                layout.SetActive(false);
            }

            currentAttribute = attributeInfo;

            page = parchmentLevel;
            maxLevelPage = levelPageOrders[page].Count - 1;
            currentPageOfLevel = currentPage;

            attributeName.SetText(attributeInfo.name);
            attributeLevel.SetText(string.Format("{0} LEVEL", parchmentLevel));
            attributeDesc.SetText(attributeInfo.desc);
            leftIcon.sprite = symbolLoader.GetImageByName(attributeInfo.id);
            rightIcon.sprite = symbolLoader.GetImageByName(attributeInfo.id);

            AttributeRecipeInfo recipe = AttributeManager.Instance.GetAttributeRecipe(attributeInfo);
            if (recipe.isFirstUnlocked)
            {
                // 첫 번째 조합 요소 이미지 로드
                string firstSubAttributeId = AttributeManager.Instance.FindIdByName(currentAttribute.combinations[0]);    // AttributeInfo 에서 Sort() 된 상태로 id 들어감.
                if (!string.IsNullOrEmpty(firstSubAttributeId))
                {
                    combination1Icon.sprite = symbolLoader.GetImageByName(firstSubAttributeId);
                }
                else
                {
                    combination1Icon.sprite = symbolLoader.GetImageByName(currentAttribute.combinations[0]);
                }
            }
            else
            {
                combination1Icon.sprite = emptySprite;
            }

            if (recipe.isSecondUnlocked)
            {
                // 두 번째 조합 요소 이미지 로드
                string secondSubAttributeId = AttributeManager.Instance.FindIdByName(currentAttribute.combinations[1]);   // AttributeInfo 에서 Sort() 된 상태로 id 들어감.
                if (!string.IsNullOrEmpty(secondSubAttributeId))
                {
                    combination2Icon.sprite = symbolLoader.GetImageByName(secondSubAttributeId);
                }
                else
                {
                    combination2Icon.sprite = symbolLoader.GetImageByName(currentAttribute.combinations[1]);
                }
            }
            else
            {
                combination2Icon.sprite = emptySprite;
            }
        }
    }

    private void LoadIndexPage()
    {
        foreach (GameObject page in attributeDetailLayouts)
        {
            page.SetActive(false);
        }

        foreach (GameObject page in indexLayouts)
        {
            page.SetActive(true);
        }

        this.page = 0;
        maxLevelPage = 0;
        currentPageOfLevel = 0;

        currentAttribute = null;
        stackPages.Clear();
    }

    public void PassPagee(bool next)
    {
        if (next && !(page == nowMaxPage && currentPageOfLevel == maxLevelPage))
        {
            SoundManager.Instance.PlaySFX("Nextpage");
            if (page == 0)
            {
                LoadAttributePage(1, 0, AttributeManager.Instance.attributeTable.Find(e => e.id == levelPageOrders[1][0]));
            }
            else
            {
                if (currentPageOfLevel == maxLevelPage)
                {
                    LoadAttributePage(page + 1, 0, AttributeManager.Instance.attributeTable.Find(e => e.id == levelPageOrders[page + 1][0]));
                }
                else
                {
                    LoadAttributePage(page, currentPageOfLevel + 1, AttributeManager.Instance.attributeTable.Find(e => e.id == levelPageOrders[page][currentPageOfLevel + 1]));
                }
            }
        }
        else if (!next && page > 0)
        {
            SoundManager.Instance.PlaySFX("Nextpage");
            if (page == 1 && currentPageOfLevel == 0)
            {
                LoadIndexPage();
            }
            else
            {
                if (currentPageOfLevel == 0)
                {
                    LoadAttributePage(page - 1, levelPageOrders[page - 1].Count - 1, AttributeManager.Instance.attributeTable.Find(e => e.id == levelPageOrders[page - 1][levelPageOrders[page - 1].Count - 1]));
                }
                else
                {
                    LoadAttributePage(page, currentPageOfLevel - 1, AttributeManager.Instance.attributeTable.Find(e => e.id == levelPageOrders[page][currentPageOfLevel - 1]));
                }
            }
        }
    }

    public void OpenFile()
    {
        if (_isOpen || !_isPlacedOnDesk)
        {
            return;
        }
        _fileAnimator.SetTrigger("Open");
        _isOpen = true;
        pageButtons.SetActive(true);
    }
    public void CloseFile()
    {
        if (!_isOpen)
        {
            return;
        }
        _isOpen = false;
        _fileAnimator.SetTrigger("Close");
        pageButtons.SetActive(false);
        stackPages.Clear();
    }

    public void UseHint()
    {
        AttributeRecipeInfo recipe = AttributeManager.Instance.GetAttributeRecipe(currentAttribute);
        if (!recipe.isUnlocked && MagicController.Instance.UseHint())
        {
            if (!recipe.isFirstUnlocked)
            {
                // 첫 번째 조합 요소 이미지 로드
                string firstSubAttributeId = AttributeManager.Instance.FindIdByName(currentAttribute.combinations[0]);    // AttributeInfo 에서 Sort() 된 상태로 id 들어감.
                if (!string.IsNullOrEmpty(firstSubAttributeId))
                {
                    combination1Icon.sprite = symbolLoader.GetImageByName(firstSubAttributeId);
                }
                else
                {
                    combination1Icon.sprite = symbolLoader.GetImageByName(currentAttribute.combinations[0]);
                }
                recipe.UnlockFirstSubElement();

                //도전과제
                GameData.Instance.isNoHint = false;
                return;
            }
            else if (!recipe.isSecondUnlocked)
            {
                // 두 번째 조합 요소 이미지 로드
                string secondSubAttributeId = AttributeManager.Instance.FindIdByName(currentAttribute.combinations[1]);   // AttributeInfo 에서 Sort() 된 상태로 id 들어감.
                if (!string.IsNullOrEmpty(secondSubAttributeId))
                {
                    combination2Icon.sprite = symbolLoader.GetImageByName(secondSubAttributeId);
                }
                else
                {
                    combination2Icon.sprite = symbolLoader.GetImageByName(currentAttribute.combinations[1]);
                }
                recipe.UnlockSecondSubElement();

                AttributeManager.Instance.ObtainAttributeById(currentAttribute.id);
                //도전과제
                GameData.Instance.isNoHint = false;
            }
        }
    }

    public void GoCombinationPage(int index)
    {
        AttributeRecipeInfo recipe = AttributeManager.Instance.GetAttributeRecipe(currentAttribute);
        if (!AttributeManager.Instance.FindIdByName(currentAttribute.combinations[index]).Equals(""))
        {
            if ((index == 0 && recipe.isFirstUnlocked) || (index == 1 && recipe.isSecondUnlocked))
            {
                stackPages.Push(currentAttribute);

                AttributeInfo ai = AttributeManager.Instance.FindAttributeById(AttributeManager.Instance.FindIdByName(currentAttribute.combinations[index]));
                (int, int) result = FindPageAndCurrentPage(ai);
                LoadAttributePage(result.Item1, result.Item2, ai);
            }
        }
    }

    private (int, int) FindPageAndCurrentPage(AttributeInfo attributeInfo)
    {
        foreach (List<string> strings in levelPageOrders)
        {
            if (!strings.Contains(attributeInfo.id))
                continue;
            else
            {
                return (levelPageOrders.IndexOf(strings), strings.IndexOf(attributeInfo.id));
            }
        }

        Debug.LogErrorFormat("페이지 찾을 수 없음");
        return (-1, -1);
    }

    public void SetRecipeCombinationContent(AttributeInfo attributeInfo)
    {
        AttributeRecipeInfo recipe = AttributeManager.Instance.GetAttributeRecipe(attributeInfo);
        if (!recipe.isUnlocked)
        {
            PopupManager.Instance.OpenUnlockUI(attributeInfo.id);

            recipe.UnlockFirstSubElement();
            recipe.UnlockSecondSubElement();
            ReloadCombinations();
        }

        // 모든 속성 해금 도전 과제
        if (AttributeManager.Instance.AttributeRecipeInfos.All(e => e.isUnlocked))
        {
            SteamAchievementManager.Instance.UnlockAchievement("NEW_ACHIEVEMENT_1_4");
        }
    }

    private void ReloadCombinations()
    {
        if (currentAttribute != null)
        {
            AttributeRecipeInfo recipe = AttributeManager.Instance.GetAttributeRecipe(currentAttribute);
            if (recipe.isFirstUnlocked)
            {
                // 첫 번째 조합 요소 이미지 로드
                string firstSubAttributeId = AttributeManager.Instance.FindIdByName(currentAttribute.combinations[0]);    // AttributeInfo 에서 Sort() 된 상태로 id 들어감.
                if (!string.IsNullOrEmpty(firstSubAttributeId))
                {
                    combination1Icon.sprite = symbolLoader.GetImageByName(firstSubAttributeId);
                }
                else if (firstSubAttributeId.Equals(""))
                {
                    combination1Icon.sprite = symbolLoader.GetImageByName(currentAttribute.combinations[0]);
                }
            }
            else
            {
                combination1Icon.sprite = emptySprite;
            }

            if (recipe.isSecondUnlocked)
            {
                // 두 번째 조합 요소 이미지 로드
                string secondSubAttributeId = AttributeManager.Instance.FindIdByName(currentAttribute.combinations[1]);   // AttributeInfo 에서 Sort() 된 상태로 id 들어감.
                if (!string.IsNullOrEmpty(secondSubAttributeId))
                {
                    combination2Icon.sprite = symbolLoader.GetImageByName(secondSubAttributeId);
                }
                else if (secondSubAttributeId.Equals(""))
                {
                    combination2Icon.sprite = symbolLoader.GetImageByName(currentAttribute.combinations[1]);
                }
            }
            else
            {
                combination2Icon.sprite = emptySprite;
            }

            if (recipe.isUnlocked)
            {
            indicies.Find(e => e.AttributeInfo == currentAttribute)?.HideCheck();
            }
        }

    }

    public void UpdateScrollBook()
    {
        levels[ScrollCreationManager.Instance.CurrentScrollLevel - 1].SetActive(true);
        levelTags[ScrollCreationManager.Instance.CurrentScrollLevel - 1].gameObject.SetActive(true);
        nowMaxPage++;
        if (nowMaxPage > 4)
            nowMaxPage = 4;
    }

    public void GoBackPage()
    {
        if (stackPages.Count > 0)
        {
            AttributeInfo ai = stackPages.Pop();
            (int, int) result = FindPageAndCurrentPage(ai);
            LoadAttributePage(result.Item1, result.Item2, ai);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (GetComponent<DragableObject>().IsDragging)
        {
            GetComponent<DragableObject>().IsDragging = false;
            return;
        }
        if (_isPlacedOnDesk)
        {
            OpenFile();
        }
    }
}
