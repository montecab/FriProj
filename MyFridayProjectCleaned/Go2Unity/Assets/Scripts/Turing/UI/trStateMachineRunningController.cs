using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Turing{
  public class trStateMachineRunningController : MonoBehaviour {

    public GameObject RunButton;

    [SerializeField]
    private GameObject _startImg;
    [SerializeField]
    private GameObject _stopImg;

    private trProgram curProgram;
    public trProgram CurProgram{
      get{
        return curProgram;
      }
      set{
        if(curProgram == value){
          return;
        }
        else{
          curProgram = value;
          resumeProgram = null;
          CurFunction = null;
        }
      }
    }
    public trFunction CurFunction;
    public piBotBase CurRobot = null;

    public bool IsRunning = false;

    public trStateMachinePanelBase SMPanel;

    public delegate void RunButtonClickedDelegate(bool isRunning);
    public RunButtonClickedDelegate RunButtonClickedListener;


    public enum UIState{
      EDIT_PROGRAM,
      EDIT_FUNCTION,
      RUN_PROGRAM,
      RUN_FUNCTION
    }
    public UIState UiState = UIState.EDIT_PROGRAM;
    public trProgram resumeProgram; //remember the program when editing the function


    private trState curState = null;
    private const float INVALID_BUTTON_PRESS_TIME = -1f;
    private float lastTimeWhenButtonPressed = INVALID_BUTTON_PRESS_TIME;

    protected virtual void Start(){     

    }

    protected virtual void OnDestroy(){
      
    }

    protected virtual void OnEnable(){
      trCurRobotController.Instance.onConnectCurRobot += onConnectRobot;
      trCurRobotController.Instance.onDisConnectCurRobot += onDisconnectRobot;
      trCurRobotController.Instance.CheckRobotType = true;
      trCurRobotController.Instance.CheckConnectRobot();
      if(trCurRobotController.Instance.CurRobot != null){
        onConnectRobot(trCurRobotController.Instance.CurRobot);
      }
    }

    protected virtual void Update () {   
      if(IsRunning){
        if(curState!= CurProgram.StateCurrent){
          SMPanel.SetEnableCurrentState(curState, false);
          curState = CurProgram.StateCurrent;
          SMPanel.SetEnableCurrentState(curState, true);
          if(UiState == UIState.RUN_FUNCTION && curState == null){ // function ended
            toggleRunningState();
          }
        }

        if(curState != null && curState.Behavior.Type == trBehaviorType.FUNCTION){
          bool isFunctionEnd = ((trFunctionBehavior)(curState.Behavior)).FunctionProgram.StateCurrent == null;
          if(isFunctionEnd){
            SMPanel.StateToButtonTable[curState].FunctionRunningIndicator.SetActive(false);
          }
        }
      }
    }

    void onConnectRobot(piBotBase robot) {
      if (CurRobot != null) {
        CurRobot.OnState -= onRobotState;
      }

      CurRobot = robot;

      CurRobot.OnState += onRobotState;
      if (IsRunning) {
        // have the new robot take over!
        CurProgram.SetState(CurProgram.StateCurrent, CurRobot);
      }
    }

    void onDisconnectRobot(piBotBase robot) {

      if(trCurRobotController.Instance.CurRobot == null){
        if(CurRobot != null){
          CurRobot.OnState -= onRobotState;
          CurRobot = null;
        }

        if (IsRunning) {
          toggleRunningState();
        }

      }
    }

    protected virtual void OnDisable(){
      Reset();

      if(IsRunning){
        toggleRunningState();
      }

      if (trCurRobotController.Instance != null){
        trCurRobotController.Instance.onConnectCurRobot -= onConnectRobot;
        trCurRobotController.Instance.onDisConnectCurRobot -= onDisconnectRobot;       
      }

      if (CurRobot != null) {
        CurRobot.OnState -= onRobotState;
      }

    }



    private bool isButtonPressed(piBotBase robot) {
      piBotBo bot = (piBotBo) robot;
      if (bot.ButtonMain.state == PI.ButtonState.BUTTON_PRESSED || bot.Button1.state == PI.ButtonState.BUTTON_PRESSED 
        || bot.Button2.state == PI.ButtonState.BUTTON_PRESSED || bot.Button3.state == PI.ButtonState.BUTTON_PRESSED) {
        return true;
      }
      return false;
    }

    private float secondsSinceLastButtonPress()  {
      return Time.time - lastTimeWhenButtonPressed;
    }

    private const float MAX_NUMBER_OF_SECONDS_OF_INACTIVITY = 10f * 60f; //10 minutes


    protected virtual void onRobotState(piBotBase robot) {
      if (robot != CurRobot) {
        if (wwDoOncePerTypeVal<string>.doIt("state for nonCurrent Robot: " + robot.Name)) {
          WWLog.logError("got state for nonCurrent robot: " + robot.Name);
        }
        //    robot.apiInterface.disconnectRobot(robot.UUID);
        return;
      }
      if(IsRunning){
        if (this.isButtonPressed(robot)) {
          lastTimeWhenButtonPressed = Time.time;
        }
        else if (secondsSinceLastButtonPress() > MAX_NUMBER_OF_SECONDS_OF_INACTIVITY) {
          lastTimeWhenButtonPressed = INVALID_BUTTON_PRESS_TIME;
          toggleRunningState();
        }

        if(UiState == UIState.RUN_PROGRAM){
          trProgram mainProgram = resumeProgram == null? CurProgram: resumeProgram;
          mainProgram.OnRobotState(robot); 

          //order matters, process main program first then functions
          trFunction curFunction = null;
          if(mainProgram.StateCurrent.Behavior.Type == trBehaviorType.FUNCTION ){
            curFunction =  ((trFunctionBehavior)(mainProgram.StateCurrent.Behavior)).FunctionProgram;
          }     
          if(curFunction != null){
            curFunction.OnRobotState(robot);
          }

          if(curFunction != CurFunction){
            if(CurFunction != null){
              CurFunction.Reset(CurRobot, true);
            }
            CurFunction = curFunction;
          }
        }
        else{ //run function separately
          CurProgram.OnRobotState(robot);
        }
      }

    }

    void adjustUIState(){
      if(IsRunning){
        if(UiState == UIState.EDIT_FUNCTION){
          UiState = UIState.RUN_FUNCTION;
        }
        else if(UiState == UIState.EDIT_PROGRAM){
          UiState = UIState.RUN_PROGRAM;
        }
      }
      else{
        if(UiState == UIState.RUN_FUNCTION){
          UiState = UIState.EDIT_FUNCTION;
        }
        else if(UiState == UIState.RUN_PROGRAM){
          UiState = UIState.EDIT_PROGRAM;
        }
      }
    }


    public void OnRunButtonClicked(){
      // toggleRunningState() may leave IsRunning unchanged if no robot is connected.
      bool IsRunningBefore = IsRunning;

      toggleRunningState();

      if (IsRunningBefore != IsRunning) {
        // we play the sound here instead of in toggleRunningState() because it sounds weird when it plays due to challenge complete, eg.
        SoundManager.trAppSound trAS = IsRunning ? SoundManager.trAppSound.SM_RUN_ON : SoundManager.trAppSound.SM_RUN_OFF;
        SoundManager.soundManager.PlaySound(trAS);
      }
    }

    bool checkRobot(){
      piRobotType currentRobotType = trDataManager.Instance.CurrentRobotTypeSelected;

      bool currentRobotTypeConnected = false;

      foreach (piBotBase robot in  piConnectionManager.Instance.BotsInState(PI.BotConnectionState.CONNECTED)) {
        if (robot.robotType == currentRobotType) {
          currentRobotTypeConnected = true;
          break;
        }
      }

      if(!currentRobotTypeConnected){
        ShowConnectToRobotDialog();
        return false;
      }

      return true;

    }

    public void ShowConnectToRobotDialog() {
      piConnectionManager.Instance.showConnectToRobotDialog(trDataManager.Instance.CurrentRobotTypeSelected);
    }

    public virtual bool toggleRunningState(){
      if(!IsRunning){
        bool isRobotRight = checkRobot();
        if(!isRobotRight){
          return false;
        }
      }

      //if the scene is loading, only resets the robot
      if(trNavigationRouter.Instance.IsLoading){
        Reset();
        return false;
      }

      IsRunning = !IsRunning;
      updateUI();

      if (IsRunning){
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        lastTimeWhenButtonPressed = Time.time;
      }
      else {
        Screen.sleepTimeout = SleepTimeout.SystemSetting;
      }

      if(RunButtonClickedListener != null){
        RunButtonClickedListener(IsRunning);
      }

      Reset();
      return true;
    }

    void updateUI(){
      _stopImg.SetActive(IsRunning);  
      _startImg.SetActive(!IsRunning);  
      adjustUIState();
      SMPanel.SetRunMode(IsRunning);
    }

    protected void Reset(){
      if (piConnectionManager.Instance != null){
        piConnectionManager.Instance.ResetRobotsState();
      }

      if(CurProgram != null && CurRobot != null){
        CurProgram.Reset(CurRobot);
      }    

      curState = null;
    }
  }
}

