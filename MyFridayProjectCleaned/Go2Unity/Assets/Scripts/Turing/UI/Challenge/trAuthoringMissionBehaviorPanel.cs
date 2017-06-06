using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using WW.UGUI;

namespace Turing{
  public class trAuthoringMissionBehaviorPanel :uGUIGridPanelBase {

    public Button AddNewButton;
    public GameObject BehaviorButtonPrefab;

    public List<trBehaviorButtonController> Buttons = new List<trBehaviorButtonController>();
    public trAuthoringMissionPanelController MissionCtrl;
    
    void Start(){
      AddNewButton.onClick.AddListener(()=>onCreateButtonClicked());
    }

    public void SetUpView(){
      foreach(trBehaviorButtonController button in Buttons){
        Destroy(button.gameObject);
      }
      Buttons.Clear();
      
      if(!trDataManager.Instance.AuthoringMissionInfo.IsLoadUserFolder){
        foreach(trMissionFileInfo mission in trDataManager.Instance.MissionMng.AuthoringMissionInfo.UUIDToMissionDic.Values){
          trBehavior beh = new trBehavior(trBehaviorType.MISSION);
          beh.MissionFileInfo = mission;
          CreateButton(beh);
        }
      }
    }

    
    public void DeleteBehavior(trBehaviorButtonController button){
      Children.Remove(button.gameObject);      
      Destroy(button.gameObject);
      
    }

    void onCreateButtonClicked(){
      
      trBehavior behavior = new trBehavior(trBehaviorType.MISSION);
      behavior.MissionFileInfo = trDataManager.Instance.MissionMng.AuthoringMissionInfo.CreateMission();
      CreateButton(behavior);
      MissionCtrl.MissionConfigCtrl.SetActive();
      //ProtoCtrl.BehaviorMakerPanelCtrl.SetActive(button);
      
    }

    void onClickMissionBehaviorPanel(trBehaviorButtonController button){
      trDataManager.Instance.MissionMng.AuthoringMissionInfo.UpdateCurMission(button.BehaviorData.MissionFileInfo);
      MissionCtrl.MissionConfigCtrl.SetActive();
    }

    public trBehaviorButtonController CreateButton(trBehavior behavior){
      GameObject newButton = Instantiate(BehaviorButtonPrefab, BehaviorButtonPrefab.transform.position, BehaviorButtonPrefab.transform.rotation) as GameObject;
      trBehaviorButtonController ctrl = newButton.GetComponent<trBehaviorButtonController>();
      ctrl.BehaviorData = behavior;
      ctrl.OnClickListeners += onClickMissionBehaviorPanel;
      ctrl.ProtoCtrl = MissionCtrl.MissionListCtrl.StateMachineCtrl;
      ctrl.HosterGameObject = GridParent;
      AddChild(newButton);
      ctrl.GeneratePrefab = newButton;
      Buttons.Add(ctrl);
      return ctrl;
    }
  
  }
}
