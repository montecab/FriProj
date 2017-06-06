#include <math.h>

#ifndef PIDEFINITIONS_H
#define PIDEFINITIONS_H

// global defines
#define EYE_LED_COUNT 12
#define EYE_LED_COUNT_PROTOTYPE 16

#define PIBRIGHTNESS_MAX 255
#define PIBRIGHTNESS_MIN 0
#define PIVELOCITY_STOP 0
#define PIVELOCITY_UNSPECIFIED 1234143525
#define PIVELOCITY_ISVALID(vel) (10000 > fabs(vel))
#define PIANGLE_ORIGIN 0
#define PIVOLUME_MUTE 0
#define PIVOLUME_MAX 20
#define COMMAND_DURATION_MIN .03

// type definitions
typedef unsigned int PIUInteger;
typedef unsigned short PIUShort;
typedef unsigned char PIBrightness; // brightness variables [0-255]
typedef unsigned short PIVolume;
#define PI_VOLUME_INVALID 101
typedef double PIAngle; // angle variables in degrees
typedef double PIAngularVelocity; // velocity variables in degrees/s
typedef double PIVelocity; // velocity variables in cm/s
typedef double PICentimeter; // distance variables in cm
typedef unsigned char PISoundTrackIndex;
#define PI_SOUND_TRACK_INDEX_INVALID 0
typedef unsigned char PIPowerState;

// global enumerations
typedef enum {
    COMPONENT_UNKNOWN = 0,
    COMPONENT_POWER,
    COMPONENT_EYE_RING = 100,
    COMPONENT_RGB_EYE,
    COMPONENT_RGB_LEFT_EAR,
    COMPONENT_RGB_RIGHT_EAR,
    COMPONENT_RGB_CHEST,
    COMPONENT_LED_TAIL,
    COMPONENT_LED_BUTTON_MAIN,
    COMPONENT_BUTTON_MAIN = 200,
    COMPONENT_BUTTON_1,
    COMPONENT_BUTTON_2,
    COMPONENT_BUTTON_3,
    COMPONENT_MOTOR_LEFT_WHEEL = 300,
    COMPONENT_MOTOR_RIGHT_WHEEL,
    COMPONENT_MOTOR_HEAD_TILT,
    COMPONENT_MOTOR_HEAD_PAN,
    COMPONENT_BODY_MOTION_LINEAR_ANGULAR,
    COMPONENT_ACCELEROMETER = 400,
    COMPONENT_GYROSCOPE,
    COMPONENT_DISTANCE_SENSOR_FRONT_LEFT = 500,
    COMPONENT_DISTANCE_SENSOR_FRONT_RIGHT,
    COMPONENT_DISTANCE_SENSOR_TAIL,
    COMPONENT_BEACON,
    COMPONENT_SPEAKER = 600,
    COMPONENT_SOUND_SENSOR,
    COMPONENT_ROBOT_PEER_SENSOR = 700,
}PIComponentId;

typedef enum {
    PI_SUCCESS = 0,
    PI_ERROR,
} PIResult;

typedef enum {
    ROBOT_UNKNOWN,
    ROBOT_BO,
    ROBOT_YANA,
} PIRobotType;

typedef enum {
    EYEANIM_NONE = 0,
    EYEANIM_HALF_BLINK,
    EYEANIM_FULL_BLINK,
    EYEANIM_GRAVITY_EYE,
    EYEANIM_CIRCLE,
    EYEANIM_FAST_BLINK,
    
    EYEANIM_BITMAP = 0xffff,
} PIEyeAnimationIndex;

typedef enum {
    BUTTON_NOTPRESSED = 0,
    BUTTON_PRESSED,
} PIButtonState;

typedef enum {
    SOUND_EVENT_NONE = 0,
    SOUND_EVENT_CLAP,
} PISoundEventIndex;

typedef enum {
    BEACON_LEVEL_HIGH = 4,
    BEACON_LEVEL_MEDIUM_HIGH = 3,
    BEACON_LEVEL_MEDIUM = 2,
    BEACON_LEVEL_LOW = 1,
    BEACON_LEVEL_OFF = 0
} PIBeaconLevel;

// Defines for linear and angular velocity. Putting these here till we figure out a way to
// export them per robot
// Linear velocity can be from -1500 cm/sec to 1500 cm/sec
#define PI_BODY_MOTION_MAX_LINEAR_VELOCITY 1000
#define PI_BODY_MOTION_MIN_LINEAR_VELOCITY -1000

// Angular veclocity can be from -8000 mRad/sec to 8000 mRad/sec
#define PI_BODY_MOTION_MAX_ANGULAR_VELOCITY 8000
#define PI_BODY_MOTION_MIN_ANGULAR_VELOCITY -8000

#endif /* PIDEFINITIONS_H */
