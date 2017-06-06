using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using WW.SimpleJSON;

using PI;


public interface piRobotShellDelegate
{
  void didExecuteShellCommand(piBotBase robot, string command, string response);
  void didFailShellCommand   (piBotBase robot, string command, string error);
}

// from HALDefinitions.h:
//typedef enum {
//  WW_ROBOT_UNKNOWN,
//  WW_ROBOT_DASH,
//  WW_ROBOT_DASH_DFU,
//  WW_ROBOT_DOT,
//  WW_ROBOT_DOT_DFU
//} WWRobotType;

//from WWConstants.h
public enum piRobotType : int {
  UNKNOWN  = 1000,
  DASH     = 1001,
  DOT      = 1002,
  D2       = 1003,
  DASH_DFU = 2001,
  DOT_DFU  = 2002,
  D2_DFU   = 2003,
}

public enum WWRobotFileTransferStatus {
  FILE_TRANSFER_STATUS_SUCCESS  = 0,
  FILE_TRANSFER_STATUS_FAILED   = 1,
  FILE_TRANSFER_STATUS_CANCELED = 2,
}

// from https://github.com/playi/APIObjectiveC/blob/bundle/engine/robots/WWRobotFileTransferManager.h#L16-L23
public enum WWRobotFileTransferFileType {
  FILE_TYPE_NONE     = 0,
  FILE_TYPE_FIRMWARE = 1,
  FILE_TYPE_AUDIO    = 2,
  FILE_TYPE_LUA      = 3,
  FILE_TYPE_SPARK    = 4,
  FILE_TYPE_ANIM     = 5,
}

public static class piRobotExtensions {
  public static string UserFacingName(this piRobotType val) {
    string ret = "Robot";

    switch(val){
      default:
        WWLog.logError("unhandled robot type: " + val.ToString());
        ret = val.ToString();
        break;
      case piRobotType.DASH:
        ret = "Dash";
        break;
      case piRobotType.DOT:
        ret = "Dot";
        break;
    }

    return ret;
  }
}

public abstract class piBotBase {
  
  private string uuId;
  private string name;
  public string SerialNumber;
  public string FwVersion;
  public int RsVersion;
  public string Lang;
  public piRobotType robotType = piRobotType.UNKNOWN;

  public int PersonalityColorIndex;
  public int HwRevision;
  public float? SignalStrength = null;
  public int PersonalityAnimIndex;
  public string AudioFilesVersion;
  public bool HasCrashDumps;
  public bool AdvButtonPress;

  public PI.BotConnectionState connectionState;

  public piRobotShellDelegate shellDelegate = null;
  // todo: extend this to include file transfer in progress
  // todo: get this from the native robot, because API sends shell commands itself.
  public int pendingShellCommandCount {
    get {
      return PIBInterface_internal.pendingShellCommandCount(uuId);
    }
  }
  
  public PIBInterface.Actions apiInterface;
  
  public Dictionary<PI.ComponentID, piBotComponentBase> components = new Dictionary<ComponentID, piBotComponentBase>();
  
  public delegate void OnStateDelegate(piBotBase robot);
  public event OnStateDelegate OnState;

  public delegate void OnSensorsDelegate(piBotBase robot, JSONClass sensors);
  public event OnSensorsDelegate OnSensors;
  
  public delegate void OnRobotEventDelegate(piBotBase robot, wwRobotEvent robotEvent);
  public event OnRobotEventDelegate OnRobotEvent;
  
  public static bool Verbose = false;
  
  
  // this is a little janky. put in for Turing. consider forcing the client app to track this.
  private const int incomingEventsMax = 20;
  public List<wwRobotEvent> IncomingEvents = new List<wwRobotEvent>();
  
  public piBotBase(string inUUID, string inName, piRobotType inRobotType, JSONClass jsonRobot=null) {
    uuId = inUUID;
    name = inName;
    robotType = inRobotType;
    if (jsonRobot != null){
      UpdateAdvertisedInfo(jsonRobot);
    }
    connectionState = BotConnectionState.UNKNOWN;
    setupComponents();
  }

  public virtual void Reset(){}

  public void UpdateAdvertisedInfo(JSONClass jsonRobot) {
    PersonalityColorIndex = jsonRobot ["personalityColorIndex"].AsInt;
    SerialNumber = jsonRobot["serial"];
    FwVersion = jsonRobot["fw"];
    RsVersion = jsonRobot["rs"].AsInt;
    Lang = jsonRobot["lang"];
    if (jsonRobot.ContainsKey("signalStrength")) {
      SignalStrength = jsonRobot["signalStrength"].AsFloat;
    }
    PersonalityAnimIndex = jsonRobot["personalityAnimIndex"].AsInt;
    AudioFilesVersion = jsonRobot["audioFilesVersion"];
    robotType = (piRobotType)jsonRobot["type"].AsInt;

    // These items not yet implemented on android and not yet needed
//    HwRevision = jsonRobot["hardwareRevision"].AsInt;
//    HasCrashDumps = jsonRobot["hasCrashDumps"].AsBool;
//    AdvButtonPress = jsonRobot["advButtonPress"].AsBool;
  }
  
  protected static void logVerbose(string msg) {
    if (Verbose) {
      WWLog.logDebug(msg);
    }
  }

  public string FwRsVersionAndLocale{
    get {
      return FwVersion + "(" + RsVersion + ")" + Lang;
    }
  }
  
  public string UUID {
    get {
      return uuId;
    }
  }

  
  public string Name {
    get {
      return name;
    }
    set{
      string proposedName = value.Trim();
      if (proposedName.Length > 0) {
        if (proposedName.Length > 10) {
          proposedName = proposedName.Substring(0, 10);
        }
        if (!name.Equals(proposedName)) {
          name = proposedName;
          apiInterface.setRobotName(UUID, name);
        }
      }
    }
  }


  public bool IsDFU {
    get {
      return ((robotType == piRobotType.DASH_DFU) || (robotType == piRobotType.DOT_DFU) || (robotType == piRobotType.D2_DFU));
    }
  }

  public bool IsUnconfigured {
    get {
      return PersonalityColorIndex == 0;
    }
  }

  public virtual void tick(float dt) {
    foreach (piBotComponentBase component in components.Values) {
      component.tick(dt);
    }
  }
  
  protected componentT addComponent<componentT>(PI.ComponentID id) where componentT:piBotComponentBase {
    componentT component = System.Activator.CreateInstance<componentT>();
    component.componentId = id;
    components[id] = component;
    return component;
  }
  
  protected virtual void setupComponents(){}
  
  /*
    gets json object w/ the form:
    "componentID" : {
      "param": "value"
    }
  */
  private piBotComponentBase validateComponentID(int componentID) {
    if (!Enum.IsDefined(typeof(PI.ComponentID), componentID)) {
      Debug.LogError("unknown component ID: " + componentID);
      componentID = (int)(PI.ComponentID.WW_UNKNOWN);
    }
    
    PI.ComponentID cId = (PI.ComponentID)componentID;
    
    if (!components.ContainsKey(cId)) {
      Debug.LogError("component not present in robot: " + cId.ToString());
      return null;
    }
    
    return components[cId];
  }
  
  // when acting as a proxy real robot
  public virtual void handleState(JSONClass jsComponent) {
    foreach(string key in jsComponent.Keys) {
      
      piBotComponentBase component = validateComponentID(int.Parse(key));
      if (component != null) {
        component.handleState(jsComponent[key].AsObject);
      }
    }
    
    if (OnSensors != null) {
      OnSensors(this, jsComponent);
    }
    if (OnState != null) {
      OnState(this);
    }

    sendAndClearStagedCommands();
  }
  
  // when acting as a proxy real robot
  public virtual void handleEvents(JSONArray jsEvents) {
    foreach (JSONClass jsEvent in jsEvents) {
      wwRobotEvent robotEvent = new wwRobotEvent(jsEvent);
      WWLog.logInfo("received robot event: " + robotEvent.Identifier + " with phase: " + robotEvent.Phase.ToString());
      IncomingEvents.Add(robotEvent);
      if (IncomingEvents.Count > incomingEventsMax) {
        IncomingEvents.RemoveRange(0, IncomingEvents.Count - incomingEventsMax);
      }
      if (OnRobotEvent != null) {
        OnRobotEvent(this, robotEvent);
      }
    }
  }
  
  // BOT COMMANDS
  public void cmd_reset() {
    WWLog.logDebug("");
    if (this.apiInterface != null) {
      apiInterface.reset(UUID);
    }
  }
  
  public void cmd_reboot(){
    WWLog.logDebug("");
    if (this.apiInterface != null) {
      apiInterface.reboot(UUID);
    }
  }
  
  public void cmd_sendRawData(byte[] data) {
//    WWLog.logDebug("count:" + data.Length + " data:" + piStringUtil.abbreviateMiddle(piStringUtil.byteArrayToString(data), 100));
    apiInterface.sendRawPacket(UUID, data);
  }
  
  public virtual void cmd_shell(string shellCommand) {
    WWLog.logDebug("shellCommand:" + shellCommand);
    PIBInterface_internal.shellCommand(UUID, shellCommand);
  }
  
  public void didExecuteShellCommand(string command, string results) {
    Debug.Log("didExecuteShellCommand " + command + " results: " + results);
    if (shellDelegate != null) {
      shellDelegate.didExecuteShellCommand(this, command, results);
    }
  }
  
  public void didFailShellCommand(string command, string error) {
    Debug.Log("didFailShellCommand " + command + " error: " + error);
    if (shellDelegate != null) {
      shellDelegate.didFailShellCommand(this, command, error);
    }
  }

  public virtual void cmd_set_personality_color_anim(int color, int anim) {
    PersonalityColorIndex = color & 0x07;
    PersonalityAnimIndex = anim & 0x07;
    int psn = (PersonalityAnimIndex << 3) | PersonalityColorIndex;
    cmd_shell("CFG SET PSN " + psn); 
  }

  public virtual void cmd_set_personality_color(int color) {
    cmd_set_personality_color_anim(color, PersonalityAnimIndex);
    Debug.Log("color set to " + PersonalityColorIndex);
  }
  
  public virtual void cmd_set_personality_anim(int anim) {
    cmd_set_personality_color_anim(PersonalityColorIndex, anim);
  }

  // SENSORS
  public JSONClass SensorState {
    get {
      JSONClass jsNode = new JSONClass();
      foreach (piBotComponentBase component in components.Values) {
        JSONClass jsContent = component.SensorState;
        if (jsContent != null) {
          jsNode[((int)component.componentId).ToString()] = jsContent;
        }
      }
      return jsNode;
    }
  }
  
  // Meta-Sensors
  public int NumberOfExecutingCommandSequences {
    get {
      if (this.apiInterface != null) {
        return apiInterface.numberOfExecutingCommandSequences(uuId);
      }
      else {
        return -1;
      }
    }
  }
  
  public bool HasIncomingEvent(string identifier) {
    foreach (wwRobotEvent re in IncomingEvents) {
      if (re.Identifier.ToLower() == identifier.ToLower()) {
        return true;
      }
    }
    
    return false;
  }

  private JSONClass stagedCommandSet = new JSONClass();

  public void stage_Command(ComponentID componentID, JSONClass parameters) {
    // merge commands into a single command set
    string componentKey = ((int)componentID).ToString();
    stagedCommandSet[componentKey] = parameters;
  }

  public void stage_CommandSet(JSONClass jsc) {
    foreach (string key in jsc.Keys) {
      ComponentID cid;
      cid = (ComponentID)(int.Parse(key));
      stage_Command(cid, jsc[key].AsObject);
    }
  }

  public void getAndClearStagedCommands(out JSONClass ret) {
    ret = stagedCommandSet;
    stagedCommandSet = new JSONClass();
  }

  public void sendAndClearStagedCommands() {
    JSONClass jsc;
    getAndClearStagedCommands(out jsc);

    if (jsc.Count == 0) {
      return;
    }

    apiInterface.sendCommandSetJson(UUID, jsc);
  }

  public void sendCommandSetJson(string jsc) {
    apiInterface.sendCommandSetJson(UUID, jsc);
  }
}
