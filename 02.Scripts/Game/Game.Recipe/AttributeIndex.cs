using System.Collections;
using System.Collections.Generic;
using Game.Attribute;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AttributeIndex : MonoBehaviour
{
    [Header("속성 정보")]
    [SerializeField] private AttributeInfo attributeInfo;

    [Header("페이지 정보")]
    [SerializeField] private int parchmentLevel;
    [SerializeField] private int pageOfLevel;
    [SerializeField] private GameObject attributeInfoPanel;
    [SerializeField] private TextMeshProUGUI attributeNameText;
    private float width;
    private float height;
    private ScrollBook scrollBook;
    private SymbolLoader symbolLoader;
    private RectTransform attributeInfoPanelRectTransform;
    private bool isMouseOver = false;

    void Awake()
    {
        scrollBook = FindObjectOfType<ScrollBook>();
        symbolLoader = FindObjectOfType<SymbolLoader>();
        attributeInfoPanel.SetActive(false);
        attributeInfoPanelRectTransform = attributeInfoPanel.GetComponent<RectTransform>();
        width = attributeInfoPanelRectTransform.rect.width;
        height = attributeInfoPanelRectTransform.rect.height;
    }

    public void SetData(AttributeInfo attributeInfo, int scrollBookLevel, int pageOfLevel)
    {
        this.attributeInfo = attributeInfo;
        attributeNameText.text = attributeInfo.name;
        Debug.Log(symbolLoader.GetImageByName(attributeInfo.id));
        GetComponent<Image>().sprite = symbolLoader.GetImageByName(attributeInfo.id);

        this.parchmentLevel = scrollBookLevel;
        this.pageOfLevel = pageOfLevel;

        // 페이지 이동 코드
        GetComponent<Button>().onClick.AddListener(() => { scrollBook.LoadAttributePage(parchmentLevel, this.pageOfLevel, attributeInfo); });
    }
}
