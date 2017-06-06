using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using WW.UGUI;

namespace Turing{
  public class trParaAngleController : trParameterPanelBase {

    public wwCircleSlider Slider;
    
    public override float GetValue ()
    {
      return Slider.Value;
    }
    
    public override void SetUpView(float value){
      Slider.Value = value;
      Label.text = value.ToString("0");
    }
    
    void Start(){
      Slider.OnValueChanged.AddListener(OnSliderValueChanged);
    }
    
    public void OnSliderValueChanged(float v){
      if(OnValueChanged != null){
        OnValueChanged.Invoke(v);
      }
      Label.text = v.ToString("0");
    }

    public override void SetRange (wwRange range)
    {
      Slider.MinValue = range.Min;
      Slider.MinValue = range.Max;
    }
    
  }
}
