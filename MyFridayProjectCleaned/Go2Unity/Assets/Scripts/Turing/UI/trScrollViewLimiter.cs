using UnityEngine;
using System.Collections;

[RequireComponent (typeof(RectTransform))]
public class trScrollViewLimiter : MonoBehaviour {

  private RectTransform LimitRectTransform;
  Vector3 topLeftCorner;

  void Awake(){
    LimitRectTransform = GetComponent<RectTransform>();
  }

  public bool CoordinateInScrollView(Vector3 screenPosition, out Vector3 worldPosition){
    return RectTransformUtility.ScreenPointToWorldPointInRectangle(LimitRectTransform, screenPosition, Camera.main, out worldPosition);
  }

  public bool IsPointInLimitRect(Vector3 point){
    Vector3[] worldCorners = new Vector3[4];
    LimitRectTransform.GetWorldCorners(worldCorners);
    topLeftCorner = new Vector3(LimitRectTransform.rect.x, LimitRectTransform.rect.y, 0);
    bool result = (point.x > worldCorners[0].x && point.x < worldCorners[2].x && point.y > worldCorners[0].y && point.y < worldCorners[2].y);
    return result;
  }

  public bool PositionInRect(Vector3 point, out Vector3 localPosition){
    Vector3[] worldCorners = new Vector3[4];
    LimitRectTransform.GetWorldCorners(worldCorners);
    topLeftCorner = new Vector3(LimitRectTransform.rect.x, LimitRectTransform.rect.y, 0);
    bool result = IsPointInLimitRect(point);
    localPosition = LimitRectTransform.InverseTransformPoint(point) - topLeftCorner;
  
    return result;
  }

  public Vector2 GetSize(){
    return LimitRectTransform.GetSize();
  }
  
  public Rect Rect {
    get {
      return LimitRectTransform.rect;
    }
  }
}
