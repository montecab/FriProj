using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using Turing;

public class trNarrativeUIController : MonoBehaviour {

  [SerializeField]
  private TextMeshProUGUI _description;
  [SerializeField]
  private Animator _animator;
  [SerializeField]
  private Button _playButton;

  private trProtoController _protoCtrl = null;
  private bool _hasMissionInfo = false;
  private bool _hasPuzzleInfo = false;
  private string _missionDescription;
  private string _puzzleDescription;

	private void Start () {
    _playButton.onClick.AddListener(OnPlayButtonClicked);
	}

  public void SetupView(trProtoController protoCtrl){
    piConnectionManager.Instance.hideChromeButton();
    _protoCtrl = protoCtrl;
    trMission mission = trDataManager.Instance.MissionMng.AuthoringMissionInfo.CurMission;
    trPuzzle puzzle = trDataManager.Instance.MissionMng.GetCurPuzzle();
    if(mission==null || puzzle==null){
      return;
    }
    _animator.ChangeState(1);
    _hasMissionInfo = !trDataManager.Instance.MissionMng.UserOverallProgress.CurMissionInfo.IsShownMissionInfo;
    _hasPuzzleInfo = puzzle.IsShowIntroduction && !trDataManager.Instance.MissionMng.UserOverallProgress.IsCurMissionCompleted;
    _missionDescription = (_hasMissionInfo)?wwLoca.Format(mission.IntroDescription):"";
    _puzzleDescription = (_hasPuzzleInfo)?wwLoca.Format(puzzle.IntroductionText):"";
    if(_hasMissionInfo){  //Show mission info
      _description.text = _missionDescription;
      trDataManager.Instance.MissionMng.UserOverallProgress.CurMissionInfo.IsShownMissionInfo = true;
      trDataManager.Instance.MissionMng.UserOverallProgress.SaveCurMissionInfo();
    }
    else if(_hasPuzzleInfo){  //Show puzzle info
      _description.text = _puzzleDescription;
    }
    else{ //No info
      _protoCtrl.TutorialCtrl.FirstTimeCheckTutorialProgress();
      _protoCtrl.ShowPuzzleInfoPanel();
      piConnectionManager.Instance.showChromeButton();
      this.gameObject.SetActive(false);
    }
  }

  private void OnPlayButtonClicked(){
    //If we have both info, display mission info first, and then puzzle info
    if(_hasMissionInfo && _hasPuzzleInfo){
      _hasMissionInfo = false;   
      _description.text = _puzzleDescription;
    }
    else{
      _protoCtrl.TutorialCtrl.FirstTimeCheckTutorialProgress();
      _protoCtrl.ShowPuzzleInfoPanel();
      piConnectionManager.Instance.showChromeButton();
      this.gameObject.SetActive(false);
    }
  }

}
