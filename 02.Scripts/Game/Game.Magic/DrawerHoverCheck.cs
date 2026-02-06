using Game.Desk;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.Magic
{
    public class DrawerHoverCheck : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField]
        private Texture2D penTexture;
        [SerializeField]
        private Vector2 hotsport = Vector2.zero;
        private bool _isDragging = false;
        public bool IsDragging
        {
            get => _isDragging;
            set => _isDragging = value;
        }
        /// <summary>
        /// 커서모양설정
        /// </summary>
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!MagicController.Instance._isEnchanting)
            {
                return;
            }
            Cursor.SetCursor(penTexture, hotsport, CursorMode.Auto);
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            if (_isDragging || !MagicController.Instance._isEnchanting)
            {
                return;
            }
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
    }
}