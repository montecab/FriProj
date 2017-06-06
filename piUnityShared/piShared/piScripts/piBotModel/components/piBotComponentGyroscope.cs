using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Collections.Generic;

using PI;

public class piBotComponentGyroscope : piBotComponentBase {
  public float x = 0.0f;
  public float y = 0.0f;
  public float z = 0.0f;

  public override void tick(float dt) {
  }

  public override void handleCommand(WW.SimpleJSON.JSONClass jsComponent) {
  }

  public override void handleState(WW.SimpleJSON.JSONClass jsComponent) {
    x = jsComponent[piJSONTokens.X].AsFloat;
    y = jsComponent[piJSONTokens.Y].AsFloat;
    z = jsComponent[piJSONTokens.Z].AsFloat;
  }

  // returns null if this component does not have a sensor aspect, or if it's not yet implemented.
  public override WW.SimpleJSON.JSONClass SensorState {
    get {
      WW.SimpleJSON.JSONClass jsState = new WW.SimpleJSON.JSONClass();
      jsState[piJSONTokens.X].AsFloat = x;
      jsState[piJSONTokens.Y].AsFloat = y;
      jsState[piJSONTokens.Z].AsFloat = z;
      return jsState;
    }
  }
}