using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using PI;

public struct piColorRGB {
	public byte r;
	public byte g;
	public byte b;
	
	public piColorRGB(byte red, byte green, byte blue) {
		r = red;
		g = green;
		b = blue;
	}
}

public class piBotComponentLightRGB : piBotComponentBase {

	public piColorRGB color;
		
	public override void tick (float dt) {
	}
	
	public override void handleCommand(SimpleJSON.JSONClass jsComponent) {
		color.r = (byte)jsComponent[piJSONTokens.RED  ].AsInt;
		color.g = (byte)jsComponent[piJSONTokens.GREEN].AsInt;
		color.b = (byte)jsComponent[piJSONTokens.BLUE ].AsInt;
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
