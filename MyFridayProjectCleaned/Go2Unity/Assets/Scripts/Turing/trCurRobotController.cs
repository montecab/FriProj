using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using WW;

namespace Turing{
  public class trCurRobotController : Singleton<trCurRobotController> {
    public piBotBase CurRobot = null;
    public piConnectionManager.OnConnectRobotDelegate onConnectCurRobot;
    public piConnectionManager.OnDisconnectRobotDelegate onDisConnectCurRobot;
    public bool CheckRobotType = false;

    private bool _isChromeOpen = false;

    void Awake () {
      trDataManager.Instance.Init();
      piConnectionManager.Instance.OnConnectRobot    += onConnectRobot;
      piConnectionManager.Instance.OnDisconnectRobot += onDisconnectRobot;
      piConnectionManager.Instance.OnChromeClose += onChromeClose;
      CheckConnectRobot();
    }

    void OnDisable(){
      if (piConnectionManager.Instance){
        piConnectionManager.Instance.OnConnectRobot    -= onConnectRobot;
        piConnectionManager.Instance.OnDisconnectRobot -= onDisconnectRobot;
      }
    }

    public void CheckConnectRobot(){
      piBotBase firstConnectedRobot = null;
      if(CheckRobotType){
        if(CurRobot == null || CurRobot.robotType != trDataManager.Instance.CurrentRobotTypeSelected){
          firstConnectedRobot = piConnectionManager.Instance.FirstConnectedRobotOfType(trDataManager.Instance.CurrentRobotTypeSelected);
        }
      }
      else{
        if(CurRobot == null){
          firstConnectedRobot = piConnectionManager.Instance.FirstConnectedRobot;
        }
      }
      if (firstConnectedRobot != null){
        onConnectRobot(firstConnectedRobot);
      }
    }

    public void CheckOpenChrome(){
      if(CurRobot == null && !_isChromeOpen){
        _isChromeOpen = true;
        piConnectionManager.Instance.openChrome();
      }
    }

    private void onChromeClose(){
      _isChromeOpen = false;
    }

    private void onDisconnectRobot(piBotBase robot) {
      if (robot == CurRobot) {
        CurRobot = null;

        // select of the other robots which may be connected.    
        List<piBotBo> connectedRobots = piConnectionManager.Instance.BotsInState(PI.BotConnectionState.CONNECTED);
        for (int n = connectedRobots.Count - 1; n >= 0; --n) {
          piBotBase oldBot = connectedRobots[n];
          if (oldBot != robot) {
            WWLog.logInfo(robot.Name + " disconnected. selecting " + oldBot.Name);
            onConnectRobot(oldBot);
            break;
          }
        }
      }

      if(onDisConnectCurRobot != null){
        onDisConnectCurRobot(robot);
      }
    }

    private void onConnectRobot(piBotBase robot) {

      if (robot == null) {
        return;
      }
      if (CheckRobotType && robot.robotType != trDataManager.Instance.CurrentRobotTypeSelected) {
        return;
      }
      if (CurRobot != null) {
        CurRobot.Reset();
      }

      CurRobot = robot;

      #if UNITY_IOS || UNITY_ANDROID
      piConnectionManager.Instance.switchRobot(robot.UUID);
      #endif

      if(onConnectCurRobot != null){
        onConnectCurRobot(robot);
      }
    }

  }
}

