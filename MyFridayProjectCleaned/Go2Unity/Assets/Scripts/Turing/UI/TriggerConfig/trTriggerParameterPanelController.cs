using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

namespace Turing{
  public class trTriggerParameterPanelController : MonoBehaviour {

    private Dictionary<trTriggerType, float> recentValueForTriggers = new Dictionary<trTriggerType, float>();
    private trTrigger curTrigger;
    public trTrigger CurTrigger{
      set{
        WW.wwUtility.SetClass(ref curTrigger, value);
        setUpView();
      }
      get{
        return curTrigger;
      }
    }

    public trParameterPanelBase CurParaPanel;

    public trTimePicker TimerPanel;
    public trParameterPanelBase AnglePanel;
    public trParameterPanelBase LinearPanel;
    public trParameterPanelBase MissionTimePanel;

    public GameObject FirstLabelObj;
    public TextMeshProUGUI FirstNumberText;
    public TextMeshProUGUI FirstUnitText;

    public GameObject SecondLabelObj;
    public TextMeshProUGUI SecondNumberText;
    public TextMeshProUGUI SecondUnitText;
    public RectTransform DescriptionRect;


    void setUpView(){
      if(CurParaPanel != null){
        CurParaPanel.gameObject.SetActive(false);
        DescriptionRect.offsetMin = new Vector2(DescriptionRect.offsetMin.x, -100);
      }
      CurParaPanel = getPanel(curTrigger);

      if(CurParaPanel != null){
        CurParaPanel.SetRange(trTrigger.ParameterRangesDict[curTrigger.Type]);
        this.gameObject.SetActive(true);
        CurParaPanel.gameObject.SetActive(true);
        DescriptionRect.offsetMin = new Vector2(DescriptionRect.offsetMin.x, 0);
        if (!curTrigger.IsParameterSet && recentValueForTriggers.ContainsKey(curTrigger.Type)){
          curTrigger.ParameterValue = recentValueForTriggers[curTrigger.Type];
        }

        CurParaPanel.SetUpView(curTrigger.ParameterValue);
        setLabelView(curTrigger.ParameterValue);
        CurParaPanel.OnValueChanged.AddListener(onParaChanged);

        //FirstLabelObj.SetActive(true);
      }
      else{
        //FirstLabelObj.SetActive(false);
        //SecondLabelObj.SetActive(false);
      }
    }

    void onParaChanged(float v){
      if(curTrigger != null){
        curTrigger.ParameterValue = v;
        recentValueForTriggers[curTrigger.Type] = v;
        setLabelView(v);
      }
    }

    void setLabelView(float v){
      // SecondLabelObj.SetActive(false);
      // switch(curTrigger.Type){
      // case trTriggerType.TIME:
      // case trTriggerType.TIME_RANDOM:
      //   FirstNumberText.text = v.ToString("0.0");
      //   FirstUnitText.text = "Seconds";
      //   break;
      // case trTriggerType.TIME_LONG:
        //Not sure we need this anymore because we have nice date picker
//        int minutes = (int)v/60;
//        int seconds = (int)v % 60;
//        int hours = minutes/60;
//        int remainingMinutes = minutes % 60;
//
//        bool isFirstLabelSet = false;
//        if (hours > 0) {
//          FirstNumberText.text = hours.ToString("0");
//          FirstUnitText.text = "Hours";
//          isFirstLabelSet = true;
//        }
//        if (remainingMinutes > 0) {
//          if(isFirstLabelSet){
//            SecondLabelObj.SetActive(true);
//            SecondNumberText.text = remainingMinutes.ToString("0"); 
//            SecondUnitText.text = "Minutes";
//            return;
//          }else{
//            FirstNumberText.text = remainingMinutes.ToString("0");
//            FirstUnitText.text = "Minutes";
//            isFirstLabelSet = true;
//          } 
//        }
//        if (hours < 1 && seconds >= 0) {
//          if (!isFirstLabelSet) {
//            FirstNumberText.text = seconds.ToString("0.0");
//            FirstUnitText.text = "Seconds";
//          } else {
//            SecondLabelObj.SetActive(true);
//            SecondNumberText.text = seconds.ToString("0"); 
//            SecondUnitText.text = "Seconds";
//          }
//        }
      //   break;
      // case trTriggerType.TRAVEL_LINEAR:
      //   FirstNumberText.text = v.ToString("0.0");
      //   FirstUnitText.text = "cm/s";
      //   break;
      // case trTriggerType.TRAVEL_ANGULAR:
      //   FirstNumberText.text = v.ToString("0.0");
      //   FirstUnitText.text = "deg/s";
      //   break;
      // }
    }

    trParameterPanelBase getPanel(trTrigger trigger){
      if(trigger == null)
        return null;
      if(trTrigger.Parameterized(trigger.Type)){
        switch(trigger.Type){
        case trTriggerType.TIME_RANDOM:
        case trTriggerType.TIME_LONG:
        case trTriggerType.TIME:
          TimerPanel.TimerMode = (trigger.Type == trTriggerType.TIME_LONG) ? trTimePicker.trTimePickerMode.LONG_TIME : trTimePicker.trTimePickerMode.SHORT_TIME;
          if(trDataManager.Instance.AuthoringMissionInfo.EditState == MissionEditState.EDIT){
            return MissionTimePanel;
          }
          else{
            return TimerPanel;
          }
        case trTriggerType.TRAVEL_LINEAR:
        case trTriggerType.TRAVEL_ANGULAR:
          return LinearPanel;
        }
      }

      return null;

    }

  }
}