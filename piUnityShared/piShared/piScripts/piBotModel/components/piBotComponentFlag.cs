using UnityEngine;
using System.Collections;
using PI;

public class piBotComponentFlag : piBotComponentBase {
  
  public bool flag = false;
  public bool valueJustBecameFalse = false;
  public bool valueJustBecameTrue  = false;

  public override void tick(float dt) {
  }
  
  public override void handleCommand(WW.SimpleJSON.JSONClass jsComponent) {
  }
  
  public override void handleState(WW.SimpleJSON.JSONClass jsComponent) {
    bool newFlag         = jsComponent[piJSONTokens.WW_SENSOR_VALUE_FLAG].AsBool;
    valueJustBecameTrue  =  newFlag && !flag;
    valueJustBecameFalse = !newFlag &&  flag;
    flag                 = newFlag;
  }
  
  // returns null if this component does not have a sensor aspect, or if it's not yet implemented.
  public override WW.SimpleJSON.JSONClass SensorState {
    get {
      WW.SimpleJSON.JSONClass jsState = new WW.SimpleJSON.JSONClass();
      jsState[piJSONTokens.WW_SENSOR_VALUE_FLAG].AsInt = (flag? 0 : 1);
      return jsState;
    }
  }
}
