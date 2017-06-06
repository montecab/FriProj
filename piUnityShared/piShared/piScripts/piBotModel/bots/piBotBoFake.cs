using UnityEngine;
using System.Collections;
using System.Timers;
using PI;
using WW.SimpleJSON;

public class piBotBoFake : piBotBo {

  const float responseDelayMin = 0.1f;
  const float responseDelayMax = 0.4f;

  // members dealing with synthetic bot shell commands
  public string NextReading_PanAngle = "";
  public string NextReading_PP2      = "";
  public string NextReading_PP3      = "";

  private ButtonState[] fakeDataButtonMain;
  private ButtonState[] fakeDataButton1;
  private ButtonState[] fakeDataButton2;
  private ButtonState[] fakeDataButton3;

  public piBotBoFake(string inUUID, string inName, piRobotType inRobotType, JSONClass jsonRobot=null) : base(inUUID, inName, inRobotType, jsonRobot) {
    fakeDataButtonMain = new ButtonState[] {
      ButtonState.BUTTON_PRESSED,
      ButtonState.BUTTON_NOTPRESSED,
      ButtonState.BUTTON_NOTPRESSED,
      ButtonState.BUTTON_NOTPRESSED,
    };
    fakeDataButton1 = new ButtonState[] {
      ButtonState.BUTTON_PRESSED,
      ButtonState.BUTTON_NOTPRESSED,
      ButtonState.BUTTON_PRESSED,
      ButtonState.BUTTON_PRESSED,
    };
    fakeDataButton2 = new ButtonState[] {
      ButtonState.BUTTON_PRESSED,
      ButtonState.BUTTON_PRESSED,
      ButtonState.BUTTON_NOTPRESSED,
      ButtonState.BUTTON_PRESSED,
    };
    fakeDataButton3 = new ButtonState[] {
      ButtonState.BUTTON_PRESSED,
      ButtonState.BUTTON_PRESSED,
      ButtonState.BUTTON_PRESSED,
      ButtonState.BUTTON_NOTPRESSED,
    };
    doFakeSensors();
  }

  public static bool test() {
    bool pass = true;
    string actualValue;
    actualValue = PIBInterface_internal.shellCommandNormalizeDown("a foo  \n");
    pass = pass && (actualValue == "FOO");
    actualValue = PIBInterface_internal.shellCommandNormalizeDown("foo  \n");
    pass = pass && (actualValue == "FOO");
    actualValue = PIBInterface_internal.shellCommandNormalizeDown("foo");
    pass = pass && (actualValue == "FOO");
    actualValue = PIBInterface_internal.shellCommandNormalizeDown("a foo");
    pass = pass && (actualValue == "FOO");
    return pass;
  }

  public override void cmd_shell(string shellCommand) {
    base.cmd_shell(shellCommand);
    string cmdNorm = PIBInterface_internal.shellCommandNormalizeDown(shellCommand);
    float delay = Random.Range(responseDelayMin, responseDelayMax);
#pragma warning disable

    if (false) {
      // no-op; for code formatting consistency below.
    }

#pragma warning restore
    else if (cmdNorm.IndexOf(piBotConstants_Internal.SHELLCMD_AD_GET_MOT_PAN) == 0) {
      piUnityDelayedExecution.Instance.delayedExecution(delayedResponse, delay, shellCommand, NextReading_PanAngle);
    } else if (cmdNorm.IndexOf(piBotConstants_Internal.SHELLCMD_FETCH_PP3) == 0) {
      piUnityDelayedExecution.Instance.delayedExecution(delayedResponse, delay, shellCommand, NextReading_PP3);
    } else if (cmdNorm.IndexOf(piBotConstants_Internal.SHELLCMD_FETCH_PP2) == 0) {
      piUnityDelayedExecution.Instance.delayedExecution(delayedResponse, delay, shellCommand, NextReading_PP2);
    } else if (cmdNorm.IndexOf(piBotConstants_Internal.SHELLCMD_SET_PP1) == 0) {
      piUnityDelayedExecution.Instance.delayedExecution(delayedResponse, delay, shellCommand, piBotConstants_Internal.SHELLRESPONSE_OK);
    } else if (cmdNorm.IndexOf(piBotConstants_Internal.SHELLCMD_SET_PP2) == 0) {
      piUnityDelayedExecution.Instance.delayedExecution(delayedResponse, delay, shellCommand, piBotConstants_Internal.SHELLRESPONSE_OK);
    } else if (cmdNorm.IndexOf(piBotConstants_Internal.SHELLCMD_SET_PP3) == 0) {
      piUnityDelayedExecution.Instance.delayedExecution(delayedResponse, delay, shellCommand, piBotConstants_Internal.SHELLRESPONSE_OK);
    } else if (cmdNorm.IndexOf(piBotConstants_Internal.SHELLCMD_SET_PP4) == 0) {
      piUnityDelayedExecution.Instance.delayedExecution(delayedResponse, delay, shellCommand, piBotConstants_Internal.SHELLRESPONSE_OK);
    } else if (cmdNorm.IndexOf(piBotConstants_Internal.SHELLCMD_CFG_RELOAD_NVT2) == 0) {
      piUnityDelayedExecution.Instance.delayedExecution(delayedResponse, delay, shellCommand, piBotConstants_Internal.SHELLRESPONSE_OK);
    } else {
      piUnityDelayedExecution.Instance.delayedExecution(delayedResponse, delay, shellCommand, piBotConstants_Internal.SHELLRESPONSE_NG);
    }
  }

  void delayedResponse(string shellCommand, string response) {
    WW.SimpleJSON.JSONArray resultsArray = new WW.SimpleJSON.JSONArray();
    resultsArray.Add(null, response);
    didExecuteShellCommand(shellCommand, resultsArray);
  }

  private int tickNum = 0;
  void doFakeSensors() {
    tickNum += 1;
    // fake buttons.
    ButtonMain.state = fakeDataButtonMain[tickNum % fakeDataButtonMain.Length];
    Button1   .state = fakeDataButton1   [tickNum % fakeDataButton1   .Length];
    Button2   .state = fakeDataButton2   [tickNum % fakeDataButton2   .Length];
    Button3   .state = fakeDataButton3   [tickNum % fakeDataButton3   .Length];
    // fake distance sensors
    float f = Time.timeSinceLevelLoad * Mathf.PI * 2.0f;
    float distMin =  3.0f;
    float distMax = 50.0f;
    DistanceSensorFrontLeft .distance = Mathf.Lerp(distMin, distMax, Mathf.Sin(f / 5.0f + (Mathf.PI * 2.0f / 3.0f * 0.0f)) * 0.5f + 0.5f);
    DistanceSensorFrontRight.distance = Mathf.Lerp(distMin, distMax, Mathf.Sin(f / 5.0f + (Mathf.PI * 2.0f / 3.0f * 1.0f)) * 0.5f + 0.5f);
    DistanceSensorTail      .distance = Mathf.Lerp(distMin, distMax, Mathf.Sin(f / 5.0f + (Mathf.PI * 2.0f / 3.0f * 2.0f)) * 0.5f + 0.5f);
    // fake kidnap sensors
    KidnapSensor.flag = (tickNum / 4) % 2 == 0 ? false : true;
    // fake clap
    SoundSensor.eventId = (tickNum % 5 == 0 ? PI.SoundEventIndex.SOUND_EVENT_CLAP : PI.SoundEventIndex.SOUND_EVENT_NONE);
    // okay, go crazy.
    handleState(new WW.SimpleJSON.JSONClass());
    // see you soon..
    piUnityDelayedExecution.Instance.delayedExecution0(doFakeSensors, 1);
  }
}


