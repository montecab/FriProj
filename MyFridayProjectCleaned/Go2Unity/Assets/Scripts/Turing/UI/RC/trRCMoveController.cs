using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

namespace Turing{
  public class trRCMoveController : MonoBehaviour {

    public wwGUIJoystick JoystickCtrl;
    public Transform DashBehindJoystick;
    private Transform DashBehindJoystickChild;

    public Slider SpeedSlider;
    public bool IsEnabled = true;


    private enum driveMode : int {
      JOYSTICK = 0,
      TILT     = 1,
    };

    private const string cDRIVE_MODE = "rc_drive_mode";
    public Toggle       DriveOptionJoystick;
    public Toggle       DriveOptionTilt;
    public Transform    TiltButtonReminder;
    public EventTrigger TiltButton;         // not a normal button.
    public GameObject   TiltBG;

    public delegate void MoveControlInUseDelegate(bool inUse);
    public MoveControlInUseDelegate OnMoveInUseChanged;


    private double linearVel = 0;
    private double angularVel = 0;
    private Vector2 moveCtrlVal = Vector2.zero;
    private float robotSpeed = 50.0f;
    private const double MAX_ANGULAR_SPEED = -Mathf.PI;
    private const double MAX_LINEAR_SPEED = 110;

    private Vector3 initAccelerometer = Vector3.zero;
    private int     tiltButtonReminderAccumulator = 0;
    private int     tiltButtonReminderLimit       = 25;
    private Vector3 tiltButtonReminderPrevAcc     = Vector3.zero;

    private bool isTiltBtnHeld = false;

    private piBotBo prevRobot = null;


    private piBotBo robot{
      get{
        return (piBotBo)(trCurRobotController.Instance.CurRobot);
      }
    }

    void Start(){
      DashBehindJoystickChild = DashBehindJoystick.GetChild(0);

      InvokeRepeating("slowUpdate", 0, 0.05f);

      JoystickCtrl.OnPress.AddListener(onJostickPress);
      JoystickCtrl.OnValueChangeBeforeRelease.AddListener(onJoystickChanged);
      JoystickCtrl.OnRelease.AddListener(onJoystickReleased);

      if(SpeedSlider != null){
        SpeedSlider.onValueChanged.AddListener(onSpeedChanged);   
      }
         

      EventTrigger.Entry entry = new EventTrigger.Entry();
      entry.eventID = EventTriggerType.PointerDown;
      entry.callback.AddListener(onTiltButtonDown);
      TiltButton.triggers.Add(entry);

      entry = new EventTrigger.Entry();
      entry.eventID = EventTriggerType.PointerUp;
      entry.callback.AddListener(onTiltButtonRelease);
      TiltButton.triggers.Add(entry);

      // set the drive mode toggles before registering for callbacks.
      driveMode dm = (driveMode)PlayerPrefs.GetInt(cDRIVE_MODE, (int)driveMode.JOYSTICK);
      DriveOptionJoystick.isOn = (dm == driveMode.JOYSTICK);
      DriveOptionTilt    .isOn = (dm == driveMode.TILT);
      onChangedDriveOption(false);
      DriveOptionJoystick.onValueChanged.AddListener(onChangedDriveOption);
      DriveOptionTilt    .onValueChanged.AddListener(onChangedDriveOption);
    }

    void OnDestroy() {
      PIBInterface_internal.setAllowAutoRotate(true);
    }

    bool crossedThreshhold(float thresh, float oldVal, float newVal, bool inIncreasingDirection) {
      if (inIncreasingDirection) {
        return (oldVal < thresh) && (newVal >= thresh);
      }
      else {
        return (oldVal >= thresh) && (newVal < thresh);
      }
    }


    void doSoundForSpeedChanged(float newVal) {
      float oldNorm = Mathf.InverseLerp(SpeedSlider.minValue, SpeedSlider.maxValue, robotSpeed);
      float newNorm = Mathf.InverseLerp(SpeedSlider.minValue, SpeedSlider.maxValue, newVal    );

      float threshLo = 1f / 3f;
      float threshHi = 2f / 3f;

      bool increasing = (oldNorm < newNorm);

      SoundManager.trAppSound soundToPlay = SoundManager.trAppSound.INVALID;

      if (increasing) {
        if (crossedThreshhold(threshLo, oldNorm, newNorm, increasing)) {
          soundToPlay = SoundManager.trAppSound.RC_SPEED_ACC1;
        }
        else if (crossedThreshhold(threshHi, oldNorm, newNorm, increasing)) {
          soundToPlay = SoundManager.trAppSound.RC_SPEED_ACC2;
        }
      }
      else {
        if (crossedThreshhold(threshLo, oldNorm, newNorm, increasing)) {
          soundToPlay = SoundManager.trAppSound.RC_SPEED_DEC1;
        }
        else if (crossedThreshhold(threshHi, oldNorm, newNorm, increasing)) {
          soundToPlay = SoundManager.trAppSound.RC_SPEED_DEC2;
        }
      }

      if (soundToPlay != SoundManager.trAppSound.INVALID) {
        SoundManager.soundManager.PlaySound(soundToPlay);
      }
    }

    void onSpeedChanged(float value) {
      trCurRobotController.Instance.CheckOpenChrome();
      doSoundForSpeedChanged(value);      
      robotSpeed = value;

    }   


    void onJostickPress(wwGUIJoystick ctrl){
      if(OnMoveInUseChanged != null){
        OnMoveInUseChanged(true);
      }
      SoundManager.StartOnce(SoundManager.trAppSound.RC_BODY_CONTROL_STICK);
    }

    void onJoystickChanged(wwGUIJoystick ctrl, Vector2 pos){
      trCurRobotController.Instance.CheckOpenChrome();
      moveCtrlVal = pos;     
    }

    void onJoystickReleased(wwGUIJoystick ctrl){
      moveCtrlVal = Vector2.zero;
      linearVel = 0;
      angularVel = 0;
      if(robot != null){
        robot.cmd_bodyMotionStop();
        moveIndicator();
      }
      if(OnMoveInUseChanged != null){
        OnMoveInUseChanged(false);
      }
      SoundManager.StopOnce(SoundManager.trAppSound.RC_BODY_CONTROL_STICK);
    }

    void joystickMove(){
      linearVel = moveCtrlVal.y * robotSpeed;
      angularVel = moveCtrlVal.x * MAX_ANGULAR_SPEED;
    }

    void onChangedDriveOption(bool unused) {
      JoystickCtrl  .gameObject.SetActive(DriveOptionJoystick.isOn);
      TiltButton.gameObject.SetActive(DriveOptionTilt    .isOn);
      TiltBG.SetActive(DriveOptionTilt.isOn);
      driveMode dm = DriveOptionJoystick.isOn ? driveMode.JOYSTICK : driveMode.TILT;
      PlayerPrefs.SetInt(cDRIVE_MODE, (int)dm);

      PIBInterface_internal.setAllowAutoRotate(dm != driveMode.TILT);
    }

    void onTiltButtonDown(BaseEventData data){
      trCurRobotController.Instance.CheckOpenChrome();
      isTiltBtnHeld = true;
      initAccelerometer = Input.acceleration;

      if(OnMoveInUseChanged != null){
        OnMoveInUseChanged(true);
      }
    }

    void onTiltButtonRelease(BaseEventData data){
      isTiltBtnHeld = false;
      linearVel = 0;
      angularVel = 0;
      if(robot != null){
        robot.cmd_bodyMotionStop();
        moveIndicator();
      }

      if(OnMoveInUseChanged != null){
        OnMoveInUseChanged(false);
      }
    }

    void showTiltButtonReminder() {
      TiltButtonReminder.DOPunchScale(Vector3.one * 1.2f, 1f, 0);
      SoundManager.soundManager.PlaySound(SoundManager.trAppSound.RC_TILT_ALERT);
    }


    private float prevDegFwd = 0;
    // converts the raw acceleration into calibrated angles.
    // assumptions:
    // the device is basically being held with the long-axis horizontal, screen facing the user.
    // rotation about the long axis away is interpreted as linear velocity
    // rotation about the axis from the user to the screen is interpreted as angular velocity.
    // Y = -1 indicates screen is vertical, facing the user.
    // Z = -1 indicates screen is facing the sky.
    void processTilt(Vector3 rawAcc, Vector3 initialAcc, out float degFwd, out float degTrn) {
      rawAcc    .Normalize();
      initialAcc.Normalize();

      // calculate amount rotated around long axis, relative to starting point
      if (!isTiltBtnHeld) {
        prevDegFwd = 0f;
      }

      float a1 = Mathf.Atan2(initialAcc.y, initialAcc.z);
      float a2 = Mathf.Atan2(rawAcc    .y, rawAcc    .z);
      float tmpDegFwd = (a2 - a1) * Mathf.Rad2Deg;
      float delt = piMathUtil.shortestDeltaDegrees(prevDegFwd, tmpDegFwd);
      degFwd = prevDegFwd + delt;

      // calculate amount rotated around turn axis. this we don't bother calibrating."
      // this still needs work - it starts decreasing past +/-90º.
      degTrn = Mathf.Lerp(-90, 90f, (rawAcc.x * 0.5f) + 0.5f);

      // attenuate forward the more we're turning.
      // ie, when the phone is turnd 90º, there's no sense in forward/back.
      float f = Mathf.Min(1f, Mathf.Abs(rawAcc.x));
      f = f * f;
      degFwd = Mathf.Lerp(degFwd, 0, f);
      while (degFwd > 180){
        degFwd -= 360;
      }
      while (degFwd < -180){
        degFwd += 360;
      }

      prevDegFwd = degFwd;
    }

    void tiltMove() {
      float degFwd = 0;
      float degTrn = 0;

      processTilt(Input.acceleration, initAccelerometer, out degFwd, out degTrn);

      double lv = 0;
      double av = 0;

      if(!isTiltBtnHeld) {
        TiltButtonReminder.gameObject.SetActive(true);
        if ((Input.acceleration - tiltButtonReminderPrevAcc).magnitude > 0.2f) {
          tiltButtonReminderPrevAcc = Input.acceleration;
          tiltButtonReminderAccumulator += 1;
          if (tiltButtonReminderAccumulator > tiltButtonReminderLimit) {
            showTiltButtonReminder();
            tiltButtonReminderAccumulator = 0;
          }
        }
      }
      else {

        TiltButtonReminder.gameObject.SetActive(false);
        //optimize for different size of devices
        //      float heightInInches = Screen.height / Screen.dpi;
        //      float multiplier = Mathf.Max(heightInInches /5.0f, 1.0f);
        //      multiplier = Mathf.Clamp(multiplier, -1, 1);
        //      oxe: i commented out the screen adjuster because it seems weird. why is it clamped at -1, 1 ?


        float multiplierLin = (1.5f / 180f);   // fudge factor!
        float multiplierAng = (1.1f /  90f);         // fudge factor!

        float fwdWithZeroPlateau = piMathUtil.createFlat(degFwd, 0, 5f, -90f, 90f);
        float trnWithZeroPlateau = piMathUtil.createFlat(degTrn, 0, 5f, -90f, 90f);

        lv = -fwdWithZeroPlateau * MAX_LINEAR_SPEED  * multiplierLin * (robotSpeed / 50f);
        av =  trnWithZeroPlateau * MAX_ANGULAR_SPEED * multiplierAng;
      }

      // use an IIR filter to smooth out the motion
      double iirFac = 0.75f;
      linearVel  = (linearVel  * iirFac) + (lv * (1f - iirFac));
      angularVel = (angularVel * iirFac) + (av * (1f - iirFac));
    }

    void handleMovement(bool force) {
      if (DriveOptionJoystick.isOn) {
        joystickMove();

      }
      else{
        tiltMove();
      }

      moveIndicator();

      if (robot != null && IsEnabled) {
        if(force || (linearVel != robot.prevLinear) || (angularVel != robot.prevLinear)){
          robot.cmd_bodyMotion(linearVel, angularVel);  // todo: leisen: we should use the accelerated version here.
        }
      }
    }

    void moveIndicator(){
      DashBehindJoystick     .localEulerAngles = new Vector3(0, 0, (float)angularVel * 20f);
      DashBehindJoystickChild.localPosition    = new Vector3(0, (float)linearVel / robotSpeed * 40f, 0);
    }

    void slowUpdate(){
     
      if(!this.gameObject.activeSelf){
        return;
      }
      bool force = (prevRobot != robot);

      handleMovement(force);
      prevRobot = robot;
    }    
  }

}
