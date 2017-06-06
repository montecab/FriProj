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
	
	public override void handleCommand(SimpleJSON.JSONClass jsComponent) {
		angle.valTarget = jsComponent[piJSONTokens.ANGLE].AsFloat;
	}
	
	public override void handleState(SimpleJSON.JSONClass jsComponent) {
		Debug.Log("TODO: implement handleState() for " + this.GetType().ToString());
	}

	// returns null if this component does not have a sensor aspect, or if it's not yet implemented.
	public override SimpleJSON.JSONClass SensorState {
		// TODO - implement this sensor.
		get {
			return null;
		}
	}
}
