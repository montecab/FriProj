using UnityEngine;
using System.Collections.Generic;
using WW.SimpleJSON;
using PI;

public class piBotComponentBeaconV2 : piBotComponentBase {

  public enum dataType_t : uint {
    color = 0,
    user  = 1,
  }
  
  private const float maxAgeSeconds = 0.3f;
  
  public List<BeaconV2Data> history = new List<BeaconV2Data>();
  private BeaconV2Data emptyData = new BeaconV2Data();


  public override void handleState(JSONClass jsComponent) {
      history.Add(BeaconV2Data.parse(jsComponent));
      removeOldEvents();
  }
  
  public BeaconV2Data CurrentData {
    get {
      removeOldEvents();
      if (history.Count > 0) {
        return history[history.Count - 1];
      }
      else {
        return emptyData;
      }
    }
  }
  
  
  protected static float timeNow {
    get {
      return Time.realtimeSinceStartup;
    }
  }
  
  // this routine gets called fairly regularly,
  // so we want it to be reasonably performant.
  private void removeOldEvents() {
    float oldThresh = piBotComponentBeaconV2.timeNow - maxAgeSeconds;
    
    if ((history.Count > 0) && (history[history.Count - 1].timeReceived < oldThresh)) {
      history.Clear();
    }
    else {
      int numOld = 0;
      for (int n = 0; n < history.Count; ++n) {
        if (history[n].timeReceived > oldThresh) {
          numOld = n;
          break;
        }
      }
      
      if (numOld > 0) {
        history.RemoveRange(0, numOld);
      }
    }
  }
  
  public bool sawRobot(piRobotType robotType, WWBeaconReceiver receiversMask) {
    removeOldEvents();
    
    foreach (BeaconV2Data bv2d in history) {
      if (bv2d.robotType == robotType) {
        if ((bv2d.receivers & receiversMask) == receiversMask) {
          return true;
        }
      }
    }
    
    return false;
  }
  
  public bool seeDashLeft {
    get {
      return sawRobot(piRobotType.DASH, WWBeaconReceiver.WW_BEACON_RECEIVER_LEFT);
    }
  }
  
  public bool seeDashRight {
    get {
      return sawRobot(piRobotType.DASH, WWBeaconReceiver.WW_BEACON_RECEIVER_RIGHT);
    }
  }
  
  
  public bool seeDash{
    get {
      return seeDashLeft || seeDashRight;
    }
  }
  public bool seeDotLeft {
    get {
      return sawRobot(piRobotType.DOT, WWBeaconReceiver.WW_BEACON_RECEIVER_LEFT);
    }
  }
  
  public bool seeDotRight {
    get {
      return sawRobot(piRobotType.DOT, WWBeaconReceiver.WW_BEACON_RECEIVER_RIGHT);
    }
  }
  
  
  public bool seeDot {
    get {
      return seeDotLeft || seeDotRight;
    }
  }
  
  public bool seeSomethingLeft {
    get {
      return seeDashLeft || seeDotLeft;
    }
  }
  
  public bool seeSomethingRight {
    get {
      return seeDashRight || seeDotRight;
    }
  }
  
  public bool seeSomething {
    get {
      return seeSomethingLeft || seeSomethingRight;
    }
  }
  
  
  
  
  public override JSONClass SensorState {
    get {
      JSONClass jsState = new JSONClass();
      return jsState;
    }
  }

  public override void tick(float dt) {}
  public override void handleCommand(WW.SimpleJSON.JSONClass jsComponent) {}
    
  public class BeaconV2Data {
    public piRobotType      robotType        = piRobotType.UNKNOWN;
    public uint             robotID;
    public dataType_t       dataType         = dataType_t.color;
    public uint             dataLengthBits;
    public uint             data;
    public WWBeaconLevel[]  receiverLevels   = new WWBeaconLevel[0];
    public WWBeaconReceiver receivers;
    
    public float            timeReceived;                 // todo: replace this with time emitted. needs API support.
    public WWBeaconLevel    distanceLevel = WWBeaconLevel.BEACON_LEVEL_OFF;
    
    
    public static BeaconV2Data parse(JSONClass jsc) {
      BeaconV2Data ret = new BeaconV2Data();
      
      ret.timeReceived              = piBotComponentBeaconV2.timeNow;
      ret.robotType                 = (piRobotType     )(jsc[PI.piJSONTokens.WW_SENSOR_VALUE_BEACON_ROBOT_TYPE         ].AsInt);
      ret.robotID                   = (uint            )(jsc[PI.piJSONTokens.WW_SENSOR_VALUE_BEACON_ROBOT_ID           ].AsInt);
      ret.dataType                  = (dataType_t      )(jsc[PI.piJSONTokens.WW_SENSOR_VALUE_BEACON_DATA_TYPE          ].AsInt);
      ret.dataLengthBits            = (uint            )(jsc[PI.piJSONTokens.WW_SENSOR_VALUE_BEACON_DATA_LENGTH_BITS   ].AsInt);
      ret.data                      = (uint            )(jsc[PI.piJSONTokens.WW_SENSOR_VALUE_BEACON_DATA               ].AsInt);
      
      // parse array of receiver power levels
      JSONArray jsaRcvrs            =                    jsc[PI.piJSONTokens.WW_SENSOR_VALUE_BEACON_RECEIVERS          ].AsArray;
      
      ret.receiverLevels            = new WWBeaconLevel[jsaRcvrs.Count];
      for (int n = 0; n < jsaRcvrs.Count; ++n) {
        ret.receiverLevels[n] = (WWBeaconLevel)(jsaRcvrs[n].AsInt);
      }
      
      // determine closest distance level
      ret.distanceLevel = WWBeaconLevel.BEACON_LEVEL_OFF;
      foreach (WWBeaconLevel lvl in ret.receiverLevels) {
        if ((lvl != WWBeaconLevel.BEACON_LEVEL_OFF) && ((lvl < ret.distanceLevel) || (ret.distanceLevel == WWBeaconLevel.BEACON_LEVEL_OFF))) {
          ret.distanceLevel = lvl;
        }
      }
      
      // map receiver levels to receiver enums
      ret.receivers = WWBeaconReceiver.WW_BEACON_RECEIVER_UNKNOWN;
      if (ret.distanceLevel != WWBeaconLevel.BEACON_LEVEL_OFF) {
        if (ret.receiverLevels[0] == ret.distanceLevel) {
          ret.receivers |= WWBeaconReceiver.WW_BEACON_RECEIVER_RIGHT;
        }
        if (ret.receiverLevels[1] == ret.distanceLevel) {
          ret.receivers |= WWBeaconReceiver.WW_BEACON_RECEIVER_LEFT;
        }
      }

      return ret;
    }
  }
}
