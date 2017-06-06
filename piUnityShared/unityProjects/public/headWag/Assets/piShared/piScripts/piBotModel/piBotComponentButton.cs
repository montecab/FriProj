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
}
