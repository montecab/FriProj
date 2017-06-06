using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Turing{
  public class trAuthoringMissionAreaConfigPanelController : MonoBehaviour {

    public ToggleGroup Group;
    public GameObject TogglePrefab;

    public Dictionary<trMapAreaType, Toggle> Toggles= new Dictionary<trMapAreaType, Toggle>();
    public GameObject TogglesPanel;
    public trStateButtonController StateButton;

    public Button StartMissionButton;
    public Button ResetAndBackToMapButton;

    private bool isInit = false;

    public trAuthoringMissionListPanelController MissionConfigCtrl;

  	// Use this for initialization
  	void Start () {
  	  initView();
  	}

    void initView(){
      if(isInit){
        return;
      }
      foreach(trMapArea area in trAuthoringMissionInfo.MapAreas.Values){
        GameObject newToggle = Instantiate(TogglePrefab) as GameObject;
        newToggle.transform.SetParent(Group.gameObject.transform, false);
        Toggle toggle = newToggle.GetComponent<Toggle>();
        toggle.group = Group;
        toggle.GetComponentInChildren<Text>().text = area.UserFacingName;
        Toggles.Add(area.Area, toggle);
        toggle.isOn = false;
      }
      StartMissionButton.onClick.AddListener(OnStartMissionButtonClicked);
      ResetAndBackToMapButton.onClick.AddListener(onResetAndBackToMapClicked);

      isInit= true;
    }

    void OnStartMissionButtonClicked(){
      trDataManager.Instance.MissionMng.UpdateUserOverallProgress(StateButton.BehaviorData.MissionFileInfo.UUID);
      trDataManager.Instance.MissionMng.LoadMission(StateButton.BehaviorData.MissionFileInfo.UUID);
      trDataManager.Instance.AuthoringMissionCtrl.SetActive(false);
      trDataManager.Instance.CurrentRobotTypeSelected = StateButton.BehaviorData.MissionFileInfo.Type.GetRobotType();
      trNavigationRouter.Instance.ShowSceneWithName(trNavigationRouter.SceneName.MAIN, trProtoController.RunMode.Challenges.ToString());
    }

    void onResetAndBackToMapClicked(){
      trDataManager.Instance.MissionMng.UpdateUserOverallProgress(StateButton.BehaviorData.MissionFileInfo.UUID);
      trDataManager.Instance.AuthoringMissionCtrl.SetActive(false);
      trDataManager.Instance.CurrentRobotTypeSelected = StateButton.BehaviorData.MissionFileInfo.Type.GetRobotType();
      trNavigationRouter.Instance.ShowSceneWithName(trNavigationRouter.SceneName.MAP);
    }

    public void ChooseArea(trStateButtonController ctrl){
      StateButton = ctrl;
      this.gameObject.SetActive(true);
      setUpView();
    }

    void setUpView(){
      initView();
      bool showToggles = trDataManager.Instance.MissionMng.AuthoringMissionInfo.IsPathValid()
                        && !trDataManager.Instance.MissionMng.AuthoringMissionInfo.IsLoadUserFolder;
      TogglesPanel.SetActive(showToggles);
      trMapAreaType area = (trMapAreaType)StateButton.StateData.BehaviorParameterValue;
      Toggles[area].isOn = true;
    }
  	
    void OnDisable(){
      if(trDataManager.Instance == null 
         || trDataManager.Instance.MissionMng.AuthoringMissionInfo.IsLoadUserFolder
         || !trDataManager.Instance.MissionMng.AuthoringMissionInfo.IsPathValid()){
        return;
      }
      trMapAreaType area = trMapAreaType.WONDER_WORKSHOP;
      foreach(trMapAreaType t in Toggles.Keys){
        if(Toggles[t].isOn){
          area = t;
          break;
        }
      }
      if(StateButton.StateData.BehaviorParameterValue == (int)area){ // nothing changed
        return;
      }
      StateButton.StateData.BehaviorParameterValue = (int)area;
      trDataManager.Instance.MissionMng.AuthoringMissionInfo.Save();
      StateButton.SetUpView();
    }
  }
}
