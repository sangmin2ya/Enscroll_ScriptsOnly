using UnityEngine;
using Game.Element;
using TMPro;
using UnityEngine.UI;

public class ElementTooltip : MonoBehaviour
{
    [Header("원소 상징 이미지")]
    [SerializeField]
    private Sprite fireSprite;
    [SerializeField]
    private Sprite waterSprite;
    [SerializeField]
    private Sprite windSprite;
    [SerializeField]
    private Sprite earthSprite;
    [Header("원소 툴팁 정보")]
    [SerializeField]
    private GameObject tooltipObject;
    [SerializeField]
    private TextMeshProUGUI elementInfoText;
    [SerializeField]
    private Image elementImage;


    public void ShowTooltip(ElementType elementType)
    {
        switch (elementType)
        {
            case ElementType.Fire:
                ChangeInfo(ElementType.Fire);
                break;
            case ElementType.Water:
                ChangeInfo(ElementType.Water);
                break;
            case ElementType.Wind:
                ChangeInfo(ElementType.Wind);
                break;
            case ElementType.Earth:
                ChangeInfo(ElementType.Earth);
                break;
        }
        tooltipObject.SetActive(true);
    }
    public void HideTooltip()
    {
        tooltipObject.SetActive(false);
    }
    private void ChangeInfo(ElementType elementType)
    {
        switch (elementType)
        {
            case ElementType.Fire:
                elementInfoText.text = "불의 기운";
                elementImage.sprite = fireSprite;
                break;
            case ElementType.Water:
                elementInfoText.text = "물의 기운";
                elementImage.sprite = waterSprite;
                break;
            case ElementType.Wind:
                elementInfoText.text = "공기의 기운";
                elementImage.sprite = windSprite;
                break;
            case ElementType.Earth:
                elementInfoText.text = "대지의 기운";
                elementImage.sprite = earthSprite;
                break;
        }
    }
}