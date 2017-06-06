using UnityEngine;
using UnityEngine.UI;
using Turing;
using WW.UGUI;
using TMPro;

public class trDoubleSliderBehaviorEditor : trSingleSliderBehaviorEditor {

  public SliderPanelController SliderPanel2;
  public TextMeshProUGUI ValueLabel2;
  public trSliderExtendedThumb ExtendedThumb2;

  protected override void Initialize(){
    base.Initialize();
    SliderPanel2.ValueSlider.onValueChanged.AddListener(onSliderValueChanged2);
  }

  protected override void SetupByState(trState newState) {
    base.SetupByState(newState);
    if (!newState.IsParameterValueSet(1)) {
      newState.SetBehaviorParameterValue(1, 0); // default to zero
      UpdateUI();
    }
    SliderPanel2.ValueSlider.UpdateValue(Mathf.Lerp(SliderPanel2.ValueSlider.minValue, SliderPanel2.ValueSlider.maxValue, newState.GetNormalizedBehaviorParameterValue(1)));
  }

  protected override void UpdateUI(){
    base.UpdateUI();
    ValueLabel2.text = trStringFactory.GetParaString(State, 1);
    if (ExtendedThumb2 != null){
      string[] values = trStringFactory.GetParaStringAndUnit(State, 1);
      ExtendedThumb2.SetValueAndUnit(values[0], values[1]);
    }    
  }

  void onSliderValueChanged2 (float value) {
    float normValue = Mathf.InverseLerp(SliderPanel2.ValueSlider.minValue, SliderPanel2.ValueSlider.maxValue, value);
    State.SetNormalizedBehaviorParameterValue(1, normValue);   
    ExecutOnRobot(false); // automatically stop testing
//    Debug.Log ("set slider 1 to: " + value + ", showing: " + State.GetNormalizedBehaviorParameterValue(0));
    UpdateUI();
  }
}
