using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

//TODO: Refactor trMissionAreaButtonController, trMissionButtonController.
//      Only trMissionListController needs to have the info on the mssions/challenges.
//      Other controllers just handle the ui visualization.
namespace Turing{
  public class trMissionListController : trUIController {

    [SerializeField]
    private PinchZoom _zoomCtrl;
    [SerializeField]
    private GameObject _mapPanel;

    [Header("Dynamic Panels")]
    [SerializeField]
    private GameObject _robotConnectionPrefab;
    [SerializeField]
    private Transform _robotConnectionHolder;
    [SerializeField]
    private GameObject _shareItemDetailPanelPrefab;
    [SerializeField]
    private Transform _shareItemDetailPanelHolder;
    private trSharedItemDetailPanelController _shareItemDetailPanel;
    [SerializeField]
    private GameObject _tutorialCanvasPrefab;

    [Header("Restart Dialog")]
    public GameObject RestartDialog;
    public Button RestartButton;
    public Button LoadAndPlayButton;
    public Button RestartDialogBackButton;
    public Button RestartDialogCancelButton;
    public TextMeshProUGUI RestartText;
    public TextMeshProUGUI MissionNameLabel;

    [Header("Mission List")]
	  public trChallengeListController MissionList;
	  public GameObject MissionButtonPrefab;
    public GameObject MissionListPrefab;
    public Transform ListParent;
    public ScrollRect ScrollCtrl;

    [Header("Mission Area")]
    public GameObject MissionAreaPrefab;
    public List<GameObject> MissionAreaContainers = new List<GameObject>(); //Do not change the order of gameobjects in this list

    private Dictionary<trState, trMissionButtonController> stateToButtonTable = new Dictionary<trState, trMissionButtonController>();
    private Dictionary<trListItemControl, trState> buttonToStateTable = new Dictionary<trListItemControl, trState >();
    private Dictionary<trMapAreaType, trMissionAreaButtonController> areaToButtonTable = new Dictionary<trMapAreaType, trMissionAreaButtonController>() ;
    private Dictionary<trMapAreaType, RectTransform> areaToListTable = new Dictionary<trMapAreaType, RectTransform>();
    private trMapAreaType recentMapArreaType;
    private RectTransform curList = null;
    private bool lateUpdateListPosition = false;

    private void Start () {
      GameObject obj = Instantiate(_robotConnectionPrefab, Vector3.zero, Quaternion.identity) as GameObject;
      obj.transform.SetParent(_robotConnectionHolder, false);
      SoundManager.StartOnce(SoundManager.trAppSound.MAP_BOUNCE);
      trDataManager.Instance.Init();
      SetupView();
      trDataManager.Instance.OnNewMissionUnlocked += updateMissionView;
      RestartDialog.AddComponent<trUIController>().BackBtnCallback = onCancelButtonClcked;
      //FTUE
      if(FTUEManager.Instance.ShouldDisplayFTUE(FTUEType.MAP_AREA_BUTTON)){
        piConnectionManager.Instance.hideChromeButton();
        GameObject tutorial = Instantiate(_tutorialCanvasPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        tutorial.transform.SetParent(this.transform, false);
        trFTUEController ftueCtrl = tutorial.GetComponent<trFTUEController>();
        ftueCtrl.SetupView(FTUEType.MAP_AREA_BUTTON, ()=>{onAreaButtonClicked(trMapAreaType.WONDER_WORKSHOP);});
      }
    }

    protected override void OnDisable(){
      if(trDataManager.Instance != null){
        trDataManager.Instance.OnNewMissionUnlocked -= updateMissionView;
        trDataManager.Instance.IsAllowShowNewMissionPanel = true;
      }
      base.OnDisable();
    }
    
    public void SetActive(bool active){
      trDataManager.Instance.IsAllowShowNewMissionPanel = !active;
      if(active){
        updateMissionView();
      }
      this.gameObject.SetActive(active);
    }
    
    private void onMissionButtonClicked(trListItemControl button){
      trState state = buttonToStateTable[button];


      if (state.Behavior.MissionFileInfo != null){
        trDataManager.Instance.MissionMng.LoadMission(state.Behavior.MissionFileInfo.UUID);
        trDataManager.Instance.MissionMng.AuthoringMissionInfo.EditState = MissionEditState.NORMAL;

        if(trDataManager.Instance.MissionMng.UserOverallProgress.IsCurMissionCompleted){
          showCompletedMissionActionDialog(trDataManager.Instance.MissionMng.GetCurMission());
        }
        else{
          loadMissionScene();
        }
      }
    }

    private void showCompletedMissionActionDialog(trMission mission){
      RestartButton.onClick.AddListener(onRestartButtonClicked);
      RestartDialogBackButton.onClick.AddListener(onCancelButtonClcked);
      RestartDialogCancelButton.onClick.AddListener(onCancelButtonClcked);

      if(!trDataManager.Instance.MissionMng.UserOverallProgress.IsFreePlayUnlocked){
        LoadAndPlayButton.gameObject.SetActive(false);
        RestartText.text = wwLoca.Format("@!@Would you like to replay this challenge?@!@");
      }         
      else{
        LoadAndPlayButton.gameObject.SetActive(true);
        LoadAndPlayButton.onClick.AddListener(onLoadAndPlayButtonClicked);
        RestartText.text = wwLoca.Format("@!@Would you like to make a copy or replay this challenge?@!@");
      }
      MissionNameLabel.text = wwLoca.Format(mission.UserFacingName);
      RestartDialog.SetActive(true);      
    }

    private void onCancelButtonClcked(){
      RestartDialog.SetActive(false);
    }

    protected override void onBackButtonClicked (){
      trNavigationRouter.Instance.ShowSceneWithName(trNavigationRouter.SceneName.LOBBY);
    }

    static void doTelemetryMissionStart() {
      trMission trM         = trDataManager.Instance.MissionMng.GetCurMission();
      bool starting         = trDataManager.Instance.MissionMng.UserOverallProgress.SetMissionStartReported(trM.UUID, true);
      bool alreadyCompleted = trDataManager.Instance.MissionMng.UserOverallProgress.IsMissionCompletedOnce (trM.UUID);

      trTelemetryEventType trTET = trTelemetryEventType.UNKNOWN;

      if (starting) {
        if (alreadyCompleted) {
          trTET = trTelemetryEventType.CHAL_START_REPLAY;
        }
        else {
          trTET = trTelemetryEventType.CHAL_START_FIRSTPLAY;
        }
      }
      else {
        if (alreadyCompleted) {
          trTET = trTelemetryEventType.CHAL_RESUME_REPLAY;
        }
        else {
          trTET = trTelemetryEventType.CHAL_RESUME_FIRSTPLAY;
        }
      }

      new trTelemetryEvent(trTET, true)
        .add(trTelemetryParamType.CHALLENGE, trDataManager.Instance.MissionMng.GetCurMission().UserFacingName)
        .emit();
    }

    public static void loadMissionScene(){
      doTelemetryMissionStart();
      trNavigationRouter.Instance.ShowSceneWithName(trNavigationRouter.SceneName.MAIN, trProtoController.RunMode.Challenges.ToString());
    }

    private void onLoadAndPlayButtonClicked(){
      trMission mission = trDataManager.Instance.MissionMng.GetCurMission();      
      trDescriptiveItem item = new trDescriptiveItem();
      item.Program = trDataManager.Instance.MissionMng.GetCurProgram();
      item.Program.UserFacingName =  wwLoca.Format(mission.UserFacingName);
      item.Name = mission.UserFacingName;
      if(_shareItemDetailPanel==null){
        GameObject obj = Instantiate(_shareItemDetailPanelPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        obj.transform.SetParent(_shareItemDetailPanelHolder, false);
        _shareItemDetailPanel = obj.GetComponent<trSharedItemDetailPanelController>();
        _shareItemDetailPanel.downloadCountEnabled = false;
        _shareItemDetailPanel.descriptionEnabled = false;
      }
      _shareItemDetailPanel.LoadDescriptiveItem(item);
    }
    
    private void onRestartButtonClicked(){
      trDataManager.Instance.MissionMng.RestartCurMission(); 
      loadMissionScene();
    }
    
    private void SetupView()
    {

      //area buttons set up
      initAreaButtons();
      initMissionButtons();
      updateMissionView();

      foreach(trMissionAreaButtonController areaButton in areaToButtonTable.Values){
        if(areaButton.isInProgressOrNewFocus){
          Vector3 pos = -areaButton.gameObject.transform.position;
          _mapPanel.transform.position = new Vector3(pos.x, pos.y, 0f);
          _zoomCtrl.LimitZoomArea();
          break;
        }
      }

      openMissionSelectionDialogIfNeeded();
    } 

    private void openMissionSelectionDialogIfNeeded(){
      RunMode mode;
      piStringUtil.ParseStringToEnum<RunMode>(trNavigationRouter.Instance.GetTransitionParameterForScene(), out mode);
      if (mode == RunMode.Continue){
        bool shouldOpenDialog = false;
        string currentMissionId = trDataManager.Instance.MissionMng.UserOverallProgress.CurMissionInfo.MissionUUID;
        foreach(var pair in stateToButtonTable){
          if (pair.Key.Behavior.MissionFileInfo.UUID == currentMissionId){
            shouldOpenDialog = true;
            if (pair.Key.OutgoingTransitions.Count == 0){
              shouldOpenDialog = false;
            } else if ((trMapAreaType)pair.Key.OutgoingTransitions[0].StateTarget.BehaviorParameterValue != recentMapArreaType){
              shouldOpenDialog = false;
            }
            break;
          }
        }
        if (shouldOpenDialog){
          onAreaButtonClicked(recentMapArreaType);
        }
      }
    }

    private void initMissionButtons(){
      trProgram program = trDataManager.Instance.MissionMng.AuthoringMissionInfo.MissionMap;

      traverseStates(program.StateStart);

      foreach(RectTransform list in areaToListTable.Values){
        list.gameObject.SetActive(false);
      }
    }

    // todo: replace the traversal logic here with a call to trAuthoringMissionInfo.buildAreaList().
    private void traverseStates(trState state){
      if(state.Behavior.IsMissionBehavior
         &&state.Behavior.MissionFileInfo.Type.GetRobotType() == trDataManager.Instance.CurrentRobotTypeSelected){
        GameObject newButton = Instantiate(MissionButtonPrefab) as GameObject;
        trMapAreaType area = (trMapAreaType)state.BehaviorParameterValue;
        newButton.transform.SetParent(areaToListTable[area].transform, false);
        trMissionButtonController ctrl = newButton.GetComponent<trMissionButtonController>();
        ctrl.SetupView(state);
        stateToButtonTable.Add(state, ctrl);
        buttonToStateTable.Add((trListItemControl)ctrl, state);
        ctrl.onItemClicked.AddListener(onMissionButtonClicked);
        areaToButtonTable[area].Missions.Add(state);
        if(state.Behavior.IsMissionBehavior && 
          state.Behavior.MissionFileInfo.UUID == trDataManager.Instance.MissionMng.UserOverallProgress.RecentPlayedMissionUUID){                 
          recentMapArreaType = area;
        }
      }
      foreach(trTransition transition in state.OutgoingTransitions){
        traverseStates(transition.StateTarget);
      }
    }

    private void initAreaButtons(){
      int index = 0;
      foreach (trMapAreaType areaType in Enum.GetValues(typeof(trMapAreaType))){
        GameObject areaButton = Instantiate(MissionAreaPrefab) as GameObject;
        areaButton.transform.SetParent(MissionAreaContainers[index].transform, false);
        areaButton.transform.localScale = Vector3.one;
        areaButton.transform.localPosition = Vector3.zero;
        trMissionAreaButtonController areaButtonCtrl = areaButton.GetComponent<trMissionAreaButtonController>();
        areaToButtonTable.Add(areaType, areaButtonCtrl);
        GameObject newList = Instantiate(MissionListPrefab) as GameObject;
        newList.SetActive(true);
        newList.transform.SetParent(ListParent, false);
        areaToListTable.Add(areaType, newList.GetComponent<RectTransform>());
        areaButtonCtrl.AreaClicked += onAreaButtonClicked;
        index++;
      }
    }

    private void onAreaButtonClicked(trMapAreaType area){
      if(curList != null){
        curList.gameObject.SetActive(false);
      }
      curList = areaToListTable[area];
      ScrollCtrl.content = curList;
      curList.gameObject.SetActive(true);

      new trTelemetryEvent(trTelemetryEventType.CHAL_AREA, true)
        .add(trTelemetryParamType.ROBOT_TYPE, trDataManager.Instance.CurrentRobotTypeSelected)
        .add(trTelemetryParamType.AREA, area.ToString())
        .emit();

      lateUpdateListPosition = true;
      MissionList.gameObject.SetActive(true);
      MissionList.SetupView(areaToButtonTable[area]);
    }

    private void LateUpdate() {
      if (lateUpdateListPosition) {
        lateUpdateListPosition = false;
        Canvas.ForceUpdateCanvases();
        setListPosition();
      }
    }

    private void setListPosition() {
      // scroll to first 'CanStartOrResume' mission
      trMissionButtonController scrollToButton = null;
      foreach (Transform child in curList.transform) {
        trMissionButtonController button = child.GetComponent<trMissionButtonController>();
        if ((button != null) && (button.CanStartOrResume)) {
          scrollToButton = button;
          break;
        }
      }

      // all missions complete, scroll to end
      if (scrollToButton == null){
        ScrollCtrl.verticalNormalizedPosition = 0;
        return;
      }

      float buttonVerticalCenter = scrollToButton.transform.localPosition.y - (scrollToButton.transform as RectTransform).rect.height * 0.5f;
      float viewportHeight = (ScrollCtrl.transform as RectTransform).rect.height;
      float listHeight = curList.rect.height;

      // calculate the center of the list in verticalNormalizedPosition (vnp) coordinates (0:bottom, 1:top) 
      // vpn1: vertical center when scrolled to the top
      // vpn0: center when scrolled to the bottom
      float vnp1 = -viewportHeight * 0.5f;
      float vnp0 = -listHeight + viewportHeight*0.5f;

      // place next mission at center, or as close as possible
      ScrollCtrl.verticalNormalizedPosition = Mathf.Clamp01((buttonVerticalCenter - vnp0) / (vnp1 - vnp0));
    }

    private void updateMissionView(){
      foreach(trMapAreaType areaType in areaToButtonTable.Keys){
        areaToButtonTable[areaType].SetupView(areaType);
      }
      foreach(trMissionButtonController button in stateToButtonTable.Values){
        button.UpdateView();
      }
    }

    public enum RunMode {
      Start = 0, 
      Continue = 1
    }
  }
}
