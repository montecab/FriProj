using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent (typeof(ScrollRect))]
public class AnimatedSnapper : MonoBehaviour, IEndDragHandler {
  private ScrollRect targetScrollRect;
  private float itemsCount = -1;
  private AnimationCurve curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

  private void _PickUpScrollRectIfNeeded() {
    if (targetScrollRect == null) {
      targetScrollRect = GetComponent<ScrollRect>();
    }

    if (itemsCount <= 0) {
      Vector2 scrollRectSize = GetComponent<RectTransform>().GetSize();
      Vector2 panelSize = transform.GetChild(0).GetComponent<RectTransform>().GetSize();
      itemsCount = (panelSize.x - scrollRectSize.x) / panelSize.y;
    }
  }

  public void OnEndDrag (PointerEventData eventData) {
    _PickUpScrollRectIfNeeded();
    StartCoroutine(_SnapRect());
  }

  private IEnumerator _SnapRect() {
    if (targetScrollRect == null) { throw new System.Exception("Scroll Rect can not be null"); }

    if (itemsCount <= 0) { throw new System.Exception("Item count can not be zero"); }

    float startNormal = targetScrollRect.horizontal ? targetScrollRect.horizontalNormalizedPosition : targetScrollRect.verticalNormalizedPosition;
    float delta = 1.0f / itemsCount;
    float over = startNormal % delta;
    float endNormal = startNormal;

    if (over > delta / 2) {
      endNormal += (delta - over);
    } else {
      endNormal -= over;
    }

    float velocity = 1f;
    float duration = Mathf.Abs((endNormal - startNormal) / velocity);
    float timer = 0f;

    while (timer < 1f) {
      timer = Mathf.Min(1f, timer + Time.deltaTime / duration);
      float value = Mathf.Lerp(startNormal, endNormal, curve.Evaluate(timer));

      if (targetScrollRect.horizontal) {
        targetScrollRect.horizontalNormalizedPosition = value;
      } else {
        targetScrollRect.verticalNormalizedPosition = value;
      }

      yield return new WaitForEndOfFrame(); // wait until next frame
    }
  }
}
