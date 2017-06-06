using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Collections.Generic;

using PI;

public class piBotComponentMotorWheel : piBotComponentBase {

  public piInertialValue velocity = new piInertialValue();
  public piTaredValue encoderDistance = new piTaredValue();

  private float prevTime = float.NaN;
  private float speed = 0;
  private float speedFiltered = 0;
  private float iirFactor = 0.9f;
  public static float speedFilteredMax = 1.8f;  // cm/s

  public float Speed {
    get {
      return speed;
    }
  }
  
  public float SpeedFiltered {
    get {
      return speedFiltered;
    }
  }

  public override void tick (float dt) {
    velocity.tick(dt);
  }

  public override void handleCommand(WW.SimpleJSON.JSONClass jsComponent) {
    velocity.valTarget = jsComponent[piJSONTokens.VELOCITY].AsFloat;
  }

  public override void handleState(WW.SimpleJSON.JSONClass jsComponent) {
    float prevDistance = encoderDistance.ValueRaw;
    encoderDistance.ValueRaw = jsComponent["encoderDistance"].AsFloat;
    float time = Time.time;

    if (!float.IsNaN(prevTime)) {
      float distance = encoderDistance.ValueRaw - prevDistance;
      float timeDiff = time - prevTime;

      if (timeDiff != 0) {
        speed = distance / timeDiff;
        speedFiltered = (speedFiltered * iirFactor) + (speed * (1.0f - iirFactor));
        speedFiltered = Mathf.Clamp(speedFiltered, -speedFilteredMax, speedFilteredMax);
      }
    }

    prevTime = time;
  }

  public void tare() {
    encoderDistance.tare();
  }

  // returns null if this component does not have a sensor aspect, or if it's not yet implemented.
  public override WW.SimpleJSON.JSONClass SensorState {
    // TODO - implement this sensor.
    get {
      return null;
    }
  }
}
