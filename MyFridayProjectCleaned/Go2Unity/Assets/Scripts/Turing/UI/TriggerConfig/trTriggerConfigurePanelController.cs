using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using WW.UGUI;
using DG.Tweening;
using WW;
using TMPro;

namespace Turing{
  public class trTriggerConfigurePanelController : uGUISegmentedController {

    private trTransitionArrowController curTransCtrl;

    private trTransition activeTransition;

    public GameObject TriggerButtonPrefab;

    public TextMeshProUGUI NameText;
    public TextMeshProUGUI DescriptionText;

    public trStateButtonController FromStateButton;
    public trStateButtonController ToStateButton;

    public RectTransform SelectionPanel;
    public uGUIAnimatescrollRect ScrollPanel;

    public Button DeleteButton;
    public Button OKButton;
    public Button ButtonCancel;

    public Button VideoButton;
    public TextMeshProUGUI VideoText;

    public Dictionary<trTriggerType, trTriggerButtonController> triggerTypeToButtonTable = new Dictionary<trTriggerType, trTriggerButtonController>();

    public trTriggerParameterPanelController TriggerParaCtrl;

    public trTriggerToggleGroup DistanceConfigCtrl;
    public trTriggerToggleGroup BeaconConfigCtrl;
    
    public trTriggerConfigCustomControllerBase BeaconV2ConfigCtrl;

    public Text NoTriggerAvailableText;

    public RectTransform AllCategoriesBG;
    public GameObject TriggerCategoryPrefab;

    private Dictionary<trTriggerArea, trTriggerCategoryController> areaToCategoryTable = new Dictionary<trTriggerArea, trTriggerCategoryController>();

    private bool isNoTriggerAvailable = false;
    
    private trTriggerToggleGroup activeToggleGroup;
    private trTriggerConfigCustomControllerBase activeCustomController;

    public delegate void TriggerConfigDelegate(trTrigger trigger, bool success);
    public TriggerConfigDelegate OnDismiss;
    
    private trProtoController _protoController;

    public GameObject WarningPanel;
    public TextMeshProUGUI WarningText;

    private List<trTriggerType> triggerOrderList = null;
    private List<trTriggerType> triggerListOrdered {
      get{
        if(triggerOrderList != null){
          return triggerOrderList;
        }
        triggerOrderList = new List<trTriggerType>() {
          trTriggerType.BEHAVIOR_FINISHED,
          trTriggerType.IMMEDIATE,
          trTriggerType.BUTTON_MAIN,
          trTriggerType.CLAP,   
          
          trTriggerType.TIME,
          trTriggerType.BUTTON_1,
          trTriggerType.BUTTON_2,
          trTriggerType.BUTTON_3,
          trTriggerType.DISTANCE_SET,
          trTriggerType.VOICE,
          trTriggerType.LEAN_LEFT,
          trTriggerType.LEAN_RIGHT,
          trTriggerType.LEAN_FORWARD,
          trTriggerType.LEAN_BACKWARD,
          trTriggerType.LEAN_UPSIDE_DOWN,
          trTriggerType.LEAN_UPSIDE_UP,
          
          trTriggerType.RANDOM,
          trTriggerType.BEACON_SET,
          trTriggerType.BEACON_V2,
          trTriggerType.DROP,
          trTriggerType.SHAKE,
          trTriggerType.TIME_RANDOM,
          trTriggerType.TIME_LONG,
          trTriggerType.KIDNAP,
          trTriggerType.KIDNAP_NOT,
          trTriggerType.STALL,
          trTriggerType.STALL_NOT,
          trTriggerType.SLIDE_X_POS,
          trTriggerType.SLIDE_X_NEG,
          trTriggerType.SLIDE_Y_POS,
          trTriggerType.SLIDE_Y_NEG,
          trTriggerType.SLIDE_Z_POS,
          trTriggerType.SLIDE_Z_NEG,
          
          trTriggerType.TRAVELING_FORWARD,
          trTriggerType.TRAVELING_BACKWARD,
          trTriggerType.TRAVELING_STOPPED,
          trTriggerType.BUTTON_MAIN_NOT,
          trTriggerType.BUTTON_1_NOT,
          trTriggerType.BUTTON_2_NOT,
          trTriggerType.BUTTON_3_NOT,
          
          trTriggerType.TRAVEL_LINEAR,
          trTriggerType.TRAVEL_ANGULAR,
          trTriggerType.BUTTON_ANY,
          trTriggerType.BUTTON_NONE
        };

        foreach(trTriggerType type in System.Enum.GetValues(typeof(trTriggerType))){
          if (trTrigger.ShowToUser(type, trDataManager.Instance.CurrentRobotTypeSelected)
              &&!triggerOrderList.Contains(type)){
              WWLog.logError("Cannot find trigger type " + type +" in ordered list. Please fix. ");
          }    
        }


        return triggerOrderList;
      }
    }

    public void SetConfiguration(trTransitionArrowController controller, trTransition transition, bool isCreateTransition = false){
      curTransCtrl = controller;
      activeTransition = transition;
      SetUpView(isCreateTransition);
    }

    void Start(){
      //DeleteButton.onClick.AddListener(()=> onDeleteButtonClicked());
      OKButton.onClick.AddListener(()=>OnBackGroundCLicked());

      if(VideoButton != null){
        VideoButton.onClick.AddListener(onVideoButtonClicked);
      }
      VideoText.text = wwLoca.Format("@!@Need more help?\nWatch tutorial@!@");
    
      piConnectionManager.Instance.OnConnectRobot += HandleRobotConnectionChanged;
      piConnectionManager.Instance.OnDisconnectRobot += HandleRobotConnectionChanged;
    }

    void onVideoButtonClicked(){
      trTrigger trigger = ((trTriggerButtonController)activatedSegment).TriggerData;
      trVideoInfo video = trDataManager.Instance.VideoManager.triggerVideos[trigger.Type];
      _protoController.VideoPanelCtrl.SetActive(true);
      _protoController.VideoPanelCtrl.Play(video.FileName, trigger.Type.ToString());
    }

    public void OnDestroy(){
      if (piConnectionManager.Instance){
        piConnectionManager.Instance.OnConnectRobot -= HandleRobotConnectionChanged;
        piConnectionManager.Instance.OnDisconnectRobot -= HandleRobotConnectionChanged;
      }
    }

    void HandleRobotConnectionChanged (piBotBase robot) {
      regenerateTriggersView(robot);
    }

    void onDeleteButtonClicked(){
      if(curTransCtrl != null){
        curTransCtrl.RemoveTransition(activeTransition);
        curTransCtrl = null;
        activeTransition = null;
        closePanel();
      }else{
        WWLog.logWarn("transition ctrl not set up while trying deleting");
      }
    }

    void resetTriggerData(){
      foreach(trTriggerType type in triggerTypeToButtonTable.Keys){
        if(trTrigger.Parameterized(type)){
          triggerTypeToButtonTable[type].TriggerData.ParameterValue = float.NaN;
        }
        if(type.IsTriggerSet()){
          triggerTypeToButtonTable[type].TriggerData.TriggerSet.ClearTriggers();
        }
      }
    }

    public void SetUpView(bool isCreateTransition = false){
      if(curTransCtrl == null){
        return;
      }
      
      if(activeTransition == null){
        WWLog.logError("Trying to set up a trigger for a non-existed transition");
        return;
      }


      resetTriggerData();

      trTrigger trigger = activeTransition.Trigger;

      bool showWarningImg = activeTransition.StateSource.Behavior.ShowMicrophoneWarning();
      if(triggerTypeToButtonTable.ContainsKey(trTriggerType.CLAP)){
        triggerTypeToButtonTable[trTriggerType.CLAP].WarningImage.gameObject.SetActive(showWarningImg);
      }
      if(triggerTypeToButtonTable.ContainsKey(trTriggerType.VOICE)){
        triggerTypeToButtonTable[trTriggerType.VOICE].WarningImage.gameObject.SetActive(showWarningImg);
      }
        
      //only enable selecting triggers that are not used for the source state
      trState sourceState = activeTransition.StateSource;
      foreach(trTriggerButtonController button in triggerTypeToButtonTable.Values){
        button.IsEnabled = true;
      }


      for(int i =0; i< sourceState.OutgoingTransitions.Count; ++i){
        trTrigger tmpTr = sourceState.OutgoingTransitions[i].Trigger;
        if(tmpTr != null && !trTrigger.AllowMultiple(tmpTr.Type)){
          if(!tmpTr.Type.IsTriggerSet() && tmpTr.Type != trigger.Type&& triggerTypeToButtonTable.ContainsKey(tmpTr.Type)){
            triggerTypeToButtonTable[tmpTr.Type].IsEnabled = false;
          }            
        }
      }
      
      // special logics
      // TUR-633: OMNI state doesn't have Auto.
      // TUR-931: nor IMMEDIATE.
      if (sourceState.Behavior.Type == trBehaviorType.OMNI) {
        if(triggerTypeToButtonTable.ContainsKey(trTriggerType.BEHAVIOR_FINISHED)){
          triggerTypeToButtonTable[trTriggerType.BEHAVIOR_FINISHED].IsEnabled = false;
        }
        if(triggerTypeToButtonTable.ContainsKey(trTriggerType.IMMEDIATE)){
          triggerTypeToButtonTable[trTriggerType.IMMEDIATE        ].IsEnabled = false;
        }
      }


      int availableTriggerCount = 0;
      foreach(trTriggerButtonController button in triggerTypeToButtonTable.Values){
        if(button.IsEnabled && !button.TriggerData.isLocked()){
          availableTriggerCount ++;
        }
      }
      isNoTriggerAvailable = availableTriggerCount == 0;
      NoTriggerAvailableText.gameObject.SetActive(isNoTriggerAvailable);    

      bool isAbleToSetDefaultTrigger = false;
      if(trigger.Type != trTriggerType.NONE){
        if(trTrigger.Parameterized(trigger.Type)){
          triggerTypeToButtonTable[trigger.Type].TriggerData.ParameterValue = trigger.ParameterValue;
        }
        if(trigger.Type.IsTriggerSet()){
          triggerTypeToButtonTable[trigger.Type].TriggerData.TriggerSet.CopyValue(trigger.TriggerSet);
        }
        if (triggerTypeToButtonTable.ContainsKey(trigger.Type)){
          ActivateSegment(triggerTypeToButtonTable[trigger.Type]);
          ScrollPanel.ScrollToElement(triggerTypeToButtonTable[trigger.Type].transform);
        }
      }
      else{
        trTriggerButtonController activatedButton = null;
        if(!trDataManager.Instance.IsMissionMode){
          trTriggerButtonController button = triggerTypeToButtonTable[trTriggerType.BEHAVIOR_FINISHED];
          if(button.IsEnabled){
            activatedButton = button;
            isAbleToSetDefaultTrigger = true;
          }
        }
        if(activatedButton == null){
          foreach(trTriggerButtonController button in triggerTypeToButtonTable.Values){
            if(button.IsEnabled && !button.TriggerData.isLocked()){
              activatedButton = button;
              break;
            }
          }
        }
        if(activatedButton != null){
          ActivateSegment(activatedButton);
          ScrollPanel.ScrollToElement(activatedButton.transform);
        }

      }

      if(isCreateTransition && (availableTriggerCount <= 1 || isAbleToSetDefaultTrigger)){
        closePanel();
      }
      else{
        SetActive(true, isCreateTransition);
      }


    }
  
    public override void ActivateSegment (uGUISegment seg) {
      trTrigger trigger = ((trTriggerButtonController)seg).TriggerData;

      // hide the previous toggle group UI
      if (activeToggleGroup != null) {
        activeToggleGroup.SetActive(false);
        activeToggleGroup = null;
      }
      
      if (activeCustomController != null) {
        activeCustomController.gameObject.SetActive(false);
        activeCustomController = null;
      }

      if(VideoButton != null){
        VideoButton.gameObject.SetActive(false);
      }


      if (trigger.isLocked()){
        NameText.text = wwLoca.Format("@!@Locked Cue@!@");
        DescriptionText.text = wwLoca.Format("@!@Play and solve more challenges to unlock this Cue!@!@");
      }
      else {
        if(VideoButton != null){
          bool hasVideo = trDataManager.Instance.VideoManager.triggerVideos.ContainsKey(trigger.Type);
          #if UNITY_ANDROID
          hasVideo = false;
          #endif
          VideoButton.gameObject.SetActive(hasVideo);
        }
       

        // trigger is unlocked, do our thing!
        TriggerParaCtrl.CurTrigger = trigger;

        if (trigger.Type.IsTriggerSet()){
          activeToggleGroup = getToggleGroup(trigger);
          //bool isDefault = seg != activatedSegment && trigger.Type != CurTransitionCtrl.TransitionData.Trigger.Type;
          activeToggleGroup.SetActive(true);
          activeToggleGroup.SetUp(activeTransition, trigger) ;
        }
        else {
          activeCustomController = getCustomConfigController(trigger);
          if (activeCustomController != null) {
            activeCustomController.SetUp(activeTransition, trigger);
            activeCustomController.gameObject.SetActive(true);
            activeCustomController.ProtoController = _protoController;
          }
        }


//      ButtonCancel.gameObject.SetActive(activeToggleGroup != null);
        if (trigger.Type == trTriggerType.DISTANCE_SET){
          NameText.text = ""; 
        } 
        else {
          NameText.text = trigger.UserFacingNameLocalized;
        }
        DescriptionText.text = trigger.DescriptionLocalized;

        if (trigger.Type.IsMicrophone()) {
          trBehavior trB = activeTransition.StateSource.Behavior;
          string warningMsg = trB.microphoneWarning(wwLoca.Format(trDataManager.Instance.CurrentRobotTypeSelected == piRobotType.DASH ? "Dash" : "Dot"));
          WarningText.text = warningMsg;
          WarningPanel.SetActive(trB.ShowMicrophoneWarning());
        }
        else if(WarningPanel != null){
          WarningPanel.SetActive(false);
        }

        base.ActivateSegment(seg);
      }
    }

    protected override void OnDoubleClickOnSegment () {
      closePanel();
    }

    trTriggerToggleGroup getToggleGroup(trTrigger trigger){
      if(!trigger.Type.IsTriggerSet()){
        WWLog.logError("Invalid trigger type");
        return null;
      }
      switch(trigger.Type){
      case trTriggerType.DISTANCE_SET:
        return DistanceConfigCtrl;
      case trTriggerType.BEACON_SET:
        return BeaconConfigCtrl;
      }
      return null;
    }
    
    trTriggerConfigCustomControllerBase getCustomConfigController(trTrigger trT) {
      switch(trT.Type) {
        default:
          return null;
        case trTriggerType.BEACON_V2:
          return BeaconV2ConfigCtrl;
      }
    }

    public void OnBackGroundCLicked(){

      closePanel();
    }

    public void closePanel(){
      if (DistanceConfigCtrl != null) {
        DistanceConfigCtrl.SetActive(false);
      }
      if (BeaconConfigCtrl != null) {
        BeaconConfigCtrl.SetActive(false);
      }
      if (BeaconV2ConfigCtrl != null) {
        BeaconV2ConfigCtrl.gameObject.SetActive(false);
      }

      if (curTransCtrl != null){
        if(isNoTriggerAvailable){
          curTransCtrl.OnDeleteButtonClicked(activeTransition);
          curTransCtrl = null;
          SetActive(false);
          return;
        }
      }

      if(activatedSegment != null && curTransCtrl != null&& activeTransition != null){
        trTriggerButtonController trigger = (trTriggerButtonController)activatedSegment;
        if(activeTransition.Trigger == null){
          activeTransition.Trigger = new trTrigger();           
        }
        
        if (!trDataManager.Instance.IsMissionMode) {
          new trTelemetryEvent(trTelemetryEventType.FP_SET_CUE, true)
            .add(trTelemetryParamType.ROBOT_TYPE, trDataManager.Instance.GetCurProgram().RobotType)
            .add(trTelemetryParamType.TYPE, trigger.TriggerData.Type)
            .add(trTelemetryParamType.TYPE_PREV, activeTransition.Trigger.Type)
            .add(trTelemetryParamType.DETAIL, trigger.TriggerData.TelemetryDetail())
            .emit();
        }

        if(!(activeTransition.Trigger.IsSameTo(trigger.TriggerData))){
          activeTransition.Trigger.CopyValue(trigger.TriggerData);
          trDataManager.Instance.SaveCurProgram();
          curTransCtrl.SetupTransitionView(activeTransition);
          _protoController.StateEditCtrl.UpdateUndoRedoUserAction();
        }

      }
      
      SetActive(false);
    }

    public void SetActive(bool enabled, bool isCreateTransition = false){
      if(enabled){
        float delay = isCreateTransition ? 0.3f : 0;
        uGUIPanelTween.Instance.TweenOpen(this.gameObject.transform, delay);
      }
      else{
        uGUIPanelTween.Instance.TweenClose(this.gameObject.transform);
        if (OnDismiss != null){
          OnDismiss(activeTransition.Trigger, true);
        }
      }
     
    }
    
    public void InitView(trProtoController protoCtrl){
      _protoController = protoCtrl;
      regenerateTriggersView(piConnectionManager.Instance.FirstConnectedRobot);

      //disable state button interactivaty
//      CanvasGroup tmp = FromStateButton.gameObject.AddComponent<CanvasGroup>();
//      tmp.interactable = false;
//      tmp.ignoreParentGroups = true;
//      FromStateButton.TransitionContainer.gameObject.SetActive(false);
//
//      tmp = ToStateButton.gameObject.AddComponent<CanvasGroup>();
//      tmp.interactable = false;
//      tmp.ignoreParentGroups = true;
//      ToStateButton.TransitionContainer.gameObject.SetActive(false);
    }

    private void regenerateTriggersView(piBotBase robot){

      foreach (var item in areaToCategoryTable.Values){
        Destroy(item.gameObject);
      }
      triggerTypeToButtonTable.Clear();
      areaToCategoryTable.Clear();

      if(trDataManager.Instance.IsMissionMode && trDataManager.Instance.MissionMng.AuthoringMissionInfo.EditState == MissionEditState.NORMAL){
        for(int i = 0; i< trDataManager.Instance.MissionMng.GetCurPuzzle().Hints.Count; ++i){
          trTriggerType[] triggers = getOrderedTriggerListFromProgram(trDataManager.Instance.MissionMng.GetCurPuzzle().Hints[i].Program); 
          for(int j = 0; j< triggers.Length; ++j){
            if(triggers[j] != 0){
              addTrigger(triggers[j]);
            }
          }

        }

        trTriggerType[] triggersTmp = getOrderedTriggerListFromProgram(trDataManager.Instance.GetCurProgram());       
        for(int i = 0; i< triggersTmp.Length; ++i){
          if(triggersTmp[i] != 0){
            addTrigger(triggersTmp[i]);
          }
        }
      }
      else{
        for(int i = 0; i< triggerListOrdered.Count; ++i) {
          if (trTrigger.ShowToUser(triggerListOrdered[i], trDataManager.Instance.CurrentRobotTypeSelected)) {
            addTrigger(triggerListOrdered[i]);
          }
        }
      }
     
      fixLayout();
    }

    private trTriggerType[] getOrderedTriggerListFromProgram(trProgram program){
      trTriggerType[] ret = new trTriggerType[triggerListOrdered.Count];
      foreach(trTransition trans in program.UUIDToTransitionTable.Values){
        trTriggerType type = trans.Trigger.Type;
        if(!triggerListOrdered.Contains(type)){
          WWLog.logError("Cannot find type " + type + " in ordered list.");
          continue;
        }
        int id = triggerListOrdered.IndexOf(type);
        ret[id] = type;
      }
      return ret;
    }


    private trTriggerCategoryController getCategoryCtrl(trTriggerType type){
      trTriggerArea area = type.GetArea();
      if (area.IsShowToUser()) {
        if(!areaToCategoryTable.ContainsKey(area)){
          GameObject newCategory = Instantiate(TriggerCategoryPrefab, TriggerCategoryPrefab.transform.position, Quaternion.identity) as GameObject;
          newCategory.transform.SetParent(AllCategoriesBG.transform, false);
          areaToCategoryTable.Add(area, newCategory.GetComponent<trTriggerCategoryController>());
          newCategory.GetComponent<trTriggerCategoryController>().Name.text = area.UserFacingName();
        }
        return areaToCategoryTable[area];
      }
      else {
        return null;
      }
    }

    private void fixLayout(){
      AllCategoriesBG.SetHeight(0);
      foreach(trTriggerArea area in System.Enum.GetValues(typeof(trTriggerArea))){
        if(areaToCategoryTable.ContainsKey(area)){
          areaToCategoryTable[area].transform.localPosition = new Vector3(areaToCategoryTable[area].transform.localPosition.x,
                                                                          - AllCategoriesBG.GetHeight(),
                                                                          areaToCategoryTable[area].transform.localPosition.z);
          AllCategoriesBG.SetHeight(AllCategoriesBG.GetHeight() + areaToCategoryTable[area].GetHeight());
        }
      }
    }


    void addTrigger(trTriggerType type){
      if(triggerTypeToButtonTable.ContainsKey(type)){
        return;
      }
      if (trTrigger.ShowToUser(type, trDataManager.Instance.CurrentRobotTypeSelected)) {
        trTrigger newTrigger = new trTrigger(type);
        newTrigger.UUID = "TRIGGER_" + type.ToString();

        trTriggerCategoryController categoryCtrl = getCategoryCtrl(type);
        if (categoryCtrl != null) {
          GameObject newTriggerButton = Instantiate(TriggerButtonPrefab, TriggerButtonPrefab.transform.position, Quaternion.identity) as GameObject;
          newTriggerButton.transform.SetParent(categoryCtrl.GridParent.transform, false);
          trTriggerButtonController ctrl =  newTriggerButton.GetComponent<trTriggerButtonController>();
          ctrl.TriggerData = newTrigger;
          ctrl.SegmentsController = this;
          AddSegment(ctrl);
          triggerTypeToButtonTable.Add(newTrigger.Type, ctrl);
        }
        else {
          // not an error. it may be a do-not-show-the-user thing.
        }
      }
    }
  }
}
