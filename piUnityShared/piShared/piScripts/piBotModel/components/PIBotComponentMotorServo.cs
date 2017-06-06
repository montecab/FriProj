using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using PI;

public class piBotComponentMotorServo : piBotComponentBase {

  // Dynamic State
  public piInertialValue angle = new piInertialValue();

  public override void tick(float dt) {
    angle.tick(dt);
  }

  public override void handleCommand(WW.SimpleJSON.JSONClass jsComponent) {
    angle.valTarget = jsComponent[piJSONTokens.ANGLE].AsFloat;
  }

  public override void handleState(WW.SimpleJSON.JSONClass jsComponent) {
    angle.valTarget = jsComponent[piJSONTokens.ANGLE].AsFloat;
    //Debug.Log("TODO: implement handleState() for " + this.GetType().ToString());
  }

  // returns null if this component does not have a sensor aspect, or if it's not yet implemented.
  public override WW.SimpleJSON.JSONClass SensorState {
    // TODO - implement this sensor.
    get {
      WW.SimpleJSON.JSONClass jsState = new WW.SimpleJSON.JSONClass();
      jsState[piJSONTokens.ANGLE].AsFloat = angle.valTarget;
      return jsState;
    }
  }
}
