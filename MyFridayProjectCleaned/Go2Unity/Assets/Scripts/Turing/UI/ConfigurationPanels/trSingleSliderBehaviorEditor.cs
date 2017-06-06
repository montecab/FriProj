using UnityEngine;
using UnityEngine.UI;
using Turing;
using WW.UGUI;
using TMPro;

namespace Turing{
  public class trSingleSliderBehaviorEditor : trSimpleBehaviorEditor {

    public SliderPanelController SliderPanel;
    public TextMeshProUGUI ValueLabel;
    public trSliderExtendedThumb ExtendedThumb;

    protected override void Initialize(){
      base.Initialize();
      SliderPanel.ValueSlider.onValueChanged.AddListener(onSliderValueChanged);
    }

    protected override void SetupByState(trState newState) {
      base.SetupByState(newState);
      if (!newState.IsParameterValueSet(0)){
        SetDefaultBehaviorParameterValues(newState);
        UpdateUI(); 
      }
      SliderPanel.ValueSlider.UpdateValue(Mathf.Lerp(SliderPanel.ValueSlider.minValue, SliderPanel.ValueSlider.maxValue, newState.GetNormalizedBehaviorParameterValue(0)));
    }

    protected virtual void SetDefaultBehaviorParameterValues(trState newState){
      newState.SetBehaviorParameterValue(0, 0); // default to zero
    }

    protected override void UpdateUI(){
      base.UpdateUI();
      ValueLabel.text = trStringFactory.GetParaString(State, 0);
      if (ExtendedThumb != null){
        string[] values = trStringFactory.GetParaStringAndUnit(State, 0);
        ExtendedThumb.SetValueAndUnit(values[0], values[1]);
      }    
    }

    void onSliderValueChanged (float value) {
      float normValue = Mathf.InverseLerp(SliderPanel.ValueSlider.minValue, SliderPanel.ValueSlider.maxValue, value);
      State.SetNormalizedBehaviorParameterValue(0, normValue);    
      ExecutOnRobot(false); // automatically stop testing
      //WWLog.logDebug("set slider 1 to: " + value + ", showing: " + State.GetNormalizedBehaviorParameterValue(0));
      UpdateUI();
    }
  }
}