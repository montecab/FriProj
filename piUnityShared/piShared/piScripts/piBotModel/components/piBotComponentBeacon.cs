using UnityEngine;
using System.Collections.Generic;
using PI;
using WW.SimpleJSON;

public class piBotComponentBeacon : piBotComponentBase {

  public const ushort invalidValue        = 4095;
  
  // these are only relevant for the next version of this sensor.
  public const ushort ROBOT_TYPE_MASK     = 0x003;
  public const ushort ROBOT_TYPE_DASH     = 0x000;
  public const ushort ROBOT_TYPE_DOT      = 0x001;
  public const ushort ROBOT_BODY_MASK     = 0x3F8;
  

  private wwBeaconFilter filterLeft;
  private wwBeaconFilter filterRight;
  
  public int FilterSize {
    get {
      return filterLeft.Size;
    }
    set {
      if ((filterLeft == null) || (filterLeft.Size != value)) {
        filterLeft  = new wwBeaconFilter(value);
        filterRight = new wwBeaconFilter(value);
      }
    }
  }
  
  public piBotComponentBeacon() {
    FilterSize = 10;
  }
  
  public ushort dataLeft {
    get {
      return filterLeft.Value;
    }
  }
  
  public ushort dataRight {
    get {
      return filterRight.Value;
    }
  }
  
  public static piRobotType beaconMessageToRobotType(ushort data) {
    piRobotType ret = piRobotType.UNKNOWN;
    
    switch(data) {
      default:
        break;
      case 0x55:
        ret = piRobotType.DASH;
        break;
      case 0xAA:
        ret = piRobotType.DOT;
        break;
    }
    return ret;
  }
    
  /*
  public static piRobotType beaconMessageToRobotType(ushort data) {
    ushort masked = (ushort)(data & ROBOT_TYPE_MASK);
    piRobotType ret = piRobotType.UNKNOWN;
    
    switch(masked) {
      default:
        WWLog.logWarn("unrecognized robot type in beacon message. message = " + data);
        break;
      case 0:
        ret = piRobotType.DASH;
        break;
      case 1:
        ret = piRobotType.DOT;
        break;
    }
    
    return ret;
  }
  
  public static byte beaconMessageToBody(ushort data) {
    ushort masked = (ushort)(data & ROBOT_TYPE_MASK);
    byte ret = (byte)(masked >> 3);
    return ret;
  }
  */

  public bool seeDashLeft{
    get{
      return (beaconMessageToRobotType(dataLeft) == piRobotType.DASH);
    }
  }
  
  public bool seeDashRight{
    get{
      return (beaconMessageToRobotType(dataRight) == piRobotType.DASH);
    }
  }
  
  public bool seeDash{
    get{
      return seeDashLeft||seeDashRight;
    }
  }

  public bool seeDotLeft{
    get{
      return (beaconMessageToRobotType(dataLeft) == piRobotType.DOT);
    }
  }

  public bool seeDotRight{
    get{
      return (beaconMessageToRobotType(dataRight) == piRobotType.DOT);
    }
  }

  public bool seeDot{
    get{
      return seeDotLeft||seeDotRight;
    }
  }
  
  public bool seeSomethingLeft {
    get {
      return (dataLeft != invalidValue);
    }
  }
  
  public bool seeSomethingRight {
    get {
      return (dataRight != invalidValue);
    }
  }
  
  public bool seeSomething {
    get {
      return seeSomethingLeft || seeSomethingRight;
    }
  }
  
  public override void handleState(JSONClass jsComponent) {
    filterLeft .addValue((ushort)jsComponent[piJSONTokens.LEFTDATA ].AsInt);
    filterRight.addValue((ushort)jsComponent[piJSONTokens.RIGHTDATA].AsInt);
  }
  
  public override JSONClass SensorState {
    get {
      JSONClass jsState = new JSONClass();
      jsState[piJSONTokens.LEFTDATA ].AsInt = filterLeft .Value;
      jsState[piJSONTokens.RIGHTDATA].AsInt = filterRight.Value;
      return jsState;
    }
  }
  
  public override void tick(float dt) {}
  public override void handleCommand(WW.SimpleJSON.JSONClass jsComponent) {}
  
  private class wwBeaconFilter {
    private ushort[] values = null;
    
    private Dictionary<ushort, int> counts = new Dictionary<ushort, int>();
    
    private int index    = 0;
    private int size     = 0;
    private ushort value = invalidValue;
    
    public wwBeaconFilter(int size) {
      Size = size;
    }
    
    public int Size {
      get {
        return size;
      }
      set {
        if ((value != size) || (values == null)) {
          values = new ushort[value];
          this.size = value;
        }
      }
    }
    
    public ushort Value {
      get {
        return value;
      }
    }
    
    public void addValue(ushort val) {
      if (values == null) {
        if (wwDoOncePerTypeVal<string>.doIt("unexpected beacon value 1")) {
          WWLog.logError("got a beacon value but didn't allocate space for it: 1");
        }
        value = val;
        return;
      }
      else if (values.Length == 0) {
        if (wwDoOncePerTypeVal<string>.doIt("unexpected beacon value 2")) {
          WWLog.logError("got a beacon value but didn't allocate space for it: 2");
        }
        value = val;
        return;
      }

      values[index] = val;
      index = (index + 1) % size;
      
      value = runFilter();
    }
    
    private ushort runFilter() {
      // returns the most common value in the values array, except that 'invalidValue' (4095) always loses.
      // seems like this could be optimized..
      
      counts.Clear();
      
      foreach(ushort val in values) {
        if (val != invalidValue) {
          if (!counts.ContainsKey(val)) {
            counts[val] = 0;
          }
          
          counts[val] += 1;
        }
      }
      
      int maxCount = 0;
      ushort maxVal = invalidValue;
      
      foreach(ushort val in counts.Keys) {
        if (counts[val] > maxCount) {
          maxCount = counts[val];
          maxVal = val;
        }
      }
      
      return maxVal;
    }
  }
  
}

