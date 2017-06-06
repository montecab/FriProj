using UnityEngine;  // for Mathf.lerp.
using WW.SimpleJSON;

public class wwRange {

  private float min;
  private float max;
  public float Epsilon = 0.001f;
  
  private const string TOKEN_MIN = "min";
  private const string TOKEN_MAX = "max";
  
  public void setMinMax(float minVal, float maxVal) {
    if (minVal > maxVal) {
      WWLog.logWarn("swapping min & max: " + minVal + " <--> " + maxVal);
      float tmp = minVal;
      minVal = maxVal;
      maxVal = tmp;
    }
    min = minVal;
    max = maxVal;
  }
  
  public float Min {
    get {
      return this.min;
    }
  }

  public float Max {
    get {
      return this.max;
    }
  }
  
  public wwRange() {
    min = 0;
    max = 1;
  }
  
  public wwRange(float minVal, float maxVal) {
    min = Mathf.Min(minVal, maxVal);
    max = Mathf.Max(minVal, maxVal);
  }

  /// <summary>
  /// returns a random value between min & max.
  /// </summary>
  /// <value>a random value in the range.</value>
  public float RandVal {
    get {
      return Random.Range(min, max);
    }
  }
  
  // convert a value in the range [0, 1] into the range [Min, Max].
  public float Denormalize(float value) {
    float ret = Mathf.Lerp(Min, Max, value);
    ret = Mathf.Clamp(ret, Min, Max);
    return ret;
  }
  
  // convert a value that lies inside the range [Min, Max] into the range [0, 1].
  public float Normalize(float value) {
    // note this handles the case where Min == Max.
    // in that case this becomes a boolean filter returning 0 if value <= Min, and 1 otherwise.
    if (value <= Min) {
      return 0;
    }
    
    if (value >= Max) {
      return 1;
    }
    
    return Mathf.InverseLerp(Min, Max, value);
  }

  public bool IsValueInRange(float value){
    return value >= Min && value <= Max;
  }

  public float ConvertToRange(float value){
    float result = value;
    if (result < Min){
      result = Min;
    } else if (result > Max){
      result = Max;
    }
    return result;
  }
  
  public float Span {
    get {
      return Max - Min;
    }
  }
  
  public JSONClass ToJson() {
    JSONClass jsc = new JSONClass();
    jsc[TOKEN_MIN].AsFloat = Min;
    jsc[TOKEN_MAX].AsFloat = Max;
    return jsc;
  }
  
  public static wwRange FromJSON(JSONNode jsn) {
    if (!(jsn is JSONClass)) {
      WWLog.logError("invalid json type send to FromJson: " + jsn.GetType().ToString());
      return null;
    }
    
    JSONClass jsc = (JSONClass)jsn;
    if (jsc[TOKEN_MIN] == null || jsc[TOKEN_MAX] == null) {
      WWLog.logError("malformed range JSON: " + jsn.ToString());
      return null;
    }
    
    return new wwRange(jsc[TOKEN_MIN].AsFloat, jsc[TOKEN_MAX].AsFloat);
  }

  public override string ToString ()
  {
    return string.Format ("[wwRange: Min={0}, Max={1}]", Min, Max);
  }
  
}
