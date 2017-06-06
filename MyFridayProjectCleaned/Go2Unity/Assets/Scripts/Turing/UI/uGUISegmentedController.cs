using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace WW.UGUI{
  public class uGUISegmentedController : MonoBehaviour {

    public List<uGUISegment> Segments = new List<uGUISegment>();

    public uGUISegment StartSegment;
    
    protected uGUISegment activatedSegment;

    protected float LastSegmentActivationTime = float.NaN;
    protected const float DOUBLE_ACTIVATION_TIME = 0.3f; //seconds


    void OnValidate(){
      foreach(uGUISegment seg in Segments){
        seg.Deactivate();
      }
      Refresh();
    }

    public virtual void AddSegment(uGUISegment seg){
      Segments.Add(seg);
    }
    
    public void Refresh(){
      if(StartSegment != null){
       ActivateSegment(StartSegment);
      }
    }
    
    public virtual void ActivateSegment(uGUISegment seg){

      if (isDoubleClickDetected(seg)) {
        OnDoubleClickOnSegment();
      }

      if (activatedSegment != null) {
        activatedSegment.Deactivate();
      }
      activatedSegment = seg;
      if (activatedSegment != null) {
        activatedSegment.Activate();
      }
      
    }

    protected virtual void OnDoubleClickOnSegment(){ }

    private bool isDoubleClickDetected (uGUISegment seg){

      bool result = false;

      if(float.IsNaN(LastSegmentActivationTime)){
        LastSegmentActivationTime = Time.fixedTime;
      } else if (seg == activatedSegment){
        result = (Time.fixedTime - LastSegmentActivationTime) < DOUBLE_ACTIVATION_TIME;
      } 

      if (result){
        LastSegmentActivationTime = float.NaN;
      } else {
        LastSegmentActivationTime = Time.fixedTime;
      }

      return result;
    }
  }
}
