
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using WW.SimpleJSON;
using Turing;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;
using TMPro;
using monoflow;

public class trProtoController : trStateMachineRunningController {

  public static readonly Vector2 DRAGGING_MARGIN = new Vector2(80f, 40f); // margin around the limiter rect where we will auto-scroll
  public static readonly float   DRAGGING_SPEED_MAX = 150f;               // speed of drag at the outer edge of the margin.

  // NOTE: if you add a modal panel, please register it in ModalPanels.
  [Header("UI Components")]
  [SerializeField]
  private Transform _uiMainTransform;
  [SerializeField]
  private GameObject _elementInfoRing;
  [SerializeField]
  private GameObject _playButtonIndicator;
  public GameObject playButtonIndicator{get{return _playButtonIndicator;}}

  public TextMeshProUGUI ProgramName;
  public trDragDeleteController TrashCanCtrl;
  public trScrollViewLimiter AllowedScrollRect;
  public GameObject ProgramTitlePanel;
  public Button BackButton;
  public trUndoRedoController UndoRedoCtrl;
  public trTutorialFingerController TutorialCtrl;
  public trBehaviorPanelController BehaviorPanelCtrl;

  [Header("UI Panels")]
  [SerializeField]
  private GameObject _videoPanelPrefab;
  private trVideoPlayerPanelController _videoPanelCtrl;
  public trVideoPlayerPanelController VideoPanelCtrl{get{return _videoPanelCtrl;}}

  [SerializeField]
  private GameObject _puzzleInfoPanelPrefab;
  [SerializeField]
  private Transform _puzzleInfoPanelHolder;
  private trPuzzleInfoPanelController _puzzleInfoPnlCtrl;
  public trPuzzleInfoPanelController PuzzleInfoPnlCtrl{get{return _puzzleInfoPnlCtrl;}}

  [SerializeField]
  private GameObject _triggerConfigPanelPrefab;
  [SerializeField]
  private Transform _triggerConfigPanelHolder;
  private trTriggerConfigurePanelController _triggerConfigPanel;
  public trTriggerConfigurePanelController TriggerConfigurePanel{get{return _triggerConfigPanel;}}

  [SerializeField]
  private GameObject _soundConfigPanelPrefab;
  [SerializeField]
  private Transform _soundConfigPanelHolder;
  private trSoundConfigurePanelController _soundConfigPanel;
  public trSoundConfigurePanelController SoundConfigurePanel{get{return _soundConfigPanel;}}

  [SerializeField]
  private GameObject _animationConfigPanelPrefab;
  [SerializeField]
  private Transform _animationConfigPanelHolder;
  private trAnimationConfigurePanelController _animationConfigPanel;
  public trAnimationConfigurePanelController AnimationConfigurePanel{get{return _animationConfigPanel;}}

  [Header("External Controllers")]
  public trStateEditController StateEditCtrl;
  public trMissionEvaluationController MissionEvaluationCtrl;

  [Header("Dynamic Prefabs")]
  [SerializeField]
  private GameObject _clipboardPanelPrefab;
  private trClipboardPanelCtrl _clipboardPanel;

  [SerializeField]
  private GameObject _narrativePanelPrefab;
  [SerializeField]
  private Transform _narrativePanelHolder;
  private trNarrativeUIController _narrativeUICtrl;

  [SerializeField]
  private GameObject _elementInfoPanelPrefab;
  [SerializeField]
  private Transform _elementInfoPanelHolder;
  private trElementInfoPanelController _elementInfoPanel;

  [SerializeField]
  private GameObject _debugPanelPrefab;
  [SerializeField]
  private Transform _debugPanelHolder;
  private trDebugPanelController _debugPnlCtrl;

  [SerializeField]
  private GameObject _exportPanelPrefab;
  [SerializeField]
  private Transform _exportPanelHolder;
  private GameObject _exportPanel;

  [SerializeField]
  private GameObject _missionAuthoringPanelPrefab;
  [SerializeField]
  private Transform _missionAuthoringPanelHolder;
  private trAuthoringHintSMPanelController _missionAuthoringPanel;
  public trAuthoringHintSMPanelController missionAuthoringPanel{get{return _missionAuthoringPanel;}}

  [SerializeField]
  private GameObject _puzzleCompletePanelPrefab;
  [SerializeField]
  private Transform _puzzleCompletePanelHolder;
  private trPuzzleCompletePanelController _puzzleCompletePanel;

  [SerializeField]
  private Button _privateShareButton;
  [SerializeField]
  private GameObject _privateSharePanelPrefab;
  [SerializeField]
  private Transform _privateSharePanelHolder;
  private trPrivateSharePanelController _privateSharePanel;

  [SerializeField]
  private GameObject _triggerCalloutPrefab;
  [SerializeField]
  private Transform _triggerCalloutHolder;
  private trTutorialTriggerCalloutController _triggerCalloutPanel;

  [SerializeField]
  private GameObject _alertDialogPrefab;
  [SerializeField]
  private Transform _alertDialogHolder;

  [SerializeField]
  private GameObject _tutorialCanvasPrefab;
  [SerializeField]
  private Transform _tutorialCanvasHolder;
  private GameObject tutorial = null;

  [SerializeField]
  private GameObject _robotListPrefab;
  private RobotListController _robotListCtrl;
  public RobotListController RobotListCtrl{get{return _robotListCtrl;}}

  public delegate void OnConfigurationPanelVisibilityDelegate(bool visible);
  public OnConfigurationPanelVisibilityDelegate OnConfigPanelVisibilityChanged;
  public piRobotType CurrentRobotType {
    get {
      return CurRobot != null ? CurRobot.robotType : piRobotType.DASH;
    }
  }

  [Header("Authoring Mission Tools")]
  public trBehaviorMakerPanelController BehaviorMakerPanelCtrl;
  public trAuthoringMissionAreaConfigPanelController MissionAreaConfigCtrl;
  public trAuthoringMissionBehaviorPanel AuthoringBehaviorCtrl;

  private float timeStateMachineWasStarted = 0;
  private trState runningState = null;
  private Dictionary<int, Vector3> SavedPositionsDict = new Dictionary<int, Vector3>();
  private trFunctionBehavior loadedFunctionBehavior = null;
#if UNITY_EDITOR
  static private bool latchedOriginalFPSSettings = false;
  static private int vSyncOrig;
  static private int fpsOrig;
#endif

  private T InstantiatePrefab<T>(GameObject prefab, Transform holder){
    GameObject obj = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;
    obj.transform.SetParent(holder, false);
    return obj.GetComponent<T>();
  }

  void Awake(){
    trDataManager.Instance.Init(); // don't remove! or we can not run from editor main scene

    string s = trNavigationRouter.Instance.GetTransitionParameterForScene();
    
    RunMode runMode;

    if (trDataManager.Instance.MissionMng.AuthoringMissionInfo.EditState == MissionEditState.NORMAL
        &&piStringUtil.ParseStringToEnum<RunMode>(s, out runMode)) {
      trDataManager.Instance.IsMissionMode = (runMode == RunMode.Challenges);
    }

    if (trDataManager.Instance.IsMissionMode == false || !trDataManager.Instance.MissionMng.AuthoringMissionInfo.CurMission.IsTutorialMission){
      if(BehaviorPanelCtrl.UsageController != null){
        BehaviorPanelCtrl.UsageController.OnShouldHide += hideBehaviorPanels;
        BehaviorPanelCtrl.UsageController.OnShouldShow += showBehaviorPanels;
      }
    }
    
#if UNITY_EDITOR
    if (!latchedOriginalFPSSettings) {
      latchedOriginalFPSSettings = true;
      vSyncOrig = QualitySettings.vSyncCount;
      fpsOrig = Application.targetFrameRate;
    }
    if (trMultivariate.Instance.getOptionValue(trMultivariate.trAppOption.FORCE_BAD_FRAMERATE_IN_EDITOR) == trMultivariate.trAppOptionValue.YES) {
      int targetFPS = 10;
      WWLog.logWarn("THE FRAMERATE IS BEING FORCED TO BE VERY VERY LOW: " + targetFPS + "fps");
      QualitySettings.vSyncCount = 0;
      Application.targetFrameRate = targetFPS;
    }
    else {
      WWLog.logInfo("setting vSyncCount to " + vSyncOrig + " and target FPS to " + fpsOrig);
      QualitySettings.vSyncCount = vSyncOrig;
      Application.targetFrameRate = fpsOrig;
    }
#endif
  }
  
	protected override void Start () {
    base.Start();
    if (Application.isPlaying){
      DontDestroyOnLoad(piConnectionManager.Instance);
      DontDestroyOnLoad(trDataManager.Instance);
    }

    #if UNITY_EDITOR
      if(_robotListCtrl==null){
        _robotListCtrl = InstantiatePrefab<RobotListController>(_robotListPrefab, _uiMainTransform);
      }
      _robotListCtrl.ShowToggleList();
    #else
      piConnectionManager.Instance.showChromeButton();
    #endif

    trDataManager.Instance.Init();
    LoadProgram();
    initView();
    HideElementInfo();
    SMPanel = StateEditCtrl;
    if(BackButton != null){ // authoring mission system doesn't need the back button 
      BackButton.onClick.AddListener(onBackButtonClicked);
    }
    if(_privateShareButton != null){
      _privateShareButton.onClick.AddListener(onPrivateShareButtonClicked);
    }

    //Check trMultivariate settings
    trMultivariate.Instance.ValueChanged += onOptionValueChanged;
    CheckMultivariateSettings();

    //FTUE
    if(FTUEManager.Instance.ShouldDisplayFTUE(FTUEType.MAIN_PLAY_BUTTON) && TutorialCtrl !=null && 
       trDataManager.Instance.IsMissionMode){
      if(tutorial==null){
        tutorial = Instantiate(_tutorialCanvasPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        tutorial.transform.SetParent(_tutorialCanvasHolder, false);
        tutorial.SetActive(false);
      }
      BackButton.gameObject.SetActive(false);
      ScrollRect scroll = StateEditCtrl.StatePanel.GetComponentInParent<ScrollRect>();
      scroll.enabled = false;
      StartCoroutine(CheckFTUEDisplay());
    }
 	}

  protected override void OnDestroy() {
    base.OnDestroy();
    if (trMultivariate.Instance != null) {
      trMultivariate.Instance.ValueChanged -= onOptionValueChanged;
    }
  }

  private IEnumerator CheckFTUEDisplay(){
    while (!TutorialCtrl.IsReadyToRun)
    {
      TutorialCtrl.CheckFTUEProgress();
      yield return new WaitForSeconds(0.25f);
    }
    // Display FTUE
    tutorial.SetActive(true);
    piConnectionManager.Instance.hideChromeButton();
    trFTUEController ftueCtrl = tutorial.GetComponent<trFTUEController>();
    ftueCtrl.SetupView(FTUEType.MAIN_PLAY_BUTTON, OnRunButtonClicked);
  }

  private void CheckMultivariateSettings(){
    if(trMultivariate.Instance.getOptionValue(trMultivariate.trAppOption.EXPORT_PANEL) == trMultivariate.trAppOptionValue.SHOW){
      if (_exportPanel==null) {
        _exportPanel = Instantiate(_exportPanelPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        _exportPanel.transform.SetParent(_exportPanelHolder, false);
      }
    }
    if (trMultivariate.Instance.getOptionValue(trMultivariate.trAppOption.OMNI_DISABLE_IN_TRAY) == trMultivariate.trAppOptionValue.YES){
      updateOmniStateViewInPanel();
    }
    if(trMultivariate.isYES(trMultivariate.trAppOption.DEBUG_MODE)){
      if(_debugPnlCtrl==null){
        _debugPnlCtrl = InstantiatePrefab<trDebugPanelController>(_debugPanelPrefab, _debugPanelHolder);
        _debugPnlCtrl.SetupView(this);
      }
    }
  }

  private void onOptionValueChanged (trMultivariate.trAppOption option, trMultivariate.trAppOptionValue newValue) {
    if (option == trMultivariate.trAppOption.CLIPBOARD){
      if (_clipboardPanel == null) {
        GameObject obj = GameObject.Instantiate(_clipboardPanelPrefab, _uiMainTransform) as GameObject;
        _clipboardPanel = obj.GetComponent<trClipboardPanelCtrl>();
      }
      _clipboardPanel.gameObject.SetActive(trMultivariate.isYESorSHOW(trMultivariate.trAppOption.CLIPBOARD));
      _clipboardPanel.protoCtrl = this;
    }
  }

  protected override void OnEnable(){
    base.OnEnable();
    WW.BackButtonController.Instance.AddListener(onBackButtonClicked);
  }

  protected override void OnDisable() {
    base.OnDisable();
    if(BehaviorPanelCtrl.UsageController != null){
      BehaviorPanelCtrl.UsageController.OnShouldHide -= hideBehaviorPanels;
      BehaviorPanelCtrl.UsageController.OnShouldShow -= showBehaviorPanels;
    }
    if(trDataManager.Instance != null && UiState == UIState.EDIT_FUNCTION){
      trDataManager.Instance.OnSaveCurProgram -= onSaveFunction;
    }
    if (WW.BackButtonController.Instance != null) {
      WW.BackButtonController.Instance.RemoveListener(onBackButtonClicked);
    }
  }

  void onLobbyButtonClicked(){
    trNavigationRouter.Instance.ShowSceneWithName(trNavigationRouter.SceneName.LOBBY);
  }

  void onPrivateShareButtonClicked(){
    if(_privateSharePanel==null){
      _privateSharePanel = InstantiatePrefab<trPrivateSharePanelController>(_privateSharePanelPrefab, _privateSharePanelHolder);
    }
    _privateSharePanel.gameObject.SetActive(true);
    _privateSharePanel.SetupView();
  }

  void onBackButtonClicked(){  
    if(trDataManager.Instance.IsMissionMode){
      trNavigationRouter.Instance.ShowSceneWithName(trNavigationRouter.SceneName.MAP);
    }
    else{
      trNavigationRouter.Instance.ShowSceneWithName(trNavigationRouter.SceneName.VAULT);
    }   
  }

  void initView(){
    if(trDataManager.Instance.MissionMng.AuthoringMissionInfo.EditState == MissionEditState.NORMAL){
      if(_videoPanelCtrl==null){
        _videoPanelCtrl = InstantiatePrefab<trVideoPlayerPanelController>(_videoPanelPrefab, null);
        _videoPanelCtrl.SetActive(false);
      }
      if(_puzzleInfoPnlCtrl==null){
          _puzzleInfoPnlCtrl = InstantiatePrefab<trPuzzleInfoPanelController>(_puzzleInfoPanelPrefab, _puzzleInfoPanelHolder);
      }
      if(_triggerConfigPanel==null){
        _triggerConfigPanel = InstantiatePrefab<trTriggerConfigurePanelController>(_triggerConfigPanelPrefab, _triggerConfigPanelHolder);
      }
      _triggerConfigPanel.InitView(this);
      if(_soundConfigPanel==null){
        _soundConfigPanel = InstantiatePrefab<trSoundConfigurePanelController>(_soundConfigPanelPrefab, _soundConfigPanelHolder);
        _soundConfigPanel.protoController = this;
      }
      if(_animationConfigPanel==null){
        _animationConfigPanel = InstantiatePrefab<trAnimationConfigurePanelController>(_animationConfigPanelPrefab, _animationConfigPanelHolder);
        _animationConfigPanel.protoController = this;
      }
    }
    if (trDataManager.Instance.IsMissionMode) {
      initViewMissionMode();
    }
  }

  void initViewMissionMode() {
    ProgramTitlePanel.SetActive(false);
    bool isAuthoring = trDataManager.Instance.MissionMng.AuthoringMissionInfo.EditState == MissionEditState.EDIT_HINT_PROGRAM;
    if(isAuthoring){
      if(_missionAuthoringPanel==null){
        _missionAuthoringPanel = InstantiatePrefab<trAuthoringHintSMPanelController>(_missionAuthoringPanelPrefab, _missionAuthoringPanelHolder);
        _missionAuthoringPanel.SetupView(this);
      }
    }
    else{
      if(trDataManager.Instance.MissionMng.UserOverallProgress.CurMissionInfo == null){
        WWLog.logWarn("CurMissionInfo is null");
      }
      else {
        if(_narrativeUICtrl==null){
          _narrativeUICtrl = InstantiatePrefab<trNarrativeUIController>(_narrativePanelPrefab, _narrativePanelHolder);
        }
        _narrativeUICtrl.SetupView(this);
      }
      _puzzleInfoPnlCtrl.SetupView(this, trDataManager.Instance.MissionMng.GetCurPuzzle(), updateBehaviourEnbaled);
      trDataManager.Instance.MissionMng.UserOverallProgress.CurMissionInfo.UpdateResetProgram();
    }
  }

  public void LoadFunction(trFunctionBehavior function){
    bool isUnlock = trMultivariate.Instance.getOptionValue(trMultivariate.trAppOption.UNLOCK_FUNCTIONS) == trMultivariate.trAppOptionValue.YES;
    if(!isUnlock){
      return;
    }
    if(UiState == UIState.EDIT_PROGRAM){
      UiState = UIState.EDIT_FUNCTION;
    }   
    resumeProgram = CurProgram;
    LoadFunctionProgram(function.FunctionProgram);
    trDataManager.Instance.OnSaveCurProgram += onSaveFunction;
    ProgramTitlePanel.SetActive(false);
    BackButton.gameObject.SetActive(false);
    loadedFunctionBehavior = function;
  }

  void onSaveFunction(){
    if(loadedFunctionBehavior != null){
      trDataManager.Instance.UserFunctions.SaveFunction(loadedFunctionBehavior);
    }   
  }

  public void ResumeProgram(){
    if(UiState == UIState.RUN_FUNCTION){
      toggleRunningState();
    }
    UiState = UIState.EDIT_PROGRAM;
    LoadFunctionProgram(resumeProgram);
    resumeProgram = null;
    trDataManager.Instance.OnSaveCurProgram -= onSaveFunction;
    ProgramTitlePanel.SetActive(true);
    BackButton.gameObject.SetActive(true);
    loadedFunctionBehavior = null;
  }

  public void ShowPuzzleCompletePanel(){
    if(_puzzleCompletePanel==null){
      _puzzleCompletePanel = InstantiatePrefab<trPuzzleCompletePanelController>(_puzzleCompletePanelPrefab, _puzzleCompletePanelHolder);
      _puzzleCompletePanel.SetupView(this);
    }
  }

  public void ShowProgramsMenu(){
    trNavigationRouter.Instance.ShowSceneWithName(trNavigationRouter.SceneName.VAULT, trVaultController.DisplayMode.MyPrograms.ToString());
  }

  public void ShowPuzzleInfoPanel(){
    PuzzleInfoPnlCtrl.gameObject.SetActive(true);
    PuzzleInfoPnlCtrl.DisplayInstructionPopup();
//    float height = PuzzleInfoPnlCtrl.TopPanel.GetHeight();
//    float yPos = PuzzleInfoPnlCtrl.TopPanel.transform.localPosition.y;
//    PuzzleInfoPnlCtrl.TopPanel.transform.DOKill();
//    PuzzleInfoPnlCtrl.TopPanel.transform.localPosition += new Vector3(0, height, 0);
//    PuzzleInfoPnlCtrl.TopPanel.transform.DOLocalMoveY(yPos, 1.0f).SetEase(Ease.InOutBack);
//    PuzzleInfoPnlCtrl.TopPanel.transform.DOShakeScale(0.3f, 0.2f).SetDelay(0.8f);
//    PuzzleInfoPnlCtrl.TopPanel.transform.DOLocalMoveY(yPos, 0.1f).SetDelay(1.0f); // make sure it's in the right position after tweening
  } 

  void onStopRobotButtonClicked(){
    foreach(piBotBo robot in piConnectionManager.Instance.BotsInState(PI.BotConnectionState.CONNECTED))
    {
      robot.cmd_bodyMotionStop();
      robot.cmd_headMove(0, 0);
    }
  }

  /* Returns the bot which is active in the workspace right now. 
   * @showChrome - If set to true and there is no robot active, returns null and shows the chrome hint dialog 
   */
  public piBotBase GetRobotToExecute(bool showChrome=true){
    if (CurRobot == null && showChrome){
      ShowConnectToRobotDialog();
    }
    return CurRobot;
  }
  
  public void SetCurState(trState state){
    if(CurProgram!= null){
      CurProgram.SetState(state, CurRobot);
    }
  }

  public void RunState(trState state){
    if(CurRobot == null){
      return;
    }
    ((piBotBo)CurRobot).cmd_move(0,0);
    ((piBotBo)CurRobot).cmd_headMove(0,0);
    state.Behavior.Execute(CurRobot);
    runningState = state;
  }
  
  public void StopRunningState() {
    if (CurRobot == null) {
      return;
    }
    ((piBotBo)CurRobot).cmd_move(0,0);
    ((piBotBo)CurRobot).cmd_headMove(0,0);
    runningState = null;
  }

  public void LoadProgram(trProgram program=null){
    if(IsRunning){
      toggleRunningState();
    }

    if (program == null){
      if(trDataManager.Instance.IsMissionMode 
         && trDataManager.Instance.AuthoringMissionInfo.EditState == MissionEditState.NORMAL
         &&trDataManager.Instance.MissionMng.GetCurPuzzle().IsLoadStartProgram){
        trDataManager.Instance.MissionMng.UserOverallProgress.CurMissionInfo.Program 
          = trDataManager.Instance.MissionMng.GetCurPuzzle().Hints[0].Program.DeepCopy();
      }
      CurProgram = trDataManager.Instance.GetCurProgram();

    } else {
     
      CurProgram = program;
    }

    if(CurProgram.IsFutureVersion){
      trNavigationRouter.Instance.ShowSceneWithName(trNavigationRouter.SceneName.VAULT);
      return;
    }

    if(trDataManager.Instance.MissionMng.AuthoringMissionInfo.EditState != MissionEditState.EDIT){
      StateEditCtrl.StatePanel.transform.localScale = Vector3.one;
    }
   
    ScrollRect scroll = StateEditCtrl.StatePanel.GetComponentInParent<ScrollRect>();
    scroll.horizontalNormalizedPosition = 0.5f;
    scroll.verticalNormalizedPosition = 0.5f;

    if(trDataManager.Instance.IsInFreePlayMode){
      CurProgram.RecentLoadedTime = System.DateTime.Now.ToFileTimeUtc();
      trDataManager.Instance.AppSaveInfo.SaveProgram(CurProgram);
    }   

    if(AuthoringBehaviorCtrl != null){
      AuthoringBehaviorCtrl.SetUpView();
    }
    else{
      BehaviorPanelCtrl.InitView(CurProgram, trDataManager.Instance.CurrentRobotTypeSelected);
    }

    StateEditCtrl.SetUpView(CurProgram);
    ProgramName.text = CurProgram.UserFacingName; //  + "(" + CurProgram.Version + ")";
  }

  public void LoadFunctionProgram(trProgram program){
    CurProgram = program;
    BehaviorPanelCtrl.InitView(CurProgram, trDataManager.Instance.CurrentRobotTypeSelected);
    StateEditCtrl.SetUpView(CurProgram);
    ProgramName.text = CurProgram.UserFacingName; 
    StateEditCtrl.SetRunMode(IsRunning);
  }
    

  public override bool toggleRunningState ()
  {
    if (!base.toggleRunningState ()) {
      return false;
    }
    animateSMStart (IsRunning);
   
    if (UndoRedoCtrl != null) {
      UndoRedoCtrl.gameObject.SetActive (!IsRunning);
    }
    if (TrashCanCtrl != null) {
      TrashCanCtrl.gameObject.SetActive(!IsRunning);
    }
    if (PuzzleInfoPnlCtrl!=null && trDataManager.Instance.IsMissionMode){
        PuzzleInfoPnlCtrl.gameObject.SetActive(!IsRunning);
    }         

    if(IsRunning){
      BehaviorPanelCtrl.UsageController.StopContinuesUsage();
      timeStateMachineWasStarted = Time.time;
      BackButton.gameObject.SetActive(false);
      if (trDataManager.Instance.IsMissionMode) {
        if(FTUEManager.Instance.IsInFTUE()){
          RunButton.SetActive(false);
        }
      }
      else {
        trProgram trPrg = trDataManager.Instance.GetCurProgram();
        new trTelemetryEvent(trTelemetryEventType.FP_SM_START, true)
          .add(trTelemetryParamType.NUM_STATES     , trPrg.UUIDToStateTable.Count)
          .add(trTelemetryParamType.NUM_TRANSITIONS, trPrg.UUIDToTransitionTable.Count)
          .emit();
      }
      if(_triggerCalloutPanel==null){
        _triggerCalloutPanel = InstantiatePrefab<trTutorialTriggerCalloutController>(_triggerCalloutPrefab, _triggerCalloutHolder);
      }
      _triggerCalloutPanel.gameObject.SetActive(true);
      _triggerCalloutPanel.SetupView(this);
    }
    else{
      BehaviorPanelCtrl.UsageController.StartContinuesUsage();
      if(trDataManager.Instance != null){
        trProgram trPrg = trDataManager.Instance.GetCurProgram();
        float duration = Time.time - timeStateMachineWasStarted;
        BackButton.gameObject.SetActive(true);
        if (trDataManager.Instance.IsMissionMode) {
          // TODO: define & emit CHAL_SM_STOP
          if(FTUEManager.Instance.IsInFTUE()){
            RunButton.SetActive(true);
          }
        }
        else {
          new trTelemetryEvent(trTelemetryEventType.FP_SM_STOP, true)
            .add(trTelemetryParamType.NUM_STATES     , trPrg.UUIDToStateTable.Count)
            .add(trTelemetryParamType.NUM_TRANSITIONS, trPrg.UUIDToTransitionTable.Count)
            .add(trTelemetryParamType.DURATION       , duration)
            .emit();
        }
      }
      if(_triggerCalloutPanel!=null){
        _triggerCalloutPanel.gameObject.SetActive(false);
      }
      //FTUE
      if(FTUEManager.Instance.ShouldDisplayFTUE(FTUEType.FREEPLAY_BACK_TO_LOBBY) && 
         !trDataManager.Instance.IsMissionMode){
        if(tutorial==null){
          tutorial = Instantiate(_tutorialCanvasPrefab, Vector3.zero, Quaternion.identity) as GameObject;
          tutorial.transform.SetParent(_tutorialCanvasHolder, false);
        }
        tutorial.SetActive(true);
        piConnectionManager.Instance.hideChromeButton();
        trFTUEController ftueCtrl = tutorial.GetComponent<trFTUEController>();
        new trTelemetryEvent(trTelemetryEventType.WONDER_FTUE_FINISH, true).emit();
        ftueCtrl.SetupView(FTUEType.FREEPLAY_BACK_TO_LOBBY, onLobbyButtonClicked);
      }
    }
    return true;
  }

  void animateSMStart(bool isStart){
    float animationDuration = 0.0f;
    ProgramTitlePanel.gameObject.transform.DOKill();
    if (isStart){
      animationDuration = 0.3f;
      Vector2 canvasSize = _uiMainTransform.GetComponent<RectTransform>().GetSize();
      float programTitleHeight = ProgramTitlePanel.GetComponent<RectTransform>().GetHeight();
      if(!SavedPositionsDict.ContainsKey(ProgramTitlePanel.gameObject.GetInstanceID())){
        saveObjectLocalCoordinates(ProgramTitlePanel.gameObject);
      }
      if (!trDataManager.Instance.IsMissionMode) {
        ProgramTitlePanel.gameObject.transform.DOLocalMoveY(canvasSize.y/2f + programTitleHeight, animationDuration);
      }
      moveBottom(BehaviorPanelCtrl.transform);
      moveBottom(BehaviorPanelCtrl.ContentPanel.gameObject.transform, true);  
    }
    else {
      animationDuration = 0.1f;
      moveBottom(BehaviorPanelCtrl.transform, true);
      if(SavedPositionsDict.ContainsKey(ProgramTitlePanel.gameObject.GetInstanceID())){
        ProgramTitlePanel.gameObject.transform.DOMove(getSavedLocalCoordinates(ProgramTitlePanel.gameObject), animationDuration);
      }
    }
  }

  void saveObjectLocalCoordinates(GameObject item, bool isLocalPos = false){
    if(isLocalPos){
      SavedPositionsDict[item.GetInstanceID()] = item.transform.localPosition;
    }
    else{
      SavedPositionsDict[item.GetInstanceID()] = item.transform.position;
    }
   
  }

  Vector3 getSavedLocalCoordinates(GameObject item){
    if (SavedPositionsDict.ContainsKey(item.GetInstanceID())) {
      return SavedPositionsDict[item.GetInstanceID()];
    }
    else {
      return Vector3.zero;
    }
  }

  protected override void onRobotState(piBotBase robot) {
    if(IsRunning){ // Order matters, do this before base because we want to evaluate the start state

      // evaluate the current state of the challenge completion - activation counts, etc.
      // it's important to do this before processing the incoming state,
      // because that may transition it out of the Start State, which may have an activation count > 0.
      MissionEvaluationCtrl.onRobotState();

    }
    base.onRobotState(robot);

    if (!IsRunning&& runningState != null) {
      if (runningState.Behavior != null) {
        runningState.Behavior.BehaveContinuous(robot);
      }
    }
  }

  public void DeleteBehavior(trBehaviorButtonController button){
    switch(button.BehaviorData.Type){
      case  trBehaviorType.MISSION:
        AuthoringBehaviorCtrl.DeleteBehavior(button);
        trDataManager.Instance.MissionMng.AuthoringMissionInfo.DeleteMission(button.BehaviorData.MissionFileInfo);
        break;
      case trBehaviorType.MAPSET:
        BehaviorPanelCtrl.BMakerPanel.GetComponent<trBMakerTabController>().DeleteBehavior(button);
        break;
      case trBehaviorType.FUNCTION:
        BehaviorPanelCtrl.DeleteFunction(button);
        break;
      default:
        WWLog.logError("Trying to delete a behavior that's not deletable.");
        break;
    }
  }

  public void DeleteState(trState state){
    if (state.Behavior.Type == trBehaviorType.OMNI){
      SetOmniState(null);
    }
    StateEditCtrl.DeleteState(state);
  }

  public void SetOmniState(trState state){
    if (state != CurProgram.StateOmni){
      if (trMultivariate.Instance.getOptionValue(trMultivariate.trAppOption.OMNI_DISABLE_IN_TRAY) == trMultivariate.trAppOptionValue.YES){
        processOmniStateTrayOptionYes(state);
      } else {
        processOmniStateTrayOptionNo(state);
      }
    } else {
      StateEditCtrl.StateToButtonTable[CurProgram.StateOmni].TweenCtrl.Shake();
    }
  }

  void processOmniStateTrayOptionNo(trState state){
    StateEditCtrl.SetEnableOmniStateButton(CurProgram.StateOmni,false);
    CurProgram.StateOmni = state;
    StateEditCtrl.SetEnableOmniStateButton(CurProgram.StateOmni,true);
  }

  void processOmniStateTrayOptionYes(trState state){
    if (CurProgram.StateOmni == null || state == null){
      if (state == null || state.Behavior.Type == trBehaviorType.OMNI){
        CurProgram.StateOmni = state;
        StateEditCtrl.SetEnableOmniStateButton(CurProgram.StateOmni,true);
        updateOmniStateViewInPanel();
      } else if (state != null){
        StateEditCtrl.StateToButtonTable[state].TweenCtrl.Shake();
      }
    } else {
      if (state.Behavior.Type == trBehaviorType.OMNI){
        StateEditCtrl.DeleteStateButton(state);
      } else {
        StateEditCtrl.StateToButtonTable[state].TweenCtrl.Shake();
      }
      StateEditCtrl.StateToButtonTable[CurProgram.StateOmni].TweenCtrl.Shake();
    }
  }

  void updateOmniStateViewInPanel(){
    foreach(GameObject item in BehaviorPanelCtrl.MiscPanel.Children){
      trBehaviorButtonController buttonController = item.GetComponent<trBehaviorButtonController>();
      if (buttonController.BehaviorData.Type == trBehaviorType.OMNI){
        float alpha = (CurProgram.StateOmni == null ? 1.0f : 0.7f);
        buttonController.Image.color = wwColorUtil.ColorWithAlpha(buttonController.Image.color, alpha);
        break;
      }
    }
  }

  void updateBehaviourEnbaled(string uuid, int countInMission){
    foreach(trBehavior behaviour in BehaviorPanelCtrl.BehaviorToButtonDic.Keys){
      if (behaviour.UUID == uuid){
        trBehaviorButtonController button = BehaviorPanelCtrl.BehaviorToButtonDic[behaviour];
        button.SetEnabled(countInMission > 0);
        break;
      }
    }
  }

  public void ShowExceedTransitionsDialog(){
    trAlertDialogController dialogCtrl = InstantiatePrefab<trAlertDialogController>(_alertDialogPrefab, _alertDialogHolder);
    dialogCtrl.TitleText.text = wwLoca.Format("@!@Cannot add transition@!@");
    dialogCtrl.DescriptionText.text = wwLoca.Format("@!@Only <b>{0}</b> transitions are allowed per state.  Please <b>remove</b> an existing transition or <b>modify</b> another state.@!@", trToFirmware.cOutgoingTransitionsPerState);
  }

  void resolveCurrentMission(){
    LoadProgram(trDataManager.Instance.MissionMng.GetTargetProgram());
    trDataManager.Instance.SaveCurProgram();
    toggleRunningState();
  }

  private static string[] successAnimations_dash = { "dash_wonder_finishChallenge_01", 
                                                "dash_wonder_finishChallenge_02",
                                                "dash_wonder_finishChallenge_03", 
                                                "dash_wonder_finishChallenge_04",
                                              };

  private static string[] successAnimations_dot = { "dot_wonder_finishChallenge_01", 
                                                "dot_wonder_finishChallenge_02",
                                                "dot_wonder_finishChallenge_03", 
                                                "dot_wonder_finishChallenge_04",
                                              };

  private static string[] failureAnimations_dash = { "dash_wonder_tryAgain_01", 
                                                  "dash_wonder_tryAgain_02",
                                                  "dash_wonder_tryAgain_03", 
                                                  "dash_wonder_tryAgain_04",
                                              };

  private static string[] failureAnimations_dot = { "dot_wonder_tryAgain_01", 
                                                  "dot_wonder_tryAgain_02",
                                              };
 
  private static string[] missionSuccessAnimations_dash =  {  
                                                  "dash_wonder_missionSuccess_01",
                                                  "dash_wonder_missionSuccess_02", 
                                                  "dash_wonder_missionSuccess_03",
                                              };


  public void PlayFailureAnimation(){
    string[] anims = (CurrentRobotType == piRobotType.DOT ? failureAnimations_dot : failureAnimations_dash);    
    playRandomAnimation(anims);
  }
  public void PlaySuccessAnimation(){
    string[] anims = (CurrentRobotType == piRobotType.DOT ? successAnimations_dot : successAnimations_dash);    
    playRandomAnimation(anims);
  }
  public void PlayCompletedAnimation(){
    string[] anims = (CurrentRobotType == piRobotType.DOT ? successAnimations_dot : missionSuccessAnimations_dash);
    playRandomAnimation(anims);
  }

  private void playRandomAnimation(string[] listOfAnimations) {
    if (CurRobot != null) {
      piBotBo bot = (piBotBo)CurRobot;
      int randomIndexForSuccessAnimation = new System.Random().Next(listOfAnimations.Length);
      string animationToPlay = listOfAnimations[randomIndexForSuccessAnimation];
      WWLog.logDebug("playing animation: " + animationToPlay);
      string animJson = trMoodyAnimations.Instance.getJsonForAnim(animationToPlay);
      bot.cmd_startSingleAnim(animJson);
    }
  }

  public enum RunMode {
    FreePlay = 0, 
    Challenges = 1
  }

  #region AutoHide UI

  void showBehaviorPanels(bool instant){
    if (AllowedScrollRect != null) {
      RectTransform rt = AllowedScrollRect.GetComponent<RectTransform>();
      rt.offsetMin = new Vector2(rt.offsetMin.x, 110f);
    }
    moveBottom(BehaviorPanelCtrl.ContentPanel, true, instant);
  }

  void hideBehaviorPanels(bool instant){
    if (AllowedScrollRect != null) {
      RectTransform rt = AllowedScrollRect.GetComponent<RectTransform>();
      rt.offsetMin = new Vector2(rt.offsetMin.x, 30f);
    }
    moveBottom(BehaviorPanelCtrl.ContentPanel, instant:instant);
  }

  void moveBottom(Transform item, bool reverse=false, bool instant=false){

    item.DOKill();

    float animationDuration;
    if (reverse){
      if (SavedPositionsDict.ContainsKey(item.gameObject.GetInstanceID())){
        animationDuration = instant ? 0 : 0.1f;
        item.DOLocalMove(getSavedLocalCoordinates(item.gameObject), animationDuration);
        if (!instant) {
          bool isAlreadyRevealed = Mathf.Approximately(getSavedLocalCoordinates(item.gameObject).y, item.localPosition.y);
          if (!isAlreadyRevealed) {
            SoundManager.soundManager.PlaySound(SoundManager.trAppSound.SM_TRAY_REVEAL);
          }
        }
      }
    } else {
      animationDuration = instant ? 0 : 0.3f;
        
      float panelHeight = item.GetComponent<RectTransform>().GetSize().y;
      // only record this the first time moving down so it's not recording wrong position
      // eg, when it's in the middle of moving down and it's asked to moving down again, it's gonna record a wrong position
      if (!SavedPositionsDict.ContainsKey(item.gameObject.GetInstanceID())){ 
        saveObjectLocalCoordinates(item.gameObject, true);
      }
      item.DOLocalMoveY(getSavedLocalCoordinates(item.gameObject).y-panelHeight *1.5f  , animationDuration);
    }
  }
  #endregion

  #region actuator tests
  private List<trActuator> actuators = null;
    
  private List<trActuator> Actuators {
    get {
      if (actuators == null) {
        actuators = new List<trActuator>();
        
        actuators.Add(new trActuator(trActuatorType.HEAD_PAN));
        actuators.Add(new trActuator(trActuatorType.HEAD_TILT));
        actuators.Add(new trActuator(trActuatorType.WHEEL_L));
        actuators.Add(new trActuator(trActuatorType.WHEEL_R));
        actuators.Add(new trActuator(trActuatorType.LED_TAIL));
        actuators.Add(new trActuator(trActuatorType.LED_TOP));
        actuators.Add(new trActuator(trActuatorType.RGB_ALL_HUE));
        actuators.Add(new trActuator(trActuatorType.RGB_ALL_VAL));
//        actuators.Add(new trActuator(trActuatorType.POSE_UNKNOWN));
//        actuators.Add(new trActuator(trActuatorType.EYERING));
      }
      return actuators;
    }
  }
  
  void actuatorTestUpdate() {
  #if false
    if (CurRobot != null) {
      float f = Mathf.Sin(Time.time * (2.0f * Mathf.PI) * 0.2f) * 0.5f + 0.5f;
      foreach(trActuator act in Actuators) {
        act.Robot = (piBotBo)CurRobot;
        act.ValueNormalized = f;
      }
    }
  #endif
  }
  #endregion
  
#if UNITY_EDITOR  
  [UnityEditor.Callbacks.DidReloadScripts]
  private static void Test() {

    #if false
    trProgram tp1 = trProgram.Example1;
    string jss1 = tp1.ToJson().ToString();
    WWLog.logError("example program 1: " + jss1);
    
    trFactory.ForgetItems();
    JSONClass jsc2 = (JSONClass)JSON.Parse(jss1);
    trProgram tp2 = trFactory.FromJson<trProgram>(jsc2);
    string jss2 = tp2.ToJson().ToString();
    WWLog.logError("example program 2: " + jss2);
    #endif
  }
#endif

#region ElementInfoPanel

  // returns false iff the info panel is already open for this element
  public bool TryShowElementInfo(trState element) {
    return TryShowElementInfo(new trElementInfo(element));
  }

  public bool TryShowElementInfo(trTransition element) {
    return TryShowElementInfo(new trElementInfo(element));
  }

  public bool TryShowElementInfo(trElementInfo element) {
    if(_elementInfoPanel==null){
      _elementInfoPanel = InstantiatePrefab<trElementInfoPanelController>(_elementInfoPanelPrefab, _elementInfoPanelHolder);
      _elementInfoPanel.SetupView(this);
      _elementInfoPanel.clear();
      AddNewElementInfoItem(element);
      return true;
    }

    if (_elementInfoPanel.isShowing(element)) {
      // This is the currently-shown item.
      // Open full detail panel.
      HideElementInfo();
      return false;
    }
    else {
      if (_elementInfoPanel.isListing(element)) {
        // it's in the list of elements, but it's not the currently-expanded one.
        // select it.
        _elementInfoPanel.selectElement(element);
      }
      else {
        // the panel is closed, or this item is not in the panel at all.
        // populate & open the panel.
        _elementInfoPanel.gameObject.SetActive(true);
        _elementInfoPanel.clear();
        AddNewElementInfoItem(element);
      }
      return true;
    }
  }

  private void AddNewElementInfoItem(trElementInfo element){
    if (trElementInfoPanelController.behaviorOnPointerUp == trElementInfoPanelController.BehaviorOnPointerUp.STAY_OPEN) {
        if (element.IsTransition) {
          foreach (trTransition trTrn in element.Transition.getCohort()) {
            if (trTrn != element.Transition) {
              _elementInfoPanel.addNewItem(new trElementInfo(trTrn));
            }
          }
        }
      }
      _elementInfoPanel.addNewItem(element);
  }

  public void ShowElementInfoRing(trElementInfo element) {
    if (element == null) {
      ShowElementInfoRing((Transform)null);
    }
    else if (element.IsState) {
      ShowElementInfoRing(element.State);
    }
    else if (element.IsTransition) {
      ShowElementInfoRing(element.Transition);
    }
    else {
      WWLog.logError("unhandled element type: " + element.Name);
    }
  }

  private void ShowElementInfoRing(trState element) {
    trStateButtonController trSBC = StateEditCtrl.ButtonByState(element);
    if (trSBC == null) {
      WWLog.logError("no such state: " + element.ToString());
      return;
    }

    ShowElementInfoRing(trSBC.RunFocus.transform);
  }

  private void ShowElementInfoRing(trTransition element) {
    trTransitionArrowController trTAC = StateEditCtrl.ArrowByTransition(element);
    if (trTAC == null) {
      ShowElementInfoRing((Transform)null);
      // error already printed
      return;
    }

    trTriggerButtonViewHolder trTBVH = trTAC.ButtonViewHolderByTransition(element);
    if (trTBVH == null) {
      ShowElementInfoRing((Transform)null);
      // error already printed
      return;
    }

    ShowElementInfoRing(trTBVH.BehaviourImage.transform, 0.7f, trTAC.transform);
  }

  private void ShowElementInfoRing(Transform centerOnThis, float scale = 1f, Transform parent = null) {
    if ( _elementInfoRing != null && trElementInfoPanelController.behaviorOnPointerUp == trElementInfoPanelController.BehaviorOnPointerUp.STAY_OPEN) {
      if (centerOnThis == null) {
        // not necessarily an error. this is also how we intentionally hide the ring.
        _elementInfoRing.SetActive(false);
        // We set paraent to null so that this doesn't get destroyed for being the child of something else that gets destroyed.
        _elementInfoRing.transform.SetParent(null);
      }
      else {
        Transform prnt = (parent == null ? centerOnThis.parent : parent);
        _elementInfoRing.SetActive(true);
        _elementInfoRing.transform.SetParent(prnt);
        _elementInfoRing.transform.position   = centerOnThis.position;
        _elementInfoRing.transform.localScale = Vector3.one * scale;
      }
    }
  }

  public void HideElementInfo() {
    if (_elementInfoPanel != null) {
      _elementInfoPanel.gameObject.SetActive(false);
    }
    ShowElementInfoRing((Transform)null);
  }


  private static void addPossiblyNullGameObject(HashSet<GameObject> hs, MonoBehaviour mb) {
    if (mb != null) {
      hs.Add(mb.gameObject);
    }
  }

  private HashSet<GameObject> modalPanels = null;
  private HashSet<GameObject> ModalPanels {
    get {
      if (modalPanels == null) {
        modalPanels = new HashSet<GameObject>();
        addPossiblyNullGameObject(modalPanels, TriggerConfigurePanel);
        addPossiblyNullGameObject(modalPanels, VideoPanelCtrl);
        addPossiblyNullGameObject(modalPanels, _narrativeUICtrl);
        addPossiblyNullGameObject(modalPanels, SoundConfigurePanel);
        addPossiblyNullGameObject(modalPanels, AnimationConfigurePanel);
        addPossiblyNullGameObject(modalPanels, _puzzleCompletePanel);
      }
      return modalPanels;
    }
  }

  public bool IsAnyModalPanelOpen {
    get {
      bool ret = false;
      foreach (GameObject go in ModalPanels) {
        ret = ret || go.activeInHierarchy;
      }

      // these ones can't be put in a table, unfortunately

      ret = ret || ( (PuzzleInfoPnlCtrl != null) && (PuzzleInfoPnlCtrl.HintPanelCtrl != null) && (PuzzleInfoPnlCtrl.HintPanelCtrl.gameObject.activeInHierarchy) );

      return ret;
    }
  }

#endregion
}


