using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace Turing{
  public class trParaSliderController : trParameterPanelBase {

    public Slider MSlider;

    public override float GetValue ()
    {
      return Mathf.Round(MSlider.value * 10.0f) / 10.0f;
    }

  	protected virtual string GetLabelValue(float value) {
  		return value.ToString("0.0");
  	}

    public override void SetUpView(float value){
      MSlider.value = value;
      if(Label != null){
        Label.text = GetLabelValue(value);
      }
	    
    }

    void Start(){
      MSlider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    public void OnSliderValueChanged(float v){
      if(OnValueChanged != null){
        OnValueChanged.Invoke(v);
      }
      if(Label != null){
	      Label.text = GetLabelValue(v);
      }
    }

    public override void SetRange (wwRange range)
    {
      MSlider.minValue = range.Min;
      MSlider.maxValue = range.Max;
    }


  }
}
