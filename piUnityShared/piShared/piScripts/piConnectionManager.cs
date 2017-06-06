using UnityEngine;
using System.Collections.Generic;
using WW.SimpleJSON;
using System.Reflection;

/// <summary>
///
/// piConnectionManager interfaces with the PIBInterface class to manage connections with robots.
/// It tracks discovered robot/s (just a single robot as of this writing), and facilitates connecting & disconnecting.
/// It provides no GUI, but see piConnectionManagerExampleUI for an example.
/// This script must be attached to a gameObject, and will force that game object's name.
///
/// TODO:
/// - extend the class to connect to multiple robots.
/// - as part of the previous, robot commands will need to be per-robot.
/// 
/// </summary>

public class piConnectionManager : MonoBehaviour {

  public enum ConnectedRobotType {
    UNKNOWN,
    DASH_ONLY,
    DOT_ONLY,
    BOTH
  };

  public class UpdateInfo{
    public string state;
    public string currentFileName;
    public int currentFilePercent;
    public int totalPercent;

    public int fwRetryCount = 0;
    public int rsRetryCount = 0;
    public int rsOpRetryCount = 0;
    public int reconnectRetryCount = 0;

    public static UpdateInfo FromJson(JSONClass json){
      UpdateInfo info  = new UpdateInfo();
      info.state = json["state"];
      info.currentFileName = json["fileName"];
      info.currentFilePercent = json["filePercent"].AsInt;
      info.totalPercent = json["percent"].AsInt;
      info.fwRetryCount = json["fwRetry"].AsInt;
      info.rsRetryCount = json["rsRetry"].AsInt;
      info.rsOpRetryCount = json["rsOpRetry"].AsInt;
      info.reconnectRetryCount = json["reconnectRetry"].AsInt;

      return info;
    }
  }
  private static string singletonName = "ThePIConnectionManager";
  private static piConnectionManager instance;
  
  public delegate void OnSoundRecordingFinishedDelegate(int soundId);
  public OnSoundRecordingFinishedDelegate OnSndRecordFinish;

  public delegate void OnChromeClosedDelegate();
  public OnChromeClosedDelegate OnChromeClose;

  public delegate void OnChromeOpenedDelegate();
  public OnChromeOpenedDelegate OnChromeOpen;
  
  public delegate void OnConnectRobotDelegate(piBotBase robot);
  public event OnConnectRobotDelegate OnConnectRobot;
  public delegate void OnDisconnectRobotDelegate(piBotBase robot);
  public event OnDisconnectRobotDelegate OnDisconnectRobot;
  public delegate void OnConnectionFailedDelegate(piBotBase robot);
  public event OnConnectionFailedDelegate OnConnectionFailed;
  public delegate void OnDiscoverRobotDelegate(piBotBase robot);
  public event OnDiscoverRobotDelegate OnDiscoverRobot;
  public delegate void OnLostRobotDelegate(piBotBase robot);
  public event OnLostRobotDelegate OnLostRobot;
  public delegate void OnReceiveUpdateInfoDelegate(piBotBase robot, UpdateInfo info);
  public event OnReceiveUpdateInfoDelegate OnReceiveUpdateInfo;
  public delegate void OnUpdateFinishDelegate(piBotBase robot, bool isSuccess);
  public event OnUpdateFinishDelegate OnUpdateFinish;
  
  public static piConnectionManager Instance {
    get {
      if (applicationIsQuitting) {
        // including this print statement seems to crash unity editor when application quits.
        // WWLog.logError("accessing " + typeof(piConnectionManager).Name + " singleton while shutting down. returning null.");
        return null;
      }
      if (instance == null) {
        instance = FindObjectOfType(typeof(piConnectionManager)) as piConnectionManager;
        if (instance == null) {
          GameObject go = new GameObject(singletonName);
          instance = go.AddComponent<piConnectionManager>();
          if (Application.isPlaying) {
            DontDestroyOnLoad(go);
          }
        }
        instance.init();
      }
      return instance;
    }
  }

  public void ResetRobotsState(){
    foreach(piBotBo robot in BotsInState(PI.BotConnectionState.CONNECTED)){
      robot.Reset();
    }
  }
  
  public void init(){
    if (botInterface == null) {
      botInterface = new PIBInterface.Actions(singletonName);
    }
  }
  
  public PIBInterface.Actions BotInterface {
    get {
      init();
      return botInterface;
    }
  }
  
  private static bool applicationIsQuitting = false;
  /// <summary>
  /// When Unity quits, it destroys objects in a random order.
  /// In principle, a Singleton is only destroyed when application quits.
  /// If any script calls Instance after it have been destroyed, 
  ///   it will create a buggy ghost object that will stay on the Editor scene
  ///   even after stopping playing the Application. Really bad!
  /// So, this was made to be sure we're not creating that buggy ghost object.
  /// </summary>

  public static List<piAPIMessageDelegateInterface> listeners = new List<piAPIMessageDelegateInterface>();
  
  private Dictionary<string, piBotBo>knownBots = new Dictionary<string, piBotBo>();
  
  private PIBInterface.Actions botInterface = null;
    
  public void Awake() {
    this.name = singletonName;
    piUnityUtils.CheckCompilePlatformEqualsRuntimePlatform(true);
  }
  
  public void OnApplicationQuit() {
    applicationIsQuitting = true;
    BotInterface.stopScan();
    DisconnectAllRobots();
  }

  public ConnectedRobotType GetConnectedRobotType(){
    ConnectedRobotType result = ConnectedRobotType.UNKNOWN;
    bool hasDash = false;
    bool hasDot = false;

    foreach(var robot in BotsInState(PI.BotConnectionState.CONNECTED)){
      if (robot.robotType == piRobotType.DASH){
        hasDash = true;
        result = ConnectedRobotType.DASH_ONLY;
      } else if (robot.robotType == piRobotType.DOT){
        hasDot = true;
        result = ConnectedRobotType.DOT_ONLY;
      } else {
        WWLog.logError("unhandled robot type: " + (int)robot.robotType);
      }

      if (hasDot && hasDash){
        result = ConnectedRobotType.BOTH;
        break;
      }
    }
    return result;
  }
  
  public  Dictionary<string, piBotBo>.ValueCollection KnownBotsList {
    get {
      return knownBots.Values;
    }
  }
  
  public int NumKnownBots {
    get {
      return knownBots.Count;
    }
  }
  
  public List<piBotBo> BotsInState(PI.BotConnectionState state) {
    return RobotsInState(state);
  }

  public List<piBotBo> RobotsInState(PI.BotConnectionState state) {
    List<piBotBo> ret = new List<piBotBo>();
    
    foreach (piBotBo bot in knownBots.Values) {
      if (bot.connectionState == state) {
        ret.Add(bot);
      }
    }
    
    return ret;
  }
  
  public void DisconnectAllRobots() {
    foreach (piBotBase bot in BotsInState(PI.BotConnectionState.CONNECTED)) {
      botInterface.disconnectRobot(bot.UUID);
    }
    foreach (piBotBase bot in BotsInState(PI.BotConnectionState.CONNECTING)) {
      botInterface.disconnectRobot(bot.UUID);
    }
  }
  
  // legacy routine
  public piBotBo AnyConnectedBo {
    get {
      foreach (piBotBo bot in knownBots.Values) {
        if (bot.connectionState == PI.BotConnectionState.CONNECTED) {
          return bot;
        }
      }
      return null;
    }
  }
  
  public piBotBo KnownBot(string uuId) {
    if (knownBots.ContainsKey(uuId)) {
      return knownBots[uuId];
    }
    else {
      return null;
    }
  }

  public piBotBase FirstConnectedRobot {
    get {
      List<piBotBo> connected = BotsInState(PI.BotConnectionState.CONNECTED);
      piBotBase result = null;
      if (connected.Count > 0){
        result = connected[0];
      }
      return result;
    }
  }

  public piBotBase FirstConnectedRobotOfType(piRobotType robotType) {
    List<piBotBo> connected = BotsInState(PI.BotConnectionState.CONNECTED);
    piBotBase result = null;
    if (connected.Count > 0){
      foreach (piBotBo robot in connected) {
        if (robot.robotType == robotType) {
          result = robot;
          break;
        }
      }
    }
    return result;
  }

  public piBotBase LastConnectedRobot {
    get {
      List<piBotBo> connected = BotsInState(PI.BotConnectionState.CONNECTED);
      piBotBase result = null;
      if (connected.Count > 0){
        result = connected[connected.Count - 1];
      }
      return result;
    }
  }
  
  public int NumberOfConnectedRobotsOfType(piRobotType robotType) {
    int ret = 0;
    
    foreach (piBotBo bot in BotsInState(PI.BotConnectionState.CONNECTED)) {
      if (bot.robotType == robotType) {
        ret += 1;
      }
    }
    
    return ret;
  }
  
  private piBotBo findOrCreateBot(JSONClass jsonRobot) {
    string uuId = jsonRobot["uuId"];
    piBotBo bot = KnownBot(uuId);
    if (bot == null) {
      piRobotType robotType = (piRobotType)jsonRobot["type"].AsInt;
      // todo - currently all the code uses piBotBo.
      // instead of doing a huge refactor to have both piBotDash and piBotBo all over the place,
      // just setting the field in piBotBo to either Dash or Dot:
      if ((robotType != piRobotType.DASH) && (robotType != piRobotType.DOT) && (robotType != piRobotType.D2)
        && (robotType != piRobotType.DASH_DFU) && (robotType != piRobotType.DOT_DFU) && (robotType != piRobotType.D2_DFU)) {
        WWLog.logError("bad robot type: " + robotType.ToString() + "   " + jsonRobot["type"]);
      }
      bot = new piBotBo(uuId, jsonRobot["name"], robotType, jsonRobot);
      bot.apiInterface = this.BotInterface;
    } else {
      // Update advertised info for unconnected robots only
      // since connected robots advertisement data isn't updated in API 
      // until disconnected and reconnected
      if (bot.connectionState != PI.BotConnectionState.CONNECTED) {
        bot.Name = jsonRobot ["name"];
        bot.UpdateAdvertisedInfo(jsonRobot);
      }
    }
    return bot;
  }  
  
  private void forgetRobot(string UUID) {
    if (knownBots.ContainsKey(UUID)) {
      knownBots.Remove(UUID);
    }
  }
  
  void onRobotManager_didDiscoverRobot(JSONClass message) {
    JSONClass jsonRobot = message["robot"].AsObject;
    piBotBo bot = findOrCreateBot(jsonRobot);
    if (bot.connectionState == PI.BotConnectionState.CONNECTED) {
      // we're good.
    }
    else {
      bot.connectionState = PI.BotConnectionState.DISCOVERED;
    }
    knownBots[bot.UUID] = bot;
    
    if (OnDiscoverRobot != null) {
      OnDiscoverRobot(bot);
    }
  }
  
  void onRobotManager_didUpdateDiscoveredRobots(JSONClass message) {
    onRobotManager_didDiscoverRobot(message);
  }
  
  void onRobotManager_didLoseRobot(JSONClass message) {
    JSONClass jsonRobot = message["robot"].AsObject;
    
    piBotBo bot = findOrCreateBot(jsonRobot);
    bot.connectionState = PI.BotConnectionState.LOST;
    knownBots[bot.UUID] = bot;

    if (OnLostRobot != null) {
      OnLostRobot(bot);
    }
  }
  
  void onRobotManager_didConnectWithRobot(JSONClass message) {
    Debug.Log ("Unity did connect to robot" + message);
    JSONClass jsonRobot = message["robot"].AsObject;
    piBotBo bot = findOrCreateBot(jsonRobot);
    bot.connectionState = PI.BotConnectionState.CONNECTED;
    bot.tareWheels();
    //this is added for chrome integration
    if (knownBots.ContainsKey(bot.UUID) ==false) {
      knownBots[bot.UUID] = bot;
    }
    
    if (OnConnectRobot != null) {
      OnConnectRobot(bot);
    }
  }
  
  void onRobotManager_didFailToConnectWithRobot(JSONClass message) {
    JSONClass jsonRobot = message["robot"].AsObject;
    piBotBo bot = findOrCreateBot(jsonRobot);
    bot.connectionState = PI.BotConnectionState.FAILEDTOCONNECT;

    if (OnConnectionFailed != null) {
      OnConnectionFailed(bot);
    }
  }
  
  void onRobotManager_didDisconnectWithRobot(JSONClass message) {
    JSONClass jsonRobot = message["robot"].AsObject;
    piBotBo bot = findOrCreateBot(jsonRobot);
    bot.connectionState = PI.BotConnectionState.DISCONNECTED;
    
    if (OnDisconnectRobot != null) {
      OnDisconnectRobot(bot);
    }
  }

  void onRobotManager_didReceiveUpdateInfo(JSONClass message){
    //Debug.Log ("Robot Sensor: " + message);
    JSONClass jsonRobot = message["robot"].AsObject;
    piBotBo bot = findOrCreateBot(jsonRobot);
    if (bot.connectionState != PI.BotConnectionState.CONNECTED) {
      // this can happen in DLL mode when reloading scripts while running the app and connected to a robot. 
      Debug.LogWarning("unexpected: didReceiveUpdateInfo received message for robot in state:" + bot.connectionState);
      #if UNITY_STANDALONE || UNITY_EDITOR
      Debug.LogWarning(".. disconnecting.");
      BotInterface.disconnectRobot(bot.UUID);
      forgetRobot(bot.UUID);
      #endif
    }
    if(OnReceiveUpdateInfo != null){
      JSONClass jsonInfo = message["info"].AsObject;
      OnReceiveUpdateInfo(bot, UpdateInfo.FromJson(jsonInfo));
    }

  }
  
  void onRobotManager_didReceiveRobotState(JSONClass message) {
    JSONClass state = message["state"].AsObject;
    //Debug.Log ("Robot Sensor: " + message);
    JSONClass jsonRobot = message["robot"].AsObject;
    piBotBo bot = findOrCreateBot(jsonRobot);
    if (bot.connectionState != PI.BotConnectionState.CONNECTED) {
      // this can happen in DLL mode when reloading scripts while running the app and connected to a robot. 
      Debug.LogWarning("unexpected: didReceiveRobotState received message for robot in state:" + bot.connectionState);
  #if UNITY_STANDALONE || UNITY_EDITOR
      Debug.LogWarning(".. disconnecting.");
      BotInterface.disconnectRobot(bot.UUID);
      forgetRobot(bot.UUID);
  #endif
    }
    bot.handleState(state);
  }
  
  void onRobotManager_didReceiveRobotEvents(JSONClass message) {
    JSONClass jsonRobot = message["robot"].AsObject;
    piBotBo bot = findOrCreateBot(jsonRobot);
    if (bot.connectionState != PI.BotConnectionState.CONNECTED) {
      // this can happen in DLL mode when reloading scripts while running the app and connected to a robot. 
      Debug.LogWarning("unexpected: received event for robot in state:" + bot.connectionState);
  #if UNITY_STANDALONE || UNITY_EDITOR
      Debug.LogWarning(".. disconnecting.");
      BotInterface.disconnectRobot(bot.UUID);
      forgetRobot(bot.UUID);
  #endif
    }
    bot.Name = jsonRobot["name"];
    JSONArray events = message["events"].AsArray;
    bot.handleEvents(events);
  }
  
  void onRobotManager_didStopCommandSequence(JSONClass message) {
    // todo: probably need to refactor this stuff out
    JSONClass jsonRobot = message["robot"].AsObject;
    piBotBo bot = findOrCreateBot(jsonRobot);
    
    int sequenceId = message["sequenceId"].AsInt;
    
    string anim = BotInterface.findAnimForAnimID((uint)sequenceId);
    
    // todo: add an event-listener system.
    if (bot != null || anim != null) {
      piRobotAnimSoundManager.Instance.informRobotStoppedAnimation(bot, anim);
    }
  }
  
  void onRobotManager_didFinishCommandSequence(JSONClass message) {
    // todo: probably need to refactor this stuff out
    JSONClass jsonRobot = message["robot"].AsObject;
    piBotBo bot = findOrCreateBot(jsonRobot);
    
    int sequenceId = message["sequenceId"].AsInt;
    
    string anim = BotInterface.findAnimForAnimID((uint)sequenceId);
    
    // todo: add an event-listener system.
    if (bot != null && anim != null) {
      piRobotAnimSoundManager.Instance.informRobotStoppedAnimation(bot,anim);
      bot.informRobotStoppedAnimation(anim);
    }
  }
  
  void onRobotManager_didExecuteShellCommand(JSONClass message) {
    string    command = message["command"].ToString();
    string results = message["results"].Value;
    
    JSONClass jsonRobot = message["robot"].AsObject;
    piBotBo bot = findOrCreateBot(jsonRobot);
    if (bot.connectionState != PI.BotConnectionState.CONNECTED) {
      Debug.LogWarning("unexpected: executed command for robot in state:" + bot.connectionState);
    }
    bot.didExecuteShellCommand(command, results);
  }
  
  void onRobotManager_didFailShellCommand(JSONClass message) {
    string    command = message["command"].ToString();
    string    error   = message["error"  ].ToString();
    
    JSONClass jsonRobot = message["robot"].AsObject;
    piBotBo bot = findOrCreateBot(jsonRobot);
    if (bot.connectionState != PI.BotConnectionState.CONNECTED) {
      Debug.LogWarning("unexpected: failed command for robot in state:" + bot.connectionState);
    }
    bot.didFailShellCommand(command, error);
  }
  
  void onRobotManager_onChromeClosed(){
    if(OnChromeClose!= null){
      OnChromeClose();
    }
  }

  void onRobotManager_onChromeOpened () {
    if(OnChromeOpen!= null){
      OnChromeOpen();
    }
  }

  void onRobotManager_didFinishUpdate(JSONClass message){
    JSONClass jsonRobot = message["robot"].AsObject;
    piBotBo bot = findOrCreateBot(jsonRobot);
    if (bot.connectionState != PI.BotConnectionState.CONNECTED) {
      // this can happen in DLL mode when reloading scripts while running the app and connected to a robot. 
      Debug.LogWarning("unexpected: didFinishUpdate received message for robot in state:" + bot.connectionState);
      #if UNITY_STANDALONE || UNITY_EDITOR
      Debug.LogWarning(".. disconnecting.");
      BotInterface.disconnectRobot(bot.UUID);
      forgetRobot(bot.UUID);
      #endif
    }
    if(OnUpdateFinish != null){
      bool success = message["success"].AsBool;
      OnUpdateFinish(bot, success);
    }
  }
  
  void onRobotManager_didSoundTranferingFinished(JSONClass message){
    int soundIndex = message["sound_id"].AsInt;
    
    if(OnSndRecordFinish!= null){
      OnSndRecordFinish(soundIndex);
    }  
    
  }
  
  public void _injectOnPIRobotManagerDelegate(string jsonString) {
    onPIRobotManagerDelegate(jsonString);
  }
  
  void onPIRobotManagerDelegate(string jsonString) {
//        Debug.Log("Unity Got Message: " + jsonString);
    
    JSONClass message = JSON.Parse(jsonString).AsObject;
    
    // todo: replace this w/ a dictionary of message names -> methods.
    switch(message["method"]) {
    default:
      Debug.LogError("Unknown method: " + message["method"]);
      break;
    case "didDiscoverRobot":
      onRobotManager_didDiscoverRobot(message);
      break;
    case "didUpdateDiscoveredRobots":
      onRobotManager_didUpdateDiscoveredRobots(message);
      break;
    case "didLoseRobot":
      onRobotManager_didLoseRobot(message);
      break;
    case "didConnectWithRobot":
      onRobotManager_didConnectWithRobot(message);
      break;
    case "didFailToConnectWithRobot":
      onRobotManager_didFailToConnectWithRobot(message);
      break;
    case "didDisconnectWithRobot":
      onRobotManager_didDisconnectWithRobot(message);
      break;
    case "didReceiveRobotState":
      onRobotManager_didReceiveRobotState(message);
      break;
    case "didReceiveRobotEvents":
      onRobotManager_didReceiveRobotEvents(message);
      break;
    case "didStopCommandSequence":
      onRobotManager_didStopCommandSequence(message);
      break;
    case "didFinishCommandSequence":
      onRobotManager_didFinishCommandSequence(message);
      break;
    case "didExecuteShellCommand":
      onRobotManager_didExecuteShellCommand(message);
      break;
    case "didFailShellCommand":
      onRobotManager_didFailShellCommand(message);
      break;
    case "didSoundTranferingFinished":
      onRobotManager_didSoundTranferingFinished(message);
      break;
    case "onChromeClosed":
      onRobotManager_onChromeClosed();
      break;
    case "onChromeOpened":
      onRobotManager_onChromeOpened();
      break;
    case "didReceiveUpdateInfo":
      onRobotManager_didReceiveUpdateInfo(message);
      break;
    case "didFinishUpdate":
      onRobotManager_didFinishUpdate(message);
      break;
    }
    
    foreach (piAPIMessageDelegateInterface listener in listeners) {
      listener.onPIMessage(message);
    }
  }

  public void openPrivacyWindow(){
    BotInterface.openPrivacyWindow();
  }
  
  public void openChrome(){
    BotInterface.openChrome();
  }

  public void showConnectToRobotDialog(piRobotType robotType){
    BotInterface.showConnectToRobotDialog((int)robotType);
  }

  public void switchRobot(string robotUUID) {
    BotInterface.switchRobot(robotUUID);
  }
  
  public void showChromeButton(){
    BotInterface.showChromeButton ();
  }
  
  public void hideChromeButton(){
    BotInterface.hideChromeButton ();
  }

  public void scheduleLocalNotification(string title, string body, string jsonAction, System.DateTime utcTime){
    WWLog.logInfo("Send local notification at Time: " + utcTime.ToLocalTime().ToString() + " with Title: " + title + ", Body: " + body);
    BotInterface.scheduleLocalNotification (title, body, jsonAction, utcTime);
  }

  public void showSystemDialog(string title, string body="", string buttonText="OK"){
    BotInterface.showSystemDialog(title, body, buttonText);
  }

  public void startDownloadLocaFiles(string appName, string version, bool isZip, string fileName, string path){
    PIBInterface_internal.startDownloadLocaFiles(appName, version, isZip, fileName, path);
  }

  public bool isCacheVersionMatched (string appName, string version){
    return PIBInterface_internal.isCacheVersionMatched(appName, version);
  }
    
  //set the chrome FTUE mode
  public void setFTUEMode(bool isFTUE){
    PIBInterface_internal.setFTUEMode(isFTUE);
  }

  public bool isAutoConnectInfoEmpty(){
    return PIBInterface_internal.isAutoConnectInfoEmpty();
  }

  public void startUpdateRobot(piBotBase robot, string lang, bool forceFwUpdate, bool forceRsUpdate){
    PIBInterface_internal.startUpdateRobot(robot.UUID, lang, forceFwUpdate, forceRsUpdate);
  }

  public void catchLogs(string path){
    PIBInterface_internal.catchLogs(path);
  }

  public void showFile(string path){
    PIBInterface_internal.showFile(path);
  }
}


