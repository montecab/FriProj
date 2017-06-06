using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using PI;

public class piBotComponentEyeRing : piBotComponentBase {

	public enum EyeAnimationIndex : ushort {
		EYEANIM_NONE         = 0,
		EYEANIM_HALF_BLINK,
		EYEANIM_FULL_BLINK,
		EYEANIM_GRAVITY_EYE,
		EYEANIM_CIRCLE,
		EYEANIM_FAST_BLINK,
		
		EYEANIM_BITMAP       = 0xffff
	}
	

	public byte              brightness  = 0;
	public EyeAnimationIndex animationID = EyeAnimationIndex.EYEANIM_BITMAP;
	public ushort            bitmap      = 0;
	
	public override void tick(float dt) {
		// play animation ?
	}	
	
	public override void handleCommand(SimpleJSON.JSONClass jsComponent) {
		brightness  = (byte             )jsComponent[piJSONTokens.BRIGHTNESS ].AsInt;
		animationID = (EyeAnimationIndex)jsComponent[piJSONTokens.ANIMATIONID].AsInt;
		bitmap      = (ushort           )jsComponent[piJSONTokens.BITMAP     ].AsInt;
		
		bool unsupported = false;
		unsupported |= (jsComponent[piJSONTokens.ANIMATIONSPEED] != null);
		unsupported |= (jsComponent[piJSONTokens.LOOPS         ] != null);
		unsupported |= (animationID != EyeAnimationIndex.EYEANIM_BITMAP);
		if (unsupported) {
			Debug.LogWarning("eye-ring animations not supported yet.");
		}
	}
	
	public override void handleState(SimpleJSON.JSONClass jsComponent) {
		Debug.Log("TODO: implement handleState() for " + this.GetType().ToString());
	}
}



