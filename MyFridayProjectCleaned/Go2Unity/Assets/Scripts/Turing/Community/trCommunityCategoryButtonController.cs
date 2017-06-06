using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;


namespace Turing{
  public class trCommunityCategoryButtonController : trBrowseTabBase {
    public CommunityCategory Category;
    public TextMeshProUGUI Label;
    public Action<bool, trCommunityCategoryButtonController> ToggleListener;

    public void HideAllLists(){
      foreach(RectTransform list in ListTable.Values){
        list.gameObject.SetActive(false);
      }
    }

    // Use this for initialization
    protected override void initView(){
      base.initView();
    }

    public override void OnToggleChange(bool isOn){
      Label.color = isOn? new Color(Label.color.r, Label.color.g, Label.color.b, 1) :
        new Color(Label.color.r, Label.color.g, Label.color.b, 0.3f);
      if(ToggleListener != null){
        ToggleListener(isOn, this);
      }
    }
  }
}

