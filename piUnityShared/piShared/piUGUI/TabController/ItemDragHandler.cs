#if false // this code is unused in Wonder, but reluctant to delete entirely.


using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.Events;

public class ItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {

  private Vector2 localCursor = Vector2.zero;
  private PointerEventData eventData;

  public DragEvent OnDragStart;
  public DragEvent OnDragEnd;
  public DragEvent OnDragChange;
  public DragEvent OnRestoreScrollPosition;

  public Vector2 PressLocalCursor {
    get { return this.localCursor; }
  }

  public PointerEventData RecentDragEventData {
    get { return this.eventData; }
  }

  public void OnBeginDrag (PointerEventData eventData) {
    localCursor = transform.localPosition;
    this.eventData = eventData;

    if (OnDragStart != null) {
      OnDragStart.Invoke(this, eventData);
    }
  }

  public void OnDrag (PointerEventData eventData) {
    this.eventData = eventData;

    if (OnDragChange != null) { 
      OnDragChange.Invoke(this, eventData); 
    }
  }

  public void OnEndDrag (PointerEventData eventData) {
    this.eventData = eventData;

    if (OnRestoreScrollPosition != null && OnRestoreScrollPosition.GetPersistentEventCount() > 0) {
      OnRestoreScrollPosition.Invoke(this, eventData);
    } else {
      DOTween.To(
        () => transform.localPosition,
        val => transform.localPosition = val,
        PressLocalCursor,
        .2f);
    }

    if (OnDragEnd != null) { 
      OnDragEnd.Invoke(this, eventData); 
    }
  }

  private Transform _GetTopMostParrent(Transform child) {
    if (child.parent != null) {
      return _GetTopMostParrent(child.parent);
    } else {
      return child;
    }
  }

  #region Events
  [System.Serializable]
  public class DragEvent: UnityEvent<ItemDragHandler, PointerEventData> {}
  #endregion

}

#endif // false