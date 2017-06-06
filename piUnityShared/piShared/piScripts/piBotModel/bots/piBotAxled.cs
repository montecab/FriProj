using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using WW.SimpleJSON;

using PI;

public abstract class piBotAxled : piBotCommon {

  public piBotAxled(string inUUID, string inName, piRobotType inRobotType, JSONClass jsonRobot) : base(inUUID, inName, inRobotType, jsonRobot) {}

  // "x" is forward.
  // "y" is right.
  // "z" is up.

  private Vector3 axlePosition;
  private float	 axleOrientation;
  public  float  axleLength  = PI.piBotConstants.axleLength;
  public  double prevLinear  = 0.0f;
  public  double prevAngular = 0.0f;
  public  double prevWheelL  = 0.0f;
  public  double prevWheelR  = 0.0f;

  private float absSpeedFiltered = 0.0f;
  private float absSpeedIIRFac   = 0.9f;
  private float absSpeedIIRMax   = 1.0f;
  
  private float totalTravelLinear = 0.0f;
  private float totalTravelAngular = 0.0f;

  // need to figure out real parameters
  public float   wheelAttenuation = 0.4f;

  // the maximum number of seconds we'll allow to pass per axle calculation tick.
  protected float maxSecondsPerTick = 0.001f * 20.0f;	// 20ms.

  public Vector3 AxlePosition {
    get {
      return axlePosition;
    }
  }

  public float LinearSpeed {
    get {
      return (WheelLeft.Speed + WheelRight.Speed) / 2.0f;
    }
  }

  public float AngularSpeed {
    get {
      return (WheelRight.Speed - WheelLeft.Speed) / axleLength;
    }
  }
  
  public float LinearSpeedFiltered {
    get {
      return (WheelLeft.SpeedFiltered + WheelRight.SpeedFiltered) / 2.0f;
    }
  }
  
  public float AbsSpeedFiltered {
    get {
      return absSpeedFiltered;
    }
  }

  public float TotalTravelLinear {
    get {
      return this.totalTravelLinear;
    }
  }

  public float TotalTravelAngular {
    get {
      return this.totalTravelAngular;
    }
  }

  public void setAxlePosition(Vector2 value) {
    axlePosition = value;
  }

  public float AxleOrientation {
    get {
      return axleOrientation;
    }
  }
  public void setAxleOrientation(float value) {
    axleOrientation = value;
  }

  // convenience accessors.
  public piBotComponentMotorWheel WheelLeft  { get { return (piBotComponentMotorWheel)(components[ComponentID.WW_SENSOR_DISTANCE_ENCODER_LEFT_WHEEL ]); }}
  public piBotComponentMotorWheel WheelRight { get { return (piBotComponentMotorWheel)(components[ComponentID.WW_SENSOR_DISTANCE_ENCODER_RIGHT_WHEEL]); }}

  protected override void setupComponents() {
    base.setupComponents();
    // effectors
    addComponent<piBotComponentMotorWheel>(PI.ComponentID.WW_SENSOR_DISTANCE_ENCODER_LEFT_WHEEL );
    addComponent<piBotComponentMotorWheel>(PI.ComponentID.WW_SENSOR_DISTANCE_ENCODER_RIGHT_WHEEL);
    // setup some physical parameters.
    // todo - add bounds checking.
    // todo - this should come from a data file instead of code, or have some other mechanism for live tuning.
    // wheels.velocity.maxRateOfChange is centimeters per second per second. THIS IS ACCELERATION, NOT VELOCITY.
    WheelLeft .velocity.maxRateOfChange = 100.0f;
    WheelRight.velocity.maxRateOfChange = 100.0f;
  }

  // vector from the center of the axle to the right wheel, in world coordinates.
  Vector3 RightWheelVector {
    get {
      return Quaternion.Euler(0, 0, axleOrientation) * new Vector3(0, axleLength * 0.5f, 0);
    }
  }

  // unit vector from the center of the axle "forward".
  Vector3 ForwardVector {
    get {
      return Quaternion.Euler(0, 0, axleOrientation) * new Vector3(1, 0, 0);
    }
  }

  public override void handleState (WW.SimpleJSON.JSONClass jsComponent) {
    base.handleState (jsComponent);

    float dWheelL = WheelLeft.encoderDistance.Value - (float)prevWheelL;
    float dWheelR = WheelRight.encoderDistance.Value - (float)prevWheelR;

    float dTravelLinear;
    float dTravelAngular;

    piMathUtil.wheelSpeedsToLinearAngular(dWheelL, dWheelR, out dTravelLinear, out dTravelAngular, axleLength);

    totalTravelLinear += dTravelLinear;
    totalTravelAngular += dTravelAngular;
    
    float absSpeed = Mathf.Abs(WheelLeft.Speed) + Mathf.Abs(WheelRight.Speed);
    absSpeedFiltered = (absSpeedFiltered * absSpeedIIRFac) + (absSpeed * (1.0f - absSpeedIIRFac));
    absSpeedFiltered = Mathf.Min(absSpeedFiltered, absSpeedIIRMax);
  }

  public override void tick (float dt) {
    // how many iterations of the core loop to do this tick:
    int numIters = Mathf.CeilToInt(dt / maxSecondsPerTick);
    // ensure at least one iteration:
    numIters = numIters > 0 ? numIters : 1;
    // seconds per iteration
    float dtPerIter = dt / numIters;

    /*
      using equation 6 from here:
      http://rossum.sourceforge.net/papers/DiffSteer :
      distAvg = (distR + distL) / 2.
      dTheta  = (distR - distL) / axleLength.
      theta   = dTheta + theta0.
      dX      = distAvg * cos(theta).
      dY      = distAvg * sin(theta).
      x       = dX + x0.
      y       = dY + y0.
    */

    for (int n = 0; n < numIters; ++n) {
      // tick the base class to update wheel velocities.
      base.tick(dtPerIter);
      float distL = WheelLeft .velocity.ValCurrent * dtPerIter * wheelAttenuation;
      float distR = WheelRight.velocity.ValCurrent * dtPerIter * wheelAttenuation;
      float distAvg = (distL + distR) * 0.5f;
      float dTheta  = (distL - distR) / axleLength;
      float theta   = dTheta + (axleOrientation * Mathf.Deg2Rad);
      float dX      = distAvg * Mathf.Cos(theta);
      float dY      = distAvg * Mathf.Sin(theta);
      float x       = dX + axlePosition.x;
      float y       = dY + axlePosition.y;
      axlePosition.x  = x;
      axlePosition.y  = y;
      axleOrientation = theta * Mathf.Rad2Deg;
    }
  }

  public void tareWheels() {
    WheelLeft .tare();
    WheelRight.tare();
    totalTravelLinear = 0.0f;
    totalTravelAngular = 0.0f;
  }


  // BOT COMMANDS
  public void cmd_move(double leftWheelVelocity, double rightWheelVelocity) {
    logVerbose("leftWheelVelocity:" + leftWheelVelocity + " rightWheelVelocity:" + rightWheelVelocity);
    prevLinear  = 0.0f;
    prevAngular = 0.0f;
    prevWheelL  = leftWheelVelocity;
    prevWheelR  = rightWheelVelocity;

    if (this.apiInterface != null) {
      apiInterface.move(this.UUID, leftWheelVelocity, rightWheelVelocity);
    }
  }

  public void cmd_bodyMotion(double linearVelocity, double angularVelocity) {
    cmd_bodyMotion(linearVelocity, angularVelocity, false);
  }
  
  public void cmd_bodyMotion(double linearVelocity, double angularVelocity, bool usePose) {
    logVerbose("linearVelocity:" + linearVelocity + " angularVelocity:" + angularVelocity + " usePose:" + usePose.ToString());
    prevLinear  = linearVelocity;
    prevAngular = angularVelocity;
    prevWheelL  = 0;
    prevWheelR  = 0;
    
    if (this.apiInterface != null) {
      apiInterface.bodyMotion(this.UUID, linearVelocity, angularVelocity, usePose);
    }
  }
    
  public void cmd_bodyMotionWithAcceleration(double linearVelocity, double angularVelocity, double linearAccMagnitude, double angularAccMagnitude){
    logVerbose(string.Format("linearVelocity: {0}, angularVelocity: {1}, linearAccMagnitude: {2}, angularAccMagnitude: {3}", linearVelocity, angularVelocity, linearAccMagnitude, angularAccMagnitude));
    prevLinear  = linearVelocity;
    prevAngular = angularVelocity;
    prevWheelL  = 0;
    prevWheelR  = 0;

    if (this.apiInterface != null){
      apiInterface.bodyMotionWithAcceleration(this.UUID, linearVelocity, angularVelocity, linearAccMagnitude, angularAccMagnitude);
    }
  }

  public void cmd_bodyMotionStop(){
    cmd_bodyMotionWithAcceleration(0, 0, 100, 10);
  }

  static byte[] coastCommand = null;
  public void cmd_bodyMotionCoast(){
    if (coastCommand == null) {
      coastCommand = new byte[1];
      coastCommand[0] = 0x27;
    }

    cmd_sendRawData(coastCommand);  
  }

  public void cmd_poseGlobal(double x, double y, double theta, double time) {
    logVerbose("x:" + x + " y:" + y + " theta:" + theta + " time:" + time);
    prevLinear  = 0.0f;
    prevAngular = 0.0f;
    prevWheelL  = 0;
    prevWheelR  = 0;

    if (this.apiInterface != null) {
      apiInterface.poseGlobal(this.UUID, x, y, theta, time);
    }
  }

  /// <summary>
  /// Set origin for the pose coordinate
  /// </summary>
  /// <param name="x">The x coordinate.</param>
  /// <param name="y">The y coordinate.</param>
  /// <param name="theta">Theta.</param>
  /// <param name="time">Time.</param>
  public void cmd_poseSetGlobal(double x, double y, double theta, double time) {
    logVerbose("x:" + x + " y:" + y + " theta:" + theta + " time:" + time);

    if (this.apiInterface != null) {
      apiInterface.poseSetGlobal(this.UUID, x, y, theta, time);
    }
  }

  /// <summary>
  /// send pose command assuming the previous command is fully executed.
  /// </summary>
  /// <param name="x">The x coordinate.</param>
  /// <param name="y">The y coordinate.</param>
  /// <param name="theta">Theta.</param>
  /// <param name="time">Time.</param>
  public void cmd_poseRelative(double x, double y, double theta, double time) {
    logVerbose("x:" + x + " y:" + y + " theta:" + theta + " time:" + time);

    if (this.apiInterface != null) {
      apiInterface.poseRelative(this.UUID, x, y, theta, time);
    }
  }

  /// <summary>
  /// send pose command with setttings
  /// </summary>
  /// <param name="x">Centimeters forward.</param>
  /// <param name="y">Centimeters to the left.</param>
  /// <param name="theta">Radians counter-clockwise.</param>
  /// <param name="time">Seconds.</param>
  /// <param name="mode">set origin or relative</param>
  /// <param name="direction">if the robot will move backwards or not</param>
  /// <param name="wrapTheta">if the robot will wrap theta or not</param>
  public void cmd_poseParam(double x, double y, double theta, double time, PI.WWPoseMode mode, PI.WWPoseDirection direction, PI.WWPoseWrap wrapTheta) {
    logVerbose("x:" + x + " y:" + y + " theta:" + theta + " time:" + time + " mode: " + mode.ToString() + " direction: " + direction.ToString() + " wrapTheta: " + wrapTheta.ToString());
    prevLinear  = 0;
    prevAngular = 0;
    prevWheelL  = 0;
    prevWheelR  = 0;

    if (this.apiInterface != null) {
      apiInterface.poseParam(this.UUID, x, y, theta, time, mode, direction, wrapTheta);
    }
  }

  public void stage_linearAngular(float velLin, float velAngRad) {
    WW.SimpleJSON.JSONClass jsc = new WW.SimpleJSON.JSONClass();
    jsc[piJSONTokens.WW_COMMAND_VALUE_SPEED_LINEAR     ].AsFloat = velLin;
    jsc[piJSONTokens.WW_COMMAND_VALUE_SPEED_ANGULAR_DEG].AsFloat = velAngRad * Mathf.Rad2Deg;
    stage_Command(ComponentID.WW_COMMAND_MOTION_BODY_LINEAR_ANGULAR, jsc);
  }
}









