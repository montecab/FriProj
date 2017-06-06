using UnityEngine;
using System.Collections;

public class trUsageController : MonoBehaviour {

  private const int USAGE_TIMEOUT = 15; //seconds

  private bool isHidden = false;
  private float lastUsageTime = float.NaN;
  private bool isInContinuesUsage = false;

	public delegate void VisabilityChangeDelegate(bool instant=false);
  public VisabilityChangeDelegate OnShouldHide;
  public VisabilityChangeDelegate OnShouldShow;

  void Update(){
    if (!isHidden && !isInContinuesUsage && !float.IsNaN(lastUsageTime) && Time.time - lastUsageTime > USAGE_TIMEOUT){
      if (OnShouldHide != null){
        OnShouldHide();
      }
      isHidden = true;
    }
  }

  public bool IsPannelHidden {
    get {
      return isHidden;
    }
  }

  public void ReportInstantUsage(){
    notifyIfHidden();
    lastUsageTime = Time.time;
  }

  public void StartContinuesUsage(){
    isInContinuesUsage = true;
  }

  public void StopContinuesUsage(){
    isInContinuesUsage = false;
    isHidden = false;
    lastUsageTime = Time.time;
  }
  
  public void ForceToggle() {
    if (!isHidden) {
      ForceHide();
    }
    else {
      ForceShow();
    }
  }

  public void ForceHide(){
    if (!isHidden){
      if (OnShouldHide != null){
        OnShouldHide();
      }
      else {
        // todo: this was printing during some activity for kevin in Challenge Authoring mode,
        //       but i can't repro. it's benign, but the todo here is understand why i can't repro,
        //       and also add a more robust check.  - oxe.
        // WWLog.logError("OnShouldHide is null.");
      }
      isHidden = true;
    }
  }

  public void ForceShow(){
    if (isHidden){
      if (OnShouldShow != null){
        OnShouldShow();
      }
      else {
        // todo: this was printing during some activity for kevin in Challenge Authoring mode,
        //       but i can't repro. it's benign, but the todo here is understand why i can't repro,
        //       and also add a more robust check.  - oxe.
        // WWLog.logError("OnShouldShow is null.");
      }
      lastUsageTime = Time.time;
      isHidden = false;
    }
  }
  
  void notifyIfHidden(bool instant=false){
    if (isHidden){
      if (OnShouldShow != null){
        OnShouldShow(instant);
      }
      isHidden = false;
    }
  }
}
