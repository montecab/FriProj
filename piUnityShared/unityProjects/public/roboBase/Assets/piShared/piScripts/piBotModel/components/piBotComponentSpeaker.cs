using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using PI;

public class piBotComponentSpeaker : piBotComponentBase {

	public int soundID   = 0;
	public int volume    = 255;
	
	public override void tick(float dt) {
	}
	
	public override void handleCommand(SimpleJSON.JSONClass jsComponent) {
		Debug.LogWarning("TODO: implement this.");
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
