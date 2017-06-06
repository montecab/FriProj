using UnityEngine;
using System;
using System.Collections;

public static class RectTransformExtensions {

  public static void SetDefaultScale(this RectTransform trans) {
    trans.localScale = new Vector3(1, 1, 1);
  }
  public static void SetPivotAndAnchors(this RectTransform trans, Vector2 aVec) {
    trans.pivot = aVec;
    trans.anchorMin = aVec;
    trans.anchorMax = aVec;
  }
  
  public static Vector2 GetSize(this RectTransform trans) {
    return trans.rect.size;
  }
  public static float GetWidth(this RectTransform trans) {
    return trans.rect.width;
  }
  public static float GetHeight(this RectTransform trans) {
    return trans.rect.height;
  }
  
  public static void SetPositionOfPivot(this RectTransform trans, Vector2 newPos) {
    trans.localPosition = new Vector3(newPos.x, newPos.y, trans.localPosition.z);
  }
  
  public static void SetLeftBottomPosition(this RectTransform trans, Vector2 newPos) {
    trans.localPosition = new Vector3(newPos.x + (trans.pivot.x * trans.rect.width), newPos.y + (trans.pivot.y * trans.rect.height), trans.localPosition.z);
  }
  public static void SetLeftTopPosition(this RectTransform trans, Vector2 newPos) {
    trans.localPosition = new Vector3(newPos.x + (trans.pivot.x * trans.rect.width), newPos.y - ((1f - trans.pivot.y) * trans.rect.height), trans.localPosition.z);
  }
  public static void SetRightBottomPosition(this RectTransform trans, Vector2 newPos) {
    trans.localPosition = new Vector3(newPos.x - ((1f - trans.pivot.x) * trans.rect.width), newPos.y + (trans.pivot.y * trans.rect.height), trans.localPosition.z);
  }
  public static void SetRightTopPosition(this RectTransform trans, Vector2 newPos) {
    trans.localPosition = new Vector3(newPos.x - ((1f - trans.pivot.x) * trans.rect.width), newPos.y - ((1f - trans.pivot.y) * trans.rect.height), trans.localPosition.z);
  }
  
  public static void SetSize(this RectTransform trans, Vector2 newSize) {
    Vector2 oldSize = trans.rect.size;
    Vector2 deltaSize = newSize - oldSize;
    trans.offsetMin = trans.offsetMin - new Vector2(deltaSize.x * trans.pivot.x, deltaSize.y * trans.pivot.y);
    trans.offsetMax = trans.offsetMax + new Vector2(deltaSize.x * (1f - trans.pivot.x), deltaSize.y * (1f - trans.pivot.y));
  }
  public static void SetWidth(this RectTransform trans, float newSize) {
    SetSize(trans, new Vector2(newSize, trans.rect.size.y));
  }
  public static void SetHeight(this RectTransform trans, float newSize) {
    SetSize(trans, new Vector2(trans.rect.size.x, newSize));
  }

  public static bool IsInRect(RectTransform trans, Vector3 worldPos){
    Vector2 v = trans.GetXYRatioWithWorldPos(worldPos);
    if(v.x <= 1 && v.x >= 0 && v.y <= 1 && v.y >= 0){
      return true;
    }
    return false;
  }

  // Given a rect and a world pos, return the ratio point 
  // eg, center point of the rect should return (0.5,0.5)
  public static Vector2 GetXYRatioWithWorldPos(this RectTransform trans, Vector3 worldPos){
    Vector3 localPos = trans.InverseTransformPoint(worldPos);   
    float relY = localPos.y +trans.GetSize().y * trans.pivot.y;
    float relX = localPos.x +trans.GetSize().x * trans.pivot.x;
    relX = relX/trans.GetSize().x;
    relY = relY/trans.GetSize().y;
    return new Vector2(relX, relY);
  }

  public static Vector3 XYRatioToLocalPos(this RectTransform trans, Vector2 value){
    Vector3 ret = Vector3.zero;
    ret.y = value.y * trans.GetSize().y - trans.GetSize().y * trans.pivot.y;
    ret.x = value.x * trans.GetSize().x - trans.GetSize().x * trans.pivot.x;
    return ret;
  }

  public static Vector3 XYRatioToWorldPos(this RectTransform trans, Vector2 value){
    Vector3 ret = Vector3.zero;
    ret.y = value.y * trans.GetSize().y - trans.GetSize().y * trans.pivot.y;
    ret.y = RoundNDP(ret.y, 2);
    ret.x = value.x * trans.GetSize().x - trans.GetSize().x * trans.pivot.x;
    ret.x = RoundNDP(ret.x, 2);
    return trans.TransformPoint(ret);
  }

  // ignore value after n digits after the decimal point
  public static float RoundNDP(float value, int n){
    float tmp = Mathf.Pow(10, n);
    return Mathf.Round(value* tmp )/tmp;
  }
}
