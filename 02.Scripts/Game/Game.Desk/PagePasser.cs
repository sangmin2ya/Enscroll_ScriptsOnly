using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PagePasser : MonoBehaviour
{
    [Header("넘기기 버튼")]
    [SerializeField] int currentPage = 0;
    [SerializeField] Button prevButton;
    [SerializeField] Button nextButton;

    [Header("이미지 리스트")]
    [SerializeField] List<Sprite> sprites;

    [Header("페이지")]
    [SerializeField] Image page0;
    [SerializeField] Image page1;

    void Awake()
    {
        if (prevButton == null)
        {
            prevButton = transform.Find("내부컨텐츠/PassButtons/PrevButton").GetComponent<Button>();
        }
        if (nextButton == null)
        {
            nextButton = transform.Find("내부컨텐츠/PassButtons/NextButton").GetComponent<Button>();
        }
        prevButton.onClick.AddListener(() => PassPage(false));
        nextButton.onClick.AddListener(() => PassPage(true));
    }

    void Start()
    {
        UpdateButton();
        page0.sprite = sprites[0];
        if (sprites.Count > 1)
            page1.sprite = sprites[1];
        if (sprites.Count == 1)
            page1.gameObject.SetActive(false);
    }

    private void PassPage(bool isNext)
    {
        if (isNext)
        {
            if (currentPage < sprites.Count - 1)
            {
                currentPage++;
            }
        }
        else
        {
            if (currentPage > 0)
                currentPage--;
        }

        page0.sprite = sprites[currentPage];
        if (currentPage == sprites.Count - 1)
            page1.gameObject.SetActive(false);
        else
        {
            page1.sprite = sprites[currentPage + 1];
            page1.gameObject.SetActive(true);
        }

        UpdateButton();
    }

    private void UpdateButton()
    {
        if (sprites.Count == 1)
        {
            prevButton.gameObject.SetActive(false);
            nextButton.gameObject.SetActive(false);
            return;
        }

        if (currentPage == 0)
        {
            prevButton.interactable = false;
            nextButton.interactable = true;
        }
        else if (currentPage == sprites.Count - 1)
        {
            prevButton.interactable = true;
            nextButton.interactable = false;
        }
        else
        {
            prevButton.interactable = true;
            nextButton.interactable = true;
        }
    }
}
