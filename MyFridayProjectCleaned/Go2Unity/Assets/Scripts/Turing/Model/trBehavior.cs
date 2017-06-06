using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using WW.SimpleJSON;
using WW;

namespace Turing {
  public enum trBehaviorType {
    DO_NOTHING                    = 0,
    START_STATE                   = 1,
    
    COLOR_OFF                     = 100,
    COLOR_RED                     = 101,
    COLOR_ORANGE                  = 102,
    COLOR_YELLOW                  = 103,
    COLOR_GREEN                   = 104,
    COLOR_CYAN                    = 105,
    COLOR_BLUE                    = 106,
    COLOR_MAGENTA                 = 107,
    COLOR_WHITE                   = 108,
    
    ANIM_TROPHY                   = 200,
    ANIM_TORNADO                  = 201,
    ANIM_OIL                      = 202,
    ANIM_TRUMPET                  = 203,
    ANIM_DELIVERY                 = 204,
    ANIM_CELEBRATION              = 205,
    ANIM_DANCE_BUTTWIGGLE         = 206,
    ANIM_DANCE_SPINLEFT           = 207,
    ANIM_DANCE_SPINRIGHT          = 208,
    ANIM_DANCE_STAYALIVELEFT      = 209,
    ANIM_DANCE_STAYALIVERIGHT     = 210,
    ANIM_DANCE_TWIST              = 211,
    ANIM_FORWARD_CYCLE_CAUTIOUS   = 212,
    ANIM_FORWARD_CYCLE_CURIOUS    = 213,
    ANIM_FORWARD_CYCLE_FRUSTRATED = 214,
    ANIM_FORWARD_CYCLE_HAPPY      = 215,
    ANIM_FORWARD_CYCLE_SILLY      = 216,

    SOUND_USER                    = 300,
    SOUND_INTERNAL                = 301,
    SOUND_VOCAL_BRAVE             = 302,
    SOUND_VOCAL_CAUTIOUS          = 303,
    SOUND_VOCAL_CURIOUS           = 304,
    SOUND_VOCAL_FRUSTRATED        = 305,
    SOUND_VOCAL_HAPPY             = 306,
    SOUND_VOCAL_SILLY             = 307,
    SOUND_VOCAL_SURPRISED         = 308,
    SOUND_ANIMAL                  = 309,
    SOUND_SFX                     = 310,
    SOUND_TRANSPORT               = 311,
    
    MOVE_STOP                     = 400,
    MOVE_CONT_STRAIGHT            = 401,
    MOVE_CONT_SPIN                = 404,
    MOVE_DISC_STRAIGHT            = 405,
    MOVE_DISC_TURN                = 406,
    MOVE_TURN_VOICE               = 407,
    PUPPET                        = 408,
    
    HEAD_PAN                      = 500,
    HEAD_TILT                     = 501,
    HEAD_PAN_VOICE                = 502,
    
    EYERING                       = 600, 

    MISSION                       = 700,   // not really a behavior: indicates a mission/challenge.
    MAPSET                        = 701,
    OMNI                          = 702,
    LAUNCH_FLING                  = 703,
    LAUNCH_RELOAD_LEFT            = 704,
    LAUNCH_RELOAD_RIGHT           = 705,
    
    //Deprecated
    MOVE_CONT_CIRCLE_CCW          = 402,
    MOVE_CONT_CIRCLE_CW           = 403,
    MOVE_FB0                      = 800,
    MOVE_F1                       = 801,
    MOVE_F2                       = 802,
    MOVE_F3                       = 803,
    MOVE_B1                       = 804,
    MOVE_B2                       = 805,
    MOVE_B3                       = 806,
    MOVE_LR0                      = 807,
    MOVE_L1                       = 808,
    MOVE_L2                       = 809,
    MOVE_L3                       = 810,
    MOVE_R1                       = 811,
    MOVE_R2                       = 812,
    MOVE_R3                       = 813,
    SOUND_USER_1                  = 814,
    SOUND_USER_2                  = 815,
    SOUND_USER_3                  = 816,
    SOUND_USER_4                  = 817,
    SOUND_USER_5                  = 818,

    MOOD_1                        = 900,
    MOOD_2                        = 901,
    MOOD_3                        = 902,
    MOOD_4                        = 903,
    MOOD_5                        = 904,
//    MOOD_6                        = 905,
//    MOOD_7                        = 906,
   
    EXPRESSION_CATEGORY_1         = 1000,
    EXPRESSION_CATEGORY_2         = 1001,
    EXPRESSION_CATEGORY_3         = 1002,
    EXPRESSION_CATEGORY_4         = 1003,
    EXPRESSION_CATEGORY_5         = 1004,
  
    MOODY_ANIMATION               = 1100,
    
    RUN_SPARK                     = 1200, // TUR-655

    FUNCTION                      = 1300,
    FUNCTION_END                   = 1301,
  }

  public enum trBehaviorArea{
    NOTHING    = 0,
    ANIM       = 1,
    SOUND      = 2,
    LIGHT      = 3,
    MOVE       = 4,
    HEAD       = 5,
    MAPPER     = 6,
    UNKNOWN    = 7,
    MOOD       = 8,
    EXPRESSION = 9,
    ACCESSORY  = 10,
    FUNCTION   = 11,
  }
  
  static class trBehaviorTypeMethods{
    public static trMoodType ToMood(this trBehaviorType type){
      if(!type.IsMood()){
        WWLog.logError("Trying to convert behavior type " + type + " to mood");
        return trMoodType.NO_CHANGE;
      }
      trMoodType mood = (trMoodType)((int)type - (int)trBehaviorType.MOOD_1 + 1);
      return mood;
    }

    public static bool IsMood(this trBehaviorType type){
      switch(type){
      case trBehaviorType.MOOD_1:
      case trBehaviorType.MOOD_2:
      case trBehaviorType.MOOD_3:
      case trBehaviorType.MOOD_4:
      case trBehaviorType.MOOD_5:
//      case trBehaviorType.MOOD_6:
//      case trBehaviorType.MOOD_7:
        return true;
      }
      return false;
    }
    
    public static bool IsAnimationOrExpression(this trBehaviorType trBT) {
      return IsAnimation(trBT) || IsExpression (trBT);
    }
    
    public static bool IsExpression(this trBehaviorType trBT) {
      switch (trBT) {
        default:
          return false;
        case trBehaviorType.EXPRESSION_CATEGORY_1:
        case trBehaviorType.EXPRESSION_CATEGORY_2:
          return true;
      }
    }

    public static bool IsAnimation(this trBehaviorType trBT) {
      if (trBT == trBehaviorType.MOODY_ANIMATION) {
        return true;
      }
      return false;
    }
    
    public static bool IsColor(this trBehaviorType trBT) {
      bool ret = false;

      switch (trBT) {
        default:
          ret = false;
          break;
        case trBehaviorType.COLOR_OFF:
        case trBehaviorType.COLOR_RED:
        case trBehaviorType.COLOR_ORANGE:
        case trBehaviorType.COLOR_YELLOW:
        case trBehaviorType.COLOR_GREEN:
        case trBehaviorType.COLOR_CYAN:
        case trBehaviorType.COLOR_BLUE:
        case trBehaviorType.COLOR_MAGENTA:
        case trBehaviorType.COLOR_WHITE:
          ret = true;
          break;
      }

      return ret;
    }

    public static wwLEDColor toLEDColor(this trBehaviorType trBT) {
      wwLEDColor ret = wwLEDColor.BLACK;

      if (!trBT.IsColor()) {
        WWLog.logError("Not a color: " + trBT.ToString());
        return ret;
      }

      switch (trBT) {
        default:
          WWLog.logError("Unhandled color type: " + trBT.ToString());
          break;
        case trBehaviorType.COLOR_OFF:
          ret = wwLEDColor.BLACK;
          break;
        case trBehaviorType.COLOR_RED:
          ret = wwLEDColor.RED;
          break;
        case trBehaviorType.COLOR_ORANGE:
          ret = wwLEDColor.ORANGE;
          break;
        case trBehaviorType.COLOR_YELLOW:
          ret = wwLEDColor.YELLOW;
          break;
        case trBehaviorType.COLOR_GREEN:
          ret = wwLEDColor.GREEN;
          break;
        case trBehaviorType.COLOR_CYAN:
          ret = wwLEDColor.CYAN;
          break;
        case trBehaviorType.COLOR_BLUE:
          ret = wwLEDColor.BLUE;
          break;
        case trBehaviorType.COLOR_MAGENTA:
          ret = wwLEDColor.MAGENTA;
          break;
        case trBehaviorType.COLOR_WHITE:
          ret = wwLEDColor.WHITE;
          break;
      }
      return ret;
    }

    public static bool IsSound(this trBehaviorType trBT) {
      if (trRobotSounds.Instance.GetCategories().Contains(trBT)) {
        return true;
      }
      bool result = false;
      switch(trBT) {
        case trBehaviorType.SOUND_USER_1:
        case trBehaviorType.SOUND_USER_2:
        case trBehaviorType.SOUND_USER_3:
        case trBehaviorType.SOUND_USER_4:
        case trBehaviorType.SOUND_USER_5:
          result = true;
          break;
      }
      return result;
    }

    public static bool IsContinuousMove(this trBehaviorType type){
      switch(type){
      case trBehaviorType.MOVE_CONT_SPIN:
      case trBehaviorType.MOVE_CONT_STRAIGHT:
      case trBehaviorType.MOVE_CONT_CIRCLE_CCW:
      case trBehaviorType.MOVE_CONT_CIRCLE_CW:
        return true;
      }
      return false;
    }

    public static bool IsDeletable(this trBehaviorType type){
      switch(type){
      case trBehaviorType.MISSION:
      case trBehaviorType.MAPSET:
      case trBehaviorType.FUNCTION:
        return true;
      }
      return false;
    }

    public static bool IsShowToUser(this trBehaviorType type){
    
      switch(type){
        case trBehaviorType.RUN_SPARK:
          return trMultivariate.isYESorSHOW(trMultivariate.trAppOption.RUN_SPARK_BEHAVIOR);
        case trBehaviorType.PUPPET:
          return trMultivariate.isYESorSHOW(trMultivariate.trAppOption.UNLOCK_PUPPET);

        // case trBehaviorType.LAUNCH_RELOAD_LEFT:
        // case trBehaviorType.LAUNCH_RELOAD_RIGHT:
        // case trBehaviorType.LAUNCH_FLING:
        //   return trMultivariate.Instance.getOptionValue(trMultivariate.trAppOption.SHOW_LAUNCH_ACTIONS) == trMultivariate.trAppOptionValue.SHOW;

        case trBehaviorType.MOVE_B1:
        case trBehaviorType.MOVE_B2:
        case trBehaviorType.MOVE_B3:
        case trBehaviorType.MOVE_F1:
        case trBehaviorType.MOVE_F2:
        case trBehaviorType.MOVE_F3:
        case trBehaviorType.MOVE_FB0:
        case trBehaviorType.MOVE_L1:
        case trBehaviorType.MOVE_L2:
        case trBehaviorType.MOVE_L3:
        case trBehaviorType.MOVE_LR0:
        case trBehaviorType.MOVE_R1:
        case trBehaviorType.MOVE_R2:
        case trBehaviorType.MOVE_R3:
        case trBehaviorType.MOVE_CONT_CIRCLE_CCW:
        case trBehaviorType.MOVE_CONT_CIRCLE_CW:
        case trBehaviorType.MISSION:
        case trBehaviorType.SOUND_USER_1:
        case trBehaviorType.SOUND_USER_2:
        case trBehaviorType.SOUND_USER_3:
        case trBehaviorType.SOUND_USER_4:
        case trBehaviorType.SOUND_USER_5:
        case trBehaviorType.SOUND_INTERNAL:
        case trBehaviorType.START_STATE:
        case trBehaviorType.ANIM_TROPHY:
        case trBehaviorType.ANIM_TORNADO:
        case trBehaviorType.ANIM_OIL:
        case trBehaviorType.ANIM_TRUMPET:
        case trBehaviorType.ANIM_DELIVERY:
        case trBehaviorType.ANIM_CELEBRATION:
        case trBehaviorType.ANIM_DANCE_BUTTWIGGLE:
        case trBehaviorType.ANIM_DANCE_SPINLEFT:
        case trBehaviorType.ANIM_DANCE_SPINRIGHT:
        case trBehaviorType.ANIM_DANCE_STAYALIVELEFT:
        case trBehaviorType.ANIM_DANCE_STAYALIVERIGHT:
        case trBehaviorType.ANIM_DANCE_TWIST:
        case trBehaviorType.ANIM_FORWARD_CYCLE_CAUTIOUS:
        case trBehaviorType.ANIM_FORWARD_CYCLE_CURIOUS:
        case trBehaviorType.ANIM_FORWARD_CYCLE_FRUSTRATED:
        case trBehaviorType.ANIM_FORWARD_CYCLE_HAPPY:
        case trBehaviorType.ANIM_FORWARD_CYCLE_SILLY:
          return false;
      }
      return true;
    }

    public static string Consolidated(this trBehaviorType trBT) {
      if (trBT.IsSound()) {
        return "SOUND";
      }
      else if (trBT.IsColor()) {
        return "COLOR";
      }
      else {
        return trBT.ToString();
      }
    }

    public static trBehaviorArea GetArea(this trBehaviorType type, piRobotType robotType){
      switch(robotType) {
        case piRobotType.DOT:
          return GetAreaForDot(type);
        case piRobotType.DASH:
        case piRobotType.UNKNOWN:
        default:
          return GetAreaForDash(type);
      }
    }
    public static trBehaviorArea GetAreaForDot(this trBehaviorType type) {
      switch(type){
      case trBehaviorType.COLOR_BLUE:
      case trBehaviorType.COLOR_CYAN:
      case trBehaviorType.COLOR_GREEN:
      case trBehaviorType.COLOR_MAGENTA:
      case trBehaviorType.COLOR_OFF:
      case trBehaviorType.COLOR_RED:
      case trBehaviorType.COLOR_WHITE:
      case trBehaviorType.COLOR_YELLOW:
      case trBehaviorType.COLOR_ORANGE:
      case trBehaviorType.EYERING:
        return trBehaviorArea.LIGHT;
        
      case trBehaviorType.MOOD_1:
      case trBehaviorType.MOOD_2:
      case trBehaviorType.MOOD_3:
      case trBehaviorType.MOOD_4:
      case trBehaviorType.MOOD_5:
        return trBehaviorArea.MOOD;

      case trBehaviorType.SOUND_USER_1:
      case trBehaviorType.SOUND_USER_2:
      case trBehaviorType.SOUND_USER_3:
      case trBehaviorType.SOUND_USER_4:
      case trBehaviorType.SOUND_USER_5:
      case trBehaviorType.SOUND_USER:
      case trBehaviorType.SOUND_VOCAL_BRAVE:
      case trBehaviorType.SOUND_VOCAL_CAUTIOUS:
      case trBehaviorType.SOUND_VOCAL_CURIOUS:
      case trBehaviorType.SOUND_VOCAL_FRUSTRATED:
      case trBehaviorType.SOUND_VOCAL_HAPPY:
      case trBehaviorType.SOUND_VOCAL_SILLY:
      case trBehaviorType.SOUND_VOCAL_SURPRISED:
      case trBehaviorType.SOUND_ANIMAL:
      case trBehaviorType.SOUND_SFX:
      case trBehaviorType.SOUND_TRANSPORT:
        return trBehaviorArea.SOUND;
        
      case trBehaviorType.DO_NOTHING:
      case trBehaviorType.START_STATE:
      case trBehaviorType.OMNI:
      case trBehaviorType.RUN_SPARK:
        return trBehaviorArea.NOTHING;

      case trBehaviorType.FUNCTION: 
      case trBehaviorType.FUNCTION_END:
        return trBehaviorArea.FUNCTION;
        
      case trBehaviorType.MAPSET:
        return trBehaviorArea.MAPPER;
        
      case trBehaviorType.EXPRESSION_CATEGORY_1:
      case trBehaviorType.EXPRESSION_CATEGORY_2:
      case trBehaviorType.EXPRESSION_CATEGORY_3:
      case trBehaviorType.EXPRESSION_CATEGORY_4:
      case trBehaviorType.EXPRESSION_CATEGORY_5:
        return trBehaviorArea.EXPRESSION;
        
      case trBehaviorType.MOODY_ANIMATION:
        return trBehaviorArea.ANIM;
      
      case trBehaviorType.MOVE_B1:
      case trBehaviorType.MOVE_B2:
      case trBehaviorType.MOVE_B3:
      case trBehaviorType.MOVE_F1:
      case trBehaviorType.MOVE_F2:
      case trBehaviorType.MOVE_F3:
      case trBehaviorType.MOVE_FB0:
      case trBehaviorType.MOVE_L1:
      case trBehaviorType.MOVE_L2:
      case trBehaviorType.MOVE_L3:
      case trBehaviorType.MOVE_LR0:
      case trBehaviorType.MOVE_R1:
      case trBehaviorType.MOVE_R2:
      case trBehaviorType.MOVE_R3:
      case trBehaviorType.PUPPET:
      case trBehaviorType.MOVE_STOP:
      case trBehaviorType.MOVE_CONT_CIRCLE_CCW:
      case trBehaviorType.MOVE_CONT_CIRCLE_CW:
      case trBehaviorType.MOVE_CONT_SPIN:
      case trBehaviorType.MOVE_CONT_STRAIGHT:
      case trBehaviorType.MOVE_DISC_STRAIGHT:
      case trBehaviorType.MOVE_DISC_TURN:
      case trBehaviorType.MOVE_TURN_VOICE:
      case trBehaviorType.HEAD_PAN:
      case trBehaviorType.HEAD_TILT:
      case trBehaviorType.HEAD_PAN_VOICE:
      case trBehaviorType.LAUNCH_RELOAD_LEFT:
      case trBehaviorType.LAUNCH_RELOAD_RIGHT:
      case trBehaviorType.LAUNCH_FLING:
        return trBehaviorArea.UNKNOWN;
      
      default:
        WWLog.logError("unknown behavior type " + type.ToString());
        break;
      }
      return trBehaviorArea.UNKNOWN;
    }

    public static trBehaviorArea GetAreaForDash(this trBehaviorType type) {
        switch(type){
          case trBehaviorType.COLOR_BLUE:
          case trBehaviorType.COLOR_CYAN:
          case trBehaviorType.COLOR_GREEN:
          case trBehaviorType.COLOR_MAGENTA:
          case trBehaviorType.COLOR_OFF:
          case trBehaviorType.COLOR_RED:
          case trBehaviorType.COLOR_WHITE:
          case trBehaviorType.COLOR_YELLOW:
          case trBehaviorType.COLOR_ORANGE:
          case trBehaviorType.EYERING:
            return trBehaviorArea.LIGHT;

          case trBehaviorType.MOOD_1:
          case trBehaviorType.MOOD_2:
          case trBehaviorType.MOOD_3:
          case trBehaviorType.MOOD_4:
          case trBehaviorType.MOOD_5:
            return trBehaviorArea.MOOD;
      
          case trBehaviorType.SOUND_USER_1:
          case trBehaviorType.SOUND_USER_2:
          case trBehaviorType.SOUND_USER_3:
          case trBehaviorType.SOUND_USER_4:
          case trBehaviorType.SOUND_USER_5:
          case trBehaviorType.SOUND_USER:
          case trBehaviorType.SOUND_VOCAL_BRAVE:
          case trBehaviorType.SOUND_VOCAL_CAUTIOUS:
          case trBehaviorType.SOUND_VOCAL_CURIOUS:
          case trBehaviorType.SOUND_VOCAL_FRUSTRATED:
          case trBehaviorType.SOUND_VOCAL_HAPPY:
          case trBehaviorType.SOUND_VOCAL_SILLY:
          case trBehaviorType.SOUND_VOCAL_SURPRISED:
          case trBehaviorType.SOUND_ANIMAL:
          case trBehaviorType.SOUND_SFX:
          case trBehaviorType.SOUND_TRANSPORT:
            return trBehaviorArea.SOUND;

          case trBehaviorType.DO_NOTHING:
          case trBehaviorType.START_STATE:
          case trBehaviorType.OMNI:
          case trBehaviorType.RUN_SPARK:         
            return trBehaviorArea.NOTHING;

          case trBehaviorType.FUNCTION: 
          case trBehaviorType.FUNCTION_END:
            return trBehaviorArea.FUNCTION;

          case trBehaviorType.LAUNCH_RELOAD_LEFT:
          case trBehaviorType.LAUNCH_RELOAD_RIGHT:
          case trBehaviorType.LAUNCH_FLING:
            return trBehaviorArea.ACCESSORY;

          case trBehaviorType.MOVE_B1:
          case trBehaviorType.MOVE_B2:
          case trBehaviorType.MOVE_B3:
          case trBehaviorType.MOVE_F1:
          case trBehaviorType.MOVE_F2:
          case trBehaviorType.MOVE_F3:
          case trBehaviorType.MOVE_FB0:
          case trBehaviorType.MOVE_L1:
          case trBehaviorType.MOVE_L2:
          case trBehaviorType.MOVE_L3:
          case trBehaviorType.MOVE_LR0:
          case trBehaviorType.MOVE_R1:
          case trBehaviorType.MOVE_R2:
          case trBehaviorType.MOVE_R3:
          case trBehaviorType.PUPPET:
          case trBehaviorType.MOVE_STOP:
          case trBehaviorType.MOVE_CONT_CIRCLE_CCW:
          case trBehaviorType.MOVE_CONT_CIRCLE_CW:
          case trBehaviorType.MOVE_CONT_SPIN:
          case trBehaviorType.MOVE_CONT_STRAIGHT:
          case trBehaviorType.MOVE_DISC_STRAIGHT:
          case trBehaviorType.MOVE_DISC_TURN:
          case trBehaviorType.MOVE_TURN_VOICE:
          
          case trBehaviorType.HEAD_PAN:
          case trBehaviorType.HEAD_TILT:
          case trBehaviorType.HEAD_PAN_VOICE:
            return trBehaviorArea.MOVE;
            
          case trBehaviorType.MAPSET:
            return trBehaviorArea.MAPPER;

          case trBehaviorType.EXPRESSION_CATEGORY_1:
          case trBehaviorType.EXPRESSION_CATEGORY_2:
          case trBehaviorType.EXPRESSION_CATEGORY_3:
          case trBehaviorType.EXPRESSION_CATEGORY_4:
          case trBehaviorType.EXPRESSION_CATEGORY_5:
            return trBehaviorArea.EXPRESSION;
    
          case trBehaviorType.MOODY_ANIMATION:
            return trBehaviorArea.ANIM;
  
      default:
        WWLog.logError("unknown behavior type " + type.ToString());
        break;
        }
        return trBehaviorArea.UNKNOWN;
    }

}
  
  
  public class trBehavior : trTypedBase<trBehaviorType> {
    
    protected enum BehaveMode {
      START,
      CONTINUE,
      STOP,
    }
    
    public const float LINSPD_1 =  10;
    public const float LINSPD_2 =  30;
    public const float LINSPD_3 =  60;
    public const float ANGSPD_1 =  90.0f * Mathf.Deg2Rad;
    public const float ANGSPD_2 = 180.0f * Mathf.Deg2Rad;
    public const float ANGSPD_3 = 360.0f * Mathf.Deg2Rad;

    private const float CONTINUOUS_MOVE_DURATION = 10.0f;
    private const float CHALLENGE_LAST_STATE_DURATION = 0.25f;
    private const float DISCRETE_MOVE_DURATION = 1.0f;
    private const float DISCRETE_MOVE_PADDING  = 1.0f;    // seconds to wait wait in addition to DISCRETE_MOVE_DURATION before moving on to next state
    private const float DISCRETE_MOVE_AUTO_TIME = DISCRETE_MOVE_DURATION + DISCRETE_MOVE_PADDING;
    private const float SHORT_TIME_DURATION = 0.2f;
    private const float LONG_TIME_DURATION = 10.0f;
    private const float HEAD_MOVE_AUTO_TIME = 0.5f;
    private const float LAUNCHER_AUTO_TIME = 2.0f;

    public const int NUM_PUPPET_SLOTS = 10;

    private static string cRexRobotName = @"ROBOT";
    private static string cRexColor     = @"\bCOLOR\b";

    
    protected delegate void BehaviorHandler(piBotBase robot, BehaveMode mode);
    protected delegate void HasFinishedDelegate(piBotBase robot);
    
    private bool active;
    protected Dictionary<trBehaviorType, BehaviorHandler> handlers = new Dictionary<trBehaviorType, BehaviorHandler>();    
    private HasFinishedDelegate hasFinishedUpdater = null;
    protected HasFinishedDelegate HasFinishedUpdater {
      get {
        return hasFinishedUpdater;
      }
      set {
        hasFinishedUpdater = value;
      }
    }
    
    protected bool hasStarted  = false;
    protected bool hasFinished = false;
    
    private float timeStart = 0;
    
    public trMapSet MapSet = null;
    public trMoodyAnimation Animation = null;

    public float RunningParamValue{
      get{
        return runningParamValues[0];
      }
      set{
        runningParamValues[0] = value;
      }
    }
    private List<float> runningParamValues = new List<float> {0};
    private trMoodType runningMood = trMoodType.NO_CHANGE;

    public trMissionFileInfo MissionFileInfo; // used in mission map to avoid loading all the missions

    public float Normalize(float val, int index=0){
      if(!IsParameterized){
        return 0;
      }
      wwRange range = ParameterRangesDict[Type][index];
      return range.Normalize(val);
    }

    public float Denomalize(float normVal, int index=0){
      if(!IsParameterized){
        return 0;
      }
      wwRange range = ParameterRangesDict[Type][index];
      return range.Denormalize(normVal);
    }


    public bool IsParameterized {
      get {
        return ParameterRangesDict.ContainsKey(Type);
      }
    }

    public bool IsMissionBehavior{
      get{
        return Type == trBehaviorType.MISSION;
      }
    }
    
    public trBehavior() {}
    public trBehavior(trBehaviorType typeVal) : base(typeVal) {
      if (Type == trBehaviorType.MAPSET) {
        MapSet = new trMapSet();
      } else if (Type == trBehaviorType.MOODY_ANIMATION) {
        Animation = new trMoodyAnimation();
      }
    }

    private static Dictionary<trBehaviorType, wwRange[]> parametersRanges = null;
    public static Dictionary<trBehaviorType, wwRange[]> ParameterRangesDict {
      get {
        if (parametersRanges == null){
          parametersRanges = new Dictionary<trBehaviorType, wwRange[]>();
          parametersRanges[trBehaviorType.MOVE_CONT_STRAIGHT      ] = new wwRange[] {new wwRange(-80, 80)};
          parametersRanges[trBehaviorType.MOVE_CONT_CIRCLE_CCW    ] = new wwRange[] {new wwRange(-80, 80)};
          parametersRanges[trBehaviorType.MOVE_CONT_CIRCLE_CW     ] = new wwRange[] {new wwRange(-80, 80)};
          parametersRanges[trBehaviorType.MOVE_CONT_SPIN          ] = new wwRange[] {new wwRange(-80, 80)};
          parametersRanges[trBehaviorType.MOVE_DISC_STRAIGHT      ] = new wwRange[] {new wwRange(-80, 80)};
          parametersRanges[trBehaviorType.MOVE_DISC_TURN          ] = new wwRange[] {new wwRange(180, -180)};
          parametersRanges[trBehaviorType.HEAD_PAN                ] = new wwRange[] {new wwRange(120, -120)};
          parametersRanges[trBehaviorType.HEAD_TILT               ] = new wwRange[] {new wwRange(10, -20)};
          parametersRanges[trBehaviorType.LAUNCH_FLING            ] = new wwRange[] {new wwRange(0, 1)};
          parametersRanges[trBehaviorType.EYERING                 ] = new wwRange[] {new wwRange(0, 0xFFF)};
          parametersRanges[trBehaviorType.SOUND_USER              ] = new wwRange[] {new wwRange(0, 0xFFFF)};
          parametersRanges[trBehaviorType.SOUND_VOCAL_BRAVE       ] = new wwRange[] {new wwRange(0, 0xFFFF)};
          parametersRanges[trBehaviorType.SOUND_VOCAL_CAUTIOUS    ] = new wwRange[] {new wwRange(0, 0xFFFF)};
          parametersRanges[trBehaviorType.SOUND_VOCAL_CURIOUS     ] = new wwRange[] {new wwRange(0, 0xFFFF)};
          parametersRanges[trBehaviorType.SOUND_VOCAL_FRUSTRATED  ] = new wwRange[] {new wwRange(0, 0xFFFF)};
          parametersRanges[trBehaviorType.SOUND_VOCAL_HAPPY    	  ] = new wwRange[] {new wwRange(0, 0xFFFF)};
          parametersRanges[trBehaviorType.SOUND_VOCAL_SILLY    	  ] = new wwRange[] {new wwRange(0, 0xFFFF)};
          parametersRanges[trBehaviorType.SOUND_VOCAL_SURPRISED   ] = new wwRange[] {new wwRange(0, 0xFFFF)};
          parametersRanges[trBehaviorType.SOUND_ANIMAL            ] = new wwRange[] {new wwRange(0, 0xFFFF)};
          parametersRanges[trBehaviorType.SOUND_SFX        	      ] = new wwRange[] {new wwRange(0, 0xFFFF)};
          parametersRanges[trBehaviorType.SOUND_TRANSPORT         ] = new wwRange[] {new wwRange(0, 0xFFFF)};
          parametersRanges[trBehaviorType.SOUND_INTERNAL          ] = new wwRange[] {new wwRange(0, 0xFFFF)};
          parametersRanges[trBehaviorType.EXPRESSION_CATEGORY_1   ] = new wwRange[] {new wwRange(0, 0xFFFF)};
          parametersRanges[trBehaviorType.EXPRESSION_CATEGORY_2   ] = new wwRange[] {new wwRange(0, 0xFFFF)};
          parametersRanges[trBehaviorType.EXPRESSION_CATEGORY_3   ] = new wwRange[] {new wwRange(0, 0xFFFF)};
          parametersRanges[trBehaviorType.EXPRESSION_CATEGORY_4   ] = new wwRange[] {new wwRange(0, 0xFFFF)};
          parametersRanges[trBehaviorType.EXPRESSION_CATEGORY_5   ] = new wwRange[] {new wwRange(0, 0xFFFF)};
          parametersRanges[trBehaviorType.MOODY_ANIMATION         ] = new wwRange[] {new wwRange(0, 0xFFFF)};
          parametersRanges[trBehaviorType.MISSION                 ] = new wwRange[] {new wwRange(0, 1)};
          parametersRanges[trBehaviorType.PUPPET                  ] = new wwRange[] {new wwRange(0, 0xFFFF)};

        }
        return parametersRanges;
      }
    }


    public bool ShowMicrophoneWarning() {
      return !string.IsNullOrEmpty(microphoneWarning(""));
    }

    // returns a string which completes this sentence:
    // "<Dash/Dot> cannot hear sounds when "...
    // empty indicates no conflict.
    public string microphoneWarning(string robotName) {
      string ret = "";
      string ufn = "<b>" + UserFacingNameLocalized + "</b>";

      switch(Type){
        // if this list changes, be sure to update missionSummary.py script.
        default:  
          ret = "";
          break;

        case trBehaviorType.MOODY_ANIMATION:
          // xl8_info: {0} will be 'Dash' or 'Dot', and {1} will be the name of an animation.
          ret = wwLoca.Format("@!@{0} can't hear while doing an animation.  This can trigger once {1} finishes.@!@", robotName, ufn);
          break;

        case trBehaviorType.SOUND_USER_1:
        case trBehaviorType.SOUND_USER_2:
        case trBehaviorType.SOUND_USER_3:
        case trBehaviorType.SOUND_USER_4:
        case trBehaviorType.SOUND_USER_5:
        case trBehaviorType.SOUND_USER:
        case trBehaviorType.SOUND_VOCAL_BRAVE:
        case trBehaviorType.SOUND_VOCAL_CAUTIOUS:
        case trBehaviorType.SOUND_VOCAL_CURIOUS:
        case trBehaviorType.SOUND_VOCAL_FRUSTRATED:
        case trBehaviorType.SOUND_VOCAL_HAPPY:
        case trBehaviorType.SOUND_VOCAL_SILLY:
        case trBehaviorType.SOUND_VOCAL_SURPRISED:
        case trBehaviorType.SOUND_ANIMAL:
        case trBehaviorType.SOUND_SFX:
        case trBehaviorType.SOUND_TRANSPORT:
          // xl8_info: {0} and {1} will both be either 'Dash' or 'Dot'.
          ret = wwLoca.Format("@!@{0} can't hear while talking.  This can trigger once <b>{1}</b> stops speaking.@!@", robotName, robotName);
          break;

        case trBehaviorType.MOVE_CONT_STRAIGHT:
        case trBehaviorType.MOVE_CONT_SPIN:
          // xl8_info: {0} will be 'Dash' or 'Dot', and {1} will be the name of a robot movement such as "Move".
          ret = wwLoca.Format("@!@{0} can't hear over the roar of motors.  {1} can't trigger this.@!@", robotName, ufn);
          break;

        case trBehaviorType.MOVE_DISC_STRAIGHT:
        case trBehaviorType.MOVE_DISC_TURN:
        case trBehaviorType.MOVE_TURN_VOICE:
          // xl8_info: {0} will be 'Dash' or 'Dot', and {1} will be the name of a robot movement such as "Move To".
          ret = wwLoca.Format("@!@{0} can't hear over the roar of motors.  This can trigger once {1} finishes.@!@", robotName, ufn);
          break;
      }

      return ret;
    }

    
    private static Dictionary<string, trBehavior> getDefaultBehaviorsForRobotType(piRobotType robotType) {

      Dictionary<string, trBehavior> defaultBehaviors = new Dictionary<string, trBehavior>();
      foreach (trBehaviorType btype in System.Enum.GetValues(typeof(trBehaviorType))) {
        if(btype != trBehaviorType.MAPSET && btype != trBehaviorType.MISSION && btype != trBehaviorType.MOODY_ANIMATION && btype != trBehaviorType.FUNCTION){
          trBehavior newBehavior = new trBehavior(btype);
          newBehavior.UUID = "BEHAVIOR_"+ btype.ToString();
          defaultBehaviors.Add(newBehavior.UUID, newBehavior);
        }
      }
      
      foreach(trMoodyAnimation animation in trMoodyAnimations.Instance.GetAllAnimations(robotType )) {
        trBehavior behavior = new trBehavior(trBehaviorType.MOODY_ANIMATION);
        behavior.Animation = animation;
        behavior.UUID = trBehaviorType.MOODY_ANIMATION.ToString() + "_" + animation.id.ToString();
        defaultBehaviors.Add(behavior.UUID, behavior);
      }
      
    return defaultBehaviors;
    }

    private static Dictionary<string, trBehavior> defaultBehaviorsForDash = null;
    private static Dictionary<string, trBehavior> defaultBehaviorsForDot = null;

    public static Dictionary<string, trBehavior> DefaultBehaviors {
      get{
        switch (trDataManager.Instance.CurrentRobotTypeSelected) {
        case piRobotType.DOT:
          if (defaultBehaviorsForDot == null) {
            defaultBehaviorsForDot = getDefaultBehaviorsForRobotType(piRobotType.DOT);
          }
          return defaultBehaviorsForDot;
        case piRobotType.DASH:
        default:
          if (defaultBehaviorsForDash == null) {
            defaultBehaviorsForDash = getDefaultBehaviorsForRobotType(piRobotType.DASH);
          }
          return defaultBehaviorsForDash;
        }
      }
    }
    
    private static List<trBehavior> defaultBehaviorsSortedDash = null;
    private static List<trBehavior> defaultBehaviorsSortedDot  = null;
    public static List<trBehavior> DefaultBehaviorsSorted {
      get {
      List<trBehavior> theList = null;
      bool sortIt = false;
      
        switch (trDataManager.Instance.CurrentRobotTypeSelected) {
          case piRobotType.DOT:
            if (defaultBehaviorsSortedDot == null) {
              defaultBehaviorsSortedDot = new List<trBehavior>(DefaultBehaviors.Values);
              sortIt = true;
            }
            theList = defaultBehaviorsSortedDot;
            break;
          case piRobotType.DASH:
          default:
            if (defaultBehaviorsSortedDash == null) {
              defaultBehaviorsSortedDash = new List<trBehavior>(DefaultBehaviors.Values);
              sortIt = true;
            }
            theList = defaultBehaviorsSortedDash;
            break;
        }

        if (sortIt) {
          // it's new; sort it.
          // the list below contains the ordering only for items we care about.
          // the list returned will be in its normal (enum-based) order,
          // except the items below will be at the front of it, in the order given.
          // note that items go into action panels based on the behaviorArea,
          // so the fact that not all the LED colors are neighbors in the list is OK.
          List<trBehaviorType> orderingWeCareAbout = new List<trBehaviorType> {
            // lights
            trBehaviorType.EYERING,
            trBehaviorType.COLOR_OFF,
            trBehaviorType.COLOR_WHITE,
            
            // movement
            trBehaviorType.PUPPET,
            trBehaviorType.MOVE_DISC_STRAIGHT,
            trBehaviorType.MOVE_DISC_TURN,
            trBehaviorType.MOVE_CONT_STRAIGHT,
            trBehaviorType.MOVE_CONT_SPIN,
            trBehaviorType.MOVE_STOP,
            trBehaviorType.HEAD_PAN,
            trBehaviorType.HEAD_TILT,
            trBehaviorType.MOVE_TURN_VOICE,
            trBehaviorType.HEAD_PAN_VOICE,
            
          };
          
          for (int n = orderingWeCareAbout.Count - 1; n >= 0; --n) {
            int foundIndex = -1;
            for (int m = 0; m < theList.Count; ++m) {
              if (theList[m].Type == orderingWeCareAbout[n]) {
                foundIndex = m;
                break;
              }
            }
            
            if (foundIndex != -1) {
            trBehavior foundItem = theList[foundIndex];
              theList.RemoveAt(foundIndex);
              theList.Insert(0, foundItem);
            }
            else {
              WWLog.logError("behavior type not found: " + orderingWeCareAbout[n].ToString());
            }
          }
        }
        
        return theList;
      }
    }

    public bool isLocked(){
      // We are not block behaviors in mission mode
      if(trDataManager.Instance.IsInNormalMissionMode){
        return false;
      }

      bool result = false; 
      switch(Type){
        case trBehaviorType.MOODY_ANIMATION:
          result = !trRewardsManager.Instance.IsAvailableRobotAnim(Animation.id);
          break;
        case trBehaviorType.MAPSET:
          result = !trRewardsManager.Instance.IsAvailableBehavior(this);
          break;
        // note: even though sounds can be locked, we are not returning the result for sounds because it is under categories
      }
      return result;
    }

    private static Dictionary<trBehaviorType, string> behaviorTypeToUserFacingName = null;
    public static Dictionary<trBehaviorType, string> TypeToUserFacingName {
      get {
        if (behaviorTypeToUserFacingName == null){
          behaviorTypeToUserFacingName = new Dictionary<trBehaviorType, string>();
          
          // DO NOT EDIT THE CODE BELOW! IT IS GENERATED BY THIS SPREADSHEET:
          // https://docs.google.com/spreadsheets/d/1gccOs1yIe2lUmDRtA9-zyCjHAi_aCFkYPwXHsqK9djM/edit#gid=859972063
          // to make changes, edit the spreadsheet, then manually copy-and-paste the code generated in the spreadsheet.
          behaviorTypeToUserFacingName[trBehaviorType.START_STATE] = "@!@Start@!@";
          behaviorTypeToUserFacingName[trBehaviorType.DO_NOTHING] = "@!@Do Nothing@!@";

          behaviorTypeToUserFacingName[trBehaviorType.COLOR_OFF] = "@!@Lights Off@!@";
          behaviorTypeToUserFacingName[trBehaviorType.COLOR_RED] = "@!@Red Lights@!@";
          behaviorTypeToUserFacingName[trBehaviorType.COLOR_YELLOW] = "@!@Yellow Lights@!@";
          behaviorTypeToUserFacingName[trBehaviorType.COLOR_ORANGE] = "@!@Orange Lights@!@";
          behaviorTypeToUserFacingName[trBehaviorType.COLOR_GREEN] = "@!@Green Lights@!@";
          behaviorTypeToUserFacingName[trBehaviorType.COLOR_CYAN] = "@!@Cyan Lights@!@";
          behaviorTypeToUserFacingName[trBehaviorType.COLOR_BLUE] = "@!@Blue Lights@!@";
          behaviorTypeToUserFacingName[trBehaviorType.COLOR_MAGENTA] = "@!@Magenta Lights@!@";
          behaviorTypeToUserFacingName[trBehaviorType.COLOR_WHITE] = "@!@White Lights@!@";
          behaviorTypeToUserFacingName[trBehaviorType.SOUND_USER] = "@!@Custom Sound@!@";
          behaviorTypeToUserFacingName[trBehaviorType.SOUND_INTERNAL] = "";
          behaviorTypeToUserFacingName[trBehaviorType.SOUND_VOCAL_BRAVE] = "@!@Brave Sound@!@";
          behaviorTypeToUserFacingName[trBehaviorType.SOUND_VOCAL_CAUTIOUS] = "@!@Cautious Sound@!@";
          behaviorTypeToUserFacingName[trBehaviorType.SOUND_VOCAL_CURIOUS] = "@!@Curious Sound@!@";
          behaviorTypeToUserFacingName[trBehaviorType.SOUND_VOCAL_FRUSTRATED] = "@!@Frustrated Sound@!@";
          behaviorTypeToUserFacingName[trBehaviorType.SOUND_VOCAL_HAPPY] = "@!@Happy Sound@!@";
          behaviorTypeToUserFacingName[trBehaviorType.SOUND_VOCAL_SILLY] = "@!@Silly Sound@!@";
          behaviorTypeToUserFacingName[trBehaviorType.SOUND_VOCAL_SURPRISED] = "@!@Surprised Sound@!@";
          behaviorTypeToUserFacingName[trBehaviorType.SOUND_ANIMAL] = "@!@Animal Sound@!@";
          behaviorTypeToUserFacingName[trBehaviorType.SOUND_SFX] = "@!@Effects Sound@!@";
          behaviorTypeToUserFacingName[trBehaviorType.SOUND_TRANSPORT] = "@!@Transport Sound@!@";

          behaviorTypeToUserFacingName[trBehaviorType.MOVE_STOP] = "@!@Stop@!@";
          behaviorTypeToUserFacingName[trBehaviorType.MOVE_CONT_STRAIGHT] = "@!@Move@!@";
          behaviorTypeToUserFacingName[trBehaviorType.MOVE_CONT_SPIN] = "@!@Moving Basics@!@";
          behaviorTypeToUserFacingName[trBehaviorType.MOVE_DISC_STRAIGHT] = "@!@Move To@!@";
          behaviorTypeToUserFacingName[trBehaviorType.MOVE_DISC_TURN] = "@!@Spin To@!@";
          behaviorTypeToUserFacingName[trBehaviorType.MOVE_TURN_VOICE] = "@!@Voice Turn@!@";



          behaviorTypeToUserFacingName[trBehaviorType.HEAD_PAN] = "@!@Head Pan@!@";
          behaviorTypeToUserFacingName[trBehaviorType.HEAD_TILT] = "@!@Head Tilt@!@";
          behaviorTypeToUserFacingName[trBehaviorType.HEAD_PAN_VOICE] = "@!@Voice Look@!@";

          behaviorTypeToUserFacingName[trBehaviorType.EYERING] = "@!@Eye Ring@!@";

          behaviorTypeToUserFacingName[trBehaviorType.MISSION] = "";
          behaviorTypeToUserFacingName[trBehaviorType.MAPSET] = "@!@Behavior@!@";
          behaviorTypeToUserFacingName[trBehaviorType.OMNI] = "@!@Listener@!@";
          behaviorTypeToUserFacingName[trBehaviorType.LAUNCH_RELOAD_LEFT] = "@!@Reload Left@!@";
          behaviorTypeToUserFacingName[trBehaviorType.LAUNCH_RELOAD_RIGHT] = "@!@Reload Right@!@";
          behaviorTypeToUserFacingName[trBehaviorType.LAUNCH_FLING] = "@!@Launch@!@";
          behaviorTypeToUserFacingName[trBehaviorType.RUN_SPARK] = "@!@Run Other Program@!@";
          behaviorTypeToUserFacingName[trBehaviorType.MOODY_ANIMATION] = "@!@Animation@!@";
          behaviorTypeToUserFacingName[trBehaviorType.PUPPET] = "@!@Puppet@!@";
        }
        return behaviorTypeToUserFacingName;
      }
    }

    private static Dictionary<trBehaviorType, string> behaviorTypeToDescription = null;
    public static Dictionary<trBehaviorType, string> TypeToDescription {
      get {
        if (behaviorTypeToDescription == null){
          behaviorTypeToDescription = new Dictionary<trBehaviorType, string>();
          
          // DO NOT EDIT THE CODE BELOW! IT IS GENERATED BY THIS SPREADSHEET:
          // https://docs.google.com/spreadsheets/d/1gccOs1yIe2lUmDRtA9-zyCjHAi_aCFkYPwXHsqK9djM/edit#gid=859972063
          // to make changes, edit the spreadsheet, then manually copy-and-paste the code generated in the spreadsheet.
          behaviorTypeToDescription[trBehaviorType.START_STATE] = "@!@ROBOT will begin the program here!@!@";
          behaviorTypeToDescription[trBehaviorType.DO_NOTHING] = "@!@ROBOT will do nothing until a cue is triggered.@!@";

          behaviorTypeToDescription[trBehaviorType.COLOR_OFF] = "@!@ROBOT will turn the colored lights <b>COLOR</b>.@!@";
          behaviorTypeToDescription[trBehaviorType.COLOR_RED] = "@!@ROBOT will turn the colored lights <b>COLOR</b>.@!@";
          behaviorTypeToDescription[trBehaviorType.COLOR_YELLOW] = "@!@ROBOT will turn the colored lights <b>COLOR</b>.@!@";
          behaviorTypeToDescription[trBehaviorType.COLOR_ORANGE] = "@!@ROBOT will turn the colored lights <b>COLOR</b>.@!@";
          behaviorTypeToDescription[trBehaviorType.COLOR_GREEN] = "@!@ROBOT will turn the colored lights <b>COLOR</b>.@!@";
          behaviorTypeToDescription[trBehaviorType.COLOR_CYAN] = "@!@ROBOT will turn the colored lights <b>COLOR</b>.@!@";
          behaviorTypeToDescription[trBehaviorType.COLOR_BLUE] = "@!@ROBOT will turn the colored lights <b>COLOR</b>.@!@";
          behaviorTypeToDescription[trBehaviorType.COLOR_MAGENTA] = "@!@ROBOT will turn the colored lights <b>COLOR</b>.@!@";
          behaviorTypeToDescription[trBehaviorType.COLOR_WHITE] = "@!@ROBOT will turn the colored lights <b>COLOR</b>.@!@";
          behaviorTypeToDescription[trBehaviorType.SOUND_USER] = "@!@ROBOT will play a custom sound recorded by <b>you</b>!@!@";
          behaviorTypeToDescription[trBehaviorType.SOUND_INTERNAL] = "";
          behaviorTypeToDescription[trBehaviorType.SOUND_VOCAL_BRAVE] = "@!@ROBOT will play a <b>brave</b> sound.\nHere we go!@!@";
          behaviorTypeToDescription[trBehaviorType.SOUND_VOCAL_CAUTIOUS] = "@!@ROBOT will play a <b>cautious</b> sound.\nBe careful!@!@";
          behaviorTypeToDescription[trBehaviorType.SOUND_VOCAL_CURIOUS] = "@!@ROBOT will play a <b>curious</b> sound.\nWhat's over there?@!@";
          behaviorTypeToDescription[trBehaviorType.SOUND_VOCAL_FRUSTRATED] = "@!@ROBOT will play a <b>frustrated</b> sound.\nDoh!@!@";
          behaviorTypeToDescription[trBehaviorType.SOUND_VOCAL_HAPPY] = "@!@ROBOT will play a <b>happy</b> sound.\nHooray!@!@";
          behaviorTypeToDescription[trBehaviorType.SOUND_VOCAL_SILLY] = "@!@ROBOT will play a <b>silly</b> sound.\nHa ha ha!@!@";
          behaviorTypeToDescription[trBehaviorType.SOUND_VOCAL_SURPRISED] = "@!@ROBOT will play a <b>surprised</b> sound.\nOhmigosh!@!@";
          behaviorTypeToDescription[trBehaviorType.SOUND_ANIMAL] = "@!@ROBOT will play an <b>animal</b> sound.\nMoo Quack Oink.@!@";
          behaviorTypeToDescription[trBehaviorType.SOUND_SFX] = "@!@ROBOT will play a cool <b>sound-effect</b>.\nPeew peew!@!@";
          behaviorTypeToDescription[trBehaviorType.SOUND_TRANSPORT] = "@!@ROBOT will play a <b>car or airplane</b> sound.\nVrrrrrrm!@!@";

          behaviorTypeToDescription[trBehaviorType.MOVE_STOP] = "@!@ROBOT will stop moving.@!@";
          behaviorTypeToDescription[trBehaviorType.MOVE_CONT_STRAIGHT] = "@!@ROBOT will begin moving <b>forward</b> or <b>backward</b> at the speed you choose.@!@";
          behaviorTypeToDescription[trBehaviorType.MOVE_CONT_SPIN] = "@!@ROBOT will begin <b>spinning</b>, <b>turning</b>, or going <b>straight</b> depending on the speed you choose for each wheel.@!@";
          behaviorTypeToDescription[trBehaviorType.MOVE_DISC_STRAIGHT] = "@!@ROBOT will move <b>forward</b> or <b>backward</b> a specific distance and then <b>stop</b>.@!@";
          behaviorTypeToDescription[trBehaviorType.MOVE_DISC_TURN] = "@!@ROBOT will turn <b>left</b> or <b>right</b> a specific amount and then <b>stop</b>.@!@";
          behaviorTypeToDescription[trBehaviorType.MOVE_TURN_VOICE] = "@!@ROBOT will turn <b>toward the person speaking</b>.@!@";



          behaviorTypeToDescription[trBehaviorType.HEAD_PAN] = "@!@ROBOT's head will turn <b>left</b> or <b>right</b> to the specified angle.@!@";
          behaviorTypeToDescription[trBehaviorType.HEAD_TILT] = "@!@ROBOT's head will tilt <b>up</b> or <b>down</b> to the specified angle.@!@";
          behaviorTypeToDescription[trBehaviorType.HEAD_PAN_VOICE] = "@!@ROBOT will look <b>toward the person speaking</b>.@!@";

          behaviorTypeToDescription[trBehaviorType.EYERING] = "@!@ROBOT will show the eye pattern you choose.@!@";

          behaviorTypeToDescription[trBehaviorType.MISSION] = "";
          behaviorTypeToDescription[trBehaviorType.MAPSET] = "";
          behaviorTypeToDescription[trBehaviorType.OMNI] = "@!@ROBOT will <b>always</b> respond to any cues linked from this action.\nTip: Cues cannot link <b>to</b> this action.@!@";
          behaviorTypeToDescription[trBehaviorType.LAUNCH_RELOAD_LEFT] = "@!@ROBOT will load the ball <b>on the left</b>.@!@";
          behaviorTypeToDescription[trBehaviorType.LAUNCH_RELOAD_RIGHT] = "@!@ROBOT will load the ball <b>on the right</b>.@!@";
          behaviorTypeToDescription[trBehaviorType.LAUNCH_FLING] = "@!@ROBOT will launch the ball with the power you choose.@!@";
          behaviorTypeToDescription[trBehaviorType.RUN_SPARK] = "";
          behaviorTypeToDescription[trBehaviorType.MOODY_ANIMATION] = "";
          behaviorTypeToDescription[trBehaviorType.PUPPET] = "@!@Puppet-Time!@!@";

          trUIUtil.normalizeUIText<trBehaviorType>(behaviorTypeToDescription);
        }
        return behaviorTypeToDescription;
      }
    }

    private Dictionary<trBehaviorType, string[]> behaviorTypeToHints = null;
    private Dictionary<trBehaviorType, string[]> TypeToHints {
      get {
        if (behaviorTypeToHints == null){
          behaviorTypeToHints = new Dictionary<trBehaviorType, string[]>();
          
          // DO NOT EDIT THE CODE BELOW! IT IS GENERATED BY THIS SPREADSHEET:
          // https://docs.google.com/spreadsheets/d/1AX3aTgnjvzAIEBgCBxmKnL01KYHxg5RmSYjyXul_bUQ/edit#gid=0
          // to make changes, edit the spreadsheet, then manually copy-and-paste the code generated in the spreadsheet.behaviorTypeToHints[trBehaviorType.DO_NOTHING] = new string[]{"<b>DEFAULT</b> hint 1", "DEFAULT hint #2"};
          behaviorTypeToHints[trBehaviorType.DO_NOTHING] = new string[]{"<b>DEFAULT</b> hint 1", "DEFAULT hint #2"};

          behaviorTypeToHints[trBehaviorType.COLOR_OFF] = new string[]{"<b>DEFAULT</b> hint 1", "DEFAULT hint #2"};
          behaviorTypeToHints[trBehaviorType.COLOR_RED] = new string[]{"<b>DEFAULT</b> hint 1", "DEFAULT hint #2"};
          behaviorTypeToHints[trBehaviorType.COLOR_YELLOW] = new string[]{"<b>DEFAULT</b> hint 1", "DEFAULT hint #2"};
          behaviorTypeToHints[trBehaviorType.COLOR_ORANGE] = new string[]{"<b>DEFAULT</b> hint 1", "DEFAULT hint #2"};
          behaviorTypeToHints[trBehaviorType.COLOR_GREEN] = new string[]{"<b>DEFAULT</b> hint 1", "DEFAULT hint #2"};
          behaviorTypeToHints[trBehaviorType.COLOR_CYAN] = new string[]{"<b>DEFAULT</b> hint 1", "DEFAULT hint #2"};
          behaviorTypeToHints[trBehaviorType.COLOR_BLUE] = new string[]{"<b>DEFAULT</b> hint 1", "DEFAULT hint #2"};
          behaviorTypeToHints[trBehaviorType.COLOR_MAGENTA] = new string[]{"<b>DEFAULT</b> hint 1", "DEFAULT hint #2"};
          behaviorTypeToHints[trBehaviorType.COLOR_WHITE] = new string[]{"<b>DEFAULT</b> hint 1", "DEFAULT hint #2"};

          behaviorTypeToHints[trBehaviorType.SOUND_USER] = new string[]{"<b>DEFAULT</b> hint 1", "DEFAULT hint #2"};
          behaviorTypeToHints[trBehaviorType.SOUND_INTERNAL] = new string[]{"<b>DEFAULT</b> hint 1", "DEFAULT hint #2"};
          behaviorTypeToHints[trBehaviorType.SOUND_VOCAL_BRAVE] = new string[]{"<b>DEFAULT</b> hint 1", "DEFAULT hint #2"};
          behaviorTypeToHints[trBehaviorType.SOUND_VOCAL_CAUTIOUS] = new string[]{"<b>DEFAULT</b> hint 1", "DEFAULT hint #2"};
          behaviorTypeToHints[trBehaviorType.SOUND_VOCAL_CURIOUS] = new string[]{"<b>DEFAULT</b> hint 1", "DEFAULT hint #2"};
          behaviorTypeToHints[trBehaviorType.SOUND_VOCAL_FRUSTRATED] = new string[]{"<b>DEFAULT</b> hint 1", "DEFAULT hint #2"};
          behaviorTypeToHints[trBehaviorType.SOUND_VOCAL_HAPPY] = new string[]{"<b>DEFAULT</b> hint 1", "DEFAULT hint #2"};
          behaviorTypeToHints[trBehaviorType.SOUND_VOCAL_SILLY] = new string[]{"<b>DEFAULT</b> hint 1", "DEFAULT hint #2"};
          behaviorTypeToHints[trBehaviorType.SOUND_VOCAL_SURPRISED] = new string[]{"<b>DEFAULT</b> hint 1", "DEFAULT hint #2"};
          behaviorTypeToHints[trBehaviorType.SOUND_ANIMAL] = new string[]{"<b>DEFAULT</b> hint 1", "DEFAULT hint #2"};
          behaviorTypeToHints[trBehaviorType.SOUND_SFX] = new string[]{"<b>DEFAULT</b> hint 1", "DEFAULT hint #2"};
          behaviorTypeToHints[trBehaviorType.SOUND_TRANSPORT] = new string[]{"<b>DEFAULT</b> hint 1", "DEFAULT hint #2"};

          behaviorTypeToHints[trBehaviorType.PUPPET] = new string[]{};
          behaviorTypeToHints[trBehaviorType.MOVE_STOP] = new string[]{};
          behaviorTypeToHints[trBehaviorType.MOVE_CONT_STRAIGHT] = new string[]{"<b>Test Move</b>", "<b>Speed</b>, cm"};
          behaviorTypeToHints[trBehaviorType.MOVE_CONT_SPIN] = new string[]{"<b>Test Spin</b>", "<b>Speed</b>, Right/Left"};
          behaviorTypeToHints[trBehaviorType.MOVE_DISC_STRAIGHT] = new string[]{"<b>Test Move</b>", "<b>Distance</b>, cm"};
          behaviorTypeToHints[trBehaviorType.MOVE_DISC_TURN] = new string[]{"<b>Test Spin</b>", "<b>Angle</b>, Right/Left"};
          behaviorTypeToHints[trBehaviorType.MOVE_TURN_VOICE] = new string[]{};

          behaviorTypeToHints[trBehaviorType.HEAD_PAN] = new string[]{"<b>Test Pan</b>", "<b>Angle</b>, Right/Left"};
          behaviorTypeToHints[trBehaviorType.HEAD_TILT] = new string[]{"<b>Test Tilt</b>", "<b>Angle</b>, Up/Down"};
          behaviorTypeToHints[trBehaviorType.HEAD_PAN_VOICE] = new string[]{};

          behaviorTypeToHints[trBehaviorType.EYERING] = new string[]{"<b>DEFAULT</b> hint 1", "DEFAULT hint #2"};

          behaviorTypeToHints[trBehaviorType.MISSION] = new string[]{"<b>DEFAULT</b> hint 1", "DEFAULT hint #2"};
          behaviorTypeToHints[trBehaviorType.MAPSET] = new string[]{"<b>DEFAULT</b> hint 1", "DEFAULT hint #2"};
          behaviorTypeToHints[trBehaviorType.OMNI] = new string[]{"<b>DEFAULT</b> hint 1", "DEFAULT hint #2"};
          behaviorTypeToHints[trBehaviorType.RUN_SPARK] = new string[]{"<b>DEFAULT</b> hint 1", "DEFAULT hint #2"};
          
          trUIUtil.normalizeUIText<trBehaviorType>(behaviorTypeToHints);          
        }
        return behaviorTypeToHints;
      }
    }

    private static string boldify(string s) {
      return "<b>" + s + "</b>";
    }

    // returns a string describing the very core of this specific action, including parameter value.
    // eg "Dash waits 5s" or "Dash turns 45º"
    public string autoTransitionDescriptionLocalized(piRobotType robotType) {
      float  param = RunningParamValue;
      float  absParam = Mathf.Abs(param);
      string ret;

      switch (Type) {
        default:
          WWLog.logError("unhandled behavior type: " + Type.ToString());
          // xl8_info: ROBOT will be "Dash or Dot".
          ret = wwLoca.Format("@!@ROBOT will go to the next state <b>automatically</b>.@!@");
          break;

        case trBehaviorType.START_STATE:
        case trBehaviorType.MISSION:
        case trBehaviorType.MAPSET:
        case trBehaviorType.RUN_SPARK:
        case trBehaviorType.OMNI:
        case trBehaviorType.DO_NOTHING:
          // xl8_info: ROBOT will be "Dash or Dot".
          ret = wwLoca.Format("@!@ROBOT will go to the next state <b>right away</b>.@!@");
          break;

        case trBehaviorType.COLOR_OFF:
        case trBehaviorType.COLOR_RED:
        case trBehaviorType.COLOR_YELLOW:
        case trBehaviorType.COLOR_ORANGE:
        case trBehaviorType.COLOR_GREEN:
        case trBehaviorType.COLOR_CYAN:
        case trBehaviorType.COLOR_BLUE:
        case trBehaviorType.COLOR_MAGENTA:
        case trBehaviorType.COLOR_WHITE:
          // xl8_info: ROBOT will be "Dash or Dot".
          ret = wwLoca.Format("@!@ROBOT will go to the next state right after <b>changing color</b>.@!@");
          break;

        case trBehaviorType.EYERING:
          // xl8_info: ROBOT will be "Dash or Dot".
          ret = wwLoca.Format("@!@ROBOT will go to the next state right after <b>changing eye-pattern</b>.@!@");
          break;

        case trBehaviorType.SOUND_USER:
          // xl8_info: ROBOT will be "Dash or Dot".
          ret = wwLoca.Format("@!@ROBOT will go to the next state after <b>playing the custom sound</b>.@!@");
          break;

        case trBehaviorType.SOUND_INTERNAL:
        case trBehaviorType.SOUND_VOCAL_BRAVE:
        case trBehaviorType.SOUND_VOCAL_CAUTIOUS:
        case trBehaviorType.SOUND_VOCAL_CURIOUS:
        case trBehaviorType.SOUND_VOCAL_FRUSTRATED:
        case trBehaviorType.SOUND_VOCAL_HAPPY:
        case trBehaviorType.SOUND_VOCAL_SILLY:
        case trBehaviorType.SOUND_VOCAL_SURPRISED:
        case trBehaviorType.SOUND_ANIMAL:
        case trBehaviorType.SOUND_SFX:
        case trBehaviorType.SOUND_TRANSPORT:
          // xl8_info: ROBOT will be "Dash or Dot".
          ret = wwLoca.Format("@!@ROBOT will go to the next state after <b>playing the sound</b>.@!@");
          break;

        case trBehaviorType.PUPPET:
        case trBehaviorType.MOODY_ANIMATION:
          // xl8_info: ROBOT will be "Dash or Dot".
          ret = wwLoca.Format("@!@ROBOT will go to the next state after <b>playing the animation</b>.@!@");
          break;

        case trBehaviorType.LAUNCH_FLING:
          // xl8_info: ROBOT will be "Dash or Dot".
          ret = wwLoca.Format("@!@ROBOT will go to the next state after <b>launching the ball</b>.@!@");
          break;

        case trBehaviorType.LAUNCH_RELOAD_LEFT:
          // xl8_info: ROBOT will be "Dash or Dot".
          ret = wwLoca.Format("@!@ROBOT will go to the next state after <b>loading the ball left</b>.@!@");
          break;

        case trBehaviorType.LAUNCH_RELOAD_RIGHT:
          // xl8_info: ROBOT will be "Dash or Dot".
          ret = wwLoca.Format("@!@ROBOT will go to the next state after <b>loading the ball right</b>.@!@");
          break;

        case trBehaviorType.MOVE_CONT_STRAIGHT:
          if (trStringFactory.isForward(param)) {
            // xl8_info: ROBOT will be "Dash or Dot". {0} will be like "7s".
            ret = wwLoca.Format("@!@ROBOT will go to the next state after <b>driving forward for {0}</b>.@!@", trStringFactory.GetTimeText(CONTINUOUS_MOVE_DURATION));
          }
          else {
            // xl8_info: ROBOT will be "Dash or Dot". {0} will be like "7s".
            ret = wwLoca.Format("@!@ROBOT will go to the next state after <b>driving backward for {0}</b>.@!@", trStringFactory.GetTimeText(CONTINUOUS_MOVE_DURATION));
          }

          break;

        case trBehaviorType.MOVE_CONT_SPIN:
          // xl8_info: ROBOT will be "Dash or Dot". {0} will be like "7s".
          ret = wwLoca.Format("@!@ROBOT will go to the next state after <b>driving for {0}</b>.@!@", trStringFactory.GetTimeText(CONTINUOUS_MOVE_DURATION));
          break;

        case trBehaviorType.MOVE_DISC_STRAIGHT:
          if (trStringFactory.isForward(param)) {
            // xl8_info: ROBOT will be "Dash or Dot". {0} will be like "7cm".
            ret = wwLoca.Format("@!@ROBOT will go to the next state after <b>moving {0} forward</b>.@!@", trStringFactory.GetDistanceText(absParam));
          }
          else {
            // xl8_info: ROBOT will be "Dash or Dot". {0} will be like "7cm".
            ret = wwLoca.Format("@!@ROBOT will go to the next state after <b>moving {0} backward</b>.@!@", trStringFactory.GetDistanceText(absParam));
          }
          break;

        case trBehaviorType.MOVE_DISC_TURN:
          if (trStringFactory.isLeft(param)) {
            // xl8_info: ROBOT will be "Dash or Dot". {0} will be like "7º".
            ret = wwLoca.Format("@!@ROBOT will go to the next state after <b>turning {0} left</b>.@!@", trStringFactory.GetDegreeText(absParam));
          }
          else {
            // xl8_info: ROBOT will be "Dash or Dot". {0} will be like "7º".
            ret = wwLoca.Format("@!@ROBOT will go to the next state after <b>turning {0} right</b>.@!@", trStringFactory.GetDegreeText(absParam));
          }
          break;

        case trBehaviorType.MOVE_STOP:
          // xl8_info: ROBOT will be "Dash or Dot".
          ret = wwLoca.Format("@!@ROBOT will go to the next state right after <b>stopping</b>.@!@");
          break;

        case trBehaviorType.MOVE_TURN_VOICE:
          // xl8_info: ROBOT will be "Dash or Dot".
          ret = wwLoca.Format("@!@ROBOT will go to the next state after <b>turning to the voice</b>.@!@");
          break;

        case trBehaviorType.HEAD_PAN:
          if (trStringFactory.isLeft(param)) {
            // xl8_info: ROBOT will be "Dash or Dot". {0} will be like "7º".
            ret = wwLoca.Format("@!@ROBOT will go to the next state after <b>looking {0} left</b>.@!@", trStringFactory.GetDegreeText(absParam));
          }
          else {
            // xl8_info: ROBOT will be "Dash or Dot". {0} will be like "7º".
            ret = wwLoca.Format("@!@ROBOT will go to the next state after <b>looking {0} right</b>.@!@", trStringFactory.GetDegreeText(absParam));
          }
          break;
        case trBehaviorType.HEAD_TILT:
          if (trStringFactory.isUp(param)) {
            // xl8_info: ROBOT will be "Dash or Dot". {0} will be like "7º".
            ret = wwLoca.Format("@!@ROBOT will go to the next state after <b>looking {0} up</b>.@!@", trStringFactory.GetDegreeText(absParam));
          }
          else {
            // xl8_info: ROBOT will be "Dash or Dot". {0} will be like "7º".
            ret = wwLoca.Format("@!@ROBOT will go to the next state after <b>looking {0} down</b>.@!@", trStringFactory.GetDegreeText(absParam));
          }
          break;
        case trBehaviorType.HEAD_PAN_VOICE:
          // xl8_info: {0} will be "Dash or Dot".
          ret = wwLoca.Format("@!@{0} will go to the next state after <b>looking to the voice</b>.@!@");
          break;
        }

      ret = DoSubstitutions(ret);

      ret = trUIUtil.normalizeUIText(ret);

      return ret;
    }

    public virtual string UserFacingNameLocalized {
      get {
        if(Type == trBehaviorType.MAPSET) {
          return wwLoca.Format(MapSet.Name);
        }
        else if(Type == trBehaviorType.MISSION){
          return wwLoca.Format(MissionFileInfo.MissionName);
        }
        else if (Type == trBehaviorType.MOODY_ANIMATION && Animation != null) {
          return Animation.UserFacingNameLocalized;
        }
        else if (isSoundBehaviour()) {
          if ((uint)RunningParamValue == 0) {
            return trRobotSounds.Instance.getCategoryNameLocalized(Type);
          }
          else {
            trRobotSound trRS = trRobotSounds.Instance.GetSound((uint)RunningParamValue);
            if (trRS == null) {
              WWLog.logError("unknown sound ID: " + (uint)RunningParamValue);
              return "unknown";
            }
            return trRS.UserFacingNameLocalized;
          }
        }
        else {
          if (TypeToUserFacingName.ContainsKey(Type)) {
            return wwLoca.Format(TypeToUserFacingName[Type]);
          }
          else {
//          apparently this happens.
//          WWLog.logError("no user-facing name known: " + Type.ToString());
            return Type.ToString();
          }
        }
      }
    }

    public string DescriptionLocalized {
      get {
        if(Type == trBehaviorType.MAPSET) {
          return wwLoca.Format(MapSet.Name);
        }
        else if(Type == trBehaviorType.MOODY_ANIMATION) {
          return Animation.UserFacingNameLocalized;
        }
        else if(Type == trBehaviorType.MISSION){
          return wwLoca.Format(MissionFileInfo.MissionName);
        }
        else if (TypeToDescription.ContainsKey(Type)) {
          return DoSubstitutions(wwLoca.Format(TypeToDescription[Type]));
        }
        else {
          return Type.ToString();
        }        
      }
    }

    public string DoSubstitutions(string s) {
      string ret = s;

      string robotName = wwLoca.Format(trDataManager.Instance.CurrentRobotTypeSelected == piRobotType.DASH ? "Dash" : "Dot");

      ret = Regex.Replace(ret, cRexRobotName, robotName);

      if (Type.IsColor()) {
        ret = Regex.Replace(ret, cRexColor, wwLoca.Format(Type.toLEDColor().UserFacingName()));
      }

      return ret;
    }

    public string[] Hints {
      get {
        if (TypeToHints.ContainsKey(Type)) {
          return TypeToHints[Type];
        }
        else {
          return new string[]{};
        }
      }
    }

    public string TelemetryDetail(trState state) {
      string ret = "";
      if (isAnimation()) {
        uint itemID = (uint)state.BehaviorParameterValue;
        trMoodyAnimation item = trMoodyAnimations.Instance.getAnimation(itemID);
        ret = item.UserFacingNameLocalized + " " + state.Mood.ToString();
      }
      else if (isSoundBehaviour()) {
        uint itemID = (uint)state.BehaviorParameterValue;
        trRobotSound item = trRobotSounds.Instance.GetSound(itemID);
        ret = item.UserFacingNameUnlocalized;
      }
      else if (IsParameterized) {
        string[] items = new string[state.BehaviorParaValuesCount];
        for (int n = 0; n < state.BehaviorParaValuesCount; ++n) {
          items[n] = state.GetBehaviorParameterValue(n).ToString();
        }
        ret = string.Join(", ", items);
      }
      else {
        ret = Type.ToString();
      }

      return ret;
    }

    public static trBehavior GetDefaultBehavior(trBehaviorType type, int animId = 0){
      string uuid = "";
      if(type == Turing.trBehaviorType.MOODY_ANIMATION){
        uuid = trBehaviorType.MOODY_ANIMATION + "_" + animId.ToString();
      }
      else{
        uuid = "BEHAVIOR_"+ type.ToString();
      }

      return GetDefaultBehavior(uuid);
    }

    public static trBehavior GetDefaultBehavior(string uuid){
      if(!DefaultBehaviors.ContainsKey(uuid)){
        WWLog.logError("Invalid default behavior type: " + uuid);
        return null;
      }
      return DefaultBehaviors[uuid];
    }

    public bool isSimpleMoveBehaviour(){
      bool result = false;
      switch(Type){
        case trBehaviorType.PUPPET:
        case trBehaviorType.MOVE_CONT_CIRCLE_CCW:
        case trBehaviorType.MOVE_CONT_CIRCLE_CW:
        case trBehaviorType.MOVE_CONT_SPIN:
        case trBehaviorType.MOVE_CONT_STRAIGHT:
        case trBehaviorType.MOVE_DISC_STRAIGHT:
        case trBehaviorType.MOVE_DISC_TURN:
        case trBehaviorType.MOVE_STOP:
          result = true;
          break;
      }
      return result;
    }
    public bool isExpression() {
      if (trExpressions.Instance.GetCategories().Contains(Type)) {
        return true;
      }
      return false;
    }

    public bool isAnimation() {
      return Type == trBehaviorType.MOODY_ANIMATION;
    }

    public bool isMapSetBehaviour(){
      return Type == trBehaviorType.MAPSET;
    }
    public bool isSoundBehaviour(){
      return Type.IsSound();
    }

    public bool isEyeRingControlBehaviour(){
      bool result = false;
      switch(Type){
        case trBehaviorType.EYERING:
          result = true;
          break;
      }
      return result;
    }

    #region serialization
    protected override void IntoJson(WW.SimpleJSON.JSONClass jsc) {
      if (Type == trBehaviorType.MAPSET) {
        jsc[TOKENS.MAP_SET] = MapSet.ToJson();
      }
      else if (Type == trBehaviorType.MOODY_ANIMATION) {
        if (Animation != null) {
          jsc[TOKENS.MOODY_ANIMATION] = Animation.ToJson();
        }
      }

      if(IsMissionBehavior){
        if (MissionFileInfo == null) {
          WWLog.logError("MissionFileInfo is NULL! Tell Leisen! TUR-319.");
        }
        else {
          jsc[TOKENS.MISSION] = MissionFileInfo.UUID;
        }
      }
      base.IntoJson(jsc);
    }
    
    protected override void OutOfJson(JSONClass jsc) {
      base.OutOfJson(jsc);
      if (Type == trBehaviorType.MAPSET) {
        MapSet = trMapSet.FromJson(jsc[TOKENS.MAP_SET].AsObject);
      }
      else if (Type == trBehaviorType.MOODY_ANIMATION) {
        if (jsc.ContainsKey(TOKENS.MOODY_ANIMATION)) {
          Animation = trMoodyAnimation.FromJson(jsc[TOKENS.MOODY_ANIMATION].AsObject);
        }
      }

      if(IsMissionBehavior){
        string uuid = jsc[TOKENS.MISSION].Value;
        //Make sure this happens after adminMissionInfo is loaded
        
        MissionFileInfo = trDataManager.Instance.MissionMng.AuthoringMissionInfo.GetMissionFile(uuid);

      }
    }

    #endregion serialization

    public override string ToString ()
    {
      return string.Format ("[trBehavior: UserFacingName={0}, \nActive={1}, \nMapSet={2}, \nHasStarted={3}, \nHasFinished={4}, \nHandlers={5}]", 
                            UserFacingNameLocalized, Active, MapSet, hasStarted, hasFinished, handlers);
    }
    
    public bool Active {
      get {
        return active;
      }
    }
    
    public void SetActive(bool value, piBotBase robot, trMoodType mood, float para = 0) {
      List<float> parameters = new List<float> { para };
      SetActive(value, robot, mood, parameters);
    }

    public void SetActive(bool value, piBotBase robot, trMoodType mood, List<float> parameters) {
      if (value == active) {
        return;
      }      
      hasStarted = false;
      hasFinished = false;      
      active = value;

      if (value) {
        if( parameters != null && parameters.Count > 0){
          runningParamValues = new List<float>(parameters);
        } 
        runningMood = mood;
        startBehaving(robot);
      }
      else {
        stopBehaving(robot);
      }
    }

    protected virtual void addHandlers(){
      
        handlers = new Dictionary<trBehaviorType, BehaviorHandler>();

        handlers.Add (trBehaviorType.DO_NOTHING,                    handleNOOP   );
        handlers.Add (trBehaviorType.OMNI,                          handleNOOP   );
        handlers.Add (trBehaviorType.START_STATE,                   handleNOOP   );
        handlers.Add(trBehaviorType.FUNCTION_END,                   handleNOOP   );

        handlers.Add(trBehaviorType.COLOR_OFF,                      handleColor  );
        handlers.Add(trBehaviorType.COLOR_RED,                      handleColor  );
        handlers.Add(trBehaviorType.COLOR_YELLOW,                   handleColor  );
        handlers.Add(trBehaviorType.COLOR_ORANGE,                   handleColor  );
        handlers.Add(trBehaviorType.COLOR_GREEN,                    handleColor  );
        handlers.Add(trBehaviorType.COLOR_CYAN,                     handleColor  );
        handlers.Add(trBehaviorType.COLOR_BLUE,                     handleColor  );
        handlers.Add(trBehaviorType.COLOR_MAGENTA,                  handleColor  );
        handlers.Add(trBehaviorType.COLOR_WHITE,                    handleColor  );

        //          handlers.Add(trBehaviorType.ANIM_TROPHY,                    handleAnim   );
        //          handlers.Add(trBehaviorType.ANIM_TORNADO,                   handleAnim   );
        //          handlers.Add(trBehaviorType.ANIM_OIL,                       handleAnim   );
        //          handlers.Add(trBehaviorType.ANIM_TRUMPET,                   handleAnim   );
        //          handlers.Add(trBehaviorType.ANIM_DELIVERY,                  handleAnim   );
        //          handlers.Add(trBehaviorType.ANIM_CELEBRATION,               handleAnim   );
        //          handlers.Add(trBehaviorType.ANIM_DANCE_BUTTWIGGLE,          handleAnim   );
        //          handlers.Add(trBehaviorType.ANIM_DANCE_SPINLEFT,            handleAnim   );
        //          handlers.Add(trBehaviorType.ANIM_DANCE_SPINRIGHT,           handleAnim   );
        //          handlers.Add(trBehaviorType.ANIM_DANCE_STAYALIVELEFT,       handleAnim   );
        //          handlers.Add(trBehaviorType.ANIM_DANCE_STAYALIVERIGHT,      handleAnim   );
        //          handlers.Add(trBehaviorType.ANIM_DANCE_TWIST,               handleAnim   );
        //          handlers.Add(trBehaviorType.ANIM_FORWARD_CYCLE_CAUTIOUS,    handleAnim   );
        //          handlers.Add(trBehaviorType.ANIM_FORWARD_CYCLE_CURIOUS,     handleAnim   );
        //          handlers.Add(trBehaviorType.ANIM_FORWARD_CYCLE_FRUSTRATED,  handleAnim   );
        //          handlers.Add(trBehaviorType.ANIM_FORWARD_CYCLE_HAPPY,       handleAnim   );
        //          handlers.Add(trBehaviorType.ANIM_FORWARD_CYCLE_SILLY,       handleAnim   );

        handlers.Add(trBehaviorType.EXPRESSION_CATEGORY_1,          handleExpression );
        handlers.Add(trBehaviorType.EXPRESSION_CATEGORY_2,          handleExpression );
        handlers.Add(trBehaviorType.EXPRESSION_CATEGORY_3,          handleExpression );
        handlers.Add(trBehaviorType.EXPRESSION_CATEGORY_4,          handleExpression );
        handlers.Add(trBehaviorType.EXPRESSION_CATEGORY_5,          handleExpression );

        handlers.Add(trBehaviorType.MOODY_ANIMATION,                handleAnim );

        handlers.Add(trBehaviorType.SOUND_USER,                     handleSound  );
        handlers.Add(trBehaviorType.SOUND_VOCAL_BRAVE,              handleSound  );
        handlers.Add(trBehaviorType.SOUND_VOCAL_CAUTIOUS,           handleSound  );
        handlers.Add(trBehaviorType.SOUND_VOCAL_CURIOUS,            handleSound  );
        handlers.Add(trBehaviorType.SOUND_VOCAL_FRUSTRATED,         handleSound  );
        handlers.Add(trBehaviorType.SOUND_VOCAL_HAPPY,              handleSound  );
        handlers.Add(trBehaviorType.SOUND_VOCAL_SILLY,              handleSound  );
        handlers.Add(trBehaviorType.SOUND_VOCAL_SURPRISED,          handleSound  );
        handlers.Add(trBehaviorType.SOUND_ANIMAL,                   handleSound  );
        handlers.Add(trBehaviorType.SOUND_SFX,                      handleSound  );
        handlers.Add(trBehaviorType.SOUND_TRANSPORT,                handleSound  );
        handlers.Add(trBehaviorType.SOUND_INTERNAL,                 handleSound  );

        handlers.Add(trBehaviorType.MOVE_FB0,                       handleMove   );
        handlers.Add(trBehaviorType.MOVE_F1,                        handleMove   );
        handlers.Add(trBehaviorType.MOVE_F2,                        handleMove   );
        handlers.Add(trBehaviorType.MOVE_F3,                        handleMove   );
        handlers.Add(trBehaviorType.MOVE_B1,                        handleMove   );
        handlers.Add(trBehaviorType.MOVE_B2,                        handleMove   );
        handlers.Add(trBehaviorType.MOVE_B3,                        handleMove   );
        handlers.Add(trBehaviorType.MOVE_LR0,                       handleMove   );
        handlers.Add(trBehaviorType.MOVE_L1,                        handleMove   );
        handlers.Add(trBehaviorType.MOVE_L2,                        handleMove   );
        handlers.Add(trBehaviorType.MOVE_L3,                        handleMove   );
        handlers.Add(trBehaviorType.MOVE_R1,                        handleMove   );
        handlers.Add(trBehaviorType.MOVE_R2,                        handleMove   );
        handlers.Add(trBehaviorType.MOVE_R3,                        handleMove   );
        handlers.Add(trBehaviorType.PUPPET,                         handlePuppet );
        handlers.Add(trBehaviorType.MOVE_STOP,                      handleMove   );
        handlers.Add(trBehaviorType.MOVE_CONT_STRAIGHT,             handleMove   );
        handlers.Add(trBehaviorType.MOVE_CONT_CIRCLE_CCW,           handleMove   );
        handlers.Add(trBehaviorType.MOVE_CONT_CIRCLE_CW,            handleMove   );
        handlers.Add(trBehaviorType.MOVE_CONT_SPIN,                 handleMove   );
        handlers.Add(trBehaviorType.MOVE_DISC_TURN,                 handleMove   );
        handlers.Add(trBehaviorType.MOVE_DISC_STRAIGHT,             handleMove   );
        handlers.Add(trBehaviorType.MOVE_TURN_VOICE,                handleTurnToVoice   );

        handlers.Add(trBehaviorType.HEAD_PAN,                       handleHead   );
        handlers.Add(trBehaviorType.HEAD_TILT,                      handleHead   );
        handlers.Add(trBehaviorType.HEAD_PAN_VOICE,                 handlePanToVoice   );

        handlers.Add(trBehaviorType.EYERING,                        handleEyeRing);

        handlers.Add(trBehaviorType.MAPSET,                         handleMapSet );

        handlers.Add(trBehaviorType.LAUNCH_RELOAD_LEFT,             handleLaunch );
        handlers.Add(trBehaviorType.LAUNCH_RELOAD_RIGHT,            handleLaunch );
        handlers.Add(trBehaviorType.LAUNCH_FLING,                   handleLaunch );

        handlers.Add(trBehaviorType.RUN_SPARK,                      handleRunSpark);
    }
    
    protected Dictionary<trBehaviorType, BehaviorHandler> Handlers {
      get {    
        if (handlers != null) {
          addHandlers();
        }
        return handlers;
      }
    }
    
    protected BehaviorHandler getHandler(trBehaviorType trBT) {
      if (Handlers.ContainsKey(trBT)) {
        return handlers[trBT];
      }
      else {
        if (wwDoOncePerTypeVal<trBehaviorType>.doIt(trBT)) {
          WWLog.logError("unhandled behaviorType: " + trBT.ToString());
        }
        return null;
      }
    }
    
    protected void tryHandle(piBotBase robot, trBehaviorType trBT, BehaveMode mode) {
      BehaviorHandler handler = getHandler(trBT);
      if (handler != null) {
        handler(robot, mode);
      }
    }
    
    public void startBehaving(piBotBase robot) {
      if (robot == null) {
        return;
      }
      hasStarted  = true;
      hasFinished = false;
      timeStart   = Time.time;
      tryHandle(robot, Type, BehaveMode.START);
    }
    
    private void stopBehaving(piBotBase robot) {
      if (robot == null) {
        return;
      }
      tryHandle(robot, Type, BehaveMode.STOP);
    }
    
    //Just execute the behaviour
    public void Execute(piBotBase robot){
      if(robot == null){
        return;
      }
      tryHandle(robot, Type, BehaveMode.START);
    }
    
    public void BehaveContinuous(piBotBase robot) {
      if (robot == null) {
        return;
      }
      tryHandle(robot, Type, BehaveMode.CONTINUE);
    }
    
    public bool isFinished(piBotBase robot) {
      if (HasFinishedUpdater != null) {
        HasFinishedUpdater(robot);
      }
      return hasStarted && hasFinished;
    }

    public bool isFinishedForChallengeCompletion(piBotBase robot){
      bool useRealAuto = false;
      
      useRealAuto = useRealAuto || Type.IsAnimation();
      useRealAuto = useRealAuto || Type.IsSound();
      useRealAuto = useRealAuto || Type == trBehaviorType.MOVE_DISC_TURN;
      useRealAuto = useRealAuto || Type == trBehaviorType.MOVE_DISC_STRAIGHT;
      
      if (useRealAuto) {
        return isFinished(robot);
      }
      
      updateHasFinished_challengeGeneric(robot);
      return hasStarted && hasFinished;
    }
 
    private void handleNOOP(piBotBase robot, BehaveMode mode) {
      // this space intentionally left blank, except for this comment.
      hasStarted = true;
      HasFinishedUpdater = updateHasFinished_shortTime;
    }

    public static Color convertColorType(trBehaviorType trBT) {
      return trBT.toLEDColor().UnityColor();
    }
    
    private void handleColor(piBotBase robot, BehaveMode mode) {
      piBotBo bot = (piBotBo)robot;

      if (mode != BehaveMode.START) {
        return;
      }

      Color color = convertColorType(Type);
      bot.cmd_rgbLights(color.r, color.g, color.b);
      
      HasFinishedUpdater = updateHasFinished_shortTime;
    }

    private void handleExpression(piBotBase robot, BehaveMode mode) {
      piBotBo bot = (piBotBo)robot;

      if (mode == BehaveMode.STOP) {
        bot.cmd_stopSingleAnim();
        bot.cmd_bodyMotionStop();
        return;
      }
      
      if (mode != BehaveMode.START) {
        return;
      }
      
      hasStartedAnim = false;
      HasFinishedUpdater = updateHasFinished_anim;
      
      uint expressionId = (uint)RunningParamValue;

      trExpression expression = trExpressions.Instance.GetExpression(expressionId);
      if (expression != null) {
        string animJson = trMoodyAnimations.Instance.getJsonForAnim(expression.filename, runningMood);
        bot.cmd_startSingleAnim(animJson);
        
//      piRobotAnimSoundManager.Instance.playRobotAnimation(bot, animName);
      }
    }

    private void handlePuppet(piBotBase robot, BehaveMode mode) {
      if (robot == null) {
        return;
      }

      piBotBo bot = (piBotBo)robot;
      
      if (mode == BehaveMode.STOP) {
        ((piBotCommon)robot).cmd_stopOnRobotAnimation();
        ((piBotBo    )robot).cmd_bodyMotionStop();
        return;
      }
      
      if (mode != BehaveMode.START) {
        return;
      }

      packetsSincePoseCommandWasIssued = 0;
      poseTimeoutMoment = Time.time + DISCRETE_MOVE_DURATION + POSE_TIMEOUT_SECONDS;
      HasFinishedUpdater = updateHasFinished_poseBased;

      uint slotIndex = (uint)RunningParamValue;
      bot.cmd_startOnRobotPuppetClip(slotIndex);
    }

    private void handleAnim(piBotBase robot, BehaveMode mode) {
      if (robot == null) {
        return;
      }
      
      if (trMultivariate.Instance!=null &&
          trMultivariate.Instance.getOptionValue(trMultivariate.trAppOption.MUTE_SOUND_AND_ANIMS) == trMultivariate.trAppOptionValue.YES) {
        HasFinishedUpdater = updateHasFinished_shortTime;
        if (wwDoOncePerTypeVal<trMultivariate.trAppOption>.doIt(trMultivariate.trAppOption.MUTE_SOUND_AND_ANIMS)) {
          WWLog.logError("MUTE: NOT PLAYING SOUNDS OR ANIMATIONS");
        }
        return;
      }
      
      piBotBo bot = (piBotBo)robot;
      
      if (mode == BehaveMode.STOP) {
        ((piBotCommon)robot).cmd_stopSingleAnim();
        ((piBotBo    )robot).cmd_bodyMotionStop();
        return;
      }
      
      if (mode != BehaveMode.START) {
        return;
      }
      
      uint animationId = (uint)RunningParamValue;
   

      hasStartedAnim = false;
      HasFinishedUpdater = updateHasFinished_anim;
      
      string animName = "";

      trMoodyAnimation animation = trMoodyAnimations.Instance.getAnimation(animationId);
      if (animation != null) {
        animName = animation.filename;      
        string animJson = trMoodyAnimations.Instance.getJsonForAnim(animName, runningMood);
        bot.cmd_startSingleAnim(animJson);
      }
      else {
        WWLog.logError("could not find animation. ID=" + animationId);
      }
    }

    private Dictionary<string, Dictionary<trMoodType, string>> AnimationDic = null;


    private void setMoodToAnimDic(string origName, trMoodType mood, string name ){
      Dictionary<trMoodType, string> newDic;
      if(!AnimationDic.ContainsKey(origName)){
        newDic = new Dictionary<trMoodType, string>();
        AnimationDic.Add(origName, newDic);
      }else{
        newDic = AnimationDic[origName];
      }

      if(newDic.ContainsKey(mood)){
        WWLog.logError("Mood " + mood + " is already set for " + origName + "->" + name);
        return;
      }
      newDic.Add(mood, name);
    }
    
    private bool hasStartedAnim = false;
    private float hasStoppedAlmost = float.NaN;
    private void updateHasFinished_anim(piBotBase robot) {
      lastPoseCommandTime = float.MinValue;
      if (!hasStartedAnim) {
        hasFinished = false;
        hasStartedAnim = (robot.NumberOfExecutingCommandSequences > 0);
        hasStoppedAlmost = float.NaN;
      }
      else {
        if (robot.NumberOfExecutingCommandSequences == 0) {
          if (float.IsNaN(hasStoppedAlmost)) {
            hasStoppedAlmost = Time.time;
          }
        }
        if (float.IsNaN(hasStoppedAlmost)) {
          hasFinished = false;
        }
        else {
          float dt = Time.time - hasStoppedAlmost;
          hasFinished = (dt > 0.1f);
        }
      }
    }
    
    private float TimeInBehavior {
      get {
        return Time.time - timeStart;
      }
    }

    private void updateHasFinished_continuousMove(piBotBase robot){
      lastPoseCommandTime = float.MinValue;
      hasFinished = (TimeInBehavior > CONTINUOUS_MOVE_DURATION);
    }
    
    private void updateHasFinished_poseBased(piBotBase robot){
      switch (Type) {
        case trBehaviorType.PUPPET:
          lastPoseCommandTime = float.MinValue;
          break;
        default:
          lastPoseCommandTime = Time.time;
          break;    
      }
      packetsSincePoseCommandWasIssued += 1;
      if (packetsSincePoseCommandWasIssued < MIN_PACKETS_TO_WAIT_FOR_POSE) {
        hasFinished = false;
      }
      else {
        piBotBo bot = (piBotBo)robot;
        hasFinished = (bot.BodyPoseSensor.IsAllDone);
        
        if (!hasFinished && Time.time > poseTimeoutMoment) {
          WWLog.logWarn("Pose timed out.");
          hasFinished = true;
        }
      }
    }

    private void updateHasFinished_headMove(piBotBase robot){
      hasFinished = (TimeInBehavior > HEAD_MOVE_AUTO_TIME);
    }
    private void updateHasFinished_launch(piBotBase robot){
      hasFinished = (TimeInBehavior > LAUNCHER_AUTO_TIME);
    }
    private void updateHasFinished_shortTime(piBotBase robot){
      hasFinished = (TimeInBehavior > SHORT_TIME_DURATION);
    }
    
    private void updateHasFinished_longTime(piBotBase robot){
      hasFinished = (TimeInBehavior > LONG_TIME_DURATION);
    }

    private void updateHasFinished_challengeGeneric(piBotBase robot){
      hasFinished = (TimeInBehavior > CHALLENGE_LAST_STATE_DURATION);
    }
    
    private void updateHasFinished_RespondToVoice(piBotBase robot) {
      if (respondToVoiceStartTime == INVALID_VOICE_START_TIME) {
        hasFinished = false;
      }
      else {
        float timeSinceFirstReponseToVoice = Time.time - respondToVoiceStartTime;
        if (timeSinceFirstReponseToVoice > DISCRETE_MOVE_AUTO_TIME)
        {
          hasFinished = true;
        }
        else {
          hasFinished = false;
        }
      }
    }

    private bool hasStartedSound = false;
    private int packetsSinceSoundStarted = 0;
    private const int MIN_PACKETS_TO_WAIT_FOR_SOUND_START = 3;
    private const int MAX_PACKETS_TO_WAIT_FOR_SOUND_START = 30;

    private void handleSound(piBotBase robot, BehaveMode mode) {
      piBotCommon bot = (piBotBo)robot;
      
      if (trMultivariate.Instance.getOptionValue(trMultivariate.trAppOption.MUTE_SOUND_AND_ANIMS) == trMultivariate.trAppOptionValue.YES) {
        HasFinishedUpdater = updateHasFinished_shortTime;
        if (wwDoOncePerTypeVal<trMultivariate.trAppOption>.doIt(trMultivariate.trAppOption.MUTE_SOUND_AND_ANIMS)) {
          WWLog.logError("MUTE: NOT PLAYING SOUNDS OR ANIMATIONS");
        }
        return;
      }
      
      if (mode != BehaveMode.START) {
        return;
      }
      
      hasStartedSound = false;
      packetsSinceSoundStarted = 0;
      HasFinishedUpdater = updateHasFinished_sound;
      
      uint soundID = 0;
      
      switch (Type) {
        case trBehaviorType.SOUND_USER_1:
          soundID = 1000;
          break;
        case trBehaviorType.SOUND_USER_2:
          soundID = 1001;
          break;
        case trBehaviorType.SOUND_USER_3:
          soundID = 1002;
          break;
        case trBehaviorType.SOUND_USER_4:
          soundID = 1003;
          break;
        case trBehaviorType.SOUND_USER_5:
          soundID = 1004;
          break;
        default:
          soundID = (uint)RunningParamValue;
          break;
      }
      
      trRobotSound sound = trRobotSounds.Instance.GetSound(soundID, robot.robotType);
      if (sound != null) {
        bot.cmd_playSound(sound.filename);
      }
    }

    private void updateHasFinished_sound(piBotBase robot) {
      packetsSinceSoundStarted++;

      //We ignore the first three packets after a sound has started. This method is called each time we receive a packet.
      if (packetsSinceSoundStarted <= MIN_PACKETS_TO_WAIT_FOR_SOUND_START) {
        return;
      }

      piBotCommon bot = (piBotCommon)robot;
      
      if (!hasStartedSound) {
        if (packetsSinceSoundStarted >= MAX_PACKETS_TO_WAIT_FOR_SOUND_START) {
          // TUR-2013. this can happen if a sound file is missing on the robot.
          WWLog.logError("Timing out waiting for sound to start.");
          hasFinished = true;
        }
        else {
          hasFinished = false;
          hasStartedSound = bot.SoundPlayingSensor.flag;
        }
      }
      else {
        hasFinished = !bot.SoundPlayingSensor.flag;
      }
    }

    
    private const float INVALID_VOICE_START_TIME = -1f;
    private float respondToVoiceStartTime = INVALID_VOICE_START_TIME;
    private void turnToVoiceDelegate(piBotBase robot) {
      piBotBo bot = (piBotBo)robot;

      if (!this.Active) {
        bot.OnState -= turnToVoiceDelegate;
        return;
      }
      PI.WWPoseMode discmode = PI.WWPoseMode.WW_POSE_MODE_RELATIVE_MEASURED;
      PI.WWPoseDirection discdir  = PI.WWPoseDirection.WW_POSE_DIRECTION_INFERRED;
      PI.WWPoseWrap discwrap = PI.WWPoseWrap.WW_POSE_WRAP_ON;
      
      float direction = bot.SoundSensor.direction;
      if (float.IsNaN(direction)) {
        return;
      }
//      TODO: Lets revisit Caching in v1.5. For now are ignoring it
//      if (float.IsNaN(direction)) {
//        if (bot.WasVoiceHeardRecently()) {
//          direction = bot.LastVoiceHeardSensorDirection;
//        }
//        else {
//          return;
//        }
//      }
      //bot.ResetVoiceSensor();

      respondToVoiceStartTime = Time.time;
      bot.cmd_poseParam(0, 0, direction, DISCRETE_MOVE_DURATION, discmode, discdir, discwrap);
    }

    private void handleTurnToVoice(piBotBase robot, BehaveMode mode) {
      
      if (mode != BehaveMode.START) {
        return;
      }

      piBotBo bot = (piBotBo)robot;
      HasFinishedUpdater = updateHasFinished_RespondToVoice;
      respondToVoiceStartTime = INVALID_VOICE_START_TIME;
      turnToVoiceDelegate(robot);
      bot.OnState += turnToVoiceDelegate;
      
    }

    private void lookToVoiceDelegate(piBotBase robot) {
      piBotBo bot = (piBotBo)robot;
      
      if (!this.Active) {
        bot.OnState -= lookToVoiceDelegate;
        return;
      }

      float direction = bot.SoundSensor.direction;

      if (float.IsNaN(direction)) {
        return;
      }
      //      TODO: Lets revisit Caching in v1.5. For now are ignoring it
      //      if (float.IsNaN(direction)) {
      //        if (bot.WasVoiceHeardRecently()) {
      //          direction = bot.LastVoiceHeardSensorDirection;
      //        }
      //        else {
      //          return;
      //        }
      //      }
      //bot.ResetVoiceSensor();
      
      respondToVoiceStartTime = Time.time; 
      direction = direction * 180f/Mathf.PI;
      bot.cmd_headPan(direction);
    }
  
    private void handlePanToVoice(piBotBase robot, BehaveMode mode) {
      piBotBo bot = (piBotBo)robot;
      if (mode != BehaveMode.START) {
        return;
      }
      HasFinishedUpdater = updateHasFinished_RespondToVoice;
      respondToVoiceStartTime = INVALID_VOICE_START_TIME;
      lookToVoiceDelegate(robot);
      bot.OnState += lookToVoiceDelegate;

    }
    
    private int packetsSincePoseCommandWasIssued = 0;
    private float poseTimeoutMoment = 0;
    private const int MIN_PACKETS_TO_WAIT_FOR_POSE = 13; // sparse packets seem to come every 10.
    private const float POSE_TIMEOUT_SECONDS = 3f;
    private const float POSE_COMMAND_TIMEOUT = 5f;
    private static float lastPoseCommandTime = float.MinValue;
    
    private void handleMove(piBotBase robot, BehaveMode mode) {
      piBotBo bot = (piBotBo)robot;
      
      if (mode != BehaveMode.START) {
        return;
      }

      if(trDataManager.Instance.IsRCFreeplayInUse){
        HasFinishedUpdater = updateHasFinished_shortTime;
        return;
      }
      
      packetsSincePoseCommandWasIssued = 0;
      poseTimeoutMoment = Time.time + DISCRETE_MOVE_DURATION + POSE_TIMEOUT_SECONDS;
      
      HasFinishedUpdater = updateHasFinished_continuousMove;

//      double linearAcc = 50;
//      double angularAcc = 10;
      double radius = 600;

      PI.WWPoseMode discmode;
      if (Time.time - lastPoseCommandTime >= POSE_COMMAND_TIMEOUT) {
        discmode = PI.WWPoseMode.WW_POSE_MODE_RELATIVE_MEASURED;
      }
      else {
        discmode = PI.WWPoseMode.WW_POSE_MODE_RELATIVE_COMMAND;
      }
      PI.WWPoseDirection discdir  = PI.WWPoseDirection.WW_POSE_DIRECTION_INFERRED;
      PI.WWPoseWrap discwrap = PI.WWPoseWrap.WW_POSE_WRAP_ON;
      
      switch (Type) {
      default:
        WWLog.logError("Unhandled handleMove type: " + Type.ToString());
        break;
      case trBehaviorType.MOVE_FB0:
        bot.cmd_bodyMotion(0, bot.prevAngular);
        break;
      case trBehaviorType.MOVE_F1:
        bot.cmd_bodyMotion(LINSPD_1, bot.prevAngular);
        break;
      case trBehaviorType.MOVE_F2:
        bot.cmd_bodyMotion(LINSPD_2, bot.prevAngular);
        break;
      case trBehaviorType.MOVE_F3:
        bot.cmd_bodyMotion(LINSPD_3, bot.prevAngular);
        break;
      case trBehaviorType.MOVE_B1:
        bot.cmd_bodyMotion(-LINSPD_1, bot.prevAngular);
        break;
      case trBehaviorType.MOVE_B2:
        bot.cmd_bodyMotion(-LINSPD_2, bot.prevAngular);
        break;
      case trBehaviorType.MOVE_B3:
        bot.cmd_bodyMotion(-LINSPD_3, bot.prevAngular);
        break;
      case trBehaviorType.MOVE_LR0:
        bot.cmd_bodyMotion(bot.prevLinear, 0);
        break;
      case trBehaviorType.MOVE_L1:
        bot.cmd_bodyMotion(bot.prevLinear, ANGSPD_1);
        break;
      case trBehaviorType.MOVE_L2:
        bot.cmd_bodyMotion(bot.prevLinear, ANGSPD_2);
        break;
      case trBehaviorType.MOVE_L3:
        bot.cmd_bodyMotion(bot.prevLinear, ANGSPD_3);
        break;
      case trBehaviorType.MOVE_R1:
        bot.cmd_bodyMotion(bot.prevLinear, -ANGSPD_1);
        break;
      case trBehaviorType.MOVE_R2:
        bot.cmd_bodyMotion(bot.prevLinear, -ANGSPD_2);
        break;
      case trBehaviorType.MOVE_R3:
        bot.cmd_bodyMotion(bot.prevLinear, -ANGSPD_3);
        break;
        
      case trBehaviorType.MOVE_STOP:
        HasFinishedUpdater = updateHasFinished_shortTime;
        bot.cmd_bodyMotionStop();
        break;

      case trBehaviorType.MOVE_CONT_STRAIGHT:
        HasFinishedUpdater = updateHasFinished_continuousMove;
        lastPoseCommandTime = float.MinValue;
        bot.cmd_bodyMotion(RunningParamValue, 0, trMultivariate.isYES(trMultivariate.trAppOption.USE_POSE_BASED_LINANG));
        break;
      case trBehaviorType.MOVE_CONT_CIRCLE_CCW:
        HasFinishedUpdater = updateHasFinished_continuousMove;
        lastPoseCommandTime = float.MinValue;
        bot.cmd_bodyMotion(RunningParamValue, RunningParamValue * 360 / (Mathf.PI * 2 * radius), true);
        break;
      case trBehaviorType.MOVE_CONT_CIRCLE_CW:
        HasFinishedUpdater = updateHasFinished_continuousMove;
        lastPoseCommandTime = float.MinValue;
        bot.cmd_bodyMotion(RunningParamValue, -RunningParamValue * 360 / (Mathf.PI * 2 * radius), true);
        break;
      case trBehaviorType.MOVE_CONT_SPIN:
        HasFinishedUpdater = updateHasFinished_continuousMove;
        lastPoseCommandTime = float.MinValue;
        float linearVelocity;
        float angularVelocity;
        piMathUtil.wheelSpeedsToLinearAngular(runningParamValues[0],
                                              runningParamValues[1],
                                              out linearVelocity,
                                              out angularVelocity,
                                              PI.piBotConstants.axleLength);

        bot.cmd_bodyMotion(linearVelocity, angularVelocity, trMultivariate.isYES(trMultivariate.trAppOption.USE_POSE_BASED_LINANG));
        break;
      case trBehaviorType.MOVE_DISC_STRAIGHT:
        HasFinishedUpdater = updateHasFinished_poseBased;
        bot.cmd_poseParam(RunningParamValue , 0, 0, DISCRETE_MOVE_DURATION, discmode, discdir, discwrap);
        break;
      case trBehaviorType.MOVE_DISC_TURN:
        HasFinishedUpdater = updateHasFinished_poseBased;
        bot.cmd_poseParam(0, 0, RunningParamValue * Mathf.PI/ 180f, DISCRETE_MOVE_DURATION, discmode, discdir, discwrap);
        break;
      }
    }
    
    private void handleHead(piBotBase robot, BehaveMode mode) {
      piBotBo bot = (piBotBo)robot;
      
      if (mode != BehaveMode.START) {
        return;
      }

      switch(Type){
      default:
        WWLog.logError("Unhandled handleHead type: " + Type.ToString());
        break;
      case trBehaviorType.HEAD_PAN:
        bot.cmd_headPan(RunningParamValue);
        break;
      case trBehaviorType.HEAD_TILT:
        bot.cmd_headTilt(RunningParamValue);
        break;
      }
      HasFinishedUpdater = updateHasFinished_headMove;
    }
    
    private void handleEyeRing (piBotBase robot, BehaveMode mode) {
      piBotBo bot = (piBotBo)robot;
      
      if (mode != BehaveMode.START) {
        return;
      }
      
      HasFinishedUpdater = updateHasFinished_shortTime;
      
      switch(Type){
        case trBehaviorType.EYERING:
          bool[] bitmap = piMathUtil.deserializeBoolArray((int)RunningParamValue);
          bot.cmd_eyeRing(1.0f, "", bitmap);
          break;
      };
    }
    
    private void handleMapSet(piBotBase robot, BehaveMode mode) {
      if (mode == BehaveMode.STOP) {
        return;
      }
      
      HasFinishedUpdater = updateHasFinished_longTime;
      
      if (this.MapSet == null) {
        WWLog.logError("the case of the missing mapSet. " + this.UUID);
        return;
      }
      
      MapSet.onRobotState(robot);
    }
    
    private void handleLaunch(piBotBase robot, BehaveMode mode) {
      piBotBo bot = (piBotBo)robot;
      
      if (mode != BehaveMode.START) {
        return;
      }

      switch(Type){
        case trBehaviorType.LAUNCH_RELOAD_LEFT:
          bot.cmd_launcher_reload_left();
          break;
        case trBehaviorType.LAUNCH_RELOAD_RIGHT:
          bot.cmd_launcher_reload_right();
          break;
        case trBehaviorType.LAUNCH_FLING:
          bot.cmd_launcher_fling(runningParamValues[0]); // max strength for now
          break;
        default:
          WWLog.logError("unknown fling type: " + Type);
          break;
      }
      HasFinishedUpdater = updateHasFinished_launch;  
    }
    
    private void handleRunSpark(piBotBase robot, BehaveMode mode) {
      if (mode != BehaveMode.START) {
        return;
      }
      
      HasFinishedUpdater = updateHasFinished_shortTime;
      
      WWLog.logInfo("todo: implement " + trBehaviorType.RUN_SPARK.ToString());
    }
  }
}




