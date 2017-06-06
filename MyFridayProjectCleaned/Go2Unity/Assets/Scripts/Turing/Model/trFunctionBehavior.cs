using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using WW.SimpleJSON;

namespace Turing{
  public class trFunctionBehavior : trBehavior
  {
    private trFunction functionProgram;
    public trFunction FunctionProgram{
      get{
        return functionProgram;
      }
      set{
        functionProgram = value;
      }
    }

    public piRobotType RobotType{
      get{
        return FunctionProgram.RobotType;
      }
    }

    private void updateHasFinished_Function(piBotBase robot){
      hasFinished = functionProgram.StateCurrent.Behavior.Type == trBehaviorType.FUNCTION_END;
    }

    protected override void addHandlers ()
    {
      base.addHandlers ();
      handlers.Add(trBehaviorType.FUNCTION,                      handleFunction);
    }

    protected void handleFunction (piBotBase robot, trBehavior.BehaveMode mode)
    {
      if (mode != BehaveMode.START) {
        return;
      }
        
      FunctionProgram.SetState(FunctionProgram.StateStart, robot);

      HasFinishedUpdater = updateHasFinished_Function;
        
    }

    public trFunctionBehavior(){
      Type = trBehaviorType.FUNCTION;
    }

    public override string UserFacingNameLocalized {
      get {
        return FunctionProgram.UserFacingName;
      }
    }

    protected override void OutOfJson (JSONClass jsc)
    {
      base.OutOfJson (jsc);
      FunctionProgram = trFactory.FromJson<trFunction>(jsc[TOKENS.FUNCTION_PROGRAM]);
    }

    protected override void IntoJson (JSONClass jsc)
    {
      base.IntoJson (jsc);
      jsc[TOKENS.FUNCTION_PROGRAM] = FunctionProgram.ToJson();
    }
  }

  public class trFunction: trProgram
  {
    public override bool OnRobotState (piBotBase robot)
    {
      if(StateCurrent != null && StateCurrent.Behavior.Type == trBehaviorType.FUNCTION_END){
        Reset(robot, true);
        return true;
      }
      return base.OnRobotState (robot);
    }

    public static trFunction NewFunction(piRobotType robotType) {
      trFunction ret = new trFunction();
      ret.Version = trProgram.CurrentVersion;
      ret.UUIDToTransitionTable.Clear();
      ret.UUIDToStateTable.Clear();
      ret.UUIDToBehaviorTable.Clear();
      ret.RobotType = robotType;      
      trState state = new trState();
      state.Behavior = trBehavior.GetDefaultBehavior(trBehaviorType.START_STATE);
      ret.AddState(state);
      ret.StateStart = state;
      ret.RecentLoadedTime = System.DateTime.Now.ToFileTimeUtc();     
      ret.UserFacingName = "Function";

      trState endState = new trState();
      endState.Behavior = trBehavior.GetDefaultBehavior(trBehaviorType.FUNCTION_END);
      endState.LayoutPosition = new Vector2(10,0);
      ret.AddState(endState);
      return ret;
    }
      
    public override bool Validate()
    {
      bool valid = base.Validate ();
      valid = valid && StateOmni == null; //omni is not allowed in functioins
      return valid;
    }

  }
}


 