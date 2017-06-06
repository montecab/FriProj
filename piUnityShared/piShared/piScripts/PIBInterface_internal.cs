using UnityEngine;
using System;
using System.Runtime.InteropServices;

#if CHROME_UNITY
using ChromeUnity;
#endif

/*
	@travis - PIBInterface_internal is secret API commands, not included in the public PIBInterface. sendRawPacket is required for LoopIt.
*/


public class PIBInterface_internal{
  #if UNITY_IPHONE
  const string LIBNAME_API = "__Internal";

#else // Mac Editor and Mac Standalone
  const string LIBNAME_API = "APIObjectiveC";
  #endif

  #if CHROME_UNITY

  private static void PIRobot_onClickSoundRecordButton(){
    ChromeController.instance.OnClickSoundRecordButton();
  }

  private static void PIRobot_onClickPuppetRecordButton(){
    ChromeController.instance.OnClickPuppetRecordButton();
  }

  private static void PIRobot_setEnableSensorPackets(bool enabled){
    ChromeController.instance.SetEnableSensorPackets(enabled);
  }

  #endif

  #if UNITY_ANDROID
  private static AndroidJavaClass jc = new AndroidJavaClass ("com.makewonder.wwUnityWrapper");

  #region robot_update

  private static void PIRobot_startUpdate(string robotUUID, string lang, bool forceFwUpdate, bool forceRsUpdate){
    jc.CallStatic("startUpdate", robotUUID, lang, forceFwUpdate, forceRsUpdate);
  }

  private static bool PIRobot_isUpdating(string robotUUID){
    return jc.CallStatic<bool>("isUpdating", robotUUID);
  }


  private static void PIRobot_catchLogs(string path){
    jc.CallStatic("catchLogs", path);
  }

  private static void PIRobot_showFile(string path){
    jc.CallStatic("showFile", path);
  }

  private static int PIRobot_getUpdateProgressPercent(string robotUUID){
    return jc.CallStatic<int>("getUpdateProgressPercent");
  }

  #endregion

  private static void PIRobot_shellCommand(string robotUUID, string shellCommand){
    jc.CallStatic("shellCommand", robotUUID, shellCommand);
  }

  private static int PIRobot_pendingShellCommandCount(string robotUUID){
    //jc.CallStatic ("pendingShellCommandCount", robotUUID);  // not actually implemented yet.
    return 0; //fake
  }

  #if !CHROME_UNITY
  private static void PIRobot_setEnableSensorPackets(bool enabled){
    jc.CallStatic ("setEnableSensorPackets", enabled);
  }

  private static void PIRobot_onClickSoundRecordButton(){
    jc.CallStatic("openVoiceRecording");
  }

  private static void PIRobot_onClickPuppetRecordButton(){
    WWLog.logError("Not Implemented: PIRobot_onClickSoundRecordButton");
    // jc.CallStatic("openPuppetRecording");
  }
  #endif

  // private static void PIRobot_setAllowAutoRotate(bool allow){} // not implemented.

  #else // UNITY_IOS, UNITY_EDITOR
  [DllImport (LIBNAME_API)]
	private extern static void PIRobot_shellCommand([MarshalAs(UnmanagedType.LPStr)] string robotUUID,
	                                                [MarshalAs(UnmanagedType.LPStr)] string shellCommand);
  
  [DllImport (LIBNAME_API)]
  private extern static int PIRobot_pendingShellCommandCount([MarshalAs(UnmanagedType.LPStr)] string robotUUID);
  
  
#if !CHROME_UNITY
  [DllImport (LIBNAME_API)]
  private extern static void PIRobot_onClickSoundRecordButton();

  [DllImport (LIBNAME_API)]
  private extern static void PIRobot_onClickPuppetRecordButton();

  [DllImport (LIBNAME_API)]
  private extern static void PIRobot_setAllowAutoRotate([MarshalAs(UnmanagedType.Bool)] bool allow);

  private static void PIRobot_setEnableSensorPackets(bool enabled){}
  #endif
  
  

#region robot_update

  
  [DllImport(LIBNAME_API)]
  private extern static bool PIRobot_isUpdating([MarshalAs(UnmanagedType.LPStr)] string robotUUID);

  
#if UNITY_IPHONE
  [DllImport(LIBNAME_API)]
  private extern static void PIRobot_loadResourceBundle();
  [DllImport(LIBNAME_API)]
  private extern static void PIRobot_startUpdate([MarshalAs(UnmanagedType.LPStr)] string robotUUID);

  private static void PIRobot_startUpdate(string robotUUID, string lang,
    bool forceUpdateFirmware, bool forceUpdateResource){
    PIRobot_loadResourceBundle();
    PIRobot_startUpdate(robotUUID);
  }
  
#else
  [DllImport(LIBNAME_API)]
  private extern static void PIRobot_startUpdateWithPath([MarshalAs(UnmanagedType.LPStr)] string robotUUID,
    //                                                           [MarshalAs(UnmanagedType.LPStr)] string lang,
    //                                                           bool forceUpdateFirmware, bool forceUpdateResource,
    [MarshalAs(UnmanagedType.LPStr)] string path);

  private static void PIRobot_startUpdate(string robotUUID, string lang,
    bool forceUpdateFirmware, bool forceUpdateResource){
    PIRobot_startUpdateWithPath(robotUUID, Application.dataPath + "/StreamingAssets");
  }
  #endif
  
  [DllImport(LIBNAME_API)]
  private extern static int PIRobot_getUpdateProgressPercent([MarshalAs(UnmanagedType.LPStr)] string robotUUID);

  private static void PIRobot_catchLogs([MarshalAs(UnmanagedType.LPStr)] string path) {
    throw new System.NotImplementedException ();
  }
  private static void PIRobot_showFile([MarshalAs(UnmanagedType.LPStr)] string path) {
    throw new System.NotImplementedException ();
  }

  //  [DllImport (LIBNAME_API)]
  //  private extern static void PIRobot_catchLogs([MarshalAs(UnmanagedType.LPStr)] string path);
  //  [DllImport (LIBNAME_API)]
  //  private extern static void PIRobot_showFile([MarshalAs(UnmanagedType.LPStr)] string path);
  

#endregion

  
#endif

  #if CHROME_UNITY
  private static void PIRobot_setFTUEMode(bool isFTUE){
    ChromeController.instance.SetFTUEMode(isFTUE);
  }

  private static bool PIRobot_isAutoConnectInfoEmpty(){
    return ChromeController.instance.IsAutoConnectInfoEmpty();
  }
  #endif

  #if UNITY_IOS
  
  [DllImport (LIBNAME_API)]
  private extern static void PIRobot_startDownloadLocaFiles([MarshalAs(UnmanagedType.LPStr)] string appName, 
                                                                 [MarshalAs(UnmanagedType.LPStr)]string version, 
                                                                 [MarshalAs(UnmanagedType.Bool)]bool isZip, 
                                                                 [MarshalAs(UnmanagedType.LPStr)]string fileName, 
                                                                 [MarshalAs(UnmanagedType.LPStr)]string path);

  [DllImport (LIBNAME_API)]
  private extern static bool PIRobot_isCacheVersionMatched(string appName, string version);

  private static int PIRobot_getUpdateProgressPercent() {
    throw new System.NotImplementedException ();
  }
  
#if !CHROME_UNITY
  [DllImport (LIBNAME_API)]
  private extern static void PIRobot_setFTUEMode([MarshalAs(UnmanagedType.Bool)]bool isFTUE);

  [DllImport (LIBNAME_API)]
  private extern static bool PIRobot_isAutoConnectInfoEmpty();
  #endif
  

#elif UNITY_ANDROID

  private static void PIRobot_startDownloadLocaFiles(string appName, string version, bool isZip, string fileName, string path){
    jc.CallStatic("startDownloadLocaFiles", appName, version, isZip, fileName, path);
  }

  private static bool PIRobot_isCacheVersionMatched(string appName, string version){
    return jc.CallStatic<bool>("isCacheVersionMatched", appName, version);
  }

  #if !CHROME_UNITY
  private static void PIRobot_setFTUEMode(bool isFTUE){
    jc.CallStatic("setFTUEMode", isFTUE);
  }

  private static bool PIRobot_isAutoConnectInfoEmpty(){
    return jc.CallStatic<bool>("isAutoConnectInfoEmpty");
  }
  #endif


  #else // editor, standalone, etc
  
  private static void PIRobot_startDownloadLocaFiles(string appName, string version, bool isZip, string fileName, string path){}
  private static bool PIRobot_isCacheVersionMatched(string appName, string version){return true;}
  
#if !CHROME_UNITY
  private static void PIRobot_setFTUEMode(bool isFTUE){}
  private static bool PIRobot_isAutoConnectInfoEmpty(){return true;}
  #endif
  #endif

  public static void startUpdateRobot(string robotUUID, string lang, bool forceFwUpdate, bool forceRsUpdate){
    PIRobot_startUpdate(robotUUID, lang, forceFwUpdate, forceRsUpdate); 
  }

  public static int getUpdateProgressPercent(string robotUUID){
    return PIRobot_getUpdateProgressPercent(robotUUID);
  }

  public static bool isUpdating(string robotUUID){
    return PIRobot_isUpdating(robotUUID);
  }

  public static void startDownloadLocaFiles(string appName, string version, bool isZip, string fileName, string path){
    PIRobot_startDownloadLocaFiles(appName, version, isZip, fileName, path);
  }

  public static bool isCacheVersionMatched(string appName, string version){
    return PIRobot_isCacheVersionMatched(appName, version);
  }

  public static void shellCommand(string robotUUID, string shellCommand){
    PIRobot_shellCommand(robotUUID, shellCommand);
  }

  public static int pendingShellCommandCount(string robotUUID){
    return PIRobot_pendingShellCommandCount(robotUUID);
  }

  public static string shellCommandNormalizeDown(string cmd){
    string ret = cmd.ToUpper().Trim();
    if (ret.Substring(0, 2) == "A "){
      ret = ret.Substring(2).Trim();
    }
		
    return ret;
  }

  public static string shellCommandNormalizeUp(string cmd){
    string ret = cmd.ToUpper().Trim();
    ret = ret + "\n";
    if (ret.Substring(0, 2) != "A "){
      ret = "A " + ret;
    }
		
    return ret;
  }

  public static void onClickSoundRecordButton(){
#if UNITY_ANDROID || UNITY_IPHONE
    PIRobot_onClickSoundRecordButton();
#else
    WWLog.logWarn("not supported on this platform: sound recording");
#endif
  }

  public static void onClickPuppetRecordButton(){
#if UNITY_ANDROID || UNITY_IPHONE
    PIRobot_onClickPuppetRecordButton();
#else
    WWLog.logWarn("not supported on this platform: sound recording");
#endif
  }

  public static void setAllowAutoRotate(bool allow){
    #if UNITY_IOS && !CHROME_UNITY
    PIRobot_setAllowAutoRotate(allow);
    #else
    Screen.autorotateToLandscapeLeft = allow;
    Screen.autorotateToLandscapeRight = allow;
    Screen.autorotateToPortrait = false;
    Screen.autorotateToPortraitUpsideDown = false;
    #endif
  }

  public static void setEnableSensorPackets(bool enabled){
    PIRobot_setEnableSensorPackets(enabled);
  }

  public static void setFTUEMode(bool isFTUE){
    PIRobot_setFTUEMode(isFTUE);
  }

  public static bool isAutoConnectInfoEmpty(){
    return PIRobot_isAutoConnectInfoEmpty();
  }

  public static void catchLogs(string path){
    PIRobot_catchLogs(path);
  }

  public static void showFile(string path){
    PIRobot_showFile(path);
  }
}


