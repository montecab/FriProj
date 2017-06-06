using UnityEngine;
using System.Collections;
using WW.UGUI;
using UnityEngine.UI;

namespace Turing{
  public class trSensorButtonController : uGUISegment {
  
    public Image SensorImage;

    private trSensor sensor;
    public trSensor SensorData{
      set{
        if(sensor == value){
          return;
        }
        sensor = value;
        SetUpView();
      }

      get{
        return sensor;
      }
    }

    public Text Label;

    void SetUpView(){
      if(sensor == null){
        return;
      }
      Label.text = sensor.Type.ToString();
      SensorImage.sprite = trIconFactory.GetIcon(sensor.Type);
    }


  	
  }
}
