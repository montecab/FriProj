using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Turing;
using WW.SimpleJSON;

public class trModelTestController : MonoBehaviour {

  public trProgram CurProgram;
  private piBotBase CurRobot;
  
  public Text      UI_Status;
  
  public Text      UI_ProgramName;
  
  private int curProgramNum;

	// Use this for initialization
	void Start () {
    SetProgram("test pitch and roll");
    
    foreach (piBotBase robot in piConnectionManager.Instance.BotsInState(PI.BotConnectionState.CONNECTED)) {
      onConnectRobot(robot);
    }
  }
  
  void OnEnable() {
    piConnectionManager.Instance.OnConnectRobot    += onConnectRobot;
    piConnectionManager.Instance.OnDisconnectRobot += onDisconnectRobot;
    
    if (CurRobot != null) {
      CurRobot.OnState += onRobotState;
    }
  }
  
  void OnDisable() {
    if (this.enabled) {
      if (piConnectionManager.Instance) {
        piConnectionManager.Instance.OnConnectRobot    -= onConnectRobot;
        piConnectionManager.Instance.OnDisconnectRobot -= onDisconnectRobot;
      }
      if (CurRobot != null) {
        CurRobot.OnState -= onRobotState;
      }
    }
  }
	
	// Update is called once per frame
	void Update () {
	}
  
  public void UpdateDisplay(piBotBase robot) {
    string s = "";
    if (robot != null) {
      float tis = CurProgram.StateCurrent == null ? 0 : CurProgram.StateCurrent.TimeInState;
      
      piBotBo bot = (piBotBo)robot;
      s += "BM:" + (bot.ButtonMain.state == PI.ButtonState.BUTTON_PRESSED ? "dn " : "up ");
      s += "B1:" + (bot.Button1   .state == PI.ButtonState.BUTTON_PRESSED ? "dn " : "up ");
      s += "B2:" + (bot.Button2   .state == PI.ButtonState.BUTTON_PRESSED ? "dn " : "up ");
      s += "B3:" + (bot.Button3   .state == PI.ButtonState.BUTTON_PRESSED ? "dn " : "up ");
      s += "  ";
      s += "FL:" + bot.DistanceSensorFrontLeft .distance.ToString("0.0").PadLeft(4) + " ";
      s += "FR:" + bot.DistanceSensorFrontRight.distance.ToString("0.0").PadLeft(4) + " ";
      s += "R:"  + bot.DistanceSensorTail      .distance.ToString("0.0").PadLeft(4) + " ";
      s += "  ";
      s += "Tm:" + tis.ToString("0.0").PadLeft(6) + " ";
      s += "\n";
      s += "\n";
      
      foreach (trBehavior trB in CurProgram.UUIDToBehaviorTable.Values) {
        if (trB.Type == trBehaviorType.MAPSET) {
          foreach (trMap trM in trB.MapSet.Maps) {
            trSensor trS = trM.Sensor;
            trS.Robot = bot;
            s += trS.Type.ToString() + ":" + trS.ValueNormalized.ToString("0.00") + "  ";
          }
        }
      }
      s += "\n";
      s += "\n";
    }
    
    s += CurProgram.AllTriggerStatus(robot);
    s += "\n";
    s += "\n";
    
    
    s += CurProgram.ToString(robot);
    s += "\n";
    s += "\n";
    
    if (CurProgram.StateCurrent !=  null) {
      if (CurProgram.StateCurrent.Behavior != null) {
        if (CurProgram.StateCurrent.Behavior.Type == trBehaviorType.MAPSET) {
          s += CurProgram.StateCurrent.Behavior.MapSet.ToString();
        }
      }
    }
    
    UI_Status.text = s;
  }
  
  public void Reset() {
    CurProgram.Reset(CurRobot);
    UpdateDisplay(null);
  }
  
  public void onClick_ProgramPrev() {
    int n = curProgramNum - 1;
    if (n < 1) {
      n = trProgramExamples.Examples.Count;
    }
    SetProgram(n);
  }
  
  public void onClick_ProgramNext() {
    int n = curProgramNum + 1;
    if (n > trProgramExamples.Examples.Count) {
      n = 1;
    }
    SetProgram(n);
  }
  
  public void SetProgram(string name) {
    for (int n = 0; n < trProgramExamples.Examples.Count; ++n) {
      if (trProgramExamples.Examples[n].UserFacingName == name) {
        SetProgram(n + 1);
        return;
      }
    }
    
    WWLog.logError("unknown program: " + name);
  }
  
  public void SetProgram(int num) {
    if (num < 1 || num > trProgramExamples.Examples.Count) {
      WWLog.logError("invalid program number: " + num);
      return;
    }
    
    curProgramNum = num;
    
    CurProgram = trProgramExamples.Examples[num - 1];
    
    Reset();
    UI_ProgramName.text = CurProgram.UserFacingName;

    // verify serialization is consistent.
    trFactory.ForgetItems();
    string s1 = CurProgram.ToJson().ToString();
    trProgram p2 = trFactory.FromJson<trProgram>(JSON.Parse(s1));
    string s2 = p2.ToJson().ToString();
    if (s1 != s2) {
      WWLog.logError("serialization is broken!\ns1 = " + s1 + "\n s2 = " + s2);
    }
    
    CurProgram = p2;
  }
  
  void onConnectRobot(piBotBase robot) {
    if (CurRobot == null) {
      robot.OnState += onRobotState;
      CurRobot = robot;
      CurProgram.Reset(robot);
    }
    else {
      WWLog.logError("connected to a robot while already connected. disconnecting.");
      robot.apiInterface.disconnectRobot(robot.UUID);
    }
  }
  
  
  void onDisconnectRobot(piBotBase robot) {
    if (robot == CurRobot) {
      if (robot != null) {
        robot.OnState -= onRobotState;
      }
      CurRobot = null;
    }
  }
  
  void onRobotState(piBotBase robot) {
    if (robot != CurRobot) {
      WWLog.logError("got state for unexpected robot: " + robot.Name);
      robot.apiInterface.disconnectRobot(robot.UUID);
      return;
    }
    CurProgram.OnRobotState(robot);
    UpdateDisplay(robot);
  }
}
