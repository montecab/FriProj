using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Collections.Generic;

using PI;

public class piBotComponentDistanceSensor : piBotComponentBase {

  public float distance = 1000000.0f;
  public float relectance   = 0.0f;

  private wwAverage windowedDistance = new wwAverage(3);

  public void setDistance(float value) {
    distance = value;
    distance = Mathf.Min(piBotConstants.simDistanceSensorMaxCm, distance);
    distance = Mathf.Max(piBotConstants.simDistanceSensorMinCm, distance);
    relectance   = (distance + 1) * piBotConstants.simDistanceSensorNoisePercent;
  }

  public float WindowedDistance {
    get {
      return windowedDistance.GetAverageValue();
    }
    set {
      windowedDistance.SetAverageValue(value);
    }
  }
  

    public override void tick(float dt) {
  }

  public override void handleCommand(WW.SimpleJSON.JSONClass jsComponent) {
  }
  
  public override void handleState(WW.SimpleJSON.JSONClass jsComponent) {
    distance = jsComponent[piJSONTokens.DISTANCE].AsFloat;
    relectance   = jsComponent[piJSONTokens.REFLECTANCE  ].AsFloat;
    windowedDistance.AddNewValue(distance);
  }

  // returns null if this component does not have a sensor aspect, or if it's not yet implemented.
  public override WW.SimpleJSON.JSONClass SensorState {
    get {
      float d = Mathf.Max(0, distance + Random.Range(-relectance, relectance));
      WW.SimpleJSON.JSONClass jsState = new WW.SimpleJSON.JSONClass();
      jsState[piJSONTokens.DISTANCE].AsFloat = d;
      jsState[piJSONTokens.REFLECTANCE  ].AsFloat = relectance;
      return jsState;
    }
  }
}
