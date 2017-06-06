using UnityEngine;
using System.Collections.Generic;

namespace Turing {
  public class trToFirmware {

    // word of warning: casting a float or double to an unsigned type (eg ushort, uint, byte) has undefined results.
    // instead, cast first to a signed integral type. at least, when compiling to C/C++.
    // I haven't been able to determine behavior for C# itself.
    
    public const string cFirmwareRequired            = "X.5.6+";
    public const int    cBytesPerHeader              = 8;
    public const int    cBytesPerTransition          = 5;
    public const byte   cInvalidIndex                = 0xff;
    public const byte   cFileFormatVersion           = 1;
    public const int    cOutgoingTransitionsPerState = 6;
    public const int    cBytesPerBehaviorMakerMap    = 10;
    public const int    cBytesPerState               = (cBytesPerTransition * cOutgoingTransitionsPerState) + 2;
    public const float  cCMtoMM                      = 10.0f;
    public const float  cMMtoCM                      =  0.1f;
    public const float  cDEGtoCENTIRAD               = Mathf.Deg2Rad * 100.0f;
    public const float  cCENTIRADtoDEG               = Mathf.Rad2Deg *   0.01f;
    
    private trProgram theProgram                                  = null;
    private byte[]    behaviorsTable                              = new byte[0];
    private byte[]    rangesTable                                 = new byte[0];
    private byte[]    threshTable                                 = new byte[0];
    private Dictionary<trState, ushort> behaviorOffsetsInTable    = new Dictionary<trState, ushort>();
    
    // maps from trigger types to trigger specifier subtypes
    private Dictionary<trTriggerType, triggerSubtypeDist_t> mapTriggerSubtypeDistance = null;
    private Dictionary<trTriggerType, triggerSubtypeBeacon_t  > mapTriggerSubtypeBeacon   = null;
    
    private piRobotType theRobotType = piRobotType.UNKNOWN;
    
    public byte[] toFirmware(trProgram trPrg, piRobotType robotType) {
      // PreProcess state machine. This is destructive, so we make a copy.
      trProgram trPrgCopy = trFactory.FromJson<trProgram>(trPrg.ToJson());
      
      // order matters!!
      eliminateLaterEvaluatedTriggersInTheFaceOfImmediate(trPrgCopy);
      
      // oxe todo: with normalized transition ordering, we put Auto after Random, so i don't think this next step is needed.
      //           leaving in for now.
      eliminateAutoInTheFaceOf_Random                    (trPrgCopy);
      replaceRandomWithAutoPlusRandom                    (trPrgCopy);
      convertImmediateToTime                             (trPrgCopy);
      trPrgCopy.NormalizeTransitionOrdering              ();         // do this last, because previous ones are adding transitions.
       
      trPrgCopy.Validate();
            
      return toFirmwareUnfiltered(trPrgCopy, robotType);
    }
    
    public byte[] toFirmwareUnfiltered(trProgram trPrg, piRobotType robotType) {
    
      if (trPrg == null) {
        WWLog.logError("null program.");
        return null;
      }
    
      theProgram = trPrg;
      theRobotType = robotType;
      
      buildBehaviorsTable();
      buildRangesTable();
      buildThreshTable();
    
      byte[] ret           = null;
      byte[] baHeader      = toByteArrayHeader();
      byte[] baStates      = toByteArrayStates();
      byte[] baBehaviors   = behaviorsTable;
      byte[] baRangesTable = rangesTable;
      byte[] baThreshTable = threshTable;
      
      ret = wwBA.append(baHeader      , ret);
      ret = wwBA.append(baStates      , ret);
      ret = wwBA.append(baBehaviors   , ret);
      ret = wwBA.append(baRangesTable , ret);
      ret = wwBA.append(baThreshTable , ret);
      
//      WWLog.logInfo("length header       : " + baHeader   .Length);
//      WWLog.logInfo("length states       : " + baStates   .Length);
//      WWLog.logInfo("inferred state count: " + (float)baStates   .Length / (float)cBytesPerState);
//      WWLog.logInfo("length behaviors    : " + baBehaviors.Length);
      
      
      WWLog.logWarn("state machine dump\n"
        + "0 header:\n"      + piStringUtil.byteArrayToString2(baHeader, 0) + "\n"
        + "0 states:\n"      + piStringUtil.byteArrayToString2(baStates, baHeader.Length) + "\n"
        + "0 behaviors:\n"   + piStringUtil.byteArrayToString2(behaviorsTable, baHeader.Length + baStates.Length) + "\n"
        + "0 ranges:\n"      + piStringUtil.byteArrayToString2(rangesTable, behaviorsTable.Length + baHeader.Length + baStates.Length) + "\n"
        + "0 threshholds:\n" + piStringUtil.byteArrayToString2(threshTable, rangesTable.Length + behaviorsTable.Length + baHeader.Length + baStates.Length) + "\n"
        + "0---------------"
        );

//      WWLog.logWarn("\n0 base-64: " + System.Convert.ToBase64String(ret));
      
      // validate
      int expectedOffsetToFirstBehavior = baHeader.Length + baStates.Length;
        
      if (expectedOffsetToFirstBehavior != fileOffsetToFirstBehavior()) {
        WWLog.logError("Unexpected offset to first behavior: expected " + expectedOffsetToFirstBehavior + " but actually " + fileOffsetToFirstBehavior());
      }
      
      return ret;
    }
    
    private void buildBehaviorsTable() {
      // clear out old data
      behaviorsTable = new byte[0];
      behaviorOffsetsInTable = new Dictionary<trState, ushort>();
      
      foreach(trState trStt in theProgram.UUIDToStateTable.Values) {
        byte[] baBhv = toByteArrayBehavior(trStt);
//        WWLog.logWarn("000 behavior: " + trStt.Behavior.Type.ToString() + "\n" + piStringUtil.byteArrayToString2(baBhv) + "\n\n");
        behaviorOffsetsInTable[trStt] = (ushort)behaviorsTable.Length;
        behaviorsTable = wwBA.append(baBhv, behaviorsTable);
      }
      
      WWLog.logInfo("built table of " + theProgram.UUIDToBehaviorTable.Count + " behaviors");
    }
    
    private void buildRangesTable() {
      byte[] val = null;
      
      val = wwBA.append(toByteArrayTriggerType(trTriggerType.DISTANCE_LEFT_NONE    , cCMtoMM * trTrigger.DistThreshFarNone, 0x7FFF                               ), val);
      val = wwBA.append(toByteArrayTriggerType(trTriggerType.DISTANCE_CENTER_NONE  , cCMtoMM * trTrigger.DistThreshFarNone, 0x7FFF                               ), val);
      val = wwBA.append(toByteArrayTriggerType(trTriggerType.DISTANCE_RIGHT_NONE   , cCMtoMM * trTrigger.DistThreshFarNone, 0x7FFF                               ), val);
      val = wwBA.append(toByteArrayTriggerType(trTriggerType.DISTANCE_REAR_NONE    , cCMtoMM * trTrigger.DistThreshFarNone, 0x7FFF                               ), val);
      
      val = wwBA.append(toByteArrayTriggerType(trTriggerType.DISTANCE_LEFT_FAR     , cCMtoMM * trTrigger.DistThreshNearFar, cCMtoMM * trTrigger.DistThreshFarNone), val);
      val = wwBA.append(toByteArrayTriggerType(trTriggerType.DISTANCE_CENTER_FAR   , cCMtoMM * trTrigger.DistThreshNearFar, cCMtoMM * trTrigger.DistThreshFarNone), val);
      val = wwBA.append(toByteArrayTriggerType(trTriggerType.DISTANCE_RIGHT_FAR    , cCMtoMM * trTrigger.DistThreshNearFar, cCMtoMM * trTrigger.DistThreshFarNone), val);
      val = wwBA.append(toByteArrayTriggerType(trTriggerType.DISTANCE_REAR_FAR     , cCMtoMM * trTrigger.DistThreshNearFar, cCMtoMM * trTrigger.DistThreshFarNone), val);
      
      val = wwBA.append(toByteArrayTriggerType(trTriggerType.DISTANCE_LEFT_NEAR    , 0.0f                                 , cCMtoMM * trTrigger.DistThreshNearFar), val);
      val = wwBA.append(toByteArrayTriggerType(trTriggerType.DISTANCE_CENTER_NEAR  , 0.0f                                 , cCMtoMM * trTrigger.DistThreshNearFar), val);
      val = wwBA.append(toByteArrayTriggerType(trTriggerType.DISTANCE_RIGHT_NEAR   , 0.0f                                 , cCMtoMM * trTrigger.DistThreshNearFar), val);
      val = wwBA.append(toByteArrayTriggerType(trTriggerType.DISTANCE_REAR_NEAR    , 0.0f                                 , cCMtoMM * trTrigger.DistThreshNearFar), val);
      
      if (val.Length != 12 * 6) {
        WWLog.logError("unexpected range table length! expected " + 12 * 6 + " actual: " + val.Length);
      }
      
      rangesTable = null;
      rangesTable = wwBA.append(wwBA.toByteArray1((byte)(val.Length / 6)), rangesTable);
      rangesTable = wwBA.append(wwBA.toByteArrayV(val)                   , rangesTable);
    }
    
    private void buildThreshTable() {
      byte[] val = null;
      
      val = wwBA.append(toByteArrayTriggerType(trTriggerType.LEAN_BACKWARD     , (          trTrigger.LeanTreshold     )), val);
      val = wwBA.append(toByteArrayTriggerType(trTriggerType.LEAN_FORWARD      , (          trTrigger.LeanTreshold     )), val);
      val = wwBA.append(toByteArrayTriggerType(trTriggerType.LEAN_LEFT         , (          trTrigger.LeanTreshold     )), val);
      val = wwBA.append(toByteArrayTriggerType(trTriggerType.LEAN_RIGHT        , (          trTrigger.LeanTreshold     )), val);
      val = wwBA.append(toByteArrayTriggerType(trTriggerType.LEAN_UPSIDE_UP    , (          trTrigger.LeanTreshold     )), val);
      val = wwBA.append(toByteArrayTriggerType(trTriggerType.LEAN_UPSIDE_DOWN  , (          trTrigger.LeanTreshold     )), val);
      
      val = wwBA.append(toByteArrayTriggerType(trTriggerType.TRAVELING_FORWARD , (cCMtoMM * trTrigger.TravelingTreshold)), val);
      val = wwBA.append(toByteArrayTriggerType(trTriggerType.TRAVELING_BACKWARD, (cCMtoMM * trTrigger.TravelingTreshold)), val);
      val = wwBA.append(toByteArrayTriggerType(trTriggerType.TRAVELING_STOPPED , (cCMtoMM * trTrigger.TravelingTreshold)), val);
      
      if (val.Length != 9 * 4) {
        WWLog.logError("unexpected thresh table length! expected: " + 9 * 4 + " actual: " + val.Length);
      }
      
      threshTable = null;
      threshTable = wwBA.append(wwBA.toByteArray1((byte)(val.Length / 4)), threshTable);
      threshTable = wwBA.append(wwBA.toByteArrayV(val)                   , threshTable);
    }
    
    private byte[] toByteArrayHeader() {
      byte   fileFormatVersion   = cFileFormatVersion;
      ushort offsetToStateStart  = cBytesPerHeader;
      byte   indexOfStateOmni    = indexOfStateInFile(theProgram.StateOmni);
      ushort offsetToRangesTable = (ushort)(fileOffsetToFirstBehavior() + behaviorsTable.Length);
      ushort offsetToThreshTable = (ushort)(offsetToRangesTable + rangesTable.Length);
      
      List<byte[]> components = new List<byte[]>();
      
      components.Add(wwBA.toByteArray1(fileFormatVersion  ));
      components.Add(wwBA.toByteArray2(offsetToStateStart ));
      components.Add(wwBA.toByteArray1(indexOfStateOmni   ));
      components.Add(wwBA.toByteArray2(offsetToRangesTable));
      components.Add(wwBA.toByteArray2(offsetToThreshTable));
      
      byte[] ret = wwBA.toByteArrayV(components);
      
      if (ret.Length != cBytesPerHeader) {
        WWLog.logError("unexpected header length: " + ret.Length);
      }
      
      return ret;
    }
    
    private byte[] toByteArrayStates() {
      byte[] ret = null;
      
      foreach (trState trs in theProgram.UUIDToStateTable.Values) {
        ret = wwBA.append(toByteArrayState(trs), ret);
      }
      
      if (ret.Length != cBytesPerState * theProgram.UUIDToStateTable.Count) {
        WWLog.logError("Unexpected states length. expected " + cBytesPerState + " but actually " + ret.Length);
      }
      
      return ret;
    }
    
    private byte[] toByteArrayState(trState trStt) {
      
      ushort offsetToBehavior = fileOffsetToBehavior(trStt);
      
      byte[] transitions = null;
      
      if (trStt.OutgoingTransitions.Count > cOutgoingTransitionsPerState) {
        WWLog.logError("Too many outgoing transitions for state. will only export " + cOutgoingTransitionsPerState + " but state has " + trStt.OutgoingTransitions.Count + ", " + trStt.ToString());
      }
      
      for (int n = 0; n < cOutgoingTransitionsPerState; ++n) {
        trTransition trTrn = null;
        if (n < trStt.OutgoingTransitions.Count) {
          trTrn = trStt.OutgoingTransitions[n];
        }
        transitions = wwBA.append(toByteArrayTransition(trTrn), transitions);
      }
      
      
      byte[] ret = null;
      ret = wwBA.append(wwBA.toByteArray2(offsetToBehavior), ret);
      ret = wwBA.append(wwBA.toByteArrayV(transitions     ), ret);
      
      if (ret.Length != cBytesPerState) {
        WWLog.logError("unexpected state length! expected: " + cBytesPerState + " actual: " + ret.Length);
      }
      
      return ret;
    }
    
    private byte[] toByteArrayTransition(trTransition trTrn) {
      if (trTrn == null) {
        // 0x0000****** indicates no such transition by virtue of being the invalid trigger.
        byte[] retn = wwBA.toByteArray5(0x0000FFFFFF);
        if (retn.Length != cBytesPerTransition) {
          WWLog.logError("unexpected bytecount for null transition! expected: " + cBytesPerTransition + " actual: " + retn.Length);
        }
        return retn;
      }
      
      ushort triggerTypeAndSubType = triggerSpecifier(trTrn.Trigger);
      ushort userParam             = 0xFFFF;
      byte   nextStateIndex        = 0x00;
      
      switch (trTrn.Trigger.Type) {
        case trTriggerType.TIME:
          userParam    = (ushort)(short)(trTrn.Trigger.ParameterValue * 10.0f);
          break;
        case trTriggerType.TIME_LONG:
          userParam    = (ushort)(short)(trTrn.Trigger.ParameterValue / 2.0f);
          break;
        case trTriggerType.TIME_RANDOM:
          userParam    = (ushort)(short)(trTrn.Trigger.ParameterValue * 10.0f);
          break;
        case trTriggerType.TRAVEL_LINEAR:
          userParam    = (ushort)(short)(trTrn.Trigger.ParameterValue * cCMtoMM);
          break;
        case trTriggerType.TRAVEL_ANGULAR:
          userParam    = (ushort)(short)(trTrn.Trigger.ParameterValue * cDEGtoCENTIRAD);
          break;
      }
      
      nextStateIndex = indexOfStateInFile(trTrn.StateTarget);
      
      byte[] ret = null;
      ret = wwBA.append(wwBA.toByteArray2(triggerTypeAndSubType), ret);
      ret = wwBA.append(wwBA.toByteArray2(      userParam      ), ret);
      ret = wwBA.append(wwBA.toByteArray1(      nextStateIndex ), ret);
      
      if (ret.Length != cBytesPerTransition) {
        WWLog.logError("unexpected transition size! expected: " + cBytesPerTransition + " actual: " + ret.Length);
      }
      
      return ret;
    }
    
    private ushort triggerSpecifier(trTrigger trTrg) {
      triggerType_t triggerType;
      ushort      triggerSubType;
      
      switch (trTrg.Type) {
      default:
        return triggerSpecifierSimple(trTrg.Type);
      case trTriggerType.DISTANCE_SET:
        triggerType     = triggerType_t.TRIGGER_TYPE_DIST;
        triggerSubType  = triggerSetToSubtypeMask(MapTriggerSubtypeDistance, trTrg.TriggerSet);
        break;
      case trTriggerType.BEACON_SET:
        triggerType     = triggerType_t.TRIGGER_TYPE_BEACON;
        triggerSubType  = triggerSetToSubtypeMask(MapTriggerSubtypeBeacon  , trTrg.TriggerSet);
        break;
      }
      
      if (((ushort)triggerType & ~(0x0f)) != 0) {
        WWLog.logError("invalid trigger Type: " + triggerType.ToString());
      }
      
      if ((triggerSubType & ~(0x0fff)) != 0) {
        WWLog.logError("invalid trigger SubType: " + triggerSubType);
      }
      
      ushort ret = (ushort)(((ushort)triggerType << 12) | triggerSubType);
      
      return ret;
    }
    
    private static ushort triggerSpecifierSimple(trTriggerType trTT) {
      triggerType_t triggerType;
      ushort      triggerSubType;
      switch (trTT) {
      default:
        triggerType    = triggerType_t.TRIGGER_TYPE_INVALID;
        triggerSubType = 0x000;
        WWLog.logError("unhandled trigger type: " + trTT.ToString());
        break;
        
        
      case trTriggerType.BUTTON_MAIN:
        triggerType    = triggerType_t.TRIGGER_TYPE_BUTTON;
        triggerSubType = (ushort)triggerSubtypeButton_t.TRIGGER_SUBTYPE_BUTTON_DN_MAIN;
        break;
      case trTriggerType.BUTTON_MAIN_NOT:
        triggerType    = triggerType_t.TRIGGER_TYPE_BUTTON;
        triggerSubType = (ushort)triggerSubtypeButton_t.TRIGGER_SUBTYPE_BUTTON_UP_MAIN;
        break;
      case trTriggerType.BUTTON_1:
        triggerType    = triggerType_t.TRIGGER_TYPE_BUTTON;
        triggerSubType = (ushort)triggerSubtypeButton_t.TRIGGER_SUBTYPE_BUTTON_DN_1;
        break;
      case trTriggerType.BUTTON_1_NOT:
        triggerType    = triggerType_t.TRIGGER_TYPE_BUTTON;
        triggerSubType = (ushort)triggerSubtypeButton_t.TRIGGER_SUBTYPE_BUTTON_UP_1;
        break;
      case trTriggerType.BUTTON_2:
        triggerType    = triggerType_t.TRIGGER_TYPE_BUTTON;
        triggerSubType = (ushort)triggerSubtypeButton_t.TRIGGER_SUBTYPE_BUTTON_DN_2;
        break;
      case trTriggerType.BUTTON_2_NOT:
        triggerType    = triggerType_t.TRIGGER_TYPE_BUTTON;
        triggerSubType = (ushort)triggerSubtypeButton_t.TRIGGER_SUBTYPE_BUTTON_UP_2;
        break;
      case trTriggerType.BUTTON_3:
        triggerType    = triggerType_t.TRIGGER_TYPE_BUTTON;
        triggerSubType = (ushort)triggerSubtypeButton_t.TRIGGER_SUBTYPE_BUTTON_DN_3;
        break;
      case trTriggerType.BUTTON_3_NOT:
        triggerType    = triggerType_t.TRIGGER_TYPE_BUTTON;
        triggerSubType = (ushort)triggerSubtypeButton_t.TRIGGER_SUBTYPE_BUTTON_UP_3;
        break;
      case trTriggerType.BUTTON_ANY:
        triggerType    = triggerType_t.TRIGGER_TYPE_BUTTON;
        triggerSubType = (ushort)triggerSubtypeButton_t.TRIGGER_SUBTYPE_BUTTON_DN_ANY;
        break;
      case trTriggerType.BUTTON_NONE:
        triggerType    = triggerType_t.TRIGGER_TYPE_BUTTON;
        triggerSubType = (ushort)triggerSubtypeButton_t.TRIGGER_SUBTYPE_BUTTON_UP_ALL;
        break;
        
        
      case trTriggerType.TIME:
        triggerType    = triggerType_t.TRIGGER_TYPE_TIMER;
        triggerSubType = (ushort)triggerSubtypeTimer_t.TRIG_SUBTYPE_TIMER_VALID;
        break;
      case trTriggerType.TIME_LONG:
        triggerType    = triggerType_t.TRIGGER_TYPE_LONG_TIME;
        triggerSubType = (ushort)triggerSubtypeTimer_t.TRIG_SUBTYPE_TIMER_VALID;
        break;
        
      case trTriggerType.DISTANCE_LEFT_NONE:
        triggerType    = triggerType_t.TRIGGER_TYPE_DIST;
        triggerSubType = (ushort)triggerSubtypeDist_t.TRIGGER_SUBTYPE_DIST_LEFT_NONE;
        break;
      case trTriggerType.DISTANCE_LEFT_FAR:
        triggerType    = triggerType_t.TRIGGER_TYPE_DIST;
        triggerSubType = (ushort)triggerSubtypeDist_t.TRIGGER_SUBTYPE_DIST_LEFT_FAR;
        break;
      case trTriggerType.DISTANCE_LEFT_NEAR:
        triggerType    = triggerType_t.TRIGGER_TYPE_DIST;
        triggerSubType = (ushort)triggerSubtypeDist_t.TRIGGER_SUBTYPE_DIST_LEFT_NEAR;
        break;
      case trTriggerType.DISTANCE_CENTER_NONE:
        triggerType    = triggerType_t.TRIGGER_TYPE_DIST;
        triggerSubType = (ushort)triggerSubtypeDist_t.TRIGGER_SUBTYPE_DIST_FRONT_NONE;
        break;
      case trTriggerType.DISTANCE_CENTER_FAR:
        triggerType    = triggerType_t.TRIGGER_TYPE_DIST;
        triggerSubType = (ushort)triggerSubtypeDist_t.TRIGGER_SUBTYPE_DIST_FRONT_FAR;
        break;
      case trTriggerType.DISTANCE_CENTER_NEAR:
        triggerType    = triggerType_t.TRIGGER_TYPE_DIST;
        triggerSubType = (ushort)triggerSubtypeDist_t.TRIGGER_SUBTYPE_DIST_FRONT_NEAR;
        break;
      case trTriggerType.DISTANCE_RIGHT_NONE:
        triggerType    = triggerType_t.TRIGGER_TYPE_DIST;
        triggerSubType = (ushort)triggerSubtypeDist_t.TRIGGER_SUBTYPE_DIST_RIGHT_NONE;
        break;
      case trTriggerType.DISTANCE_RIGHT_FAR:
        triggerType    = triggerType_t.TRIGGER_TYPE_DIST;
        triggerSubType = (ushort)triggerSubtypeDist_t.TRIGGER_SUBTYPE_DIST_RIGHT_FAR;
        break;
      case trTriggerType.DISTANCE_RIGHT_NEAR:
        triggerType    = triggerType_t.TRIGGER_TYPE_DIST;
        triggerSubType = (ushort)triggerSubtypeDist_t.TRIGGER_SUBTYPE_DIST_RIGHT_NEAR;
        break;
      case trTriggerType.DISTANCE_REAR_NONE:
        triggerType    = triggerType_t.TRIGGER_TYPE_DIST;
        triggerSubType = (ushort)triggerSubtypeDist_t.TRIGGER_SUBTYPE_DIST_REAR_NONE;
        break;
      case trTriggerType.DISTANCE_REAR_FAR:
        triggerType    = triggerType_t.TRIGGER_TYPE_DIST;
        triggerSubType = (ushort)triggerSubtypeDist_t.TRIGGER_SUBTYPE_DIST_REAR_FAR;
        break;
      case trTriggerType.DISTANCE_REAR_NEAR:
        triggerType    = triggerType_t.TRIGGER_TYPE_DIST;
        triggerSubType = (ushort)triggerSubtypeDist_t.TRIGGER_SUBTYPE_DIST_REAR_NEAR;
        break;
        
        
      case trTriggerType.LEAN_BACKWARD:
        triggerType    = triggerType_t.TRIGGER_TYPE_ACCEL;
        triggerSubType = (ushort)triggerSubtypeAccel_t.TRIGGER_SUBTYPE_ACCEL_LEAN_BACKWARD;
        break;
      case trTriggerType.LEAN_FORWARD:
        triggerType    = triggerType_t.TRIGGER_TYPE_ACCEL;
        triggerSubType = (ushort)triggerSubtypeAccel_t.TRIGGER_SUBTYPE_ACCEL_LEAN_FORWARD;
        break;
      case trTriggerType.LEAN_LEFT:
        triggerType    = triggerType_t.TRIGGER_TYPE_ACCEL;
        triggerSubType = (ushort)triggerSubtypeAccel_t.TRIGGER_SUBTYPE_ACCEL_LEAN_LEFT;
        break;
      case trTriggerType.LEAN_RIGHT:
        triggerType    = triggerType_t.TRIGGER_TYPE_ACCEL;
        triggerSubType = (ushort)triggerSubtypeAccel_t.TRIGGER_SUBTYPE_ACCEL_LEAN_RIGHT;
        break;
      case trTriggerType.LEAN_UPSIDE_DOWN:
        triggerType    = triggerType_t.TRIGGER_TYPE_ACCEL;
        triggerSubType = (ushort)triggerSubtypeAccel_t.TRIGGER_SUBTYPE_ACCEL_UPSIDE_DOWN;
        break;
      case trTriggerType.LEAN_UPSIDE_UP:
        triggerType    = triggerType_t.TRIGGER_TYPE_ACCEL;
        triggerSubType = (ushort)triggerSubtypeAccel_t.TRIGGER_SUBTYPE_ACCEL_UPSIDE_UP;
        break;
      case trTriggerType.DROP:
        triggerType    = triggerType_t.TRIGGER_TYPE_ACCEL;
        triggerSubType = (ushort)triggerSubtypeAccel_t.TRIGGER_SUBTYPE_ACCEL_DROP;
        break;
      case trTriggerType.SHAKE:
        triggerType    = triggerType_t.TRIGGER_TYPE_ACCEL;
        triggerSubType = (ushort)triggerSubtypeAccel_t.TRIGGER_SUBTYPE_ACCEL_SHAKE;
        break;
      case trTriggerType.SLIDE_X_POS:
        triggerType    = triggerType_t.TRIGGER_TYPE_ACCEL;
        triggerSubType = (ushort)triggerSubtypeAccel_t.TRIGGER_SUBTYPE_ACCEL_SLIDE_FWD;
        break;
      case trTriggerType.SLIDE_X_NEG:
        triggerType    = triggerType_t.TRIGGER_TYPE_ACCEL;
        triggerSubType = (ushort)triggerSubtypeAccel_t.TRIGGER_SUBTYPE_ACCEL_SLIDE_BACKWARD;
        break;
      case trTriggerType.SLIDE_Y_POS:
        triggerType    = triggerType_t.TRIGGER_TYPE_ACCEL;
        triggerSubType = (ushort)triggerSubtypeAccel_t.TRIGGER_SUBTYPE_ACCEL_SLIDE_LEFT;
        break;
      case trTriggerType.SLIDE_Y_NEG:
        triggerType    = triggerType_t.TRIGGER_TYPE_ACCEL;
        triggerSubType = (ushort)triggerSubtypeAccel_t.TRIGGER_SUBTYPE_ACCEL_SLIDE_RIGHT;
        break;
      case trTriggerType.SLIDE_Z_POS:
        triggerType    = triggerType_t.TRIGGER_TYPE_ACCEL;
        triggerSubType = (ushort)triggerSubtypeAccel_t.TRIGGER_SUBTYPE_ACCEL_SLIDE_UP;
        break;
      case trTriggerType.SLIDE_Z_NEG:
        triggerType    = triggerType_t.TRIGGER_TYPE_ACCEL;
        triggerSubType = (ushort)triggerSubtypeAccel_t.TRIGGER_SUBTYPE_ACCEL_SLIDE_DOWN;
        break;
        
        
      case trTriggerType.TRAVEL_LINEAR:
        triggerType    = triggerType_t.TRIGGER_TYPE_DISTANCE_TRAVELED;
        triggerSubType = (ushort)TriggerSubtypeDistanceTraveled_t.TRIGGER_SUBTYPE_DISTANCE_TRAVELED_LINEAR;
        break;
      case trTriggerType.TRAVEL_ANGULAR:
        triggerType    = triggerType_t.TRIGGER_TYPE_DISTANCE_TRAVELED;
        triggerSubType = (ushort)TriggerSubtypeDistanceTraveled_t.TRIGGER_SUBTYPE_DISTANCE_TRAVELED_ANGLE;
        break;
      case trTriggerType.TRAVELING_FORWARD:
        triggerType    = triggerType_t.TRIGGER_TYPE_WHEEL_ENCODER_SPEED;
        triggerSubType = (ushort)triggerSubtypeWheelSpeed_t.TRIGGER_SUBTYPE_WHEEL_ENCODER_FORWARD;
        break;
      case trTriggerType.TRAVELING_BACKWARD:
        triggerType    = triggerType_t.TRIGGER_TYPE_WHEEL_ENCODER_SPEED;
        triggerSubType = (ushort)triggerSubtypeWheelSpeed_t.TRIGGER_SUBTYPE_WHEEL_ENCODER_BACKWARD;
        break;
      case trTriggerType.TRAVELING_STOPPED:
        triggerType    = triggerType_t.TRIGGER_TYPE_WHEEL_ENCODER_SPEED;
        triggerSubType = (ushort)triggerSubtypeWheelSpeed_t.TRIGGER_SUBTYPE_WHEEL_ENCODER_STOPPED;
        break;
        
        
      case trTriggerType.RANDOM:
        triggerType    = triggerType_t.TRIGGER_TYPE_RANDOM;
        triggerSubType = (ushort)triggerSubtypeRandom_t.TRIGGER_SUBTYPE_RANDOM_NEXT_STATE_TRANSITION;
        break;
      case trTriggerType.TIME_RANDOM:
        triggerType    = triggerType_t.TRIGGER_TYPE_RANDOM;
        triggerSubType = (ushort)triggerSubtypeRandom_t.TRIGGER_SUBTYPE_RANDOM_TIME;
        break;

      case trTriggerType.BEHAVIOR_FINISHED:
        triggerType    = triggerType_t.TRIGGER_TYPE_AUTO;
        triggerSubType = (ushort)triggerSubtypeAuto_t.TRIG_SUBTYPE_AUTO_VALID;
        break;
        
        
      case trTriggerType.CLAP:
        triggerType    = triggerType_t.TRIGGER_TYPE_CLAP;
        triggerSubType = (ushort)triggerSubtypeClap_t.TRIGGER_SUBTYPE_CLAP_SINGLE;
        break;

       case trTriggerType.VOICE:
        triggerType    = triggerType_t.TRIGGER_TYPE_VOICE;
        triggerSubType = (ushort)triggerSubtypeVoice_t.TRIG_SUBTYPE_VOICE_DETECTED;
        break;
  
      case trTriggerType.KIDNAP:
        triggerType    = triggerType_t.TRIGGER_TYPE_KIDNAP;
          triggerSubType = (ushort)triggerSubtypeKidnap_t.TRIGGER_SUBTYPE_KIDNAP;
        break;
      case trTriggerType.KIDNAP_NOT:
        triggerType    = triggerType_t.TRIGGER_TYPE_KIDNAP;
        triggerSubType = (ushort)triggerSubtypeKidnap_t.TRIGGER_SUBTYPE_NOT_KIDNAP;
        break;
      case trTriggerType.STALL:
        triggerType    = triggerType_t.TRIGGER_TYPE_STALL_BUMP;
        triggerSubType = (ushort)triggerSubtypeStallBump_t.TRIGGER_SUBTYPE_STALL_BUMP;
        break;
      case trTriggerType.STALL_NOT:
        triggerType    = triggerType_t.TRIGGER_TYPE_STALL_BUMP;
        triggerSubType = (ushort)triggerSubtypeStallBump_t.TRIGGER_SUBTYPE_NOT_STALL_BUMP;
        break;
        
        
      }

      if (((ushort)triggerType & ~(0x0f)) != 0) {
        WWLog.logError("invalid trigger Type: " + triggerType.ToString());
      }
      
      if ((triggerSubType & ~(0x0fff)) != 0) {
        WWLog.logError("invalid trigger SubType: " + triggerSubType);
      }
                
      ushort ret = (ushort)(((ushort)triggerType << 12) | triggerSubType);
      
      return ret;
      }
    
    private byte[] toByteArrayBehavior(trState trStt) {
      // this handles the compound mood command.
      // note that altho the file format allows nested mood compounds,
      // this code will only generate at most one contained behavior.
      // which makes sense because there's no user-facing concept of that anyhow.
      
      byte[] plainBehavior = toByteArrayBehaviorNoMood(trStt);
      
      if (trStt.Mood == trMoodType.NO_CHANGE) {
        return plainBehavior;
      }
      
      behaviorType_t bhvType = behaviorType_t.BEHAVIOR_TYPE_SET_MOOD;
      
      byte[] val = null;
      int moodIndex = (int)trStt.Mood - 1; // minus one because robot uses 0 as first.
//    val = wwBA.append(wwBA.toByteArray2((ushort)(plainBehavior.Length + 1)), val);
      val = wwBA.append(wwBA.toByteArray1((byte)moodIndex                   ), val);
      val = wwBA.append(plainBehavior                                        , val);

      return toByteArrayBehaviorTypeAndValue(bhvType, val);
    }
    
    private byte[] toByteArrayBehaviorTypeAndValue(behaviorType_t bhvType, byte[] value) {
      byte[] ret = null;
      
      ret = wwBA.append(wwBA.toByteArray1((byte)bhvType)         , ret);
      ret = wwBA.append(wwBA.toByteArray2((ushort)(value.Length)), ret);
      ret = wwBA.append(value                               , ret);
      
      return ret;
    }
    
    private byte[] toByteArrayBehaviorNoMood(trState trStt) {
    
      trBehavior trBhv = trStt.Behavior;
    
      behaviorType_t typ = behaviorType_t.BEHAVIOR_TYPE_INVALID;
      byte[]         val = new byte[0];
    
      switch (trBhv.Type) {
        default:
          WWLog.logError("unhandled behavior type: " + trBhv.Type.ToString());
          break;
        case trBehaviorType.DO_NOTHING:
        case trBehaviorType.START_STATE:
        case trBehaviorType.OMNI:
          typ = behaviorType_t.BEHAVIOR_TYPE_DO_NOTHING;
          val = new byte[0];
          break;
        case trBehaviorType.MAPSET:
          typ = behaviorType_t.BEHAVIOR_TYPE_MAKER;
          val = toByteArrayMapSet(trBhv.MapSet);
          break;
        case trBehaviorType.COLOR_OFF:
        case trBehaviorType.COLOR_RED:
        case trBehaviorType.COLOR_ORANGE:
        case trBehaviorType.COLOR_YELLOW:
        case trBehaviorType.COLOR_GREEN:
        case trBehaviorType.COLOR_CYAN:
        case trBehaviorType.COLOR_BLUE:
        case trBehaviorType.COLOR_MAGENTA:
        case trBehaviorType.COLOR_WHITE:
          typ = behaviorType_t.BEHAVIOR_TYPE_RGB_LIGHT;
          val = toByteArrayRGBLight(trBhv.Type);
          break;
        case trBehaviorType.SOUND_USER:
        case trBehaviorType.SOUND_INTERNAL:
        case trBehaviorType.SOUND_VOCAL_BRAVE:
        case trBehaviorType.SOUND_VOCAL_CAUTIOUS:
        case trBehaviorType.SOUND_VOCAL_CURIOUS:
        case trBehaviorType.SOUND_VOCAL_FRUSTRATED:
        case trBehaviorType.SOUND_VOCAL_HAPPY:
        case trBehaviorType.SOUND_VOCAL_SILLY:
        case trBehaviorType.SOUND_VOCAL_SURPRISED:
        case trBehaviorType.SOUND_ANIMAL:
        case trBehaviorType.SOUND_SFX:
        case trBehaviorType.SOUND_TRANSPORT:
          typ = behaviorType_t.BEHAVIOR_TYPE_SOUND;
          val = toByteArraySound(trStt);
          break;
        case trBehaviorType.EXPRESSION_CATEGORY_1:
        case trBehaviorType.EXPRESSION_CATEGORY_2:
          typ = behaviorType_t.BEHAVIOR_TYPE_ANIMATION;
          val = toByteArrayExpression(trStt);
          break;
        case trBehaviorType.MOODY_ANIMATION:
          typ = behaviorType_t.BEHAVIOR_TYPE_ANIMATION;
          val = toByteArrayAnimation(trStt);
          break;
      case trBehaviorType.PUPPET:
          typ = behaviorType_t.BEHAVIOR_TYPE_PLAY_PUPPET;
          val = toByteArrayPuppet(trStt);
          break;
        case trBehaviorType.EYERING:
          typ = behaviorType_t.BEHAVIOR_TYPE_SET_EYERING;
          val = toByteArrayEyeRing(trStt);
          break;
        case trBehaviorType.MOVE_STOP:
        case trBehaviorType.MOVE_DISC_STRAIGHT:
        case trBehaviorType.MOVE_DISC_TURN:
        case trBehaviorType.MOVE_CONT_SPIN:
        case trBehaviorType.MOVE_CONT_STRAIGHT:
        case trBehaviorType.MOVE_CONT_CIRCLE_CCW:
        case trBehaviorType.MOVE_CONT_CIRCLE_CW:
        case trBehaviorType.HEAD_PAN:
        case trBehaviorType.HEAD_TILT:
          typ = behaviorType_t.BEHAVIOR_TYPE_SIMPLE_MOVES;
          val = toByteArraySimpleMove(trStt);
          break;
        case trBehaviorType.LAUNCH_RELOAD_LEFT:
        case trBehaviorType.LAUNCH_RELOAD_RIGHT:
          typ = behaviorType_t.BEHAVIOR_TYPE_LAUNCH_RELOAD;
          val = toByteArrayLaunch(trStt);
          break;
        case trBehaviorType.LAUNCH_FLING:
          typ = behaviorType_t.BEHAVIOR_TYPE_LAUNCH_FLING;
          val = toByteArrayLaunch(trStt);
          break;
        case trBehaviorType.RUN_SPARK:
          typ = behaviorType_t.BEHAVIOR_TYPE_RUN_SPARK;
          val = toByteArrayRunSpark(trStt);
          break;
        case trBehaviorType.MOVE_TURN_VOICE:
          typ = behaviorType_t.BEHAVIOR_TYPE_TURN_TO_VOICE;
          val = new byte[0];
          break;
        case trBehaviorType.HEAD_PAN_VOICE:
          typ = behaviorType_t.BEHAVIOR_TYPE_LOOK_TO_VOICE;
          val = new byte[0];
          break;
      }
      
      return toByteArrayBehaviorTypeAndValue(typ, val);
    }
    
    private byte[] toByteArrayMapSet(trMapSet trMS) {
      byte[] ret = new byte[0];
      
      int expectedByteCount = 0;
      
      foreach (trMap trM in trMS.Maps) {
        if (trM.Active) {
          ret = wwBA.append(toByteArrayMap(trM), ret);
          expectedByteCount += cBytesPerBehaviorMakerMap;
        }
      }
      
      if (ret.Length != expectedByteCount) {
        WWLog.logError("incorrect byte-count for mapset. expected " + expectedByteCount + " but actually " + ret.Length);
      }
      
      return ret;
    }
    
    private byte[] toByteArrayMap(trMap trM) {
      byte[] ret = null;
      

      ushort sensorUserParam = 0xbeef;
      switch (trM.Sensor.Type) {
        case trSensorType.TIME_IN_STATE:
          // 2 bytes represents 1/10ths of second
          sensorUserParam = (ushort)(short)(trM.Sensor.ParameterValue * 10.0f);
          break;
        case trSensorType.TRAVEL_LINEAR:
          // 2 bytes represents mm.
          sensorUserParam = (ushort)(short)(trM.Sensor.ParameterValue * 10.0f);
          break;
        case trSensorType.TRAVEL_ANGULAR:
          // 2 bytes represents radians / 100.
          sensorUserParam = (ushort)(short)(trM.Sensor.ParameterValue * Mathf.Deg2Rad * 100.0f);
          break;
      }
      
      // actuator ID - 1 bytes
      ret = wwBA.append(wwBA.toByteArray1((byte)swfwConv(trM.Actuator.Type)), ret);
      
      // actuator range - 2 x 3 nibbles
      wwRange actuatorRange = new wwRange(trM.ActuatorPoints.Points[0].y, trM.ActuatorPoints.Points[trM.ActuatorPoints.Points.Count - 1].y);
      ret = wwBA.append(wwBA.toByteArrayRange(actuatorRange, trM.InvertSensor), ret);
      
      // sensor ID - 1 byte
      ret = wwBA.append(wwBA.toByteArray1(swfwConv(trM.Sensor.Type)), ret);
      
      // sensor range - 2 x 3 nibbles
      ret = wwBA.append(wwBA.toByteArrayRange(trM.RangeSensor), ret);
      
      // sensor user param
      ret = wwBA.append (wwBA.toByteArray2(sensorUserParam), ret);
      
      if (ret.Length != cBytesPerBehaviorMakerMap) {
        WWLog.logError("incorrect byte count for BehaviorMap. expecting " + cBytesPerBehaviorMakerMap + ", but actually: " + ret.Length);
      }
      
      return ret;
    }
    
    private byte[] toByteArrayRGBLight(trBehaviorType trBT) {
      Color c = trBehavior.convertColorType(trBT);
      return wwBA.toByteArray3(c.r, c.g, c.b);
    }
    
    private byte[] toByteArrayMediaFile(bool repeat, string filename) {
      byte[] ret = null;
      
      ret = wwBA.append(wwBA.toByteArray(repeat), ret);           // auto-repeat
      ret = wwBA.append(wwBA.toByteArray(filename), ret);
      
      return ret;
    }
    
    private byte[] toByteArraySound(trState trStt) {
      uint soundID = (uint)(int)trStt.BehaviorParameterValue;
      trRobotSound sound = trRobotSounds.Instance.GetSound(soundID, theRobotType);
      if (sound == null) {
        WWLog.logError("sound not found! robotType = " + theRobotType.ToString() + "  soundID = " + soundID);
        return new byte[0];
      }
      
      string filename = "";
      filename = filename + sharedConstants.TOK_SYST;
      filename = filename + sound.filename;
      
      return toByteArrayMediaFile(false, filename);
    }
    
    private byte[] toByteArrayExpression(trState trStt) {
      trExpression trExp = trExpressions.Instance.GetExpression(trStt);
      string animBaseName = sharedConstants.TOK_SYST + "E" + trExp.id.ToString();
      return toByteArrayMediaFile(false, animBaseName);
    }
    
    private byte[] toByteArrayAnimation(trState trStt) {
      trMoodyAnimation trAnm = trMoodyAnimations.Instance.GetAnimation(trStt);
      string animBaseName = sharedConstants.TOK_SYST + "A" + trAnm.id.ToString();
      return toByteArrayMediaFile(false, animBaseName);
    }
    
    private byte[] toByteArrayPuppet(trState trStt) {
      int slotIndex = (int)trStt.BehaviorParameterValue;

      string filename = WW.Puppet.fullPathForSlot(slotIndex, false);
      return toByteArrayMediaFile(false, filename);
    }
    
    private byte[] toByteArrayEyeRing(trState trStt) {
      return wwBA.toByteArray2((ushort)(short)trStt.BehaviorParameterValue);
    }
    
    private byte[] toByteArraySimpleMove(trState trStt) {
      byte subTyp = (byte)behaviorSubtypeSimpleMove_t.BEHAVIOR_SUBTYPE_SIMPLE_MOVE_INVALID;
      uint val = 0;   // 4 bytes
      switch (trStt.Behavior.Type) {
        default:
          WWLog.logError("unhandled behavior type: " + trStt.Behavior.Type);
          break;
        case trBehaviorType.MOVE_STOP:
          subTyp = (byte)behaviorSubtypeSimpleMove_t.BEHAVIOR_SUBTYPE_SIMPLE_MOVE_STOP;
          val = 0;
          break;
        case trBehaviorType.MOVE_CONT_STRAIGHT:
          subTyp = (byte)behaviorSubtypeSimpleMove_t.BEHAVIOR_SUBTYPE_SIMPLE_MOVE_CONT;
          val = wwBA.toUintFloatFloat(trStt.BehaviorParameterValue * cCMtoMM, 0);
          break;
        case trBehaviorType.MOVE_CONT_SPIN:
          subTyp = (byte)behaviorSubtypeSimpleMove_t.BEHAVIOR_SUBTYPE_SIMPLE_MOVE_CONT;
          float linearVelocity;
          float angularVelocity;
          piMathUtil.wheelSpeedsToLinearAngular(trStt.GetBehaviorParameterValue(0),
                                                trStt.GetBehaviorParameterValue(1),
                                                out linearVelocity,
                                                out angularVelocity,
                                                PI.piBotConstants.axleLength);

          sharedConstants.clampLinearAngular(ref linearVelocity, ref angularVelocity);

          val = wwBA.toUintFloatFloat(linearVelocity  * cCMtoMM, angularVelocity * 100.0f);
          break;
        case trBehaviorType.MOVE_DISC_STRAIGHT:
          subTyp = (byte)behaviorSubtypeSimpleMove_t.BEHAVIOR_SUBTYPE_SIMPLE_MOVE_DISC_STRAIGHT;
          val = (ushort)(short)(trStt.BehaviorParameterValue * cCMtoMM);
          break;
        case trBehaviorType.MOVE_DISC_TURN:
          subTyp = (byte)behaviorSubtypeSimpleMove_t.BEHAVIOR_SUBTYPE_SIMPLE_MOVE_DISC_TURN;
          val = (ushort)(short)(trStt.BehaviorParameterValue * cDEGtoCENTIRAD);
          val = (ushort)-val; // TUR-1416.  TODO: this is the proliferation of a bug in UI. Fix in a backwards-compatible manner.
          break;
        case trBehaviorType.HEAD_PAN:
          subTyp = (byte)behaviorSubtypeSimpleMove_t.BEHAVIOR_SUBTYPE_SIMPLE_MOVE_HEAD_PAN;
          val = (ushort)(short)(trStt.BehaviorParameterValue * cDEGtoCENTIRAD);
//          val <<= 16; // TUR-617
          break;
        case trBehaviorType.HEAD_TILT:
          subTyp = (byte)behaviorSubtypeSimpleMove_t.BEHAVIOR_SUBTYPE_SIMPLE_MOVE_HEAD_TILT;
          val = (ushort)(short)(trStt.BehaviorParameterValue * cDEGtoCENTIRAD);
//          val <<= 16; // TUR-617
          break;
      }
      
      byte[] ret = null;
      ret = wwBA.append(wwBA.toByteArray1(subTyp), ret);
      ret = wwBA.append(wwBA.toByteArray4(val   ), ret);
      
      return ret;
    }
    
    private byte[] toByteArrayLaunch(trState trStt) {      
      byte val = (byte) 0; 
      switch (trStt.Behavior.Type) {
      case trBehaviorType.LAUNCH_RELOAD_LEFT:
        val = (byte) 1;
        break;
      case trBehaviorType.LAUNCH_RELOAD_RIGHT:
        val = (byte) 2;
        break;
      case trBehaviorType.LAUNCH_FLING:
        val = (byte)(int)(trStt.GetBehaviorParameterValue(0) * 255.0f);
        break;
      }
      
      byte[] ret = wwBA.toByteArray1(val);
      
      return ret;
    }
        
    private byte[] toByteArrayRunSpark(trState trStt) {
      // todo: find a way to store the filename in the state.
      string filename = "";
      filename = filename + sharedConstants.TOK_SPKU;
      filename = filename + sharedConstants.TOK_MAIN_SPARK_FILE;
      
      return wwBA.toByteArray(filename);
    }
    
    #region enum conversion routines
    
    private static Dictionary<trActuatorType, actuatorID_t> swfwMapActuatorType = null;
    
    actuatorID_t swfwConv(trActuatorType trAT) {
      if (swfwMapActuatorType == null) {
        swfwMapActuatorType = new Dictionary<trActuatorType, actuatorID_t>();
        swfwMapActuatorType[trActuatorType.HEAD_PAN   ] = actuatorID_t.ACT_HEAD_PAN;
        swfwMapActuatorType[trActuatorType.HEAD_TILT  ] = actuatorID_t.ACT_HEAD_TILT;
        swfwMapActuatorType[trActuatorType.WHEEL_L    ] = actuatorID_t.ACT_LEFT_WHEEL_SPEED;
        swfwMapActuatorType[trActuatorType.WHEEL_R    ] = actuatorID_t.ACT_RIGHT_WHEEL_SPEED;
        swfwMapActuatorType[trActuatorType.RGB_ALL_HUE] = actuatorID_t.ACT_RGB_HUE;
        swfwMapActuatorType[trActuatorType.RGB_ALL_VAL] = actuatorID_t.ACT_RGB_BRIGHTNESS;
        swfwMapActuatorType[trActuatorType.LED_TOP    ] = actuatorID_t.ACT_LED_TOP;
        swfwMapActuatorType[trActuatorType.LED_TAIL   ] = actuatorID_t.ACT_LED_TAIL;
      }
      
      if (!swfwMapActuatorType.ContainsKey(trAT)) {
        WWLog.logError("unhandled actuator type: " + trAT.ToString());
        return actuatorID_t.ACT_INVALID;
      }
      
      return swfwMapActuatorType[trAT];
    }
    
    private static Dictionary<trSensorType, byte> swfwMapSensorTypeAndSubtype = null;
    
    byte swfwConv(trSensorType trST) {
      if (swfwMapSensorTypeAndSubtype == null) {
        swfwMapSensorTypeAndSubtype = new Dictionary<trSensorType, byte>();
        Dictionary<trSensorType, byte> map = swfwMapSensorTypeAndSubtype;
        map[trSensorType.DISTANCE_FRONT             ] = wwBA.mergeNibbles((byte)sensorID_t.SENSE_DISTANCE, (byte)sensorSubtypeDistance_t.SENSOR_SUBTYPE_DISTANCE_FRONT);
        map[trSensorType.DISTANCE_FRONT_DELTA       ] = wwBA.mergeNibbles((byte)sensorID_t.SENSE_DISTANCE, (byte)sensorSubtypeDistance_t.SENSOR_SUBTYPE_DISTANCE_FRONT_DELTA);
        map[trSensorType.DISTANCE_FRONT_LEFT_FACING ] = wwBA.mergeNibbles((byte)sensorID_t.SENSE_DISTANCE, (byte)sensorSubtypeDistance_t.SENSOR_SUBTYPE_DISTANCE_LEFT_FACING);
        map[trSensorType.DISTANCE_FRONT_RIGHT_FACING] = wwBA.mergeNibbles((byte)sensorID_t.SENSE_DISTANCE, (byte)sensorSubtypeDistance_t.SENSOR_SUBTYPE_DISTANCE_RIGHT_FACING);
        map[trSensorType.DISTANCE_REAR              ] = wwBA.mergeNibbles((byte)sensorID_t.SENSE_DISTANCE, (byte)sensorSubtypeDistance_t.SENSOR_SUBTYPE_DISTANCE_REAR);
        map[trSensorType.HEAD_PAN                   ] = wwBA.mergeNibbles((byte)sensorID_t.SENSE_HEAD    , (byte)sensorSubtypeHead_t    .SENSOR_SUBTYPE_HEAD_PAN);
        map[trSensorType.HEAD_TILT                  ] = wwBA.mergeNibbles((byte)sensorID_t.SENSE_HEAD    , (byte)sensorSubtypeHead_t    .SENSOR_SUBTYPE_HEAD_TILT);
        map[trSensorType.PITCH                      ] = wwBA.mergeNibbles((byte)sensorID_t.SENSE_ACCEL   , (byte)sensorSubtypeAccel_t   .SENSOR_SUBTYPE_ACCEL_PITCH);
        map[trSensorType.ROLL                       ] = wwBA.mergeNibbles((byte)sensorID_t.SENSE_ACCEL   , (byte)sensorSubtypeAccel_t   .SENSOR_SUBTYPE_ACCEL_ROLL);
        map[trSensorType.RANDOM_NOISE               ] = wwBA.mergeNibbles((byte)sensorID_t.SENSE_RANDOM  , (byte)sensorSubtypeRandom_t  .SENSOR_SUBTYPE_RANDOM_VALID);
        map[trSensorType.TIME_IN_STATE              ] = wwBA.mergeNibbles((byte)sensorID_t.SENSE_TIME    , (byte)sensorSubtypeTime_t    .SENSOR_SUBTYPE_TIME_VALID);
        map[trSensorType.TRAVEL_LINEAR              ] = wwBA.mergeNibbles((byte)sensorID_t.SENSE_TRAVEL  , (byte)sensorSubtypeTravel_t  .SENSOR_SUBTYPE_TRAVEL_LINEAR);
        map[trSensorType.TRAVEL_ANGULAR             ] = wwBA.mergeNibbles((byte)sensorID_t.SENSE_TRAVEL  , (byte)sensorSubtypeTravel_t  .SENSOR_SUBTYPE_TRAVEL_ANGULAR);
      }
      
      if (!swfwMapSensorTypeAndSubtype.ContainsKey(trST)) {
        WWLog.logError("unhandled sensor type: " + trST.ToString());
        return (byte)sensorID_t.SENSE_INVALID;
      }
      
      return swfwMapSensorTypeAndSubtype[trST];
    }
    
    #endregion enum conversion routines
    
    
    #region toByteArray routines
    private static byte[] toByteArrayTriggerType(trTriggerType trTT, float thresh) {
      byte[] ret = null;

      ret = wwBA.append(wwBA.toByteArray2(triggerSpecifierSimple(trTT)), ret);
      ret = wwBA.append(wwBA.toByteArray2((short)(255.0f * thresh)    ), ret);

      if (ret.Length != 4) {
        WWLog.logError("unexpected bytecount: " + ret.Length);
      }

      return ret;
    }

    private static byte[] toByteArrayTriggerType(trTriggerType trTT, float min, float max) {
      byte[] ret = null;

      ret = wwBA.append(wwBA.toByteArray2(triggerSpecifierSimple(trTT)), ret);
      ret = wwBA.append(wwBA.toByteArray2((short)min                  ), ret);
      ret = wwBA.append(wwBA.toByteArray2((short)max                  ), ret);

      if (ret.Length != 6) {
        WWLog.logError("unexpected bytecount: " + ret.Length);
      }

      return ret;
    }
    #endregion toByteArray routines
    
    private byte indexOfStateInFile(trState trs) {
      if (trs == null) {
        return cInvalidIndex;
      }
    
      if (theProgram.StateStart == trs) {
        return 0;
      }
    
      byte ret = 1;
      
      foreach (trState state in theProgram.UUIDToStateTable.Values) {
        if (state == trs) {
          return ret;
        }
        if (state == theProgram.StateStart) {
          ret -= 1;
        }
        
        ret += 1;
      }
      
      WWLog.logError("did not find state!");
      return cInvalidIndex;
    }
    
    // todo - cache
    private ushort fileOffsetToFirstBehavior() {
      ushort ret = 0;
      
      ret += cBytesPerHeader;
      ret += (ushort)(cBytesPerState * theProgram.UUIDToStateTable.Count);
      
      return ret;
    }
    
    private ushort fileOffsetToBehavior(trState trStt) {
      return (ushort)(fileOffsetToFirstBehavior() + behaviorOffsetsInTable[trStt]);
    }
    
    private Dictionary<trTriggerType, triggerSubtypeDist_t> MapTriggerSubtypeDistance {
      get {
        if (mapTriggerSubtypeDistance == null) {
          mapTriggerSubtypeDistance = new Dictionary<trTriggerType, triggerSubtypeDist_t>();
          mapTriggerSubtypeDistance[trTriggerType.DISTANCE_CENTER_NONE] = triggerSubtypeDist_t.TRIGGER_SUBTYPE_DIST_FRONT_NONE;
          mapTriggerSubtypeDistance[trTriggerType.DISTANCE_CENTER_FAR ] = triggerSubtypeDist_t.TRIGGER_SUBTYPE_DIST_FRONT_FAR;
          mapTriggerSubtypeDistance[trTriggerType.DISTANCE_CENTER_NEAR] = triggerSubtypeDist_t.TRIGGER_SUBTYPE_DIST_FRONT_NEAR;
          mapTriggerSubtypeDistance[trTriggerType.DISTANCE_RIGHT_NONE ] = triggerSubtypeDist_t.TRIGGER_SUBTYPE_DIST_RIGHT_NONE;
          mapTriggerSubtypeDistance[trTriggerType.DISTANCE_RIGHT_FAR  ] = triggerSubtypeDist_t.TRIGGER_SUBTYPE_DIST_RIGHT_FAR;
          mapTriggerSubtypeDistance[trTriggerType.DISTANCE_RIGHT_NEAR ] = triggerSubtypeDist_t.TRIGGER_SUBTYPE_DIST_RIGHT_NEAR;
          mapTriggerSubtypeDistance[trTriggerType.DISTANCE_LEFT_NONE  ] = triggerSubtypeDist_t.TRIGGER_SUBTYPE_DIST_LEFT_NONE;
          mapTriggerSubtypeDistance[trTriggerType.DISTANCE_LEFT_FAR   ] = triggerSubtypeDist_t.TRIGGER_SUBTYPE_DIST_LEFT_FAR;
          mapTriggerSubtypeDistance[trTriggerType.DISTANCE_LEFT_NEAR  ] = triggerSubtypeDist_t.TRIGGER_SUBTYPE_DIST_LEFT_NEAR;
          mapTriggerSubtypeDistance[trTriggerType.DISTANCE_REAR_NONE  ] = triggerSubtypeDist_t.TRIGGER_SUBTYPE_DIST_REAR_NONE;
          mapTriggerSubtypeDistance[trTriggerType.DISTANCE_REAR_FAR   ] = triggerSubtypeDist_t.TRIGGER_SUBTYPE_DIST_REAR_FAR;
          mapTriggerSubtypeDistance[trTriggerType.DISTANCE_REAR_NEAR  ] = triggerSubtypeDist_t.TRIGGER_SUBTYPE_DIST_REAR_NEAR;
        }
        
        return mapTriggerSubtypeDistance;
      }
    }
    
    private Dictionary<trTriggerType, triggerSubtypeBeacon_t> MapTriggerSubtypeBeacon {
      get {
        if (mapTriggerSubtypeBeacon == null) {
          mapTriggerSubtypeBeacon = new Dictionary<trTriggerType, triggerSubtypeBeacon_t>();
          
          mapTriggerSubtypeBeacon[trTriggerType.BEACON_NONE      ] = (triggerSubtypeBeacon_t.TRIGGER_SUBTYPE_BEACON_DASH_AND_DOT_NONE);
          mapTriggerSubtypeBeacon[trTriggerType.BEACON_NONE_DASH ] =  triggerSubtypeBeacon_t.TRIGGER_SUBTYPE_BEACON_DASH_NONE;
          mapTriggerSubtypeBeacon[trTriggerType.BEACON_NONE_DOT  ] =  triggerSubtypeBeacon_t.TRIGGER_SUBTYPE_BEACON_DOT_NONE;
          
          mapTriggerSubtypeBeacon[trTriggerType.BEACON_BOTH      ] = (triggerSubtypeBeacon_t.TRIGGER_SUBTYPE_BEACON_DASH_BOTH  | triggerSubtypeBeacon_t.TRIGGER_SUBTYPE_BEACON_DOT_BOTH);
          mapTriggerSubtypeBeacon[trTriggerType.BEACON_BOTH_DASH ] =  triggerSubtypeBeacon_t.TRIGGER_SUBTYPE_BEACON_DASH_BOTH;
          mapTriggerSubtypeBeacon[trTriggerType.BEACON_BOTH_DOT  ] =  triggerSubtypeBeacon_t.TRIGGER_SUBTYPE_BEACON_DOT_BOTH;
          
          mapTriggerSubtypeBeacon[trTriggerType.BEACON_LEFT      ] = (triggerSubtypeBeacon_t.TRIGGER_SUBTYPE_BEACON_DASH_LEFT  | triggerSubtypeBeacon_t.TRIGGER_SUBTYPE_BEACON_DOT_LEFT);
          mapTriggerSubtypeBeacon[trTriggerType.BEACON_LEFT_DASH ] =  triggerSubtypeBeacon_t.TRIGGER_SUBTYPE_BEACON_DASH_LEFT;
          mapTriggerSubtypeBeacon[trTriggerType.BEACON_LEFT_DOT  ] =  triggerSubtypeBeacon_t.TRIGGER_SUBTYPE_BEACON_DOT_LEFT;
          
          mapTriggerSubtypeBeacon[trTriggerType.BEACON_RIGHT     ] = (triggerSubtypeBeacon_t.TRIGGER_SUBTYPE_BEACON_DASH_RIGHT | triggerSubtypeBeacon_t.TRIGGER_SUBTYPE_BEACON_DOT_RIGHT);
          mapTriggerSubtypeBeacon[trTriggerType.BEACON_RIGHT_DASH] =  triggerSubtypeBeacon_t.TRIGGER_SUBTYPE_BEACON_DASH_RIGHT;
          mapTriggerSubtypeBeacon[trTriggerType.BEACON_RIGHT_DOT ] =  triggerSubtypeBeacon_t.TRIGGER_SUBTYPE_BEACON_DOT_RIGHT;
        }
        
        return mapTriggerSubtypeBeacon;
      }
    }
    
    private static ushort triggerSetToSubtypeMask<T>(Dictionary<trTriggerType, T> map, trTriggerSet trTS) where T : struct, System.IConvertible {
      ushort triggerSubTypeMask = 0;
      
      foreach (trTriggerType trTT in map.Keys) {
        ushort maskBit = System.Convert.ToUInt16(map[trTT]);
        if (trTS.containsTriggerOfType(trTT)) {
          bool isCombo = ((trTT == trTriggerType.BEACON_NONE) || (trTT == trTriggerType.BEACON_BOTH) || (trTT == trTriggerType.BEACON_LEFT) || (trTT == trTriggerType.BEACON_RIGHT));
          if (!isCombo && (triggerSubTypeMask & maskBit) != 0) {
            WWLog.logError("subtype mask corrupt: already contains bit " + maskBit + " for " + trTT.ToString());
          }
          triggerSubTypeMask |= maskBit;
        }
      }
      
      return triggerSubTypeMask;
    }
    
    #region filtering
    // for each outgoing transition of trStt,
    // if any transition has trigger matchThis,
    // then remove all other outgoing triggers except those with type spareThese.
    void filterTransitions(trProgram trPrg, trState trStt, trTriggerType matchThis, trTriggerType spareThese) {
      HashSet<trTransition> toRemove = new HashSet<trTransition>();
      foreach (trTransition trTrn in trStt.OutgoingTransitions) {
        if (trTrn.Trigger.Type == matchThis) {
          foreach (trTransition trTrnRem in trStt.OutgoingTransitions) {
            if (trTrnRem != trTrn) {
              if (trTrnRem.Trigger.Type != spareThese) {
                toRemove.Add(trTrnRem);
              }
            }
          }
        }
      }
      
      foreach (trTransition trTrnRem in toRemove) {
        trPrg.RemoveTransition(trTrnRem);
      }
    }
    
    void removeTransitionsFromState(trProgram trPrg, trState trStt, trTriggerType matchThis) {
      HashSet<trTransition> toRemove = new HashSet<trTransition>();
      foreach (trTransition trTrn in trStt.OutgoingTransitions) {
        if (trTrn.Trigger.Type == matchThis) {
          toRemove.Add(trTrn);
        }
      }
      
      foreach (trTransition trTrnRem in toRemove) {
        trPrg.RemoveTransition(trTrnRem);
      }
    }
    
    void filterTransitionsThatGetEvaluatedLater(trProgram trPrg, trState trStt, int laterThanThis) {
      HashSet<trTransition> toRemove = new HashSet<trTransition>();
      foreach (trTransition trTrn in trStt.OutgoingTransitions) {
        if (trTrn.Trigger.Type.evaluationOrder() > laterThanThis) {
          toRemove.Add(trTrn);
        }
      }
      
      foreach (trTransition trTrnRem in toRemove) {
        trPrg.RemoveTransition(trTrnRem);
      }
    }

    // TUR-221    
    void eliminateOtherTransitionsInTheFaceOf_Immediate(trProgram trPrg) {
      foreach (trState trStt in trPrg.UUIDToStateTable.Values) {
        filterTransitions(trPrg, trStt, trTriggerType.IMMEDIATE, trTriggerType.NONE);
      }
    }
    
    // TUR-850
    void eliminateLaterEvaluatedTriggersInTheFaceOfImmediate(trProgram trPrg) {
      foreach (trState trStt in trPrg.UUIDToStateTable.Values) {
        if (trStt.HasOutgoingTransitionWithTriggerType(trTriggerType.IMMEDIATE)) {
          filterTransitionsThatGetEvaluatedLater(trPrg, trStt, trTriggerType.IMMEDIATE.evaluationOrder());
        }
      }
    }
    
    // TUR-705
    // random trigger acts as auto as well.
    void eliminateAutoInTheFaceOf_Random(trProgram trPrg) {
      foreach (trState trStt in trPrg.UUIDToStateTable.Values) {
        if (trStt.HasOutgoingTransitionWithTriggerType(trTriggerType.RANDOM)) {
          removeTransitionsFromState(trPrg, trStt, trTriggerType.BEHAVIOR_FINISHED);
        }
      }
    }
    
    // TUR-705
    // firmware deals with Random triggers immediately.
    void replaceRandomWithAutoPlusRandom(trProgram trPrg) {
      HashSet<trState> hasRandom = new HashSet<trState>();
      foreach (trState trStt in trPrg.UUIDToStateTable.Values) {
        if (trStt.HasOutgoingTransitionWithTriggerType(trTriggerType.RANDOM)) {
          hasRandom.Add(trStt);
        }
      }
      
      foreach(trState trStt in hasRandom) {
        HashSet<trTransition> theRandomTransitions = new HashSet<trTransition>();
        foreach(trTransition trTrn in trStt.OutgoingTransitions) {
          if (trTrn.Trigger.Type == trTriggerType.RANDOM) {
            theRandomTransitions.Add(trTrn);
          }
        }
        
        if (trStt.HasOutgoingTransitionWithTriggerType(trTriggerType.BEHAVIOR_FINISHED)) {
          WWLog.logError("unexpected: TUR-705 handling: auto-trigger should be eliminated by now");
          // but continue anyhow. godspeed.
        }
        
        if (theRandomTransitions.Count == 0) {
          WWLog.logError("unexpected: random transitions don't add up");
        }
        else {
          // make new interstitial state.
          trState trSttTmp = new trState("temp");
          trSttTmp.Behavior = new trBehavior(trBehaviorType.DO_NOTHING);
          trPrg.AddState(trSttTmp);
          
          // add transition from original state to new one
          trTransition trSttTmpIncoming = new trTransition();
          trSttTmpIncoming.StateSource = trStt;
          trSttTmpIncoming.StateTarget = trSttTmp;
          trSttTmpIncoming.Trigger     = new trTrigger(trTriggerType.BEHAVIOR_FINISHED);
          trStt.AddOutgoingTransition(trSttTmpIncoming);
          trPrg.UUIDToTransitionTable.Add(trSttTmpIncoming.UUID, trSttTmpIncoming);
          
          // add transitions from new one to old destinations
          foreach(trTransition trTrn in theRandomTransitions) {
            trTransition trSttTmpOutGoing = new trTransition();
            trSttTmpOutGoing.StateSource = trSttTmp;
            trSttTmpOutGoing.StateTarget = trTrn.StateTarget;
            trSttTmpOutGoing.Trigger     = new trTrigger(trTriggerType.RANDOM);
            trSttTmp.AddOutgoingTransition(trSttTmpOutGoing);
            trPrg.UUIDToTransitionTable.Add(trSttTmpOutGoing.UUID, trSttTmpOutGoing);
          }
          
          // remove original random transitions
          foreach(trTransition trTrn in theRandomTransitions) {
            trPrg.RemoveTransition(trTrn);
          }
        }
      }
    }
    
    
    // if a given state has at least one outgoing transition triggered via the RANDOM trigger,
    // eliminate all other non-RANDOM triggered transitions.
    // TUR-221, TUR-434.
    void eliminateOtherTransitionsInTheFaceOf_Random(trProgram trPrg) {
      foreach (trState trStt in trPrg.UUIDToStateTable.Values) {
        filterTransitions(trPrg, trStt, trTriggerType.RANDOM, trTriggerType.RANDOM);
      }
    }
    
    // TUR-580
    void convertImmediateToTime(trProgram trPrg) {
      foreach (trTransition trTrn in trPrg.UUIDToTransitionTable.Values) {
        trTrigger trTrg = trTrn.Trigger;
        if (trTrg.Type == trTriggerType.IMMEDIATE) {
          trTrg.Type = trTriggerType.TIME;
          trTrg.ParameterValue = 0;
        }
      }
    }
    
    // TUR-616
    void ensureStartStateHasMood(trProgram trPrg) {
      trState start = trPrg.StateStart;
      if (start == null) {
        WWLog.logError("awooooga. no start state");
        return;
      }
      
      if (start.Mood == trMoodType.NO_CHANGE) {
        start.Mood = trMood.DefaultMood;
      }
    }
    
    #endregion filtering
  }
}


