using UnityEngine;
using System.Collections;

using WW.UGUI;

public class asPlayMarker : uGUIDragDrop {

  public delegate void onDragDelegate_t();

  public onDragDelegate_t onDragDelegate;

  public override void OnDrag (UnityEngine.EventSystems.PointerEventData eventData)
  {
    base.OnDrag (eventData);
    if (onDragDelegate != null) {
      onDragDelegate();
    }
  }

}
