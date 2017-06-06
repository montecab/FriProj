using UnityEngine;
using System.Collections;

namespace Turing {

  // ugh, tried to have this inherit from a singleton base-class, but ran into trouble.
  public class trTelemetry_Flurry : Singleton<trTelemetry_Flurry> {
  
    private FlurryAgent service;
  
    public trTelemetry_Flurry() {
      service = FlurryAgent.Instance;
    }

    private bool haveWarnedNotEmitting = false;

    public void Emit (trTelemetryEvent trTEI) {
      WWLog.logInfo("sending to flurry: " + trTEI.ToString());
      #if UNITY_EDITOR || UNITY_STANDALONE_OSX
      bool mute = true;
      #else
      bool mute = false;
      #endif

      if (mute) {
        if (!haveWarnedNotEmitting) {
          haveWarnedNotEmitting = true;
          WWLog.logWarn("WARNING: NOT EMITTING EVENTS TO FLURRY BECAUSE THIS IS UNITY EDITOR OR DESKTOP BUILD.");
        }
      }
      else {
        service.logEvent(trTEI.eventType.ToString(), trTEI.paramDict);
      }
    }
  }  
}