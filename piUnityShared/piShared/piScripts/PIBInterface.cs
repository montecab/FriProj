using UnityEngine;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;
using AOT;

#if CHROME_UNITY
using ChromeUnity;
#endif

namespace PIBInterface{

  // todo: rename this to something like NameSpace:WonderWorkshop, Class:WWEngine.
  public class Actions{
    
    public delegate void MessageToUnityCallbackDelegate(IntPtr a, IntPtr b, IntPtr c);
      
    #if UNITY_IPHONE
      const string LIBNAME_API = "__Internal";
      
#else // Mac Editor and Mac Standalone
    const string LIBNAME_API = "APIObjectiveC";
    #endif
      
    //------------------------------------------------------
    // platform-independent members
    private Dictionary<string, uint> _loadedAnimations = new Dictionary<string, uint> ();

      
    //------------------------------------------------------
    // platform-dependent members
    #if UNITY_IPHONE
      // no platform-dependent members for iphone yet.
      
#elif UNITY_ANDROID
    private static AndroidJavaClass jc = new AndroidJavaClass ("com.makewonder.wwUnityWrapper"); 
    #endif
      
    // see https://forum.unity3d.com/threads/making-calls-from-c-to-c-with-il2cpp-instead-of-mono_runtime_invoke.295697/
    public delegate void MessageCallbackDelegate(IntPtr a, IntPtr b, IntPtr c);

    [MonoPInvokeCallback(typeof(MessageCallbackDelegate))]
    static void MessageCallback(IntPtr a, IntPtr b, IntPtr c){
      if (!Application.isPlaying){
        return;
      }

      string objName = Marshal.PtrToStringAuto(a);
      string cmdName = Marshal.PtrToStringAuto(b);
      string cmdData = Marshal.PtrToStringAuto(c);

      GameObject found = GameObject.Find(objName);
      if (found != null){
        found.SendMessage(cmdName, cmdData);
      } else{
        Debug.LogError("could not find named object: " + objName);
      }
    }

    //------------------------------------------------------
    // constructor
    public Actions(string piReceiverName){
      // set the receipient for SendMessageToUnity
      PIInterface_setPIMessageReceiverName(piReceiverName);
      #if CHROME_UNITY || (!UNITY_IPHONE && !UNITY_ANDROID && !UNITY_WEBGL)
      PIInterface_setMessageCallback(MessageCallback);
      #endif
    }


    #region logging

    #if UNITY_IPHONE
      [DllImport (LIBNAME_API, CallingConvention = CallingConvention.Cdecl)]
      private extern static void WWUtil_log(uint level, [MarshalAs(UnmanagedType.LPStr)] string line);
      
#elif UNITY_ANDROID
    private static void WWUtil_log(uint level, string line){
      jc.CallStatic("wwLog", (int)level, line);
    }
    #else // UNITY_EDITOR or WEBGL
    private static void WWUtil_log (uint level, string line)
    {
      switch ((WWLog.logLevel)level) {
      default:
        Debug.LogError ("unhandled log level: " + ((WWLog.logLevel)level).ToString () + " - " + line);
        break;
      case WWLog.logLevel.EMERGENCY:
      case WWLog.logLevel.CRITICAL:
      case WWLog.logLevel.ALERT:
      case WWLog.logLevel.ERROR:
        Debug.LogError (line);
        break;
      case WWLog.logLevel.WARNING:
        Debug.LogWarning (line);
        break;
      case WWLog.logLevel.NOTICE:
      case WWLog.logLevel.INFO:
      case WWLog.logLevel.DEBUG:
        Debug.Log (line);
        break;
      }
    }
      #endif
    #endregion logging

    #region local_notification

    #if UNITY_IPHONE && UNITY_5
      private void WW_scheduleLocalNotification(string title, string body, string jsonAction, System.DateTime utcTime){
        UnityEngine.iOS.LocalNotification notif = new UnityEngine.iOS.LocalNotification();
        notif.fireDate = utcTime.ToLocalTime();
        notif.alertAction = title;
        notif.alertBody = body;
        UnityEngine.iOS.NotificationServices.ScheduleLocalNotification(notif);
        if (!string.IsNullOrEmpty(jsonAction)) {
          Debug.LogError("non-empty jsonAction provided: unused. " + jsonAction);
        }
      }

      private void WW_cancelAllLocalNotifications(){
        UnityEngine.iOS.NotificationServices.CancelAllLocalNotifications();
      }
      
#elif UNITY_ANDROID
    private void WW_scheduleLocalNotification(string title, string body, string jsonAction, System.DateTime utcTime){
      long millsec = (long)((utcTime - new System.DateTime (1970, 1, 1)).TotalMilliseconds);
      jc.CallStatic("scheduleNotification", title, body, jsonAction, millsec.ToString());
    }

    private void WW_cancelAllLocalNotifications(){
      Debug.LogError("Not implemented yet in android: WW_CancelAllLocalNotifications()");
    }
    #else
      private void WW_scheduleLocalNotification(string title, string body, string jsonAction, System.DateTime millsec){}
      private void WW_cancelAllLocalNotifications(){}
      #endif
    #endregion local_notification


    #region chrome

    #if CHROME_UNITY
    private void PIRobot_openChrome(){
      ChromeController.instance.ShowChromeButton();
    }

    private void PIRobot_showConnectToRobotDialog(int robotType){
      ChromeController.instance.ShowConnectToRobotDialog(robotType);
    }

    private void PIRobot_hideChromeButton(){
      ChromeController.instance.HideChromeButton();
    }

    private void PIRobot_showChromeButton(){
      ChromeController.instance.ShowChromeButton();
    }

    private void PIRobot_showSystemDialog(string title, string body, string buttonText){
      ChromeController.instance.ShowSystemDialog(title, body, buttonText);
    }

    private void PIRobot_openPrivacyWindow(){
      ChromeController.instance.OpenPrivacyWindow();
    }

    private void PIRobot_switchRobot(string UUID){
      ChromeController.instance.SwitchRobot(UUID);
    }

    #elif UNITY_IPHONE
      [DllImport (LIBNAME_API)]
      private extern static void PIRobot_openChrome();
      
      [DllImport (LIBNAME_API)]
      private extern static void PIRobot_showConnectToRobotDialog(int robotType);

      [DllImport (LIBNAME_API)]
      private extern static void PIRobot_hideChromeButton();
      
      [DllImport (LIBNAME_API)]
      private extern static void PIRobot_showChromeButton();

      [DllImport (LIBNAME_API)]
      private extern static void PIRobot_showSystemDialog([MarshalAs(UnmanagedType.LPStr)] string title, [MarshalAs(UnmanagedType.LPStr)] string body, [MarshalAs(UnmanagedType.LPStr)] string buttonText);

      [DllImport (LIBNAME_API)]
      private extern static void PIRobot_openPrivacyWindow();
    
      [DllImport (LIBNAME_API)]
      private extern static void PIRobot_switchRobot([MarshalAs(UnmanagedType.LPStr)] string UUID);


      
#elif UNITY_ANDROID
      private void PIRobot_openChrome() {
        jc.CallStatic("openChrome");
      }

      private void PIRobot_showConnectToRobotDialog(int robotType) {
        jc.CallStatic("showConnectToRobotDialog", new object[] { robotType });
      }
      
      private void PIRobot_hideChromeButton() {
        jc.CallStatic("hideChromeButton");
      }
      
      private void PIRobot_showChromeButton() {
        jc.CallStatic("showChromeButton");
      }

      private void PIRobot_showSystemDialog(string title, string body, string buttonText) {
        jc.CallStatic("showSystemDialog", title, body, buttonText);
      }

      private void PIRobot_openPrivacyWindow(){
        jc.CallStatic("openPrivacyWindow");
      }

      private void PIRobot_switchRobot(string uuid) {
      
#warning not implemented yet in Java.
        //jc.CallStatic("switchRobot", uuid);
      }

      
#else // UNITY_EDITOR or UNITY_WEBGL
      private void PIRobot_openChrome() {}

      private void PIRobot_showConnectToRobotDialog(int robotType) {
        WWLog.logInfo("showConnectToRobotDialog for: " + robotType);
      }

      private void PIRobot_openPrivacyWindow() {}
    
      private void PIRobot_hideChromeButton() {}
    
      private void PIRobot_showChromeButton() {}

      private void PIRobot_showSystemDialog(string title, string body, string buttonText) {
        string s = "show system dialog: title - (" + title + "), message - (" + body + "), buttonText - (" + buttonText + ")";
        WWLog.logInfo(s);
        wwCrudeModal.showModal(s);
      }
    
      private void PIRobot_switchRobot (string robotUUID) {}
      #endif
    #endregion chrome

    #if UNITY_ANDROID
    #region android

    #region plugin_configuration

    public void PIInterface_setMessageCallback(MessageToUnityCallbackDelegate cb){
      #warning not implemented in java.
      //jc.CallStatic("setMessageCallback", cb);
    }

    private void PIInterface_setPIMessageReceiverName(string s){
      jc.CallStatic("setPIMessageReceiverName", s);
    }

    #endregion plugin_configuration

    #region robot_connection

    private void PIRobot_startScan(){
      jc.CallStatic("startScan");
    }

    private void PIRobot_stopScan(){
      jc.CallStatic("stopScan");
    }

    private void PIRobot_connect(string robotUUID){
      jc.CallStatic("connect", robotUUID);
    }

    private void PIRobot_disconnect(string robotUUID){    
      jc.CallStatic("disconnect", robotUUID);
    }

    #endregion robot_connection

    #region misc

    private void PIRobot_reboot(string robotUUID){
      #warning not implemented in java.     
      //jc.CallStatic("reboot", robotUUID);
    }

    private void PIRobot_reset(string robotUUID){
      jc.CallStatic("resetRobot", robotUUID);
    }


    private void PIRobot_sendCommandSetJson(string robotUUID, string jsonString){
      #warning not implemented in java.     
    }

    #endregion misc

    #region lights

    private void PIRobot_rgb(string robotUUID, double red, double green, double blue, uint[] components, int unused){
      // IMPORTANT: before passing to java, cast the uint[] array to int[].  eg WWUtil_log.
      int[] comps = new int[components.Length];
      for (int i = 0; i < components.Length; i++){
        comps [i] = (int)components [i];
      }
      //Debug.Log("unity set color");
      jc.CallStatic("setRGBLight", robotUUID, red, green, blue, comps);
    }

    private void PIRobot_led(string robotUUID, uint component, double brightness){
      // IMPORTANT: before passing to java, cast the uint to int.  eg WWUtil_log.
      jc.CallStatic("setLED", robotUUID, (int)component, brightness);
        
    }

    private void PIRobot_eyeRing(string robotUUID, double brightness, string animationFile, short[] bitmapLEDs, int bitmapLEDsCount){
      jc.CallStatic("eyeRing", robotUUID, brightness, animationFile, bitmapLEDs);
    }

    #endregion lights

    #region head

    private void PIRobot_headTilt(string robotUUID, double angle){
      jc.CallStatic("headTilt", robotUUID, angle);
    }

    private void PIRobot_headPan(string robotUUID, double angle){
      jc.CallStatic("headPan", robotUUID, angle);
    }

    private void PIRobot_headPanWithTime(string robotUUID, double angle, double time){
      jc.CallStatic("headPanWithDuration", robotUUID, angle, time);
    }

    private void PIRobot_headTiltWithTime(string robotUUID, double angle, double time){
      jc.CallStatic("headTiltWithDuration", robotUUID, angle, time);
    }

    private void PIRobot_headBang(string robotUUID){
      jc.CallStatic("headBang", robotUUID);
    }

    private void PIRobot_launcherReloadLeft(string robotUUID){
      jc.CallStatic("launcherReloadLeft", robotUUID);
    }

    private void PIRobot_launcherReloadRight(string robotUUID){
      jc.CallStatic("launcherReloadRight", robotUUID);
    }

    private void PIRobot_launcherFling(string robotUUID, double power){
      jc.CallStatic("launcherFling", robotUUID, power);
    }

    private void PIRobot_headMove(string robotUUID, double panAngle, double tiltAngle){
      jc.CallStatic("headMove", robotUUID, panAngle, tiltAngle);
    }

    #endregion head

    #region body

    private void PIRobot_move(string robotUUID, double leftWheelVelocity, double rightWheelVelocity){
      jc.CallStatic("moveWheels", robotUUID, leftWheelVelocity, rightWheelVelocity);
    }

    private void PIRobot_bodyMotion(string robotUUID, double linearVelocity, double angularVelocity, bool usePose){
      jc.CallStatic("bodyLinearAngularVelMove", robotUUID, linearVelocity, angularVelocity, usePose);
    }

    private void PIRobot_bodyMotionWithAcceleration(string robotUUID, double linearVelocity, double angularVelocity, double linearAccMagnitude, double angularAccMagnitude){
      jc.CallStatic("bodyLinearAngularVelWithAccelerationMove", robotUUID, linearVelocity, angularVelocity, linearAccMagnitude, angularAccMagnitude);
    }

    private void PIRobot_poseParam(string robotUUID,
                                     double x, double y, double theta, double time, uint mode, uint direction, uint wrapTheta){
      // IMPORTANT: cast the uint's to int's before calling java. se WWLog.
        
      bool isWrap = wrapTheta == 1 ? true : false;
      jc.CallStatic("poseParam", robotUUID, x, y, theta, time, (int)mode, (int)direction, isWrap);
    }

    #endregion body

    #region sound

    private void PIRobot_playSound(string robotUUID, string soundFileName, string soundDirectory, double volume){
      jc.CallStatic("playSound", robotUUID, soundFileName, soundDirectory, volume);
    }

    private void PIRobot_playSystemSound(string robotUUID, string soundFileName){
      jc.CallStatic("playSystemSound", robotUUID, soundFileName);
    }

    private void PIRobot_soundTransfer(string robotUUID, short[] data, int unused, string name){
      jc.CallStatic("transferSound", robotUUID, data, name);
    }

    #endregion sound

    #region fileTransfer

    private void PIRobot_fileTransfer(string robotUUID, byte[] data, int unused, string name, uint fileType){
      jc.CallStatic("transferFileToBot", robotUUID, data, unused, name, (int)fileType);
    }

    private float PIRobot_fileTransferProgress(string robotUUID){
      float ret = jc.CallStatic<float >("fileTransferProgress", robotUUID);
      return ret;
    }

    #endregion fileTransfer

    #region animation

    // note: no robot ID
    private uint PIRobot_loadJsonAnimation(string jsonAnimation){
      int animID = jc.CallStatic<int>("loadJsonAnimation", jsonAnimation);
      return (uint)animID;
    }
    // note: no robot ID
    private void PIRobot_unloadAnimation(uint animId){
      jc.CallStatic("unloadAnimation", animId);
    }

    private void PIRobot_performAnimation(string robotUUID, uint animId){
      jc.CallStatic("performAnimation", robotUUID, (int)animId);
    }

    private void PIRobot_stopAnimation(string robotUUID, uint animId){
      jc.CallStatic("stopAnimation", robotUUID, (int)animId);
    }

    private int PIRobot_numberOfExecutingCommandSequences(string robotUUID){
      int ret = jc.CallStatic<int>("robotNumberOfExecutingCommandSequences", robotUUID);
      return ret;
    }

    private void PIRobot_startOnRobotAnimation(string robotUUID, string dir, string name){
      WWLog.logError("not implemented: PIRobot_startOnRobotAnimation");
      // jc.CallStatic("startOnRobotAnimation", robotUUID, dir, name);
    }

    #endregion animation

    #region internal

    private void PIRobot_sendRawPacket(string robotUUID, byte[] bytes, uint count){
      jc.CallStatic("sendRawPacket", robotUUID, bytes);
    }

    #endregion

    #region localization

    private static void WWGetCanonicalTextLanguage(System.Text.StringBuilder str, int strlen){
      string lang = jc.CallStatic<string>("getCanonicalTextLanguage");
      str.Append(lang);
    }

    #endregion

    #region personalization

    private void PIRobot_setRobotName(string robotUUID, string name){
      WWLog.logError("not implemented: PIRobot_setRobotName");
//          jc.CallStatic("setRobotName", robotUUID, name);
    }

    #endregion

    #endregion android

    #elif UNITY_WEBGL
      private void PIInterface_setPIMessageReceiverName(string s){}
      private void PIRobot_startScan(){}
      private void PIRobot_stopScan(){}
      private void PIRobot_connect(string robotUUID){}
      private void PIRobot_disconnect(string robotUUID){    }
      private void PIRobot_reboot(string robotUUID){}
      private void PIRobot_reset(string robotUUID){}
      private void PIRobot_rgb(string robotUUID, double red, double green, double blue, uint[] components, int unused){}
      private void PIRobot_led(string robotUUID, uint component, double brightness){}
      private void PIRobot_eyeRing(string robotUUID, double brightness, string animationFile, short[] bitmapLEDs, int bitmapLEDsCount){}
      private void PIRobot_headTilt(string robotUUID, double angle){}
      private void PIRobot_headPan(string robotUUID, double angle){}
      private void PIRobot_headPanWithTime(string robotUUID, double angle, double time){}
      private void PIRobot_headTiltWithTime(string robotUUID, double angle, double time){}
      private void PIRobot_headBang(string robotUUID){}
      private void PIRobot_launcherReloadLeft(string robotUUID){}
      private void PIRobot_launcherReloadRight(string robotUUID){}
      private void PIRobot_launcherFling(string robotUUID, double power){}
      private void PIRobot_headMove(string robotUUID, double panAngle, double tiltAngle){}
      private void PIRobot_move(string robotUUID, double leftWheelVelocity, double rightWheelVelocity){}
      private void PIRobot_bodyMotion(string robotUUID, double linearVelocity, double angularVelocity, bool usePose){}
      private void PIRobot_bodyMotionWithAcceleration(string robotUUID, double linearVelocity, double angularVelocity, double linearAccMagnitude, double angularAccMagnitude){}
      private void PIRobot_poseParam(string robotUUID, double x, double y, double theta, double time, uint mode, uint direction, uint wrapTheta) {}
      private void PIRobot_playSound(string robotUUID, string soundFileName, string soundDirectory, double volume){}
      private void PIRobot_playSystemSound(string robotUUID, string soundFileName){}
      private void PIRobot_soundTransfer(string robotUUID, short[] data, int unused, string name){}
      private void PIRobot_fileTransfer(string robotUUID, byte[] data, int unused, string name, uint fileType) {}
      private float PIRobot_fileTransferProgress(string robotUUID){return 0;}
      private uint PIRobot_loadJsonAnimation(string jsonAnimation){return 0;}
      private void PIRobot_unloadAnimation(uint animId){}
      private void PIRobot_performAnimation(string robotUUID, uint animId){}
      private void PIRobot_stopAnimation(string robotUUID, uint animId){}
      private void PIRobot_startOnRobotAnimation(string robotUUID, string dir, string name){}
      private void PIRobot_setRobotName(string robotUUID, string name){}
      private int PIRobot_numberOfExecutingCommandSequences(string robotUUID){return 0;}
      private void PIRobot_sendRawPacket(string robotUUID, byte[] bytes, uint count){}
      private static void WWGetCanonicalTextLanguage(System.Text.StringBuilder str, int strlen){}

      
#else
      

#region ios_editor_desktop_etc

      

#region plugin_configuration

    [DllImport (LIBNAME_API)]
    private extern static void PIInterface_setMessageCallback ([MarshalAs(UnmanagedType.FunctionPtr)]MessageToUnityCallbackDelegate cb);
      
    [DllImport (LIBNAME_API)]
    private extern static void PIInterface_setPIMessageReceiverName ([MarshalAs(UnmanagedType.LPStr)] string s);
      

#endregion plugin_configuration

      

#region robot_connection

    [DllImport (LIBNAME_API)]
    private extern static void PIRobot_startScan ();
      
    [DllImport (LIBNAME_API)]
    private extern static void PIRobot_stopScan ();
      
    [DllImport (LIBNAME_API)]
    private extern static void PIRobot_connect ([MarshalAs(UnmanagedType.LPStr)] string robotUUID);
      
    [DllImport (LIBNAME_API)]
    private extern static void PIRobot_disconnect ([MarshalAs(UnmanagedType.LPStr)] string robotUUID);
      

#endregion robot_connection

      

#region misc

    [DllImport (LIBNAME_API)]
    private extern static void PIRobot_reboot ([MarshalAs(UnmanagedType.LPStr)] string robotUUID);
      
    [DllImport (LIBNAME_API)]
    private extern static void PIRobot_reset ([MarshalAs(UnmanagedType.LPStr)] string robotUUID);


    [DllImport (LIBNAME_API)]
    private extern static void PIRobot_sendCommandSetJson ([MarshalAs(UnmanagedType.LPStr)] string robotUUID, [MarshalAs(UnmanagedType.LPStr)] string jsonString);

      

#endregion misc

      

#region lights

    [DllImport (LIBNAME_API)]
    private extern static void PIRobot_rgb ([MarshalAs(UnmanagedType.LPStr)] string robotUUID,
                                             double red, double green, double blue, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=1)] uint[] components, int componentCount);

    [DllImport (LIBNAME_API)]
    private extern static void PIRobot_led ([MarshalAs(UnmanagedType.LPStr)] string robotUUID,
                                             uint component, double brightness);
      
    [DllImport (LIBNAME_API)]
    private extern static void PIRobot_eyeRing ([MarshalAs(UnmanagedType.LPStr)] string robotUUID,
                                                 double brightness, [MarshalAs(UnmanagedType.LPStr)] string animationFile, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=1)] short[] bitmapLEDs, int bitmapLEDsCount);
      

#endregion lights

      

#region head

    [DllImport (LIBNAME_API)]
    private extern static void PIRobot_headTilt ([MarshalAs(UnmanagedType.LPStr)] string robotUUID,
                                                  double angle);
                                                  
    [DllImport (LIBNAME_API)]
    private extern static void PIRobot_headPan ([MarshalAs(UnmanagedType.LPStr)] string robotUUID,
                                                 double angle);
                                                 
    [DllImport (LIBNAME_API)]
    private extern static void PIRobot_headPanWithTime ([MarshalAs(UnmanagedType.LPStr)] string robotUUID,
                                                         double angle, double time);
                                                 
    [DllImport (LIBNAME_API)]
    private extern static void PIRobot_headTiltWithTime ([MarshalAs(UnmanagedType.LPStr)] string robotUUID,
                                                          double angle, double time);
                                                          
    [DllImport (LIBNAME_API)]
    private extern static void PIRobot_headBang ([MarshalAs(UnmanagedType.LPStr)] string robotUUID);

    [DllImport (LIBNAME_API)]
    private extern static void PIRobot_launcherReloadLeft ([MarshalAs(UnmanagedType.LPStr)] string robotUUID);

    [DllImport (LIBNAME_API)]
    private extern static void PIRobot_launcherReloadRight ([MarshalAs(UnmanagedType.LPStr)] string robotUUID);

    [DllImport (LIBNAME_API)]
    private extern static void PIRobot_launcherFling ([MarshalAs(UnmanagedType.LPStr)] string robotUUID, double power);
      
    [DllImport (LIBNAME_API)]
    private extern static void PIRobot_headMove ([MarshalAs(UnmanagedType.LPStr)] string robotUUID,
                                                  double panAngle, double tiltAngle);
      

#endregion head

      

#region body

    [DllImport (LIBNAME_API)]
    private extern static void PIRobot_move ([MarshalAs(UnmanagedType.LPStr)] string robotUUID,
                                              double leftWheelVelocity, double rightWheelVelocity);

    [DllImport (LIBNAME_API)]
    private extern static void PIRobot_bodyMotion ([MarshalAs(UnmanagedType.LPStr)] string robotUUID,
                                                    double linearVelocity, double angularVelocity, bool usePose);

    [DllImport (LIBNAME_API)]
    private extern static void PIRobot_bodyMotionWithAcceleration ([MarshalAs(UnmanagedType.LPStr)] string robotUUID,
                                                        double linearVelocity, double angularVelocity, double linearAccelerationMagnitude, double angularAccelerationMagnitude);
                                                    
    [DllImport (LIBNAME_API)]
    private extern static void PIRobot_poseParam ([MarshalAs(UnmanagedType.LPStr)] string robotUUID,
                                                   double x, double y, double theta, double time, uint mode, uint direction, uint wrapTheta);
      

#endregion body

      

#region sound

    [DllImport (LIBNAME_API)]
    private extern static void PIRobot_playSound ([MarshalAs(UnmanagedType.LPStr)] string robotUUID,
                                                   [MarshalAs(UnmanagedType.LPStr)] string soundFileName,
                                                   [MarshalAs(UnmanagedType.LPStr)] string soundDirectory, double volume);
      
    [DllImport (LIBNAME_API)]
    private extern static void PIRobot_playSystemSound ([MarshalAs(UnmanagedType.LPStr)] string robotUUID,
                                                         [MarshalAs(UnmanagedType.LPStr)] string soundFileName);
      
    [DllImport (LIBNAME_API)]
    private extern static void PIRobot_soundTransfer ([MarshalAs(UnmanagedType.LPStr)] string robotUUID,
                                                       [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=1)] short[]data,
                                                       int dataLength,
                                                       [MarshalAs(UnmanagedType.LPStr)] string name);
      
    [DllImport (LIBNAME_API)]
    private extern static void PIRobot_fileTransfer ([MarshalAs(UnmanagedType.LPStr)] string robotUUID,
                                                       [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=1)] byte[]data,
                                                       int dataLength,
                                                       [MarshalAs(UnmanagedType.LPStr)] string name,
                                                       uint fileType);
      
    [DllImport (LIBNAME_API)]
    private extern static float PIRobot_fileTransferProgress ([MarshalAs(UnmanagedType.LPStr)] string robotUUID);
      

#endregion sound

      

#region animation

    // note: no robot ID
    [DllImport (LIBNAME_API)]
    private extern static uint PIRobot_loadJsonAnimation ([MarshalAs(UnmanagedType.LPStr)] string jsonAnimation);
      
    // note: no robot ID
    [DllImport (LIBNAME_API)]
    private extern static void PIRobot_unloadAnimation (uint animId);
      
    [DllImport (LIBNAME_API)]
    private extern static void PIRobot_performAnimation ([MarshalAs(UnmanagedType.LPStr)] string robotUUID,
                                                          uint animId);
                                                          
    [DllImport (LIBNAME_API)]
    private extern static void PIRobot_stopAnimation ([MarshalAs(UnmanagedType.LPStr)] string robotUUID,
                                                       uint animId);
                                                       
    [DllImport (LIBNAME_API)]
    private extern static int PIRobot_numberOfExecutingCommandSequences ([MarshalAs(UnmanagedType.LPStr)] string robotUUID);

    [DllImport (LIBNAME_API)]
    private extern static void PIRobot_startOnRobotAnimation ([MarshalAs(UnmanagedType.LPStr)] string robotUUID,
                                                              [MarshalAs(UnmanagedType.LPStr)] string dir,
                                                              [MarshalAs(UnmanagedType.LPStr)] string name);

      

#endregion animation

      

#region internal

    [DllImport (LIBNAME_API)]
    private extern static void PIRobot_sendRawPacket ([MarshalAs(UnmanagedType.LPStr)] string robotUUID,
                                                       [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=1)] byte[] bytes, uint count);
      
      

#endregion

      

#region localization

    [DllImport (LIBNAME_API)]
    private static extern void WWGetCanonicalTextLanguage(System.Text.StringBuilder str, int strlen);
      

#endregion

    
      

#region personailization

    [DllImport (LIBNAME_API)]
    private extern static void PIRobot_setRobotName ([MarshalAs(UnmanagedType.LPStr)] string robotUUID,
      [MarshalAs(UnmanagedType.LPStr)] string name);
      

#endregion

    
      

#endregion ios_editor_desktop_etc

      #endif


      
    // end of external plugin stubs

    public void startScan(){
      PIRobot_startScan();
    }

    public void stopScan(){
      PIRobot_stopScan();
    }

    public void connectRobot(string robotUUID){
      PIRobot_connect(robotUUID);
    }

    public void disconnectRobot(string robotUUID){
      PIRobot_disconnect(robotUUID);
    }

    public void scheduleLocalNotification(string title, string body, string jsonAction, System.DateTime utcTime){
      WW_scheduleLocalNotification(title, body, jsonAction, utcTime);
    }

    public void cancelAllLocalNotificatons(){
      WW_cancelAllLocalNotifications();
    }

    public void openChrome(){
      PIRobot_openChrome();
    }

    public void showConnectToRobotDialog(int robotType){
      PIRobot_showConnectToRobotDialog(robotType);
    }

    public void hideChromeButton(){
      PIRobot_hideChromeButton();
    }

    public void showChromeButton(){
      PIRobot_showChromeButton();
    }

    public void showSystemDialog(string title, string body = "", string buttonText = "OK"){
      PIRobot_showSystemDialog(title, body, buttonText);
    }

    public void openPrivacyWindow(){
      PIRobot_openPrivacyWindow();
    }

    public void switchRobot(string robotUUID){
      PIRobot_switchRobot(robotUUID);
    }

    // todo: remove or move to _internal.
    public void reset(string robotUUID){
      PIRobot_reset(robotUUID);
    }

    // todo: remove or move to _internal.
    public void reboot(string robotUUID){
      PIRobot_reboot(robotUUID);
    }

    public void move(string robotUUID, double leftWheelVelocity, double rightWheelVelocity){
      PIRobot_move(robotUUID, leftWheelVelocity, rightWheelVelocity);
    }

    public void bodyMotion(string robotUUID, double linearVelocity, double angularVelocity){
      PIRobot_bodyMotion(robotUUID, linearVelocity, angularVelocity, false);
    }

    public void bodyMotion(string robotUUID, double linearVelocity, double angularVelocity, bool usePose){
      PIRobot_bodyMotion(robotUUID, linearVelocity, angularVelocity, usePose);
    }

    public void bodyMotionWithAcceleration(string robotUUID, double linearVelocity, double angularVelocity, double linearAccelerationMagnitude, double angularAccelerationMagnitude){
      PIRobot_bodyMotionWithAcceleration(robotUUID, linearVelocity, angularVelocity, linearAccelerationMagnitude, angularAccelerationMagnitude);
    }

    public void headTilt(string robotUUID, double angle){
      PIRobot_headTilt(robotUUID, angle);
    }

    public void headPan(string robotUUID, double angle){
      PIRobot_headPan(robotUUID, angle);
    }

    public void headBang(string robotUUID){
      PIRobot_headBang(robotUUID);
    }

    public void launcherReloadLeft(string robotUUID){
      PIRobot_launcherReloadLeft(robotUUID);
    }

    public void launcherReloadRight(string robotUUID){
      PIRobot_launcherReloadRight(robotUUID);
    }

    public void launcherFling(string robotUUID, double power){
      PIRobot_launcherFling(robotUUID, power);
    }

    public void headPanWithTime(string robotUUID, double angle, double time){
      PIRobot_headPanWithTime(robotUUID, angle, time);
    }

    public void headTiltWithTime(string robotUUID, double angle, double time){
      PIRobot_headTiltWithTime(robotUUID, angle, time);
    }

    public void headMove(string robotUUID, double panAngle, double tiltAngle){
      PIRobot_headMove(robotUUID, panAngle, tiltAngle);
    }

    public void rgbLights(string robotUUID, double red, double green, double blue, uint[] components){
      PIRobot_rgb(robotUUID, red, green, blue, components, components.Length);
    }

    public void eyeRing(string robotUUID, double brightness, string animationFile, bool[] bitmap){
      short[] shortBitmapLEDs = null;
      int length = 0;
      if ((bitmap != null) && (bitmap.Length > 0)){
        // IMPORTANT: before passing to java, cast the ushort[] array to short[].  eg WWUtil_log.
        length = bitmap.Length;
        shortBitmapLEDs = new short[length];
        for (int i = 0; i < length; i++){
          shortBitmapLEDs [i] = (bitmap [i] ? (short)1 : (short)0);
        }
      }
      PIRobot_eyeRing(robotUUID, brightness, animationFile, shortBitmapLEDs, length);
    }

    public void ledTail(string robotUUID, double brightness){
      PIRobot_led(robotUUID, (uint)PI.ComponentID.WW_COMMAND_LED_TAIL, brightness);
    }

    public void ledButtonMain(string robotUUID, double brightness){
      PIRobot_led(robotUUID, (uint)PI.ComponentID.WW_COMMAND_LED_BUTTON_MAIN, brightness);
    }

    public void playSound(string robotUUID, string soundFileName){
      PIRobot_playSystemSound(robotUUID, soundFileName);
    }

    public void playSound(string robotUUID, string soundFileName, string soundDirectory, double volume){
      PIRobot_playSound(robotUUID, soundFileName, soundDirectory, volume);
    }

    public void poseGlobal(string robotUUID, double x, double y, double theta, double time){
      PIRobot_poseParam(robotUUID, x, y, theta, time, (uint)PI.WWPoseMode.WW_POSE_MODE_GLOBAL, (uint)PI.WWPoseDirection.WW_POSE_DIRECTION_FORWARD, (uint)PI.WWPoseWrap.WW_POSE_WRAP_OFF);
    }

    public void poseSetGlobal(string robotUUID, double x, double y, double theta, double time){
      PIRobot_poseParam(robotUUID, x, y, theta, time, (uint)PI.WWPoseMode.WW_POSE_MODE_SET_GLOBAL, (uint)PI.WWPoseDirection.WW_POSE_DIRECTION_FORWARD, (uint)PI.WWPoseWrap.WW_POSE_WRAP_OFF);
    }

    public void poseRelative(string robotUUID, double x, double y, double theta, double time){
      PIRobot_poseParam(robotUUID, x, y, theta, time, (uint)PI.WWPoseMode.WW_POSE_MODE_RELATIVE_COMMAND, (uint)PI.WWPoseDirection.WW_POSE_DIRECTION_FORWARD, (uint)PI.WWPoseWrap.WW_POSE_WRAP_OFF);
    }

    public void poseParam(string robotUUID, double x, double y, double theta, double time, PI.WWPoseMode mode, PI.WWPoseDirection direction, PI.WWPoseWrap wrapTheta){
      PIRobot_poseParam(robotUUID, x, y, theta, time, (uint)mode, (uint)direction, (uint)wrapTheta);
    }

    public void sendCommandSetJson(string robotUUID, string jsonString){
      PIRobot_sendCommandSetJson(robotUUID, jsonString);
    }

    public void sendCommandSetJson(string robotUUID, WW.SimpleJSON.JSONClass jscCommandSet){
      sendCommandSetJson(robotUUID, jscCommandSet.ToString(" "));
    }
      
    // idempotent.
    // note, no robot ID.
    public uint preloadJsonAnimation(string jsonAnimation){
      if (!_loadedAnimations.ContainsKey(jsonAnimation)){
        uint id = PIRobot_loadJsonAnimation(jsonAnimation);
        if (id == 0){
          Debug.LogError("could not load animation. length = " + jsonAnimation.Length);
          return 0;
        }
          
        _loadedAnimations [jsonAnimation] = id;
      }
        
      return _loadedAnimations [jsonAnimation];
    }
      
    // note, no robot ID.
    public void unloadJsonAnimation(string jsonAnimation){
      if (_loadedAnimations.ContainsKey(jsonAnimation)){
        uint id = PIRobot_loadJsonAnimation(jsonAnimation);
        PIRobot_unloadAnimation(id);
        _loadedAnimations.Remove(jsonAnimation);
      }
    }

    public int numberOfExecutingCommandSequences(string robotUUID){     
      return PIRobot_numberOfExecutingCommandSequences(robotUUID);
    }

    public string findAnimForAnimID(uint animID){
      foreach (string k in _loadedAnimations.Keys){
        if (_loadedAnimations [k] == animID){
          return k;
        }
      }
      return "";
    }
      
    // make the assumption that a single json animation is playing at a time.
    // this enables us to start/stop much more simply.
    private Dictionary<string, uint> playingAnimIDs = new Dictionary<string, uint> ();

    public uint getPlayingAnimID(string robotUUID){
      if (!playingAnimIDs.ContainsKey(robotUUID)){
        return 0;
      } else{
        return playingAnimIDs [robotUUID];
      }
    }

    public void startSingleAnim(string robotUUID, string jsonAnimation){
      stopSingleAnim(robotUUID);
      uint animId = preloadJsonAnimation(jsonAnimation);
      if (animId > 0){
        PIRobot_performAnimation(robotUUID, animId);
        playingAnimIDs [robotUUID] = animId;
      }
    }

    public void stopSingleAnim(string robotUUID){
      uint animID = getPlayingAnimID(robotUUID);
      if (animID > 0){
        PIRobot_stopAnimation(robotUUID, animID);
        playingAnimIDs [robotUUID] = 0;
      }
    }

    public void performJsonAnimation(string robotUUID, string jsonAnimation){
      uint animId = preloadJsonAnimation(jsonAnimation);
      if (animId > 0){
        PIRobot_performAnimation(robotUUID, animId);
      }
    }

    public void stopJsonAnimation(string robotUUID, string jsonAnimation){
      uint animId = preloadJsonAnimation(jsonAnimation);
      if (animId > 0){
        PIRobot_stopAnimation(robotUUID, animId);
      }
    }

    public void startOnRobotAnimation(string robotUUID, string dir, string name){
      PIRobot_startOnRobotAnimation(robotUUID, dir, name);
    }

    public void setRobotName(string robotUUID, string name){
      PIRobot_setRobotName(robotUUID, name);
    }

    public void soundTransfer(string robotUUID, short[] data, string name){
      PIRobot_soundTransfer(robotUUID, data, data.Length, name);
    }

    public void fileTransfer(string robotUUID, byte[] data, string fileName, WWRobotFileTransferFileType fileType){
      PIRobot_fileTransfer(robotUUID, data, data.Length, fileName, (uint)fileType);
    }

    public float fileTransferProgress(string robotUUID){
      return PIRobot_fileTransferProgress(robotUUID);
    }

    public static void log(WWLog.logLevel level, string line){
      WWUtil_log((uint)level, line);
    }

    public void sendRawPacket(string robotUUID, byte[] packet){
      bool printIt = false;
      if (printIt){
        string d = "";
        for (int n = 0; n < packet.Length; ++n){
          if (n > 0){
            d += " ";
          }
          d += packet [n].ToString("X2");
        }
        WWLog.logInfo("raw bytes: " + d);
      }

      PIRobot_sendRawPacket(robotUUID, packet, (uint)packet.Length);
    }

    public static string getCanonicalTextLanguage(){
      System.Text.StringBuilder sb = new System.Text.StringBuilder ();
      sb.EnsureCapacity(100);
      WWGetCanonicalTextLanguage(sb, sb.Capacity);
      return sb.ToString();
    }
      
  }
}

