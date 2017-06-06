using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

/// <summary>
/// piBotModel
/// The classes in this folder are used to represent a physical model of a PlayI Robot.
/// These classes are pure C#, without any unity-specific inclusions (with the exception of logging).
/// 
/// This code serves two distinct use-cases:
/// * The standard "controller" situation,
///   in which case the classes here are acting as a proxy for a physical robot connected to the app.
///   In this case, you send commands to the piBot model and read back sensors.
/// * The simulated robot case,
///   where the classes are a simulated robot, and another controller app is connected.
///   In this case, the piBot receives commands, and provides sensors. (sensors not yet implemented)
///
/// A piBot is fundamentally a collection of piBotComponents.
/// Components are either effectors (wheels, lights, etc) or sensors (distance, accelerometer),
/// or both (eg, wheels contain both a velocity effector and a distance encoder sensor).
///
/// Simulated bots should be "ticked" periodically by calling tick() with the elapsed time.
/// The bot in turn ticks all its relevant components.
/// 
/// Robot class hierarchy:
/// * piBotBase is an abstract class providing connection routines and component framework, but no actual components.
///   * piBotCommon adds in components common to both Bo and Yanna.
///     * piBotAxled adds in wheels. (note, Bo is the only axled robot currently)
///       * piBotBo adds a couple other components in Bo.
///     * piBotYanna adds in a couple Yanna-specific components.
/// 
/// </summary>

namespace PI {
  
  public enum ComponentID : int {
    WW_UNKNOWN                              =    0,
    WW_COMMAND_POWER                        =    1,
    WW_COMMAND_EYE_RING                     =  100,
    WW_COMMAND_RGB_EYE                      =  101,
    WW_COMMAND_RGB_LEFT_EAR                 =  102,
    WW_COMMAND_RGB_RIGHT_EAR                =  103,
    WW_COMMAND_RGB_CHEST                    =  104,
    WW_COMMAND_LED_TAIL                     =  105,
    WW_COMMAND_LED_BUTTON_MAIN              =  106,
    WW_COMMAND_MOTOR_LEFT_WHEEL             =  200,
    WW_COMMAND_MOTOR_RIGHT_WHEEL            =  201,
    WW_COMMAND_MOTOR_HEAD_TILT              =  202,
    WW_COMMAND_MOTOR_HEAD_PAN               =  203,
    WW_COMMAND_MOTION_BODY_LINEAR_ANGULAR   =  204,
    WW_COMMAND_MOTION_BODY_POSE             =  205,
    WW_COMMAND_MOTOR_HEAD_TILT_TIME         =  206,
    WW_COMMAND_MOTOR_HEAD_PAN_TIME          =  207,
    WW_COMMAND_MOTOR_HEAD_TILT_VELOCITY     =  208,
    WW_COMMAND_MOTOR_HEAD_PAN_VELOCITY      =  209,
    WW_COMMAND_MOTOR_HEAD_BANG              =  210,
    WW_COMMAND_SPEAKER                      =  300,
    WW_COMMAND_STOP_SOUND                   =  301,
    WW_COMMAND_PAMPLEMOUSSE_START           =  450,
    WW_COMMAND_PAMPLEMOUSSE_STOP            =  451,


    WW_SENSOR_BUTTON_MAIN                   = 1000,
    WW_SENSOR_BUTTON_1                      = 1001,
    WW_SENSOR_BUTTON_2                      = 1002,
    WW_SENSOR_BUTTON_3                      = 1003,
    WW_SENSOR_MOTOR_HEAD_PAN                = 2000,
    WW_SENSOR_MOTOR_HEAD_TILT               = 2001,
    WW_SENSOR_MOTION_BODY_POSE              = 2002,
    WW_SENSOR_MOTION_ACCELEROMETER          = 2003,
    WW_SENSOR_MOTION_GYROSCOPE              = 2004,
    WW_SENSOR_DISTANCE_FRONT_LEFT_FACING    = 3000,
    WW_SENSOR_DISTANCE_FRONT_RIGHT_FACING   = 3001,
    WW_SENSOR_DISTANCE_BACK                 = 3002,
    WW_SENSOR_DISTANCE_ENCODER_LEFT_WHEEL   = 3003,
    WW_SENSOR_DISTANCE_ENCODER_RIGHT_WHEEL  = 3004,
    WW_SENSOR_MICROPHONE                    = 3005,
    WW_SENSOR_BATTERY                       = 3006,
    WW_SENSOR_BEACON                        = 3007,
    WW_SENSOR_MIC_EVENT                     = 3008,
    WW_SENSOR_BEACON_V2                     = 3009,
    WW_SENSOR_KIDNAP                        = 4001,
    WW_SENSOR_STALLBUMP                     = 4002,
    WW_SENSOR_SOUND_PLAYING                 = 4003,
    // WW_SENSOR_ROBOT_STOPPED_ALL          = 4004,   not implemented yet
    // WW_SENSOR_ROBOT_STOPPED_WHEELS       = 4005,   not implemented yet
    WW_SENSOR_ANIMATION_PLAYING             = 4006,
    WW_COMMAND_SECRET_USER_SETTING          = 5000,
    WW_SENSOR_RAW1                          = 5101,
    WW_SENSOR_RAW2                          = 5102,
  };
  
  public enum ButtonState : int {
    BUTTON_NOTPRESSED = 0,
    BUTTON_PRESSED
  };
  
  public enum SoundEventIndex : int {
    SOUND_EVENT_NONE = 0,
    SOUND_EVENT_CLAP
  };
  
  public enum SoundVolumeSpecialValues : uint {
    PI_VOLUME_INVALID = 101
  };
  
  public enum BotConnectionState : int {
    UNKNOWN   = 0,
    DISCOVERED,
    CONNECTING,
    CONNECTED,
    FAILEDTOCONNECT,
    DISCONNECTING,
    DISCONNECTED,
    LOST,
  }
  
  public enum WWPoseMode : uint {
    WW_POSE_MODE_GLOBAL = 0,
    WW_POSE_MODE_RELATIVE_COMMAND,
    WW_POSE_MODE_RELATIVE_MEASURED,
    WW_POSE_MODE_SET_GLOBAL
  }
  
  public enum WWPoseDirection : uint {
    WW_POSE_DIRECTION_FORWARD = 0,
    WW_POSE_DIRECTION_BACKWARD,
    WW_POSE_DIRECTION_INFERRED,
  }
  
  public enum WWPoseWrap : uint {
    WW_POSE_WRAP_OFF = 0,
    WW_POSE_WRAP_ON,
  }
  
  public enum WWBeaconLevel : uint {
    BEACON_LEVEL_OFF         = 0,
    BEACON_LEVEL_LOW         = 1,
    BEACON_LEVEL_MEDIUM      = 2,
    BEACON_LEVEL_MEDIUM_HIGH = 3,
  }
  
  // from WWRobotColorIndex in WWConstants.h
  public enum WWBeaconColor : uint {
    WW_ROBOT_COLOR_WHITE    = 0,
    WW_ROBOT_COLOR_YELLOW   = 1,
    WW_ROBOT_COLOR_GREEN    = 2,
    WW_ROBOT_COLOR_ORANGE   = 3,
    WW_ROBOT_COLOR_BLUE     = 4,
    WW_ROBOT_COLOR_RED      = 5,
    WW_ROBOT_COLOR_PURPLE   = 6,
    WW_ROBOT_COLOR_BLUE2    = 7,
    WW_ROBOT_COLOR_OFF      = 8,
    WW_ROBOT_COLOR_INVALID  = 255,
  }
  
  public enum WWBeaconReceiver : uint {
    WW_BEACON_RECEIVER_UNKNOWN = (     0),
    WW_BEACON_RECEIVER_LEFT    = (1 << 0),
    WW_BEACON_RECEIVER_RIGHT   = (1 << 1),
    
  }
  
  public abstract class piBotConstants {
    public const int   eyeRingNumLEDs = 12;
    public const float axleLength     =  9.6f;    // centimeters between wheelcenters.
    // OXE TODO - axle length should be 9.6!
    // correct the simulation of robot motion,
    // and also verify that scale calculations are correct.
    
    public const float simDistanceSensorNoisePercent =  0.1f;
    public const float simDistanceSensorMinCm        =  0.0f;
    public const float simDistanceSensorMaxCm        = 50.0f;
    
    public const string USERSOUND_FILE_NAME      = "VOICE";
    
    public static string shortString(BotConnectionState value) {
      switch (value) {
      default:
      case BotConnectionState.UNKNOWN:
        return "unkn";
      case BotConnectionState.CONNECTED:
        return "cncd";
      case BotConnectionState.CONNECTING:
        return "cncg";
      case BotConnectionState.DISCONNECTED:
        return "dscd";
      case BotConnectionState.DISCONNECTING:
        return "dscg";
      case BotConnectionState.DISCOVERED:
        return "dscv";
      case BotConnectionState.FAILEDTOCONNECT:
        return "cncf";
      case BotConnectionState.LOST:
        return "lost";
      }
    }    
  }
  
  public abstract class piJSONTokens {
    public const string RED               = "r";
    public const string GREEN             = "g";
    public const string BLUE              = "b";
    public const string VELOCITY          = "velocity";
    public const string ANGLE             = "angle";
    public const string BRIGHTNESS        = "brightness";
    public const string ANIMATIONID       = "animationID";
    public const string ANIMATIONSPEED    = "animationSpeed";
    public const string LOOPS             = "loops";
    public const string BITMAP            = "bitmap";
    public const string STATE             = "state";
    public const string DISTANCE          = "distance";
    public const string REFLECTANCE       = "reflectance";
    public const string OTHERDISTANCE     = "otherDistance";
    public const string OTHERMARGIN       = "otherMargin";
    public const string AMPLITUDE         = "amplitude";
    public const string DIRECTION         = "direction";
    public const string EVENT             = "event";
    public const string VOICE_CONFIDENCE  = "voice_detection_confidence";
    public const string X                 = "x";
    public const string Y                 = "y";
    public const string Z                 = "z";
    public const string LEFTDATA          = "left";
    public const string RIGHTDATA         = "right";
    public const string RADIANS           = "radians";
    public const string WATERMARK         = "watermark";

    public const string WW_COMMAND_VALUE_SPEED_LINEAR              = "linear_cm_s";
    public const string WW_COMMAND_VALUE_SPEED_ANGULAR_RAD         = "angular_cm_s";
    public const string WW_COMMAND_VALUE_SPEED_ANGULAR_DEG         = "angular_deg_s";
    public const string WW_COMMAND_VALUE_ACCELERATION_LINEAR       = "linear_acc_cm_s_s";
    public const string WW_COMMAND_VALUE_ACCELERATION_ANGULAR      = "angular_acc_deg_s_s";
    public const string WW_COMMAND_VALUE_COLOR_RED                 = "r";
    public const string WW_COMMAND_VALUE_COLOR_GREEN               = "g";
    public const string WW_COMMAND_VALUE_COLOR_BLUE                = "b";

    public const string WW_COMMAND_VALUE_AMPLITUDE_DEG             = "amp_deg";
    public const string WW_COMMAND_VALUE_AMPLITUDE_CM_PER_S        = "amp_cm_s";
    public const string WW_COMMAND_VALUE_ANIM_ID                   = "anim";
    public const string WW_COMMAND_VALUE_BACKUP                    = "bkup";
    public const string WW_COMMAND_VALUE_ID                        = "id";
    public const string WW_COMMAND_VALUE_MAX_SCALE                 = "max_scl";
    public const string WW_COMMAND_VALUE_MEAN_DEG                  = "avg_deg";
    public const string WW_COMMAND_VALUE_PARAMETERS                = "params";
    public const string WW_COMMAND_VALUE_PERIOD_S                  = "prd_s";
    public const string WW_COMMAND_VALUE_REPEAT                    = "rpt";
    public const string WW_COMMAND_VALUE_SIDE_LENGTH_CM            = "sidelen_cm";
    public const string WW_COMMAND_VALUE_SIDE_TIME_S               = "sidetm_s";
    public const string WW_COMMAND_VALUE_TURN_TIME_S               = "turntm_s";
    public const string WW_COMMAND_VALUE_WEIGHT                    = "weight";




    public const string WW_SENSOR_VALUE_BATTERY_CHARGING           = "chg";
    public const string WW_SENSOR_VALUE_BATTERY_LEVEL              = "level";
    public const string WW_SENSOR_VALUE_BATTERY_VOLTAGE            = "volt";
    public const string WW_SENSOR_VALUE_BEACON_ROBOT_TYPE          = "rbtType";
    public const string WW_SENSOR_VALUE_BEACON_ROBOT_ID            = "rbtID";
    public const string WW_SENSOR_VALUE_BEACON_DATA_TYPE           = "dataType";
    public const string WW_SENSOR_VALUE_BEACON_DATA_LENGTH_BITS    = "dataLnBits";
    public const string WW_SENSOR_VALUE_BEACON_DATA                = "data";
    public const string WW_SENSOR_VALUE_BEACON_RECEIVERS           = "rcvrs";
    public const string WW_SENSOR_VALUE_BEACON_DISTANCE_LEVEL      = "dist";
    public const string WW_SENSOR_VALUE_BEACON_HEADING             = "hdng";
    public const string WW_SENSOR_VALUE_BEACON_HEADING_UNCERTAINTY = "hdngUncrt";
    public const string WW_SENSOR_VALUE_FLAG                       = "flag";
    public const string WW_SENSOR_VALUE_DATA                       = "data";
  }
  
  public struct piColorRGB {
    public byte r;
    public byte g;
    public byte b;
    
    public piColorRGB(byte red, byte green, byte blue) {
      r = red;
      g = green;
      b = blue;
    }
  }
  
  public struct piVector3 {
    public float x;
    public float y;
    public float z;
    
    public piVector3(float vx, float vy, float vz) {
      x = vx;
      y = vy;
      z = vz;
    }
  }
}

