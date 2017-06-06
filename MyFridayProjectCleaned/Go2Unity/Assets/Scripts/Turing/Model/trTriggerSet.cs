using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using WW.SimpleJSON;

namespace Turing{

  public class trTriggerSet {
    public List<trTrigger> Triggers = new List<trTrigger>();

    public void ClearTriggers(){
      Triggers.Clear();
    }
    
    public bool containsTriggerOfType(trTriggerType trTT) {
      foreach (trTrigger trTrg in Triggers) {
        if (trTrg.Type == trTT) {
          return true;
        }
      }
      
      return false;
    }

    public JSONClass ToJson() {
      JSONClass jsc = new JSONClass();
      
      JSONArray jsmaps = new JSONArray();
      
      foreach (trTrigger trM in Triggers) {
        jsmaps.Add(trM.ToJson());
      }
      
      jsc[TOKENS.TRIGGERS] =jsmaps;
      
      return jsc;
    } 

    public static trTriggerSet FromJson(JSONClass jsc) {
      trTriggerSet ret = new trTriggerSet();
      
      foreach (JSONClass jsT in jsc[TOKENS.TRIGGERS].AsArray) {
        trTrigger trigger =  trFactory.FromJson<trTrigger>(jsT);
        ret.AddTrigger(trigger);
      }
      
      return ret;
    }

    public void CopyValue(trTriggerSet copy){
      if(this == copy){
        return;
      }
      ClearTriggers();
      for(int i = 0; i< copy.Triggers.Count; ++i){
        Triggers.Add(new trTrigger(copy.Triggers[i].Type));
      }
    }

    public void AddTrigger(trTrigger trigger){
      if(Triggers.Contains(trigger)){
        WWLog.logError("Adding the same trigger to the trigger set is not allowed. " + trigger.Type.ToString() );
        return;
      }

      if(!trigger.Type.IsAllowedToAddToTriggerSet()){
        WWLog.logError("Trigger type " + trigger.Type.ToString() + " is invalid to add to trigger set");
        return;
      }

//      Debug.LogError("add trigger" + trigger.Type);
      Triggers.Add(trigger);
    }

    public bool conditionMatches(piBotBase robot, trState state) {
      for(int i = 0; i< Triggers.Count; ++i){
        if(Triggers[i].conditionMatches(robot, state) == trTriggerConditionIsMet.YES){
          return true;
        }
      }
      return false;
    }

    public override string ToString ()
    {
      return string.Format ("[trTriggerSet: Triggers={0}]", Triggers);
    }

    #region Default Values

    public static trTriggerSet DefaultValue(trTriggerType type){
      trTriggerSet result = new trTriggerSet();
      switch(type){
        case trTriggerType.DISTANCE_SET:
          result.AddTrigger(new trTrigger(trTriggerType.DISTANCE_CENTER_NEAR));
          result.AddTrigger(new trTrigger(trTriggerType.DISTANCE_RIGHT_NEAR));
          result.AddTrigger(new trTrigger(trTriggerType.DISTANCE_LEFT_NEAR));
          break;
        case trTriggerType.BEACON_SET:
          result.AddTrigger(new trTrigger(trTriggerType.BEACON_RIGHT));
          result.AddTrigger(new trTrigger(trTriggerType.BEACON_BOTH));
          result.AddTrigger(new trTrigger(trTriggerType.BEACON_LEFT));
          break;
      }
      return result;
    }

    #endregion
    
    
  }


}
