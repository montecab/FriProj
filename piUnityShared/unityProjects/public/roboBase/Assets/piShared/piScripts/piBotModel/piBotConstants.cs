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
		COMPONENT_UNKNOWN                     =   0,
		
		COMPONENT_EYE_RING                    = 100,
		COMPONENT_RGB_EYE                     = 101,
		COMPONENT_RGB_LEFT_EAR                = 102,
		COMPONENT_RGB_RIGHT_EAR               = 103,
		COMPONENT_RGB_CHEST                   = 104,
		COMPONENT_LED_TAIL                    = 105,
		COMPONENT_LED_BUTTON_MAIN             = 106,
		
		COMPONENT_BUTTON_MAIN                 = 200,
		COMPONENT_BUTTON_1                    = 201,
		COMPONENT_BUTTON_2                    = 202,
		COMPONENT_BUTTON_3                    = 203,
		
		COMPONENT_MOTOR_LEFT_WHEEL            = 300,
	    COMPONENT_MOTOR_RIGHT_WHEEL           = 301,
		COMPONENT_MOTOR_HEAD_TILT             = 302,
		COMPONENT_MOTOR_HEAD_PAN              = 303,
		
	    COMPONENT_ACCELEROMETER               = 400,
		COMPONENT_GYROSCOPE                   = 401,
		
	    COMPONENT_DISTANCE_SENSOR_FRONT_LEFT  = 500,
	    COMPONENT_DISTANCE_SENSOR_FRONT_RIGHT = 501,
	    COMPONENT_DISTANCE_SENSOR_TAIL        = 502,
	    
		COMPONENT_SPEAKER                     = 600,
	    COMPONENT_SOUND_SENSOR                = 601,
	    
	    COMPONENT_ROBOT_PEER_SENSOR           = 700,
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
		SELECTED,
	}
	
	public abstract class piBotConstants {
		public const int   eyeRingNumLEDs = 12;
//		public const float axleLength     =  9.6f;		// centimeters between wheelcenters.
		public const float axleLength     =  6.0f;		// centimeters between wheelcenters.
		// OXE TODO - axle length should be 9.6!
		// correct the simulation of robot motion,
		// and also verify that scale calculations are correct.
		
		public const float simDistanceSensorNoisePercent =  0.1f;
		public const float simDistanceSensorMinCm        =  0.0f;
		public const float simDistanceSensorMaxCm        = 40.0f;
		
		public const float linearVelocityMagnitudeMax = 150.0f;	// cm / s
		public const float angularVelocityMagnitudeMax = 8.0f;	// rad / s
	}
	
	public abstract class piJSONTokens {
		public const string RED            = "r";
		public const string GREEN          = "g";
		public const string BLUE           = "b";
		public const string VELOCITY       = "velocity";
		public const string ANGLE          = "angle";
		public const string BRIGHTNESS     = "brightness";
		public const string ANIMATIONID    = "animationID";
		public const string ANIMATIONSPEED = "animationSpeed";
		public const string LOOPS          = "loops";
		public const string BITMAP         = "bitmap";
		public const string STATE          = "state";
		public const string DISTANCE       = "distance";
		public const string REFLECTANCE    = "reflectance";
		public const string OTHERDISTANCE  = "otherDistance";
		public const string OTHERMARGIN    = "otherMargin";
		public const string VOLUME         = "volume";
		public const string DIRECTION      = "direction";
		public const string EVENT          = "event";
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

