using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Collections.Generic;

using PI;

public class piBotComponentSoundSensor : piBotComponentBase {

	// as of 20140730, volume & direction do not come over the wire.
	// also, note that in APIObjectiveC, the field "eventId" is named "event",
	// but "event" is a reserved word in C#, so we use "eventId".
	
	public uint               volume    = 0;
	public float              direction = 0.0f;
	public PI.SoundEventIndex eventId   = PI.SoundEventIndex.SOUND_EVENT_NONE;
	
	public void setVolume(uint value) {
		volume = (uint)Mathf.Min(value, ((uint)PI.SoundVolumeSpecialValues.PI_VOLUME_INVALID) - 1);
	}	
	
	public void setDirection(float value) {
		direction = value;
	}
	
	public void setEventId(PI.SoundEventIndex value) {
		eventId = value;
	}	
	
	public override void tick(float dt) {
	}
	
	public override void handleCommand(SimpleJSON.JSONClass jsComponent) {
	}
	
	public override void handleState(SimpleJSON.JSONClass jsComponent) {
		volume    = (uint              )jsComponent[piJSONTokens.VOLUME   ].AsInt;
		direction = (float             )jsComponent[piJSONTokens.DIRECTION].AsFloat;
		eventId   = (PI.SoundEventIndex)jsComponent[piJSONTokens.EVENT    ].AsInt;
	}
	
	// returns null if this component does not have a sensor aspect, or if it's not yet implemented.
	public override SimpleJSON.JSONClass SensorState {
		get {
			SimpleJSON.JSONClass jsState = new SimpleJSON.JSONClass();
			jsState[piJSONTokens.VOLUME   ].AsInt   = (int)volume;
			jsState[piJSONTokens.DIRECTION].AsFloat =      direction;
			jsState[piJSONTokens.EVENT    ].AsInt   = (int)eventId;
			return jsState;
		}
	}
}
