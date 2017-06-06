using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;

namespace Turing{
  public class trRCFreeplayController : MonoBehaviour {
    public trRCMoveController MoveCtrl;
    public trProtoController ProtoCtrl;
    public Button MinimizeButton;

    public GameObject Jostick;

    private bool isShow = false;
    private bool isJoystickShown = false;

    private piBotBo robot{
      get{
        return (piBotBo)(trCurRobotController.Instance.CurRobot);
      }
    }
    private piBotBo preRobot = null;
   

    // Use this for initialization
    void Start () {
      checkShowPanel();
      MinimizeButton.onClick.AddListener(onButtonClicked);
     
      MoveCtrl.OnMoveInUseChanged += onMoveInUse;
    }

    void onMoveInUse(bool inUse){
      trDataManager.Instance.IsRCFreeplayInUse = inUse;
      MoveCtrl.IsEnabled = inUse;
      trState curState = ProtoCtrl.CurProgram.StateCurrent;
      if(curState != null){
        if(inUse && curState.Behavior.Type == trBehaviorType.MOODY_ANIMATION){
            robot.cmd_stopSingleAnim();
        }
        else if(!inUse && curState.Behavior.Type.IsContinuousMove()){
          curState.SetActive(false, robot); 
          curState.SetActive(true, robot); 
        }
      }
     
    }


    void onButtonClicked(){
      ToggleJoystick();
    }

    public void ToggleJoystick(){
      isJoystickShown = !isJoystickShown;
      MinimizeButton.gameObject.SetActive(!isJoystickShown);
      if(isJoystickShown){
        maxmizeJoystick();
      }
      else{
        minimizeJoystick();
      }
    }
      
    void maxmizeJoystick(){
      Jostick.SetActive(true);
      Jostick.transform.localScale = Vector3.one *0.1f;
      Jostick.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);
      ProtoCtrl.BehaviorPanelCtrl.UsageController.ForceHide();
    }

    void minimizeJoystick(){
      Jostick.transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.OutBack);
    }

    void checkShowPanel(){
      bool shouldShow = robot != null && robot.robotType == piRobotType.DASH;
      shouldShow &= trDataManager.Instance.IsInFreePlayMode;
      showPanel(shouldShow);
    }


    void showPanel(bool show){
      bool allowed = trMultivariate.Instance.getOptionValue(trMultivariate.trAppOption.UNLOCK_CONTROLLER) == trMultivariate.trAppOptionValue.YES;
      if(!allowed){
        return;
      }
      if(isShow == show){
        return;
      }
      isShow = show;

      if(!isShow){
        Jostick.SetActive(false);
        MinimizeButton.gameObject.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.OutBack);
      }
      else{        
        MinimizeButton.gameObject.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
      }

    }
      

    void Update(){
      bool isMinimize = isJoystickShown && Input.GetMouseButtonUp(0)&& !trDataManager.Instance.IsRCFreeplayInUse;
      #if UNITY_IOS || UNITY_ANDROID
      isMinimize = isMinimize && Input.touchCount == 1;
      #endif
      if(isMinimize){
        Vector3 p = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        isMinimize = isMinimize && !RectTransformExtensions.IsInRect(this.GetComponent<RectTransform>(), p);
      }

      if(isMinimize){
        ToggleJoystick();
      }
      if(preRobot != robot){
        checkShowPanel();
      }
      preRobot = robot;
    }   
  }

}
