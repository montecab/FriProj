using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;
using Turing;
using WW.SimpleJSON;


public enum trTransferStatus {
  IDLE_NO_HISTORY,        // no transfer has been started
  TRANSFERRING,           // a transfer is in process
  IDLE_WITH_SUCCESS,      // the most recent transfer succeeded
  IDLE_WITH_FAILURE,      // the most recent transfer failed
}

public delegate void trTransferStatusDelegate(trTransferStatus status);

public class trTransferButtonController : MonoBehaviour {

  public trProtoController ProtoController;

  [SerializeField]
  private GameObject _transferConfirmationPrefab;
  [SerializeField]
  private Transform _transferConfirmationHolder;
  private trOkCancelDialog _transferConfirmationDialog;
  private trTransferButtonConfirmationDialog _transferButtonConfirmationDialog;

  public Button TransferToRobotButton;
  public Sprite DashTransferImage;
  public Sprite DotTransferImage;
  public Image RobotTypeImage;
  public trTransferStatusDelegate StatusDelegates;

  private bool isAllowedTransfer = false;
  private trTransferStatus status = trTransferStatus.IDLE_NO_HISTORY;
  private piRobotType recentRobotType = piRobotType.UNKNOWN;

  private trProgram programToSend;
  public trProgram ProgramToSend {
    get {
      if (programToSend == null){
        programToSend = ProtoController.CurProgram;
      }
      return programToSend;
    }
    set {
      programToSend = value;
    }
  }

	private void Start (){
    if (ProtoController == null) {
      WWLog.logError ("no proto controller");
    }
    if (trDataManager.Instance.IsInNormalMissionMode) {
      this.gameObject.SetActive (false);
    }
    if (TransferToRobotButton != null) {
      TransferToRobotButton.onClick.AddListener (showTransferConfirmationDialog);
      UpdateTransferToRobotButton();
    }
    piConnectionManager.Instance.OnConnectRobot += OnRobotStateChanged;
    piConnectionManager.Instance.OnDisconnectRobot += OnRobotStateChanged;
    ProtoController.RunButtonClickedListener += OnRunButtonClicked;
  }

  private void OnDestroy() {
    if (piConnectionManager.Instance != null) {
      piConnectionManager.Instance.OnConnectRobot -= OnRobotStateChanged;
      piConnectionManager.Instance.OnDisconnectRobot -= OnRobotStateChanged;
    }
  }

  private void OnRobotStateChanged(piBotBase robot){
    UpdateTransferToRobotButton();
  }

  private void OnRunButtonClicked(bool isRunning){
    UpdateTransferToRobotButton();
  }

  private void UpdateTransferToRobotButton(){
    if (TransferToRobotButton != null) {
      isAllowedTransfer = AllowedToBeginTransfer;
      TransferToRobotButton.gameObject.SetActive(isAllowedTransfer);
    }
    updateTransferRobotImage();
  }

  private void showTransferConfirmationDialog(){
    string robotName = ProtoController.CurRobot.Name;
    if (TMPro.TMP_TextUtilities.HasUnsupportedCharacters(robotName)) {
      WWLog.logDebug("Robot name has unsupported characters.  Replacing with Dash/Dot");
      robotName = wwLoca.Format(trCurRobotController.Instance.CurRobot.robotType == piRobotType.DASH ? "Dash" : "Dot");
    }
    if(_transferConfirmationDialog==null){
      GameObject obj = GameObject.Instantiate(_transferConfirmationPrefab, Vector3.zero, Quaternion.identity) as GameObject;
      obj.transform.SetParent(_transferConfirmationHolder, false);
      _transferConfirmationDialog = obj.GetComponent<trOkCancelDialog>();
      _transferButtonConfirmationDialog = obj.GetComponent<trTransferButtonConfirmationDialog>();
    }
    _transferConfirmationDialog.gameObject.SetActive(true);
    _transferButtonConfirmationDialog.SetupView(ProtoController, this);
    _transferConfirmationDialog.DescriptionText = wwLoca.Format("@!@Do you want to transfer\n'{0}'\nto\n'{1}'?@!@",
                                                                ProtoController.CurProgram.UserFacingName, 
                                                                robotName);
  }
  
  private piBotCommon TheBot {
    get {
      return (piBotCommon)(ProtoController.CurRobot);
    }
  }

  void updateTransferRobotImage(){
    piRobotType robotType = trDataManager.Instance.CurrentRobotTypeSelected;
    if (robotType != recentRobotType){
      recentRobotType = robotType;
      Sprite imageSprite = null;
      switch(recentRobotType){
        case piRobotType.DOT:
          imageSprite = DotTransferImage;
          break;
        case piRobotType.DASH:
          imageSprite = DashTransferImage;
          break;
      }
      if (imageSprite != null){
        RobotTypeImage.sprite = imageSprite;
      }
    }
  }
  
  public bool AllowedToBeginTransfer {
    get {
      if (TheBot == null) {
        return false;
      }
      
      if (TheBot.NumberOfExecutingCommandSequences > 0) {
        return false;
      }
      
      if (TheBot.pendingShellCommandCount > 0) {
        return false;
      }
      
      if (Status == trTransferStatus.TRANSFERRING) {
        return false;
      }

      if(ProtoController.IsRunning){
        return false;
      }

      if(ProtoController.CurProgram != null && ProtoController.CurProgram.RobotType != ProtoController.CurrentRobotType){
        return false;
      }

      return true;    
    }
  }
  
  public trTransferStatus Status{
    get {
      return status;
    }
  }
    
  void setStatus(trTransferStatus value) {
    if (value != status) {
      status = value;
      if (StatusDelegates != null) {
        StatusDelegates(status);
      }
    }
  }
  
  private byte[] getBytesToTransfer() {
    if (TheBot == null) {
      WWLog.logError("null robot.");
      return null;
    }
    
    trProgram theProgram = ProgramToSend;
    if (theProgram == null) {
      WWLog.logError("null program.");
      return null;
    }
    
    byte[] ret = new trToFirmware().toFirmware(theProgram, TheBot.robotType);
    
    return ret;
  }
  
  private string RobotFilePath {
    get {
      string ret = "";
      ret += sharedConstants.TOK_SPKU;
      ret += sharedConstants.TOK_MAIN_SPARK_FILE;
      ret += sharedConstants.TOK_ROBOT_SUFFIX_SPARK;
      return ret;
    }
  }
  
  public void onClick() {
      StartCoroutine(transfer(TheBot));
  }
  
  IEnumerator transferPowerOn(piBotCommon robot) {
    if (robot == null) {
      setStatus(trTransferStatus.IDLE_WITH_FAILURE);
      yield break;
    }
    if (robot.NumberOfExecutingCommandSequences > 0) {
      setStatus(trTransferStatus.IDLE_WITH_FAILURE);
      yield break;
    }
    

    trMultivariate.trAppOptionValue option = trMultivariate.Instance.getOptionValue(trMultivariate.trAppOption.POWER_ON_STATE_MACHINE);

    yield return transferPowerOn(robot, option);
  }

  public static IEnumerator transferPowerOn(piBotCommon robot, trMultivariate.trAppOptionValue option) {
    trProgram trPrg = null;

    if (option == trMultivariate.trAppOptionValue.CURRENT) {
      trPrg = trDataManager.Instance.GetCurProgram();
      WWLog.logInfo("Using poweron state-machine: (current)");
    }
    else {
      Dictionary<trMultivariate.trAppOptionValue, string> jsons = new Dictionary<trMultivariate.trAppOptionValue, string>()
      {
        {trMultivariate.trAppOptionValue.NO_FRILLS_IMMEDIATE, "PowerOn_Immediate"},
        {trMultivariate.trAppOptionValue.NO_FRILLS_BUTTON   , "PowerOn_Button"},
        {trMultivariate.trAppOptionValue.EYERING            , "PowerOn_EyeRing"},
        {trMultivariate.trAppOptionValue.PRODUCTION         , "PowerOn_Production_<ROBOT>"},
      };
      
      if (!jsons.ContainsKey(option)) {
        WWLog.logError("unhandled option: " + option.ToString());
        option = trMultivariate.trAppOptionValue.PRODUCTION;
      }
      
      string robotType = robot.robotType == piRobotType.DOT ? "Dot" : "Dash";
      string baseName = jsons[option].Replace(sharedConstants.TOK_ROBOT, robotType);
      
      WWLog.logInfo("Using poweron state-machine: " + baseName);
      
      string powerOnStateMachineSource = "RobotResources/OnRobot/Spark/" + baseName;
      string s = Resources.Load<TextAsset>(powerOnStateMachineSource).text;
      JSONNode jsn = JSON.Parse(s).AsObject;      
      trPrg = trFactory.FromJson<trProgram>(jsn);      
    }
    
    
    byte[] payload = new trToFirmware().toFirmware(trPrg, robot.robotType);
    
    WWLog.logInfo("starting transfer of power on state machine - " + payload.Length + "bytes");
    
    string robotPath = "";
    robotPath += sharedConstants.TOK_SYST;
    robotPath += "POWERON";
    robotPath += sharedConstants.TOK_ROBOT_SUFFIX_SPARK;
    
    piConnectionManager.Instance.BotInterface.fileTransfer(robot.UUID, payload, robotPath, WWRobotFileTransferFileType.FILE_TYPE_SPARK);
    
    // TODO orion: add timeout mechanism here. progress should update regularly. 1s delay is too long.    
    do
    {
      yield return null;
    } while (robot != null && piConnectionManager.Instance.BotInterface.fileTransferProgress(robot.UUID) < 1);
    
    
    WWLog.logInfo("finishing transfer of power on state machine");
    yield return new WaitForSeconds(sharedConstants.fileTransferSafetyTime);

    string animFileToPlay = (robot.robotType == piRobotType.DASH ? trTransferButtonConfirmationDialog.kDashAnimationFileName : trTransferButtonConfirmationDialog.kDotAnimationFileName);
    string animJson = trMoodyAnimations.Instance.getJsonForAnim(animFileToPlay);
    robot.cmd_startSingleAnim(animJson);
  }
  
  IEnumerator transfer(piBotCommon robot) {
    if (robot == null) {
      setStatus(trTransferStatus.IDLE_WITH_FAILURE);
      yield break;
    }
    if (robot.NumberOfExecutingCommandSequences > 0) {
      setStatus(trTransferStatus.IDLE_WITH_FAILURE);
      yield break;
    }
    
    setStatus(trTransferStatus.TRANSFERRING);

    if (trMultivariate.Instance.getOptionValue(trMultivariate.trAppOption.POWER_ON_STATE_MACHINE) != trMultivariate.trAppOptionValue.PRODUCTION){
      yield return StartCoroutine(transferPowerOn(robot));
    }
    
    byte[] payload = getBytesToTransfer();
    if (payload == null) {
      setStatus(trTransferStatus.IDLE_WITH_FAILURE);
    }
    
    string robotPath = RobotFilePath;
    
    WWLog.logInfo("beginning file transfer to " + robot.Name + ":" + robotPath);
    
    new trTelemetryEvent(trTelemetryEventType.FP_TRANSFER, true)
      .add(trTelemetryParamType.ROBOT_TYPE, robot.robotType.ToString())
      .add(trTelemetryParamType.NUM_STATES, ProgramToSend.UUIDToStateTable.Count)
      .add(trTelemetryParamType.NUM_TRANSITIONS, ProgramToSend.UUIDToTransitionTable.Count)
      .emit();
    
    piConnectionManager.Instance.BotInterface.fileTransfer(robot.UUID, payload, robotPath, WWRobotFileTransferFileType.FILE_TYPE_SPARK);

    // TODO orion: add timeout mechanism here. progress should update regularly. 1s delay is too long.    
    do
    {
      yield return null;
    } while (robot != null && piConnectionManager.Instance.BotInterface.fileTransferProgress(robot.UUID) < 1);
    
    WWLog.logInfo("nearly finished file transfer to " + robot.Name + ":" + robotPath);
    
    yield return new WaitForSeconds(sharedConstants.fileTransferSafetyTime);
    setStatus(trTransferStatus.IDLE_WITH_SUCCESS);
  }
  
  public static bool ExportToFile(trProgram trPrg, string exportFilePath) {
    if (trPrg == null) {
      WWLog.logError("program is null. sorry!");
      return false;
    }
    
    byte[] converted = new Turing.trToFirmware().toFirmware(trPrg, trPrg.RobotType);
    
    if (converted == null) {
      WWLog.logError("error converting program.");
      return false;
    }
    
    try {
      File.WriteAllBytes(exportFilePath, converted);
    }
    catch (System.Exception e) {
      WWLog.logError("problem writing to file " + exportFilePath + ".   error = " + e.ToString());
      return false;
    }
    
    return true;
  }
}




