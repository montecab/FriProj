using System;
using PI;
using WW.SimpleJSON;

// might want to think about moving more trigger functionality out of trTrigger and into sub-classes.
// for now, this is just a value-object thing.

namespace Turing {
  public class trTriggerBeaconV2  {
    public class Parameters_t {
      public piRobotType      otherType          = piRobotType.UNKNOWN;
      public WWBeaconLevel    otherDistanceLevel = WWBeaconLevel.BEACON_LEVEL_OFF;
      public uint             otherID            = 0;
      public WWBeaconColor    otherColor         = WWBeaconColor.WW_ROBOT_COLOR_INVALID;
      public WWBeaconReceiver selfReceivers      = WWBeaconReceiver.WW_BEACON_RECEIVER_UNKNOWN;
      public bool             match              = true;
      
      #region serialization
      
      public Parameters_t Duplicate() {
        return Parameters_t.FromJson(ToJson());
      }
      
      public JSONClass ToJson() {
        JSONClass jsc = new JSONClass();
        jsc[TOKENS.ROBOT_TYPE    ].AsInt  = (int)otherType;
        jsc[TOKENS.DISTANCE_LEVEL].AsInt  = (int)otherDistanceLevel;
        jsc[TOKENS.ID            ].AsInt  = (int)otherID;
        jsc[TOKENS.COLOR         ].AsInt  = (int)otherColor;
        jsc[TOKENS.RECEIVERS     ].AsInt  = (int)selfReceivers;
        jsc[TOKENS.MATCH         ].AsBool = match;
        return jsc;
      }
      
      public static Parameters_t FromJson(JSONClass jsc) {
        Parameters_t ret = new Parameters_t();
        ret.otherType          = (piRobotType     )jsc[TOKENS.ROBOT_TYPE    ].AsInt;
        ret.otherDistanceLevel = (WWBeaconLevel   )jsc[TOKENS.DISTANCE_LEVEL].AsInt;
        ret.otherID            = (uint            )jsc[TOKENS.ID            ].AsInt;
        ret.otherColor         = (WWBeaconColor   )jsc[TOKENS.COLOR         ].AsInt;
        ret.selfReceivers      = (WWBeaconReceiver)jsc[TOKENS.RECEIVERS     ].AsInt;
        ret.match              =                   jsc[TOKENS.MATCH         ].AsBool;
        
        // some validating
        if (!Enum.IsDefined(typeof(piRobotType), ret.otherType)) {
          WWLog.logError("unrecognized robot type: " + ret.otherType);
          ret.otherType = new Parameters_t().otherType;
        }
        
        if (!Enum.IsDefined(typeof(WWBeaconColor), ret.otherColor)) {
          WWLog.logError("unrecognized robot color: " + ret.otherColor);
          ret.otherColor = new Parameters_t().otherColor;
        }
        
        return ret;
      }
      
      public override string ToString ()
      {
        string ret = "";
        
        ret += typeof(trTriggerBeaconV2)                   .ToString();
        ret += ".";
        ret += typeof(Parameters_t     )                   .ToString();
        ret += " otherType: "          + otherType         .ToString();
        ret += " otherDistanceLevel: " + otherDistanceLevel.ToString();
        ret += " otherID: "            + otherID           .ToString();
        ret += " otherColor: "         + otherColor        .ToString();
        ret += " selfReceivers: "      + selfReceivers     .ToString();
        ret += " match: "              + match             .ToString();
        
        return ret;
      }
      
      #endregion
    }
  }
}
