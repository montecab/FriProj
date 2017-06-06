using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Turing{
  public class trStateMachinePanelBase : MonoBehaviour {

    public Dictionary<trState, trStateButtonController> StateToButtonTable = new Dictionary<trState, trStateButtonController>();
    public Dictionary<trTransition, trTransitionArrowController> TransitionToArrowTable = new Dictionary<trTransition, trTransitionArrowController>();

    public GameObject StateButtonPrefab;
    public GameObject ArrowPrefab;
    public GameObject StatePanel;

    public trProgram CurProgram;
    public piBotBase CurRobot;

    public bool IsShowObscure = false;
    public bool IsDisableInteraction = false;

    public trScrollAnimatedController ScrollMoveController;

    public virtual void SetUpView(trProgram program){
      Reset();

      if(program == null){
        WWLog.logError("Program is null.");
        return;
      }
      CurProgram = program;

      foreach(trState state in program.UUIDToStateTable.Values){
        CreateStateButton(state);
      }
      
      foreach(trTransition trans in program.UUIDToTransitionTable.Values){
        CreateTransitionArrow(trans);
      }

      if(program.StateOmni!= null){
        StateToButtonTable[program.StateOmni].SetEnableOmniState(true);
      }
    }
    
    public virtual void Reset(){
      
      foreach(trState state in StateToButtonTable.Keys){
        // add this because Destroy happens at the end of the frame so calculations sometimes are mistaken(eg. wwUtility.FitContent2DShallow()) 
        StateToButtonTable[state].gameObject.SetActive(false); 
        Destroy(StateToButtonTable[state].gameObject);
      }
      StateToButtonTable.Clear();
      
      foreach(trTransitionArrowController arrow in TransitionToArrowTable.Values){
        arrow.TransitionLine.gameObject.SetActive(false);
        arrow.gameObject.SetActive(false);
        Destroy(arrow.TransitionLine.gameObject);
        Destroy(arrow.gameObject);
      }
      TransitionToArrowTable.Clear();

    }

    public virtual void SetRunMode(bool runMode){

      foreach(trStateButtonController stateButton in StateToButtonTable.Values){
        stateButton.SetEnableRunningUI(runMode);
        if(!IsDisableInteraction){
          stateButton.EnableUserInteraction(!runMode);
        }
      }

      if(!IsDisableInteraction){
        foreach(trTransitionArrowController transitionArrow in TransitionToArrowTable.Values){
          transitionArrow.EnableUserInteraction(!runMode);
        }
      }



    }

    public trStateButtonController ButtonByState(trState state){
      trStateButtonController button = null;
      if (state != null) {
        StateToButtonTable.TryGetValue(state, out button);
      }
      return button; 
    }

    public void SetEnableCurrentState(trState state, bool isEnabled){
      trStateButtonController button = ButtonByState(state);
      if(button == null) {
        return;
      }
      button.SetRunFocus(isEnabled);

      Vector2 buttonPosition = button.transform.localPosition;
      Vector2 positionFromTopLeft = ScrollMoveController.ContentRectTransform.GetSize() / 2 + buttonPosition;
      if (isEnabled && !ScrollMoveController.IsPointVisible(positionFromTopLeft)){
        ScrollMoveController.ScrollToMakeRectVisible(positionFromTopLeft, 0.3f);
      }

      if(state.Behavior.Type == trBehaviorType.FUNCTION){
        button.FunctionRunningIndicator.SetActive(isEnabled);
      }

    }

    public List<trTransitionArrowController> GetTransitionArrowsForButtonState(trStateButtonController button){
      List<trTransitionArrowController> result = new List<trTransitionArrowController>();
      foreach (trTransitionArrowController arrow in TransitionToArrowTable.Values){
        if (arrow.SourceButton == button || arrow.TargetButton == button){
          result.Add(arrow);
        }
      }
      return result;
    }

    public List<trTransitionArrowController> GetOutgoingTransitionArrowsForButtonState(trStateButtonController button){
      List<trTransitionArrowController> result = new List<trTransitionArrowController>();
      foreach (trTransitionArrowController arrow in TransitionToArrowTable.Values){
        if (arrow.SourceButton == button){
          result.Add(arrow);
        }
      }
      return result;
    }

    public virtual trStateButtonController CreateStateButton(trState state){
      Vector3 posV = new Vector3(state.LayoutPosition.x, state.LayoutPosition.y, StateButtonPrefab.transform.localPosition.z);
      GameObject newStateBtn = Instantiate(StateButtonPrefab, posV, Quaternion.identity) as GameObject;
      newStateBtn.GetComponent<trButtonTween>().TweenDrop();
      
      newStateBtn.transform.SetParent(StatePanel.transform);
      newStateBtn.transform.localPosition = posV;

      trStateButtonController button = newStateBtn.GetComponent<trStateButtonController>();
      button.IsObscureAllowed = IsShowObscure;
      button.StateMachinePnlCtrl = this;
      button.StateData = state;
      button.BehaviorData = state.Behavior; 
      button.AttachAdditionalViewIfNeeded();
      StateToButtonTable.Add(state, button);

      if(IsDisableInteraction){
        button.EnableUserInteraction(false);
      }
      if (state.Behavior.Type == trBehaviorType.START_STATE) {
        button.StateImage.sprite = trIconFactory.GetStartStateIcon(CurProgram.RobotType);
      }

      return button;
    }

    public  virtual trTransitionArrowController CreateTransitionArrow(trTransition transition){
      trState targetState = transition.StateTarget;
      trState sourceState = transition.StateSource;
      if(targetState == null || sourceState == null){
        string s = targetState == null? "target" : "source";
        System.Text.StringBuilder sb = new System.Text.StringBuilder(s) ;
        WWLog.logError( sb.Append(" is null.").ToString());
        return null;
      }
      trStateButtonController sourceStateButton = null;
      trStateButtonController targetStateButton = null;
      StateToButtonTable.TryGetValue(sourceState, out sourceStateButton);
      StateToButtonTable.TryGetValue(targetState, out targetStateButton);
      
      if(sourceStateButton == null || targetStateButton == null){
        WWLog.logError("source or target state button doesn't exist");
        return null;
      }

      trTransitionArrowController arrowCtrl = null;
      foreach(trTransition existingTransition in TransitionToArrowTable.Keys){
        if (existingTransition.StateSource == transition.StateSource && existingTransition.StateTarget == transition.StateTarget){
          arrowCtrl = TransitionToArrowTable[existingTransition];
          break;
        }
      }

      if (arrowCtrl == null){
        Quaternion rot = Quaternion.Euler(Vector3.zero);

        GameObject arrow = Instantiate(ArrowPrefab, sourceStateButton.transform.position, rot ) as GameObject;   
        arrow.transform.SetParent(StatePanel.transform); 
        arrow.transform.localScale = Vector3.one;
        
        //put arrow in the back so it would not affect dropping detection
        arrow.transform.SetAsFirstSibling();
        //set all the transition lines to be back so it's not blocking trigger interaction
        foreach(trTransitionArrowController ctrl in TransitionToArrowTable.Values){
          ctrl.TransitionLine.gameObject.transform.SetAsFirstSibling();
        }
        
        arrowCtrl = arrow.GetComponent<trTransitionArrowController>();
        arrowCtrl.SetUp(transition, sourceStateButton, targetStateButton, IsShowObscure);
      } else {
        arrowCtrl.AddTransition(transition, IsShowObscure);
      }

      TransitionToArrowTable.Add(transition,arrowCtrl);

      if(IsDisableInteraction){
        arrowCtrl.EnableUserInteraction(false);
      }
      return arrowCtrl;

    }

    public int NextValueForBehaviourType(trBehaviorType type){
      int result = 0;
      List<int> indexes = new List<int>();
      foreach(trStateButtonController button in StateToButtonTable.Values){
        if (button.BehaviorData.Type != type){
          continue;
        }
        int parsed;
        if (int.TryParse(button.IndexText.text, out parsed)){
          indexes.Add(parsed);
        }
      }
      indexes.Sort();
      for(int i = 0; i < indexes.Count; i++){
        int number = i + 1;
        if (number != indexes[i] && !indexes.Contains(number)){
          result = number;
          break;
        }
      }
      if (result == 0){
        result = indexes.Count + 1;
      }
      return result;
    }

  }
}
