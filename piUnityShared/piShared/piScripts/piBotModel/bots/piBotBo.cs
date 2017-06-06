using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using WW.SimpleJSON;

using PI;

public class piBotBo : piBotAxled {
  
  public piBotBo(string inUUID, string inName, piRobotType inRobotType, JSONClass jsonRobot=null) : base(inUUID, inName, inRobotType, jsonRobot) {}
  
  public enum HeadCalibrationState_t {
    UNKNOWN,       // we have not yet determined the calibration state for this robot.
    CALIB_3,       // this bot has only 3-point calibration. needs 7 !
    CALIB_3_BEGUN, // we started the calibration process on this guy. user may have cancelled.
    CALIB_7,       // this bot has 7-point calibration: good to go.
  }
  
  public HeadCalibrationState_t HeadCalibrationState = HeadCalibrationState_t.UNKNOWN;
  
  // convenience accessors.
  public piBotComponentMotorServo         HeadPan                  { get{ return (piBotComponentMotorServo        )(components[ComponentID.WW_COMMAND_MOTOR_HEAD_PAN             ]); }}
  public piBotComponentMotorServo         HeadTilt                 { get{ return (piBotComponentMotorServo        )(components[ComponentID.WW_COMMAND_MOTOR_HEAD_TILT            ]); }}
  public piBotComponentLightRGB           RGBChest                 { get{ return (piBotComponentLightRGB          )(components[ComponentID.WW_COMMAND_RGB_CHEST                  ]); }}
  public piBotComponentLED                LEDTail                  { get{ return (piBotComponentLED               )(components[ComponentID.WW_COMMAND_LED_TAIL                   ]); }}
  public piBotComponentDistanceSensorPair DistanceSensorFrontLeft  { get{ return (piBotComponentDistanceSensorPair)(components[ComponentID.WW_SENSOR_DISTANCE_FRONT_LEFT_FACING  ]); }}
  public piBotComponentDistanceSensorPair DistanceSensorFrontRight { get{ return (piBotComponentDistanceSensorPair)(components[ComponentID.WW_SENSOR_DISTANCE_FRONT_RIGHT_FACING ]); }}
  public piBotComponentDistanceSensor     DistanceSensorTail       { get{ return (piBotComponentDistanceSensor    )(components[ComponentID.WW_SENSOR_DISTANCE_BACK               ]); }}
  public piBotComponentGyroscope          GyroscopeSensor          { get{ return (piBotComponentGyroscope         )(components[ComponentID.WW_SENSOR_MOTION_GYROSCOPE            ]); }}
  public piBotComponentMotorServo         HeadPanSensor            { get{ return (piBotComponentMotorServo        )(components[ComponentID.WW_SENSOR_MOTOR_HEAD_PAN              ]); }}
  public piBotComponentMotorServo         HeadTiltSensor           { get{ return (piBotComponentMotorServo        )(components[ComponentID.WW_SENSOR_MOTOR_HEAD_TILT             ]); }}
  public piBotComponentBodyPose           BodyPoseSensor           { get{ return (piBotComponentBodyPose          )(components[ComponentID.WW_SENSOR_MOTION_BODY_POSE            ]); }}
  public piBotComponentFlag               KidnapSensor             { get{ return (piBotComponentFlag              )(components[ComponentID.WW_SENSOR_KIDNAP                      ]); }}
  public piBotComponentFlag               StallBumpSensor          { get{ return (piBotComponentFlag              )(components[ComponentID.WW_SENSOR_STALLBUMP                   ]); }}
  public piBotComponentBeacon             Beacon                   { get{ return (piBotComponentBeacon            )(components[ComponentID.WW_SENSOR_BEACON                      ]); }}
  public piBotComponentBeaconV2           BeaconV2                 { get{ return (piBotComponentBeaconV2          )(components[ComponentID.WW_SENSOR_BEACON_V2                    ]); }}
  
  protected override void setupComponents() {
    base.setupComponents();
    
    // effectors
    addComponent<piBotComponentMotorServo        >(PI.ComponentID.WW_COMMAND_MOTOR_HEAD_PAN            );
    addComponent<piBotComponentMotorServo        >(PI.ComponentID.WW_COMMAND_MOTOR_HEAD_TILT           );
    addComponent<piBotComponentLightRGB          >(PI.ComponentID.WW_COMMAND_RGB_CHEST                 );
    addComponent<piBotComponentLED               >(PI.ComponentID.WW_COMMAND_LED_TAIL                  );
    
    // sensors
    addComponent<piBotComponentDistanceSensorPair>(PI.ComponentID.WW_SENSOR_DISTANCE_FRONT_LEFT_FACING );
    addComponent<piBotComponentDistanceSensorPair>(PI.ComponentID.WW_SENSOR_DISTANCE_FRONT_RIGHT_FACING);
    addComponent<piBotComponentDistanceSensor    >(PI.ComponentID.WW_SENSOR_DISTANCE_BACK              );
    addComponent<piBotComponentGyroscope         >(PI.ComponentID.WW_SENSOR_MOTION_GYROSCOPE           );
    addComponent<piBotComponentMotorServo        >(PI.ComponentID.WW_SENSOR_MOTOR_HEAD_PAN             );
    addComponent<piBotComponentMotorServo        >(PI.ComponentID.WW_SENSOR_MOTOR_HEAD_TILT            );
    addComponent<piBotComponentBodyPose          >(PI.ComponentID.WW_SENSOR_MOTION_BODY_POSE           );
    addComponent<piBotComponentFlag              >(PI.ComponentID.WW_SENSOR_KIDNAP                     );
    addComponent<piBotComponentFlag              >(PI.ComponentID.WW_SENSOR_STALLBUMP                  );
    addComponent<piBotComponentBeacon            >(PI.ComponentID.WW_SENSOR_BEACON                     );
    addComponent<piBotComponentBeaconV2          >(PI.ComponentID.WW_SENSOR_BEACON_V2                  );
    
    // setup some physical parameters.
    // todo - add bounds checking.
    // todo - this should come from a data file instead of code, or have some other mechanism for live tuning.
    axleLength = PI.piBotConstants.axleLength;
    
    // servos. maxRateOfChange is velocity.
    HeadPan .angle.maxRateOfChange = 300.0f;
    HeadTilt.angle.maxRateOfChange = 300.0f;
  }
  
  public override void Reset () {
    base.Reset ();
    cmd_bodyMotionStop();
    cmd_headMove(0, 0);
  }
  
  // SENSORS
  
  private void insertNullBeaconIfNotPresent(WW.SimpleJSON.JSONClass jsComponent) {
    // if nothing is seen in the beacon, the sensor is not included in the incoming robot state.
    // this means we won't process it, so we won't clear out our beacon component.
    // this code detects the absence of a beacon component, and injects a null one.
    
    bool seenBeacon = false;
    foreach(string key in jsComponent.Keys) {
      int componentId = int.Parse(key);
      if (componentId == (int)(PI.ComponentID.WW_SENSOR_BEACON)) {
        seenBeacon = true;
      }
    }
    
    if (!seenBeacon) {
      // insert null beacon
      WW.SimpleJSON.JSONClass jsc = new piBotComponentBeacon().SensorState;
      jsComponent.Add(((int)PI.ComponentID.WW_SENSOR_BEACON).ToString(), jsc);
    }
  }
  
  public override void handleState(WW.SimpleJSON.JSONClass jsComponent) {
    insertNullBeaconIfNotPresent(jsComponent);
    base.handleState(jsComponent);
  }
  
  public piBotComponentDistanceSensorPair DistanceSensorFrontCenter {
    get {
      piBotComponentDistanceSensorPair ret = new piBotComponentDistanceSensorPair();
      float val = (DistanceSensorFrontLeft.distance + DistanceSensorFrontRight.distance) / 2f;
      ret.setDistance(val);
      ret.WindowedDistance = val;
      return ret;
    }
  }
  
  
  // BOT COMMANDS
  public void cmd_headTilt (double angle) {
    logVerbose("tilt angle:" + angle);
    if (this.apiInterface != null) {
      apiInterface.headTilt(this.UUID, angle);
    }
  }
  
  public void cmd_headPan (double angle) {
    logVerbose("pan angle:" + angle);
    if (this.apiInterface != null) {
      apiInterface.headPan(this.UUID, angle);
    }
  }
  
  public void cmd_headPan (double angle, double time) {
    logVerbose("angle:" + angle + " time:" + time);
    if (this.apiInterface != null) {
      apiInterface.headPanWithTime(this.UUID, angle, time);
    }
  }
  
  public void cmd_headTilt (double angle, double time) {
    logVerbose("angle:" + angle + " time:" + time);
    if (this.apiInterface != null) {
      apiInterface.headTiltWithTime(this.UUID, angle, time);
    }
  }
  
  public void cmd_headMove (double panAngle, double tiltAngle) {
    logVerbose("panAngle:" + panAngle + " tiltAngle:" + tiltAngle);
    if (this.apiInterface != null) {
      apiInterface.headMove(this.UUID, panAngle, tiltAngle);
    }
  }
  
  public void cmd_LEDTail(double brightness) {
    logVerbose("brightness:" + brightness);
    if (apiInterface != null) {
      apiInterface.ledTail(UUID, brightness);
    }
  }
  
  public void cmd_headBang() {
    logVerbose("");
    if (apiInterface != null) {
      apiInterface.headBang(UUID);
    }
  }
  
  public void cmd_launcher_fling(float strength) {
    logVerbose("fling with power: " + strength);
    if (apiInterface != null) {
      apiInterface.launcherFling(UUID, strength);
    }
  }
  
  public void cmd_launcher_reload_left(){
    logVerbose("launcher reload left");
    if (apiInterface != null) {
      apiInterface.launcherReloadLeft(UUID);
    }
  }
  
  public void cmd_launcher_reload_right(){
    logVerbose("launcher reload right");
    if (apiInterface != null) {
      apiInterface.launcherReloadRight(UUID);
    }
  }

  public override void stage_LEDColorAll(Color c) {
    stage_LEDColors(c, new ComponentID[]{
      ComponentID.WW_COMMAND_RGB_LEFT_EAR,
      ComponentID.WW_COMMAND_RGB_RIGHT_EAR,
      ComponentID.WW_COMMAND_RGB_CHEST,
    });
  }

  public void stage_LEDColorCenter(Color c) {
    stage_LEDColors(c, new ComponentID[]{
      ComponentID.WW_COMMAND_RGB_CHEST,
    });
  }

}









