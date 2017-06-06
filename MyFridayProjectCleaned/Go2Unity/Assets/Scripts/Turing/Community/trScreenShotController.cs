using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO;

namespace Turing{
  public class trScreenShotController : ScreenShotCaptureController {
    public trStateMachinePanelBase Statemachine;
    private Vector3 prePos;
    private Vector3 preScale;
    string screenshotPath = "null";

    protected override void Start ()
    {
      base.Start ();
    }

    public override string ScreenShotName (int width, int height)
    {
      return screenshotPath;
    }

    public void takeScreenshot(trProgram program){
      screenshotPath = program.ThumbnailPath ;
      takeShot = true;
    }

    protected override void doBeforeScreenShot ()
    {
      base.doBeforeScreenShot ();
      //order matters!
      prePos = Statemachine.StatePanel.transform.localPosition;
      preScale = Statemachine.StatePanel.transform.localScale;
      centerResizeStateMachine();
    }

    protected override void doAfterScreenShot ()
    {
      base.doAfterScreenShot ();
      Statemachine.StatePanel.transform.localPosition = prePos;
      Statemachine.StatePanel.transform.localScale = preScale;
    }

    public void centerResizeStateMachine(bool isImmediate = false){
      RectTransform minX = null, minY = null, maxX = null, maxY = null;
      foreach (Transform childTrans in Statemachine.StatePanel.transform) {
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
      if(minX == null || minY == null || maxX == null || maxY == null){
        return;
      }

      Vector3 center = new Vector3((minX.localPosition.x + maxX.localPosition.x)/2.0f, (minY.localPosition.y + maxY.localPosition.y)/2.0f, 0);
      Statemachine.StatePanel.gameObject.transform.localPosition = -center;

      float width= maxX.localPosition.x- minX.localPosition.x;
      float height = maxY.localPosition.y - minY.localPosition.y;
      Vector2 maxPos = Vector2.zero;
      Vector2 minPos = Vector2.zero;
      RectTransformUtility.ScreenPointToLocalPointInRectangle(Statemachine.StatePanel.GetComponent<RectTransform>(), 
        new Vector2(Screen.width, Screen.height), Camera.main, out maxPos);
      RectTransformUtility.ScreenPointToLocalPointInRectangle(Statemachine.StatePanel.GetComponent<RectTransform>(), 
        Vector2.zero, Camera.main, out minPos);

      float scaleX = Mathf.Clamp01((maxPos.x - minPos.x)/width);
      float scaleY = Mathf.Clamp01((maxPos.y - minPos.y)/height);
      float scale = Mathf.Min(scaleX, scaleY) ;
      Statemachine.StatePanel.gameObject.transform.localScale = new Vector3(scale, scale);

    }


  }
}

