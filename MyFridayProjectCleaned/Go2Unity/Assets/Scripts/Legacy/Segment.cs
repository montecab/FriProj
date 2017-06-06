using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Segment : MonoBehaviour {
    
  public GameObject Content;
  public SegmentedController SegmentsController;
  public Button SegButton;

  void Start(){
    Init();
  }

  public virtual void Init(){
    SegButton.onClick.AddListener(() => OnClickSegment());
  }

  public virtual void OnClickSegment(){
    SegmentsController.ActivateSegment(this);
  }

  public virtual void Activate(){
    if(Content != null){
      this.Content.SetActive (true);
    }
  }

  public virtual void Deactivate(){
    if(Content != null){
      this.Content.SetActive (false);
    }
  }
}
