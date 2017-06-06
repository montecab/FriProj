using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Turing{
  public class trStateMachineEvaluation  {

    // stack of states in program1 which we've already decided are equivalent or not
    private Stack<trState> comparedStatesP1 = new Stack<trState>();
    
    // map of states in program1 to equivalent states in program2
    public Dictionary<trState, trState> P1ToP2StateDict = new Dictionary<trState, trState>();

    //information about what in p1 don't match with p2
    public trStateMachineIncorrectInfo P1UnmatchedInfo = new trStateMachineIncorrectInfo();

    // amount by which we compare normalized parameters etc for "similarity".
    public const float EPSILON = 0.1f;

    public bool IsEquivalent(trProgram p1, trProgram p2){
      comparedStatesP1.Clear();
      P1ToP2StateDict.Clear();

      if (p1.UUIDToStateTable.Count != p2.UUIDToStateTable.Count){
        P1UnmatchedInfo.IsSame = false;
        return false;
      }

      if(!IsEquivalent(p1.StateStart, p2.StateStart,ref P1UnmatchedInfo)){
        P1UnmatchedInfo.IsSame = false;
        return false;
      }

      if(!IsEquivalent(p1.StateOmni, p2.StateOmni, ref P1UnmatchedInfo)){
        P1UnmatchedInfo.IsSame = false;
        return false;
      }

      P1UnmatchedInfo.IsSame = true;
      return true;
    }

    public bool IsEquivalent(trState s1, trState s2, ref trStateMachineIncorrectInfo errorInfo){
      if(s1 == null && s2 == null){
        return true;
      }

      if(s1 == null || s2 == null){
        return false;
      }
      bool isSame  = true;
      // It's true because if it were false we would have stopped execution.
      if(P1ToP2StateDict.ContainsKey(s1)){
        isSame = P1ToP2StateDict[s1] == s2;
        return isSame;
      }

      // cheap comparisons earlier
      if(s1.OutgoingTransitions.Count < s2.OutgoingTransitions.Count){
        errorInfo.WrongStates.Add(s1);
        return false;
      }
      
      if(!areSimilar(s1, s2)){
        if(!comparedStatesP1.Contains(s1)){
          errorInfo.WrongStates.Add(s1);
        }
        return false;
      }

      P1ToP2StateDict.Add(s1, s2);
      comparedStatesP1.Push(s1);

      //for each s1's outgoing transitions, try to find its equivalent transition 
      // in s2's transitions, if not found, find the closest transition and get the error info from that transiton branch
      List<trTransition> comparedTransitionsP2 = new List<trTransition>();
      foreach(trTransition t1 in s1.OutgoingTransitions){
        bool isFoundEquivalent = false;
        trStateMachineIncorrectInfo newErrorInfo = new trStateMachineIncorrectInfo();
        trTransition closestTransition = null;
        foreach(trTransition t2 in s2.OutgoingTransitions){
          if(!areSimilar(t1.Trigger, t2.Trigger)){
            continue;
          }
          trStateMachineIncorrectInfo tmpErrorInfo = new trStateMachineIncorrectInfo();
          if(IsEquivalent(t1.StateTarget, t2.StateTarget, ref tmpErrorInfo)){
            isFoundEquivalent = true;
            break;
          }

          // find the closets unchosen transition 
          if(!comparedTransitionsP2.Contains(t2) && (tmpErrorInfo.Similarity > newErrorInfo.Similarity || closestTransition == null)){
            closestTransition = t2;
            newErrorInfo = tmpErrorInfo;
          }
        }
        if(!isFoundEquivalent){
          // 0 check is important since we are checking the transitions recursively
          //so if not check the result will be the first transition which will link to the wrong transition/state
          //we cannot get the real cause then
          if(newErrorInfo.WrongTransitions.Count == 0 && newErrorInfo.WrongStates.Count == 0 ){ 
            errorInfo.WrongTransitions.Add(t1);
          }
          else{
            if(closestTransition != null && !comparedTransitionsP2.Contains(closestTransition)){
              comparedTransitionsP2.Add(closestTransition);
            }

            if(newErrorInfo.WrongTransitions.Count > 0 && !errorInfo.WrongTransitions.Contains(newErrorInfo.WrongTransitions[0])){
              foreach(trTransition trans in newErrorInfo.WrongTransitions ){
                errorInfo.WrongTransitions.Add(trans);
              }

            }
            if(newErrorInfo.WrongStates.Count >0){
              foreach(trState state in newErrorInfo.WrongStates){
                errorInfo.WrongStates.Add(state);
              }
            }           
          }
          errorInfo.Similarity += newErrorInfo.Similarity +1;

          while(comparedStatesP1.Peek() != s1){
            trState state = comparedStatesP1.Pop();
            P1ToP2StateDict.Remove(state);
          }
          isSame = false;
        }
      }

      if(!isSame){
        comparedStatesP1.Pop();
        P1ToP2StateDict.Remove(s1);
        return false;
      }

      errorInfo.Reset();
      return true;
    }

    private bool areSimilar(trTrigger t1, trTrigger t2 ){
      if(t1 == null || t2 == null){
        WWLog.logError("trigger shouldn't be null ");
        return false;
      }

      if(t1.Type != t2.Type){
        return false;
      }

      if(t1.Type.IsTriggerSet()){
        if(t1.TriggerSet.Triggers.Count != t2.TriggerSet.Triggers.Count){
          return false;
        }

        foreach(trTrigger trigger1 in t1.TriggerSet.Triggers){
          bool isfound = false;
          foreach(trTrigger trigger2 in t2.TriggerSet.Triggers){
            if(trigger1.Type == trigger2.Type){
              isfound = true;
              break;
            }
          }
          if(isfound == false){
            return false;
          }
        }
      }
      else if(trTrigger.Parameterized(t1.Type)){
        if(t1.Type == trTriggerType.TIME || t1.Type == trTriggerType.TIME_LONG){
          if(!piMathUtil.withinSpecifiedEpsilon(t1.ParameterValue, t2.ParameterValue, EPSILON)){
            return false;
          }
        }
        else if(!piMathUtil.withinSpecifiedEpsilon(t1.NormalizedValue, t2.NormalizedValue, EPSILON)){
          return false;
        }
      }
      return true;
    }  

    // note - this is really comparing behaviors, but we store the behavior param in the state.
    private bool areSimilar(trState s1, trState s2){
      if(s1.Behavior == null || s2.Behavior == null){
        WWLog.logError("state's behavior shouldn't be null ");
        return false;
      }
      bool isParaWrong = false;
      return s1.IsSimilarTo(s2, ref isParaWrong);
    }

  }

  public class trStateMachineIncorrectInfo{
    public bool IsSame = true;
    public int Similarity = 0;
    public List<trState> WrongStates = new List<trState>();
    public List<trTransition> WrongTransitions = new List<trTransition>();

    public void Reset(){
      IsSame = true;
      Similarity = 0;
      WrongStates.Clear();
      WrongTransitions.Clear();
    }

//    public override string ToString ()
//    {
//      string ret = "";
//      if(WrongStates != null){
//        if(IsStateParameterWrong){
//          ret += "State " + WrongStates.Behavior.UserFacingName + " has wrong para: " + WrongStates.BehaviorParameterValue + ". ";
//
//        }
//        else if(IsStateTransitionNumberWrong){
//          ret += "State " + WrongStates.Behavior.UserFacingName + " needs more transitions" ;
//        }
//        else{
//          ret += "Wrong Behavior: " + WrongStates.Behavior.UserFacingName;
//        }
//      }
//
//      if(WrongTransitions != null){
//        ret += "\n";
//        if(IsTriggerParameterWrong){
//          ret += "Trigger "+ WrongTransitions.Trigger.Type.ToString()
//                 +" between "+ WrongTransitions.StateSource.Behavior.UserFacingName + "->" + WrongTransitions.StateTarget.Behavior.UserFacingName
//                +" has wrong para. ";
//        }
//        else{
//          ret += "Trigger "+ WrongTransitions.Trigger.Type.ToString()
//            +" between "+ WrongTransitions.StateSource.Behavior.UserFacingName + "->" + WrongTransitions.StateTarget.Behavior.UserFacingName
//              +" has wrong type or is redundent.";
//        }
//      }
//
//      if(ret == ""){
//        ret += "Program matches";
//      }
//      //TODO: finish
//      return ret;
//    }
  }

}
