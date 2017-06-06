using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using Turing;
using System;

public class trIconFactory {
  
  static private Dictionary<trTriggerType , string> iconsTrigger      = null;
  static private Dictionary<trActuatorType, string> iconsActuator     = null;
  static private Dictionary<trBehaviorType, string> iconsBehavior     = null;
  static private Dictionary<trSensorType  , string> iconsSensor       = null;
  static private Dictionary<string        , string> iconsUserBehavior = null;
  static private Dictionary<string        , string> iconsChallenge    = null;
  static private Dictionary<trMoodType    , string> iconsMood         = null;

  static private Dictionary<string        , Sprite> spriteDic         =  new Dictionary<string, Sprite>();
  
  static public string[] UserIconNames = new string[]{
    "birthday",
    "bright_idea",
    "Bro_Jacob",
    "building",
    "car",
    "cat",
    "chicken",
    "clock",
    "Da_Bridge",
    "flower",
    "heart",
    "itsy_Bitsy",
    "Little_Lamb",
    "Little_Star",
    "moon",
    "Reward_1_icon",
    "Reward_2_icon",
    "Reward_3_icon",
    "Reward_4_icon",
    "Reward_5_icon",
    "reward_default_icon"
  };

  static string[] eliImages = new string[]{"ui_eli_0", "ui_eli_1", "ui_eli_2", "ui_eli_3"};
  static public bool IsEliImage(string name){
    for(int i = 0; i< eliImages.Length; ++i){
      if(name == eliImages[i]){
        return true;
      }
    }
    return false;
  }
  
  static public string[] ChallengeIconNames = new string[]{
    "SkatingRink",
    "PlanetX",
    "DrivingSchool",
    "DogShow",
    "DanceOff",
    "Creamery",
    "ui_eli_0",
    "ui_eli_1",
    "ui_eli_2",
    "ui_eli_3",
    "big_dash",
    "big_dot",
    "WW",
    "Lagoon",
    "Jungle",
    "Arctic Snow",
    "Mountain",
    "Volcano",
    "Grassland",
    "City",
    "Desert",
    "space",
    "DarkForest",
    "scene_1_Heeyorg",
    "scene_2_Hi",
    "scene_3_Araby_Hi"

  };

//  static public void Load(){
//    createMoodIconDic();
//    createSensorIconDic();
//    createTriggerIconDic();
//    createUserBehaviorIconDic();
//    createChallengeIconDic();
//    createBehaviorIconDic();
//    createActuatorIconDic();
//
//    loadSprites(iconsMood);
//    loadSprites(iconsSensor);
//    loadSprites(iconsUserBehavior);
//    loadSprites(iconsTrigger);
//    loadSprites(iconsActuator);
//    loadSprites(iconsSensor);
//    loadSprites(iconsChallenge);
//  }

  static private void loadSprites<T>(Dictionary<T, string> dic){
    foreach(string path in dic.Values){
      Sprite sprite = Resources.Load<Sprite>(path);
      if(spriteDic.ContainsKey(path)){
        continue;
      }
      spriteDic.Add(path, sprite);
      if (sprite == null) {
        WWLog.logError("could not find sprite for " + path);
      }
    }
  }
  
  static private Sprite getSprite<T>(Dictionary<T, string> dict, T value) {
    if (dict == null) {
      return null;
    }
    
    string path;
    
    if (!dict.ContainsKey(value)) {
      if (wwDoOncePerTypeVal<T>.doIt(value)) {
        WWLog.logError("no icon for " + typeof(T).Name + "." + value.ToString() + " - using unknown.");
      }
      path = "Sprites/Triggers/eventUnknown";
    }
    else {
      path = dict[value];
    }
    
    if(spriteDic.ContainsKey(path)){
      return spriteDic[path];
    }
    else{
      Sprite ret = Resources.Load<Sprite>(path);
      spriteDic.Add(path, ret);
      if (ret == null) {
        WWLog.logError("could not find sprite for " + path);
      }
      
      return ret;
    }
   
  }

  static public Sprite GetIcon(trState state){
    if(state != null){
      if (state.Behavior.Type == trBehaviorType.MOODY_ANIMATION) {
        return GetAnimationIcon(state);
      }
      else {
        return GetIcon(state.Behavior);       
      }
    }
    WWLog.logError("Cannot find icon for state behavior.");
    return null;
  }

  static public Sprite GetIcon(trBehavior behavior){
    if (behavior != null){
      if(behavior.Type == trBehaviorType.MAPSET){
        return GetIcon(behavior.MapSet.IconName);
      }
      else if(behavior.Type == trBehaviorType.MISSION){
        if(behavior.MissionFileInfo != null){
          return GetMissionTypeIcon(behavior.MissionFileInfo.Type);
        }       
      }
      else{
        return GetIcon(behavior.Type);
      }
    }
    WWLog.logError("Cannot find icon for behavior.");
    return null;
  }
  
  static public Sprite GetAnimationIcon(trState state) {
    uint animationId = (uint)state.BehaviorParameterValue;
    trMoodyAnimation animation = trMoodyAnimations.Instance.getAnimation(animationId);    
    return trMoodyAnimations.Instance.GetIcon(animation);
  }

  static public Sprite GetMissionTypeIcon(trMissionType type){
    string name = "";
    switch(type){
      case trMissionType.DASH:
        name = "big_dash";
        break;
      case trMissionType.DOT:
        name = "big_dot";
        break;
    }
    return GetMissionIcon(name);

  }

  static public Sprite GetMissionIcon(string value){
    return getSprite(ChallengeIconDic, value);
  }
  
  static public Sprite GetIcon(string value){
    return getSprite(UserBehaviorIconDic, value);
  }
  
  static public Sprite GetIcon(trSensorType value) {
    return getSprite(SensorIconDic, value);
  }
  
  static public Sprite GetIcon(trActuatorType value) {
    return getSprite(ActuatorIconDic, value);
  }
  
  static public Sprite GetIcon(trTriggerType value) {
    return getSprite(TriggerIconDic, value);
  }

  static public Sprite GetStartStateIcon(piRobotType robotType) {
    string startStatePathPrefix = BehaviorIconDic[trBehaviorType.START_STATE];
    string path = startStatePathPrefix;
    switch (robotType) {
      case piRobotType.DASH:
        path = path + "_DASH";
        break;
      case piRobotType.DOT:
        path = path + "_DOT";
        break;
    }
    Sprite ret = Resources.Load<Sprite>(path);
    return ret;
  }

  static public Sprite GetIcon(trBehaviorType value) {
    if(value.IsMood()){
      trMoodType mood = value.ToMood();
      return GetIcon(mood);
    }
    
    return getSprite(BehaviorIconDic, value);
  }

  static public Sprite GetIcon(trMoodType mood){
    return getSprite(MoodIconDic, mood);
  }

  static private Dictionary<string, string> UserBehaviorIconDic {
    get {
      if(iconsUserBehavior == null){
        iconsUserBehavior = new Dictionary<string, string>();
        string prefix = "Sprites/Icons/";
        for(int i = 0; i< UserIconNames.Length; ++i){
          string name = UserIconNames[i];
          iconsUserBehavior[name] = prefix + name;
        }
      }
      return iconsUserBehavior;
    }
  }

  
  static private Dictionary<trSensorType, string> SensorIconDic {
    get {
      if(iconsSensor == null){
        iconsSensor = new Dictionary<trSensorType, string>();
        
        string prefix = "Sprites/Sensors/";
        iconsSensor[trSensorType.TRAVEL_ANGULAR             ] = prefix + "TravelAng";
        iconsSensor[trSensorType.TRAVEL_ANGULAR_360         ] = prefix + "TravelAng3";
        iconsSensor[trSensorType.TRAVEL_ANGULAR_180         ] = prefix + "TravelAng2";
        iconsSensor[trSensorType.TRAVEL_ANGULAR_90          ] = prefix + "TravelAng1";
        
        iconsSensor[trSensorType.TRAVEL_LINEAR              ] = prefix + "TravelLin";
        iconsSensor[trSensorType.TRAVEL_LINEAR_100          ] = prefix + "TravelLin3";
        iconsSensor[trSensorType.TRAVEL_LINEAR_50           ] = prefix + "TravelLin2";
        iconsSensor[trSensorType.TRAVEL_LINEAR_10           ] = prefix + "TravelLin1";
        
        iconsSensor[trSensorType.TIME_IN_STATE              ] = prefix + "Time";
        iconsSensor[trSensorType.TIME_IN_STATE_1            ] = prefix + "Time1";
        iconsSensor[trSensorType.TIME_IN_STATE_2            ] = prefix + "Time2";
        iconsSensor[trSensorType.TIME_IN_STATE_5            ] = prefix + "Time5";
        iconsSensor[trSensorType.TIME_IN_STATE_10           ] = prefix + "Time10";
        
        iconsSensor[trSensorType.HEAD_PAN                   ] = prefix + "HeadPan";
        iconsSensor[trSensorType.HEAD_TILT                  ] = prefix + "HeadTilt";
        
        iconsSensor[trSensorType.ROLL                       ] = prefix + "Roll";
        iconsSensor[trSensorType.PITCH                      ] = prefix + "Pitch";
        
        iconsSensor[trSensorType.DISTANCE_FRONT             ] = prefix + "DistanceFront";
        iconsSensor[trSensorType.DISTANCE_FRONT_LEFT_FACING ] = prefix + "DistanceLeftFacing";
        iconsSensor[trSensorType.DISTANCE_FRONT_RIGHT_FACING] = prefix + "DistanceRightFacing";
        iconsSensor[trSensorType.DISTANCE_REAR              ] = prefix + "DistanceRear";
        iconsSensor[trSensorType.DISTANCE_FRONT_DELTA       ] = prefix + "DistanceFrontDiff";
        
        iconsSensor[trSensorType.RANDOM_NOISE               ] = prefix + "Random";
      }
      
      return iconsSensor;
    }
  }
  
  static private Dictionary<trActuatorType, string> ActuatorIconDic {
    get {
      if(iconsActuator == null){
        iconsActuator = new Dictionary<trActuatorType, string>();
        
        string prefix = "Sprites/Actuators/";
        iconsActuator[trActuatorType.WHEEL_L           ] = prefix + "WheelL";
        iconsActuator[trActuatorType.WHEEL_R           ] = prefix + "WheelR";
        iconsActuator[trActuatorType.HEAD_PAN          ] = prefix + "HeadPan";
        iconsActuator[trActuatorType.HEAD_TILT         ] = prefix + "HeadTilt";
        iconsActuator[trActuatorType.RGB_ALL_HUE       ] = prefix + "RGBHue";
        iconsActuator[trActuatorType.RGB_ALL_VAL       ] = prefix + "RGBVal";
        iconsActuator[trActuatorType.LED_TOP           ] = prefix + "LEDMain";
        iconsActuator[trActuatorType.LED_TAIL          ] = prefix + "LEDTail";
        iconsActuator[trActuatorType.EYERING           ] = prefix + "LEDMain";      
      }
      
      return iconsActuator;
    }
  }
  
  static private Dictionary<string, string> ChallengeIconDic {
    get {
      if(iconsChallenge == null){
        iconsChallenge = new Dictionary<string, string>();
        string prefix = "Sprites/Challenge Icons/";
        for(int i = 0; i< ChallengeIconNames.Length; ++i){
          string name = ChallengeIconNames[i];
          iconsChallenge[name] = prefix + name;
        }
      }
      
      return iconsChallenge;
    }
  }
  
  static private Dictionary<trTriggerType, string> TriggerIconDic {
    get {
      if(iconsTrigger == null){
        iconsTrigger = new Dictionary<trTriggerType, string>();
        
        string prefix = "Sprites/Triggers/";
        
        iconsTrigger[trTriggerType.BUTTON_MAIN           ] = prefix + "eventButtonMain";
        iconsTrigger[trTriggerType.BUTTON_1              ] = prefix + "eventButton1";
        iconsTrigger[trTriggerType.BUTTON_2              ] = prefix + "eventButton2";
        iconsTrigger[trTriggerType.BUTTON_3              ] = prefix + "eventButton3";
        iconsTrigger[trTriggerType.BUTTON_ANY            ] = prefix + trTriggerType.BUTTON_ANY     .ToString();
        iconsTrigger[trTriggerType.BUTTON_MAIN_NOT       ] = prefix + trTriggerType.BUTTON_MAIN_NOT.ToString();
        iconsTrigger[trTriggerType.BUTTON_1_NOT          ] = prefix + trTriggerType.BUTTON_1_NOT   .ToString();
        iconsTrigger[trTriggerType.BUTTON_2_NOT          ] = prefix + trTriggerType.BUTTON_2_NOT   .ToString();
        iconsTrigger[trTriggerType.BUTTON_3_NOT          ] = prefix + trTriggerType.BUTTON_3_NOT   .ToString();
        iconsTrigger[trTriggerType.BUTTON_NONE           ] = prefix + trTriggerType.BUTTON_NONE    .ToString();
        
        iconsTrigger[trTriggerType.DISTANCE_SET          ] = prefix + trTriggerType.DISTANCE_SET   .ToString();
        iconsTrigger[trTriggerType.BEACON_SET            ] = prefix + trTriggerType.BEACON_SET     .ToString();
        iconsTrigger[trTriggerType.BEACON_V2             ] = prefix + trTriggerType.BEACON_V2      .ToString();
        
        
        iconsTrigger[trTriggerType.DISTANCE_REAR_NEAR    ] = prefix + "eventObjectBehind_near";
        iconsTrigger[trTriggerType.DISTANCE_REAR_MIDDLE  ] = prefix + "eventObjectBehind_middle";
        iconsTrigger[trTriggerType.DISTANCE_REAR_FAR     ] = prefix + "eventObjectBehind_far";
        iconsTrigger[trTriggerType.DISTANCE_REAR_NONE    ] = prefix + "eventObjectBehind_absent";
        iconsTrigger[trTriggerType.DISTANCE_LEFT_NEAR    ] = prefix + "eventObjectFrontLeft_near";
        iconsTrigger[trTriggerType.DISTANCE_LEFT_MIDDLE  ] = prefix + "eventObjectFrontLeft_middle";
        iconsTrigger[trTriggerType.DISTANCE_LEFT_FAR     ] = prefix + "eventObjectFrontLeft_far";
        iconsTrigger[trTriggerType.DISTANCE_LEFT_NONE    ] = prefix + "eventObjectFrontLeft_absent";
        iconsTrigger[trTriggerType.DISTANCE_RIGHT_NEAR   ] = prefix + "eventObjectFrontRight_near";
        iconsTrigger[trTriggerType.DISTANCE_RIGHT_MIDDLE ] = prefix + "eventObjectFrontRight_middle";
        iconsTrigger[trTriggerType.DISTANCE_RIGHT_FAR    ] = prefix + "eventObjectFrontRight_far";
        iconsTrigger[trTriggerType.DISTANCE_RIGHT_NONE   ] = prefix + "eventObjectFrontRight_absent";
        iconsTrigger[trTriggerType.DISTANCE_CENTER_NEAR  ] = prefix + "eventObjectFront_near";
        iconsTrigger[trTriggerType.DISTANCE_CENTER_MIDDLE] = prefix + "eventObjectFront_middle";
        iconsTrigger[trTriggerType.DISTANCE_CENTER_FAR   ] = prefix + "eventObjectFront_far";
        iconsTrigger[trTriggerType.DISTANCE_CENTER_NONE  ] = prefix + "eventObjectFront_absent";
        iconsTrigger[trTriggerType.TIME                  ] = prefix + "time";
        iconsTrigger[trTriggerType.TIME_LONG             ] = prefix + "time_long";
        iconsTrigger[trTriggerType.IMMEDIATE             ] = prefix + "clock_0s";
        iconsTrigger[trTriggerType.TIME_1                ] = prefix + "clock_1s";
        iconsTrigger[trTriggerType.TIME_3                ] = prefix + "clock_3s";
        iconsTrigger[trTriggerType.TIME_5                ] = prefix + "clock_5s";
        iconsTrigger[trTriggerType.TIME_10               ] = prefix + "clock_10s";
        iconsTrigger[trTriggerType.BEHAVIOR_FINISHED     ] = prefix + "clock_auto";
        iconsTrigger[trTriggerType.CLAP                  ] = prefix + "eventSoundClapDash";
        iconsTrigger[trTriggerType.VOICE                 ] = prefix + "eventSoundVoice";
        iconsTrigger[trTriggerType.KIDNAP                ] = prefix + "eventKidnap";
        iconsTrigger[trTriggerType.KIDNAP_NOT            ] = prefix + "eventKidnapNOT";
        iconsTrigger[trTriggerType.STALL                 ] = prefix + "eventStuck";
        iconsTrigger[trTriggerType.STALL_NOT             ] = prefix + "eventStuckNOT";
        iconsTrigger[trTriggerType.TRAVEL_LINEAR         ] = prefix + "TravelLin";
        iconsTrigger[trTriggerType.TRAVEL_ANGULAR        ] = prefix + "TravelAng";
        iconsTrigger[trTriggerType.BEACON_BOTH           ] = prefix + "BEACON_ANY";
        iconsTrigger[trTriggerType.BEACON_NONE           ] = prefix + "BEACON_NONE";
        iconsTrigger[trTriggerType.BEACON_LEFT           ] = prefix + "BEACON_LEFT";
        iconsTrigger[trTriggerType.BEACON_RIGHT          ] = prefix + "BEACON_RIGHT";
        iconsTrigger[trTriggerType.TRAVELING_FORWARD     ] = prefix + trTriggerType.TRAVELING_FORWARD .ToString();
        iconsTrigger[trTriggerType.TRAVELING_BACKWARD    ] = prefix + trTriggerType.TRAVELING_BACKWARD.ToString();
        iconsTrigger[trTriggerType.TRAVELING_STOPPED     ] = prefix + trTriggerType.TRAVELING_STOPPED .ToString();
        iconsTrigger[trTriggerType.LEAN_UPSIDE_UP        ] = prefix + trTriggerType.LEAN_UPSIDE_UP    .ToString();
        iconsTrigger[trTriggerType.LEAN_UPSIDE_DOWN      ] = prefix + trTriggerType.LEAN_UPSIDE_DOWN  .ToString();
        iconsTrigger[trTriggerType.LEAN_BACKWARD         ] = prefix + trTriggerType.LEAN_BACKWARD     .ToString();
        iconsTrigger[trTriggerType.LEAN_FORWARD          ] = prefix + trTriggerType.LEAN_FORWARD      .ToString();
        iconsTrigger[trTriggerType.LEAN_LEFT             ] = prefix + trTriggerType.LEAN_LEFT         .ToString();
        iconsTrigger[trTriggerType.LEAN_RIGHT            ] = prefix + trTriggerType.LEAN_RIGHT        .ToString();
        iconsTrigger[trTriggerType.DROP                  ] = prefix + trTriggerType.DROP              .ToString();
        iconsTrigger[trTriggerType.SHAKE                 ] = prefix + trTriggerType.SHAKE             .ToString();
        iconsTrigger[trTriggerType.SLIDE_X_POS           ] = prefix + trTriggerType.SLIDE_X_POS       .ToString();
        iconsTrigger[trTriggerType.SLIDE_X_NEG           ] = prefix + trTriggerType.SLIDE_X_NEG       .ToString();
        iconsTrigger[trTriggerType.SLIDE_Y_POS           ] = prefix + trTriggerType.SLIDE_Y_POS       .ToString();
        iconsTrigger[trTriggerType.SLIDE_Y_NEG           ] = prefix + trTriggerType.SLIDE_Y_NEG       .ToString();
        iconsTrigger[trTriggerType.SLIDE_Z_POS           ] = prefix + trTriggerType.SLIDE_Z_POS       .ToString();
        iconsTrigger[trTriggerType.SLIDE_Z_NEG           ] = prefix + trTriggerType.SLIDE_Z_NEG       .ToString();
        iconsTrigger[trTriggerType.TIME_RANDOM           ] = prefix + trTriggerType.TIME_RANDOM       .ToString();
        iconsTrigger[trTriggerType.RANDOM                ] = prefix + trTriggerType.RANDOM            .ToString();
        
        iconsTrigger[trTriggerType.NONE                  ] = "Sprites/Triggers/eventUnknown";
      }
      
      return iconsTrigger;
    }
  }
  
  static private Dictionary<trMoodType, string> MoodIconDic {
    get {
      if(iconsMood == null){
        iconsMood = new Dictionary<trMoodType, string>();
        string prefix = "Sprites/Moods/";
        //iconsMood[trMoodType.BRAVE     ] = prefix + "ui_spark_mood_brave";
        iconsMood[trMoodType.HAPPY     ] = prefix + "ui_spark_mood_happy";
        //iconsMood[trMoodType.SURPRISED ] = prefix + "ui_spark_mood_surprised";
        iconsMood[trMoodType.CAUTIOUS  ] = prefix + "ui_spark_mood_cautious";
        iconsMood[trMoodType.CURIOUS   ] = prefix + "ui_spark_mood_curious";
        iconsMood[trMoodType.FRUSTRATED] = prefix + "ui_spark_mood_frustrated";
        iconsMood[trMoodType.SILLY     ] = prefix + "ui_spark_mood_silly";
      }
      
      return iconsMood;
    }
  }
  
  static private Dictionary<trBehaviorType, string> BehaviorIconDic {
    get {
      if(iconsBehavior == null){
        iconsBehavior = new Dictionary<trBehaviorType, string>();
        
        string prefix = "Sprites/Behaviors/";
        
        iconsBehavior[trBehaviorType.OMNI                           ] = prefix + "OMNI";
        iconsBehavior[trBehaviorType.DO_NOTHING                     ] = prefix + "DO_NOTHING";
        iconsBehavior[trBehaviorType.START_STATE                    ] = prefix + "START_STATE";
        iconsBehavior[trBehaviorType.COLOR_OFF                      ] = prefix + "COLOR_OFF";
        iconsBehavior[trBehaviorType.COLOR_RED                      ] = prefix + "COLOR_RED";
        iconsBehavior[trBehaviorType.COLOR_ORANGE                   ] = prefix + "COLOR_ORANGE";
        iconsBehavior[trBehaviorType.COLOR_YELLOW                   ] = prefix + "COLOR_YELLOW";
        iconsBehavior[trBehaviorType.COLOR_GREEN                    ] = prefix + "COLOR_GREEN";
        iconsBehavior[trBehaviorType.COLOR_CYAN                     ] = prefix + "COLOR_CYAN";
        iconsBehavior[trBehaviorType.COLOR_BLUE                     ] = prefix + "COLOR_BLUE";
        iconsBehavior[trBehaviorType.COLOR_MAGENTA                  ] = prefix + "COLOR_MAGENTA";
        iconsBehavior[trBehaviorType.COLOR_WHITE                    ] = prefix + "COLOR_WHITE";
        iconsBehavior[trBehaviorType.ANIM_TROPHY                    ] = prefix + "ANIM_TROPHY";
        iconsBehavior[trBehaviorType.ANIM_TORNADO                   ] = prefix + "ANIM_TORNADO";
        iconsBehavior[trBehaviorType.ANIM_OIL                       ] = prefix + "ANIM_OIL";
        iconsBehavior[trBehaviorType.ANIM_TRUMPET                   ] = prefix + "ANIM_TRUMPET";
        iconsBehavior[trBehaviorType.ANIM_DELIVERY                  ] = prefix + "ANIM_DELIVERY";
        iconsBehavior[trBehaviorType.ANIM_CELEBRATION               ] = prefix + "ANIM_CELEBRATE";
        iconsBehavior[trBehaviorType.ANIM_DANCE_BUTTWIGGLE          ] = prefix + "ANIM_DANCE";
        iconsBehavior[trBehaviorType.ANIM_DANCE_SPINLEFT            ] = prefix + "ANIM_DANCE";
        iconsBehavior[trBehaviorType.ANIM_DANCE_SPINRIGHT           ] = prefix + "ANIM_DANCE";
        iconsBehavior[trBehaviorType.ANIM_DANCE_STAYALIVELEFT       ] = prefix + "ANIM_DANCE";
        iconsBehavior[trBehaviorType.ANIM_DANCE_STAYALIVERIGHT      ] = prefix + "ANIM_DANCE";
        iconsBehavior[trBehaviorType.ANIM_DANCE_TWIST               ] = prefix + "ANIM_DANCE_TWIST";
        iconsBehavior[trBehaviorType.ANIM_FORWARD_CYCLE_CAUTIOUS    ] = prefix + "MOVE_F_CYCLE_CAUTIOUS";
        iconsBehavior[trBehaviorType.ANIM_FORWARD_CYCLE_CURIOUS     ] = prefix + "MOVE_F_CYCLE_CURIOUS";
        iconsBehavior[trBehaviorType.ANIM_FORWARD_CYCLE_FRUSTRATED  ] = prefix + "MOVE_F_CYCLE_FRUSTRATED";
        iconsBehavior[trBehaviorType.ANIM_FORWARD_CYCLE_HAPPY       ] = prefix + "MOVE_F_CYCLE_HAPPY";
        iconsBehavior[trBehaviorType.ANIM_FORWARD_CYCLE_SILLY       ] = prefix + "MOVE_F_CYCLE_SILLY";
        iconsBehavior[trBehaviorType.SOUND_USER_1                   ] = prefix + "SOUND_USER_1";
        iconsBehavior[trBehaviorType.SOUND_USER_2                   ] = prefix + "SOUND_USER_2";
        iconsBehavior[trBehaviorType.SOUND_USER_3                   ] = prefix + "SOUND_USER_3";
        iconsBehavior[trBehaviorType.SOUND_USER_4                   ] = prefix + "SOUND_USER_4";
        iconsBehavior[trBehaviorType.SOUND_USER_5                   ] = prefix + "SOUND_USER_5";
        iconsBehavior[trBehaviorType.SOUND_USER                     ] = prefix + "SOUND_USER_SPARK";
        iconsBehavior[trBehaviorType.SOUND_ANIMAL                   ] = prefix + trBehaviorType.SOUND_ANIMAL.ToString();
        iconsBehavior[trBehaviorType.SOUND_VOCAL_BRAVE              ] = prefix + trBehaviorType.SOUND_VOCAL_BRAVE.ToString();
        iconsBehavior[trBehaviorType.SOUND_VOCAL_CAUTIOUS           ] = prefix + trBehaviorType.SOUND_VOCAL_CAUTIOUS.ToString();
        iconsBehavior[trBehaviorType.SOUND_VOCAL_CURIOUS            ] = prefix + trBehaviorType.SOUND_VOCAL_CURIOUS.ToString();
        iconsBehavior[trBehaviorType.SOUND_VOCAL_FRUSTRATED         ] = prefix + trBehaviorType.SOUND_VOCAL_FRUSTRATED.ToString();
        iconsBehavior[trBehaviorType.SOUND_VOCAL_HAPPY              ] = prefix + trBehaviorType.SOUND_VOCAL_HAPPY.ToString();
        iconsBehavior[trBehaviorType.SOUND_VOCAL_SILLY              ] = prefix + trBehaviorType.SOUND_VOCAL_SILLY.ToString();
        iconsBehavior[trBehaviorType.SOUND_VOCAL_SURPRISED          ] = prefix + trBehaviorType.SOUND_VOCAL_SURPRISED.ToString();
        iconsBehavior[trBehaviorType.SOUND_SFX                      ] = prefix + trBehaviorType.SOUND_SFX.ToString();
        iconsBehavior[trBehaviorType.SOUND_TRANSPORT                ] = prefix + trBehaviorType.SOUND_TRANSPORT.ToString();
        iconsBehavior[trBehaviorType.SOUND_INTERNAL                 ] = prefix + trBehaviorType.SOUND_INTERNAL.ToString();
        iconsBehavior[trBehaviorType.MOVE_FB0                       ] = prefix + "MOVE_FB0";
        iconsBehavior[trBehaviorType.MOVE_F1                        ] = prefix + "MOVE_F1";
        iconsBehavior[trBehaviorType.MOVE_F2                        ] = prefix + "MOVE_F2";
        iconsBehavior[trBehaviorType.MOVE_F3                        ] = prefix + "MOVE_F3";
        iconsBehavior[trBehaviorType.MOVE_B1                        ] = prefix + "MOVE_B1";
        iconsBehavior[trBehaviorType.MOVE_B2                        ] = prefix + "MOVE_B2";
        iconsBehavior[trBehaviorType.MOVE_B3                        ] = prefix + "MOVE_B3";
        iconsBehavior[trBehaviorType.MOVE_LR0                       ] = prefix + "MOVE_LR0";
        iconsBehavior[trBehaviorType.MOVE_L1                        ] = prefix + "MOVE_L1";
        iconsBehavior[trBehaviorType.MOVE_L2                        ] = prefix + "MOVE_L2";
        iconsBehavior[trBehaviorType.MOVE_L3                        ] = prefix + "MOVE_L3";
        iconsBehavior[trBehaviorType.MOVE_R1                        ] = prefix + "MOVE_R1";
        iconsBehavior[trBehaviorType.MOVE_R2                        ] = prefix + "MOVE_R2";
        iconsBehavior[trBehaviorType.MOVE_R3                        ] = prefix + "MOVE_R3";
        iconsBehavior[trBehaviorType.MOVE_STOP                      ] = prefix + "MOVE_STOP";
        iconsBehavior[trBehaviorType.PUPPET                         ] = prefix + trBehaviorType.PUPPET.ToString();
        iconsBehavior[trBehaviorType.MAPSET                         ] = prefix + "MAPPER";
        iconsBehavior[trBehaviorType.MOVE_CONT_STRAIGHT             ] = prefix + trBehaviorType.MOVE_CONT_STRAIGHT.ToString();
        iconsBehavior[trBehaviorType.MOVE_CONT_SPIN                 ] = prefix + trBehaviorType.MOVE_CONT_SPIN.ToString();
        iconsBehavior[trBehaviorType.MOVE_CONT_CIRCLE_CCW           ] = prefix + trBehaviorType.MOVE_CONT_CIRCLE_CCW.ToString();
        iconsBehavior[trBehaviorType.MOVE_CONT_CIRCLE_CW            ] = prefix + trBehaviorType.MOVE_CONT_CIRCLE_CW.ToString();
        iconsBehavior[trBehaviorType.MOVE_DISC_STRAIGHT             ] = prefix + trBehaviorType.MOVE_DISC_STRAIGHT.ToString();
        iconsBehavior[trBehaviorType.MOVE_DISC_TURN                 ] = prefix + trBehaviorType.MOVE_DISC_TURN.ToString();
        iconsBehavior[trBehaviorType.MOVE_TURN_VOICE                ] = prefix + trBehaviorType.MOVE_TURN_VOICE.ToString(); 
        iconsBehavior[trBehaviorType.HEAD_PAN                       ] = prefix + trBehaviorType.HEAD_PAN.ToString();
        iconsBehavior[trBehaviorType.HEAD_TILT                      ] = prefix + trBehaviorType.HEAD_TILT.ToString();
        iconsBehavior[trBehaviorType.HEAD_PAN_VOICE                 ] = prefix + trBehaviorType.HEAD_PAN_VOICE.ToString(); 
        iconsBehavior[trBehaviorType.EYERING                        ] = prefix + trBehaviorType.EYERING.ToString();
        iconsBehavior[trBehaviorType.LAUNCH_RELOAD_LEFT             ] = prefix + trBehaviorType.LAUNCH_RELOAD_LEFT.ToString();
        iconsBehavior[trBehaviorType.LAUNCH_RELOAD_RIGHT            ] = prefix + trBehaviorType.LAUNCH_RELOAD_RIGHT.ToString();
        iconsBehavior[trBehaviorType.LAUNCH_FLING                   ] = prefix + trBehaviorType.LAUNCH_FLING.ToString();
        iconsBehavior[trBehaviorType.RUN_SPARK                      ] = prefix + trBehaviorType.RUN_SPARK.ToString();
        
        iconsBehavior[trBehaviorType.EXPRESSION_CATEGORY_1          ] = prefix + "ui_spark_expression_cautious";
        iconsBehavior[trBehaviorType.EXPRESSION_CATEGORY_2          ] = prefix + "ui_spark_expression_curious";
        iconsBehavior[trBehaviorType.EXPRESSION_CATEGORY_3          ] = prefix + "ui_spark_expression_frustrated";
        iconsBehavior[trBehaviorType.EXPRESSION_CATEGORY_4          ] = prefix + "ui_spark_expression_joyful";
        iconsBehavior[trBehaviorType.EXPRESSION_CATEGORY_5          ] = prefix + "ui_spark_expression_silly";

        iconsBehavior[trBehaviorType.FUNCTION                       ] = prefix + "FUNCTION";
        iconsBehavior[trBehaviorType.FUNCTION_END                    ] = prefix + "FUNCTION_END";
      
  //    iconsBehavior[trBehaviorType.ANIMATION_CATEGORY_1          ] = prefix + "ui_spark_animation_movement";
  //    iconsBehavior[trBehaviorType.ANIMATION_CATEGORY_2          ] = prefix + "ui_spark_animation_emotion";
  //    iconsBehavior[trBehaviorType.ANIMATION_CATEGORY_3          ] = prefix + "ui_spark_animation_interactive";
  //    iconsBehavior[trBehaviorType.ANIMATION_CATEGORY_4          ] = prefix + "ui_spark_animation_passive";
      }
      
      return iconsBehavior;
    }
  }
}


