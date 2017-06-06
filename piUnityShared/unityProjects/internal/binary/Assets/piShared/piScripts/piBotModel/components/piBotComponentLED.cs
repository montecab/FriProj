using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using PI;

public class piBotComponentLED : piBotComponentBase {
	
    // Desired State
    public byte brightness; 

	public override void tick(float dt) {
		// not much to do here.
	}	
	
	public override void handleCommand(SimpleJSON.JSONClass jsComponent) {
		brightness = (byte)jsComponent[piJSONTokens.BRIGHTNESS].AsInt;
	}
	
	public override void handleState(SimpleJSON.JSONClass jsComponent) {
		Debug.Log("TODO: implement handleState() for " + this.GetType().ToString());
	}
	
	// returns null if this component does not have a sensor aspect, or if it's not yet implemented.
	public override SimpleJSON.JSONClass SensorState {
		get {
			return null;
		}
	}
}
