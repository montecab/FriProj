using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Turing{
  public class trSensorParaPanelController : MonoBehaviour {
    
    private trSensor curSensor;
    public trSensor CurSensor{
      set{
        WW.wwUtility.SetClass(ref curSensor, value);
        setUpView();
      }
      get{
        return curSensor;
      }
    }
    
    private trParameterPanelBase curParaPanel;

    public trParameterPanelBase TimerPanel;
    public trParameterPanelBase AnglePanel;
    public trParameterPanelBase LinearPanel;
    
    
    void setUpView(){
      if(curParaPanel != null){
        curParaPanel.gameObject.SetActive(false);
      }
      curParaPanel = getPanel(curSensor);
      if(curParaPanel != null){
        this.gameObject.SetActive(true);
        curParaPanel.gameObject.SetActive(true);
        curParaPanel.SetUpView(curSensor.ParameterValue);
        curParaPanel.OnValueChanged.AddListener(onParaChanged);
      }
    }
    
    void onParaChanged(float v){
      if(curSensor != null){
        curSensor.ParameterValue = v;
      }
    }
    
    trParameterPanelBase getPanel(trSensor sensor){
      if(sensor == null)
        return null;
      if(trSensor.Parameterized(sensor.Type)){
        switch(sensor.Type){
        case trSensorType.TIME_IN_STATE:
          return TimerPanel;
        case trSensorType.TRAVEL_ANGULAR:
          return AnglePanel;
        case trSensorType.TRAVEL_LINEAR:
          return LinearPanel;
        }
      }
      
      return null;
      
    }
    
  }
}