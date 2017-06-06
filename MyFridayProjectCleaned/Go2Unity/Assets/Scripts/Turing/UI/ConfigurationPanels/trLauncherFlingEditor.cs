using UnityEngine;
using UnityEngine.UI;
using Turing;
using WW.UGUI;
using UnityEngine.EventSystems;
using System;

namespace Turing{
  public class trLauncherFlingEditor : trSingleSliderBehaviorEditor {

    public Button HelpButton;
    public GameObject HelpDialog;

    protected override void Initialize(){
      base.Initialize();
      HelpButton.onClick.AddListener(helpButtonPressed);
    }

    // public void OnHandleTouchUp(){
    //   WWLog.logDebug("touch up on handle: " + ValueSlider.value);
    //   if (ValueSlider.value < 0.25f) {
    //     ValueSlider.value = 0.0f;
    //   }
    //   else if (ValueSlider.value < 0.75f) {
    //     ValueSlider.value = 0.5f;
    //   }
    //   else {
    //     ValueSlider.value = 1.0f;
    //   }
    // }

    protected override void SetDefaultBehaviorParameterValues(trState newState){
      //WWLog.logDebug("setting default parameter to 1.0!");
      newState.SetBehaviorParameterValue(0, 1.0f); // default to max power
    }    

    protected override void SetupByState(trState newState) {
      base.SetupByState(newState);
      TestButton.Toggleable = false;
    }

    void helpButtonPressed(){
      HelpDialog.SetActive(true);
    }
  }

}
