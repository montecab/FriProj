using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

namespace Turing{
  public class trPuzzleCompletePanelController : MonoBehaviour {

    [SerializeField]
    private GameObject _missionCompletePanelPrefab;
    private trMissionCompletePanelController _missionCompletePanelCtrl;

    [SerializeField]
    private Button _okButton;
    [SerializeField]
    private Button _bgButton;
    [SerializeField]
    private TextMeshProUGUI _description;
    [SerializeField]
    private GameObject _background;

    private trProtoController _protoCtrl;
    private bool isShowNextPuzzle = false;

    public void SetupView(trProtoController protoCtrl){
      _protoCtrl = protoCtrl;
      SetActive(true);
      _okButton.onClick.AddListener(onOKButtonClicked);
      _bgButton.onClick.AddListener(onOKButtonClicked);
    }

    private void OnDisable(){
      if(trDataManager.Instance != null){
        trDataManager.Instance.IsAllowShowNewMissionPanel = true;
      }
    }

    private void SetActive(bool active){
      _background.SetActive(true);
      trDataManager.Instance.IsAllowShowNewMissionPanel = !active;
      if(active){
        int incorrectRuns = _protoCtrl.MissionEvaluationCtrl.IncorrectRuns;
        int incompleteRuns = _protoCtrl.MissionEvaluationCtrl.TotalRuns - incorrectRuns - 1;
        int time = _protoCtrl.MissionEvaluationCtrl.SecondsSpent;        
        isShowNextPuzzle = trDataManager.Instance.MissionMng.ActivateNextPuzzle(incorrectRuns,incompleteRuns,time);
        if (isShowNextPuzzle){
          //Debug.Log("playing step success animation!");
          _protoCtrl.PlaySuccessAnimation(); // completed a step inside the puzzle, show step-succss animation
          gameObject.SetActive(true);
          uGUIPanelTween.Instance.TweenSetActive(true, this.gameObject.transform, 0);
        }
        else{
          if(_missionCompletePanelCtrl==null){
            GameObject obj = GameObject.Instantiate(_missionCompletePanelPrefab, Vector3.zero, Quaternion.identity) as GameObject;
            obj.transform.SetParent(this.transform.parent, false);
            _missionCompletePanelCtrl = obj.GetComponent<trMissionCompletePanelController>();
          }
          _missionCompletePanelCtrl.SetupView(_protoCtrl, trDataManager.Instance.MissionMng.UserOverallProgress.CurMissionInfo);
          _missionCompletePanelCtrl.gameObject.SetActive(true);
          gameObject.SetActive(false);
        }
      }
    }

    private void onOKButtonClicked(){
      trNavigationRouter.Instance.ShowSceneWithName(sceneName: trNavigationRouter.SceneName.MAIN);
    }

  }
}
