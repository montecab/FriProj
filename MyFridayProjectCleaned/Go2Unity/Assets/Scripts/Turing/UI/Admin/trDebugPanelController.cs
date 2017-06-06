using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Text;
using TMPro;

namespace Turing{
  public class trDebugPanelController : MonoBehaviour {

    [SerializeField]
    private Button _skipStepButton;
    [SerializeField]
    private TextMeshProUGUI _currentChallengeText;

    private trProtoController _protoCtrl;

    public void SetupView(trProtoController protoCtrl) {
      _protoCtrl = protoCtrl;
      if(trDataManager.Instance.IsInNormalMissionMode){
        _skipStepButton.gameObject.SetActive(true);
        _skipStepButton.onClick.AddListener(onSkipButtonClicked);
        _currentChallengeText.gameObject.SetActive(true);
        updateCurrentChallengeText();
      }
    }

    private void onSkipButtonClicked(){
      _protoCtrl.MissionEvaluationCtrl.CompletePuzzle();
      updateCurrentChallengeText();
    }

    private void updateCurrentChallengeText() {
      StringBuilder sb = new StringBuilder();
      sb.Append(BuildInfo.Summary);
      sb.Append("\n");
      sb.Append("Device:"+SystemInfo.deviceModel);
      sb.Append("\n");
      sb.Append(trDataManager.Instance.MissionMng.GetCurMission().UserFacingName);
      sb.Append(" step # ");
      sb.Append(_protoCtrl.PuzzleInfoPnlCtrl.currentStep+1);
      sb.Append(" of ");
      sb.Append(_protoCtrl.PuzzleInfoPnlCtrl.stepsCount);
      _currentChallengeText.text = sb.ToString();
    }
  }

}
