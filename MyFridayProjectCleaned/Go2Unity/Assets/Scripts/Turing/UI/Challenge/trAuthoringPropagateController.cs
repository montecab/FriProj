using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
namespace Turing{
  public class trAuthoringPropagateController : MonoBehaviour {

    private bool isPropgStateBehaviorAndPara = false;
    private bool isPropgStateActivationCnt = false;
    private bool isPropgStateLayoutPos = false;
    private bool isPropgStateCheckingPara = false;
    private bool isPropgTrigger = false;

    private string errorInfo = "";


    public Button PropagateForwardButton;
    public Button PropagateBackwardButton;
    public Button PropagateAllButton;

    public Toggle StateBehavAndParaToggle;
    public Toggle StateCheckingParaToggle;
    public Toggle StateLayoutToggle;
    public Toggle StateActivationCountToggle;
    public Toggle TriggerToggle;


    public TextMeshProUGUI ErrorText;
    public GameObject ErrorPanel;
    private trState curState;
    private trTransition curTransition;


    // Use this for initialization
    void Start () {
      PropagateForwardButton.onClick.AddListener(onPushForwardButtonClicked);
      PropagateAllButton.onClick.AddListener(onPushAllButtonClicked);
      PropagateBackwardButton.onClick.AddListener(onPushBackwardButtonClicked);
      StateCheckingParaToggle.onValueChanged.AddListener(onStateCheckingParaToggleChange);
      StateBehavAndParaToggle.onValueChanged.AddListener(onStateBehavToggleChange);
      StateLayoutToggle.onValueChanged.AddListener(onStateLayoutToggleChange);
      StateActivationCountToggle.onValueChanged.AddListener(onStateActivationCountToggleChange);
      TriggerToggle.onValueChanged.AddListener(onTriggerToggleChange);
    }

    void onStateCheckingParaToggleChange(bool isOn){
      isPropgStateCheckingPara = isOn;
    }

    void onStateBehavToggleChange(bool isOn){
      isPropgStateBehaviorAndPara = isOn;
    }

    void onStateLayoutToggleChange(bool isOn){
      isPropgStateLayoutPos = isOn;
    }

    void onStateActivationCountToggleChange(bool isOn){
      isPropgStateActivationCnt = isOn;
    }

    void onTriggerToggleChange(bool isOn){
      isPropgTrigger = isOn;
    }


    void onPushForwardButtonClicked(){
      propagateToPuzzles(PropagateType.FORWARD, curState, curTransition);
    }

    void onPushBackwardButtonClicked(){
      propagateToPuzzles(PropagateType.BACKWARD, curState, curTransition);
    }

    void onPushAllButtonClicked(){
      propagateToPuzzles(PropagateType.ALL, curState, curTransition);
    }

    public void showPropagatePanel(trState state){
      this.gameObject.SetActive(true);
      curState = state;
      curTransition = null;
      StateBehavAndParaToggle.gameObject.SetActive(true);
      StateActivationCountToggle.gameObject.SetActive(true);
      StateLayoutToggle.gameObject.SetActive(true);
      TriggerToggle.gameObject.SetActive(false);

    }

    public void showPropagatePanel(trTransition transition){
      this.gameObject.SetActive(true);
      curState = null;
      curTransition = transition;
      StateBehavAndParaToggle.gameObject.SetActive(false);
      StateActivationCountToggle.gameObject.SetActive(false);
      StateLayoutToggle.gameObject.SetActive(false);
      TriggerToggle.gameObject.SetActive(true);
    }

    enum PropagateType{
      BACKWARD,
      FORWARD,
      ALL
    }

    void propagateToPuzzles(PropagateType type, trState state, trTransition transition){
      errorInfo = "";
      int startPuzzleId = 0;
      int startHintId = 0;
      int endPuzzleId = 0;
      int endHintId = 0;

      trMission mission = trDataManager.Instance.MissionMng.GetCurMission();
      trPuzzle puzzle  = trDataManager.Instance.MissionMng.GetCurPuzzle();
      trHint hint = trDataManager.Instance.MissionMng.GetCurHint();

      int curPuzzleId = mission.Puzzles.IndexOf(puzzle);
      int curHintId = puzzle.Hints.IndexOf(hint);

      if(type == PropagateType.FORWARD || type == PropagateType.ALL){
        startPuzzleId = curPuzzleId;
        startHintId = curHintId + 1;

        if(startHintId == mission.Puzzles[startPuzzleId].Hints.Count){
          startPuzzleId ++;
          startHintId = 0;
        }       

        endPuzzleId = mission.Puzzles.Count - 1;
        endHintId = mission.Puzzles[endPuzzleId].Hints.Count - 1;

        if(startPuzzleId < mission.Puzzles.Count ){
          propagatePuzzles(startPuzzleId, startHintId, endPuzzleId, endHintId, state, transition);
        }
      }

      if(type == PropagateType.BACKWARD || type == PropagateType.ALL){
        startPuzzleId = 0;
        startHintId = 0;
        endPuzzleId = curPuzzleId;
        endHintId = curHintId - 1;
        if(endHintId == -1){
          endPuzzleId--;
        }
        if(endPuzzleId >= 0){
          endHintId = mission.Puzzles[endPuzzleId].Hints.Count - 1;
          propagatePuzzles(startPuzzleId, startHintId, endPuzzleId, endHintId, state, transition);
        }
      }

      trDataManager.Instance.MissionMng.SaveCurProgram();

      ErrorText.text = "Finished! \n";
      ErrorText.text += errorInfo;
      ErrorPanel.gameObject.SetActive(true);
      this.gameObject.SetActive(false);
    }

    void propagatePuzzles(int startPuzzleId, int startHintId, int endPuzzleId, int endHintId, trState state, trTransition transition ){
      trMission mission = trDataManager.Instance.MissionMng.GetCurMission();
      for(int i = startPuzzleId; i <= endPuzzleId; ++i){
        trPuzzle puzzle = mission.Puzzles[i];

        int start = i == startPuzzleId ? startHintId: 0;
        int end = i == endPuzzleId ? endHintId: puzzle.Hints.Count - 1;

        for(int j = start; j <= end; ++j){
          string error = "";
          if(state != null){
            error += propagateState(state, puzzle.Hints[j].Program);
          }

          if(transition != null){
            error += propagateTransition(transition, puzzle.Hints[j].Program);
          }

          if(error != ""){
            errorInfo += "Puzzle[" +(i+1).ToString() + "] Hint[" + (j+1).ToString() +"]: \n" + error;
          }
        }
      }
    }

    string propagateTransition(trTransition tran, trProgram program){
      foreach(trTransition transition in program.UUIDToTransitionTable.Values){
        if(tran.UUID == transition.UUID){
          if(isPropgTrigger){
            transition.Trigger = tran.Trigger.DeepCopy();
          }

          return "";
        }
      }
      return "Cannot find corresponding transition.\n";

    }
    
    string propagateState(trState s, trProgram program){
      string error = "";
      foreach(trState state in program.UUIDToStateTable.Values){
        if(state.UUID == s.UUID){
          if(isPropgStateBehaviorAndPara){
            program.setStateBehaviour(state, s.Behavior);
            for(int i = 0; i< s.BehaviorParaValuesCount; ++i){
              state.SetBehaviorParameterValue(i, s.GetBehaviorParameterValue(i));
            }
          }

          if(isPropgStateCheckingPara){
            state.IsCheckingParameter = s.IsCheckingParameter;
          }

          if(isPropgStateLayoutPos){
            state.LayoutPosition = s.LayoutPosition;
          }

          if(isPropgStateActivationCnt){
            state.ActivationCount = s.ActivationCount;
          }

          return error;
        }
      }

      error += "Cannot find corresponding state.\n";
      return error;
    }
  }

}
