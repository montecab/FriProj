using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Turing{
  public class trStringFactory {
    public static string GetParaString(trState state){
      switch (state.Behavior.Type) {
        default:
          return GetParaString(state, 0);
        case trBehaviorType.MOVE_CONT_SPIN:
          return GetParaString(state, 0) + ", " + GetParaString(state, 1);
      }
    }
      
    public static string GetParaString(trState state, int index){
      if(state.Behavior.IsParameterized){
        float para = state.GetBehaviorParameterValue(index);
        switch(state.Behavior.Type){
//        case trBehaviorType.MOVE_CONT_CIRCLE_CCW:
//        case trBehaviorType.MOVE_CONT_CIRCLE_CW:
          case trBehaviorType.MOVE_DISC_TURN:
          case trBehaviorType.HEAD_PAN:
          case trBehaviorType.HEAD_TILT:
            return GetDegreeText(-para);
          case trBehaviorType.MOVE_CONT_SPIN:
          case trBehaviorType.MOVE_CONT_STRAIGHT:
            return GetVelocityText(para);
          case trBehaviorType.MOVE_DISC_STRAIGHT:
            return GetDistanceText(para);
          case trBehaviorType.LAUNCH_FLING:
            return GetPowerText(para);
        }
      }
      return "";
    }

    public static string[] GetParaStringAndUnit(trState state, int index=0){
      if(state.Behavior.IsParameterized){
        float para = state.GetBehaviorParameterValue(index);
        switch(state.Behavior.Type){
          //        case trBehaviorType.MOVE_CONT_CIRCLE_CCW:
          //        case trBehaviorType.MOVE_CONT_CIRCLE_CW:
        case trBehaviorType.MOVE_DISC_TURN:
        case trBehaviorType.HEAD_PAN:
        case trBehaviorType.HEAD_TILT:
          return GetDegreeValuAndUnits(-para);
        case trBehaviorType.MOVE_CONT_SPIN:
        case trBehaviorType.MOVE_CONT_STRAIGHT:
          return GetVelocityValuAndUnits(para);
        case trBehaviorType.MOVE_DISC_STRAIGHT:
          return GetDistanceValuAndUnits(para);
        case trBehaviorType.LAUNCH_FLING:
          return GetPowerValueAndUnits(para);
        }
      }
      return new string[]{"", ""};
    }

    public static string GetParaString(trTrigger trigger, int index=0){
      if(trTrigger.Parameterized(trigger.Type)){
        switch(trigger.Type){
          case trTriggerType.TIME:
          case trTriggerType.TIME_RANDOM:
  		    case trTriggerType.TIME_LONG:
            return GetTimeText(trigger.ParameterValue);
          case trTriggerType.TRAVEL_LINEAR:
            return GetDistanceText(trigger.ParameterValue);
          case trTriggerType.TRAVEL_ANGULAR:
            return GetDegreeText(trigger.ParameterValue);          
        }
      }
      return "";
    }

    public static string GetParaString(trSensor sensor, int index=0){
      if(trSensor.Parameterized(sensor.Type)){
        switch(sensor.Type){
          case trSensorType.TIME_IN_STATE:
            return GetTimeText(sensor.ParameterValue);
          case trSensorType.TRAVEL_LINEAR:
            return GetDistanceText(sensor.ParameterValue);
          case trSensorType.TRAVEL_ANGULAR:
            return GetDegreeText(sensor.ParameterValue);
        }
      }
      return "";
    }

    public static string GetDegreeText(float v){
      return string.Join("", GetDegreeValuAndUnits(v));
    }

    public static string GetDistanceText(float v){
      return string.Join("", GetDistanceValuAndUnits(v));
    }

    public static string GetVelocityText(float v){
      return string.Join("", GetVelocityValuAndUnits(v));
    }

    public static string GetPowerText(float v){
      return string.Join("", GetPowerValueAndUnits(v));
    }

    public static string[] GetDistanceValuAndUnits(float v){
      return new string[]{v.ToString("0"), "cm"};
    }

    public static string[] GetVelocityValuAndUnits(float v){
      return new string[]{v.ToString("0"), "cm/s"};
    }

    public static string[] GetDegreeValuAndUnits(float v){
      return new string[]{v.ToString("0"), "\u00B0"}; // "\u00B0" in the units portion would be better, but see TUR-699
    }

    public static string[] GetPowerValueAndUnits(float v){
      float powerText = v * 100f;     
      return new string[]{powerText.ToString("0"), "%"};
    }

    public static bool isForward(float v) {
      return v >= 0;
    }

    public static string forwardOrBackward(float v) {
      return isForward(v) ? "forward" : "backward";
    }

    public static bool isLeft(float v) {
      return v > 0;
    }

    public static string leftOrRight(float v) {
      return isLeft(v) ? "left" : "right";
    }

    public static bool isUp(float v) {
      return v <= 0;
    }

    public static string upOrDown(float v) {
      return isUp(v) ? "up" : "down";
    }

    public static string GetTimeText(float value){
  	  int minutes = (int)value/60;
  	  int seconds = (int)value % 60;
  	  int hours = minutes/60;
      int days = hours/24;
      int remainingHours = hours%24;
  	  int remainingMinutes = minutes % 60;
  	  string displayString = "";
      if(days>0){
        displayString = displayString + days.ToString("0") + "d ";
      }
  	  if (remainingHours > 0) {
        displayString = displayString + remainingHours.ToString("0") + "h ";
  	  }
  	  if (remainingMinutes > 0) {
    		displayString = displayString + remainingMinutes.ToString("0") + "m ";
  	  }
      if (remainingHours < 1 && seconds >= 0) {
    		if (remainingMinutes < 1) {
    			displayString = value.ToString("0.0") + "s";
    		} 
        else {
    			displayString = displayString + seconds.ToString("0") + "s";
    		}
  	  }
  	  return displayString ;
  	}

    public static Dictionary<float, string> GetAxisInfo(trActuatorType type){
      
      Dictionary<float, string> newInfo = new Dictionary<float, string>();
      return newInfo;
    }

    public static Dictionary<float, string> GetAxisInfo(trSensor sensor){

      Dictionary<float, string> newInfo = new Dictionary<float, string>();
      switch(sensor.Type){
        case trSensorType.DISTANCE_FRONT:
        case trSensorType.DISTANCE_FRONT_LEFT_FACING:
        case trSensorType.DISTANCE_FRONT_RIGHT_FACING:
        case trSensorType.DISTANCE_REAR:
          newInfo.Add(0, "Near");
          newInfo.Add(0.5f, "Middle");
          newInfo.Add(1, "Far");
          break;
        case trSensorType.DISTANCE_FRONT_DELTA:
          newInfo.Add(0,"Right");
          newInfo.Add(0.5f, "Center");
          newInfo.Add(1, "Left");
          break;
        case trSensorType.HEAD_PAN:
          newInfo.Add(0, GetDegreeText(-120));
          newInfo.Add(0.5f, GetDegreeText(0));
          newInfo.Add(1, GetDegreeText(120));
          break;
        case trSensorType.HEAD_TILT:
          newInfo.Add(0, GetDegreeText(-7));
          newInfo.Add(0.26f, GetDegreeText(0));
          newInfo.Add(1, GetDegreeText(20));
          break;
        case trSensorType.TIME_IN_STATE:
          newInfo.Add(0, GetTimeText(0));
          newInfo.Add(0.5f, GetTimeText(sensor.ParameterValue*0.5f));
          newInfo.Add(1, GetTimeText(sensor.ParameterValue));
          break;
        case trSensorType.TRAVEL_LINEAR:
          newInfo.Add(0, GetDistanceText(0));
          newInfo.Add(0.5f, GetDistanceText(sensor.ParameterValue*0.5f));
          newInfo.Add(1, GetDistanceText(sensor.ParameterValue));
          break;
        case trSensorType.TRAVEL_ANGULAR:
          newInfo.Add(0, GetDegreeText(0));
          newInfo.Add(0.5f, GetDegreeText(sensor.ParameterValue*0.5f));
          newInfo.Add(1, GetDegreeText(sensor.ParameterValue));
          break;
      }
      return newInfo;
    }
  }
}
