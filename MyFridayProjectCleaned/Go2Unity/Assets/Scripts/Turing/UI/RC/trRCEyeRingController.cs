using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace Turing{
  public class trRCEyeRingController : MonoBehaviour {

    public Button EyeRingBlinkButton;
    public Button EyeRingOffButton;
    public Button EyeRingSmileButton;
    public Button EyeRingSpinButton;

    private piBotCommon robot{
      get{
        return (piBotCommon)(trCurRobotController.Instance.CurRobot);
      }
    }

    // Use this for initialization
  	void Start () {
      EyeRingBlinkButton.onClick.AddListener(onEyeRingButtonBlinkClicked);
      EyeRingSpinButton.onClick.AddListener(onEyeRingButtonSpinClicked);
      EyeRingOffButton.onClick.AddListener(onEyeRingButtonOffClicked);
      EyeRingSmileButton.onClick.AddListener(onEyeRingButtonSmileClicked);
    }
    
    void setEyeAnimation(string name, bool checkChrome) {
      if (checkChrome) {
        trCurRobotController.Instance.CheckOpenChrome();
      }
      
      if (robot != null) {
        robot.cmd_eyeRing(1.0, name, piMathUtil.deserializeBoolArray(0));
      }
    }
    
    void onEyeRingButtonBlinkClicked(){
      setEyeAnimation("full_blink", true);
      playTheSound();
    }
    
    void onEyeRingButtonSmileClicked(){
      setEyeAnimation("wink", true);
      playTheSound();
    }
    
    void onEyeRingButtonOffClicked(){
      trCurRobotController.Instance.CheckOpenChrome();
      eyeRingButton(0);
      playTheSound();
    }
    
    void onEyeRingButtonSpinClicked(){
      setEyeAnimation("swirl", true);
      playTheSound();
    }
    
    void playTheSound() {
      SoundManager.trAppSound soundToPlay = SoundManager.trAppSound.RC_EYE_DASH;
      
      if (robot != null) {
        if (robot.robotType == piRobotType.DOT) {
          soundToPlay = SoundManager.trAppSound.RC_EYE_DOT;
        }
      }
      
      SoundManager.soundManager.PlaySound(soundToPlay);
    }
    
    void eyeRingButton(int val){
      if(robot != null){
        bool[] bitmap = piMathUtil.deserializeBoolArray(val);
        robot.cmd_eyeRing(1.0f, "", bitmap);
      }
    }
  }
}
