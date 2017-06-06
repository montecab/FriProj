using UnityEngine;
using System.Collections.Generic;
using WW.SimpleJSON;

// this class is responsible for de-normalizing the range [0, 1] into the natural input for a given actuator.
// this data is the same for all programs.

namespace Turing {

  public enum trActuatorType {
    // note: the code below is generated by the spreadsheet at:
    //       https://docs.google.com/a/makewonder.com/spreadsheets/d/1cy1842eDd83pRsmid_8TE-RVlIhWCCR6s9kRRddGYig/edit#gid=1257986866
    WHEEL_L,  // 
    WHEEL_R,  // 
    POSE_UNKNOWN,  // needs thought !
    HEAD_PAN,  // 
    HEAD_TILT,  // 
    RGB_ALL_HUE,  // 0 = black, 0.001 - 0.999 = RGB HUES, 1 = white
    RGB_ALL_VAL,  // change all colors brightness (in terms HSV - value), [0;1]
    LED_TOP,  // 
    LED_TAIL,  // 
    EYERING,  // 0 = all off, .5 = half on, 1 = all on
  }

  static class trActuatorTypeMethods{
    public static string GetUserFacingName(this trActuatorType type){
      switch(type){
      case trActuatorType.WHEEL_L:
        return "Left Wheel";
      case trActuatorType.WHEEL_R:
        return "Right Wheel";
      case trActuatorType.EYERING:
        return "Everything";
      case trActuatorType.HEAD_PAN:
        return "Head Pan";
      case trActuatorType.HEAD_TILT:
        return "Head Tilt";
      case trActuatorType.LED_TAIL:
        return "Tail Light";
      case trActuatorType.LED_TOP:
        return "Top Button Light";
      case trActuatorType.RGB_ALL_HUE:
        return "All Light Color";
      case trActuatorType.RGB_ALL_VAL:
        return "All Light Brightness";
      case trActuatorType.POSE_UNKNOWN:
        return "Pose";
      
      default:
        WWLog.logError("unknown actuator type " + type.ToString());
        break;
      }
      return "";
    }
  }

  
  // used when we have special behavior at either end of a range.
  
      
  public class trActuator : trTypedBase<trActuatorType> {
  
    public const float NEAR_END_OF_RANGE_EPSILON = 0.001f;
    
    private static Dictionary<trActuatorType, wwRange> actuatorRanges = null;
    
    public piBotBo Robot;
    
    public trActuator() {}
    public trActuator(trActuatorType typeVal) : base(typeVal) {
      UUID = this.GetType().Name + "_" + typeVal.ToString();
    }
    
    
    public static Dictionary<trActuatorType, wwRange> ActuatorRanges {
      get {
        if (actuatorRanges == null) {
          actuatorRanges = new Dictionary<trActuatorType, wwRange>();
          
          // note: the code below is generated by the spreadsheet at:
          //       https://docs.google.com/a/makewonder.com/spreadsheets/d/1cy1842eDd83pRsmid_8TE-RVlIhWCCR6s9kRRddGYig/edit#gid=1257986866
          // TODO: provide 'soft' ranges for all actuators, which are the natural limits to use. these should be normalized.
          actuatorRanges[trActuatorType.WHEEL_L] = new wwRange(-80f, 80f);
          actuatorRanges[trActuatorType.WHEEL_R] = new wwRange(-80f, 80f);
          actuatorRanges[trActuatorType.POSE_UNKNOWN] = new wwRange(0f, 1f);
          actuatorRanges[trActuatorType.HEAD_PAN] = new wwRange(-120f, 120f);
          actuatorRanges[trActuatorType.HEAD_TILT] = new wwRange(-22.5f, 7.5f);
          actuatorRanges[trActuatorType.RGB_ALL_HUE] = new wwRange(0f, 1f);
          actuatorRanges[trActuatorType.RGB_ALL_VAL] = new wwRange(0f, 1f);
          actuatorRanges[trActuatorType.LED_TOP] = new wwRange(0f, 1f);
          actuatorRanges[trActuatorType.LED_TAIL] = new wwRange(0f, 1f);
          actuatorRanges[trActuatorType.EYERING] = new wwRange(0f, 360f);
        }        
        return actuatorRanges;
      }
    }
        
    public static float DenormalizeValue(trActuatorType actuatorType, float value) {
      return ActuatorRanges[actuatorType].Denormalize(value);
    }
    
    public static Color NormalizedValueToColor(float value, float brightness) {
      if (value < NEAR_END_OF_RANGE_EPSILON) {
        return Color.black;
      }
      if (value > (1.0f - NEAR_END_OF_RANGE_EPSILON)) {
        return new Color(brightness, brightness, brightness);
      }
      float renormalized = Mathf.InverseLerp(NEAR_END_OF_RANGE_EPSILON, 1.0f - NEAR_END_OF_RANGE_EPSILON, value);
      return wwColorUtil.HSVtoRGB(renormalized, 1, brightness);
    }
  }
  
  
  public class trActuatorAccumulator {
    
    private Dictionary<trActuatorType, float> values = new Dictionary<trActuatorType, float>();
    
    public void SetNormalizedValue(trActuatorType actuatorType, float value) {
      if (values.ContainsKey(actuatorType)) {
        WWLog.logWarn("setting duplicate actuator: " + actuatorType.ToString() + " : " + value);
      }
      values[actuatorType] = value;
    }
    
    public void Clear() {
      values.Clear();
    }
    
    public void ApplyAndClear(piBotBase robot) {
      ApplyAndClearWheels((piBotBo    )robot);
      ApplyAndClearHead  ((piBotBo    )robot);
      ApplyAndClearRGBs  ((piBotCommon)robot);
      ApplyAndClearLEDs  ((piBotBo    )robot);
      
      foreach (trActuatorType trAT in values.Keys) {
        if (wwDoOncePerTypeVal<trActuatorType>.doIt(trAT)) {
          WWLog.logError("unhandled actuator: " + trAT.ToString() + " : " + values[trAT].ToString("0.00"));
        }
      }
      
      Clear();
    }
    
    private void denormalizeAndClear(trActuatorType at, out float val, out bool present, float defaultVal, bool invert) {
      present = values.ContainsKey(at);
      
      if (present) {
        val = values[at];
        if (invert) {
          val = 1.0f - val;
        }
        val = trActuator.DenormalizeValue(at, val);
      }
      else {
        val = defaultVal;
      }
      
      values.Remove(at);
    }
    
    private void ApplyAndClearWheels(piBotBo robot) {
      bool hasL;
      bool hasR;
      float vL;
      float vR;
      
      denormalizeAndClear(trActuatorType.WHEEL_L, out vL, out hasL, (float)robot.prevWheelL, false);
      denormalizeAndClear(trActuatorType.WHEEL_R, out vR, out hasR, (float)robot.prevWheelR, false);
      
      if (!hasL && !hasR) {
        return;
      }
      
      if (hasL && hasR) {
        // convert two wheel values to linear/angular.
        float speedLin;
        float speedAng;
        
        piMathUtil.wheelSpeedsToLinearAngular(vL, vR, out speedLin, out speedAng, robot.axleLength);

        // TODO - use the version of the command which includes acceleration.
        robot.cmd_bodyMotion(speedLin, speedAng);
      }
      else {
        WWLog.logError("ActuatorAccumulator has one wheel but not the other!");
      }
    }
    
    private void ApplyAndClearHead(piBotBo robot) {
      bool hasP;
      bool hasT;
      float vP;
      float vT;

      denormalizeAndClear(trActuatorType.HEAD_PAN , out vP, out hasP, 0, true);
      denormalizeAndClear(trActuatorType.HEAD_TILT, out vT, out hasT, 0, true);
      
      if (hasP && hasT) {
        robot.cmd_headMove(vP, vT);
      }
      else if (hasP) {
        robot.cmd_headPan(vP);
      }
      else if (hasT) {
        robot.cmd_headTilt(vT);
      }
    }
    
    private void ApplyAndClearRGBs(piBotCommon robot) {
      bool hasH;
      bool hasV;
      float vH;
      float vV;
      
      denormalizeAndClear(trActuatorType.RGB_ALL_HUE, out vH, out hasH, robot.prevHue, false);
      denormalizeAndClear(trActuatorType.RGB_ALL_VAL, out vV, out hasV, robot.prevVal, false);
      
      if (!hasH && !hasV) {
        return;
      }
      
      if (!hasH) {
        vH = robot.prevHue;
      }
      if (!hasV) {
        vV = robot.prevVal;
      }
      
      Color c = trActuator.NormalizedValueToColor(vH, vV);
      
      robot.cmd_rgbLights(c.r, c.g, c.b);
    }
    
    private void ApplyAndClearLEDs(piBotBo robot) {
      bool hasM;
      bool hasT;
      float vM;
      float vT;
      
      denormalizeAndClear(trActuatorType.LED_TOP , out vM, out hasM, robot.prevHue, false);
      denormalizeAndClear(trActuatorType.LED_TAIL, out vT, out hasT, robot.prevVal, false);
      
      if (hasM) {
        robot.cmd_LEDButtonMain(vM);
      }
      if (hasT) {
        robot.cmd_LEDTail(vT);
      }
    }    
  }
}