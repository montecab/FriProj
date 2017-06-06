using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Collections.Generic;

using PI;

public class piBotComponentButton : piBotComponentBase {

  public ButtonState state = ButtonState.BUTTON_NOTPRESSED;

  public override void tick(float dt) {
  }

  public override void handleCommand(WW.SimpleJSON.JSONClass jsComponent) {
  }

  public override void handleState(WW.SimpleJSON.JSONClass jsComponent) {
    state = (ButtonState)(jsComponent[piJSONTokens.STATE].AsInt);
  }

  // returns null if this component does not have a sensor aspect, or if it's not yet implemented.
  public override WW.SimpleJSON.JSONClass SensorState {
    get {
      WW.SimpleJSON.JSONClass jsState = new WW.SimpleJSON.JSONClass();
      jsState[piJSONTokens.STATE].AsInt = (state == ButtonState.BUTTON_NOTPRESSED ? 0 : 1);
      return jsState;
    }
  }
}
