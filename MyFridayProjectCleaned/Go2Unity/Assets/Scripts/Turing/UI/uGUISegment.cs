using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace WW.UGUI{
  public class uGUISegment : MonoBehaviour {

    public List<GameObject> Contents = new List<GameObject>();
    public uGUISegmentedController SegmentsController;
    public Button SegButton;
    protected bool isInit = false;

    void Start(){
      Init();
    }
    
    public virtual void Init(){
      if(isInit){
        return;
      }
      isInit = true;
      SegButton.onClick.AddListener(() => OnClickSegment());
    }
    
    public virtual void OnClickSegment(){
      SegmentsController.ActivateSegment(this);
    }
    
    public virtual void Activate(){
      for(int i = 0; i< Contents.Count; ++i){
        Contents[i].SetActive(true);
      }
    }
    
    public virtual void Deactivate(){
      for(int i = 0; i< Contents.Count; ++i){
        Contents[i].SetActive(false);
      }
    }
  }
}
