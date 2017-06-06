using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI; 

namespace WW.UGUI{
  [RequireComponent(typeof(Toggle))]
  public class uGUIToggle<T> : MonoBehaviour {
    public Image BG;
    public GameObject Cover;
    public bool IsEnable{
      set{

        float alpha = value? 1.0f: 0.4f;
        if(BG != null){
          BG.color = new Color(BG.color.r, BG.color.g, BG.color.b, alpha);
        }

        if(Cover != null){
          Cover.SetActive(!value);
        }

        MToggle.interactable = value;
      }
      get{
        return MToggle.interactable;
      }
    }
    public T Type;
    public Toggle MToggle;
    public bool IsOn{
      set{

        if(MToggle!= null){
          MToggle.isOn = value;
          setView();
        }
      }
      get{
        if(MToggle != null){
          return MToggle.isOn;
        }
        WWLog.logError("MToggle Not Set up");
        return false;
      }
    }
    public List<uGUIToggle<T>> ReverseToggles = new List<uGUIToggle<T>>();// if this toggle is on, these toggles will be turned off
    public List<uGUIToggle<T>> DisableToggles = new List<uGUIToggle<T>>();// if this toggle is on, these toggles will be disabled
   
    private bool isInit = false;

    void Awake(){
      if (MToggle == null){
        MToggle = GetComponent<Toggle>();
      }
      Init();
    }

    public void Init(){
      if(isInit){
        return;
      }
      MToggle.onValueChanged.AddListener(onToggle);
      isInit = true;
    }

    void setView(){

      bool isOn = MToggle.isOn;
      if(isOn){
        for(int i = 0; i< ReverseToggles.Count; ++i){
          ReverseToggles[i].MToggle.isOn = false;
        }
      }
      for(int i = 0; i< DisableToggles.Count; ++i){
        if(isOn){
          DisableToggles[i].IsOn = false;         
        }       
        DisableToggles[i].IsEnable = !isOn;
      }
    }

    public void onToggle(bool isOn){
      setView();
    }

#if UNITY_EDITOR
    protected void OnValidate(){
      if(MToggle == null){
        MToggle = this.gameObject.GetComponent<Toggle>();
      }
    }
#endif
  }
}
