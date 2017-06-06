using UnityEngine;
using System.Collections.Generic;
using WW.SimpleJSON;


// this class is a core part of the 'behaviorMaker'.
// it established many connections from a single sensor to a single actuator,
// including the sensor range, sensor inversion, and actuator range.
// note that a single actuator is connected to only one sensor,
// but one sensor may be connected to multiple actuators, altho w/ different ranges & inversion.
namespace Turing {
  public class trMapSet { // note, does not inherit from trBase
    public List<trMap> Maps = new List<trMap>();

    public string Name;
    public string IconName;
    
    private trActuatorAccumulator actuatorAccumulator = new trActuatorAccumulator();
    
    
    #region serialization
    
    public JSONClass ToJson() {
      JSONClass jsc = new JSONClass();

      JSONArray jsmaps = new JSONArray();

      foreach (trMap trM in Maps) {
        jsmaps.Add(trM.ToJson());
      }
      
      jsc[TOKENS.MAPS] =jsmaps;
      
      if (Name != null) {
        jsc[TOKENS.USER_FACING_NAME] = Name;
      }
      
      if (IconName != null) {
        jsc[TOKENS.ICON_NAME] = IconName;
      }
      
      return jsc;
    }
    
    public static trMapSet FromJson(JSONClass jsc) {
      trMapSet ret = new trMapSet();
      
      if (jsc.ContainsKey(TOKENS.USER_FACING_NAME)) {
        ret.Name = jsc[TOKENS.USER_FACING_NAME];
      }
      
      if (jsc.ContainsKey(TOKENS.ICON_NAME)) {
        ret.IconName = jsc[TOKENS.ICON_NAME];
      }
      
      foreach (JSONClass jsM in jsc[TOKENS.MAPS].AsArray) {
        trMap trM = trMap.FromJson(jsM);
        if (ret.ContainsActuatorType(trM.Actuator.Type)) {
          WWLog.logError("duplicate actuatorType in mapper: " + jsM.ToString() + "  - keeping first.");
        }
        else {
          ret.Maps.Add(trM);
        }
      }
      
      return ret;
    }
    
    #endregion serialization
    
    public void onRobotState(piBotBase robot) {
      foreach (trMap map in Maps) {
        map.onRobotState(robot, actuatorAccumulator);
      }
      
      actuatorAccumulator.ApplyAndClear(robot);
    }
    
    public bool ContainsActuatorType(trActuatorType value) {
      foreach (trMap trM in Maps) {
        if (trM.Actuator.Type == value) {
          return true;
        }
      }
      
      return false;
    }
    
    public void resetTime() {
      foreach(trMap trM in Maps) {
        trM.resetTime();
      }
    }
    
    public override string ToString() {
      string ret = "";
      ret += Name + "\n";
      foreach (trMap trM in Maps) {
        ret += trM.ToString();
        ret += "\n";
      }
      return ret;
    }
  }
  
  public class trMap {  // note, does not inherit from trBase.
    public trSensor           Sensor;
    public trActuator         Actuator;
    public bool               InvertSensor;
    public wwRange            RangeSensor;
    public trActuatorPoints   ActuatorPoints;
    public bool               Active = true;
    public bool               SimpleMode = false;

    public trMap(){
      RangeSensor = new wwRange();
      ActuatorPoints = new trActuatorPoints();
    }
    
    #region serialization
    
    public JSONClass ToJson() {
      JSONClass jsc = new JSONClass();
      jsc[TOKENS.SENSOR_TYPE   ]          = Sensor  .Type.ToString();
      if(trSensor.Parameterized(Sensor.Type)){
        jsc[TOKENS.PARAMETER_VALUE].AsFloat  = Sensor.ParameterValue;
      }
      jsc[TOKENS.ACTUATOR_TYPE ]          = Actuator.Type.ToString();
      jsc[TOKENS.SENSOR_INVERT ].AsBool   = InvertSensor;
      jsc[TOKENS.SENSOR_RANGE  ]          = RangeSensor  .ToJson();
      jsc[TOKENS.ACTUATOR_POINT]          = ActuatorPoints.ToJson();
      jsc[TOKENS.ACTIVE        ].AsBool   = Active;
      jsc[TOKENS.SIMPLE_MODE   ].AsBool   = SimpleMode;
      return jsc;
    }
    
    public static trMap FromJson(JSONClass jsc) {
      trMap ret = new trMap();
      trSensorType   trST;
      trActuatorType trAT;
      piStringUtil.ParseStringToEnum<trSensorType  >(jsc[TOKENS.SENSOR_TYPE  ], out trST);
      piStringUtil.ParseStringToEnum<trActuatorType>(jsc[TOKENS.ACTUATOR_TYPE], out trAT);
      ret.Sensor        = new trSensor  (trST);
      if(jsc[TOKENS.PARAMETER_VALUE]!= null){
        ret.Sensor.ParameterValue = jsc[TOKENS.PARAMETER_VALUE].AsFloat;
        if(!trSensor.Parameterized(ret.Sensor.Type)){
          WWLog.logError("Sensor type "+ ret.Sensor.Type.ToString() + " shouldn't have parameterValue but the value is saved."  );
        }
      }
      ret.Actuator      = new trActuator(trAT);
      ret.InvertSensor  = jsc[TOKENS.SENSOR_INVERT].AsBool;
      ret.RangeSensor   = wwRange.FromJSON(jsc[TOKENS.SENSOR_RANGE  ]);

      if(jsc[TOKENS.ACTUATOR_RANGE] != null){ // legacy handling
        wwRange legacyAcRange = wwRange.FromJSON(jsc[TOKENS.ACTUATOR_RANGE]);
        ret.ActuatorPoints.Points[0] = new Vector2(0, legacyAcRange.Min);
        ret.ActuatorPoints.Points[1] = new Vector2(1, legacyAcRange.Max);
      }else{
        ret.ActuatorPoints = trActuatorPoints.FromJson(jsc[TOKENS.ACTUATOR_POINT]);
      }

      ret.Active        = jsc[TOKENS.ACTIVE].AsBool;
      ret.SimpleMode    = jsc[TOKENS.SIMPLE_MODE].AsBool;
      return ret;
    }
    
    #endregion serialization
    
    public void onRobotState(piBotBase robot, trActuatorAccumulator trAA) {

      //TODO: need to make this work even if sensor is null(simple mode)
      if((Sensor == null) || (Actuator == null) || (Active == false)){
        return;
      }
      // todo: tidy up.
      Sensor.Robot = (piBotBo)robot;
      Actuator.Robot = (piBotBo)robot;
      
      trAA.SetNormalizedValue(Actuator.Type, NormalizedSensorValueToNormalizedActuatorValue(Sensor.ValueNormalized));
      
//      Actuator.setValueNormalized(NormalizedSensorValueToNormalizedActuatorValue(Sensor.ValueNormalized));
    }
    
    public float NormalizedSensorValueToNormalizedActuatorValue(float val) {
      // expand the user-specified range of the sensor into [0, 1].
      val = RangeSensor.Normalize(val);
      
      // invert the sensor value.
      // note this happens after range expansion, so it's actually equivalent to inverting the actuator.
      val = InvertSensor ? (1.0f - val) : (val);
      
      // compress 0-1 into the range of the actuator.
      val = ActuatorPoints.Denormalize(val);
      
      return val;
    }
    
    public void resetTime() {
      if (Sensor.Type == trSensorType.TIME_IN_STATE) {
        Sensor.reset();
      }
    }
    
    public override string ToString() {
      string ret = "";
      
      ret += Sensor.Type.ToString().PadRight(25);
      ret += "[" + RangeSensor  .Min.ToString("0.00") + ", " + RangeSensor  .Max.ToString("0.00") + "]";
      ret += InvertSensor ? " (inv)" : "      ";
      ret += " ----> ";
      ret += Actuator.Type.ToString().PadRight(25);
      ret += "{ ";
      foreach(Vector2 v in ActuatorPoints.Points){
        ret += v.ToString() + ", ";
      }
      ret += "}";
      //ret += "[" + RangeActuator.Min.ToString("0.00") + ", " + RangeActuator.Max.ToString("0.00") + "]";
      return ret;
    }
  }

  public class trActuatorPoints{
    public List<Vector2> Points = new List<Vector2>();

    public const int POINT_NUM_LIMIT = 4;

    public trActuatorPoints(){
      Points.Add(Vector2.zero);
      Points.Add(Vector2.one);
    }

    public void RemoveAt(int index){
      if(index<0 || index > Points.Count -1){
        WWLog.logError("Index out of range. id: " + index );
        return;
      }
      Points.RemoveAt(index);

    }

    public int UpdatePoint(int index, Vector2 p){
      if(index<0 || index > Points.Count -1){
        WWLog.logError("Invalid index " + index);
        return -1;
      }

      if(Points[index] == p){
        return index;
      }

      if(!IsPointValid(p)){
        WWLog.logError("Invalid point "+ p);
        return -1;
      }

      if(index == 0){
        p.x = 0;
      }
      if(index == Points.Count - 1){
        p.x = 1;
      }

      bool isOrderChanged = false;
      isOrderChanged = isOrderChanged || (index>0 && Points[index-1].x >p.x);
      isOrderChanged = isOrderChanged || (index< Points.Count -1 && Points[index+1].x <p.x);
      if(isOrderChanged){
        Points.RemoveAt(index);
        return InsertPoint(p);
      }else{
        Points[index] = p;
      }

      return index;
    }

    public static bool IsPointValid(Vector2 value){
      return value.x >=0 && value.x <=1 && value.y >=0 && value.y <=1;
    }

    public int InsertPoint(Vector2 p){
      if(!IsPointValid(p)){
        WWLog.logError("Invalid point "+ p);
        return -1;
      }
      if(Points.Count >= POINT_NUM_LIMIT){
        return -1;
      }
      for(int i = 0; Points.Count > 1 && i< Points.Count-1; ++i){
        if(p.x >= Points[i].x && p.x <= Points[i+1].x){
          Points.Insert(i+1, p);
          return i+1;
        }
      }
      WWLog.logError("Trying to insert point  " + p.ToString() + " which is out of range. " + p.x);
      return -1;

    }

    public float Denormalize(float val){
      float ret = 0;
      for(int i = 0; Points.Count > 1 && i< Points.Count-1; ++i){
        if(val >= Points[i].x && val <= Points[i+1].x){
          float x1 = Points[i].x;
          float y1 = Points[i].y;
          float x2 = Points[i+1].x;
          float y2 = Points[i+1].y;
          if(x1 == x2){ // actually this shouldn't be allowed
            ret = y1;
          }else{
            ret = (val - x1)*(y2-y1)/(x2-x1) + y1;
          }         
          return ret;
        }
      }
      WWLog.logError("Value " + val + " out of range.");
      return ret;
    }

    public JSONNode ToJson() {
      JSONArray jspoints = new JSONArray();      
      foreach (Vector2 vec in Points) {
        jspoints.Add(trFactory.ToJson(vec));
      }      
      return jspoints;
    } 
    
    public static trActuatorPoints FromJson(JSONNode jsc) {
      trActuatorPoints ret = new trActuatorPoints();
      if(jsc.AsArray.Count > 0){
        ret.Points.Clear(); // clear constructor points
      }
      foreach (JSONClass jsT in jsc.AsArray) {
        Vector2 vec =  trFactory.FromJson(jsT);
        ret.Points.Add(vec);
      }
      
      return ret;
    }
  }
}
