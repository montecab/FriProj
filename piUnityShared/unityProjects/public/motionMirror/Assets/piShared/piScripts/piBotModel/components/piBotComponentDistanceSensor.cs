using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Collections.Generic;

using PI;

public class piBotComponentDistanceSensor : piBotComponentBase {

	public float distance = 0.0f;
	public float margin   = 0.0f;
	
	public override void tick(float dt) {
	}
		
	public override void handleCommand(SimpleJSON.JSONClass jsComponent) {
	}
	
	public override void handleState(SimpleJSON.JSONClass jsComponent) {
		distance = jsComponent[piJSONTokens.DISTANCE].AsFloat;
		margin   = jsComponent[piJSONTokens.MARGIN  ].AsFloat;
	}
}
