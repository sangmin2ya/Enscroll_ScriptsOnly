using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public interface IHoverable : IPointerEnterHandler, IPointerExitHandler
{
    public abstract void OnHover();
}
