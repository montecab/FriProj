using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Collections.Generic;

using PI;

public class piBotComponentAccelerometer : piBotComponentBase {
  
  public float x = 0.0f;
  public float y = 0.0f;
  public float z = 0.0f;
  
  public float WindowedX {
    get {
      return this.windowedX.GetAverageValue();
    }
  }
  
  public float WindowedY {
    get {
      return this.windowedY.GetAverageValue();
    }
  }
  
  public float WindowedZ {
    get {
      return this.windowedZ.GetAverageValue();
    }
  }
  
  public float WindowedPitch {
    get {
      return CalcPitch(WindowedX, WindowedY, WindowedZ);
    }
  }
  
  public float WindowedRoll {
    get {
      return CalcRoll(WindowedX, WindowedY, WindowedZ);
    }
  }
  
  public float Pitch {
    get {
      return CalcPitch(x, y, z);
    }
  }
  
  public float Roll {
    get {
      return CalcRoll(x, y, z);
    }
  }
  
  public static float CalcPitch(float ax, float ay, float az) {
    return Mathf.Atan2(ax, az) * Mathf.Rad2Deg;
  }
  
  public static float CalcRoll(float ax, float ay, float az) {
    return Mathf.Atan2(ay, az) * Mathf.Rad2Deg;
  }
  
  private const uint WINDOW_SIZE = 10;
  private wwAverage windowedX = new wwAverage(WINDOW_SIZE);
  private wwAverage windowedY = new wwAverage(WINDOW_SIZE);
  private wwAverage windowedZ = new wwAverage(WINDOW_SIZE);
  
  public override void tick (float dt) {
  }  
  
  public override void handleCommand(WW.SimpleJSON.JSONClass jsComponent) {
    // no-op
  }
  
  public override void handleState(WW.SimpleJSON.JSONClass jsComponent) {
    x = jsComponent["x"].AsFloat;
    y = jsComponent["y"].AsFloat;
    z = jsComponent["z"].AsFloat;
    
    windowedX.AddNewValue(x);
    windowedY.AddNewValue(y);
    windowedZ.AddNewValue(z);
  }
  
  // returns null if this component does not have a sensor aspect, or if it's not yet implemented.
  public override WW.SimpleJSON.JSONClass SensorState {
    get {
      WW.SimpleJSON.JSONClass jsState = new WW.SimpleJSON.JSONClass();
      jsState[piJSONTokens.X].AsFloat = x;
      jsState[piJSONTokens.Y].AsFloat = y;
      jsState[piJSONTokens.Z].AsFloat = z;
      return jsState;
    }
  }
  
  
}
