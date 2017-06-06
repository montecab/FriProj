using UnityEngine;
using System.Collections.Generic;
using WW.SimpleJSON;

namespace Turing {
  
  
  public class trMapSetBehaviors : Singleton<trMapSetBehaviors> {
    public Dictionary<string, trBehavior> Behaviors = new Dictionary<string, trBehavior>();
    
    bool initialized = false;
    
    #region list management
    void init() {
      if (initialized) {
        return;
      }      
      initialized = true;
      
      // this code is GENERATED from this spreadsheet: https://docs.google.com/spreadsheets/d/1gccOs1yIe2lUmDRtA9-zyCjHAi_aCFkYPwXHsqK9djM/edit#gid=214382843
      // DO NOT EDIT HERE. edit the spreadsheet, then copy-paste the code.
      addBehavior(" { \"id\": \"13816143_2\", \"map_set\": { \"icon_name\": \"bright_idea\", \"maps\": [ { \"active\": false, \"actuator_point\": [ { \"x\": 0, \"y\": 0 }, { \"x\": 1, \"y\": 1 } ], \"actuator_type\": \"LED_TOP\", \"parameter_value\": 0.5384879, \"sensor_invert\": false, \"sensor_range\": { \"max\": 1, \"min\": 0 }, \"sensor_type\": \"TIME_IN_STATE\", \"simple_mode\": false }, { \"active\": false, \"actuator_point\": [ { \"x\": 0, \"y\": 0 }, { \"x\": 1, \"y\": 1 } ], \"actuator_type\": \"LED_TAIL\", \"parameter_value\": 5.048799, \"sensor_invert\": true, \"sensor_range\": { \"max\": 1, \"min\": 0 }, \"sensor_type\": \"TIME_IN_STATE\", \"simple_mode\": false }, { \"active\": false, \"actuator_point\": [ { \"x\": 0, \"y\": 0 }, { \"x\": 1, \"y\": 1 } ], \"actuator_type\": \"WHEEL_L\", \"parameter_value\": 5, \"sensor_invert\": false, \"sensor_range\": { \"max\": 1, \"min\": 0 }, \"sensor_type\": \"TIME_IN_STATE\", \"simple_mode\": false }, { \"active\": false, \"actuator_point\": [ { \"x\": 0, \"y\": 0 }, { \"x\": 1, \"y\": 1 } ], \"actuator_type\": \"WHEEL_R\", \"parameter_value\": 5, \"sensor_invert\": false, \"sensor_range\": { \"max\": 1, \"min\": 0 }, \"sensor_type\": \"TIME_IN_STATE\", \"simple_mode\": false }, { \"active\": true, \"actuator_point\": [ { \"x\": 0, \"y\": 0.8228706 }, { \"x\": 1, \"y\": 0.84 } ], \"actuator_type\": \"RGB_ALL_HUE\", \"parameter_value\": 5, \"sensor_invert\": false, \"sensor_range\": { \"max\": 1, \"min\": 0 }, \"sensor_type\": \"TIME_IN_STATE\", \"simple_mode\": false }, { \"active\": true, \"actuator_point\": [ { \"x\": 0, \"y\": 0 }, { \"x\": 1, \"y\": 1 } ], \"actuator_type\": \"RGB_ALL_VAL\", \"parameter_value\": 1.536934, \"sensor_invert\": false, \"sensor_range\": { \"max\": 1, \"min\": 0 }, \"sensor_type\": \"TIME_IN_STATE\", \"simple_mode\": false } ], \"user_facing_name\": \"pulsingMagenta\" }, \"type\": \"MAPSET\" } ");
    }
    
    public trBehavior GetBehavior(string uuid){
      trBehavior behavior = null;
      init();
      if (Behaviors.ContainsKey(uuid)) {
        behavior = Behaviors[uuid];
      }
      return behavior;
    }

    public Sprite GetIcon(trBehavior behavior){
      Sprite icon = null;
      init();
      if ((behavior != null) && behavior.isMapSetBehaviour() && Behaviors.ContainsKey(behavior.UUID)) {
        icon = trIconFactory.GetIcon(behavior);
      }
      return icon;
    }

    public List<trBehavior> GetAllBehaviors(){
      List<trBehavior> behaviors = new List<trBehavior>();
      foreach(trBehavior behavior in Behaviors.Values){
        behaviors.Add(behavior);
      }
      return behaviors;
    }
    
    private void addBehavior(string json) {
      if (!string.IsNullOrEmpty(json)){
        trBehavior behavior = trFactory.FromJson<trBehavior>(JSON.Parse(json));
        if (behavior != null) {
          Behaviors[behavior.UUID] = behavior;          
        }
      }
    }
    
    #endregion list management
  }
}
