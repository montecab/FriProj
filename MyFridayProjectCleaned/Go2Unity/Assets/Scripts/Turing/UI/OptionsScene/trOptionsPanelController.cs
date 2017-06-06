using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using Turing;


public class trOptionsPanelController : MonoBehaviour, piRobotShellDelegate {

  public Button           backBtn;
  public Button           okayBtn;
  public Transform        itemsList;
  public trOptionItem     exampleItem;
  public trOptionItem     exampleSpacer;
  public trOkCancelDialog confirmationDialog;
  public TextMeshProUGUI  buildInfoTxt;

  // this is a hack to deal with a phontom-click issue:
  // http://answers.unity3d.com/questions/1103259/android-phantom-ugui-button-click-when-loading-new.html
  private bool  swallowOptionClick = true;
  private float swallowFirstOptionClickTimeThresh = 0.5f;

  void Start () {
    swallowOptionClick = true;

    createItems();
    
    backBtn.onClick.AddListener(onClickDone);
    okayBtn.onClick.AddListener(onClickDone);
    
    buildInfoTxt.text = BuildInfo.Summary;

    // ensure the singleton is instantiated so it catches robot-connect events in unity IDE.
    GameObject tmp = trCurRobotController.Instance.gameObject;
    tmp.name = "unused";
  }
  
  trOptionItem createItem(trOptionItem exampleObject) {
    trOptionItem ret = GameObject.Instantiate<trOptionItem>(exampleObject);
    ret.transform.SetParent(itemsList);
    ret.transform.localScale = Vector3.one;
    return ret;
  }
  
  void createItem(string label, trMultivariate.trAppOption optionYesNo) {
    trOptionItem item = createItem(exampleItem);
    item.label.text = label;
    item.AppOptionYESNO = optionYesNo;
    item.GetComponent<Button>().onClick.AddListener(() => {onClickToggleYesNo(item);});
  }
  
  void createItem(string label, UnityEngine.Events.UnityAction ua) {
    trOptionItem item = createItem(exampleItem);
    item.label.text = label;
    item.AppOptionYESNO = trMultivariate.trAppOption.NULL_OPTION;
    item.GetComponent<Button>().onClick.AddListener(ua);
  }
  
  void createItems() {
    exampleItem  .gameObject.SetActive(true);
    exampleSpacer.gameObject.SetActive(true);
    
    createItemsTeachers();

    if (trDataManager.Instance.optionsPanelShowInternal) {
      createItem(exampleSpacer);
      createItemsInternal();
    }
    
    // turn off the example items
    exampleItem  .gameObject.SetActive(false);
    exampleSpacer.gameObject.SetActive(false);
    
    updateStatus();
  }
  
  void createItemsTeachers() {
    createItem(wwLoca.Format("@!@Enable Controller Mode@!@")  , trMultivariate.trAppOption.UNLOCK_CONTROLLER);
    createItem(wwLoca.Format("@!@Reset App@!@")               , onClickResetChallenges);
    createItem(wwLoca.Format("@!@Unlock All Cues@!@")         , trMultivariate.trAppOption.UNLOCK_ALL_CUE_REWARDS);
    createItem(wwLoca.Format("@!@Unlock FreePlay@!@")         , trMultivariate.trAppOption.UNLOCK_FREEPLAY);
    createItem(wwLoca.Format("@!@Unlock Wonder Cloud@!@")     , trMultivariate.trAppOption.UNLOCK_COMMUNITY);
  }
    
  void createItemsInternal() {
    createItem(wwLoca.Format("@!@Enable Debug Cursor@!@")     , trMultivariate.trAppOption.SHOW_TOUCH_CURSOR);
    createItem("Smiley-Face Strings"     , trMultivariate.trAppOption.DEBUG_TRANSLATION_MODE);
    createItem("Unlock Experimental Cues", trMultivariate.trAppOption.UNLOCK_EXPERIMENTAL_TRIGGERS);
    createItem("Unlock All Rewards"      , trMultivariate.trAppOption.UNLOCK_ALL_REWARDS);
    createItem("Show Internal Animations", trMultivariate.trAppOption.UNLOCK_INTERNAL_ANIMATIONS);
    createItem("FPS Meter"               , trMultivariate.trAppOption.SHOW_FPS_METER);
    createItem("Video Timestamp"         , trMultivariate.trAppOption.SHOW_VIDEO_TIMESTAMP);
    createItem("Functions"               , trMultivariate.trAppOption.UNLOCK_FUNCTIONS);
    createItem("Vault: Internal Prog's"  , trMultivariate.trAppOption.VAULT_ALLOW_INTERNAL);
    createItem("Clipboard"               , trMultivariate.trAppOption.CLIPBOARD);
    createItem("Element Info Panel"      , trMultivariate.trAppOption.ELEMENT_INFO_PANEL);
    createItem("Use Pose-Based Lin/Ang"  , trMultivariate.trAppOption.USE_POSE_BASED_LINANG);
    createItem("Old Sharing Prototype"   , trMultivariate.trAppOption.UNLOCK_SHARING);
//  createItem("State Machine Grid"      , trMultivariate.trAppOption.STATE_MACHINE_GRID);
    createItem("Puppet Action"           , trMultivariate.trAppOption.UNLOCK_PUPPET);
    createItem("Anim Shop"               , trMultivariate.trAppOption.UNLOCK_ANIM_SHOP);
    createItem("Run-Spark Action"        , trMultivariate.trAppOption.RUN_SPARK_BEHAVIOR);
    createItem("Crash App"               , onClickCrashApp);
    createItem("Power-On: Default"       , onClickPwrOnDefault);
    createItem("Power-On: Calm"          , onClickPwrOnCalm);
    createItem("Power-On: User-SM"       , onClickPwrOnUserSM);
    createItem("Power-On: Current"       , onClickPwrOnCurrent);
    createItem("Volume: 0%"              , onClickVolume0);
    createItem("Volume: 6%"              , onClickVolume6);
    createItem("Volume: 50%"             , onClickVolume50);
    createItem("Volume: 100%"            , onClickVolume100);
  }
  
  void updateItemStatus(trOptionItem item) {
    if ((item.graphicON == null) || (item.graphicOFF == null)) {
      // it's a spacer
      return;
    }
    
    if (item.AppOptionYESNO == trMultivariate.trAppOption.NULL_OPTION) {
      item.graphicOFF.gameObject.SetActive(false);
      item.graphicON .gameObject.SetActive(false);
    }
    else {
      bool on = false;
      on = on || (trMultivariate.Instance.getOptionValue(item.AppOptionYESNO) == trMultivariate.trAppOptionValue.YES );
      on = on || (trMultivariate.Instance.getOptionValue(item.AppOptionYESNO) == trMultivariate.trAppOptionValue.SHOW);
      
      item.graphicOFF.gameObject.SetActive(!on);
      item.graphicON .gameObject.SetActive( on);
    }
  }
  
  void updateStatus() {
    trOptionItem[] items = itemsList.GetComponentsInChildren<trOptionItem>();
    foreach (trOptionItem item in items) {
      updateItemStatus(item);
    }
  }

  // see explanation in declaration of swallowOptionClick.
  bool shouldSwallowClick() {
    if (swallowOptionClick && (Time.timeSinceLevelLoad < swallowFirstOptionClickTimeThresh)) {
      swallowOptionClick = false;
      return true;
    }
    return false;
  }
  
  void onClickResetChallenges() {
    if (shouldSwallowClick()) {
      return;
    }

    confirmationDialog.TitleText         = wwLoca.Format("@!@Reset@!@");
    confirmationDialog.DescriptionText   = wwLoca.Format("@!@Reset all the following on this device ?\n  * Challenge Progress\n  * Rewards\n  * Show-Once Dialogs & Screens\n\n\"My Programs\" will remain.@!@");

    confirmationDialog.OnOKButtonClicked = onClickResetChallengesConfirm;
    confirmationDialog.SetActive(true);
  }
  
  void onClickResetChallengesConfirm() {
    trDataManager.Instance.MissionMng.UserOverallProgress.Clear();
    trRewardsManager.Instance.clearAllRewards();
    PlayerPrefs.DeleteAll();
  }
  
  void onClickToggleYesNo(trOptionItem sender) {
    if (shouldSwallowClick()) {
      return;
    }
    
    trMultivariate.Instance.incrementOptionValue(sender.AppOptionYESNO);
    updateItemStatus(sender);
  }
  
  void onClickDone() {
    trNavigationRouter.Instance.ShowSceneWithName(trNavigationRouter.SceneName.LOBBY);
  }

  private trMultivariate.trAppOptionValue powerOnOption;

  void onClickPwrOn(trMultivariate.trAppOptionValue option, string desc) {
    if (shouldSwallowClick()) {
      return;
    }

    piBotCommon bot = (piBotCommon)(trCurRobotController.Instance.CurRobot);
    if (bot == null) {
      showConnectToRobot();
    }
    else {
      powerOnOption = option;
      confirmationDialog.TitleText         = "Power-On S.M."; // internal-only string
      confirmationDialog.DescriptionText   = option.ToString() + " -> " + bot.Name + "\n" + desc;
      confirmationDialog.OnOKButtonClicked = onClickConfirmPowerOn;
      confirmationDialog.SetActive(true);
    }
  }

  void onClickConfirmPowerOn() {
    piBotCommon bot = (piBotCommon)(trCurRobotController.Instance.CurRobot);
    if (bot != null) {
      StartCoroutine(trTransferButtonController.transferPowerOn(bot, powerOnOption));
    }
    else {
      showConnectToRobot();
    }
  }

  void showDialog(string title, string body) {
    confirmationDialog.TitleText         = title;
    confirmationDialog.DescriptionText   = body;
    confirmationDialog.OnOKButtonClicked = null;
    confirmationDialog.SetActive(true);
  }

  void showConnectToRobot() {
    showDialog(wwLoca.Format("@!@Connect to a Robot@!@"), wwLoca.Format("@!@Please connect to a robot and try again.@!@"));
  }

  void showError() {
    showDialog(wwLoca.Format("@!@Error@!@"), wwLoca.Format("@!@Something went wrong.@!@"));
  }

  void showSuccess(string msg) {
    showDialog(wwLoca.Format("@!@Success!@!@"), msg);
  }

  void onClickPwrOnDefault() {
    onClickPwrOn(trMultivariate.trAppOptionValue.PRODUCTION, "The standard production power-on state-machine.");
  }

  void onClickPwrOnCalm() {
    onClickPwrOn(trMultivariate.trAppOptionValue.NO_FRILLS_BUTTON, "Does nothing until main button is pressed,\nthen runs the user's state-machine.");
  }

  void onClickPwrOnUserSM() {
    onClickPwrOn(trMultivariate.trAppOptionValue.NO_FRILLS_IMMEDIATE, "Immediately runs the user's state-machine.");
  }

  void onClickPwrOnCurrent() {
    onClickPwrOn(trMultivariate.trAppOptionValue.CURRENT, "Transfer your most recent Free-Play state-machine.\nYou probably want to enable the 'Run Spark' Action!");
  }

  void onClickCrashApp() {
    if (shouldSwallowClick()) {
      return;
    }

    confirmationDialog.TitleText         = "@noloc@Crash The App!";
    confirmationDialog.DescriptionText   = "@noloc@This will crash the app for testing purposes.\nWe use a null-dereference followed by a stack overflow.";
    confirmationDialog.OnOKButtonClicked = onClickConfirmCrashApp;
    confirmationDialog.SetActive(true);
  }

  void onClickConfirmCrashApp() {
    // kick off a timer for an alternate crash method
    piUnityDelayedExecution.Instance.delayedExecution0(doStackOverflow, 0.5f);

    GameObject go = null;
    go.SetActive(true);
  }

  void doStackOverflow() {
    doStackOverflow();
  }

  void onClickVolume(int val) {
    if (shouldSwallowClick()) {
      return;
    }

    piBotCommon bot = (piBotCommon)(trCurRobotController.Instance.CurRobot);
    if (bot == null) {
      showConnectToRobot();
    }
    else {
      bot.shellDelegate = this;
      bot.cmd_shell("CFG SET VOL " + val.ToString());
    }
  }

  void onClickVolume0() {
    onClickVolume(0);
  }
  void onClickVolume6() {
    onClickVolume(6);
  }
  void onClickVolume50() {
    onClickVolume(50);
  }
  void onClickVolume100() {
    onClickVolume(100);
  }

  public void didExecuteShellCommand(piBotBase robot, string command, string response) {
    bool requiresRobotRebootBeforeNewVolumeIsActive = true;
    if (requiresRobotRebootBeforeNewVolumeIsActive) {
      robot.cmd_shell("RST");
    }
    else {
      string animFileToPlay = (robot.robotType == piRobotType.DASH ? trTransferButtonConfirmationDialog.kDashAnimationFileName : trTransferButtonConfirmationDialog.kDotAnimationFileName);
      string animJson = trMoodyAnimations.Instance.getJsonForAnim(animFileToPlay);
      ((piBotCommon)robot).cmd_startSingleAnim(animJson);
    }
  }

  public void didFailShellCommand   (piBotBase robot, string command, string error) {
    showError();
  }

}
