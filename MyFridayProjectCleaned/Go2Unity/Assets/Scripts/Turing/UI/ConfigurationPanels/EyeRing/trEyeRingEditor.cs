using UnityEngine;
using UnityEngine.UI;
using Turing;
using WW.UGUI;

namespace Turing{
  public class trEyeRingEditor : trSimpleBehaviorEditor {

    public trEyeRingView EyeRingView;

    void onEyeRingValueChanged(trEyeRingView sender){
      State.SetBehaviorParameterValue(0, sender.SerializedValue);
      ExecutOnRobot(true);
    }

    protected override void UpdateUI(){
      EyeRingView.SerializedValue = (int) State.GetBehaviorParameterValue(0);
    }

    protected override void Initialize(){
      base.Initialize();
      EyeRingView.OnChanged.AddListener(onEyeRingValueChanged);
      // TUR-1252: by default lights off, and thus not triggering value change, force trigger it!
      onEyeRingValueChanged(EyeRingView); 
    }

    // protected override void SetupByState(trState newState) {
    //   base.SetupByState(newState);
    // }

//    protected override void SetupWideView () {
//      gameObject.AddComponent<HorizontalLayoutGroup>();
//    }
  }  
}

