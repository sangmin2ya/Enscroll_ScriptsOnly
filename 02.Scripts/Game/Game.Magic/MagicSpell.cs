using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

using Game.Attribute;
using Game.Element;
using Game.Scroll;
using Game.Symbols;
using UnityEngine.UI;

namespace Game.Magic
{
    public class MagicSpell : MonoBehaviour, IHoverable
    {
        [Header("마법 이름")]
        private string _attributeName;
        [SerializeField]
        private TextMeshProUGUI spellName;
        [Header("마법 속성")]
        [SerializeField]
        private Symbol symbol;
        [Header("하이라이팅 용 색상")]
        private Color previousColor;
        [SerializeField]
        private Color highlightColor = Color.blue;
        [Header("쓰레기통")]
        [SerializeField]
        private GameObject trashCan;

        public void SetSpell(Symbol symbol)
        {
            this.symbol = symbol;
            string symbolId = this.symbol.SymbolData.Id;
            if (this.symbol is OuterCircleDrawer)
            {
                if (!AttributeManager.Instance.IsNameUnlocked(symbolId))
                {
                    _attributeName = "???";
                }
                else
                {
                    _attributeName = AttributeManager.Instance.FindNameById(symbolId);
                }
            }
            else
            {
                _attributeName = ElementManager.Instance.FindNameById(symbolId);
            }
            spellName.text = _attributeName;
            symbol.MagicSpell = this;
        }
        public void ConfrimSymbol()
        {
            _attributeName = AttributeManager.Instance.FindNameById(symbol.SymbolData.Id);
            spellName.text = _attributeName;
        }
        public void EraseSymbol()
        {
            if (symbol == null)
            {
                Destroy(gameObject);
            }
            ScrollCreationManager.Instance.RemoveSymbol(symbol);
            Destroy(gameObject);
            // SoundManager.Instance.PlaySFX("RemoveMagic");
            SoundManager.Instance.PlaySFX("RemoveSpell");
        }
        public void OnHover()
        {
            throw new System.NotImplementedException();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            previousColor = symbol.color;
            symbol.color = highlightColor;
            if (GetComponent<Button>().interactable)
            {
                trashCan.SetActive(true);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            symbol.color = previousColor;
            trashCan.SetActive(false);
        }
    }
}