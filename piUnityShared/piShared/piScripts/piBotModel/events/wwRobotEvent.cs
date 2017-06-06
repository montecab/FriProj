using UnityEngine;
using System.Collections.Generic;
using WW.SimpleJSON;

public class wwRobotEvent {
  public enum gesturePhase {
    WW_EVENT_GESTURE_IDLE       ,
    WW_EVENT_GESTURE_STARTED    ,
    WW_EVENT_GESTURE_ESTABLISHED,
    WW_EVENT_GESTURE_COMPLETED  ,
    WW_EVENT_GESTURE_CANCELLED  ,
    WW_EVENT_GESTURE_UNKNOWN    ,
  }
  
  private static Dictionary<string, gesturePhase> phaseDict = new Dictionary<string, gesturePhase>() {
    { "idle"       , gesturePhase.WW_EVENT_GESTURE_IDLE       },
    { "started"    , gesturePhase.WW_EVENT_GESTURE_STARTED    },
    { "established", gesturePhase.WW_EVENT_GESTURE_ESTABLISHED},
    { "completed"  , gesturePhase.WW_EVENT_GESTURE_COMPLETED  },
    { "cancelled"  , gesturePhase.WW_EVENT_GESTURE_CANCELLED  },
  };
  
  static gesturePhase stringToGesturePhase(string val) {
    if (phaseDict.ContainsKey(val)) {
      return phaseDict[val];
    }
    else {
      if (wwDoOncePerTypeVal<string>.doIt(val)) {
        WWLog.logError("unknown gesture phase: " + val);
      }
      return gesturePhase.WW_EVENT_GESTURE_UNKNOWN;
    }
  }
  
  string       identifier = "";
  gesturePhase phase      = gesturePhase.WW_EVENT_GESTURE_IDLE;
  
  public string Identifier {
    get {
      return identifier;
    }
  }
  
  public gesturePhase Phase {
    get {
      return phase;
    }
  }
  
  public wwRobotEvent(JSONClass jsEvent) {
    identifier = jsEvent["id"];
    phase      = stringToGesturePhase(jsEvent["phase"]);
  }
}
