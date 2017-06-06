using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// utility to set the cell-size of a GridLayout element to reflect the width of the grid layout element.
// madness.


[ExecuteInEditMode]
[RequireComponent (typeof(GridLayoutGroup  ))]
[RequireComponent (typeof(ContentSizeFitter))]
public class wwGridFixer : MonoBehaviour {

  private GridLayoutGroup   uiGLG;
  private ContentSizeFitter uiCSF;
  private bool              uiCSFNeedsEnable;


  float cellAspect = float.NaN;

  public bool needsUpdate = false;
  public bool fitInParent = false;

  // Use this for initialization
  void Start () {
    uiGLG = GetComponent<GridLayoutGroup>();
    if (uiGLG == null) {
      WWLog.logError("no grid layout component.");
      this.enabled = false;
      return;
    }
    
    if (uiGLG.constraint == GridLayoutGroup.Constraint.Flexible) {
      WWLog.logError("do not use with flexible column count.");
    }

    uiCSF = GetComponent<ContentSizeFitter>();
    if (uiCSF == null) {
      WWLog.logError("no content size fitter.");
      this.enabled = false;
      return;
    }
  }

  void Update() {
    if (needsUpdate) {
      OnRectTransformDimensionsChange();
    }

    if (uiCSFNeedsEnable) {
      uiCSF.enabled = true;
      uiCSFNeedsEnable = false;
    }
  }

  public float CellAspect {
    get {
      return cellAspect;
    }
    set {
      bool update = false;
      update = update || ( float.IsNaN(value) && !float.IsNaN(cellAspect));
      update = update || (!float.IsNaN(value) &&  float.IsNaN(cellAspect));
      update = update || (!float.IsNaN(value) && !float.IsNaN(cellAspect) && (value != cellAspect));
      cellAspect = value;
      if (update) {
        OnRectTransformDimensionsChange();
      }
    }
  }

  protected void OnRectTransformDimensionsChange() {
    needsUpdate = false;

    if (uiGLG == null) {
      // we already printed an error in Start().
      return;
    }
    
    Vector2 cs = uiGLG.cellSize;

    if (uiGLG.constraint == GridLayoutGroup.Constraint.FixedColumnCount) {
      float availSpace = GetComponent<RectTransform>().GetWidth();
      availSpace -= (uiGLG.padding.left + uiGLG.padding.right);
      availSpace -= uiGLG.spacing.x * (uiGLG.constraintCount - 1);
      cs.x = availSpace / uiGLG.constraintCount;
      if (!float.IsNaN(cellAspect)) {
        cs.y = cs.x / cellAspect;
      }

      if (fitInParent) {
        RectTransform parentRectTrans = transform.parent.GetComponent<RectTransform>();
        int numChildren = transform.childCount;
        int numRows     = Mathf.CeilToInt((float)numChildren / (float)uiGLG.constraintCount);

        float k  = 0;
        k  += (numRows - 1) * uiGLG.spacing.y;
        k  += uiGLG.padding.top;
        k  += uiGLG.padding.bottom;
        float fac = (parentRectTrans.rect.height - k) / (numRows * cs.y);
        fac = Mathf.Clamp(fac, 0.01f, 1f);

        cs *= fac;
      }
    }
    else if (uiGLG.constraint == GridLayoutGroup.Constraint.FixedRowCount) {
      float availSpace = GetComponent<RectTransform>().GetHeight();
      availSpace -= (uiGLG.padding.top + uiGLG.padding.bottom);
      availSpace -= uiGLG.spacing.y * (uiGLG.constraintCount - 1);
      cs.y = availSpace / uiGLG.constraintCount;
      if (!float.IsNaN(cellAspect)) {
        cs.x = cs.y * cellAspect;
      }

      if (fitInParent) {
        WWLog.logError("todo: horizontal fit-in-parent");
      }
    }
    uiGLG.cellSize = cs;

    // this is a hack. in every-other re-layout, the contentSizeFitter isn't picking up the fact that contents have changed.
    // hack is to disable it here and re-enable it in the next Update().
    uiCSF.enabled = false;
    uiCSFNeedsEnable = true;
  }
}
