using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Collections.Generic;

using PI;

public class piBotComponentDistanceSensorPair : piBotComponentDistanceSensor {

	public float otherDistance = 0.0f;
	public float otherMargin   = 0.0f;
	
	public override void tick(float dt) {
	}
		
	public override void handleCommand(SimpleJSON.JSONClass jsComponent) {
	}
	
	public override void handleState(SimpleJSON.JSONClass jsComponent) {
//		otherDistance = jsComponent[piJSONTokens.OTHERDISTANCE].AsFloat;
//		otherMargin   = jsComponent[piJSONTokens.OTHERMARGIN  ].AsFloat;
		
		base.handleState(jsComponent);
	}
}