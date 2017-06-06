using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GridSetup : MonoBehaviour {

  public GridLayoutGroup grid;
  
  public void Reposition () {
    RectTransform rectT = grid.gameObject.GetComponent<RectTransform>();
    
    float tmp = rectT.rect.height;
    tmp = tmp - grid.padding.top  - grid.padding.bottom;
    
    int childrenCount = grid.transform.childCount;
    float width = grid.padding.left + grid.padding.right + tmp * childrenCount + grid.spacing .x * (childrenCount - 1);
    rectT.sizeDelta = new Vector2(width, 0) ; // sizeDelta is relative to the anchors
    
    grid.cellSize = new Vector2(tmp, tmp);
  }
  
}
