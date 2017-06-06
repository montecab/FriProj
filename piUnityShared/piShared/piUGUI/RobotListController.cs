using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class RobotListController : MonoBehaviour {

  public GameObject uiPanel_Content;
  public Button     uiButton_ToggleButton;
  public Text       uiText_ToggleButton;
  public Button     uiButton_ScanButton;
  public Text       uiText_ScanButton;
  public GameObject uiPanel_RobotList;
  public GameObject sample_RobotButton;
  public GameObject uiImage_Spinner;
  public GameObject uiButton_QuickScan;
  public int        quickScanTime = 4;
  public bool       forceActive = false; // use in testing app
  
  private bool scanning = false;
  private float quickScanStopTime = -1;
  
  void OnEnable() {
    piConnectionManager.Instance.OnDiscoverRobot   += onDiscoverRobot;
    piConnectionManager.Instance.OnLostRobot       += onLostRobot;
    piConnectionManager.Instance.OnConnectRobot    += onConnectRobot;
    piConnectionManager.Instance.OnDisconnectRobot += onDisconnectRobot;
  }
  
  void OnDisable() {
    if (this.enabled) {
      if (piConnectionManager.Instance){
        piConnectionManager.Instance.OnDiscoverRobot   -= onDiscoverRobot;
        piConnectionManager.Instance.OnLostRobot       -= onLostRobot;
        piConnectionManager.Instance.OnConnectRobot    -= onConnectRobot;
        piConnectionManager.Instance.OnDisconnectRobot -= onDisconnectRobot;
      }
    }
  }

  // Use this for initialization
  void Start () {
    #if (UNITY_IPHONE || UNITY_ANDROID)
    if(!forceActive){
      gameObject.SetActive(false);
    }   
    #else
    uiPanel_Content.SetActive(false);
    
    StartQuickScan();
    
    foreach (piBotBase robot in piConnectionManager.Instance.KnownBotsList) {
      UpdateRobot(robot);
    }

    #endif
  }
  
  // Update is called once per frame
  void Update () {
    int connected = piConnectionManager.Instance.BotsInState(PI.BotConnectionState.CONNECTED ).Count;
    int total = connected;
    total += piConnectionManager.Instance.BotsInState(PI.BotConnectionState.DISCOVERED   ).Count;
    total += piConnectionManager.Instance.BotsInState(PI.BotConnectionState.DISCONNECTED ).Count;
    total += piConnectionManager.Instance.BotsInState(PI.BotConnectionState.DISCONNECTING).Count;
    total += piConnectionManager.Instance.BotsInState(PI.BotConnectionState.CONNECTING   ).Count;
    uiText_ToggleButton.text = "robots (" + connected.ToString() + "/" + total.ToString() + ")";
    uiText_ScanButton.text = scanning ? "stop scan" : "start scan";
    uiButton_ScanButton.GetComponent<Image>().color = scanning ? new Color(1.0f, 0.7f, 0.3f) : Color.white;
    uiText_ScanButton.color = scanning ? new Color(0.7f, 0.0f, 0.0f) : Color.black;
    
    if (scanning && (quickScanStopTime > 0) && (Time.time >= quickScanStopTime)) {
      setScanning(false);
    }
    
    if (connected > 0) {
      uiButton_ToggleButton.GetComponent<Image>().color = new Color(0.7f, 1.0f, 0.7f);
    }
    else {
      uiButton_ToggleButton.GetComponent<Image>().color = Color.white;
    }
  }
  
  
  public void UpdateRobot(piBotBase robot) {
    bool found = false;
    List<RobotButtonController> itemsToRemove = new List<RobotButtonController>();
    
    for (int n = 0; n < uiPanel_RobotList.transform.childCount; ++n) {
      RobotButtonController rbc = uiPanel_RobotList.transform.GetChild(n).GetComponent<RobotButtonController>();
      if (rbc.Robot.UUID == robot.UUID) {
        rbc.Robot = robot;
        found = true;
        if (robot.connectionState == PI.BotConnectionState.LOST) {
          itemsToRemove.Add(rbc);
        }
      }
    }
    
    if (!found && robot.connectionState != PI.BotConnectionState.LOST) {
      GameObject tmp = (GameObject)Instantiate<GameObject>(sample_RobotButton);
      tmp.transform.SetParent(uiPanel_RobotList.transform);
      tmp.transform.localPosition = Vector3.zero;
      tmp.transform.localScale = Vector3.one;
      RobotButtonController rbc = tmp.GetComponent<RobotButtonController>();
      rbc.robotListCtrl = this;
      rbc.Robot = robot;      
    }
    
    foreach (RobotButtonController rbc in itemsToRemove) {
      Object.Destroy(rbc.gameObject);
    }
  }
#region RobotManagerDelegate
  public void onDiscoverRobot(piBotBase robot) {
    UpdateRobot(robot);
    if (IsAutoConnect(robot.UUID)) {
      WWLog.logInfo("autoconnect : " + robot.Name + "  -  " + robot.UUID);
      piConnectionManager.Instance.BotInterface.connectRobot(robot.UUID);
    }
  }
  public void onLostRobot(piBotBase robot) {
    UpdateRobot(robot);
  }  
  public void onConnectRobot(piBotBase robot) {
    UpdateRobot(robot);
  }
  public void onDisconnectRobot(piBotBase robot) {
    UpdateRobot(robot);
  }
#endregion RobotManagerDelegate

  public void OnClickToggleList() {
    uiPanel_Content.SetActive(!uiPanel_Content.activeSelf);
  }

  public void ShowToggleList() {
    uiPanel_Content.SetActive(true);
  }
  
  public void OnClickQuickScan() {
    StartQuickScan();
  }
  
  public void StartQuickScan() {
    quickScanStopTime = Time.time + (float)quickScanTime;
    setScanning(true);
  }
  
  public void setScanning(bool value) {
    if (value) {
      piConnectionManager.Instance.BotInterface.startScan();
    }
    else {
      piConnectionManager.Instance.BotInterface.stopScan();
    }
    scanning = value;
    uiImage_Spinner.SetActive   ( scanning);
    uiButton_QuickScan.SetActive(!scanning);
  }
  
  public void OnClickScan() {
    setScanning(!scanning);
    quickScanStopTime = -1;
  }
  
  private string autoConnectKey(string robotUUID) {
    return "autoconnect_" + robotUUID;
  }
  
  public bool IsAutoConnect(string robotUUID) {
    return PlayerPrefs.HasKey(autoConnectKey(robotUUID));
  }
  
  public void SetAutoConnect(string robotUUID, bool value) {
    string key = autoConnectKey(robotUUID);
    
    if (value) {
      PlayerPrefs.SetFloat(key, (float)piUnityUtils.SystemWallTime);
    }
    else {
      if (PlayerPrefs.HasKey(key)) {
        PlayerPrefs.DeleteKey(key);
      }
    }
    
    PlayerPrefs.Save();
  }
  
}
