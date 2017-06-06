using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace Turing{
  public class trParaTimeInputPanelController : trParameterPanelBase {

    //temp, will change to picker later
    public InputField DayInput;
    public InputField HourInput;
    public InputField MinuteInput;

    private int days;
    private int hours;
    private int minutes;
    
    public override float GetValue ()
    {
      return (float)(minutes + hours * 60 + days * 24 * 60)*60;
    }
    
    public override void SetUpView(float value){
      //order matters!
      minutes = ((int)value)/60;
      hours = minutes/60;
      minutes = minutes %60;
      days = hours/24;
      hours = hours % 24;

      DayInput.text = days.ToString();
      HourInput.text = hours.ToString();
      MinuteInput.text = minutes.ToString();
    }
    
    void Start(){
      DayInput.onEndEdit.AddListener(onDayChanged);
      HourInput.onEndEdit.AddListener(onHourChanged);
      MinuteInput.onEndEdit.AddListener(onMinuteChanged);
    }

    void onDayChanged(string s){
      days = System.Convert.ToInt16(s);
      if(OnValueChanged != null){
        OnValueChanged.Invoke(GetValue());
      }
    }

    void onHourChanged(string s){
      hours = System.Convert.ToInt16(s);
      if(OnValueChanged != null){
        OnValueChanged.Invoke(GetValue());
      }
    }

    void onMinuteChanged(string s){
      minutes = System.Convert.ToInt16(s);
      if(OnValueChanged != null){
        OnValueChanged.Invoke(GetValue());
      }
    }

  }
}
