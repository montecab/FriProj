using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Turing{
  public class trRCDotController : MonoBehaviour {
       
    public Button RecordButton;
    
    public Slider LeftEarRGBSlider;
    public Slider RightEarRGBSlider;
    public Image LeftColorIndicator;
    public Image RightColorIndicator;

    public wwGUIJoystick EyeColorWheelCtrl;
    public Image EyeColorIndicator;
    
    private float leftRgbVal = 1;
    private float preLeftRgbVal = 1;
    
    private float rightRgbVal = 1;
    private float preRightRgbVal = 1;  

    private Vector2 eyeRgbVal = Vector2.zero;
    private Vector2 preEyeRgbVal = Vector2.zero;
    private float brightnessVal = 1;
    private float preBrightnessVal = 1;    
    
    private piBotCommon prevRobot = null;
    
    //    public Button EyeRingFullButton;
//    public Button EyeRingOffButton;
//    public Button EyeRingSmileButton;
//    public Button EyeRingFrownButton;  
    
    
    public enum PatternType{
      ON = 0, 
      OFF = 1,
      BINARY = 2,
      SINE = 3
    }
    public Button NextButton;
    public Button PreviousButton;
    
    private GameObject curPatternImage;
    public GameObject OnSprite;
    public GameObject OffSprite;
    public GameObject BinarySprite;
    public GameObject SineSprite;
    
    public GameObject PatternPanel;
    
    private PatternType curPattern = PatternType.ON;
    private int frameCount = 0;
    
    private piBotCommon robot{
      get{
        return (piBotCommon)(trCurRobotController.Instance.CurRobot);
      }
    }
    
    // Use this for initialization
    void Start () {
      InvokeRepeating("slowUpdate", 0, 0.05f);
      NextButton.onClick.AddListener(onNextButtonClicked);
      PreviousButton.onClick.AddListener(onPreviousButtonClicked);
      setView();
      LeftEarRGBSlider.onValueChanged.AddListener(onLeftEarRGBChanged);
      RightEarRGBSlider.onValueChanged.AddListener(onRightEarRGBChanged);
      
//      EyeRingFullButton.onClick.AddListener(onEyeRingButtonFullClicked);
//      EyeRingFrownButton.onClick.AddListener(onEyeRingButtonFrownClicked);
//      EyeRingOffButton.onClick.AddListener(onEyeRingButtonOffClicked);
//      EyeRingSmileButton.onClick.AddListener(onEyeRingButtonSmileClicked);
      
      EyeColorWheelCtrl.OnValueChange.AddListener(onEyeColorChanged);
      RecordButton.onClick.AddListener(onRecordButtonClicked);
      
      
      
      // sound callback setup. so laborious.
      EventTrigger et;
      EventTrigger.Entry ete;
      
      et = LeftEarRGBSlider.GetComponent<EventTrigger>();
      ete = new EventTrigger.Entry();
      ete.eventID = EventTriggerType.PointerDown;
      ete.callback.AddListener(onPointerDownAnyRGBSlider);
      et.triggers.Add(ete);
      ete = new EventTrigger.Entry();
      ete.eventID = EventTriggerType.PointerUp;
      ete.callback.AddListener(onPointerUpAnyRGBSlider);
      et.triggers.Add(ete);
      
      et = RightEarRGBSlider.GetComponent<EventTrigger>();
      ete = new EventTrigger.Entry();
      ete.eventID = EventTriggerType.PointerDown;
      ete.callback.AddListener(onPointerDownAnyRGBSlider);
      et.triggers.Add(ete);
      ete = new EventTrigger.Entry();
      ete.eventID = EventTriggerType.PointerUp;
      ete.callback.AddListener(onPointerUpAnyRGBSlider);
      et.triggers.Add(ete);

      EyeColorWheelCtrl.OnDragStart.AddListener(onJoystickDownRGB);
      EyeColorWheelCtrl.OnRelease  .AddListener(onJoystickUpRGB);
    }
    
    void onRecordButtonClicked(){
      trCurRobotController.Instance.CheckOpenChrome();
      if(robot != null){
        PIBInterface_internal.onClickSoundRecordButton();
      }
      SoundManager.StartOnce(SoundManager.trAppSound.USER_SOUNDS_MIC);
    }
    
    void onEyeColorChanged( wwGUIJoystick ctrl, Vector2 val){
      trCurRobotController.Instance.CheckOpenChrome();
      float sat = val.magnitude ;
      float hue = Mathf.Atan2(val.y, val.x)/(2 * Mathf.PI);
      Color color = wwColorUtil.HSVtoRGB(hue, sat, 1);
      EyeColorIndicator.color = color;

      eyeRgbVal = new Vector2(hue, sat);
    }
    
    void onLeftEarRGBChanged(float val){
      trCurRobotController.Instance.CheckOpenChrome();
      leftRgbVal = val;
      LeftColorIndicator.color =   wwColorUtil.HSVtoRGB(leftRgbVal, 1, 1);
      
    }
    
    void onRightEarRGBChanged(float val){
      trCurRobotController.Instance.CheckOpenChrome();
      rightRgbVal = val;
      RightColorIndicator.color =   wwColorUtil.HSVtoRGB(rightRgbVal, 1, 1);
    }
    
    void onEyeRingButtonFullClicked(){
      trCurRobotController.Instance.CheckOpenChrome();
      if(robot != null){
        robot.cmd_eyeRing(1.0, "full_blink", piMathUtil.deserializeBoolArray(0));
      }
    }
    
    void onEyeRingButtonSmileClicked(){
      trCurRobotController.Instance.CheckOpenChrome();
      if(robot != null){
        robot.cmd_eyeRing(1.0, "wink", piMathUtil.deserializeBoolArray(0));
      }
      //eyeRingButton(0x09F2);
    }
    
    void onEyeRingButtonOffClicked(){
      trCurRobotController.Instance.CheckOpenChrome();
      eyeRingButton(0);
    }
    
    void onEyeRingButtonFrownClicked(){
      trCurRobotController.Instance.CheckOpenChrome();
      if(robot != null){
        robot.cmd_eyeRing(1.0, "swirl", piMathUtil.deserializeBoolArray(0));
      }
    }
    
    void eyeRingButton(int val){
      if(robot != null){
        bool[] bitmap = piMathUtil.deserializeBoolArray(val);
        robot.cmd_eyeRing(1.0f, "", bitmap);
      }
    }
    
    void handleLight(bool force){
      
      frameCount ++;
      
      if(PatternPanel.gameObject.activeSelf){
        switch(curPattern){
        case PatternType.ON:
          brightnessVal = 1;
          break;
        case PatternType.OFF:
          brightnessVal = 0;
          break;
        case PatternType.BINARY:
          int tmp = frameCount/10;
          brightnessVal = tmp % 2 == 0 ? 1:0;
          break;
        case PatternType.SINE:
          float param = frameCount * 2.0f * Mathf.PI / 30;
          brightnessVal = ((Mathf.Sin(param)) + 1.0f) / 2.0f * (1.0f - 0.01f) + 0.01f;
          break;
        }
      }
      
      bool isBrightnessChanged = brightnessVal != preBrightnessVal;
      if(isBrightnessChanged){
        preBrightnessVal = brightnessVal;
      }
      
      if(force || (eyeRgbVal != preEyeRgbVal) || isBrightnessChanged){
//        eyeRingButton(0);
        uint[] components = new uint[] {
          (uint)PI.ComponentID.WW_COMMAND_RGB_CHEST
        };
        Color color = wwColorUtil.HSVtoRGB(eyeRgbVal.x, eyeRgbVal.y, brightnessVal);
        robot.cmd_rgbLights(color.r, color.g, color.b, components);
        preEyeRgbVal = eyeRgbVal;
      }
      
      if(force || (leftRgbVal != preLeftRgbVal) || isBrightnessChanged){
        Color color = wwColorUtil.HSVtoRGB(leftRgbVal, 1, brightnessVal);
        uint[] components = new uint[] {
          (uint)PI.ComponentID.WW_COMMAND_RGB_LEFT_EAR
        };
        robot.cmd_rgbLights(color.r, color.g, color.b, components);
        preLeftRgbVal = leftRgbVal;
      }
      
      if(force || (rightRgbVal != preRightRgbVal) || isBrightnessChanged){
        Color color = wwColorUtil.HSVtoRGB(rightRgbVal, 1, brightnessVal);
        uint[] components = new uint[] {
          (uint)PI.ComponentID.WW_COMMAND_RGB_RIGHT_EAR
        };
        robot.cmd_rgbLights(color.r, color.g, color.b,components);
        preRightRgbVal = rightRgbVal;
      }
    }
  
    void playWaveformSound() {

      SoundManager.trAppSound trAS = SoundManager.trAppSound.INVALID;
      
      switch (curPattern) {
        default:
          WWLog.logError("unhandled pattern -> sound mapping: " + curPattern.ToString());
          break;
        case PatternType.OFF:
          break;
        case PatternType.SINE:
          trAS = SoundManager.trAppSound.RC_WAVEFORM_SINE;
          break;
        case PatternType.BINARY:
            trAS = SoundManager.trAppSound.RC_WAVEFORM_STEPPED;
            break;
        case PatternType.ON:
            trAS = SoundManager.trAppSound.RC_WAVEFORM_SOLID;
            break;
      }

      List<SoundManager.trAppSound> killThese = new List<SoundManager.trAppSound> {
        SoundManager.trAppSound.RC_WAVEFORM_SINE,
        SoundManager.trAppSound.RC_WAVEFORM_STEPPED,
        SoundManager.trAppSound.RC_WAVEFORM_SOLID,
      };

      foreach (SoundManager.trAppSound killThis in killThese) {
        if (SoundManager.soundManager.isSoundPlaying(killThis)) {
          SoundManager.soundManager.StopSound(killThis);
        }
      }

      if (trAS != SoundManager.trAppSound.INVALID) {
        SoundManager.soundManager.PlaySound(trAS);
      }

    }

    void changePattern(int dp) {
      int cp = (int)curPattern;
      cp += dp;
      if (cp < 0) {
        cp = 3;
      }
      else if (cp > 3) {
        cp = 0;
      }
      curPattern = (PatternType)cp;

      trCurRobotController.Instance.CheckOpenChrome();
      setView();
      playWaveformSound();
    }
    
    void onNextButtonClicked(){
      changePattern(1);
    }
    
    void onPreviousButtonClicked(){
      changePattern(-1);
    }
    
    void setView(){
      if(curPatternImage != null){
        curPatternImage.SetActive(false);
      }     
      switch(curPattern){
      case PatternType.ON:
        curPatternImage = OnSprite;
        break;
      case PatternType.OFF:
        curPatternImage = OffSprite;
        break;
      case PatternType.SINE:
        curPatternImage = SineSprite;
        break;
      case PatternType.BINARY:
        curPatternImage = BinarySprite;
        break;
      }
      curPatternImage.SetActive(true);
    }
    
    void slowUpdate () {
      if(!this.gameObject.activeSelf){
        return;
      }
      
      if(robot != null){
        bool force = (prevRobot != robot);
        handleLight(force);
      }
      
      prevRobot = robot;
    }
    
    int rgbDownCount = 0;

    void onPointerDownAnyRGBSlider(BaseEventData unused) {
      rgbDownCount += 1;
      SoundManager.StartOnce(SoundManager.trAppSound.RC_COLOR_SLIDER_LOOP);
    }

    void onPointerUpAnyRGBSlider(BaseEventData unused) {
      rgbDownCount -= 1;
      if (rgbDownCount <= 0) {
        SoundManager.StopOnce(SoundManager.trAppSound.RC_COLOR_SLIDER_LOOP);
      }
    }
        
    void onJoystickDownRGB(wwGUIJoystick unused) {
      SoundManager.StartOnce(SoundManager.trAppSound.RC_COLOR_JOYSTICK_LOOP);
    }

    void onJoystickUpRGB(wwGUIJoystick unused) {
      SoundManager.StopOnce(SoundManager.trAppSound.RC_COLOR_JOYSTICK_LOOP);
    }
  }
}

