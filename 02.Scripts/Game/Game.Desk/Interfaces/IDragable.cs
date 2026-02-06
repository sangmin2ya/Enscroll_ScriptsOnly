using UnityEngine.EventSystems;

namespace Game.Desk
{
    public interface IDragable : IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        void ClampToParentBounds();
    }
}