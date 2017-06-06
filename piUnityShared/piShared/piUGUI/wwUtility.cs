using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace WW{
  public static class wwUtility {

    public static bool SetStruct<T>(ref T currentValue, T newValue) where T : struct{
      if (currentValue.Equals(newValue)){
        return false;
      }
      
      currentValue = newValue;
      return true;
    }

    public static void FitContent2DShallow(GameObject obj, float widthPadding, float heightPadding){
      RectTransform minX = null, minY = null, maxX = null, maxY = null;
      foreach (Transform childTrans in obj.transform) {
        if(!childTrans.gameObject.activeSelf){
          continue;
        }
        RectTransform child = childTrans.gameObject.GetComponent<RectTransform>();
        if(child != null){
          if (minX == null || minX.offsetMin.x > child.offsetMin.x) {
            minX = child; 
          }
          if (minY == null || minY.offsetMin.y > child.offsetMin.y) {
            minY = child; 
          }
          if (maxX == null || maxX.offsetMax.x < child.offsetMax.x) {
            maxX = child; 
          }
          if (maxY == null || maxY.offsetMax.y < child.offsetMax.y) {
            maxY = child; 
          }
        }
       
      }

      RectTransform rt = obj.GetComponent<RectTransform> ();
      RectTransform parentRT = rt.parent.GetComponent<RectTransform> (); 
      rt.SetWidth (Mathf.Max (parentRT.GetWidth(), maxX.offsetMax.x - minX.offsetMin.x + widthPadding * 2));
      rt.SetHeight (Mathf.Max (parentRT.GetHeight(), maxY.offsetMax.y - minY.offsetMin.y + heightPadding * 2));
      Vector3 childrenCentroid = new Vector3 ((maxX.offsetMax.x + minX.offsetMin.x) / 2, (maxY.offsetMax.y + minY.offsetMin.y) / 2);
      foreach (Transform child in obj.transform) {
        child.localPosition -= childrenCentroid;
      }   
    }

    // Get the height of a grid layout group, this is used because the layout preferred size is not calculated after the children are set
    public static float GetHeight(this GridLayoutGroup grid){
      int row = 1;
      if(grid.constraint == GridLayoutGroup.Constraint.FixedColumnCount){
        row = grid.gameObject.transform.childCount/grid.constraintCount;
        row += grid.gameObject.transform.childCount%grid.constraintCount == 0? 0 : 1;
      }
      return grid.padding.top + grid.padding.bottom + grid.cellSize.y * row + grid.spacing.y * (row - 1);

    }

    public static bool SetColor(ref Color currentValue, Color newValue){
      if (currentValue.r == newValue.r && currentValue.g == newValue.g && currentValue.b == newValue.b && currentValue.a == newValue.a){
        return false;
      }
      
      currentValue = newValue;
      return true;
    }

    public static bool SetClass<T>(ref T currentValue, T newValue) where T : class{
      if ((currentValue == null && newValue == null) || (currentValue != null && currentValue.Equals(newValue))){
        return false;
      }
      
      currentValue = newValue;
      return true;
    }

    static public T FindInParents<T>(GameObject go) where T : Component{
      if (go == null){
        return null;
      }
      var comp = go.GetComponent<T>();
      
      if (comp != null){
        return comp;
      }
      
      Transform t = go.transform.parent;
      if (t != null){
        return FindInParents<T>(t.gameObject);
      }
      return null;
    }

    static public bool isParent(Transform child, Transform potentialParent) {
      Transform t = child.parent;
      while (t != null) {
        if (t == potentialParent) {
          return true;
        }
        t = t.parent;
      }
      return false;
    }
    
    // imagine Rect has a margin inside each of the 4 edges.
    // this returns the normalized distance into the margin that 'pt' is.
    // if the point is not inside the margin, it returns (0, 0).
    // a point which were centered in X and halfway into the upper margin would return (0, 0.5).
    static public Vector2 percentageInMargin(Vector2 pt, Rect rect, Vector2 margin) {
      float px = percentageInMargin(pt.x, rect.xMin, rect.xMax, margin.x);
      float py = percentageInMargin(pt.y, rect.yMin, rect.yMax, margin.y);
      return new Vector2(px, py);
    }
    
    static public Vector2 percentageInMargin(Vector2 pt, Rect rect, float margin) {
      return percentageInMargin(pt, rect, Vector2.one * margin);
    }
    
    // assumes pA <= pB, margin <= (pB - pA)
    static public float percentageInMargin(float pt, float pA, float pB, float margin) {
      float pAB = pB - pA;
      float ptN = (pt - pA) / pAB;
      float mAN = margin / pAB;
      float mBN = 1f - mAN;
      
      if ((ptN > mAN) && (ptN < mBN)) {
        return 0f;
      }
      
      if (ptN <= mAN) {
        return Mathf.InverseLerp(0f, mAN, ptN) - 1f;
      }
      else {
        return Mathf.InverseLerp(mBN, 1f, ptN);
      }
    }
  }
}
