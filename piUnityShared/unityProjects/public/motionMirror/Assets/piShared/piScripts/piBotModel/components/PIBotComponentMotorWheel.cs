using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Collections.Generic;

using PI;

public class piBotComponentMotorWheel : piBotComponentBase {

	public piInertialValue velocity = new piInertialValue();
	public piTaredValue encoderDistance = new piTaredValue();
	
	public override void tick (float dt) {
		velocity.tick(dt);
	}	
	
	public override void handleCommand(SimpleJSON.JSONClass jsComponent) {
		velocity.valTarget = jsComponent[piJSONTokens.VELOCITY].AsFloat;
	}
	
	public override void handleState(SimpleJSON.JSONClass jsComponent) {
		encoderDistance.ValueRaw = jsComponent["encoderDistance"].AsFloat;
	}
	
	public void tare() {
		encoderDistance.tare();
	}
}
