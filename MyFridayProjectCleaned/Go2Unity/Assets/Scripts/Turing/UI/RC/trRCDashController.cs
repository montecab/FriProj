using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

namespace Turing{
  public class trRCDashController : MonoBehaviour {
    public Button RecordButton;
    
    public Button ModeSelectDrive;
    public Button ModeSelectLauncher;

    public wwGUIJoystick HeadCtrl;
    public Transform DashHeadBehindJoystick;

    public Slider RGBSlider;
    public Image ColorIndicator;

   
    private float rgbVal = 1;
    private float preRgbVal = 1;
    
 
    private Vector2 headCtrlVal = Vector2.zero;
    private Vector2 preHeadCtrlVal = Vector2.zero;
  
    private piBotBo prevRobot = null;

    
    private piBotBo robot{
      get{
        return (piBotBo)(trCurRobotController.Instance.CurRobot);
      }
    }
    
    void Start(){
    
      InvokeRepeating("slowUpdate", 0, 0.05f);
      HeadCtrl.OnValueChange.AddListener(onHeadChanged);

      RGBSlider.onValueChanged.AddListener(onRGBChanged);
      EventTrigger et = RGBSlider.GetComponent<EventTrigger>();
      EventTrigger.Entry ete;
      ete = new EventTrigger.Entry();
      ete.eventID = EventTriggerType.PointerDown;
      ete.callback.AddListener(onPointerDownRGB);
      et.triggers.Add(ete);
      ete = new EventTrigger.Entry();
      ete.eventID = EventTriggerType.PointerUp;
      ete.callback.AddListener(onPointerUpRGB);
      et.triggers.Add(ete);
      
      HeadCtrl.OnDragStart.AddListener(onJoystickDownHead);
      HeadCtrl.OnRelease  .AddListener(onJoystickUpHead);
      
      RecordButton.onClick.AddListener(onRecordButtonClicked);
      
      ModeSelectDrive   .onClick.AddListener(onClickModeDrive);
      ModeSelectLauncher.onClick.AddListener(onClickModeLauncher);

    }
    
    void onRecordButtonClicked(){
      trCurRobotController.Instance.CheckOpenChrome();
      if(robot != null){
        PIBInterface_internal.onClickSoundRecordButton();
      }
      SoundManager.StartOnce(SoundManager.trAppSound.USER_SOUNDS_MIC);
    }

    
    void setEyeAnimation(string name, bool checkChrome) {
      if (checkChrome) {
        trCurRobotController.Instance.CheckOpenChrome();
      }
      
      if (robot != null) {
        robot.cmd_eyeRing(1.0, name, piMathUtil.deserializeBoolArray(0));
      }
    }

    void onEyeRingButtonFullClicked(){
      setEyeAnimation("full_blink", true);
    }
    
    void onEyeRingButtonSmileClicked(){
      setEyeAnimation("wink", true);
    }
    
    void onEyeRingButtonOffClicked(){
      trCurRobotController.Instance.CheckOpenChrome();
      eyeRingButton(0);
    }
    
    void onEyeRingButtonFrownClicked(){
      setEyeAnimation("swirl", true);
    }
    
    void eyeRingButton(int val){
      if(robot != null){
        bool[] bitmap = piMathUtil.deserializeBoolArray(val);
        robot.cmd_eyeRing(1.0f, "", bitmap);
      }
    }
    
    void onHeadChanged(wwGUIJoystick ctrl, Vector2 pos){
      trCurRobotController.Instance.CheckOpenChrome();
      headCtrlVal = pos;
    }

    void handleHeadMovement(bool force){
      if(force || (headCtrlVal != preHeadCtrlVal)){
        float pan = headCtrlVal.x * -120.0f;
        float tilt = headCtrlVal.y;
        if(tilt >0){
          tilt *= -20;
        }
        else{
          tilt *= -10;
        }
        
        DashHeadBehindJoystick.localEulerAngles = new Vector3(0, 0, pan);
        
        
        if (robot != null) {
          robot.cmd_headMove(pan, tilt);
        }
        
        preHeadCtrlVal = headCtrlVal;
      }
    }

    void handleLight(bool force){   
      if(force || (rgbVal != preRgbVal)){
        Color color = wwColorUtil.HSVtoRGB(rgbVal, 1, 1);
        
        if (robot != null) {
          robot.cmd_rgbLights(color.r, color.g, color.b);
        }
        
        preRgbVal = rgbVal;
      }
    }

    void onRGBChanged(float v){
      trCurRobotController.Instance.CheckOpenChrome();
      rgbVal = v;
      ColorIndicator.color =   wwColorUtil.HSVtoRGB(rgbVal, 1, 1);
    }
    
    void onPointerDownRGB(BaseEventData unused) {
      SoundManager.StartOnce(SoundManager.trAppSound.RC_COLOR_SLIDER_LOOP);
    }
    
    void onPointerUpRGB(BaseEventData unused) {
      SoundManager.StopOnce(SoundManager.trAppSound.RC_COLOR_SLIDER_LOOP);
    }
    
    void onJoystickDownHead(wwGUIJoystick unused) {
      SoundManager.StartOnce(SoundManager.trAppSound.RC_HEAD_PANTILT);
    }
    
    void onJoystickUpHead(wwGUIJoystick unused) {
      SoundManager.StopOnce(SoundManager.trAppSound.RC_HEAD_PANTILT);
    }
    
    // super hacky. should do this as part of tab control.
    private int _lastMode = 0;
    void onClickModeDrive() {
      if (_lastMode != 0) {
        _lastMode = 0;
        SoundManager.soundManager.PlaySound(SoundManager.trAppSound.RC_DASH_MODE_DRIVE);
      }
    }
    
    void onClickModeLauncher() {
      if (_lastMode != 1) {
        _lastMode = 1;
        SoundManager.soundManager.PlaySound(SoundManager.trAppSound.RC_DASH_MODE_LAUNCHER);
      }
    }

   
    void slowUpdate(){
      if(!this.gameObject.activeSelf){
        return;
      }
      bool force = (prevRobot != robot);

      handleHeadMovement(force);
      handleLight(force);
      
      if (force) {
        setEyeAnimation("full_blink", false);
      }
      
      prevRobot = robot;
    }    
  }
}

