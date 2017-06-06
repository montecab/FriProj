using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Collections.Generic;

using PI;

public class piBotComponentDistanceSensor : piBotComponentBase {

	public float distance = 0.0f;
	public float margin   = 0.0f;

	public void setDistance(float value) {
		distance = value;
		distance = Mathf.Min(piBotConstants.simDistanceSensorMaxCm, distance);
		distance = Mathf.Max(piBotConstants.simDistanceSensorMinCm, distance);
		margin   = (distance + 1) * piBotConstants.simDistanceSensorNoisePercent;
	}	
	
	public override void tick(float dt) {
	}
		
	public override void handleCommand(SimpleJSON.JSONClass jsComponent) {
	}
	
	public override void handleState(SimpleJSON.JSONClass jsComponent) {
		distance = jsComponent[piJSONTokens.DISTANCE].AsFloat;
		margin   = jsComponent[piJSONTokens.MARGIN  ].AsFloat;
	}
	
	// returns null if this component does not have a sensor aspect, or if it's not yet implemented.
	public override SimpleJSON.JSONClass SensorState {
		get {
			float d = Mathf.Max(0, distance + Random.Range(-margin, margin));
		
			SimpleJSON.JSONClass jsState = new SimpleJSON.JSONClass();
			jsState[piJSONTokens.DISTANCE].AsFloat = d;
			jsState[piJSONTokens.MARGIN  ].AsFloat = margin;
			return jsState;
		}
	}
}
