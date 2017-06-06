using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

namespace Turing{

  public enum AreaStateType{
    ACTIVE,
    COMPLETE,
    LOCKED,
  }

  public class trMissionAreaButtonController : MonoBehaviour {

    public delegate void OnClickDelegate(trMapAreaType type);
    public OnClickDelegate AreaClicked;
    public List<trState> Missions = new List<trState>();
    public bool isInProgressOrNewFocus{get; private set;}
    public int completedMissions{get; private set;}
    public string areaName{get; private set;}
    public trMapAreaType areaType{get{return _area;}}

    [Header("Active area")]
    [SerializeField]
    private GameObject _activeGroup;
    [SerializeField]
    private TextMeshProUGUI _progessText;
    [SerializeField]
    private TextMeshProUGUI _areaText;
    [SerializeField]
    private Button _activeButton;
    [SerializeField]
    private Image _dashImage;
    [SerializeField]
    private Image _dotImage;
    [SerializeField]
    private wwGUISimpleMoves _animCtrl;

    [Header("Complete area")]
    [SerializeField]
    private GameObject _completeGroup;
    [SerializeField]
    private TextMeshProUGUI _progessCompleteText;
    [SerializeField]
    private TextMeshProUGUI _areaCompleteText;
    [SerializeField]
    private Button _completeButton;

    [Header("Locked area")]
    [SerializeField]
    private GameObject _lockedGroup;
    [SerializeField]
    private TextMeshProUGUI _areaLockedText;
    [SerializeField]
    private Button _lockedButton;
    [SerializeField]
    private GameObject _tooltip;
    [SerializeField]
    private TextMeshProUGUI _tooltipText;

    private trMapAreaType _area;

    private void Start(){
      _activeButton.onClick.AddListener(onButtonClicked);
      _completeButton.onClick.AddListener(onButtonClicked);
    }

    public void onPointerDown(){
      _tooltip.SetActive(true);
    }

    public void onPointerUp(){
      _tooltip.SetActive(false);
    }

    public void SetupView(trMapAreaType area){
      isInProgressOrNewFocus = false;
      _area = area;
      areaName = wwLoca.Format(trAuthoringMissionInfo.MapAreas[_area].UserFacingName);
      completedMissions = 0;
      int unlockedNum = 0;
      bool isNewUnlockArea = false;

      for(int i = 0; i< Missions.Count; ++i){
        string uuid = Missions[i].Behavior.MissionFileInfo.UUID;
        if(trDataManager.Instance.MissionMng.UserOverallProgress.IsMissionUnlocked(uuid)){
          unlockedNum++;
          if(trDataManager.Instance.MissionMng.UserOverallProgress.IsMissionCompletedOnce(uuid)){
            completedMissions++;
          }
          if(i==0 && !trDataManager.Instance.MissionMng.UserOverallProgress.IsMissionStarted(uuid)){ //New unlocked area
            isNewUnlockArea = true;
          }
        }
      }

      if(unlockedNum == 0){                     //Locked area
        SetArea(AreaStateType.LOCKED);
        _areaLockedText.text = areaName;
        _tooltipText.text = wwLoca.Format("@!@Complete your other challenges first@!@");
      }
      else if(completedMissions == Missions.Count){  //Complete area
        SetArea(AreaStateType.COMPLETE);
        _areaCompleteText.text = areaName;
        _progessCompleteText.text = wwLoca.Format("@!@{0}/{1} Completed@!@", completedMissions, Missions.Count);
      }
      else{                                     //Active area
        piRobotType robotType = Missions[0].Behavior.MissionFileInfo.Type.GetRobotType();
        _dashImage.enabled = (robotType==piRobotType.DASH);
        _dotImage.enabled = (robotType==piRobotType.DOT);
        isInProgressOrNewFocus = true;
        _animCtrl.enabled = true;
        SetArea(AreaStateType.ACTIVE);
        _areaText.text = areaName;
        _progessText.text = wwLoca.Format("@!@{0}/{1} Completed@!@", completedMissions, Missions.Count);
        if(isNewUnlockArea){                    //New unlocked area
          _activeGroup.GetComponent<Animator>().enabled = true;
          _lockedGroup.GetComponent<Animator>().enabled = true;
        }
      }
    }

    private void SetArea(AreaStateType type){
      _activeGroup.SetActive(type==AreaStateType.ACTIVE);
      _completeGroup.SetActive(type==AreaStateType.COMPLETE);
      _lockedGroup.SetActive(type==AreaStateType.LOCKED);
    }

    private void onButtonClicked(){
      if (AreaClicked != null){
        AreaClicked(_area);
      }
      else {
        WWLog.logError("map button clicked but area is null");
      }
    }
  }
}
