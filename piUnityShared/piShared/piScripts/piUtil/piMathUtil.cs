using UnityEngine;
using System;


public static class piMathUtil
{
	public const float epsilon = 0.0001f;
	
	// maps the number line to a sawtooth.
	// ∀ N ∈ ℤ, absmod(f + m * N, m) == absmod(f, m) for all 

  public static Vector3 clampVec(Vector3 orig, Vector3 min, Vector3 max){
    Vector3 ret = orig;
    ret.x = Mathf.Clamp(ret.x, min.x, max.x);
    ret.y = Mathf.Clamp(ret.y, min.y, max.y);
    ret.z = Mathf.Clamp(ret.z, min.z, max.z);
    return ret;
  }

	public static float absmod(float f, float m) {
		float ret = f % m;
		if (ret < 0) {
			ret += m;
		}
	return ret;
	}
	
	public static bool withinEpsilon(float smallValue) {
		return withinSpecifiedEpsilon(smallValue, epsilon);
	}

	public static bool withinEpsilon(float a, float b) {
		return withinSpecifiedEpsilon(a, b, epsilon);
	}

  public static bool withinSpecifiedEpsilon(float num, float specificEpsilon){
    return (num >= -specificEpsilon && num <= specificEpsilon);
  }

  public static bool withinSpecifiedEpsilon(float a, float b, float specificEpsilon){
    return withinSpecifiedEpsilon(a - b, specificEpsilon);
  }

  public static bool areaWithinSpecifiedEpsilon(Rect a, Rect b, float epsilon) {
    float aa = a.width * a.height;
    float ab = b.width * b.height;
    return withinSpecifiedEpsilon(aa, ab, epsilon);
  }
	
	public static float clamp(float value, float min, float max) {
		return Math.Min(max, Math.Max(min, value));
	}
	
	// returns the signed radians between the positive X axis and pt. radians increase counter-clockwise.
	public static float radiansToPoint(Vector2 pt) {
		if (pt.x == 0.0f && pt.y == 0.0f) {
			// point is at the origin: return 0 radians.
			return 0.0f;
		}
		
		float theta = Mathf.Atan2(pt.y, pt.x);
		
		return theta;
	}

	public static double degreesToRadians(double degrees){
		return (Math.PI / 180) * degrees;
	}

	//get bounding box from min and max pos
	public static Bounds boundsFromTwoPoints(Vector3 leftBottomPos, Vector3 rightTopPos){
		Vector3 center = (leftBottomPos + rightTopPos)/2.0f;
		Vector3 size = rightTopPos - leftBottomPos;
		return new Bounds(center, size);
	}
  
  public static void wheelSpeedsToLinearAngular(float wsL, float wsR, out float linearSpeed, out float angularSpeed, float wheelBase) {
    linearSpeed  = (wsR + wsL) / 2.0f;
    angularSpeed = (wsR - wsL) / wheelBase;
  }

  public static int serializeBoolArray(bool[] values){
    int result = 0;

    for (int i = 0; i < values.Length; i++){
      if (values[i]){
        result += 1 << i;
      }
    }
    return result;
  }

  public static bool[] deserializeBoolArray(int value){
    int size = sizeof(int) * 8;
    bool[] result = new bool[12];

    for (int i = 0; i < size; i++){
      if (i >= result.Length) {
        break;
      }
      if ((value & (1 << i)) != 0){
        result[i] = true;
      }
    }
    return result;
  }
  
  public static float parabolaLengthApproximate(float width, float height) {
    // home-rolling this.
    width = Mathf.Abs(width);
    height = Mathf.Abs(height);
    
    if (withinEpsilon(width)) {
      return height * 2f;
    }
    
    float aspect = height / width;
    
    if (aspect < 1f) {
      return Mathf.Lerp(width, height * 2f, aspect);
    }
    else {
      return height * 2f;
    }
  }

  // if val is a point on the function "y = x",
  // this inserts a plateau centered at flatCenter, with width flatWidth.
  // fixedPoint1 and fixedPoint2 should bracked flatCenter +/- flatWidth;
  // good values for them would be the minimum and maximum possible for val.
  // illustration here: https://docs.google.com/spreadsheets/d/1cYwa7Fvf6NHCL5Ft-vFg0QGt9ATpkwIGtGXE4b6QHYA/edit#gid=0
  public static float createFlat(float val, float flatCenter, float flatWidth, float fixedPoint1, float fixedPoint2) {
    float flat1 = flatCenter - flatWidth;
    float flat2 = flatCenter + flatWidth;
    if ((val < fixedPoint1) || (val > fixedPoint2)) {
      return val;
    }
    if (val < flat1) {
      return Mathf.Lerp(fixedPoint1, flatCenter, Mathf.InverseLerp(fixedPoint1, flat1, val));
    }
    else if (val > flat2) {
      return Mathf.Lerp(flatCenter, fixedPoint2, Mathf.InverseLerp(flat2, fixedPoint2, val));
    }
    else {
      return flatCenter;
    }
  }

  // wrap into +/- 180
  public static float normalizeDegrees(float f) {
    while (f > 180f) {
      f -= 360f;
    }
    while (f < -180f) {
      f += 360f;
    }

    return f;
  }

  public static float shortestDeltaDegrees(float fromThis, float toThis) {
    float ret = toThis - fromThis;
    ret = normalizeDegrees(ret);
    return ret;
  }
}

public static class piMathUtilTests {
  public static bool test() {
    int numFailed = 0;
//  numFailed += (testFlats() ? 0 : 1);
    return (numFailed > 0);
  }

  public static bool testFlats() {
    for (float f = -5f; f <= 5f; f += 0.1f) {
      float v = f;
      v = piMathUtil.createFlat(v,  2f, 1f, -1f, 4f);
//    v = piMathUtil.createFlat(v, -2f, 1f, -4f, 0f);
      WWLog.logError("testFlats: " + f + ", " + v);
    }
    return true;
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
    get {
      return valueRaw;
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
