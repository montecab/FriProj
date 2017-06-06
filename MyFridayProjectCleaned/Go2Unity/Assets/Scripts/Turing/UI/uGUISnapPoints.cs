using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace WW.UGUI{
  public class uGUISnapPoints : MonoBehaviour {
    public int SnapPointNum = -1;
    public GridLayoutGroup SnapPointsGrid;
    public GameObject SnapPointPrefab;
    public Direction Dir = Direction.Vertical;
    public bool IsAllowedInMid = false;
    public bool Display = true;

    public void SetUp(int snapPoint){
      SetUp(snapPoint, Dir);
    }

    public float SnappedValueNormalized(float v){
      return SnappedValue(0,1,v);
    }

    public float SnappedValue(float min, float max, float v){
      if(SnapPointNum > 0){
        float snapvalue = (max - min)/(SnapPointNum+1);
        int snapPointId = Mathf.RoundToInt(v/snapvalue);
        float ret = (float)snapPointId * snapvalue + min;
        if(IsAllowedInMid){
          float diffNorm = Mathf.Abs(ret - v)/snapvalue;
          if(diffNorm > 0.2f){
            return v;
          }
        }
        return ret;
      }
      return v;
    }

    public void SetUp(int snapPoint, Direction dir){

      SnapPointsGrid.gameObject.SetActive(Display);

      SnapPointNum = snapPoint;
      if(SnapPointsGrid.transform.childCount != SnapPointNum && SnapPointNum > 0){
        foreach(Transform child in SnapPointsGrid.transform){
          Destroy(child.gameObject);
        }
        
        for(int i = 0; i< SnapPointNum; ++i){
          GameObject newPoint = Instantiate(SnapPointPrefab, SnapPointPrefab.transform.position, SnapPointPrefab.transform.rotation) as GameObject;
          newPoint.transform.SetParent(SnapPointsGrid.transform, false);
        }
        
        if(dir == Direction.Horizontal){
          float snapvalue = this.GetComponent<RectTransform>().rect.height/ (SnapPointNum+1);
          float width = this.GetComponent<RectTransform>().rect.width -6;
          SnapPointsGrid.spacing = new Vector2(0, snapvalue - 1);
          SnapPointsGrid.cellSize = new Vector2(width, 1);
          SnapPointsGrid.padding.top = (int)snapvalue;
          SnapPointsGrid.padding.left = 3;
          SnapPointsGrid.padding.right = 3;
        }
        else{
          float snapvalue = this.GetComponent<RectTransform>().rect.width/ (SnapPointNum+1);
          float height = this.GetComponent<RectTransform>().rect.height -6;
          SnapPointsGrid.spacing = new Vector2( snapvalue - 1,0);
          SnapPointsGrid.cellSize = new Vector2(1, height);
          SnapPointsGrid.padding.left = (int)snapvalue;
          SnapPointsGrid.padding.top = 3;
          SnapPointsGrid.padding.bottom = 3;
        }
      }
    }


  }

  public enum Direction{
    Horizontal,
    Vertical
  }
}
