using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace Turing{
  public class trAuthoringMissionPanelController : MonoBehaviour, IPointerClickHandler {
    public trAuthoringMissionListPanelController MissionListCtrl;
    public trAuthoringMissionConfigPanelController MissionConfigCtrl;

    public Toggle BMakerEnabledToggle;

    void Awake(){
      BMakerEnabledToggle.onValueChanged.AddListener((value) => {
        trDataManager.Instance.IsBehaviorMakerEnabeld = value;
      });
    }

    public void SetActive(bool active){
      if(active){
        trDataManager.Instance.IsMissionMode = false;
        trDataManager.Instance.MissionMng.AuthoringMissionInfo.EditState = MissionEditState.EDIT;
        BMakerEnabledToggle.isOn = trDataManager.Instance.IsBehaviorMakerEnabeld;
      }
      else{
        if(trDataManager.Instance.MissionMng.AuthoringMissionInfo.EditState == MissionEditState.EDIT){
          trDataManager.Instance.MissionMng.AuthoringMissionInfo.EditState = MissionEditState.NORMAL;
        }
       

        string s = trNavigationRouter.Instance.GetTransitionParameterForScene();
        trProtoController.RunMode runMode;
        if (piStringUtil.ParseStringToEnum<trProtoController.RunMode>(s, out runMode)) {
          trDataManager.Instance.IsMissionMode = (runMode == trProtoController.RunMode.Challenges);
        }
      }
      this.gameObject.SetActive(active);
      if (trDataManager.Instance.onAuthorinModeChanged != null){
        trDataManager.Instance.onAuthorinModeChanged(active);
      }
    }

    void Update(){
      if(Input.GetKeyDown(KeyCode.Escape)){
        SetActive(false);
      }
    }

    #region IPointerClickHandler implementation

    public void OnPointerClick (PointerEventData eventData)
    {
      SetActive(false);
    }

    #endregion
  }
}