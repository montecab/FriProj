using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using WW.UGUI;
using System;

namespace Turing{
  public class trAuthoringMissionIconPanelController : uGUISegmentedController {

    public GameObject ButtonParent;

    private Dictionary<string, uGUISegment> typeToButtonDic = null;
    private Dictionary<uGUISegment, string> buttonToTypeDic = null;

    public Action<string> IconChangeListener = null;
    
    // Use this for initialization
    void Start () {
      InitView();
    }

    public void Open(string sprite){
      this.gameObject.SetActive(true);
      SetUpView(sprite);
    }
    
    public void SetUpView(string sprite){
      InitView();
      uGUISegment button = typeToButtonDic[sprite];
      ActivateSegment(button);
     
    }

    public override void ActivateSegment (uGUISegment seg)
    {
      base.ActivateSegment (seg);
      if(!buttonToTypeDic.ContainsKey(seg)){
        Debug.LogError("seg not in dic: "  + seg.GetComponent<trButtonBase>().Img.sprite.name);
      }

      if(IconChangeListener != null){
        IconChangeListener(buttonToTypeDic[seg]);
      }
        
    }
    
    public void InitView(){
      if(typeToButtonDic != null){
        return;
      }
      typeToButtonDic = new Dictionary<string, uGUISegment>();
      buttonToTypeDic = new Dictionary<uGUISegment, string>();
      for(int i = 0; i< trIconFactory.ChallengeIconNames.Length; ++i){
        string name = trIconFactory.ChallengeIconNames[i];
        GameObject newButton = trButtonFactory.CreateRoundButton();
        newButton.transform.SetParent(ButtonParent.transform, false);
        trButtonBase buttonbase = newButton.GetComponent<trButtonBase>();
        buttonbase.Img.sprite = trIconFactory.GetMissionIcon(name);
        
        uGUISegment segment = newButton.AddComponent<uGUISegment>();
        segment.Contents.Add(buttonbase.Focus.gameObject);
        segment.SegmentsController = this;
        segment.SegButton = buttonbase.Btn;
        
        typeToButtonDic.Add(name, segment);
        buttonToTypeDic.Add(segment, name);
        
      }
    }

  }
}

