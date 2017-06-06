using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class wwDragHelper : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler {


  public delegate void DragHelperDelegate(PointerEventData data);

  public DragHelperDelegate onBeginDrag;
  public DragHelperDelegate onDrag;
  public DragHelperDelegate onEndDrag;

  public DragHelperDelegate onPointerDown;
  public DragHelperDelegate onPointerUp;



  public void OnBeginDrag(PointerEventData ped) {
    if (onBeginDrag != null) {
      onBeginDrag(ped);
    }
  }
  public void OnDrag(PointerEventData ped) {
    if (onDrag != null) {
      onDrag(ped);
    }
  }
  public void OnEndDrag(PointerEventData ped) {
    if (onEndDrag != null) {
      onEndDrag(ped);
    }
  }
  public void OnPointerDown(PointerEventData ped) {
    if (onPointerDown != null) {
      onPointerDown(ped);
    }
  }
  public void OnPointerUp(PointerEventData ped) {
    if (onPointerUp != null) {
      onPointerUp(ped);
    }
  }

}
