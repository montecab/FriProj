using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using PI;

public class RobotButtonController : MonoBehaviour {

  private piBotBase          robot;
  public Text                label;
  public RawImage            robotIcon;
  public Texture             textureDash;
  public Texture             textureDot;
  public Texture             textureUnknown;
  public Toggle              uiToggle_AutoConnect;
  public Image               chargingIcon;
  public Text                serialLabel;
  public Text                versionLabel;
  public RobotListController robotListCtrl;

  private bool ignoreToggle = false;
  
  // Use this for initialization
  void Start () {  
  }
  
  // Update is called once per frame
  void Update () {
    updateBatteryIcon();

    //these could change in the middle of play, currently only in testing app 
    label.text = robot.Name;
    serialLabel.text = robot.SerialNumber;
    versionLabel.text = robot.FwRsVersionAndLocale;
  }
  
  public piBotBase Robot {
    set {
      if (robot == null) {
        bool auto = robotListCtrl.IsAutoConnect(value.UUID);
        ignoreToggle = true;
        uiToggle_AutoConnect.isOn = auto;
        ignoreToggle = false;
      }
      
      robot = value;

      label.text = robot.Name;
      serialLabel.text = robot.SerialNumber;
      versionLabel.text = robot.FwRsVersionAndLocale;

      Color bgColor;

      switch (robot.connectionState) {
      default:
      case BotConnectionState.UNKNOWN:
        bgColor = new Color(1.0f, 0.0f, 0.0f);
        break;
      case BotConnectionState.CONNECTING:
      case BotConnectionState.DISCONNECTING:
      case BotConnectionState.LOST:
        bgColor = new Color(1.0f, 0.5f, 0.5f);
        break;

      case BotConnectionState.FAILEDTOCONNECT:
      case BotConnectionState.DISCONNECTED:
      case BotConnectionState.DISCOVERED:
        bgColor = new Color(1.0f, 1.0f, 1.0f);
        break;

      case BotConnectionState.CONNECTED:
        bgColor = new Color(0.5f, 1.0f, 0.5f);
        break;
      }

      GetComponent<Image>().color = bgColor;

      if (robot.robotType == piRobotType.DOT) {
        robotIcon.texture = textureDot;
      }
      else if (robot.robotType == piRobotType.DASH) {
        robotIcon.texture = textureDash;
      }
      else {
        robotIcon.texture = textureUnknown;
      }
    }
    get {
      return robot;
    }
  }

  public void onClick() {
    bool connect    = false;
    bool disconnect = false;

    if (robot == null) {
      // this can happen when reloading scripts while running the app.
      WWLog.logError("robot became null. destroying self.");
      GameObject.Destroy(gameObject);
      return;
    }

    switch (robot.connectionState) {
      default:
      case BotConnectionState.UNKNOWN:
      case BotConnectionState.CONNECTING:
      case BotConnectionState.DISCONNECTING:
      case BotConnectionState.LOST:
        break;

      case BotConnectionState.FAILEDTOCONNECT:
      case BotConnectionState.DISCONNECTED:
      case BotConnectionState.DISCOVERED:
        connect = true;
        break;
        
      case BotConnectionState.CONNECTED:
        disconnect = true;
        break;
    }
    
    if (connect) {
      WWLog.logInfo("connect : " + Robot.Name + "  -  " + Robot.UUID);
      piConnectionManager.Instance.BotInterface.connectRobot(Robot.UUID);
    }
    if (disconnect) {
      WWLog.logInfo("disconnect : " + Robot.Name + "  -  " + Robot.UUID);
      piConnectionManager.Instance.BotInterface.disconnectRobot(Robot.UUID);
    }
  }
  
  public void onClickAutoConnect() {
    if (ignoreToggle) {
      return;
    }
    robotListCtrl.SetAutoConnect(robot.UUID, uiToggle_AutoConnect.isOn);
    
    if ((Robot.connectionState != BotConnectionState.CONNECTED) && uiToggle_AutoConnect.isOn) {
      WWLog.logInfo("connect : " + Robot.Name + "  -  " + Robot.UUID);
      piConnectionManager.Instance.BotInterface.connectRobot(Robot.UUID);
    }
  }

  private void updateBatteryIcon() {
    chargingIcon.gameObject.SetActive(false);

    piBotCommon bot = (piBotCommon)robot;

    if (bot == null) {
      return;
    }
    else if (bot.Battery.charging) {
      chargingIcon.color = Color.black;
      chargingIcon.gameObject.SetActive(true);
    }
    else if (bot.Battery.level == piBotComponentBattery.Level.WW_BATTERY_LEVEL_NORMAL) {
      // do nothing
    }
    else {
      float f = (Mathf.Sin(Time.time * Mathf.PI * 2f) * 0.5f + 0.5f);
      if (bot.Battery.level == piBotComponentBattery.Level.WW_BATTERY_LEVEL_WARNING) {
        chargingIcon.color = new Color(0.7f, 0.5f, 0.3f, f);
      }
      else {
        chargingIcon.color = new Color(1.0f, 0.0f, 0.0f, f);
      }

      chargingIcon.gameObject.SetActive(true);
    }
  }
}

