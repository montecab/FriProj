using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI; 

namespace WW.UGUI{
  public class uGUIToggleGroup<T> : MonoBehaviour {
    
    public Dictionary<T, uGUIToggle<T>> ToggleTable = new Dictionary<T, uGUIToggle<T>>();
    public Button ClearButton;
    public Transform ToggleParentTransform;
    protected bool isInited = false;

    void Awake(){
      Init();
    }

    public virtual void Init(){
      if(isInited){
        return;
      }

      if(ClearButton != null){
        ClearButton.onClick.AddListener(()=>onClearButtonClicked());
      }
      if (ToggleParentTransform == null){
        ToggleParentTransform = this.transform;
      } 
      foreach(Transform child in ToggleParentTransform){
        uGUIToggle<T> toggle = child.GetComponent<uGUIToggle<T>>();
        if(toggle != null && toggle.Type.GetType() == typeof(T)){
          if(!ToggleTable.ContainsValue(toggle)){
            Register(toggle); 
          }
        }
      }
      isInited = true;
    }

    public void SetToggle(T type, bool isOn){
      if(!ToggleTable.ContainsKey(type)){
        WWLog.logError(type + " not exist.");
        return;
      }
      ToggleTable[type].IsOn = isOn;
    }

    void onClearButtonClicked(){
      foreach(T t in ToggleTable.Keys){
        ToggleTable[t].IsOn = false;
      }
    }

    public uGUIToggle<T> GetFirstOnToggle(){
      foreach(T type in ToggleTable.Keys){
        if(ToggleTable[type].IsOn){
          return ToggleTable[type];
        }
      }
      return null;
    }

    public void AddDisableGroup(List<T> first, List<T> second){
      for(int i = 0; i< first.Count; i++){
        T type1 = first[i]; 
        if(!ToggleTable.ContainsKey(type1)){
          WWLog.logError("Trying to add a disable toggle" + "(" + type1.ToString() + ")" +" that's not in toggle table. Did you set that up?");
        }
        for(int j = 0; j< second.Count; j++){
          T type2 = second[j];
          ToggleTable[type1].DisableToggles.Add(ToggleTable[type2]);
        }
      }
    }

    public void AddReverseGroup(List<T> first, List<T> second){
      AddReverseGroupToFirst(first, second);
      AddReverseGroupToFirst(second, first);
    }

    public void AddReverseGroupToFirst(List<T> first, List<T> second){
      for(int i = 0; i< first.Count; i++){
        T type1 = first[i]; 
        if(!ToggleTable.ContainsKey(type1)){
          WWLog.logError("Trying to add a reverse toggle" + "(" + type1.ToString() + ")" +" that's not in toggle table. Did you set that up?");
        }
        for(int j = 0; j< second.Count; j++){
          T type2 = second[j];
          if (!ToggleTable.ContainsKey(type2)){
            WWLog.logError("Trying to remove reverse toggle" + "(" + type2.ToString() + ")" +" that's not in toggle table. Did you set that up?");
          } else {
            ToggleTable[type1].ReverseToggles.Add(ToggleTable[type2]);
          }
        }
      }
    }
    
    public void Register(uGUIToggle<T> toggle){
      if(ToggleTable.ContainsKey(toggle.Type)){
        WWLog.logError("Trying to add a same type to toggle group");
      }
      else{
        ToggleTable.Add(toggle.Type, toggle);
        toggle.MToggle.onValueChanged.AddListener(onAnyToggleChanged);
      }
    }

    protected virtual void onAnyToggleChanged(bool val){

    }

    
    public void UnRegister(uGUIToggle<T> toggle){
      if(ToggleTable.ContainsKey(toggle.Type)){
        ToggleTable.Remove(toggle.Type);
        toggle.MToggle.onValueChanged.RemoveListener(onAnyToggleChanged);
      }
    }

//    #if UNITY_EDITOR
//    protected void OnValidate(){
//      foreach(Transform child in this.transform){
//        uGUIToggle<T> toggle = child.GetComponent<uGUIToggle<T>>();
//        if(toggle != null){
//          if(!ToggleTable.ContainsValue(toggle)){
//            Register(toggle); 
//          }
//        }
//      }
//    }
//    #endif
  }
}
