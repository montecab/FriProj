using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using PI;

public abstract class piBotAxled : piBotCommon {
	
	public piBotAxled(string inUUID, string inName) : base(inUUID, inName) {}

	// "x" is forward.
	// "y" is right.
	// "z" is up.
	
	private Vector3 axlePosition;
	private float	axleOrientation;
	public  float   axleLength = PI.piBotConstants.axleLength;
	
	// need to figure out real parameters
	public float    wheelAttenuation = 0.4f;
	
	// the maximum number of seconds we'll allow to pass per axle calculation tick.
	protected float maxSecondsPerTick = 0.001f * 20.0f;	// 20ms.
	
	public Vector3 AxlePosition {
		get {
			return axlePosition;
		}
	}
	public void setAxlePosition(Vector2 value) {
		axlePosition = value;
	}
	
	public float AxleOrientation {	
		get {
			return axleOrientation;
		}
	}
	public void setAxleOrientation(float value) {
		axleOrientation = value;
	}
	
	// convenience accessors.
	public piBotComponentMotorWheel WheelLeft  { get{ return (piBotComponentMotorWheel)(components[ComponentID.COMPONENT_MOTOR_LEFT_WHEEL ]); }}
	public piBotComponentMotorWheel WheelRight { get{ return (piBotComponentMotorWheel)(components[ComponentID.COMPONENT_MOTOR_RIGHT_WHEEL]); }}
	
	protected override void setupComponents() {
		base.setupComponents();
		
		// effectors
		addComponent<piBotComponentMotorWheel>(PI.ComponentID.COMPONENT_MOTOR_LEFT_WHEEL );
		addComponent<piBotComponentMotorWheel>(PI.ComponentID.COMPONENT_MOTOR_RIGHT_WHEEL);
		
		// setup some physical parameters.
		// todo - add bounds checking.
		// todo - this should come from a data file instead of code, or have some other mechanism for live tuning.
		
		// wheels.velocity.maxRateOfChange is centimeters per second per second. THIS IS ACCELERATION, NOT VELOCITY.
		WheelLeft .velocity.maxRateOfChange = 100.0f;
		WheelRight.velocity.maxRateOfChange = 100.0f;
	}
	
	// vector from the center of the axle to the right wheel, in world coordinates.
	Vector3 RightWheelVector {
		get {
			return Quaternion.Euler(0, 0, axleOrientation) * new Vector3(0, axleLength * 0.5f, 0);
		}
	}
	
	// unit vector from the center of the axle "forward".
	Vector3 ForwardVector {
		get {
			return Quaternion.Euler(0, 0, axleOrientation) * new Vector3(1, 0, 0);
		}
	}
	
	public override void tick (float dt) {
		// how many iterations of the core loop to do this tick:
		int numIters = Mathf.CeilToInt(dt / maxSecondsPerTick);
		
		// ensure at least one iteration:
		numIters = numIters > 0 ? numIters : 1;
		
		// seconds per iteration
		float dtPerIter = dt / numIters;
		
		/*
		using equation 6 from here:
		http://rossum.sourceforge.net/papers/DiffSteer :
		distAvg = (distR + distL) / 2.
		dTheta  = (distR - distL) / axleLength.
		theta   = dTheta + theta0.
		dX      = distAvg * cos(theta).
		dY      = distAvg * sin(theta).
		x       = dX + x0.
		y       = dY + y0.
		*/
		
		for (int n = 0; n < numIters; ++n) {
			// tick the base class to update wheel velocities.
			base.tick(dtPerIter);
			
			float distL = WheelLeft .velocity.ValCurrent * dtPerIter * wheelAttenuation;
			float distR = WheelRight.velocity.ValCurrent * dtPerIter * wheelAttenuation;
			
			float distAvg = (distL + distR) * 0.5f;
			float dTheta  = (distL - distR) / axleLength;
			float theta   = dTheta + (axleOrientation * Mathf.Deg2Rad);
			float dX      = distAvg * Mathf.Cos(theta);
			float dY      = distAvg * Mathf.Sin(theta);
			float x       = dX + axlePosition.x;
			float y       = dY + axlePosition.y;
			
			axlePosition.x  = x;
			axlePosition.y  = y;
			axleOrientation = theta * Mathf.Rad2Deg;			
		}
	}
	
	public void tareWheels() {
		WheelLeft .tare();
		WheelRight.tare();
	}
	
	
	// BOT COMMANDS
	public void cmd_move(double leftWheelVelocity, double rightWheelVelocity) {
		if (this.apiInterface != null) {
			apiInterface.move(this.UUID, leftWheelVelocity, rightWheelVelocity);
		}
	}
	
	public void cmd_moveWithDuration (double leftWheelVelocity, double rightWheelVelocity, double duration) {
		if (this.apiInterface != null) {
			apiInterface.moveWithDuration(this.UUID, leftWheelVelocity, rightWheelVelocity, duration);
		}
	}
	
}









