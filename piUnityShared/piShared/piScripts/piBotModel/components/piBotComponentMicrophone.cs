using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Collections.Generic;

using PI;

public class piBotComponentMicrophone : piBotComponentBase {

  // as of 20140730, volume & direction do not come over the wire.
  // also, note that in APIObjectiveC, the field "eventId" is named "event",
  // but "event" is a reserved word in C#, so we use "eventId".

  public float              amplitude    = 0.0f; // Amplitude is a value between 0 and 1
  public float              direction = float.NaN;
  public PI.SoundEventIndex eventId   = PI.SoundEventIndex.SOUND_EVENT_NONE;
  public int                voiceConfidence = 0;


  public void setAmplitude(float value) {
	amplitude = value;	
  }

  public void setDirection(float value) {
    direction = value;
  }

  public void setEventId(PI.SoundEventIndex value) {
    eventId = value;
  }

  public override void tick(float dt) {
  }

  public override void handleCommand(WW.SimpleJSON.JSONClass jsComponent) {
  }

  public override void handleState(WW.SimpleJSON.JSONClass jsComponent) {
    amplitude    = (float              )jsComponent[piJSONTokens.AMPLITUDE   ].AsFloat;
    eventId   	 = (PI.SoundEventIndex)jsComponent[piJSONTokens.EVENT    ].AsInt;

    if (jsComponent[piJSONTokens.DIRECTION] == null) {
      direction = float.NaN;
    } else {
      direction = (float)jsComponent[piJSONTokens.DIRECTION].AsFloat;
    }
    
    if (jsComponent[piJSONTokens.VOICE_CONFIDENCE] == null) {
      voiceConfidence = 0;
    } else {
      voiceConfidence = (int)jsComponent[piJSONTokens.VOICE_CONFIDENCE].AsInt;
    }
  }

  // returns null if this component does not have a sensor aspect, or if it's not yet implemented.
  public override WW.SimpleJSON.JSONClass SensorState {
    get {
      WW.SimpleJSON.JSONClass jsState = new WW.SimpleJSON.JSONClass();
      jsState[piJSONTokens.AMPLITUDE   ].AsFloat   = (float)amplitude;
      jsState[piJSONTokens.DIRECTION].AsFloat =      direction;
      jsState[piJSONTokens.VOICE_CONFIDENCE].AsInt = voiceConfidence;
      jsState[piJSONTokens.EVENT    ].AsInt   = (int)eventId;
      return jsState;
    }
  }
}
