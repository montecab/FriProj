using UnityEngine;
using System.Collections.Generic;
using WW.SimpleJSON;

namespace Turing {
  public class trTransition : trBase {
  
    public trState   StateSource;
    public trState   StateTarget;
    public trTrigger Trigger;
    public bool      IsObscured = false;

    public delegate void TransitionActivationDelegate();
    public TransitionActivationDelegate OnTransitionActivated;

    public trTransition(){
      Trigger = new trTrigger(trTriggerType.NONE);
    }

    public void Copy(trTransition trans){
      Trigger = trans.Trigger.DeepCopy();
      IsObscured = trans.IsObscured;
    }
  
    #region serialization
    protected override void IntoJson(JSONClass jsc) {
      jsc[TOKENS.STATE_SOURCE_ID] = StateSource.UUID;
      jsc[TOKENS.STATE_TARGET_ID] = StateTarget.UUID;
      jsc[TOKENS.OBSCURED].AsBool = IsObscured;

      if(Trigger != null){
        jsc[TOKENS.TRIGGER        ] = Trigger.ToJson();
        jsc[TOKENS.COMMENT        ] = "( " + StateSource.UserFacingName + " ) ----" + Trigger.Type.ToString() + "----> ( " + StateTarget.UserFacingName + " )";
      }


      base.IntoJson(jsc);
    }
    
    protected override void OutOfJson(JSONClass jsc) {
      base.OutOfJson(jsc);
      if(jsc[TOKENS.TRIGGER]!= null){
        if (trFactory.HasItem(jsc[TOKENS.TRIGGER].AsObject[TOKENS.ID])) {
          Trigger = trFactory.GetItem<trTrigger>(jsc[TOKENS.TRIGGER].AsObject[TOKENS.ID]);
        }
        else {
          Trigger = trFactory.FromJson<trTrigger>(jsc[TOKENS.TRIGGER]);
        }
      }

      if(jsc[TOKENS.OBSCURED] != null){
        IsObscured = jsc[TOKENS.OBSCURED].AsBool;
      }

      StateSource = trFactory.GetItem <trState  >(jsc[TOKENS.STATE_SOURCE_ID]);
      StateTarget = trFactory.GetItem <trState  >(jsc[TOKENS.STATE_TARGET_ID]);
      
      StateSource.AddOutgoingTransition(this);
    }
    #endregion serialization

    public override string ToString () {
      return string.Format ("[trTransition: StateSource={0}, StateTarget={1}, Trigger={2}]", StateSource, StateTarget, Trigger);
    }

    // "cohort" is all the transitions which share the same Stource and Target states as this one.
    public List<trTransition> getCohort() {
      List<trTransition> ret = new List<trTransition>();
      foreach (trTransition trTrn in StateSource.OutgoingTransitions) {
        if (trTrn.StateTarget == StateTarget) {
          ret.Add(trTrn);
        }
      }

      if (ret.Count == 0) {
        WWLog.logError("empty cohort for transition: " + this.ToString());
      }

      return ret;
    }

  }
}

