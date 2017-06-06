using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Collections.Generic;

using PI;

public class piBotComponentAccelerometer : piBotComponentBase {

	public float x = 0.0f;
	public float y = 0.0f;
	public float z = 0.0f;
	
	public override void tick (float dt) {
	}	
	
	public override void handleCommand(SimpleJSON.JSONClass jsComponent) {
		// no-op
	}
	
	public override void handleState(SimpleJSON.JSONClass jsComponent) {
		x = jsComponent["x"].AsFloat;
		y = jsComponent["y"].AsFloat;
		z = jsComponent["z"].AsFloat;
	}
}
