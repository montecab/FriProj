using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Collections.Generic;

using PI;

public class piBotComponentDistanceSensorPair : piBotComponentDistanceSensor {

  public float otherDistance = 0.0f;
  public float otherMargin   = 0.0f;

  public override void tick(float dt) {
  }

  public override void handleCommand(WW.SimpleJSON.JSONClass jsComponent) {
  }

  public override void handleState(WW.SimpleJSON.JSONClass jsComponent) {
//  otherDistance = jsComponent[piJSONTokens.OTHERDISTANCE].AsFloat;
//  otherMargin   = jsComponent[piJSONTokens.OTHERMARGIN  ].AsFloat;
    base.handleState(jsComponent);
  }

  // returns null if this component does not have a sensor aspect, or if it's not yet implemented.
  public override WW.SimpleJSON.JSONClass SensorState {
    get {
      return base.SensorState;
    }
  }
}
