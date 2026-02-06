using UnityEngine;
using Game.Scroll;
using Game.Symbols;
using UnityEngine.EventSystems;
using Game.Desk;
using UnityEngine.UI;
using TMPro;
public class ElementSpellPreview : MonoBehaviour, IHoverable
{
    [SerializeField]
    private Symbol elementSymbol;
    [SerializeField]
    private Image panel;
    [SerializeField]
    private Image rune;
    [SerializeField]
    private Image line;
    [SerializeField]
    private TextMeshProUGUI spellName;
    private CanvasGroup canvasGroup;
    private bool isTransparent = false;

    void Start()
    {
        // CanvasGroup 컴포넌트를 사용하여 투명도와 상호작용을 관리
        canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;  // 기본 투명도
        canvasGroup.blocksRaycasts = true; // 기본적으로 Raycast를 막음
    }
    public void OnHover()
    {
        throw new System.NotImplementedException();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SetTransparency(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetTransparency(false);
    }

    // private void SetAlpha(float alpha)
    // {
    //     panel.color = new Color(panel.color.r, panel.color.g, panel.color.b, alpha);
    //     rune.color = new Color(rune.color.r, rune.color.g, rune.color.b, alpha);
    //     line.color = new Color(line.color.r, line.color.g, line.color.b, alpha);
    //     spellName.color = new Color(spellName.color.r, spellName.color.g, spellName.color.b, alpha);
    // }
    private void SetTransparency(bool makeTransparent)
    {
        if (makeTransparent && !isTransparent)
        {
            canvasGroup.alpha = 0f;  // 원하는 투명도 (0.0은 완전히 투명)
            canvasGroup.blocksRaycasts = false;  // Raycast를 통과하도록 허용
            isTransparent = true;
        }
        else if (!makeTransparent && isTransparent)
        {
            canvasGroup.alpha = 1f;  // 원래 불투명 상태로 복원
            canvasGroup.blocksRaycasts = true;  // 다시 Raycast를 막음
            isTransparent = false;
        }
    }
}