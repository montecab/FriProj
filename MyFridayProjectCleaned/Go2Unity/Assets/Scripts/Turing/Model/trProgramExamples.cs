using UnityEngine;
using System.Collections.Generic;

namespace Turing {

  public class trProgramExamples {
  
    private static List<trProgram> examples = null;
    
    public static List<trProgram> Examples {
      get {
        if (examples == null) {
          examples = new List<trProgram>();
          examples.Add(Example1);
//        examples.Add(Example2);
          examples.Add(Example3);
          examples.Add(Example4);
          examples.Add(Example5);
          examples.Add(Example6);
          examples.Add(Example7);
          examples.Add(ExampleLinAng);
          examples.Add(ExampleParamTime);
          examples.Add(ExampleParamTravel);
          examples.Add(ExampleParamSensTravel);
        }
        
        return examples;
      }
    }
    
    public static trProgram ProgramWithName(string name) {
      foreach (trProgram trP in Examples) {
        if (trP.UserFacingName == name) {
          return trP;
        }
      }
      
      return null;
    }
    
  
    private static trProgram Example1 {
      get {
        trProgram    prg1   = new trProgram();
        trBehavior   bhv0   = new trBehavior(trBehaviorType.DO_NOTHING);
        trBehavior   bhv1   = new trBehavior(trBehaviorType.COLOR_CYAN);
        trBehavior   bhv2   = new trBehavior(trBehaviorType.COLOR_RED);
        trBehavior   bhv3   = new trBehavior(trBehaviorType.COLOR_WHITE);
        trBehavior   bhv4   = new trBehavior(trBehaviorType.COLOR_YELLOW);
        trBehavior   bhvANM = new trBehavior(trBehaviorType.ANIM_TORNADO);
        trState      stt0   = new trState();
        trState      stt1   = new trState();
        trState      stt2   = new trState();
        trState      stt3   = new trState();
        trState      stt4   = new trState();
        trState      stt5   = new trState();
        trTransition trn12  = new trTransition();
        trTransition trn23  = new trTransition();
        trTransition trn34  = new trTransition();
        trTransition trn41  = new trTransition();
        trTransition trn13  = new trTransition();
        trTransition trn21  = new trTransition();
        trTransition trn32  = new trTransition();
        trTransition trn05  = new trTransition();
        trTransition trn51  = new trTransition();
        trTransition trn12t = new trTransition();
        trTransition trn21t = new trTransition();
        trTrigger    trgM   = new trTrigger(trTriggerType.BUTTON_MAIN);
        trTrigger    trg1   = new trTrigger(trTriggerType.BUTTON_1);
        trTrigger    trg2   = new trTrigger(trTriggerType.BUTTON_2);
        trTrigger    trgIM  = new trTrigger(trTriggerType.IMMEDIATE);
        trTrigger    trgT5  = new trTrigger(trTriggerType.TIME_5);
        trTrigger    trgBF  = new trTrigger(trTriggerType.BEHAVIOR_FINISHED);
        
        prg1.UserFacingName = "program 1";
        
        stt0.Behavior       = bhv0;
        stt0.UserFacingName = "omni";
        stt0.LayoutPosition = new Vector2(-1.5f, 2.5f);
        
        stt1.Behavior       = bhv1;
        stt1.UserFacingName = "state one";
        stt1.LayoutPosition = new Vector2( 0.0f, 2.0f);
        
        stt2.Behavior       = bhv2;
        stt2.UserFacingName = "state two";
        stt2.LayoutPosition = new Vector2(-1.5f, 0.0f);
        
        stt3.Behavior       = bhv3;
        stt3.UserFacingName = "state three";
        stt3.LayoutPosition = new Vector2( 1.5f, 2.0f);
        
        stt4.Behavior       = bhv4;
        stt4.UserFacingName = "state four";
        stt4.LayoutPosition = new Vector2( 2.0f, 0.0f);
        
        stt5.Behavior       = bhvANM;
        stt5.UserFacingName = "state five";
        stt5.LayoutPosition = new Vector2( 2.5f, 2.0f);
        
        trn12.StateSource = stt1;
        trn12.Trigger     = trgM;
        trn12.StateTarget = stt2;
        trn23.StateSource = stt2;
        trn23.Trigger     = trgM;
        trn23.StateTarget = stt3;
        trn34.StateSource = stt3;
        trn34.Trigger     = trgM;
        trn34.StateTarget = stt4;
        trn41.StateSource = stt4;
        trn41.Trigger     = trgIM;
        trn41.StateTarget = stt1;
        trn51.StateSource = stt5;
        trn51.Trigger     = trgBF;
        trn51.StateTarget = stt1;
        
        trn13.StateSource = stt1;
        trn13.Trigger     = trg1;
        trn13.StateTarget = stt3;
        trn21.StateSource = stt2;
        trn21.Trigger     = trg1;
        trn21.StateTarget = stt1;
        trn32.StateSource = stt3;
        trn32.Trigger     = trg1;
        trn32.StateTarget = stt2;
        
        trn05.StateSource = stt0;
        trn05.Trigger     = trg2;
        trn05.StateTarget = stt5;
        
        trn12t.StateSource = stt1;
        trn12t.Trigger     = trgT5;
        trn12t.StateTarget = stt2;
        trn21t.StateSource = stt2;
        trn21t.Trigger     = trgT5;
        trn21t.StateTarget = stt1;
        
        stt0.AddOutgoingTransition(trn05);
        stt1.AddOutgoingTransition(trn12);
        stt1.AddOutgoingTransition(trn13);
        stt1.AddOutgoingTransition(trn12t);
        stt2.AddOutgoingTransition(trn21);
        stt2.AddOutgoingTransition(trn23);
        stt2.AddOutgoingTransition(trn21t);
        stt3.AddOutgoingTransition(trn32);
        stt3.AddOutgoingTransition(trn34);
        stt4.AddOutgoingTransition(trn41);
        stt5.AddOutgoingTransition(trn51);
        
        prg1.UUIDToBehaviorTable  .Clear();
        prg1.UUIDToStateTable     .Clear();
        prg1.UUIDToTransitionTable.Clear();
        prg1.UUIDToBehaviorTable  .Add(bhv0.UUID, bhv0);
        prg1.UUIDToBehaviorTable  .Add(bhv1.UUID, bhv1);
        prg1.UUIDToBehaviorTable  .Add(bhv2.UUID, bhv2);
        prg1.UUIDToBehaviorTable  .Add(bhv3.UUID, bhv3);
        prg1.UUIDToBehaviorTable  .Add(bhv4.UUID, bhv4);
        prg1.UUIDToBehaviorTable  .Add(bhvANM.UUID, bhvANM);
        prg1.UUIDToStateTable     .Add(stt0.UUID, stt0);
        prg1.UUIDToStateTable     .Add(stt1.UUID,stt1);
        prg1.UUIDToStateTable     .Add(stt2.UUID,stt2);
        prg1.UUIDToStateTable     .Add(stt3.UUID,stt3);
        prg1.UUIDToStateTable     .Add(stt4.UUID,stt4);
        prg1.UUIDToStateTable     .Add(stt5.UUID,stt5);
        prg1.UUIDToTransitionTable.Add(trn12.UUID,trn12);
        prg1.UUIDToTransitionTable.Add(trn23.UUID,trn23);
        prg1.UUIDToTransitionTable.Add(trn34.UUID,trn34);
        prg1.UUIDToTransitionTable.Add(trn41.UUID,trn41);
        prg1.UUIDToTransitionTable.Add(trn51.UUID,trn51);
        prg1.UUIDToTransitionTable.Add(trn13.UUID,trn13);
        prg1.UUIDToTransitionTable.Add(trn21.UUID,trn21);
        prg1.UUIDToTransitionTable.Add(trn32.UUID,trn32);
        prg1.UUIDToTransitionTable.Add(trn05.UUID,trn05);
        prg1.StateStart = stt1;
        prg1.StateOmni  = stt0;
        
        return prg1;
      }
    }
    
    private static trProgram Example2 {
      get {
        trProgram    prg1  = new trProgram();
        prg1.UserFacingName = "no states";
        
        trBehavior   bhv1  = new trBehavior(trBehaviorType.COLOR_CYAN);
        trBehavior   bhv2  = new trBehavior(trBehaviorType.COLOR_RED);
        trBehavior   bhv3  = new trBehavior(trBehaviorType.DO_NOTHING);
        
        prg1.UUIDToBehaviorTable  .Clear();
        prg1.UUIDToStateTable     .Clear();
        prg1.UUIDToTransitionTable.Clear();
        prg1.UUIDToBehaviorTable  .Add(bhv1.UUID, bhv1);
        prg1.UUIDToBehaviorTable  .Add(bhv2.UUID, bhv2);
        prg1.UUIDToBehaviorTable  .Add(bhv3.UUID, bhv3);
        
        return prg1;
      }
    }
    
    private static trProgram Example3 {
      get {
        trProgram    prg1   = new trProgram();
        
        prg1.UserFacingName = "chase";
        
        trBehavior bhvIdl = new trBehavior(trBehaviorType.DO_NOTHING);
        trBehavior bhvFwd = new trBehavior(trBehaviorType.MOVE_F2   );
        trBehavior bhvTrn = new trBehavior(trBehaviorType.MOVE_L1   );
        trBehavior bhvStp = new trBehavior(trBehaviorType.MOVE_STOP );
        trBehavior bhvStn = new trBehavior(trBehaviorType.MOVE_LR0  );
        trBehavior bhvBak = new trBehavior(trBehaviorType.MOVE_B2   );
        prg1.UUIDToBehaviorTable.Add(bhvIdl.UUID, bhvIdl);
        prg1.UUIDToBehaviorTable.Add(bhvFwd.UUID, bhvFwd);
        prg1.UUIDToBehaviorTable.Add(bhvTrn.UUID, bhvTrn);
        prg1.UUIDToBehaviorTable.Add(bhvStp.UUID, bhvStp);
        prg1.UUIDToBehaviorTable.Add(bhvStn.UUID, bhvStn);
        prg1.UUIDToBehaviorTable.Add(bhvBak.UUID, bhvBak);
        
        
        trState sttOmn = new trState("Omni");
        sttOmn.Behavior = bhvIdl;
        prg1.UUIDToStateTable.Add(sttOmn.UUID, sttOmn);
        prg1.StateOmni = sttOmn;
        
        trState sttIdl = new trState("Idle");
        sttIdl.Behavior = bhvStp;
        prg1.UUIDToStateTable.Add(sttIdl.UUID, sttIdl);
        prg1.StateStart = sttIdl;
        
        trState sttFwd = new trState("Forward");
        sttFwd.Behavior = bhvFwd;
        prg1.UUIDToStateTable.Add(sttFwd.UUID,sttFwd);
        
        trState sttBak = new trState("Back");
        sttBak.Behavior = bhvBak;
        prg1.UUIDToStateTable.Add(sttBak.UUID,sttBak);
        
        trState sttTrn = new trState("Turn");
        sttTrn.Behavior = bhvTrn;
        prg1.UUIDToStateTable.Add(sttTrn.UUID,sttTrn);
        
        trState sttStn = new trState("StopTurn");
        sttStn.Behavior = bhvStn;
        prg1.UUIDToStateTable.Add(sttStn.UUID,sttStn);
        
        trState sttStp = new trState("Stop");
        sttStp.Behavior = bhvStp;
        prg1.UUIDToStateTable.Add(sttStp.UUID,sttStp);
        
        trTransition tran;
        
        tran = new trTransition();
        tran.StateSource = sttOmn;
        tran.StateTarget = sttIdl;
        tran.Trigger     = new trTrigger(trTriggerType.BUTTON_MAIN);
        tran.StateSource.AddOutgoingTransition(tran);
        prg1.UUIDToTransitionTable.Add(tran.UUID, tran);
        
        tran = new trTransition();
        tran.StateSource = sttIdl;
        tran.StateTarget = sttFwd;
        tran.Trigger     = new trTrigger(trTriggerType.CLAP);
        tran.StateSource.AddOutgoingTransition(tran);
        prg1.UUIDToTransitionTable.Add(tran.UUID, tran);
        
        tran = new trTransition();
        tran.StateSource = sttFwd;
        tran.StateTarget = sttBak;
        tran.Trigger     = new trTrigger(trTriggerType.DISTANCE_REAR_MIDDLE);
        tran.StateSource.AddOutgoingTransition(tran);
        prg1.UUIDToTransitionTable.Add(tran.UUID,tran );
        
        tran = new trTransition();
        tran.StateSource = sttBak;
        tran.StateTarget = sttTrn;
        tran.Trigger     = new trTrigger(trTriggerType.IMMEDIATE);
        tran.StateSource.AddOutgoingTransition(tran);
        prg1.UUIDToTransitionTable.Add(tran.UUID, tran);
        
        tran = new trTransition();
        tran.StateSource = sttStp;
        tran.StateTarget = sttFwd;
        tran.Trigger     = new trTrigger(trTriggerType.DISTANCE_REAR_NONE);
        tran.StateSource.AddOutgoingTransition(tran);
        prg1.UUIDToTransitionTable.Add(tran.UUID,tran );
        
        tran = new trTransition();
        tran.StateSource = sttFwd;
        tran.StateTarget = sttTrn;
        tran.Trigger     = new trTrigger(trTriggerType.DISTANCE_CENTER_MIDDLE);
        tran.StateSource.AddOutgoingTransition(tran);
        prg1.UUIDToTransitionTable.Add(tran.UUID,tran );
        
        tran = new trTransition();
        tran.StateSource = sttTrn;
        tran.StateTarget = sttStn;
        tran.Trigger     = new trTrigger(trTriggerType.DISTANCE_CENTER_NONE);
        tran.StateSource.AddOutgoingTransition(tran);
        prg1.UUIDToTransitionTable.Add(tran.UUID, tran);
        
        tran = new trTransition();
        tran.StateSource = sttStn;
        tran.StateTarget = sttFwd;
        tran.Trigger     = new trTrigger(trTriggerType.IMMEDIATE);
        tran.StateSource.AddOutgoingTransition(tran);
        prg1.UUIDToTransitionTable.Add(tran.UUID, tran);
        
        return prg1;  
      }
    }
    
    private static trProgram Example4 {
      get {
        trProgram    prg1   = new trProgram();
        
        prg1.UserFacingName = "back and forth";
        
        trBehavior bhvNop = new trBehavior(trBehaviorType.DO_NOTHING);
        trBehavior bhvStp = new trBehavior(trBehaviorType.MOVE_STOP );
        trBehavior bhvLft = new trBehavior(trBehaviorType.MOVE_L1   );
        trBehavior bhvRgt = new trBehavior(trBehaviorType.MOVE_R1   );
        prg1.UUIDToBehaviorTable.Add(bhvNop.UUID, bhvNop);
        prg1.UUIDToBehaviorTable.Add(bhvStp.UUID, bhvStp);
        prg1.UUIDToBehaviorTable.Add(bhvLft.UUID, bhvLft);
        prg1.UUIDToBehaviorTable.Add(bhvRgt.UUID, bhvRgt);
        
        trState sttOmn = new trState("Omni");
        sttOmn.Behavior = bhvNop;
        prg1.UUIDToStateTable.Add(sttOmn.UUID, sttOmn);
        prg1.StateOmni = sttOmn;
        
        trState sttIdl = new trState("Idle");
        sttIdl.Behavior = bhvStp;
        prg1.UUIDToStateTable.Add(sttIdl.UUID, sttIdl);
        prg1.StateStart = sttIdl;
        
        trState sttLft = new trState("Left");
        sttLft.Behavior = bhvLft;
        prg1.UUIDToStateTable.Add(sttLft.UUID, sttLft);
        
        trState sttRgt = new trState("Right");
        sttRgt.Behavior = bhvRgt;
        prg1.UUIDToStateTable.Add(sttRgt.UUID,sttRgt);
        
        trTransition tran;
        
        tran = new trTransition();
        tran.StateSource = sttOmn;
        tran.StateTarget = sttLft;
        tran.Trigger     = new trTrigger(trTriggerType.BUTTON_MAIN);
        tran.StateSource.AddOutgoingTransition(tran);
        prg1.UUIDToTransitionTable.Add(tran.UUID, tran);
        
        tran = new trTransition();
        tran.StateSource = sttIdl;
        tran.StateTarget = sttLft;
        tran.Trigger     = new trTrigger(trTriggerType.CLAP);
        tran.StateSource.AddOutgoingTransition(tran);
        prg1.UUIDToTransitionTable.Add(tran.UUID, tran);
        
        tran = new trTransition();
        tran.StateSource = sttLft;
        tran.StateTarget = sttRgt;
        tran.Trigger     = new trTrigger(trTriggerType.CLAP);
        tran.StateSource.AddOutgoingTransition(tran);
        prg1.UUIDToTransitionTable.Add(tran.UUID, tran);
        
        tran = new trTransition();
        tran.StateSource = sttRgt;
        tran.StateTarget = sttLft;
        tran.Trigger     = new trTrigger(trTriggerType.CLAP);
        tran.StateSource.AddOutgoingTransition(tran);
        prg1.UUIDToTransitionTable.Add(tran.UUID, tran);
        
        return prg1;  
      }
    }
    
    private static trProgram Example5 {
      get {
        trProgram    prg1   = new trProgram();
        
        prg1.UserFacingName = "pan to color";
        
        trBehavior bhvMp1 = new trBehavior(trBehaviorType.MAPSET);
        prg1.UUIDToBehaviorTable.Add(bhvMp1.UUID, bhvMp1);
        trMap map;

        bhvMp1.MapSet.Name = "head pan controls color and leds and tilt";
        
        map               = new trMap();
        map.Actuator      = new trActuator(trActuatorType.RGB_ALL_HUE);
        map.Sensor        = new trSensor  (trSensorType  .HEAD_PAN   );
        map.RangeSensor   = new wwRange(0.121f, 0.879f);
       // map.RangeActuator = new wwRange(0.000f, 1.000f);
        bhvMp1.MapSet.Maps.Add(map);

        // HEAD_TILT <-- HEAD_PAN        
        map               = new trMap();
        map.Actuator      = new trActuator(trActuatorType.HEAD_TILT);
        map.Sensor        = new trSensor  (trSensorType  .HEAD_PAN );
        map.RangeSensor   = new wwRange(0.121f, 0.879f);
       // map.RangeActuator = new wwRange(0.450f, 0.650f);
        bhvMp1.MapSet.Maps.Add(map);
  
        // HEAD_PAN <-- HEAD_TILT    
//        map               = new trMap();
//        map.Actuator      = new trActuator(trActuatorType.HEAD_PAN );
//        map.Sensor        = new trSensor  (trSensorType  .HEAD_TILT);
//        map.RangeSensor   = new wwRange(0.480f, 0.550f);
//        map.RangeActuator = new wwRange(0.121f, 0.879f);
//        bhvMp1.MapSet.Maps.Add(map);
        
        map               = new trMap();
        map.Actuator      = new trActuator(trActuatorType.LED_TOP  );
        map.Sensor        = new trSensor  (trSensorType  .HEAD_TILT);
        map.RangeSensor   = new wwRange(0.480f, 0.550f);
      //  map.RangeActuator = new wwRange(0.000f, 1.000f);
        map.InvertSensor  = false;
        bhvMp1.MapSet.Maps.Add(map);
        
        map               = new trMap();
        map.Actuator      = new trActuator(trActuatorType.LED_TAIL );
        map.Sensor        = new trSensor  (trSensorType  .HEAD_TILT);
        map.RangeSensor   = new wwRange(0.468f, 0.472f);
        map.RangeSensor   = new wwRange(0.470f, 0.470f);
 //       map.RangeActuator = new wwRange(0.000f, 1.000f);
        map.InvertSensor  = true;
        bhvMp1.MapSet.Maps.Add(map);
        
        trState stt1 = new trState("State1");
        stt1.Behavior = bhvMp1;
        prg1.UUIDToStateTable.Add(stt1.UUID, stt1);
        prg1.StateStart = stt1;
        
        return prg1;  
      }
    }
    
    private static trProgram Example6 {
      get {
        trProgram    prg1   = new trProgram();
        
        prg1.UserFacingName = "look at obstacle";
        
        trBehavior bhvMp1 = new trBehavior(trBehaviorType.MAPSET);
        prg1.UUIDToBehaviorTable.Add(bhvMp1.UUID, bhvMp1);
        trMap map;

        bhvMp1.MapSet.Name = "look at obstacle";
                
        map               = new trMap();
        map.Actuator      = new trActuator(trActuatorType.HEAD_PAN            );
        map.Sensor        = new trSensor  (trSensorType  .DISTANCE_FRONT_DELTA);
        map.RangeSensor   = new wwRange(0.000f, 1.000f);
     //   map.RangeActuator = new wwRange(0.300f, 0.700f);
        map.InvertSensor  = false;
        bhvMp1.MapSet.Maps.Add(map);

        map               = new trMap();
        map.Actuator      = new trActuator(trActuatorType.HEAD_TILT           );
        map.Sensor        = new trSensor  (trSensorType  .DISTANCE_FRONT      );
        map.RangeSensor   = new wwRange(0.000f, 1.000f);
     //   map.RangeActuator = new wwRange(0.000f, 0.500f);
        map.InvertSensor  = false;
        bhvMp1.MapSet.Maps.Add(map);
        
        trState stt1 = new trState("State1");
        stt1.Behavior = bhvMp1;
        prg1.UUIDToStateTable.Add(stt1.UUID, stt1);
        prg1.StateStart = stt1;
        
        return prg1;  
      }
    }

    private static trProgram Example7 {
      get {
        trProgram    prg1   = new trProgram();
        
        prg1.UserFacingName = "test pitch and roll";
        
        trBehavior bhvMp1 = new trBehavior(trBehaviorType.MAPSET);
        prg1.UUIDToBehaviorTable.Add(bhvMp1.UUID, bhvMp1);
        trMap map;
        
        bhvMp1.MapSet.Name = "test pitch and roll";
        
        map               = new trMap();
        map.Actuator      = new trActuator(trActuatorType.RGB_ALL_HUE           );
        map.Sensor        = new trSensor  (trSensorType  .ROLL                  );
        map.RangeSensor   = new wwRange(0.000f, 1.000f);
     //   map.RangeActuator = new wwRange(0.000f, 1.000f);
        map.InvertSensor  = false;
        bhvMp1.MapSet.Maps.Add(map);
        
        map               = new trMap();
        map.Actuator      = new trActuator(trActuatorType.RGB_ALL_VAL           );
        map.Sensor        = new trSensor  (trSensorType  .PITCH                 );
        map.RangeSensor   = new wwRange(0.000f, 1.000f);
      //  map.RangeActuator = new wwRange(0.000f, 1.000f);
        map.InvertSensor  = false;
        bhvMp1.MapSet.Maps.Add(map);
        
        trState stt1 = new trState("State1");
        stt1.Behavior = bhvMp1;
        prg1.UUIDToStateTable.Add(stt1.UUID, stt1);
        prg1.StateStart = stt1;
        
        return prg1;  
      }
    }
    
    private static trProgram ExampleLinAng {
      get {
        trProgram    prg1   = new trProgram();
        
        prg1.UserFacingName = "test lin/ang";
        
        trBehavior bhvMp1 = new trBehavior(trBehaviorType.MAPSET);
        prg1.UUIDToBehaviorTable.Add(bhvMp1.UUID, bhvMp1);
        trMap map;
        
        bhvMp1.MapSet.Name = "lin/ang";
        
        map               = new trMap();
        map.Actuator      = new trActuator(trActuatorType.WHEEL_L               );
        map.Sensor        = new trSensor  (trSensorType  .ROLL                  );
        map.RangeSensor   = new wwRange(0.000f, 1.000f);
    //    map.RangeActuator = new wwRange(0.900f, 0.900f);
        map.InvertSensor  = false;
        bhvMp1.MapSet.Maps.Add(map);
        
        map               = new trMap();
        map.Actuator      = new trActuator(trActuatorType.WHEEL_R               );
        map.Sensor        = new trSensor  (trSensorType  .PITCH                 );
        map.RangeSensor   = new wwRange(0.000f, 1.000f);
    //    map.RangeActuator = new wwRange(0.700f, 0.700f);
        map.InvertSensor  = false;
        bhvMp1.MapSet.Maps.Add(map);
        
        trState stt1 = new trState("State1");
        stt1.Behavior = bhvMp1;
        prg1.UUIDToStateTable.Add(stt1.UUID, stt1);
        prg1.StateStart = stt1;
        
        return prg1;  
      }
    }
    
    private static trProgram ExampleParamTime {
      get {
        trProgram    prg1   = new trProgram();
        
        prg1.UserFacingName = "test parametric trigger: time";
        
        
        trBehavior   bhv1  = new trBehavior(trBehaviorType.COLOR_CYAN);
        trBehavior   bhv2  = new trBehavior(trBehaviorType.COLOR_RED);
        prg1.UUIDToBehaviorTable  .Add(bhv1.UUID, bhv1);
        prg1.UUIDToBehaviorTable  .Add(bhv2.UUID, bhv2);
        
        trState      stt1 = new trState("state1");
        stt1.Behavior     = bhv1;
        trState      stt2 = new trState("state2");
        stt2.Behavior     = bhv2;
        prg1.UUIDToStateTable.Add(stt1.UUID, stt1);
        prg1.UUIDToStateTable.Add(stt2.UUID, stt2);
        prg1.StateStart   = stt1;
        
        
        trTransition trn1  = new trTransition();
        trTransition trn2  = new trTransition();
        prg1.UUIDToTransitionTable.Add(trn1.UUID, trn1);
        trn1.StateSource   = stt1;
        trn1.StateTarget   = stt2;
        trn1.Trigger       = new trTrigger(trTriggerType.TIME, 2.0f);
        stt1.AddOutgoingTransition(trn1);
        trn2.StateSource   = stt2;
        trn2.StateTarget   = stt1;
        trn2.Trigger       = new trTrigger(trTriggerType.TIME, 0.5f);
        stt2.AddOutgoingTransition(trn2);
        
        
        return prg1;  
      }
    }

    private static trProgram ExampleParamTravel {
      get {
        trProgram    prg1   = new trProgram();
        
        prg1.UserFacingName = "test parametric trigger: travel linear and angular";
        
        
        trBehavior   bhv1  = new trBehavior(trBehaviorType.COLOR_CYAN);
        trBehavior   bhv2  = new trBehavior(trBehaviorType.COLOR_RED);
        prg1.UUIDToBehaviorTable  .Add(bhv1.UUID, bhv1);
        prg1.UUIDToBehaviorTable  .Add(bhv2.UUID, bhv2);
        
        trState      stt1 = new trState("state1");
        stt1.Behavior     = bhv1;
        trState      stt2 = new trState("state2");
        stt2.Behavior     = bhv2;
        prg1.UUIDToStateTable.Add(stt1.UUID, stt1);
        prg1.UUIDToStateTable.Add(stt2.UUID, stt1);
        prg1.StateStart   = stt1;
        
        
        trTransition trn1  = new trTransition();
        trTransition trn2  = new trTransition();
        prg1.UUIDToTransitionTable.Add(trn1.UUID, trn1);
        trn1.StateSource   = stt1;
        trn1.StateTarget   = stt2;
        trn1.Trigger       = new trTrigger(trTriggerType.TRAVEL_LINEAR, 1.0f);
        stt1.AddOutgoingTransition(trn1);
        trn2.StateSource   = stt2;
        trn2.StateTarget   = stt1;
        trn2.Trigger       = new trTrigger(trTriggerType.TRAVEL_ANGULAR, 1.0f);
        stt2.AddOutgoingTransition(trn2);
        
        return prg1;  
      }
    }

    private static trProgram ExampleParamSensTravel {
      get {
        trProgram    prg1   = new trProgram();
        
        prg1.UserFacingName = "test parameterized sensor: travel";
        
        trBehavior bhvMp1 = new trBehavior(trBehaviorType.MAPSET);
        prg1.UUIDToBehaviorTable.Add(bhvMp1.UUID, bhvMp1);
        trMap map;
        
        bhvMp1.MapSet.Name = "param_travel";
        
        map                       = new trMap();
        map.Actuator              = new trActuator(trActuatorType.HEAD_PAN              );
        map.Sensor                = new trSensor  (trSensorType  .TRAVEL_LINEAR_100     );
        map.Sensor.ParameterValue = 10.0f;
        map.RangeSensor           = new wwRange(0.000f, 1.000f);
    //    map.RangeActuator         = new wwRange(0.000f, 1.000f);
        map.InvertSensor          = false;
        bhvMp1.MapSet.Maps.Add(map);
        
        map                       = new trMap();
        map.Actuator              = new trActuator(trActuatorType.RGB_ALL_VAL           );
        map.Sensor                = new trSensor  (trSensorType  .TRAVEL_ANGULAR_180    );
        map.Sensor.ParameterValue = 90.0f;
        map.RangeSensor           = new wwRange(0.000f, 1.000f);
    //    map.RangeActuator         = new wwRange(0.000f, 1.000f);
        map.InvertSensor          = false;
        bhvMp1.MapSet.Maps.Add(map);
        
        trState stt1              = new trState("State1");
        stt1.Behavior             = bhvMp1;
        prg1.UUIDToStateTable.Add(stt1.UUID, stt1);
        prg1.StateStart           = stt1;
        
        return prg1;  
      }
    }
  }
}
