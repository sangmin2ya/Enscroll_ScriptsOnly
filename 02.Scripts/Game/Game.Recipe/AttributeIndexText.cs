using System.Collections;
using System.Collections.Generic;
using Game.Attribute;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AttributeIndexText : MonoBehaviour
{
    [Header("속성 정보")]
    [SerializeField] private AttributeInfo attributeInfo;
    public AttributeInfo AttributeInfo => attributeInfo;

    [Header("페이지 정보")]
    [SerializeField] private int parchmentLevel;
    [SerializeField] private int pageOfLevel;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private GameObject checkUI;

    private ScrollBook scrollBook;

    void Awake()
    {
        scrollBook = FindObjectOfType<ScrollBook>();
    }

    public void SetData(AttributeInfo attributeInfo, int scrollBookLevel, int pageOfLevel)
    {
        this.attributeInfo = attributeInfo;
        nameText.SetText(attributeInfo.name);

        this.parchmentLevel = scrollBookLevel;
        this.pageOfLevel = pageOfLevel;

        // 페이지 이동 코드
        GetComponent<Button>().onClick.AddListener(() => { scrollBook.LoadAttributePage(parchmentLevel, this.pageOfLevel, attributeInfo); HideCheck(); });
    }

    public void ShowCheck()
    {
        checkUI.SetActive(true);
    }

    public void HideCheck()
    {
        checkUI.SetActive(false);
    }

}
