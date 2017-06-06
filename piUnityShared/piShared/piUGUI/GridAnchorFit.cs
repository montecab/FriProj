using UnityEngine;
using System.Collections;
using UnityEngine.UI;


/// <summary>
/// Attach this script to a gameObject which has a component of GridLayoutGroup.
/// This class is to solve the problem that the content of a grid group doesn't 
/// resize with the anchor of the grid parent.
/// </summary>
[RequireComponent (typeof (GridLayoutGroup))]
public class GridAnchorFit : MonoBehaviour {

  private GridLayoutGroup grid;
  private RectTransform rectT;
  private bool isInit = false;

  public bool IsKeepingRatio;
  public float RatioXY = 1.0f; // the ratio of cellsize.x/cellsize.y, only working when IsKeepingRatio is true

  void init(){
    if(!isInit){
      grid = this.gameObject.GetComponent<GridLayoutGroup>();
      rectT = grid.transform.parent.gameObject.GetComponent<RectTransform>();
      isInit = true;
    }
  }

  void Start(){
    Reposition();
  }

  public void Reposition () {
    init();
    if(grid.constraint == GridLayoutGroup.Constraint.Flexible){
      WWLog.logError("Not enough infomation to fit the grid. Please set the constraint for the grid group");
      return;
    }

    float cellX = 0;
    float cellY = 0;
    float fitLength = 0;

    if(grid.constraint == GridLayoutGroup.Constraint.FixedRowCount){
      fitLength =  rectT.GetHeight();
      cellY = (fitLength - grid.padding.top - grid.padding.bottom - grid.spacing.y * (grid.constraintCount - 1))/grid.constraintCount;
      if(IsKeepingRatio){
        cellX = cellY*RatioXY;
      }
    }
    else if(grid.constraint == GridLayoutGroup.Constraint.FixedColumnCount){
      fitLength =  rectT.GetWidth();
      cellX = (fitLength - grid.padding.left - grid.padding.right - grid.spacing.x * (grid.constraintCount - 1))/grid.constraintCount;
      if(IsKeepingRatio){
        cellY = cellX/RatioXY;
      }
    }
    grid.cellSize = new Vector2(cellX, cellY);
   

  }
}
  

