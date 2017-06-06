using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;
using WW.SaveLoad;
using WW.SimpleJSON;

namespace Turing{
  public class trAuthoringMissionListPanelController :MonoBehaviour {

    public Button AddNewButton;

    public InputField PathInput;
    public InputField FileNameInput;

    public Toggle LoadFromExternalFolderToggle;

    public Text ErrorInfo;

    public trAuthoringMissionConfigPanelController ConfigPnlCtrl;

    public trProtoController StateMachineCtrl;

    private trAuthoringMissionInfo missionInfo;

    public Button CheckRewardCompatibilityButton;

    public Button VideoButton;
    public trVideoManagementPanelController VideoManagementPanel;

    void Start(){
      trDataManager.Instance.Init();
      missionInfo = trDataManager.Instance.AuthoringMissionInfo;
      ErrorInfo.text = "Path Invalid";
      PathInput.onEndEdit.AddListener(onPathChanged);
      PathInput.text = trDataManager.Instance.AuthoringMissionInfo.Path;
      LoadFromExternalFolderToggle.onValueChanged.AddListener(onToggleChange);
      InitView();

      CheckRewardCompatibilityButton.onClick.AddListener(onCheckRewardButtonClicked);

      VideoButton.onClick.AddListener (onVideoButtonClicked);
#if UNITY_IPHONE || UNITY_ANDROID
      LoadFromExternalFolderToggle.gameObject.SetActive(false);
#endif
    }

    void onVideoButtonClicked(){
      VideoManagementPanel.gameObject.SetActive(true);
    }

    void onCheckRewardButtonClicked(){
      trState start = trDataManager.Instance.AuthoringMissionInfo.MissionMap.StateStart;

      foreach(trTransition tran in start.OutgoingTransitions){
        checkMission(tran.StateTarget, 0);
      }
    }

    void checkMission(trState missionState, int parentPoints){
      string uuid = missionState.Behavior.MissionFileInfo.UUID;
      trMission mission = trDataManager.Instance.AuthoringMissionInfo.MissionDic[uuid];
      int iq = mission.MaxIQPoints;
      iq += parentPoints;
      trPuzzle lastPuzzle = mission.Puzzles[mission.Puzzles.Count - 1];
      trProgram program = lastPuzzle.LastHint.Program;

      trProgramIngredientInfo ingredient = new trProgramIngredientInfo();
      ingredient.CalculateIngredients(program);

      foreach(trTriggerType type in ingredient.TriggerTable.Keys){
        foreach(trReward reward in trRewardsManager.Instance.AvailableRewards){
          if(reward.IQPointsRequired > iq){
            foreach(trRewardDurable rd in reward.Durables){
              if(rd.Category == trRewardDurableCategory.CUE){
                if(type == rd.trigger.Type){
                  Debug.LogError("Challenge " + mission.UserFacingName + " has a locked cue " + type.ToString());
                }
              }
            }
          }
        }        
      }

      foreach(trTransition tran in missionState.OutgoingTransitions){
        checkMission(tran.StateTarget, iq);
      }

    }

    void onToggleChange(bool isOn){      
      trDataManager.Instance.AuthoringMissionInfo.IsLoadUserFolder = !isOn;
      trDataManager.Instance.AuthoringMissionInfo.Load();
      trDataManager.Instance.VideoManager.Load();
      if(isOn && !trDataManager.Instance.AuthoringMissionInfo.IsPathValid()){
        ErrorInfo.gameObject.SetActive(true);
      }
      else{
        CheckRewardCompatibilityButton.gameObject.SetActive(isOn);
        VideoButton.gameObject.SetActive(isOn);
        InitView();
        ResetStateMachineView();
      }
    }

    void InitView(){
      PathInput.text = missionInfo.Path;
    }
  
    public void ResetStateMachineView(){
      StateMachineCtrl.LoadProgram();
    }

    void onPathChanged(string s){
      ErrorInfo.gameObject.SetActive(false);
      
      s = s.Trim();
      missionInfo.Path = s;

      if(!trDataManager.Instance.AuthoringMissionInfo.IsPathValid()){
        WWLog.logError("Directory Invalid: \"" + s + "\"");
        ErrorInfo.gameObject.SetActive(true);
        return;
      }

      trDataManager.Instance.AuthoringMissionInfo.Load();
      ResetStateMachineView();
    }
      
  }

}
