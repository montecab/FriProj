using UnityEngine;
using UnityEngine.UI;
using Turing;
using WW.UGUI;
using TMPro;
using System.Collections.Generic;

namespace Turing{

  public enum trTankModeDirection {
    STOP = 0,
    FORWARD_FACE_STRAIGHT = 1,
    FORWARD_FACE_LEFT = 2,
    FORWARD_FACE_RIGHT = 3,
    BACKWARD_FACE_STRAIGHT = 101,
    BACKWARD_FACE_LEFT = 102,
    BACKWARD_FACE_RIGHT = 103,
    SPIN_LEFT = 201,
    SPIN_RIGHT = 202
  }

  public class trSimpleBehaviorEditor : MonoBehaviour {

    public wwToggleableButton TestButton;    
    public TextMeshProUGUI Description;
    public trProtoController ProtoController;

    private List<float> stateBehaviorParaStarted = new List<float>(); //stored state's para when opening behavior editor, used to check if the para changes when closing 
    public bool StateParaChanged{
      get{
        if(stateBehaviorParaStarted.Count != State.BehaviorParaValuesCount){
          return true;
        }
        for(int i = 0; i< stateBehaviorParaStarted.Count; ++i){
          if(stateBehaviorParaStarted[i] != State.GetBehaviorParameterValue(i)){
            return true;
          }
        }
        return false;
      }
    }

    private trState state;
    public trState State{
      get{
        return state;
      }
      set{
        SetupByState(value);
      }
    }

    void Awake(){
      if (piUnityUtils.IsWideScreen()){
        SetupWideView();
      }
    }

    void Start () {
      Initialize();
    }

    void onTestTogglePress(bool toTest){
      if (!ExecutOnRobot(toTest)){
        TestButton.Reset(); // didn't execute on robot, resetting
      }
    }

    public bool ExecutOnRobot(bool toExecute){
      // always stop whatever robot is currently doing
      piBotBase currentRobot = ProtoController.GetRobotToExecute(toExecute);
      if (currentRobot != null){
        state.SetActive(false, currentRobot); 
        ((piBotBo)ProtoController.CurRobot).cmd_bodyMotionStop();
        if(toExecute) {
          state.SetActive(true, currentRobot);   
        }
        return true; // executed on robot    
      }
      return false; // did not execute on robot
    }

    public void Reset(){
      ExecutOnRobot(false); // stop execution
      if (TestButton != null) TestButton.Reset(); // reset button status
    }

    protected virtual void Initialize(){
      if (TestButton != null) TestButton.OnValueChanged += onTestTogglePress;
    }

    protected virtual void SetupWideView(){}

    protected virtual void SetupByState(trState newState) {
      state = newState;
      Description.text = newState.Behavior.DescriptionLocalized;
      stateBehaviorParaStarted.Clear();
      for(int i = 0 ; i< state.BehaviorParaValuesCount; ++i){
        stateBehaviorParaStarted.Add(state.GetBehaviorParameterValue(i));
      }
      UpdateUI();
    }

    protected virtual void UpdateUI(){
    }
  }

}
