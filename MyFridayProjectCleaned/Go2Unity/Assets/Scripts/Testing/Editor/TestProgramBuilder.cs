using Turing;
using System.Collections.Generic;

public class TestProgramBuilder{
  private trProgram _program;
  private Dictionary<string, trState> _states = new Dictionary<string, trState>();
  private List<trTransition> _transitions = new List<trTransition>();

  public trProgram Program{
    get{
      return _program;
    }
  }

  public TestProgramBuilder(){
    _program = new trProgram();
  }

  public TestProgramBuilder AddState(string stateName, trBehaviorType behaviorType){
    if(_states.ContainsKey(stateName)){
      throw new System.Exception("Already contains state named " + stateName);
    }
    trState state = new trState(stateName);
    state.Behavior = new trBehavior();
    state.Behavior.Type = behaviorType;
    _program.AddState(state);
    _states[stateName] = state;
    return this;
  }

  public TestProgramBuilder AddTransition(string sourceName, string targetName, trTriggerType triggerType = trTriggerType.NONE){
    trTransition t = new trTransition();
    if(triggerType != trTriggerType.NONE){
      t.Trigger = new trTrigger(triggerType);
    }
    t.StateSource = _states[sourceName];
    t.StateTarget = _states[targetName];
    _program.AddTransition(t);
    _transitions.Add(t);
    return this;
  }

  public TestProgramBuilder SetStart(string startStateName){
    _program.StateStart = _states[startStateName];
    return this;
  }

  public void SetOmni(string name){
    _program.StateOmni = _states[name];
  }

  public trState GetState(string name){
    return _states[name];
  }

  public trTransition GetTransition(int i){
    return _transitions[i];
  }
}
