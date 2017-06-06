using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

namespace Turing{
  public class trBrowseTabBase : MonoBehaviour {
    
    public Dictionary<piRobotType, RectTransform> ListTable = new Dictionary<piRobotType, RectTransform>();
    public Toggle TabToggle;
    public Action<bool, trBrowseTabBase> TabListener;

    public RectTransform CurList;
    
    // Use this for initialization
    void Start () {
      initView();
    }

    public virtual void OnToggleChange(bool isOn){
      if(TabListener != null){
        TabListener(isOn, this);
      }
    }

    protected virtual void initView(){
      TabToggle.onValueChanged.AddListener(OnToggleChange);
    }

    public void SetList(piRobotType type){
      if(TabToggle.isOn){
        if(CurList != null && ListTable.ContainsKey(type) && ListTable[type] != CurList){
          CurList.gameObject.SetActive(false);
          CurList = null;
        }
        if(ListTable.ContainsKey(type)){
          ListTable[type].gameObject.SetActive(true);
          CurList = ListTable[type];
        }
      }
      else{
        if(CurList != null){
          CurList.gameObject.SetActive(false);
        }
        CurList = null;
      }
      

    }
  }
}

