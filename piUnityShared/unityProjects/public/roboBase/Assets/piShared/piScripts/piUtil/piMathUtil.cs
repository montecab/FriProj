using System;

public class piMathUtil
{
	public piMathUtil ()
	{
	}
	
	public const float epsilon = 0.0001f;
	
	// maps the number line to a sawtooth.
	// ∀ N ∈ ℤ, absmod(f + m * N, m) == absmod(f, m) for all 
	public static float absmod(float f, float m) {
		float ret = f % m;
		if (ret < 0) {
			ret += m;
		}
	return ret;
	}
	
	public static bool withinEpsilon(float smallValue) {
		return (smallValue >= -epsilon && smallValue <= epsilon);
	}

	public static bool withinEpsilon(float a, float b) {
		return withinEpsilon(a - b);
	}
	
	public static float clamp(float value, float min, float max) {
		return Math.Min(max, Math.Max(min, value));
	}
}


public class piInertialValue
{
	public  float valTarget       = 0;
	private float valCurrent      = 0;
	public  float maxRateOfChange = 1;
	
	public float ValCurrent {
		get {
			return valCurrent;
		}
	}
	
	public void setValCurrent(float value) {
		valCurrent = value;
	}
	
	public void tick(float dt) {
		float maxChange = maxRateOfChange * dt;
		
		if (valCurrent < valTarget) {
			valCurrent += maxChange;
			if (valCurrent > valTarget) {
				valCurrent = valTarget;
			}
		}
		else {
			// valCurrent is >= valTarget
			valCurrent -= maxChange;
			if (valCurrent < valTarget) {
				valCurrent = valTarget;
			}
		}
	}
}

public class piTaredValue
{
	private float valueRaw  = 0.0f;
	private float valueTare = 0.0f;

	public float ValueRaw {
		set {
			valueRaw = value;
		}
	}
			
	public float Value {
		get {
			return valueRaw - valueTare;
		}
	}
	
	public void tare() {
		valueTare = valueRaw;
	}
}
