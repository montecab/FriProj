using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Collections.Generic;

using PI;

public class piBotComponentDistanceSensor : piBotComponentBase {

	public float distance    = 0.0f;
	public float reflectance = 0.0f;

	public void setDistance(float value) {
		// TODO orion - this should set reflectance. need to use PIReflectance from API.
		distance = value;
		distance = Mathf.Min(piBotConstants.simDistanceSensorMaxCm, distance);
		distance = Mathf.Max(piBotConstants.simDistanceSensorMinCm, distance);
	}
	
	public void setReflectance(float value) {
		// TODO orion - this should set distance. need to use PIReflectance from API.
		reflectance = value;
	}
	
	public override void tick(float dt) {
	}
		
	public override void handleCommand(SimpleJSON.JSONClass jsComponent) {
	}
	
	public override void handleState(SimpleJSON.JSONClass jsComponent) {
		distance    = jsComponent[piJSONTokens.DISTANCE   ].AsFloat;
		reflectance = jsComponent[piJSONTokens.REFLECTANCE].AsFloat;
	}
	
	// returns null if this component does not have a sensor aspect, or if it's not yet implemented.
	public override SimpleJSON.JSONClass SensorState {
		get {
			float r = Mathf.Max(0, reflectance + Random.Range(-20, 20));
		
			SimpleJSON.JSONClass jsState = new SimpleJSON.JSONClass();
			jsState[piJSONTokens.REFLECTANCE].AsFloat = r;
			return jsState;
		}
	}
}
