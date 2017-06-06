using UnityEngine;
using System.Collections;

namespace WW.UGUI{
  public class uGUIContentScaler : MonoBehaviour {
    public RectTransform StartPoint;
    public RectTransform EndPoint;

    public bool IsSubstractStartEnd = false;
    public bool IsRotating = true;

    public bool IsRotationYEnabled = true;

    private uGUIContentScalerHistory history = new uGUIContentScalerHistory();
  	
  	// Update is called once per frame
  	void Update () {
      if(StartPoint != null && EndPoint != null && history.isChanged(StartPoint.position, EndPoint.position)){
        UpdateView();
      } 
  	}

    public void UpdateView(){
      updateView();
      history.Update(StartPoint.position, EndPoint.position);
    }

    void updateView(){
     
      RectTransform parentTrans = this.transform.parent.GetComponent<RectTransform>();
      Vector3 start = parentTrans.InverseTransformPoint(StartPoint.transform.position);
      Vector3 end = parentTrans.InverseTransformPoint(EndPoint.transform.position);
      float width = (end - start).magnitude;

      Vector3 dir = EndPoint.transform.position - StartPoint.transform.position;


      // NOTE: Removed by leisen, this caused a problem: lines will never be straight. 
      // And I didn't see the huge rotation for y-axis. Please let me know if there is issue.
      //because small values makes huge rotation for y-axis  TUR-232
//      if (dir.y > -1 && dir.y < 1){
//        dir.y += 2f;
//      }

      if(IsSubstractStartEnd){
        Vector3[] corners = new Vector3[4];
        StartPoint.GetWorldCorners(corners);
        Vector3 startWidthDir = corners[0] - corners[1];
        float startWidth = startWidthDir.magnitude;

        EndPoint.GetWorldCorners(corners);
        Vector3 endWidthDir = corners[0] - corners[1];
        float endWidth = endWidthDir.magnitude;

        start = parentTrans.InverseTransformPoint(StartPoint.transform.position + dir.normalized * startWidth/2f);
        end = parentTrans.InverseTransformPoint(EndPoint.transform.position - dir.normalized * endWidth/2f);

        width = (end - start).magnitude;
      }

      this.transform.localPosition = (start + end)/2;

      this.GetComponent<RectTransform>().SetWidth(width);
      if(IsRotating){
        Quaternion rotation =  Quaternion.FromToRotation(Vector3.right, new Vector3(dir.x, dir.y, 0));
        if (!IsRotationYEnabled && rotation.y > 0){
          rotation.z = rotation.y;
          rotation.y = 0;
        }
        this.transform.rotation = rotation;
      }

    }
  }

  public struct uGUIContentScalerHistory{
    private Vector3 start;
    private Vector3 end;

    public bool isChanged(Vector3 startPos, Vector3 endPos){
      bool result = false;
      result = result || !piMathUtil.withinEpsilon(calculateDistanceSquared(startPos, this.start));
      result = result || !piMathUtil.withinEpsilon(calculateDistanceSquared(endPos, this.end));
      return result;
    }

    public void Update(Vector3 startPos, Vector3 endPos){
      this.start = startPos;
      this.end = endPos;

    }

    private float calculateDistanceSquared(Vector3 a, Vector3 b){
      return (b-a).sqrMagnitude;
    }
  }
}


