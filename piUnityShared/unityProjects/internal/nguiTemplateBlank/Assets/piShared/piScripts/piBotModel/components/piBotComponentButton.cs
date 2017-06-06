using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Collections.Generic;

using PI;

public class piBotComponentButton : piBotComponentBase {

	public ButtonState state = ButtonState.BUTTON_NOTPRESSED;
	
	public override void tick(float dt) {
	}
		
	public override void handleCommand(SimpleJSON.JSONClass jsComponent) {
	}
	
	public override void handleState(SimpleJSON.JSONClass jsComponent) {
		state = (ButtonState)(jsComponent[piJSONTokens.STATE].AsInt);
	}

	// returns null if this component does not have a sensor aspect, or if it's not yet implemented.
	public override SimpleJSON.JSONClass SensorState {
		get {
			SimpleJSON.JSONClass jsState = new SimpleJSON.JSONClass();
			jsState[piJSONTokens.STATE].AsInt = (state == ButtonState.BUTTON_NOTPRESSED ? 0 : 1);
			return jsState;
		}
	}
}
