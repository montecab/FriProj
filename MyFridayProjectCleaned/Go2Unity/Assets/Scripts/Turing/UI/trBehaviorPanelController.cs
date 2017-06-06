using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using WW.UGUI;
using UnityEngine.UI;

namespace Turing{
  public class trBehaviorPanelController : uGUISegmentedController {

    [SerializeField]
    private Transform _uiMainTransform;
  	public GameObject ButtonsParent;
    public GameObject BehaviorButtonPrefab;
    public GameObject LockedModal;
    public trProtoController ProtoCtrl;
    public RectTransform TabBarTransform;

    public Transform ContentPanel;
    public trUsageController UsageController;
    [SerializeField]
    private GameObject _accessoryAlertDialogPrefab;
    private bool alreadyShownAccessoryWarningModal;

    public uGUIGridPanelBase LightPanel;
    public uGUIGridPanelBase AnimPanel;
    public uGUIGridPanelBase SoundPanel;
    public uGUIGridPanelBase MovePanel;
    public uGUIGridPanelBase BMakerPanel;
    public uGUIGridPanelBase MiscPanel;
    public uGUIGridPanelBase MoodPanel;
    public uGUIGridPanelBase ExpressionPanel;
    public uGUIGridPanelBase AccessoryPanel;
    public uGUIGridPanelBase FunctionPanel;

    public uGUISegment BMakerTab;
    public uGUISegment LightTab;
    public uGUISegment AnimTab;
    public uGUISegment SoundTab;
    public uGUISegment MoveTab;
    public uGUISegment MiscTab;
    public uGUISegment MoodTab;
    public uGUISegment ExpressionTab;
    public uGUISegment AccessoryTab;
    public uGUISegment FunctionTab;

    public Button FunctionCreateButton;
    
    
    public Dictionary<trBehavior, trBehaviorButtonController> BehaviorToButtonDic = new Dictionary<trBehavior, trBehaviorButtonController>();

    private bool isAppUserSaveInfoLoad = false;
    private piRobotType currentRobotType;

    private trBehaviorButtonController FunctionEndButton;

    void Awake(){
      trDataManager.Instance.OnChangedBehaviorMakerFlag += updateBMakerPlusButton;
      FunctionCreateButton.onClick.AddListener(onCreateFunctionButtonClicked);
    }
      
    void OnDestroy(){
      if (trDataManager.Instance != null){
        trDataManager.Instance.OnChangedBehaviorMakerFlag -= updateBMakerPlusButton;
      }
    }

    public void DeleteFunction(trBehaviorButtonController button){
      uGUIGridPanelBase panel = getParentPanel(button.BehaviorData);
      panel.Children.Remove(button.gameObject);
      trDataManager.Instance.UserFunctions.RemoveBehavior(button.BehaviorData);

      if(ProtoCtrl.CurProgram.UUIDToBehaviorTable.ContainsKey(button.BehaviorData.UUID)){
        ProtoCtrl.CurProgram.RemoveBehavior(button.BehaviorData);
        ProtoCtrl.StateEditCtrl.UpdateStates();
        trDataManager.Instance.SaveCurProgram();
      }     
      BehaviorToButtonDic.Remove(button.BehaviorData);
      Destroy(button.gameObject);

    }

    void updateBMakerPlusButton(bool enabled){
      //BMakerPanel.GetComponent<trBMakerTabController>().CreateButton.gameObject.SetActive(enabled);
      setActiveUserSavedBehaviors(enabled);

      BMakerTab.gameObject.SetActive(enabled);
      if(Segments.Contains(BMakerTab) && !enabled){
        Segments.Remove(BMakerTab);
      }
      else if(!Segments.Contains(BMakerTab) && enabled){
        Segments.Add(BMakerTab);
      }
    }

    void setActiveUserSavedBehaviors(bool isActive){
      if(isActive){
        if(isAppUserSaveInfoLoad){
          return;
        }
        if(!trDataManager.Instance.IsBehaviorMakerEnabeld){
          return;
        }
        //add user behaviors from user saved behaviors
        
        trDataManager.Instance.AppUserSettings.UpdateBehaviors(trDataManager.Instance.GetCurProgram());
        foreach(trBehavior behavior in trDataManager.Instance.AppUserSettings.UuidToBehaviorDic.Values){
          trBehaviorArea area = behavior.Type.GetArea(currentRobotType);
          uGUIGridPanelBase parentPanel = getParentPanel(area);
          if(parentPanel != null){
            CreateButton(behavior, parentPanel);
          }    
        }
        isAppUserSaveInfoLoad = true;
      }
      else{
        if(!isAppUserSaveInfoLoad){
          return;
        }
        foreach(trBehavior behavior in trDataManager.Instance.AppUserSettings.UuidToBehaviorDic.Values){
          if(BehaviorToButtonDic.ContainsKey(behavior)){
            Destroy(BehaviorToButtonDic[behavior].gameObject);
            BehaviorToButtonDic.Remove(behavior);
          }
        }
        isAppUserSaveInfoLoad = false;
      }

    }

    private uGUISegment getTabButtonForParentPanel(uGUIGridPanelBase parentPanel) {
      if (parentPanel == AnimPanel){
        return AnimTab;
      }
      else if (parentPanel == SoundPanel) {
        return SoundTab;
      }
      else if (parentPanel ==  MovePanel) {
        return MoveTab;
      }
      else if (parentPanel == BMakerPanel) {
        return BMakerTab;
      } 
      else if (parentPanel ==  MiscPanel) {
        return MiscTab;
      }
      else if (parentPanel ==  MoodPanel) {
        return MoodTab;
      }
      else if (parentPanel == ExpressionPanel) {
        return ExpressionTab;
      }
      else if (parentPanel == AccessoryPanel) {
        return AccessoryTab;
      }
      else if(parentPanel == FunctionPanel){
        return FunctionTab;
      }
      return null;
    }

    public bool IsTabWithBehaviorActivated(trBehavior beh){
      trBehaviorArea area = beh.Type.GetArea(currentRobotType);
      uGUIGridPanelBase parentPanel = getParentPanel(area);
      if(activatedSegment.Contents.Contains(parentPanel.gameObject)){
        return true;
      }
      return false;
    }

    public uGUISegment GetTabWithBehavior(trBehavior beh){
      trBehaviorArea area = beh.Type.GetArea(currentRobotType);
      uGUIGridPanelBase parentPanel = getParentPanel(area);
      return getTabButtonForParentPanel(parentPanel);
    }

    public void ActivateTabWithBehavior(trBehavior beh){
      trBehaviorArea area = beh.Type.GetArea(currentRobotType);
      uGUIGridPanelBase parentPanel = getParentPanel(area);
      activeteSegmentBasedOnPanel(parentPanel);
    }

    public void InitView(trProgram program, piRobotType robotType){
      isAppUserSaveInfoLoad = false;
      
      currentRobotType = robotType;
      
      LightPanel.Clear();
      AnimPanel.Clear();
      SoundPanel.Clear();
      MovePanel.Clear();
      BMakerPanel.Clear();
      MiscPanel.Clear();
      MoodPanel.Clear();
      ExpressionPanel.Clear();
      AccessoryPanel.Clear();
      FunctionPanel.Clear();
      BehaviorToButtonDic.Clear();


      ActivateSegment(null);
      
      //Clear BMaker panel
      updateBMakerPlusButton(trDataManager.Instance.IsBehaviorMakerEnabeld);
      
      if(trDataManager.Instance.IsInNormalMissionMode){
        bool isSetActivatedSegment = false;
        bool isThereMapSet= false;

        foreach(trBehavior behavior in trDataManager.Instance.MissionMng.GetCurPuzzle().LastHint.Program.UUIDToBehaviorTable.Values){
          if(behavior.Type == trBehaviorType.START_STATE){
            continue;
          }

          if(behavior.Type.IsShowToUser()){
            trBehaviorArea area = behavior.Type.GetArea(currentRobotType);
            uGUIGridPanelBase parentPanel = getParentPanel(area);
            if(parentPanel != null){
              CreateButton(behavior, parentPanel);
              if(!isSetActivatedSegment 
                 && !trDataManager.Instance.MissionMng.GetCurPuzzle().FirstHint.Program.UUIDToBehaviorTable.ContainsKey(behavior.UUID)){ //if the beahvior doesn't exist in the start state machine, show the tab panel
                activeteSegmentBasedOnPanel(parentPanel);
                isSetActivatedSegment = true;
              }
              if(activatedSegment == null){
                activeteSegmentBasedOnPanel(parentPanel);
              }
            }
            
            if(behavior.Type == trBehaviorType.MAPSET){
              isThereMapSet = true;
            }
          }     
        }

//        foreach(trBehavior behavior in trDataManager.Instance.MissionMng.GetCurPuzzle().UUIDToBehaviorDic.Values){
//          if(behavior.Type.IsShowToUser()){
//            trBehaviorArea area = behavior.Type.GetArea(currentRobotType);
//            uGUIGridPanelBase parentPanel = getParentPanel(area);
//            if(parentPanel != null){
//              CreateButton(behavior, parentPanel);
//              if(!isSetActivatedSegment ){
//                activeteSegmentBasedOnPanel(parentPanel);
//                isSetActivatedSegment = true;
//              }
//            }
//
//            if(behavior.Type == trBehaviorType.MAPSET){
//              isThereMapSet = true;
//            }
//          }     
//        }

        if(!isThereMapSet){
          BMakerPanel.GetComponent<trBMakerTabController>().CreateButton.gameObject.SetActive(false);
        }

        setupSegmentsBar();
      }
      else{
        uGUIGridPanelBase[] allParentPanels = {
          LightPanel,
          AnimPanel,
          SoundPanel,
          MovePanel,
          BMakerPanel,
          MiscPanel,
          MoodPanel,
          ExpressionPanel,
          AccessoryPanel,
          FunctionPanel
        };

        List<uGUIGridPanelBase> parentPanelsList = new List<uGUIGridPanelBase>(allParentPanels);

        //add default behaviors
        foreach(trBehavior behavior in trBehavior.DefaultBehaviorsSorted){
          if(behavior.Type.IsShowToUser()){
            trBehaviorArea area = behavior.Type.GetArea(currentRobotType);
            uGUIGridPanelBase parentPanel = getParentPanel(area);
            parentPanelsList.Remove(parentPanel);
            if(parentPanel != null){
              CreateButton(behavior, parentPanel);
            }
          }    
        }
        
        //add custom behaviors from rewards
        foreach(trBehavior behavior in trMapSetBehaviors.Instance.GetAllBehaviors()){
          trBehaviorArea area = behavior.Type.GetArea(currentRobotType);
          uGUIGridPanelBase parentPanel = getParentPanel(area);
          parentPanelsList.Remove(parentPanel);
          if(parentPanel != null){
            CreateButton(behavior, parentPanel);
          }    
        }

        bool isFunctionUnlock = trMultivariate.Instance.getOptionValue(trMultivariate.trAppOption.UNLOCK_FUNCTIONS) == trMultivariate.trAppOptionValue.YES;

        if(isFunctionUnlock){
          //add function behaviors
          trDataManager.Instance.UserFunctions.UpdateBehaviors(trDataManager.Instance.GetCurProgram());
          foreach(trFunctionBehavior behavior in trDataManager.Instance.UserFunctions.GetFunctionsWithRobotType(currentRobotType)){
            trBehaviorArea area = behavior.Type.GetArea(currentRobotType);
            uGUIGridPanelBase parentPanel = getParentPanel(area);
            parentPanelsList.Remove(parentPanel);
            if(parentPanel != null){
              CreateButton(behavior, parentPanel);
            }    
          }
          showHideFunctionRelatedBehaviors();
        }
       
       
       
        setActiveUserSavedBehaviors(true);

        foreach(uGUISegment segment in Segments){
          segment.gameObject.SetActive(true);
        }
        // If the list is not empty, it means that we found a category for which there are no behaviors at this point. So we do not show it
        if (parentPanelsList.Count > 0) {
          foreach (uGUIGridPanelBase parentPanel in parentPanelsList) {
            uGUISegment tabButton = getTabButtonForParentPanel(parentPanel);
            if (tabButton != null)
            {
              tabButton.gameObject.SetActive(false);
            } 
          }
        }
        if (StartSegment != null){
          ActivateSegment(StartSegment);
        }
        else {
          ActivateSegment(Segments[0]);          
        }

        if(!isFunctionUnlock){
          FunctionTab.gameObject.SetActive(false);
        }

      }
    }

    void setupSegmentsBar(){
      foreach(uGUISegment segment in Segments){
        bool isSegmentEnabled = false;
        foreach(GameObject item in segment.Contents){
          uGUIGridPanelBase contentPanel = item.GetComponent<uGUIGridPanelBase>();
          if (contentPanel){
            isSegmentEnabled = contentPanel.Children.Count > 0;
          }
          if (isSegmentEnabled){
            break;
          }
        }
        segment.gameObject.SetActive(isSegmentEnabled);
      }
    }

    private void activeteSegmentBasedOnPanel(uGUIGridPanelBase panel){
      for(int i = 0; i< Segments.Count; ++i){
        if(Segments[i].Contents.Contains(panel.gameObject)){
          ActivateSegment(Segments[i]);
        }
      }
    }

    public override void ActivateSegment (uGUISegment seg){
      bool hasSelectedTab = activatedSegment != null;
      bool isTheSameTabClicked = activatedSegment == seg;
      base.ActivateSegment (seg);
      if (hasSelectedTab && seg != null){
        if (!UsageController.IsPannelHidden) {
          SoundManager.soundManager.PlaySound(SoundManager.trAppSound.UI_SOUND);
        }

        int index = Segments.IndexOf(seg);
        if ((seg == AccessoryTab) && !alreadyShownAccessoryWarningModal){
          GameObject obj = Instantiate(_accessoryAlertDialogPrefab, Vector3.zero, Quaternion.identity) as GameObject;
          obj.transform.SetParent(_uiMainTransform, false);
          trAlertDialogController dialogCtrl = obj.GetComponent<trAlertDialogController>();
          dialogCtrl.TitleText.text = wwLoca.Format("@!@You need the Launcher!@!@");
          dialogCtrl.DescriptionText.text = wwLoca.Format("@!@Dash can launch balls at targets! But Dash needs to be wearing the Launcher accessory. \n\nAsk a parent to visit makewonder.com for more details about buying a Launcher.@!@");
          alreadyShownAccessoryWarningModal = true;
        }
        if (index < 0) {
          WWLog.logError("segment not in list! " + seg.name);
          return;
        }
        trSecretAdminController.Instance.addToKeyPhrase(string.Format("B{0}", index + 1));
      }
      if (isTheSameTabClicked && !UsageController.IsPannelHidden){
        UsageController.ForceHide();
      } else {
        UsageController.ReportInstantUsage();
      }
    }

    uGUIGridPanelBase getParentPanel(trBehavior beh){
      trBehaviorArea area = beh.Type.GetArea(currentRobotType);
      return getParentPanel(area);
    }

    uGUIGridPanelBase getParentPanel(trBehaviorArea area){
      switch(area){
      case trBehaviorArea.LIGHT:
        return LightPanel;
      case trBehaviorArea.ANIM:
        return AnimPanel;
      case trBehaviorArea.SOUND:
        return SoundPanel;
      case trBehaviorArea.MOVE:
        return MovePanel;
      case trBehaviorArea.MAPPER: 
        return BMakerPanel;
      case trBehaviorArea.NOTHING:
        return MiscPanel;
      case trBehaviorArea.MOOD:
        return MoodPanel;
      case trBehaviorArea.EXPRESSION:
        return ExpressionPanel;
      case trBehaviorArea.ACCESSORY:
        return AccessoryPanel;
      case trBehaviorArea.FUNCTION:
        return FunctionPanel;
      }
      return null;
    }

    public void showHideFunctionRelatedBehaviors(){
      bool isShow = (ProtoCtrl.UiState == trProtoController.UIState.EDIT_FUNCTION);
      foreach(GameObject obj in FunctionPanel.Children){
        obj.SetActive(!isShow);
      }
      trBehavior beh = trBehavior.GetDefaultBehavior(trBehaviorType.FUNCTION_END);
      BehaviorToButtonDic[beh].gameObject.SetActive(isShow);

      FunctionCreateButton.gameObject.SetActive(!isShow);
     
      beh = trBehavior.GetDefaultBehavior(trBehaviorType.OMNI);
      BehaviorToButtonDic[beh].gameObject.SetActive(!isShow);
    }

    void onCreateFunctionButtonClicked(){

      trFunctionBehavior behavior = new trFunctionBehavior();
      behavior.FunctionProgram = trFunction.NewFunction(trDataManager.Instance.CurrentRobotTypeSelected);
      behavior.UUID = wwUID.getUID();

      behavior.FunctionProgram.UserFacingName = "My Function";

      trDataManager.Instance.UserFunctions.AddBehavior(behavior);

      CreateButton(behavior, FunctionPanel);
    }

    void onButtonClicked(trBehaviorButtonController button){
      if(button.BehaviorData.Type == trBehaviorType.MAPSET && trDataManager.Instance.IsBehaviorMakerEnabeld){
        ProtoCtrl.BehaviorMakerPanelCtrl.SetActive(button);
      }
      else if(button.BehaviorData.Type == trBehaviorType.FUNCTION){
        ProtoCtrl.LoadFunction((trFunctionBehavior)(button.BehaviorData));
      }
    }

    void onItemUsageContinues(trBehaviorButtonController button){
      if (button.IsActive){
        UsageController.StartContinuesUsage();
      } else {
        UsageController.StopContinuesUsage();
      }
    }

    public trBehaviorButtonController CreateButton(trBehavior behavior, uGUIGridPanelBase grid){
      GameObject newButton = Instantiate(BehaviorButtonPrefab, BehaviorButtonPrefab.transform.position, BehaviorButtonPrefab.transform.rotation) as GameObject;
      newButton.GetComponent<LayoutElement>().preferredWidth = grid.gameObject.GetComponent<RectTransform>().GetHeight();
      trBehaviorButtonController ctrl = newButton.GetComponent<trBehaviorButtonController>();
      ctrl.BehaviorData = behavior;
      ctrl.ProtoCtrl = ProtoCtrl;
      ctrl.OnClickListeners += onButtonClicked;
      ctrl.OnUsageChanged += onItemUsageContinues;
      ctrl.HosterGameObject = grid.gameObject;
      ctrl.LockedModal = LockedModal;
      grid.AddChild(newButton);
      ctrl.GeneratePrefab = newButton;
      BehaviorToButtonDic.Add(behavior, ctrl);
      return ctrl;
    }
  }
}
