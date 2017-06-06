
// https://github.com/playi/RobotFirmware/blob/Spark/NRF_Bo_App/Spark.h

namespace Turing {

  public static class sharedConstants {
    public const byte   fileDirChars           =  4;
    public const byte   fileBodyMaxChars       = 10;
    public const byte   filePathMaxChars       = fileDirChars + fileBodyMaxChars;
    public const string TOK_SYST               = "SYST";
    public const string TOK_SPKU               = "SPKU";
    public const string TOK_MAIN_SPARK_FILE    = "MAIN";
    public const string TOK_ROBOT_SUFFIX_ANIM  = "AN";
    public const string TOK_ROBOT_SUFFIX_SPARK = "SK";
    public const string TOK_ROBOT              = "<ROBOT>";
    public const string anFilePrefix           = "RobotResources/OnRobot/Animations/";
    public const float  fileTransferSafetyTime = 0.25f;  // seconds to wait after percent complete = 100%.

    public const float  robotLinearVelocityMin  = -150f;   // cm/s
    public const float  robotLinearVelocityMax  =  150f;   // cm/s 
    public const float  robotAngularVelocityMin =   -8f;   // r/s
    public const float  robotAngularVelocityMax =    8f;   // r/s 

    public static void clampLinearAngular(ref float linearVelocity, ref float angularVelocity) {
      if (linearVelocity < robotLinearVelocityMin) {
        linearVelocity = robotLinearVelocityMin;
      }
      else if (linearVelocity > robotLinearVelocityMax) {
        linearVelocity = robotLinearVelocityMax;
      }
      if (angularVelocity < robotAngularVelocityMin) {
        angularVelocity = robotAngularVelocityMin;
      }
      else if (angularVelocity > robotAngularVelocityMax) {
        angularVelocity = robotAngularVelocityMax;
      }
    }
  }

  public enum bluetoothCommand_t : byte {
    COMMAND_TYPE_PLAY_ANIMATION_BY_NAME = 0x26,
  }
  
  
  public enum behaviorType_t : byte {
    BEHAVIOR_TYPE_INVALID            = 0,
    BEHAVIOR_TYPE_ANIMATION          = 1,
    BEHAVIOR_TYPE_MAKER              = 2,
    BEHAVIOR_TYPE_SOUND              = 3,
    BEHAVIOR_TYPE_SOUND_RANDOM       = 4,   // unused
    BEHAVIOR_TYPE_SOUND_CONT         = 5,   // unused
    BEHAVIOR_TYPE_ANIMATION_CONT     = 6,   // unused
    BEHAVIOR_TYPE_DO_NOTHING         = 7,
    BEHAVIOR_TYPE_SIMPLE_MOVES       = 8,
    BEHAVIOR_TYPE_SET_MOOD           = 9,
    BEHAVIOR_TYPE_SET_EYERING        = 10,
    BEHAVIOR_TYPE_RGB_LIGHT          = 11,
    BEHAVIOR_TYPE_LAUNCH_FLING       = 12,
    BEHAVIOR_TYPE_TURN_TO_VOICE      = 13,
    BEHAVIOR_TYPE_LOOK_TO_VOICE      = 14,
    BEHAVIOR_TYPE_RUN_SPARK          = 15,
    BEHAVIOR_TYPE_LAUNCH_RELOAD      = 16,
    BEHAVIOR_TYPE_PLAY_PUPPET        = 17,
    BEHAVIOR_TYPE_PLAY_PUPPET_CONT   = 18,   // unused
    BEHAVIOR_TYPE_MAX_NUM            = 19
  }
  
  public enum behaviorSubtypeSimpleMove_t : byte {
    BEHAVIOR_SUBTYPE_SIMPLE_MOVE_INVALID = 0,
    BEHAVIOR_SUBTYPE_SIMPLE_MOVE_CONT = 1,
    BEHAVIOR_SUBTYPE_SIMPLE_MOVE_DISC_STRAIGHT = 2,
    BEHAVIOR_SUBTYPE_SIMPLE_MOVE_DISC_TURN = 3,
    BEHAVIOR_SUBTYPE_SIMPLE_MOVE_STOP = 4,
    BEHAVIOR_SUBTYPE_SIMPLE_MOVE_HEAD_PAN = 5,
    BEHAVIOR_SUBTYPE_SIMPLE_MOVE_HEAD_TILT = 6,
    BEHAVIOR_SUBTYPE_SIMPLE_MOVE_MAX_NUM = 7
  }
  
  
  public enum triggerType_t : byte {
    TRIGGER_TYPE_INVALID             =  0,
    TRIGGER_TYPE_BUTTON              =  1,    /*triggerSubtypeButton_t*/
    TRIGGER_TYPE_DIST                =  2,    /*TriggerSubtypeDist_t*/
    TRIGGER_TYPE_AUTO                =  3,    /*triggerSubtypeAuto_t*/
    TRIGGER_TYPE_TIMER               =  4,    /*triggerSubtypeTimer_t*/
    TRIGGER_TYPE_ACCEL               =  5,    /*TriggerSubtypeAccel_t*/
    TRIGGER_TYPE_WHEEL_ENCODER_SPEED =  6,    /*TriggerSubtypeWheelEncoderSpeed_t*/
    TRIGGER_TYPE_STALL_BUMP          =  7,    /*triggerSubtypeStallBump_t*/
    TRIGGER_TYPE_KIDNAP              =  8,    /*triggerSubtypeKidnap_t*/
    TRIGGER_TYPE_CLAP                =  9,    /*triggerSubtypeClap_t*/
    TRIGGER_TYPE_BEACON              = 10,    /*TriggerSubtypeBeacon_t*/
    TRIGGER_TYPE_DISTANCE_TRAVELED   = 11,    /*TriggerSubtypeDistanceTraveled_t*/
    TRIGGER_TYPE_RANDOM              = 12,    /*triggerSubtypeRandom_t*/
    TRIGGER_TYPE_VOICE               = 13,    /*triggerSubtypeVoice_t*/
    TRIGGER_TYPE_LONG_TIME           = 14,    /*triggerSubTypeLongTime_t*/
  }
  
  
  public enum triggerSubtypeButton_t : ushort {
    TRIGGER_SUBTYPE_BUTTON_INVALID =  0,
    TRIGGER_SUBTYPE_BUTTON_DN_MAIN =  1,
    TRIGGER_SUBTYPE_BUTTON_UP_MAIN =  2,
    TRIGGER_SUBTYPE_BUTTON_DN_1    =  3,
    TRIGGER_SUBTYPE_BUTTON_UP_1    =  4,
    TRIGGER_SUBTYPE_BUTTON_DN_2    =  5,
    TRIGGER_SUBTYPE_BUTTON_UP_2    =  6,
    TRIGGER_SUBTYPE_BUTTON_DN_3    =  7,
    TRIGGER_SUBTYPE_BUTTON_UP_3    =  8,
    TRIGGER_SUBTYPE_BUTTON_UP_ALL  =  9,
    TRIGGER_SUBTYPE_BUTTON_DN_ANY  = 10,
    TRIGGER_SUBTYPE_BUTTON_MAX_NUM = 11,
  }
  
  public enum triggerSubtypeDist_t : ushort {
    TRIGGER_SUBTYPE_DIST_LEFT_FAR   = 1 <<  0,  // 0x001
    TRIGGER_SUBTYPE_DIST_LEFT_NEAR  = 1 <<  1,  // 0x002
    TRIGGER_SUBTYPE_DIST_LEFT_NONE  = 1 <<  2,  // 0x004
    TRIGGER_SUBTYPE_DIST_RIGHT_FAR  = 1 <<  3,  // 0x008
    TRIGGER_SUBTYPE_DIST_RIGHT_NEAR = 1 <<  4,  // 0x010
    TRIGGER_SUBTYPE_DIST_RIGHT_NONE = 1 <<  5,  // 0x020
    TRIGGER_SUBTYPE_DIST_FRONT_FAR  = 1 <<  6,  // 0x040
    TRIGGER_SUBTYPE_DIST_FRONT_NEAR = 1 <<  7,  // 0x080
    TRIGGER_SUBTYPE_DIST_FRONT_NONE = 1 <<  8,  // 0x100
    TRIGGER_SUBTYPE_DIST_REAR_FAR   = 1 <<  9,  // 0x200
    TRIGGER_SUBTYPE_DIST_REAR_NEAR  = 1 << 10,  // 0x400
    TRIGGER_SUBTYPE_DIST_REAR_NONE  = 1 << 11,  // 0x800
  }
  
  public enum triggerSubtypeAuto_t : ushort {
    TRIG_SUBTYPE_AUTO_VALID = 0
  }
  
  public enum triggerSubtypeTimer_t : ushort {
    TRIG_SUBTYPE_TIMER_VALID = 0
  }
  
  public enum triggerSubtypeAccel_t : ushort {
    TRIGGER_SUBTYPE_ACCEL_INVALID = 0,
    TRIGGER_SUBTYPE_ACCEL_LEAN_LEFT = 1,
    TRIGGER_SUBTYPE_ACCEL_LEAN_RIGHT = 2,
    TRIGGER_SUBTYPE_ACCEL_LEAN_FORWARD = 3,
    TRIGGER_SUBTYPE_ACCEL_LEAN_BACKWARD = 4,
    TRIGGER_SUBTYPE_ACCEL_UPSIDE_UP = 5,
    TRIGGER_SUBTYPE_ACCEL_UPSIDE_DOWN = 6,
    TRIGGER_SUBTYPE_ACCEL_SHAKE = 7,
    TRIGGER_SUBTYPE_ACCEL_TOSS = 8,
    TRIGGER_SUBTYPE_ACCEL_PICKED_UP = 9,
    TRIGGER_SUBTYPE_ACCEL_NOT_MOVING = 10,
    TRIGGER_SUBTYPE_ACCEL_NOT_SHAKE = 11,
    TRIGGER_SUBTYPE_ACCEL_SLIDE_FWD = 12,
    TRIGGER_SUBTYPE_ACCEL_SLIDE_BACKWARD = 13,
    TRIGGER_SUBTYPE_ACCEL_SLIDE_LEFT = 14,
    TRIGGER_SUBTYPE_ACCEL_SLIDE_RIGHT = 15,
    TRIGGER_SUBTYPE_ACCEL_SLIDE_UP = 16,
    TRIGGER_SUBTYPE_ACCEL_SLIDE_DOWN = 17,
    TRIGGER_SUBTYPE_ACCEL_DROP = 18
  }  
  
  public enum triggerSubtypeWheelSpeed_t : ushort {
    TRIGGER_SUBTYPE_WHEEL_ENCODER_INVALID = 0,
    TRIGGER_SUBTYPE_WHEEL_ENCODER_FORWARD = 1,
    TRIGGER_SUBTYPE_WHEEL_ENCODER_BACKWARD = 2, 
    TRIGGER_SUBTYPE_WHEEL_ENCODER_STOPPED = 3  
  }
  
  public enum triggerSubtypeStallBump_t {
    TRIGGER_SUBTYPE_STALL_BUMP_INVALID = 0,
    TRIGGER_SUBTYPE_STALL_BUMP = 1,
    TRIGGER_SUBTYPE_NOT_STALL_BUMP = 2
  }
  
  public enum triggerSubtypeKidnap_t {
    TRIGGER_SUBTYPE_KIDNAP_INVALID = 0,
    TRIGGER_SUBTYPE_KIDNAP = 1,
    TRIGGER_SUBTYPE_NOT_KIDNAP = 2
  }
  
  public enum triggerSubtypeClap_t : byte {
    TRIGGER_SUBTYPE_CLAP_INVALID = 0,
    TRIGGER_SUBTYPE_CLAP_SINGLE = 1,
    TRIGGER_SUBTYPE_CLAP_DOUBLE = 2
  }

  public enum triggerSubtypeBeacon_t : ushort {
    TRIGGER_SUBTYPE_BEACON_INVALID            = 0,      // 0x000
    TRIGGER_SUBTYPE_BEACON_DOT_NONE           = 1 << 0, // 0x001
    TRIGGER_SUBTYPE_BEACON_DOT_LEFT           = 1 << 1, // 0x002
    TRIGGER_SUBTYPE_BEACON_DOT_RIGHT          = 1 << 2, // 0x004
    TRIGGER_SUBTYPE_BEACON_DOT_BOTH           = 1 << 3, // 0x008
    TRIGGER_SUBTYPE_BEACON_DASH_NONE          = 1 << 4, // 0x010
    TRIGGER_SUBTYPE_BEACON_DASH_LEFT          = 1 << 5, // 0x020
    TRIGGER_SUBTYPE_BEACON_DASH_RIGHT         = 1 << 6, // 0x040
    TRIGGER_SUBTYPE_BEACON_DASH_BOTH          = 1 << 7, // 0x080
    TRIGGER_SUBTYPE_BEACON_DASH_AND_DOT_NONE  = 1 << 8, // 0x100
  }
  
  public enum TriggerSubtypeDistanceTraveled_t {
    TRIGGER_SUBTYPE_DISTANCE_TRAVELED_INVALID = 0,
    TRIGGER_SUBTYPE_DISTANCE_TRAVELED_LINEAR = 1,
    TRIGGER_SUBTYPE_DISTANCE_TRAVELED_ANGLE = 2
  }
      
  public enum triggerSubtypeRandom_t : ushort {
    TRIGGER_SUBTYPE_RANDOM_INVALID = 0,
    TRIGGER_SUBTYPE_RANDOM_TIME = 1,
    TRIGGER_SUBTYPE_RANDOM_NEXT_STATE_TRANSITION = 2
  }
 
  public enum actuatorID_t : byte {
    ACT_INVALID = 0,
    ACT_HEAD_PAN = 1,
    ACT_HEAD_TILT = 2,
    ACT_RGB_BRIGHTNESS = 3,
    ACT_RGB_HUE = 4, 
    ACT_LEFT_WHEEL_SPEED = 5,
    ACT_RIGHT_WHEEL_SPEED = 6,
    ACT_LED_TOP = 7,
    ACT_LED_TAIL = 8,
    ACT_MAX_NUM = 9
  }
  
  public enum sensorID_t : byte {
    SENSE_INVALID  = 0,
    SENSE_DISTANCE = 1,
    SENSE_TIME     = 2,
    SENSE_TRAVEL   = 3,
    SENSE_HEAD     = 4,
    SENSE_ACCEL    = 5,
    SENSE_RANDOM   = 6,
    SENSE_MAX_NUM  = 7
  }
  
  public enum sensorSubtypeDistance_t : byte {
    SENSOR_SUBTYPE_DISTANCE_INVALID = 0,
    SENSOR_SUBTYPE_DISTANCE_LEFT_FACING = 1,
    SENSOR_SUBTYPE_DISTANCE_RIGHT_FACING = 2,
    SENSOR_SUBTYPE_DISTANCE_REAR = 3,
    SENSOR_SUBTYPE_DISTANCE_FRONT = 4,
    SENSOR_SUBTYPE_DISTANCE_FRONT_DELTA = 5
  }
  
  public enum sensorSubtypeTime_t : byte {
    SENSOR_SUBTYPE_TIME_VALID = 0
  }
  
  public enum sensorSubtypeTravel_t : byte {
    SENSOR_SUBTYPE_TRAVEL_INVALID = 0,
    SENSOR_SUBTYPE_TRAVEL_LINEAR = 1,  
    SENSOR_SUBTYPE_TRAVEL_ANGULAR = 2  
  }
  
  public enum sensorSubtypeHead_t : byte {
    SENSOR_SUBTYPE_HEAD_INVALID = 0,
    SENSOR_SUBTYPE_HEAD_PAN = 1,
    SENSOR_SUBTYPE_HEAD_TILT = 2  
  }
  
  public enum sensorSubtypeAccel_t : byte {
    SENSOR_SUBTYPE_ACCEL_INVALID = 0,
    SENSOR_SUBTYPE_ACCEL_ROLL = 1,
    SENSOR_SUBTYPE_ACCEL_PITCH = 2
  }
  
  public enum sensorSubtypeRandom_t : byte {
    SENSOR_SUBTYPE_RANDOM_VALID = 0
  }
  
 public enum triggerSubtypeVoice_t : byte {
  TRIG_SUBTYPE_VOICE_DETECTED = 0
 }

  
}



