using UnityEngine;
using System.Collections.Generic;
using WW.SimpleJSON;
using PI;
using System.Text.RegularExpressions;

namespace Turing {

  public enum trTriggerType {
    BUTTON_MAIN            = 100,
    BUTTON_MAIN_NOT        = 101,
    BUTTON_1               = 102,
    BUTTON_1_NOT           = 103,
    BUTTON_2               = 104,
    BUTTON_2_NOT           = 105,
    BUTTON_3               = 106,
    BUTTON_3_NOT           = 107,
    BUTTON_ANY             = 108,
    BUTTON_NONE            = 109,
    
    // trigger set which contains multiple triggers
    DISTANCE_SET           = 200,
    BEACON_SET             = 201,
    BEACON_V2              = 202,

    DISTANCE_REAR_NEAR     = 300,
    DISTANCE_REAR_MIDDLE   = 301,
    DISTANCE_REAR_FAR      = 302,
    DISTANCE_REAR_NONE     = 303,

    DISTANCE_LEFT_NEAR     = 304,
    DISTANCE_LEFT_MIDDLE   = 305,
    DISTANCE_LEFT_FAR      = 306,
    DISTANCE_LEFT_NONE     = 307,

    DISTANCE_RIGHT_NEAR    = 308,
    DISTANCE_RIGHT_MIDDLE  = 309,
    DISTANCE_RIGHT_FAR     = 310,
    DISTANCE_RIGHT_NONE    = 311,

    DISTANCE_CENTER_NEAR   = 312,
    DISTANCE_CENTER_MIDDLE = 313,
    DISTANCE_CENTER_FAR    = 314,
    DISTANCE_CENTER_NONE   = 315,

    TIME                   = 400,
    TIME_RANDOM            = 401,
    BEHAVIOR_FINISHED      = 402,      // eg, animation.
    RANDOM                 = 403,
    NONE                   = 404,
    TIME_LONG              = 405,
    
    CLAP                   = 500,
    KIDNAP                 = 501,
    KIDNAP_NOT             = 502,
    STALL                  = 503,
    STALL_NOT              = 504,
    VOICE                  = 505,

    TRAVEL_LINEAR          = 600,          // parameterized
    TRAVEL_ANGULAR         = 601,         // parameterized
    TRAVELING_FORWARD      = 602,
    TRAVELING_BACKWARD     = 603,
    TRAVELING_STOPPED      = 604,

    LEAN_LEFT              = 700,
    LEAN_RIGHT             = 701,
    LEAN_FORWARD           = 702,
    LEAN_BACKWARD          = 703,
    LEAN_UPSIDE_DOWN       = 704,
    LEAN_UPSIDE_UP         = 705,
    DROP                   = 706,
    SHAKE                  = 707,
    SLIDE_X_POS            = 708,
    SLIDE_X_NEG            = 709,
    SLIDE_Y_POS            = 710,
    SLIDE_Y_NEG            = 711,
    SLIDE_Z_POS            = 712,
    SLIDE_Z_NEG            = 713,
   
    BEACON_LEFT            = 800,
    BEACON_RIGHT           = 801,
    BEACON_BOTH            = 802,
    BEACON_NONE            = 803,
    BEACON_LEFT_DOT        = 804,
    BEACON_RIGHT_DOT       = 805,
    BEACON_BOTH_DOT        = 806,
    BEACON_NONE_DOT        = 807,
    BEACON_LEFT_DASH       = 808,
    BEACON_RIGHT_DASH      = 809,
    BEACON_BOTH_DASH       = 810,
    BEACON_NONE_DASH       = 811,



    //deprecated
    IMMEDIATE              = 1000,              // todo: consider renaming to "TIME_0".
    TIME_1                 = 1001,
    TIME_3                 = 1002,
    TIME_5                 = 1003,
    TIME_10                = 1004,
  }

  public enum trTriggerArea{
    NOTHING       = 0,
    BASICS        = 1,
    NEXT_STEPS    = 2,
    ADVANCED      = 3,
    GOING_BEYOND  = 4,
    FUTURE        = 5,
    EXPERIMENTAL  = 6,
  }

  static class trTriggerTypeMethods {

    //used for deciding if show warnings
    public static bool IsMicrophone(this trTriggerType type){
      switch(type){
        case trTriggerType.CLAP:
        case trTriggerType.VOICE:
          return true;
      }
      return false;
    }
  
    public static string UserFacingName(this trTriggerArea area) {
      switch (area) {
        default:
          WWLog.logError("unhandled trigger area: " + area.ToString());
          return wwLoca.Format("@!@Misc@!@");
        case trTriggerArea.BASICS:
          return wwLoca.Format("@!@Basics@!@");
        case trTriggerArea.NEXT_STEPS:
          return wwLoca.Format("@!@Next Steps@!@");
        case trTriggerArea.ADVANCED:
          return wwLoca.Format("@!@Advanced@!@");
        case trTriggerArea.GOING_BEYOND:
          return wwLoca.Format("@!@Going Beyond@!@");
        case trTriggerArea.FUTURE:
          return wwLoca.Format("@!@Future App Release@!@");
        case trTriggerArea.EXPERIMENTAL:
          return wwLoca.Format("@!@Experimental@!@");
      }
    }
    
    public static bool IsShowToUser(this trTriggerArea area) {
      switch (area) {
        case trTriggerArea.EXPERIMENTAL:
          return trMultivariate.isYES(trMultivariate.trAppOption.UNLOCK_EXPERIMENTAL_TRIGGERS);
        case trTriggerArea.FUTURE:
          return false;
        default:
          return true;
      }
    }

    public static bool isRelatedToRobot(this trTriggerType type){
      switch(type){
        case trTriggerType.BUTTON_MAIN:
        case trTriggerType.CLAP:      
        case trTriggerType.BUTTON_1:
        case trTriggerType.BUTTON_2:
        case trTriggerType.BUTTON_3:
        case trTriggerType.DISTANCE_SET:
        case trTriggerType.VOICE:
        case trTriggerType.LEAN_LEFT:
        case trTriggerType.LEAN_RIGHT:
        case trTriggerType.LEAN_FORWARD:
        case trTriggerType.LEAN_BACKWARD:
        case trTriggerType.LEAN_UPSIDE_DOWN:
        case trTriggerType.LEAN_UPSIDE_UP:      
        case trTriggerType.BEACON_SET:
        case trTriggerType.BEACON_V2:
        case trTriggerType.DROP:
        case trTriggerType.SHAKE:
        case trTriggerType.KIDNAP:
        case trTriggerType.KIDNAP_NOT:
        case trTriggerType.STALL:
        case trTriggerType.STALL_NOT:
        case trTriggerType.TRAVELING_FORWARD:
        case trTriggerType.TRAVELING_BACKWARD:
        case trTriggerType.TRAVELING_STOPPED:
        case trTriggerType.BUTTON_MAIN_NOT:
        case trTriggerType.BUTTON_1_NOT:
        case trTriggerType.BUTTON_2_NOT:
        case trTriggerType.BUTTON_3_NOT:      
        case trTriggerType.TRAVEL_LINEAR:
        case trTriggerType.TRAVEL_ANGULAR:
        case trTriggerType.SLIDE_X_POS:
        case trTriggerType.SLIDE_X_NEG:
        case trTriggerType.SLIDE_Y_POS:
        case trTriggerType.SLIDE_Y_NEG:
        case trTriggerType.SLIDE_Z_POS:
        case trTriggerType.SLIDE_Z_NEG:
        case trTriggerType.BUTTON_ANY:
        case trTriggerType.BUTTON_NONE:
       return true;
      }
      return false;
    }
    
    private static List<trTriggerType> triggerEvaluationOrder = null;
    
    // triggers which appear earlier in this list get evaluated earlier.
    // so eg, DISTANCE_SET is evaluated before IMMEDIATE.
    // since DISTANCE is now level-triggered, that means if the distance trigger want to fire, it will win out.
    // TUR-850.
    public static int evaluationOrder(this trTriggerType trTT) {
      if (triggerEvaluationOrder == null) {
        triggerEvaluationOrder = new List<trTriggerType>() {
          trTriggerType.DISTANCE_SET,
          trTriggerType.BEACON_SET,
          trTriggerType.BEACON_V2,
          trTriggerType.VOICE,
          trTriggerType.TRAVELING_FORWARD,
          trTriggerType.TRAVELING_BACKWARD,
          trTriggerType.TRAVELING_STOPPED,
          trTriggerType.LEAN_BACKWARD,
          trTriggerType.LEAN_FORWARD,
          trTriggerType.LEAN_LEFT,
          trTriggerType.LEAN_RIGHT,
          trTriggerType.LEAN_UPSIDE_DOWN,
          trTriggerType.LEAN_UPSIDE_UP,
          trTriggerType.STALL,
          trTriggerType.KIDNAP,
          
          trTriggerType.IMMEDIATE,
          
          trTriggerType.TIME,
          trTriggerType.TIME_LONG,
          trTriggerType.TIME_RANDOM,
          trTriggerType.RANDOM,
          trTriggerType.BEHAVIOR_FINISHED,
          
          // specific buttons come before catch-all buttons
          trTriggerType.BUTTON_MAIN,
          trTriggerType.BUTTON_1,
          trTriggerType.BUTTON_2,
          trTriggerType.BUTTON_3,
          trTriggerType.BUTTON_MAIN_NOT,
          trTriggerType.BUTTON_1_NOT,
          trTriggerType.BUTTON_2_NOT,
          trTriggerType.BUTTON_3_NOT,
          trTriggerType.BUTTON_ANY,
          trTriggerType.BUTTON_NONE,
        };
      }
        
      int index = triggerEvaluationOrder.IndexOf(trTT);
      if (index < 0) {
        return triggerEvaluationOrder.Count + 1;
      }
      else {
        return index;
      }
    }

    public static trTriggerArea GetArea(this trTriggerType type) {
      switch (type) {
        default:
          WWLog.logError("Unhandled Trigger: " + type.ToString());
          return trTriggerArea.NOTHING;
          
        case trTriggerType.BUTTON_MAIN:
        case trTriggerType.IMMEDIATE:
        case trTriggerType.BEHAVIOR_FINISHED:
        case trTriggerType.CLAP:
          return trTriggerArea.BASICS;
              
        case trTriggerType.BUTTON_1:
        case trTriggerType.BUTTON_2:
        case trTriggerType.BUTTON_3:
        case trTriggerType.DISTANCE_SET:
        case trTriggerType.VOICE:
        case trTriggerType.LEAN_LEFT:
        case trTriggerType.LEAN_RIGHT:
        case trTriggerType.LEAN_FORWARD:
        case trTriggerType.LEAN_BACKWARD:
        case trTriggerType.LEAN_UPSIDE_DOWN:
        case trTriggerType.LEAN_UPSIDE_UP:
        case trTriggerType.TIME:
          return trTriggerArea.NEXT_STEPS;
                
        case trTriggerType.BEACON_SET:
        case trTriggerType.DROP:
        case trTriggerType.SHAKE:
        case trTriggerType.TIME_RANDOM:
        case trTriggerType.TIME_LONG:
        case trTriggerType.RANDOM:
        case trTriggerType.KIDNAP:
        case trTriggerType.KIDNAP_NOT:
        case trTriggerType.STALL:
        case trTriggerType.STALL_NOT:
        case trTriggerType.SLIDE_X_POS:
        case trTriggerType.SLIDE_X_NEG:
        case trTriggerType.SLIDE_Y_POS:
        case trTriggerType.SLIDE_Y_NEG:
        case trTriggerType.SLIDE_Z_POS:
        case trTriggerType.SLIDE_Z_NEG:
          return trTriggerArea.ADVANCED;
          
        case trTriggerType.TRAVELING_FORWARD:
        case trTriggerType.TRAVELING_BACKWARD:
        case trTriggerType.TRAVELING_STOPPED:
        case trTriggerType.BUTTON_MAIN_NOT:
        case trTriggerType.BUTTON_1_NOT:
        case trTriggerType.BUTTON_2_NOT:
        case trTriggerType.BUTTON_3_NOT:
          return trTriggerArea.GOING_BEYOND;
          
        case trTriggerType.TRAVEL_LINEAR:
        case trTriggerType.TRAVEL_ANGULAR:
        case trTriggerType.BUTTON_ANY:
        case trTriggerType.BUTTON_NONE:
          return trTriggerArea.FUTURE;
        
        case trTriggerType.BEACON_V2:
          return trTriggerArea.EXPERIMENTAL;
      }
    }

#if false
    public static trTriggerArea GetArea(this trTriggerType type){
      
      switch(type){
      case trTriggerType.BUTTON_MAIN:
      case trTriggerType.BUTTON_MAIN_NOT: 
      case trTriggerType.BUTTON_1:
      case trTriggerType.BUTTON_1_NOT:
      case trTriggerType.BUTTON_2:
      case trTriggerType.BUTTON_2_NOT:
      case trTriggerType.BUTTON_3:
      case trTriggerType.BUTTON_3_NOT:
      case trTriggerType.BUTTON_ANY:
      case trTriggerType.BUTTON_NONE:
        return trTriggerArea.BUTTON;            

      case trTriggerType.TIME:
      case trTriggerType.TIME_RANDOM:
      case trTriggerType.TIME_LONG:
      case trTriggerType.IMMEDIATE:
        return trTriggerArea.TIME;

      case trTriggerType.TRAVEL_LINEAR:
      case trTriggerType.TRAVEL_ANGULAR:
      case trTriggerType.TRAVELING_FORWARD:
      case trTriggerType.TRAVELING_BACKWARD:
      case trTriggerType.TRAVELING_STOPPED:
        return trTriggerArea.MOVE;

      case trTriggerType.CLAP:
      case trTriggerType.KIDNAP:
      case trTriggerType.KIDNAP_NOT:
      case trTriggerType.STALL:
      case trTriggerType.STALL_NOT:
      case trTriggerType.VOICE:
        return trTriggerArea.EVENT;

      case trTriggerType.BEHAVIOR_FINISHED:
      case trTriggerType.RANDOM:
      case trTriggerType.DISTANCE_SET:      
      case trTriggerType.BEACON_SET:
        return trTriggerArea.SPECIAL;
           
      case trTriggerType.LEAN_LEFT:
      case trTriggerType.LEAN_RIGHT:
      case trTriggerType.LEAN_FORWARD:
      case trTriggerType.LEAN_BACKWARD:
      case trTriggerType.LEAN_UPSIDE_DOWN:
      case trTriggerType.LEAN_UPSIDE_UP:
      case trTriggerType.DROP:
      case trTriggerType.SHAKE:
      case trTriggerType.SLIDE_X_POS:
      case trTriggerType.SLIDE_X_NEG:
      case trTriggerType.SLIDE_Y_POS:
      case trTriggerType.SLIDE_Y_NEG:
      case trTriggerType.SLIDE_Z_POS:
      case trTriggerType.SLIDE_Z_NEG:
        return trTriggerArea.GESTURE;           
      }

      return trTriggerArea.NOTHING;
    }
#endif // false


    public static bool IsAllowedToAddToTriggerSet(this trTriggerType type) {
      switch (type) {
        case trTriggerType.DISTANCE_CENTER_FAR:
        case trTriggerType.DISTANCE_CENTER_MIDDLE:
        case trTriggerType.DISTANCE_CENTER_NEAR:
        case trTriggerType.DISTANCE_CENTER_NONE:
        case trTriggerType.DISTANCE_LEFT_FAR:
        case trTriggerType.DISTANCE_LEFT_MIDDLE:
        case trTriggerType.DISTANCE_LEFT_NEAR:
        case trTriggerType.DISTANCE_LEFT_NONE:
        case trTriggerType.DISTANCE_RIGHT_FAR:
        case trTriggerType.DISTANCE_RIGHT_MIDDLE:
        case trTriggerType.DISTANCE_RIGHT_NEAR:
        case trTriggerType.DISTANCE_RIGHT_NONE:
        case trTriggerType.DISTANCE_REAR_FAR:
        case trTriggerType.DISTANCE_REAR_MIDDLE:
        case trTriggerType.DISTANCE_REAR_NEAR:
        case trTriggerType.DISTANCE_REAR_NONE:
        case trTriggerType.BEACON_BOTH:
        case trTriggerType.BEACON_LEFT:
        case trTriggerType.BEACON_NONE:
        case trTriggerType.BEACON_RIGHT:
        case trTriggerType.BEACON_BOTH_DOT:
        case trTriggerType.BEACON_LEFT_DOT:
        case trTriggerType.BEACON_NONE_DOT:
        case trTriggerType.BEACON_RIGHT_DOT:
        case trTriggerType.BEACON_BOTH_DASH:
        case trTriggerType.BEACON_LEFT_DASH:
        case trTriggerType.BEACON_NONE_DASH:
        case trTriggerType.BEACON_RIGHT_DASH:
          return true;
      }

      return false;
    }

    public static bool IsTriggerSet(this trTriggerType type) {
      switch (type) {
        case trTriggerType.BEACON_SET:
        case trTriggerType.DISTANCE_SET:
          return true;
      }

      return false;
    }

    public static bool IsPossiblyTimeBased(this trTriggerType type) {
      switch (type) {
        case trTriggerType.BEHAVIOR_FINISHED:   // might be time-based, might not. hence 'possibly'
        case trTriggerType.TIME:
        case trTriggerType.TIME_LONG:
        case trTriggerType.TIME_RANDOM:
          return true;
      }

      return false;
    }

  }

  public enum trTriggerConditionIsMet {
    UNKNOWN,
    YES,
    NO,
  }

  public class trTrigger : trTypedBase<trTriggerType> {

    public static float DistThreshFarNone = 44.0f;
    public static float DistThreshNearFar =  14.0f;

    public static float LeanTreshold      = 0.8f;
    public static float TravelingTreshold = 0.9f;
    public static float StoppedTreshold   = 0.5f;
    
    public  static int VOICE_CONFIDENCE_THRESHOLD = 10;
    
    private static Dictionary <trTriggerType, bool> infoEdgeTriggered = null;
    private float parameterValue = float.NaN;
    private float dynamicParameter = float.NaN;

    private static string cRobotNamePattern = "ROBOT";
    
    public trTriggerSet TriggerSet = null;
    
    public trTriggerBeaconV2.Parameters_t BeaconV2Params = null;

    public trTrigger DeepCopy(){
      return FromJson<trTrigger>(this.ToJson());
    }

    public static string GetDescriptionLocalized(trTriggerType type){
      if(!typeToDescription.ContainsKey(type)){
        WWLog.logError("Cannot find description for type " + type.ToString());
        return "description";
      }

      string des = wwLoca.Format(typeToDescription[type]);
      string replace = wwLoca.Format(trDataManager.Instance.CurrentRobotTypeSelected == piRobotType.DASH ? "Dash" : "Dot");
      string ret = des.Replace(cRobotNamePattern, replace);
      return ret;
    }

    public string UserFacingNameLocalized {
      get{
        if(!typeToUserFacingName.ContainsKey(Type)){
          WWLog.logError("Cannot find user facing name for type " + Type.ToString());
          return Type.ToString();
        }
        return wwLoca.Format(typeToUserFacingName[Type]);
      }
    }

    public string DescriptionLocalized{
      get{
        return GetDescriptionLocalized(Type);
      }
    }

    public string TelemetryDetail() {
      if (Type.IsTriggerSet()) {
        if (TriggerSet == null) {
          WWLog.logError("type is trigger set but TriggerSet is null");
          return "error";
        }
        else {
          string[] items = new string[TriggerSet.Triggers.Count];
          for (int n = 0; n < TriggerSet.Triggers.Count; ++n) {
            items[n] = TriggerSet.Triggers[n].Type.ToString();
          }
          return string.Join(", ", items);
        }
      }
      else {
        return ParameterValue.ToString();
      }
    }

    public Sprite ImageIcon{
      get{
        return trIconFactory.GetIcon(Type);
      }
    }

    public bool isLocked(){
      if(trDataManager.Instance.IsInNormalMissionMode){
        return false;
      }
      return !trRewardsManager.Instance.IsAvailableTrigger(this);
    }
    
    
    public trTrigger() {}

    public static bool AllowMultiple(trTriggerType type){
      bool result = false;
      result |= (type == trTriggerType.RANDOM);
      result |= (type == trTriggerType.BEACON_V2);
      return result;
    }

    private static Dictionary<trTriggerType, string> triggerTypeToUserFacingName = null;
    public static Dictionary<trTriggerType, string> typeToUserFacingName {
      get {
        if (triggerTypeToUserFacingName == null){
          triggerTypeToUserFacingName = new Dictionary<trTriggerType, string>();
          
          // DO NOT EDIT THE CODE BELOW! IT IS GENERATED BY THIS SPREADSHEET:
          // https://docs.google.com/spreadsheets/d/1gccOs1yIe2lUmDRtA9-zyCjHAi_aCFkYPwXHsqK9djM/edit#gid=327158842
          // to make changes, edit the spreadsheet, then manually copy-and-paste the code generated in the spreadsheet.
          triggerTypeToUserFacingName[trTriggerType.BUTTON_MAIN] = "@!@Top Button Pressed@!@";
          triggerTypeToUserFacingName[trTriggerType.BUTTON_MAIN_NOT ] = "@!@Top Button Released@!@";
          triggerTypeToUserFacingName[trTriggerType.BUTTON_1 ] = "@!@Button 1 Pressed@!@";
          triggerTypeToUserFacingName[trTriggerType.BUTTON_1_NOT] = "@!@Button 1 Released@!@";
          triggerTypeToUserFacingName[trTriggerType.BUTTON_2] = "@!@Button 2 Pressed@!@";
          triggerTypeToUserFacingName[trTriggerType.BUTTON_2_NOT] = "@!@Button 2 Released@!@";
          triggerTypeToUserFacingName[trTriggerType.BUTTON_3] = "@!@Button 3 Pressed@!@";
          triggerTypeToUserFacingName[trTriggerType.BUTTON_3_NOT] = "@!@Button 3 Released@!@";

          triggerTypeToUserFacingName[trTriggerType.DISTANCE_SET] = "@!@Obstacle Seen@!@";
          triggerTypeToUserFacingName[trTriggerType.BEACON_SET] = "@!@Robot Seen@!@";
          triggerTypeToUserFacingName[trTriggerType.BEACON_V2] = "@!@Beacon V2@!@";

          triggerTypeToUserFacingName[trTriggerType.TIME] = "@!@Wait For@!@";
          triggerTypeToUserFacingName[trTriggerType.TIME_RANDOM] = "@!@Random Wait@!@";
          triggerTypeToUserFacingName[trTriggerType.BEHAVIOR_FINISHED] = "@!@Auto@!@";
          triggerTypeToUserFacingName[trTriggerType.RANDOM] = "@!@Random Link@!@";
          triggerTypeToUserFacingName[trTriggerType.NONE] = "NONE";
          triggerTypeToUserFacingName[trTriggerType.TIME_LONG] = "@!@Long Wait@!@";
          triggerTypeToUserFacingName[trTriggerType.CLAP] = "@!@Clap Heard@!@";
          triggerTypeToUserFacingName[trTriggerType.KIDNAP] = "@!@Picked Up@!@";
          triggerTypeToUserFacingName[trTriggerType.KIDNAP_NOT] = "@!@Put Down@!@";
          triggerTypeToUserFacingName[trTriggerType.STALL] = "@!@Stuck@!@";
          triggerTypeToUserFacingName[trTriggerType.STALL_NOT] = "@!@Not Stuck@!@";
          triggerTypeToUserFacingName[trTriggerType.VOICE] = "@!@Voice Heard@!@";

          triggerTypeToUserFacingName[trTriggerType.TRAVELING_FORWARD] = "@!@Move Forward@!@";
          triggerTypeToUserFacingName[trTriggerType.TRAVELING_BACKWARD] = "@!@Move Backward@!@";
          triggerTypeToUserFacingName[trTriggerType.TRAVELING_STOPPED] = "@!@Stopped@!@";
          triggerTypeToUserFacingName[trTriggerType.LEAN_LEFT] = "@!@Lean Left@!@";
          triggerTypeToUserFacingName[trTriggerType.LEAN_RIGHT] = "@!@Lean Right@!@";
          triggerTypeToUserFacingName[trTriggerType.LEAN_FORWARD] = "@!@Face Down@!@";
          triggerTypeToUserFacingName[trTriggerType.LEAN_BACKWARD] = "@!@Face Up@!@";
          triggerTypeToUserFacingName[trTriggerType.LEAN_UPSIDE_DOWN] = "@!@Upside Down@!@";
          triggerTypeToUserFacingName[trTriggerType.LEAN_UPSIDE_UP] = "@!@Upright@!@";
          triggerTypeToUserFacingName[trTriggerType.DROP] = "@!@Drop@!@";
          triggerTypeToUserFacingName[trTriggerType.SHAKE] = "@!@Shake@!@";
          triggerTypeToUserFacingName[trTriggerType.SLIDE_X_POS] = "@!@Slide Forward@!@";
          triggerTypeToUserFacingName[trTriggerType.SLIDE_X_NEG] = "@!@Slide Backward@!@";
          triggerTypeToUserFacingName[trTriggerType.SLIDE_Y_POS] = "@!@Slide Left@!@";
          triggerTypeToUserFacingName[trTriggerType.SLIDE_Y_NEG] = "@!@Slide Right@!@";
          triggerTypeToUserFacingName[trTriggerType.SLIDE_Z_POS] = "@!@Move Up@!@";
          triggerTypeToUserFacingName[trTriggerType.SLIDE_Z_NEG] = "@!@Move Down@!@";

          triggerTypeToUserFacingName[trTriggerType.IMMEDIATE] = "@!@Instant@!@";
        }
        return triggerTypeToUserFacingName;
      }
    }


    private static Dictionary<trTriggerType, string> triggerTypeToDescription = null;
    private static Dictionary<trTriggerType, string> typeToDescription {
      get {
        if (triggerTypeToDescription == null){
          triggerTypeToDescription = new Dictionary<trTriggerType, string>();
          
          // DO NOT EDIT THE CODE BELOW! IT IS GENERATED BY THIS SPREADSHEET:
          // https://docs.google.com/spreadsheets/d/1gccOs1yIe2lUmDRtA9-zyCjHAi_aCFkYPwXHsqK9djM/edit#gid=327158842
          // to make changes, edit the spreadsheet, then manually copy-and-paste the code generated in the spreadsheet.    
          triggerTypeToDescription[trTriggerType.BUTTON_MAIN] = "@!@ROBOT will go to the next state when you press the <b>big white button</b> on ROBOT's head.@!@";
          triggerTypeToDescription[trTriggerType.BUTTON_MAIN_NOT ] = "@!@ROBOT will go to the next state when you release the <b>big white button</b> on ROBOT's head.@!@";
          triggerTypeToDescription[trTriggerType.BUTTON_1 ] = "@!@ROBOT will go to the next state when you press the <b>orange 1-dot button</b> on ROBOT's head.@!@";
          triggerTypeToDescription[trTriggerType.BUTTON_1_NOT] = "@!@ROBOT will go to the next state when you release the <b>orange 1-dot button</b> on ROBOT's head.@!@";
          triggerTypeToDescription[trTriggerType.BUTTON_2] = "@!@ROBOT will go to the next state when you press the <b>orange 2-dot button</b> on ROBOT's head.@!@";
          triggerTypeToDescription[trTriggerType.BUTTON_2_NOT] = "@!@ROBOT will go to the next state when you release the <b>orange 2-dot button</b> on ROBOT's head.@!@";
          triggerTypeToDescription[trTriggerType.BUTTON_3] = "@!@ROBOT will go to the next state when you press the <b>orange 3-dot button</b> on ROBOT's head.@!@";
          triggerTypeToDescription[trTriggerType.BUTTON_3_NOT] = "@!@ROBOT will go to the next state when you release the <b>orange 3-dot button</b> on ROBOT's head.@!@";

          triggerTypeToDescription[trTriggerType.DISTANCE_SET] = "@!@ROBOT will go to the next state when it <b>sees obstacles</b> in front or in back.@!@";
          triggerTypeToDescription[trTriggerType.BEACON_SET] = "@!@ROBOT will go to the next state when it <b>sees other robots</b> in front.@!@";
          triggerTypeToDescription[trTriggerType.BEACON_V2] = "@!@Experimental!@!@";

          triggerTypeToDescription[trTriggerType.TIME] = "@!@ROBOT will go to the next state after waiting the <b>number of seconds</b> you choose.@!@";
          triggerTypeToDescription[trTriggerType.TIME_RANDOM] = "@!@ROBOT will go to the next state after waiting <b>between zero and the number of seconds</b> you choose.@!@";
          triggerTypeToDescription[trTriggerType.BEHAVIOR_FINISHED] = "@!@ROBOT will go to the next state after <b>finishing the current action</b>.@!@";
          triggerTypeToDescription[trTriggerType.RANDOM] = "@!@ROBOT will <b>randomly</b> go to one of the states linked with this cue. <b>Tip</b>: You can link to multiple states with this cue!@!@";
          triggerTypeToDescription[trTriggerType.NONE] = "NONE";
          triggerTypeToDescription[trTriggerType.TIME_LONG] = "@!@ROBOT will go to the next state after waiting the <b>hours, minutes, and seconds</b> you choose.@!@";
          triggerTypeToDescription[trTriggerType.CLAP] = "@!@ROBOT will go to the next state when a <b>clap is heard</b>.@!@";
          triggerTypeToDescription[trTriggerType.KIDNAP] = "@!@ROBOT will go to the next state after <b>being picked up</b>.@!@";
          triggerTypeToDescription[trTriggerType.KIDNAP_NOT] = "@!@ROBOT will go to the next state after being <b>put down</b>.@!@";
          triggerTypeToDescription[trTriggerType.STALL] = "@!@ROBOT will go to the next state when <b>stuck</b>.@!@";
          triggerTypeToDescription[trTriggerType.STALL_NOT] = "@!@ROBOT will go to the next state when <b>not stuck</b>.@!@";
          triggerTypeToDescription[trTriggerType.VOICE] = "@!@ROBOT will go to the next state when a <b>voice is heard</b>.@!@";

          triggerTypeToDescription[trTriggerType.TRAVELING_FORWARD] = "@!@ROBOT will go to the next state when <b>moving forward</b>.@!@";
          triggerTypeToDescription[trTriggerType.TRAVELING_BACKWARD] = "@!@ROBOT will go to the next state when <b>moving backward</b>.@!@";
          triggerTypeToDescription[trTriggerType.TRAVELING_STOPPED] = "@!@ROBOT will go to the next state when <b>not moving</b>.@!@";
          triggerTypeToDescription[trTriggerType.LEAN_LEFT] = "@!@ROBOT will go to the next state when <b>tilted left</b>.@!@";
          triggerTypeToDescription[trTriggerType.LEAN_RIGHT] = "@!@ROBOT will go to the next state when <b>tilted right</b>.@!@";
          triggerTypeToDescription[trTriggerType.LEAN_FORWARD] = "@!@ROBOT will go to the next state when <b>tilted face down</b>.@!@";
          triggerTypeToDescription[trTriggerType.LEAN_BACKWARD] = "@!@ROBOT will go to the next state when <b>tilted face up</b>.@!@";
          triggerTypeToDescription[trTriggerType.LEAN_UPSIDE_DOWN] = "@!@ROBOT will go to the next state when <b>upside down</b>.@!@";
          triggerTypeToDescription[trTriggerType.LEAN_UPSIDE_UP] = "@!@ROBOT will go to the next state when <b>upright</b>.@!@";
          triggerTypeToDescription[trTriggerType.DROP] = "@!@ROBOT will go to the next state when <b>dropped</b>.@!@";
          triggerTypeToDescription[trTriggerType.SHAKE] = "@!@ROBOT will go to the next state when <b>shaken</b>.@!@";
          triggerTypeToDescription[trTriggerType.SLIDE_X_POS] = "@!@ROBOT will go to the next state after being moved <b>forward</b> (while facing away from you).@!@";
          triggerTypeToDescription[trTriggerType.SLIDE_X_NEG] = "@!@ROBOT will go to the next state after being moved <b>backward</b> (while facing away from you).@!@";
          triggerTypeToDescription[trTriggerType.SLIDE_Y_POS] = "@!@ROBOT will go to the next state after being moved <b>to the left</b> (while facing away from you).@!@";
          triggerTypeToDescription[trTriggerType.SLIDE_Y_NEG] = "@!@ROBOT will go to the next state after being moved <b>to the right</b> (while facing away from you).@!@";
          triggerTypeToDescription[trTriggerType.SLIDE_Z_POS] = "@!@ROBOT will go to the next state after being moved <b>up</b> (while facing away from you).@!@";
          triggerTypeToDescription[trTriggerType.SLIDE_Z_NEG] = "@!@ROBOT will go to the next state after being moved <b>down</b> (while facing away from you).@!@";

          triggerTypeToDescription[trTriggerType.IMMEDIATE] = "@!@ROBOT will go to the next state <b>immediately</b>.@!@";

          trUIUtil.normalizeUIText<trTriggerType>(triggerTypeToDescription);
        }
        return triggerTypeToDescription;
      }
    }
    
    private static Dictionary<trTriggerType, float> parameterDefaults;
    public static Dictionary<trTriggerType, float> ParameterDefaultsDict {
      get {
        if (parameterDefaults == null) {
          parameterDefaults = new Dictionary<trTriggerType, float>();
          
          parameterDefaults[trTriggerType.TIME          ] = 1.0f;
          parameterDefaults[trTriggerType.TRAVEL_LINEAR ] = 50.0f;
          parameterDefaults[trTriggerType.TRAVEL_ANGULAR] = 180.0f;
          parameterDefaults[trTriggerType.TIME_RANDOM   ] = 5.0f;
          parameterDefaults[trTriggerType.TIME_LONG     ] = 30.0f;
        }
        return parameterDefaults;
      }
    }

    private static Dictionary<trTriggerType, wwRange> parametersRanges;
    public static Dictionary<trTriggerType, wwRange> ParameterRangesDict {
      get {
        if (parametersRanges == null){
          parametersRanges = new Dictionary<trTriggerType, wwRange>();
          
          parametersRanges[trTriggerType.TIME          ] = new wwRange(0, 10);
          parametersRanges[trTriggerType.TRAVEL_LINEAR ] = new wwRange(0, 100);
          parametersRanges[trTriggerType.TRAVEL_ANGULAR] = new wwRange(-180, 180);
          parametersRanges[trTriggerType.TIME_RANDOM   ] = new wwRange(0, 10);
          parametersRanges[trTriggerType.TIME_LONG     ] = new wwRange(2, 2*(Mathf.Pow(2,16) - 1));

        }
        return parametersRanges;
      }
    }

    public float NormalizedValue{
      get{
        wwRange range = ParameterRangesDict[Type];
        return range.Normalize(ParameterValue);
      }
    }

    public float ParameterValue {
      get {
        float result = parameterValue;

        if (float.IsNaN(parameterValue)) {
          if (ParameterDefaultsDict.ContainsKey(Type)) {
            result = ParameterDefaultsDict[Type];
          }
          else {
            result = 0;
            if (wwDoOncePerTypeVal<string>.doIt("no such param: " + Type)) {
              WWLog.logWarn("no such default param for type: " + Type);
            }
          }
        }

        return result;
      } set {
        parameterValue = value;
      }
    }

    public bool IsParameterSet {
      get {
        return !float.IsNaN(parameterValue);
      }
    }

    public bool IsDynamicData {
      get {
        return Type == trTriggerType.TIME_RANDOM;
      }
    }

    public float DynamicParameter {
      get {
        if (float.IsNaN(dynamicParameter)){
          dynamicParameter = getDynamicParameterValue();
        }
        return this.dynamicParameter;
      }
      set {
        dynamicParameter = value;
      }
    }

    trTriggerConditionIsMet isConditionMet = trTriggerConditionIsMet.UNKNOWN;
    
    public void CopyValue(trTrigger src) {
      Type = src.Type;
      if (src.IsParameterSet) {
        ParameterValue = src.ParameterValue;
      }

      if (Type.IsTriggerSet()) {
        if (TriggerSet == null) {
          TriggerSet = trTriggerSet.DefaultValue(Type);
        }

        TriggerSet.CopyValue(src.TriggerSet);
      }
      
      if (src.BeaconV2Params != null) {
        BeaconV2Params = src.BeaconV2Params.Duplicate();
      }
    }

    public bool IsParaDifferent(trTrigger trigger){
      if(trigger.Type != this.Type){
        return false;
      }  

      return !IsSameTo(trigger);
    }
      
    public bool IsSameTo(trTrigger trigger){
      if(this == trigger){
        return true;
      }

      if(this.Type != trigger.Type){
        return false;
      }

      if(trTrigger.Parameterized(Type)
        && ParameterValue != trigger.ParameterValue){
        return false;
      }

      if(Type.IsTriggerSet()){
        if(TriggerSet.Triggers.Count != trigger.TriggerSet.Triggers.Count){
          return false;
        }

        foreach(trTrigger trigger1 in TriggerSet.Triggers){
          bool isfound = false;
          foreach(trTrigger trigger2 in trigger.TriggerSet.Triggers){
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

      return true;
    }


    public trTrigger(trTriggerType typeVal) : base(typeVal) {
      //remove because if the trigger set is set in another outgoing transition, we dont want this default value
      //default value should be set if there is no other outgoing transition with this trigger set
      if (typeVal.IsTriggerSet()) {
        TriggerSet = new trTriggerSet();
      }
      
      if (typeVal == trTriggerType.BEACON_V2) {
        BeaconV2Params = new trTriggerBeaconV2.Parameters_t();
      }
    }

    public trTrigger(trTriggerType typeVal, float paramValue) : this(typeVal) {
      ParameterValue = paramValue;

      if (!Parameterized(typeVal)) {
        WWLog.logError("new trigger of type " + typeVal.ToString() + " should not be initialized w/ a parameter value!");
      }
    }
    
    #region serialization
    protected override void IntoJson(JSONClass jsc) {
      if (Parameterized(Type)) {
        jsc[TOKENS.PARAMETER_VALUE].AsFloat = ParameterValue;
      }

      if (Type.IsTriggerSet()) {
        jsc[TOKENS.TRIGGER_SET] = TriggerSet.ToJson();
      }
      
      if (Type == trTriggerType.BEACON_V2) {
        jsc[TOKENS.PARAMETERS] = BeaconV2Params.ToJson();
      }

      base.IntoJson(jsc);
    }

    protected override void OutOfJson(JSONClass jsc) {
      base.OutOfJson(jsc);

      if (Parameterized(Type)) {
        ParameterValue = jsc[TOKENS.PARAMETER_VALUE].AsFloat;
      }

      if (Type.IsTriggerSet()) {
        TriggerSet = trTriggerSet.FromJson(jsc[TOKENS.TRIGGER_SET].AsObject);
      }

      if (Type == trTriggerType.BEACON_V2) {
        BeaconV2Params = trTriggerBeaconV2.Parameters_t.FromJson(jsc[TOKENS.PARAMETERS].AsObject);
      }

      fixDeprecatedTriggerTypes();
    }

    private void fixDeprecatedTriggerTypes() {
      trTriggerType validType = Type;
      float parameterValue = float.NaN;

      switch (Type) {
        case trTriggerType.TIME_1:
          parameterValue = 1;
          validType = trTriggerType.TIME;
          break;

        case trTriggerType.TIME_3:
          parameterValue = 3;
          validType = trTriggerType.TIME;
          break;

        case trTriggerType.TIME_5:
          parameterValue = 5;
          validType = trTriggerType.TIME;
          break;

        case trTriggerType.TIME_10:
          parameterValue = 10;
          validType = trTriggerType.TIME;
          break;
      }

      if (!float.IsNaN(parameterValue)) {
        WWLog.logInfo(string.Format("Promoting trigger type {0} to {1} with parameter {2}", Type, validType, parameterValue));
        Type = validType;
        ParameterValue = parameterValue;
      }
    }
    #endregion serialization

    public override string ToString () {
      return string.Format ("[trTrigger: {0} infoEdgeTriggered={1}, isConditionMet={2}, TriggerSet={3}, Primed={4}]", Type.ToString(), infoEdgeTriggered, isConditionMet, TriggerSet, Primed);
    }

    // "Primed" means "Ready to fire as soon as the condition matches".
    public bool Primed {
      set {
        isConditionMet = (value ? trTriggerConditionIsMet.NO : trTriggerConditionIsMet.UNKNOWN);
      }

      get {
        return isConditionMet == trTriggerConditionIsMet.NO;
      }
    }

    public bool Evaluate(piBotBase robot, trState state) {
      bool ret = false;
      trTriggerConditionIsMet isMet = conditionMatches(robot, state);
      
      bool isOmni = (state.Behavior.Type == Turing.trBehaviorType.OMNI);

      if (EdgeOnly(Type) || isOmni) {
        if (Primed && (isMet == trTriggerConditionIsMet.YES)) {
          ret = true;
        }
      }
      else {
        ret = (isMet == trTriggerConditionIsMet.YES);
      }

      isConditionMet = isMet;

      //TUR-961: because we handle most OMNI triggers as Edge, for this particular case isConditionMet never goes to NO, so Primed will always be false
      if (isOmni && Type == trTriggerType.TIME && parameterValue == 0){
        isConditionMet = trTriggerConditionIsMet.NO;
      }
      return ret;
    }
    
    private static bool IsValidRobotTypeForTrigger(piRobotType weHaveThisRobotType, piRobotType triggerWantsThisRobotType) {
      if (trMultivariate.Instance.getOptionValue(trMultivariate.trAppOption.MULTIBOT_TRIGGERS) == trMultivariate.trAppOptionValue.YES) {
        return true;
      }
      else {
        return weHaveThisRobotType == triggerWantsThisRobotType;
      }
    }

    public static bool ShowToUser(trTriggerType t, piRobotType robotType) {
    
      bool ret;

      switch (t) {
        // ROBOT-AGNOSTIC
        default:
          ret = true;
          break;

        // DEPRECATED TRIGGERS
        case trTriggerType.TIME_1:
        case trTriggerType.TIME_3:
        case trTriggerType.TIME_5:
        case trTriggerType.TIME_10:
        case trTriggerType.DISTANCE_CENTER_FAR:
        case trTriggerType.DISTANCE_CENTER_MIDDLE:
        case trTriggerType.DISTANCE_CENTER_NEAR:
        case trTriggerType.DISTANCE_CENTER_NONE:
        case trTriggerType.DISTANCE_LEFT_FAR:
        case trTriggerType.DISTANCE_LEFT_MIDDLE:
        case trTriggerType.DISTANCE_LEFT_NEAR:
        case trTriggerType.DISTANCE_LEFT_NONE:
        case trTriggerType.DISTANCE_RIGHT_FAR:
        case trTriggerType.DISTANCE_RIGHT_MIDDLE:
        case trTriggerType.DISTANCE_RIGHT_NEAR:
        case trTriggerType.DISTANCE_RIGHT_NONE:
        case trTriggerType.DISTANCE_REAR_FAR:
        case trTriggerType.DISTANCE_REAR_MIDDLE:
        case trTriggerType.DISTANCE_REAR_NEAR:
        case trTriggerType.DISTANCE_REAR_NONE:
        case trTriggerType.BEACON_BOTH:
        case trTriggerType.BEACON_LEFT:
        case trTriggerType.BEACON_NONE:
        case trTriggerType.BEACON_RIGHT:
        case trTriggerType.BEACON_BOTH_DOT:
        case trTriggerType.BEACON_LEFT_DOT:
        case trTriggerType.BEACON_NONE_DOT:
        case trTriggerType.BEACON_RIGHT_DOT:
        case trTriggerType.BEACON_BOTH_DASH:
        case trTriggerType.BEACON_LEFT_DASH:
        case trTriggerType.BEACON_NONE_DASH:
        case trTriggerType.BEACON_RIGHT_DASH:
        case trTriggerType.NONE:
//        vikas is using these two for power-on SM:
//        case trTriggerType.BUTTON_ANY:
//        case trTriggerType.BUTTON_NONE:
        case trTriggerType.TRAVEL_ANGULAR:
        case trTriggerType.TRAVEL_LINEAR:
          ret = false;
          break;

        // DASH-ONLY
        case trTriggerType.TRAVELING_BACKWARD:
        case trTriggerType.TRAVELING_FORWARD:
        case trTriggerType.TRAVELING_STOPPED:
        case trTriggerType.KIDNAP:
        case trTriggerType.KIDNAP_NOT:
        case trTriggerType.BEACON_SET:
        case trTriggerType.BEACON_V2:
        case trTriggerType.DISTANCE_SET:
        case trTriggerType.STALL:
        case trTriggerType.STALL_NOT:
          ret = IsValidRobotTypeForTrigger(robotType, piRobotType.DASH);
          break;
       
        // DOT-ONLY
        case trTriggerType.LEAN_LEFT:
        case trTriggerType.LEAN_RIGHT:
        case trTriggerType.LEAN_FORWARD:
        case trTriggerType.LEAN_BACKWARD:
        case trTriggerType.LEAN_UPSIDE_DOWN:
        case trTriggerType.LEAN_UPSIDE_UP:
        case trTriggerType.SLIDE_X_POS:
        case trTriggerType.SLIDE_X_NEG:
        case trTriggerType.SLIDE_Y_POS:
        case trTriggerType.SLIDE_Y_NEG:
        case trTriggerType.SLIDE_Z_POS:
        case trTriggerType.SLIDE_Z_NEG:
        case trTriggerType.DROP:
        case trTriggerType.SHAKE:
          ret = IsValidRobotTypeForTrigger(robotType, piRobotType.DOT);
          break;
      }

      return ret;
    }


    public static bool EdgeOnly(trTriggerType t) {
      // all triggers are edge-triggered except for a handful.
      if (infoEdgeTriggered == null) {
        infoEdgeTriggered = new Dictionary<trTriggerType, bool>();

        // set defaults
        foreach (trTriggerType enumCandidate in System.Enum.GetValues(typeof(trTriggerType))) {
          infoEdgeTriggered.Add(enumCandidate, true);
        }

        // and exceptions
        infoEdgeTriggered[trTriggerType.IMMEDIATE             ] = false;
        infoEdgeTriggered[trTriggerType.TIME                  ] = false;
        infoEdgeTriggered[trTriggerType.TIME_LONG             ] = false;
        infoEdgeTriggered[trTriggerType.TIME_RANDOM           ] = false;
        infoEdgeTriggered[trTriggerType.RANDOM                ] = false;
        infoEdgeTriggered[trTriggerType.BEHAVIOR_FINISHED     ] = false;
        
        bool TUR_673 = true;
        
        if (TUR_673) {
          infoEdgeTriggered[trTriggerType.DISTANCE_SET          ] = false;
          infoEdgeTriggered[trTriggerType.DISTANCE_LEFT_FAR     ] = false;
          infoEdgeTriggered[trTriggerType.DISTANCE_LEFT_MIDDLE  ] = false;
          infoEdgeTriggered[trTriggerType.DISTANCE_LEFT_NEAR    ] = false;
          infoEdgeTriggered[trTriggerType.DISTANCE_LEFT_NONE    ] = false;
          infoEdgeTriggered[trTriggerType.DISTANCE_CENTER_FAR   ] = false;
          infoEdgeTriggered[trTriggerType.DISTANCE_CENTER_MIDDLE] = false;
          infoEdgeTriggered[trTriggerType.DISTANCE_CENTER_NEAR  ] = false;
          infoEdgeTriggered[trTriggerType.DISTANCE_CENTER_NONE  ] = false;
          infoEdgeTriggered[trTriggerType.DISTANCE_RIGHT_FAR    ] = false;
          infoEdgeTriggered[trTriggerType.DISTANCE_RIGHT_MIDDLE ] = false;
          infoEdgeTriggered[trTriggerType.DISTANCE_RIGHT_NEAR   ] = false;
          infoEdgeTriggered[trTriggerType.DISTANCE_RIGHT_NONE   ] = false;
          infoEdgeTriggered[trTriggerType.DISTANCE_REAR_FAR     ] = false;
          infoEdgeTriggered[trTriggerType.DISTANCE_REAR_MIDDLE  ] = false;
          infoEdgeTriggered[trTriggerType.DISTANCE_REAR_NEAR    ] = false;
          infoEdgeTriggered[trTriggerType.DISTANCE_REAR_NONE    ] = false;
          
          infoEdgeTriggered[trTriggerType.BEACON_SET            ] = false;
          infoEdgeTriggered[trTriggerType.BEACON_V2             ] = false;
          infoEdgeTriggered[trTriggerType.BEACON_LEFT           ] = false;
          infoEdgeTriggered[trTriggerType.BEACON_RIGHT          ] = false;
          infoEdgeTriggered[trTriggerType.BEACON_BOTH           ] = false;
          infoEdgeTriggered[trTriggerType.BEACON_NONE           ] = false;
          infoEdgeTriggered[trTriggerType.BEACON_LEFT_DOT       ] = false;
          infoEdgeTriggered[trTriggerType.BEACON_RIGHT_DOT      ] = false;
          infoEdgeTriggered[trTriggerType.BEACON_BOTH_DOT       ] = false;
          infoEdgeTriggered[trTriggerType.BEACON_NONE_DOT       ] = false;
          infoEdgeTriggered[trTriggerType.BEACON_LEFT_DASH      ] = false;
          infoEdgeTriggered[trTriggerType.BEACON_RIGHT_DASH     ] = false;
          infoEdgeTriggered[trTriggerType.BEACON_BOTH_DASH      ] = false;
          infoEdgeTriggered[trTriggerType.BEACON_NONE_DASH      ] = false;
          
          infoEdgeTriggered[trTriggerType.LEAN_LEFT             ] = false;
          infoEdgeTriggered[trTriggerType.LEAN_RIGHT            ] = false;
          infoEdgeTriggered[trTriggerType.LEAN_FORWARD          ] = false;
          infoEdgeTriggered[trTriggerType.LEAN_BACKWARD         ] = false;
          infoEdgeTriggered[trTriggerType.LEAN_UPSIDE_DOWN      ] = false;
          infoEdgeTriggered[trTriggerType.LEAN_UPSIDE_UP        ] = false;
          
          infoEdgeTriggered[trTriggerType.STALL                 ] = false;
          infoEdgeTriggered[trTriggerType.KIDNAP                ] = false;
          infoEdgeTriggered[trTriggerType.TRAVELING_FORWARD     ] = false;
          infoEdgeTriggered[trTriggerType.TRAVELING_BACKWARD    ] = false;
          infoEdgeTriggered[trTriggerType.TRAVELING_STOPPED     ] = false;

          infoEdgeTriggered[trTriggerType.VOICE                 ] = false;
        }
        
      }

      return infoEdgeTriggered[t];
    }

    public static bool Parameterized(trTriggerType t) {
      return ParameterRangesDict.ContainsKey(t);
    }

    public trTriggerConditionIsMet conditionMatches(piBotBase robot, trState state) {
      if (robot == null) {
        return trTriggerConditionIsMet.UNKNOWN;
      }

      piBotBo bot = (piBotBo)robot;
      bool ret = false;
      
      switch (Type) {
        default:
          if (wwDoOncePerTypeVal<trTriggerType>.doIt(Type)) {
            WWLog.logError("unhandled case: " + Type.ToString());
          }

          break;

        // buttons
        case trTriggerType.BUTTON_MAIN:
          ret = (bot.ButtonMain.state == PI.ButtonState.BUTTON_PRESSED ? true : false);
          break;

        case trTriggerType.BUTTON_1:
          ret = (bot.Button1.state == PI.ButtonState.BUTTON_PRESSED ? true : false);
          break;

        case trTriggerType.BUTTON_2:
          ret = (bot.Button2.state == PI.ButtonState.BUTTON_PRESSED ? true : false);
          break;

        case trTriggerType.BUTTON_3:
          ret = (bot.Button3.state == PI.ButtonState.BUTTON_PRESSED ? true : false);
          break;

        case trTriggerType.BUTTON_ANY:
          ret = (bot.ButtonMain.state == PI.ButtonState.BUTTON_PRESSED) ||
                (bot.Button1.state    == PI.ButtonState.BUTTON_PRESSED) ||
                (bot.Button2.state    == PI.ButtonState.BUTTON_PRESSED) ||
                (bot.Button3.state    == PI.ButtonState.BUTTON_PRESSED);
          break;

        case trTriggerType.BUTTON_MAIN_NOT:
          ret = bot.ButtonMain.state == PI.ButtonState.BUTTON_NOTPRESSED;
          break;

        case trTriggerType.BUTTON_1_NOT:
          ret = bot.Button1.state == PI.ButtonState.BUTTON_NOTPRESSED;
          break;

        case trTriggerType.BUTTON_2_NOT:
          ret = bot.Button2.state == PI.ButtonState.BUTTON_NOTPRESSED;
          break;

        case trTriggerType.BUTTON_3_NOT:
          ret = bot.Button3.state == PI.ButtonState.BUTTON_NOTPRESSED;
          break;

        case trTriggerType.BUTTON_NONE:
          ret = (bot.ButtonMain.state == PI.ButtonState.BUTTON_NOTPRESSED) &&
                (bot.Button1.state    == PI.ButtonState.BUTTON_NOTPRESSED) &&
                (bot.Button2.state    == PI.ButtonState.BUTTON_NOTPRESSED) &&
                (bot.Button3.state    == PI.ButtonState.BUTTON_NOTPRESSED);
          break;

        // misc
        case trTriggerType.IMMEDIATE:
          ret = true;
          break;

        case trTriggerType.RANDOM:
        case trTriggerType.BEHAVIOR_FINISHED:
          ret = state.Behavior.isFinished(robot);
          break;

        case trTriggerType.CLAP:
          ret = (bot.SoundSensor.eventId == PI.SoundEventIndex.SOUND_EVENT_CLAP) ? true : false;
          break;

        case trTriggerType.VOICE:
          ret = (bot.SoundSensor.voiceConfidence > VOICE_CONFIDENCE_THRESHOLD) ? true : false;
//      TODO: Lets revisit Caching in v1.5. For now are ignoring it
//          if (!ret)  {
//            ret = (bot.WasVoiceHeardRecently()) ? true : false;
//          }
//          Debug.Log (Time.time.ToString() + ret.ToString());
          break;

        case trTriggerType.KIDNAP:
          ret = bot.KidnapSensor.flag;
          break;

        case trTriggerType.KIDNAP_NOT:
          ret = !bot.KidnapSensor.flag;
          break;

        case trTriggerType.STALL:
          ret = bot.StallBumpSensor.flag;
          break;

        case trTriggerType.STALL_NOT:
          ret = !bot.StallBumpSensor.flag;
          break;

        // distance
        case trTriggerType.DISTANCE_REAR_NONE:
          ret = DistanceIsNone  (bot.DistanceSensorTail.distance);
          break;

        case trTriggerType.DISTANCE_REAR_FAR:
          ret = DistanceIsFar   (bot.DistanceSensorTail.distance);
          break;

        case trTriggerType.DISTANCE_REAR_NEAR:
          ret = DistanceIsNear  (bot.DistanceSensorTail.distance);
          break;

        // note that we swap LEFT and RIGHT in these triggers.
        case trTriggerType.DISTANCE_LEFT_NONE:
          ret = DistanceIsNone  (bot.DistanceSensorFrontRight.distance);
          break;

        // note that we swap LEFT and RIGHT in these triggers.
        case trTriggerType.DISTANCE_LEFT_FAR:
          if (DistanceIsFarOrCloser(bot.DistanceSensorFrontLeft.distance)) {
            ret = false;
          }
          else {
            ret = DistanceIsFar   (bot.DistanceSensorFrontRight.distance);
          }
          break;

        // note that we swap LEFT and RIGHT in these triggers.
        case trTriggerType.DISTANCE_LEFT_NEAR:
          if (DistanceIsNearOrCloser(bot.DistanceSensorFrontLeft.distance)) {
            ret = false;
          }
          else {
            ret = DistanceIsNear  (bot.DistanceSensorFrontRight.distance);
          }
          break;

        // note that we swap LEFT and RIGHT in these triggers.
        case trTriggerType.DISTANCE_RIGHT_NONE:
          ret = DistanceIsNone  (bot.DistanceSensorFrontLeft.distance);
          break;

        // note that we swap LEFT and RIGHT in these triggers.
        case trTriggerType.DISTANCE_RIGHT_FAR:
          if (DistanceIsFarOrCloser(bot.DistanceSensorFrontRight.distance)) {
            ret = false;
          }
          else {
            ret = DistanceIsFar   (bot.DistanceSensorFrontLeft.distance);
          }
          break;

        // note that we swap LEFT and RIGHT in these triggers.
        case trTriggerType.DISTANCE_RIGHT_NEAR:
          if (DistanceIsNearOrCloser(bot.DistanceSensorFrontRight.distance)) {
            ret = false;
          }
          else {
            ret = DistanceIsNear  (bot.DistanceSensorFrontLeft.distance);
          }
          break;

        case trTriggerType.DISTANCE_CENTER_NONE:
          ret = DistanceIsNone  (bot.DistanceSensorFrontCenter.distance);
          break;

        case trTriggerType.DISTANCE_CENTER_FAR:
          ret = DistanceIsFar(bot.DistanceSensorFrontLeft.distance) && DistanceIsFar(bot.DistanceSensorFrontRight.distance);
          break;

        case trTriggerType.DISTANCE_CENTER_NEAR:
          ret = DistanceIsNear(bot.DistanceSensorFrontLeft.distance) && DistanceIsNear(bot.DistanceSensorFrontRight.distance);
          break;

        // time and time_long
        case trTriggerType.TIME:
        case trTriggerType.TIME_LONG:
          if (ParameterValue == 0) {
            ret = true;
          } else {
            if (state == null) {
              ret = false;
            } else {
              ret = state.TimeInState >= ParameterValue;
            }
          }

          break;

        case trTriggerType.TIME_1:
          if (state == null) {
            ret = false;
          } else {
            ret = state.TimeInState >= 1.0f;
          }

          break;

        case trTriggerType.TIME_3:
          if (state == null) {
            ret = false;
          } else {
            ret = state.TimeInState >= 3.0f;
          }

          break;

        case trTriggerType.TIME_5:
          if (state == null) {
            ret = false;
          } else {
            ret = state.TimeInState >= 5.0f;
          }

          break;

        case trTriggerType.TIME_10:
          if (state == null) {
            ret = false;
          } else {
            ret = state.TimeInState >= 10.0f;
          }

          break;

        case trTriggerType.TIME_RANDOM:
          if (state == null) {
            ret = false;
          }

          ret = state.TimeInState >= DynamicParameter;

          if (ret) {
            DynamicParameter = float.NaN;
          }

          break;


        //travel
        case trTriggerType.TRAVEL_LINEAR:
          ret = state != null;

          if (ret) {
            ret = shouldTrigger(state.TravelInStateLinear, ParameterValue);
          }

          break;

        case trTriggerType.TRAVEL_ANGULAR:
          ret = state != null;

          if (ret) {
            if (ParameterValue > 0) {
              // note flip
              ret = -state.TravelInStateAngular > ParameterValue;
            } else {
              ret = -state.TravelInStateAngular < ParameterValue;
            }
          }

          break;

        case trTriggerType.TRAVELING_FORWARD:
          if (state == null) {
            ret = false;
          } else {
            ret = state.LinearSpeedFiltered > TravelingTreshold;
          }

          break;

        case trTriggerType.TRAVELING_BACKWARD:
          if (state == null) {
            ret = false;
          } else {
            ret = state.LinearSpeedFiltered < -TravelingTreshold;
          }

          break;

        case trTriggerType.TRAVELING_STOPPED:
          if (state == null) {
            ret = false;
          } else {
            ret = (state.AbsSpeedFiltered > -StoppedTreshold) && (state.AbsSpeedFiltered < StoppedTreshold);
          }

          break;

        //lean
        case trTriggerType.LEAN_LEFT:
          ret = bot.Accelerometer.WindowedY < -LeanTreshold;
          break;

        case trTriggerType.LEAN_RIGHT:
          ret = bot.Accelerometer.WindowedY >  LeanTreshold;
          break;

        case trTriggerType.LEAN_FORWARD:
          ret = bot.Accelerometer.WindowedX < -LeanTreshold;
          break;

        case trTriggerType.LEAN_BACKWARD:
          ret = bot.Accelerometer.WindowedX >  LeanTreshold;
          break;

        case trTriggerType.LEAN_UPSIDE_DOWN:
          ret = bot.Accelerometer.WindowedZ < -LeanTreshold;
          break;

        case trTriggerType.LEAN_UPSIDE_UP:
          ret = bot.Accelerometer.WindowedZ >  LeanTreshold;
          break;

        // beacons
        case trTriggerType.BEACON_LEFT:
          ret = bot.Beacon.seeSomethingLeft && !bot.Beacon.seeSomethingRight;
          break;

        case trTriggerType.BEACON_RIGHT:
          ret = bot.Beacon.seeSomethingRight && !bot.Beacon.seeSomethingLeft;
          break;

        case trTriggerType.BEACON_BOTH:
          ret = bot.Beacon.seeSomethingLeft && bot.Beacon.seeSomethingRight;
          break;

        case trTriggerType.BEACON_NONE:
          ret = !bot.Beacon.seeSomething;
          break;

        case trTriggerType.BEACON_LEFT_DOT:
          ret = bot.Beacon.seeDotLeft && !bot.Beacon.seeDotRight;
          break;

        case trTriggerType.BEACON_RIGHT_DOT:
          ret = bot.Beacon.seeDotRight && !bot.Beacon.seeDotLeft;
          break;

        case trTriggerType.BEACON_BOTH_DOT:
          ret = bot.Beacon.seeDotLeft && bot.Beacon.seeDotRight;
          break;

        case trTriggerType.BEACON_NONE_DOT:
          ret = !bot.Beacon.seeDot;
          break;

        case trTriggerType.BEACON_LEFT_DASH:
          ret = bot.Beacon.seeDashLeft && !bot.Beacon.seeDashRight;
          break;

        case trTriggerType.BEACON_RIGHT_DASH:
          ret = bot.Beacon.seeDashRight && !bot.Beacon.seeDashLeft;
          break;

        case trTriggerType.BEACON_BOTH_DASH:
          ret = bot.Beacon.seeDashLeft && bot.Beacon.seeDashRight;
          break;

        case trTriggerType.BEACON_NONE_DASH:
          ret = !bot.Beacon.seeDash;
          break;

        case trTriggerType.BEACON_SET:
        case trTriggerType.DISTANCE_SET:
          if (TriggerSet == null) {
            WWLog.logError("trigger set is missing. " + this.UUID);
            ret = false;
          }

          ret = TriggerSet.conditionMatches(robot, state);
          break;
          
        case trTriggerType.BEACON_V2:
          ret = evaluateBeaconV2(robot, state);
          break;

        case trTriggerType.DROP:
          ret = robot.HasIncomingEvent("gestureDrop");
          break;

        case trTriggerType.SHAKE:
          ret = robot.HasIncomingEvent("OrientationShake");
          break;

        case trTriggerType.SLIDE_X_POS:
          ret = robot.HasIncomingEvent("gestureSlide +x");
          break;

        case trTriggerType.SLIDE_X_NEG:
          ret = robot.HasIncomingEvent("gestureSlide -x");
          break;

        case trTriggerType.SLIDE_Y_POS:
          ret = robot.HasIncomingEvent("gestureSlide +y");
          break;

        case trTriggerType.SLIDE_Y_NEG:
          ret = robot.HasIncomingEvent("gestureSlide -y");
          break;

        case trTriggerType.SLIDE_Z_POS:
          ret = robot.HasIncomingEvent("gestureSlide +z");
          break;

        case trTriggerType.SLIDE_Z_NEG:
          ret = robot.HasIncomingEvent("gestureSlide -z");
          break;

        case trTriggerType.NONE:
          ret = false;
          break;
      }
     
      return ret ? trTriggerConditionIsMet.YES : trTriggerConditionIsMet.NO;
    }
    
    private bool evaluateBeaconV2(piBotBase robot, trState state) {
      
      piBotComponentBeaconV2.BeaconV2Data bv2 = ((piBotBo)robot).BeaconV2.CurrentData;
    
      bool primaryConditionsMatch = true;
      
      // robot type
      if (BeaconV2Params.otherType == piRobotType.UNKNOWN) {
        primaryConditionsMatch &= (bv2.robotType != piRobotType.UNKNOWN);
      }
      else {
        primaryConditionsMatch &= (bv2.robotType == BeaconV2Params.otherType);
      }
      
      // robot distance
      if (BeaconV2Params.otherDistanceLevel != WWBeaconLevel.BEACON_LEVEL_OFF) {
        primaryConditionsMatch &= (bv2.distanceLevel == BeaconV2Params.otherDistanceLevel);
      }
      
      // robot color
      if (BeaconV2Params.otherColor != WWBeaconColor.WW_ROBOT_COLOR_INVALID) {
        primaryConditionsMatch &= ((WWBeaconColor)(bv2.data) == BeaconV2Params.otherColor);
      }
      
      // receivers
      if (BeaconV2Params.selfReceivers != WWBeaconReceiver.WW_BEACON_RECEIVER_UNKNOWN) {
        primaryConditionsMatch &= (bv2.receivers == BeaconV2Params.selfReceivers);
      }
      
      // robot id
      if (BeaconV2Params.otherID != 0) {
        primaryConditionsMatch &= (bv2.robotID == BeaconV2Params.otherID);
      }
      
      bool overallMatch = (BeaconV2Params.match ? primaryConditionsMatch : !primaryConditionsMatch);
      
      return overallMatch;
    }
      

    private float getDynamicParameterValue(){
      float result = 0;
      switch(Type){
        case trTriggerType.TIME_RANDOM:
          result = ((float)new System.Random().NextDouble()) * ParameterValue;
          break;
      }
      return result;
    }

    private bool shouldTrigger(float parameter, float treshold) {
      return Mathf.Abs(parameter) >= treshold;
    }

    public static bool DistanceIsNone(float value) {
      return value >= DistThreshFarNone;
    }
    public static bool DistanceIsFar(float value) {
      return ((value >= DistThreshNearFar) && (value <= DistThreshFarNone));
    }
    public static bool DistanceIsNear(float value) {
      return value <= DistThreshNearFar;
    }
    public static bool DistanceIsNearOrCloser(float value) {
      return DistanceIsNear(value);
    }
    public static bool DistanceIsFarOrCloser(float value) {
      return DistanceIsFar(value) || DistanceIsNearOrCloser(value);
    }
  }
}




