using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using PI;

public class piBotBo : piBotAxled {

	public piBotBo(string inUUID, string inName) : base(inUUID, inName) {}
	

	// convenience accessors.
	public piBotComponentMotorServo         HeadPan                  { get{ return (piBotComponentMotorServo        )(components[ComponentID.COMPONENT_MOTOR_HEAD_PAN             ]); }}
	public piBotComponentMotorServo         HeadTilt                 { get{ return (piBotComponentMotorServo        )(components[ComponentID.COMPONENT_MOTOR_HEAD_TILT            ]); }}
	public piBotComponentLightRGB           RGBChest                 { get{ return (piBotComponentLightRGB          )(components[ComponentID.COMPONENT_RGB_CHEST                  ]); }}
	public piBotComponentLED                LEDTail                  { get{ return (piBotComponentLED               )(components[ComponentID.COMPONENT_LED_TAIL                   ]); }}
	public piBotComponentDistanceSensorPair DistanceSensorFrontLeft  { get{ return (piBotComponentDistanceSensorPair)(components[ComponentID.COMPONENT_DISTANCE_SENSOR_FRONT_LEFT ]); }}
	public piBotComponentDistanceSensorPair DistanceSensorFrontRight { get{ return (piBotComponentDistanceSensorPair)(components[ComponentID.COMPONENT_DISTANCE_SENSOR_FRONT_RIGHT]); }}
	public piBotComponentDistanceSensor     DistanceSensorTail       { get{ return (piBotComponentDistanceSensor    )(components[ComponentID.COMPONENT_DISTANCE_SENSOR_TAIL       ]); }}
	
	protected override void setupComponents() {
		base.setupComponents();
		
		// effectors
		addComponent<piBotComponentMotorServo        >(PI.ComponentID.COMPONENT_MOTOR_HEAD_PAN             );
		addComponent<piBotComponentMotorServo        >(PI.ComponentID.COMPONENT_MOTOR_HEAD_TILT            );
		addComponent<piBotComponentLightRGB          >(PI.ComponentID.COMPONENT_RGB_CHEST                  );
		addComponent<piBotComponentLED               >(PI.ComponentID.COMPONENT_LED_TAIL                   );
		
		// sensors
		addComponent<piBotComponentDistanceSensorPair>(PI.ComponentID.COMPONENT_DISTANCE_SENSOR_FRONT_LEFT );
		addComponent<piBotComponentDistanceSensorPair>(PI.ComponentID.COMPONENT_DISTANCE_SENSOR_FRONT_RIGHT);
		addComponent<piBotComponentDistanceSensor    >(PI.ComponentID.COMPONENT_DISTANCE_SENSOR_TAIL       );
		
		// setup some physical parameters.
		// todo - add bounds checking.
		// todo - this should come from a data file instead of code, or have some other mechanism for live tuning.
		axleLength = PI.piBotConstants.axleLength;
			
		// servos. maxRateOfChange is velocity.
		HeadPan .angle.maxRateOfChange = 300.0f;
		HeadTilt.angle.maxRateOfChange = 300.0f;
	}
	
	
	
	// BOT COMMANDS
	public void cmd_headTilt (double angle) {
		if (this.apiInterface != null) {
			apiInterface.headTilt(this.UUID, angle);
		}
	}
	
	public void cmd_headPan (double angle) {
		if (this.apiInterface != null) {
			apiInterface.headPan(this.UUID, angle);
		}
	}
	
	public void cmd_headMove (double panAngle, double tiltAngle) {
		if (this.apiInterface != null) {
			apiInterface.headMove(this.UUID, panAngle, tiltAngle);
		}
	}
	
	public void cmd_LEDTail(byte brightness) {
		if (apiInterface != null) {
			apiInterface.ledTail(UUID, brightness);
		}
	}
}









