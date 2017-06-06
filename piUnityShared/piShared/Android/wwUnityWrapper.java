package com.makewonder;


import android.content.Intent;
import android.net.Uri;
import android.os.Environment;
import android.util.Log;
import android.util.SparseArray;

import com.unity3d.player.UnityPlayer;
import com.w2.api.engine.components.commands.EyeRing;
import com.w2.api.engine.components.commands.LauncherFling;
import com.w2.api.engine.components.commands.LauncherReload;
import com.w2.api.engine.constants.RobotCommandId;
import com.w2.api.engine.events.RobotSensorSetUpdated;
import com.w2.api.engine.events.gesture.GestureEvent;
import com.w2.api.engine.events.gesture.GestureEventFactory;
import com.w2.api.engine.operators.RobotCommandSet;
import com.w2.api.engine.operators.RobotSensorSet;
import com.w2.api.engine.constants.RobotType;
import com.w2.api.engine.components.commands.BodyLinearAngular;
import com.w2.api.engine.components.commands.Speaker;
import com.w2.api.engine.components.commands.BodyPose.PoseDirection;
import com.w2.api.engine.components.commands.BodyPose.PoseMode;
import com.w2.api.engine.components.sensors.BodyPose;
import com.w2.impl.engine.component.commands.HeadPositionTime;
import com.w2.api.engine.components.commands.HeadBang;
import com.w2.api.engine.components.commands.HeadPosition;
import com.w2.api.engine.components.commands.LightMono;
import com.w2.api.engine.events.ShellCommandExecuted;
import com.w2.impl.engine.component.commands.SetPower;
import com.w2.api.engine.components.commands.LightRGB;
import com.w2.api.engine.components.commands.BodyWheels;
import com.w2.api.engine.components.sensors.Accelerometer;
import com.w2.api.engine.components.sensors.Button;
import com.w2.api.engine.components.sensors.Encoder;
import com.w2.api.engine.components.sensors.Gyroscope;
import com.w2.impl.engine.component.sensors.Battery;
import com.w2.impl.engine.component.sensors.MicrophoneInternal;
import com.w2.impl.engine.component.sensors.SoundPlaying;
import com.w2.api.engine.errorhandling.APIException;
import com.w2.api.engine.events.EventBusFactory;
import com.w2.api.engine.events.RobotCommandSetSequenceActivity;
import com.w2.api.engine.events.RobotCommandSetSequenceFinished;
import com.w2.api.engine.events.RobotCommandSetSequenceStopped;
import com.w2.api.engine.operators.CommandSetSequence;
import com.w2.api.engine.robots.Robot;
import com.w2.impl.engine.component.sensors.DistanceInternal;
import com.w2.impl.engine.component.sensors.Kidnap;
import com.w2.impl.engine.component.sensors.StallBump;
import com.w2.impl.engine.constants.RobotCommandIdInternal;
import com.w2.impl.engine.constants.RobotSensorIdInternal;
import com.w2.impl.engine.events.EventBusFactoryInternal;
import com.w2.impl.engine.events.filetransfer.FileTransferComplete;
import com.w2.impl.engine.events.filetransfer.FileTransferFailed;
import com.w2.impl.engine.events.filetransfer.FileTransferProgress;
import com.w2.impl.engine.operators.RobotSensorSetImpl;
import com.w2.impl.engine.robots.RobotControl;
import com.w2.impl.engine.robots.RobotImpl;
import com.w2.impl.engine.robots.filetransfer.TransferDestination;
import com.w2.impl.engine.robots.filetransfer.TransferFileType;
import com.w2.libraries.chrome.events.EventDialogClosed;
import com.w2.libraries.chrome.events.PrivacyDialogShow;
import com.w2.libraries.chrome.localization.LocaDownloader;
import com.w2.libraries.chrome.localization.LocaManager;
import com.w2.libraries.chrome.update.UpdateManager;
import com.w2.libraries.chrome.update.UpdateManagerCallback;
import com.w2.libraries.chrome.utils.WWConfiguration;
import com.w2.libraries.chrome.utils.sharedRobots.SharedRobotsHelper;
import com.w2.logging.LoggingHelper;
import com.w2.utils.LocaleUtility;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;


import java.io.File;
import java.io.IOException;
import java.util.HashMap;
import java.util.List;
import java.util.Set;

import de.greenrobot.event.EventBus;

/**
 * Created by Leisen Huang on 1/8/15.
 *
 * A wrapper used for communication between Unity and Android API/ Android Chrome
 * Wonder Workshop
 */
public class wwUnityWrapper {

    private static final String TAG = wwUnityWrapper.class.getSimpleName();
    public static String wwMessageReceiverName;
    public static HashMap<String, Robot> connectedRobots= new HashMap<String, Robot>();
    public static HashMap<String, Robot> discoveredRobots = new HashMap<String, Robot>();

    // This is the hash map of a robot to the status of the file transfer that is currently being run on the robot
    private HashMap<Robot, Float> mFileTransferProgressDictionary = new HashMap<Robot, Float>();
    private static final float kFileTransferComplete = 1f;
    private static final float kFileTransferNotStarted = 0f;

    private static int nextAnimationLoadId = 1;
    private static SparseArray<CommandSetSequence> loadedAnimations = new SparseArray<CommandSetSequence>();

    //Singleton
    private static wwUnityWrapper mInstance;
    public static wwUnityWrapper getInstance(){
        if(mInstance == null)
            mInstance = new wwUnityWrapper();
        return mInstance;
    }


    public void sendToUnity(String jsonMessage){
        if(wwMessageReceiverName == null){
            Log.w(TAG, "error: no message receiver name is set. you must call PIInterface_setPIReceiverName(). message dropped: " + jsonMessage);
            return;
        }
        UnityPlayer.UnitySendMessage(wwMessageReceiverName, "onPIRobotManagerDelegate", jsonMessage);
    }

    public void sendUnityMessageWithMethodName(String methodName, Robot robot) throws JSONException
    {
        JSONObject json = new JSONObject();
        json.put("method", methodName);
        json.putOpt("robot", robotToJson(robot));
        String jsonMessage = json.toString();
        sendToUnity(jsonMessage);
    }

    private  static String getAddress(Robot robot) {
        RobotImpl robotImpl = (RobotImpl)robot;
        return robotImpl.getAddress();
    }

    private JSONObject robotToJson(Robot robot) throws JSONException {
        JSONObject json = new JSONObject();
        json.put("name", robot.getName());
        json.put("uuId", getAddress(robot));
        json.put("type", robot.getRobotType().getValue());

        json.put("fw", robot.getFirmwareVersion());
        json.put("rs", robot.getRobotFirmwareResourceVersion());
        json.put("lang", robot.getRobotLocale());
        RobotImpl rb = (RobotImpl)robot;
        json.put("serial", rb.getSerialNumber());

        json.put("personalityColorIndex", rb.getPersonalityColorIndex().mColor);
        json.put("personalityAnimIndex", rb.getPersonalityAnimationIndex().mIndex);
        json.put("audioFilesVersion", rb.getAudioFilesVersion());

        // These calls require new jni calls.  Implement as needed
//        json.put("hardwareRevision", TODO);
//        json.put("signalStrength", TODO);
//        json.put("hasCrashDumps", TODO);
//        json.put("advButtonPress", TODO);

        return json;
    }

    public static void setPIMessageReceiverName(String name) throws JSONException
    {
        wwMessageReceiverName = name;
        Log.d(TAG, "set receiver name " + name);
        syncConnectedRobots();
    }

    public static void showSystemDialog(String title, String message, String buttonText)
    {
      sChromeInterface.showSystemDialog(title, message, buttonText);
    }

    public static void setConnectedRobots(HashMap<String, Robot> connectedRobots) {
        wwUnityWrapper.connectedRobots = connectedRobots;
    }

    /******************************* FW Update *******************************/
  public static void startUpdate(String robotUUID, String lang, boolean forceFwUpdate, boolean forceRsUpdate) {
    Robot robot = getRobotForUUID(robotUUID);
    if(robot == null){
      LoggingHelper.e(TAG, "Cannot find connected robot: " + robotUUID);
      return;
    }
    UpdateManagerCallback callback = new UpdateManagerCallback() {
      @Override
      public void didCompleteFirmwareUpdate(Robot robot) {
        wwUnityWrapper.getInstance().sendUpdateFinishToUnity(robot, true);
      }

      @Override
      public void didFailFirmwareUpdate(Robot robot) {
        wwUnityWrapper.getInstance().sendUpdateFinishToUnity(robot, false);
      }

      @Override
      public void percentageUpdate(Robot robot, int percent, UpdateManager.UpdateInfo info) {
        wwUnityWrapper.getInstance().sendUpdateInfoToUnity(robot, percent, info);
      }
    };
    UpdateManager.getInstance().updateRobotFirmware(robot, callback, lang, forceFwUpdate, forceRsUpdate);
  }

  public static void catchLogs(String path){
    try {
      LoggingHelper.i(TAG, path);
      String cmd = "logcat -d -f "+ path;
      Runtime.getRuntime().exec(cmd);
    } catch (IOException e) {
      e.printStackTrace();
    }
  }

  public static void showFile(String path){
    Intent browserIntent = new Intent(Intent.ACTION_VIEW, Uri.parse("file://" + path));
    UnityWrapperMainActivity.sMainActivity.startActivity(browserIntent);
  }

  public static int getUpdateProgressPercent() {
      return UpdateManager.getInstance().getUpdateProgressPercent();
  }

  public static boolean isUpdating(String uuid) {
      return UpdateManager.getInstance().isUpdating();
  }

  public void sendUpdateFinishToUnity(Robot robot, boolean isSuccess){
    try{
      JSONObject json = new JSONObject();
      json.put("method", "didFinishUpdate");
      json.putOpt("robot", robotToJson(robot));
      json.put("success", isSuccess);
      sendToUnity(json.toString());
    }
    catch (JSONException error){

    }
  }

  public void sendUpdateInfoToUnity(Robot robot, int percent, UpdateManager.UpdateInfo info){
    try{
      JSONObject json = new JSONObject();
      json.put("method", "didReceiveUpdateInfo");
      json.putOpt("robot", robotToJson(robot));
      json.put("info", updateInfoToJson(info, percent));
      sendToUnity(json.toString());
    }
    catch (JSONException error){

    }
  }

  private JSONObject updateInfoToJson(UpdateManager.UpdateInfo info, int percent) throws JSONException{
    JSONObject json = new JSONObject();
    json.put("percent", percent);
    json.put("state", info.currentState.toString());
    json.put("fileName", info.currentFileName);
    json.put("filePercent", info.currentFileProgress);
    json.put("fwRetry", info.fwRetryCount);
    json.put("rsRetry", info.rsRetryCount);
    json.put("rsOpRetry", info.rsOperationRetryCount);
    json.put("reconnectRetry", info.reconnectRetryCount);
    return json;
  }

  /******************************* FTUE *******************************/
    public static void setFTUEMode(boolean isFTUE) {
        //LoggingHelper.i(TAG, "ftue: " + isFTUE);
        WWConfiguration.getInstance().setFirstUserExperience(isFTUE, null);
    }

    public static boolean isAutoConnectInfoEmpty(){
        //LoggingHelper.i(TAG, "auto connect: " + SharedRobotsHelper.getSavedRobots().size());
        return SharedRobotsHelper.getSavedRobots().isEmpty();
    }

    /******************************* LOCALIZITION *******************************/

    public static void startDownloadLocaFiles(String appName, String version, boolean zip, String fileName, String path ){
        LoggingHelper.i(TAG, "start download loca files: " + appName + ", " + version + ", " + path);
        if(sChromeInterface != null) {
            sChromeInterface.startDownloadLocaFiles(appName, version, zip, fileName, path);
        }
    }

    public static boolean isCacheVersionMatched(String appName, String version){
        return LocaDownloader.isCachedVersionMatched(appName, version);
    }

    /******************************* LOCAL NOTIFICATION *******************************/

    public  static LocalNotificationInterface sLocalNotificationInterface;

    public static void scheduleNotification(String title, String body, String jsonParams, String millisecondsSince1970){
        if(sLocalNotificationInterface != null){
            long sec = Long.parseLong(millisecondsSince1970);
            sLocalNotificationInterface.scheduleNotification(title,body,jsonParams,sec);
        }
    }

    /******************************* CHROME *******************************/

    public static ChromeInterface sChromeInterface;

    public static void openPrivacyWindow(){
        if(sChromeInterface != null) {
            sChromeInterface.openPrivacyDialog();
        }
    }

    public static void openChrome(){
        //LoggingHelper.d(TAG, "open Chrome");
        if(sChromeInterface != null) {
            sChromeInterface.openChrome();
        }
    }

    public static void showConnectToRobotDialog(int robotType){
        //LoggingHelper.i(TAG, "showConnectToRobotDialog called with param: " + robotType);
        if (sChromeInterface != null) {
            sChromeInterface.showConnectToRobotDialog(robotType);
        }
    }

    public static void hideChromeButton(){
        //LoggingHelper.d(TAG, "hide Chrome button");
        if(sChromeInterface != null) {
            sChromeInterface.hideChromeButton();
        }
    }


    public static void showChromeButton(){
        //LoggingHelper.d(TAG, "show Chrome button");
        if(sChromeInterface != null) {
            sChromeInterface.showChromeButton();
        }
    }

    public static void syncConnectedRobots() throws  JSONException{
        for(Robot bot : connectedRobots.values()){
            wwUnityWrapper.getInstance().didConnectWithRobot(bot);
        }
    }

    public static void setEnableSensorPackets(boolean enabled){
        for(Robot bot : connectedRobots.values()){
            RobotImpl robot = (RobotImpl) bot;
            robot.setEnableSensorPackets(enabled);
        }
    }


    public void didExecuteShellCommand(ShellCommandExecuted event) throws JSONException{
        Robot robot = event.getRobot();
        String cmd = event.getCommand();
        String result = event.getResult();

        JSONObject jso = new JSONObject();
        jso.put   ("method"    , "didExecuteShellCommand");
        jso.putOpt("robot"     , robotToJson(robot));
        jso.put   ("command"   , cmd);
        jso.put   ("results"   , result);

        sendToUnity(jso.toString());
    }

    public static  void openVoiceRecording(){
        if(sChromeInterface != null) {
            sChromeInterface.openVoiceRecording();
        }
    }

    /******************************* ROBOT CONNECTION *******************************/
    public void didConnectWithRobot(Robot robot) throws JSONException
    {
        if(!connectedRobots.containsKey(getAddress(robot)))
            connectedRobots.put(getAddress(robot), robot);
        Log.d(TAG, "connected with " + robot.getName() + " type " + robot.getRobotType());  // should print "connected with KevinBot"

        sendUnityMessageWithMethodName("didConnectWithRobot", robot);

        EventBus robotEventBus = EventBusFactory.getRobotEventBus(robot.getRobotId());
        if(!robotEventBus.isRegistered(this)){
            robotEventBus.register(this);
        }

        EventBus internalEventBus = EventBusFactoryInternal.getRobotInternalEventBus(robot.getRobotId());
        if(!internalEventBus.isRegistered(this)){
            internalEventBus.register(this);
        }

        // Add gesture events
        // Drop event
        robot.subscribeEvent(GestureEventFactory.gestureDrop(robot));

        //Shake event
        robot.subscribeEvent(GestureEventFactory.orientationShake(robot));

        //Slide along x axis
        robot.subscribeEvent(GestureEventFactory.gestureSlideAlongAxis(GestureEventFactory.WW_SENSOR_VALUE_AXIS_X, true, robot));
        robot.subscribeEvent(GestureEventFactory.gestureSlideAlongAxis(GestureEventFactory.WW_SENSOR_VALUE_AXIS_X, false, robot));

        //Slide along y axis
        robot.subscribeEvent(GestureEventFactory.gestureSlideAlongAxis(GestureEventFactory.WW_SENSOR_VALUE_AXIS_Y, true, robot));
        robot.subscribeEvent(GestureEventFactory.gestureSlideAlongAxis(GestureEventFactory.WW_SENSOR_VALUE_AXIS_Y, false, robot));


        //Slide along z axis
        robot.subscribeEvent(GestureEventFactory.gestureSlideAlongAxis(GestureEventFactory.WW_SENSOR_VALUE_AXIS_Z, true, robot));
        robot.subscribeEvent(GestureEventFactory.gestureSlideAlongAxis(GestureEventFactory.WW_SENSOR_VALUE_AXIS_Z, false, robot));

    }

    public void didDiscoverWithRobot(Robot robot) throws JSONException
    {
        Log.d(TAG,"didDiscoverWithRobot " + robot.getName() + " of type " + robot.getRobotType());
        if(robot == null){
            LoggingHelper.e(TAG, "robot is null");
            return;
        }
        discoveredRobots.put(getAddress(robot), robot);
        sendUnityMessageWithMethodName("didDiscoverRobot", robot);
    }

    public void didDisconnectWithRobot(Robot robot) throws JSONException
    {
        if(connectedRobots.containsKey(getAddress(robot)))
          connectedRobots.remove(getAddress(robot));

        if (mFileTransferProgressDictionary.containsKey(robot)) {
          mFileTransferProgressDictionary.remove(robot);
        }

        Log.d(TAG, "disconnected with "+ robot.getName());
        sendUnityMessageWithMethodName("didDisconnectWithRobot", robot);

        EventBus robotEventBus = EventBusFactory.getRobotEventBus(robot.getRobotId());
        robotEventBus.unregister(this);

        EventBus internalEventBus = EventBusFactoryInternal.getRobotInternalEventBus(robot.getRobotId());
        internalEventBus.unregister(this);
    }

    /******************************* ROBOT COMMANDS *******************************/

    public static void shellCommand(String robotUUID, String commandString){
        LoggingHelper.d(TAG, "robot " + robotUUID + " send shell command " + commandString);
        RobotImpl robot =(RobotImpl) getRobotForUUID(robotUUID);
        robot.sendShellCommand(commandString);
    }

    public static void headPan(String robotUUID,double degree)
    {
        HeadPosition panComponent = new HeadPosition();
        panComponent.setAngle(degree);
        RobotCommandSet commandSet = RobotCommandSet.emptySet();
        commandSet.addCommand(RobotCommandId.HEAD_POSITION_PAN, panComponent);
        wwSendCommandSet(robotUUID, commandSet);
    }

    public static void headTilt(String robotUUID,double degree)
    {
        HeadPosition tiltComponent = new HeadPosition();
        tiltComponent.setAngle(degree);
        RobotCommandSet commandSet = RobotCommandSet.emptySet();
        commandSet.addCommand(RobotCommandId.HEAD_POSITION_TILT, tiltComponent);
        wwSendCommandSet(robotUUID, commandSet);
    }

    //todo: test
    public static void headPanWithDuration(String robotUUID, double degree, double time)
    {
        double radians = Math.toRadians(degree);
        HeadPositionTime panComponent = new HeadPositionTime(radians, time);
        RobotCommandSet commandSet = RobotCommandSet.emptySet();
        commandSet.addCommand(RobotCommandIdInternal.HEAD_POSITION_PAN_TIME, panComponent);
        wwSendCommandSet(robotUUID, commandSet);
    }

    //todo: test
    public static void headTiltWithDuration(String robotUUID, double degree, double time)
    {
        double radians = Math.toRadians(degree);
        HeadPositionTime tiltComponent = new HeadPositionTime(radians, time);
        RobotCommandSet commandSet = RobotCommandSet.emptySet();
        commandSet.addCommand(RobotCommandIdInternal.HEAD_POSITION_TILT_TIME, tiltComponent);
        wwSendCommandSet(robotUUID, commandSet);
    }

    //todo: test
    public static void headMove(String robotUUID, double panDegree, double tiltDegree)
    {
        HeadPosition panComponent = new HeadPosition();
        panComponent.setAngle(panDegree);
        HeadPosition tiltComponent = new HeadPosition();
        tiltComponent.setAngle(tiltDegree);
        RobotCommandSet commandSet = RobotCommandSet.emptySet();
        commandSet.addCommand(RobotCommandId.HEAD_POSITION_PAN, panComponent);
        commandSet.addCommand(RobotCommandId.HEAD_POSITION_TILT, tiltComponent);
        wwSendCommandSet(robotUUID, commandSet);
    }

    //todo: test
    public static void eyeRing(String robotUUID, double brightness, String animationFile, short[]bitmapLEDs)
    {

        EyeRing eyeComponent = new EyeRing();
        if ((animationFile != null) && (animationFile.length() > 0)){
            eyeComponent.setAnimationFile(animationFile);
        }
        else {
            eyeComponent.setBrightness(brightness);
            for(int i= 0; i<bitmapLEDs.length; i++){
                eyeComponent.setLedValue(i, bitmapLEDs[i]==1);
            }
        }

        RobotCommandSet commandSet = RobotCommandSet.emptySet();
        commandSet.addCommand(RobotCommandId.EYE_RING, eyeComponent);
        wwSendCommandSet(robotUUID, commandSet);
    }

    public static void headBang(String robotUUID){
        Robot robot = getRobotForUUID(robotUUID);
        RobotCommandSet commandSet = RobotCommandSet.emptySet();
        HeadBang headBangCmd = new HeadBang();
        commandSet.addCommand(RobotCommandIdInternal.MOTOR_HEAD_BANG, headBangCmd);
        wwSendCommandSet(robotUUID,commandSet);
    }

    public static void launcherReloadLeft(String robotUUID){
        Robot robot = getRobotForUUID(robotUUID);
        RobotCommandSet commandSet = RobotCommandSet.emptySet();
        LauncherReload loadLeft = new LauncherReload(LauncherReload.ReloadDirection.LEFT);
        commandSet.addCommand(RobotCommandId.LAUNCHER_RELOAD, loadLeft);
        wwSendCommandSet(robotUUID, commandSet);
    }

    public static void launcherReloadRight(String robotUUID){
        Robot robot = getRobotForUUID(robotUUID);
        RobotCommandSet commandSet = RobotCommandSet.emptySet();
        LauncherReload loadRight = new LauncherReload(LauncherReload.ReloadDirection.RIGHT);
        commandSet.addCommand(RobotCommandId.LAUNCHER_RELOAD, loadRight);
        wwSendCommandSet(robotUUID,commandSet);      
    }

    public static void launcherFling(String robotUUID, double power){
        Robot robot = getRobotForUUID(robotUUID);
        RobotCommandSet commandSet = RobotCommandSet.emptySet();
        LauncherFling fling = new LauncherFling(power);
        commandSet.addCommand(RobotCommandId.LAUNCHER_FLING, fling);
        wwSendCommandSet(robotUUID,commandSet);      
    }

    public static void setRGBLight(String robotUUID,double red, double green, double blue, int[] components)
    {
        //Log.d(TAG, "setLight");
        RobotCommandSet commandSet = RobotCommandSet.emptySet();
        LightRGB colorSetter = new LightRGB(red, green, blue);
        for (int i = 0; i < components.length; i++) {
            commandSet.addCommand(getCommandId(components[i]), colorSetter);
        }
        wwSendCommandSet(robotUUID, commandSet);
    }

    public static void setLED(String robotUUID, int component, double brightness){
        // LoggingHelper.d(TAG, "brightness: " + brightness);
        LightMono ledComponent = new LightMono(brightness);
        RobotCommandSet commandSet = RobotCommandSet.emptySet();
        commandSet.addCommand(getCommandId(component), ledComponent);
        wwSendCommandSet(robotUUID, commandSet);
    }


    public static void bodyLinearAngularVelMove(String robotUUID, double linearVelocity, double angularVelocity, boolean usePose){
        BodyLinearAngular motion = new BodyLinearAngular(linearVelocity, angularVelocity, usePose);
        RobotCommandSet commandSet = RobotCommandSet.emptySet();
        commandSet.addCommand(RobotCommandId.BODY_LINEAR_ANGULAR, motion);
        wwSendCommandSet(robotUUID, commandSet);
    }

    public static void bodyLinearAngularVelWithAccelerationMove(String robotUUID, double linearVelocity, double angularVelocity, double linearAccMagnitude, double angularAccMagnitude){
        BodyLinearAngular motion = new BodyLinearAngular(linearVelocity, angularVelocity, linearAccMagnitude, angularAccMagnitude);
        RobotCommandSet commandSet = RobotCommandSet.emptySet();
        commandSet.addCommand(RobotCommandId.BODY_LINEAR_ANGULAR, motion);
        wwSendCommandSet(robotUUID, commandSet);
    }

    public static void moveWheels(String robotUUID,double leftVelocity, double rightVelocity)
    {
        //Log.d(TAG, "moveWheels");
        BodyWheels wheels = new BodyWheels(leftVelocity, rightVelocity);
        RobotCommandSet commandSet = RobotCommandSet.emptySet();
        commandSet.addCommand(RobotCommandId.BODY_WHEELS, wheels);
        wwSendCommandSet(robotUUID, commandSet);
    }


    public static void poseParam(String robotUUID, double x, double y, double radians, double time, int mode, int direction, boolean wrapTheta)
    {
        PoseDirection dir = getPoseDirection(direction);
        PoseMode md = getPoseMode(mode);
        com.w2.api.engine.components.commands.BodyPose pose = new com.w2.api.engine.components.commands.BodyPose(x, y , radians, time, md, dir, wrapTheta);
        RobotCommandSet commandSet = RobotCommandSet.emptySet();
        commandSet.addCommand(RobotCommandIdInternal.BODY_POSE, pose);
        wwSendCommandSet(robotUUID, commandSet);
    }

    //todo: test
    public static void playSound(String robotUUID, String fileName, String directory, double volume) throws APIException
    {
        // Log.d(TAG, "play sound: " + directory+"_"+fileName);
        Speaker soundtrack = new Speaker(fileName, directory, volume);
        RobotCommandSet commandSet = RobotCommandSet.emptySet();
        commandSet.addCommand(RobotCommandId.SPEAKER, soundtrack);
        wwSendCommandSet(robotUUID, commandSet);
    }

    public static void playSystemSound(String robotUUID, String fileName) throws APIException
    {
        Speaker soundtrack = new Speaker(fileName);
        RobotCommandSet commandSet = RobotCommandSet.emptySet();
        commandSet.addCommand(RobotCommandId.SPEAKER, soundtrack);
        wwSendCommandSet(robotUUID, commandSet);
    }

    public static void transferSound(String robotUUID, short[] data, String name){
      byte[] processedSnd = RobotControl.soundNormalizeAndEncode(data);

      transferFileToBot(robotUUID, processedSnd, 0, name, 2);
    }

    public static void transferFileToBot(String robotUUID, byte[] data, int unused, String name, int fileType) {
      Robot robot = getRobotForUUID(robotUUID);
      TransferFileType transferFileType = TransferFileType.getTransferFileTypeById(fileType);
      TransferDestination destination = transferFileType.getTransFerDestination();
      if (destination == TransferDestination.NONE) {
        LoggingHelper.e(TAG, "Cannot send file " + name + " with destination not specified");
        return;
      }

      if(robot!= null)
      {
        try {
          if (wwUnityWrapper.getInstance().mFileTransferProgressDictionary.containsKey(robot)) {

            float fileTransferProgressForCurrentRobot = wwUnityWrapper.getInstance().mFileTransferProgressDictionary.get(robot);
            if ( fileTransferProgressForCurrentRobot != wwUnityWrapper.kFileTransferComplete
                && fileTransferProgressForCurrentRobot != wwUnityWrapper.kFileTransferNotStarted) {
              // There is a file that is being transferred to this robot right now.
              LoggingHelper.e(TAG, "Cannot send multiple files at the same time.");
              return;
            }
          }
          wwUnityWrapper.getInstance().mFileTransferProgressDictionary.put(robot, kFileTransferNotStarted);
          ((RobotImpl) robot).sendFile(data, name, destination);

        }
        catch (IOException e) {
          LoggingHelper.e(TAG, "Error sending file to Bot: " + name);
        }
      }

    }

    public static void startScan() throws APIException{
        discoveredRobots.clear();
        UnityWrapperMainActivity.sMainActivity.startScan();
    }

    public static void stopScan() throws APIException{
        UnityWrapperMainActivity.sMainActivity.stopScan();
    }

    public static void connect(String robotUUID){
        Robot robot = null;

        if(discoveredRobots.containsKey(robotUUID)){
            robot = discoveredRobots.get(robotUUID);
        }
        if (robot != null) {
            SharedRobotsHelper.saveConnctedRobotInfo(robot, true);
            robot.connect(UnityWrapperMainActivity.sMainActivity);
        }
        else{
            LoggingHelper.w(TAG, "Cannot connect due to robot not found");
        }
    }

    public static void disconnect(String robotUUID) {
      Robot robot = getRobotForUUID(robotUUID);
      if (robot != null) {
        robot.disconnect();
      }
    }

    public static float fileTransferProgress(String robotUUID) {
      Robot robot = getRobotForUUID(robotUUID);
      if (wwUnityWrapper.getInstance().mFileTransferProgressDictionary.containsKey(robot)) {
        return wwUnityWrapper.getInstance().mFileTransferProgressDictionary.get(robot);
      }
      return kFileTransferNotStarted;
    }

    public void onEventMainThread(FileTransferFailed fileTransferFailed) {
      // There was an error while transferring the file. Reset the status.
      mFileTransferProgressDictionary.put(fileTransferFailed.getRobot(), kFileTransferNotStarted);
    }

    public void onEventMainThread(FileTransferComplete fileTransferComplete) {
      mFileTransferProgressDictionary.put(fileTransferComplete.getRobot(), kFileTransferComplete);
    }
    public void onEventMainThread(FileTransferProgress fileTransferProgress) {
      mFileTransferProgressDictionary.put(fileTransferProgress.getRobot(), fileTransferProgress.getPercentComplete()/100f);
    }

    ////////////////////////////////////////////
    // animations

    public static int loadJsonAnimation(String jsonAnimation)
    {
        try {
            JSONObject jso = new JSONObject(jsonAnimation);
            CommandSetSequence sequence = new CommandSetSequence();
            sequence.fromJson(jso);
            if (sequence == null) {
                LoggingHelper.e(TAG, "could not load json animation. StringLength:%d", jsonAnimation.length());
                return 0;
            }

            int animId = nextAnimationLoadId;
            nextAnimationLoadId += 1;

            loadedAnimations.put(animId, sequence);

            return animId;

        }
        catch (JSONException ex) {
            LoggingHelper.logException(ex);
            return 0;
        }
    }

    public static void unloadAnimation(int animId)
    {
        loadedAnimations.remove(animId);
    }

    public static int robotNumberOfExecutingCommandSequences(String robotUUID)
    {
        Robot robot = getRobotForUUIDWithLogging(robotUUID);
        if (robot == null) {
            return 0;
        }
        CommandSetSequence currentSequence = robot.getCommandSetSequence();

        return currentSequence == null ? 0 : 1;
    }

    public static void performAnimation(String robotUUID, int animId) {
        Robot robot = getRobotForUUIDWithLogging(robotUUID);
        if (robot == null) {
            return;
        }

        CommandSetSequence sequence = loadedAnimations.get(animId);
        if (sequence == null) {
            LoggingHelper.e(TAG, "Unknown sequence: %d", animId);
            return;
        }

        robot.startCommandSetSequence(sequence);
    }

    public static void stopAnimation(String robotUUID, int animId) {
        Robot robot = getRobotForUUIDWithLogging(robotUUID);
        if (robot == null) {
            return;
        }

        CommandSetSequence sequence = loadedAnimations.get(animId);
        if (sequence == null) {
            LoggingHelper.e(TAG, "Unknown sequence: %d", animId);
            return;
        }

        if (robot.getCommandSetSequence() == sequence) {
            robot.cancelCommandSetSequence();
        }
    }

    ////////////////////////////////////////////

    public static void resetRobot(String robotUUID){
        SetPower powerState = new SetPower(SetPower.State.RESET_PERIPHERALS);
        RobotCommandSet commandSet = RobotCommandSet.emptySet();
        commandSet.addCommand(RobotCommandIdInternal.POWER, powerState);
        wwSendCommandSet(robotUUID, commandSet);
    }

    // add this because java default int to enum function is very expensive
    // will get rid of it if unity change commands to setChestLight, SetEarLight etc.
    private static RobotCommandId getCommandId(int id){
        switch (id){
            case 101:
                return RobotCommandId.LIGHT_RGB_EYE;
            case 102:
                return RobotCommandId.LIGHT_RGB_LEFT_EAR;
            case 103:
                return RobotCommandId.LIGHT_RGB_RIGHT_EAR;
            case 104:
                return RobotCommandId.LIGHT_RGB_CHEST;
            case 105:
                return RobotCommandId.LIGHT_MONO_TAIL;
            case 106:
                return RobotCommandId.LIGHT_MONO_BUTTON_MAIN;
            default:
                Log.e(TAG, "unknown light command id : " + id);
                return RobotCommandId.LIGHT_RGB_CHEST;
        }
    }

    private static PoseDirection getPoseDirection(int dir){
        switch (dir){
            case 0:
                return PoseDirection.WW_POSE_DIRECTION_FORWARD;
            case 1:
                return PoseDirection.WW_POSE_DIRECTION_BACKWARD;
            case 2:
                return PoseDirection.WW_POSE_DIRECTION_INFERRED;
            default:
                Log.e(TAG, "unknown pose direction: " + dir);
                return PoseDirection.WW_POSE_DIRECTION_INFERRED;
        }
    }

    private static PoseMode getPoseMode(int mode){
        switch (mode){
            case 0:
                return PoseMode.WW_POSE_MODE_GLOBAL;
            case 1:
                return PoseMode.WW_POSE_MODE_RELATIVE_COMMAND;
            case 2:
                return PoseMode.WW_POSE_MODE_RELATIVE_MEASURED;
            case 3:
                return PoseMode.WW_POSE_MODE_SET_GLOBAL;
            default:
                Log.e(TAG, "unknown pose mode: " + mode);
                return PoseMode.WW_POSE_MODE_RELATIVE_COMMAND;
        }
    }

    public static Robot getRobotForUUIDWithLogging(String robotUUID) {
        Robot robot = getRobotForUUID(robotUUID);
        if (robot == null) {
            // alternatively, throw an exception here.
            // difficulty is that it would be nice to extend the ErrorCode enum in APIException.java.
            LoggingHelper.e(TAG, "Unknown robot:%s", robot);
        }
        return robot;
    }

    public static Robot getRobotForUUID(String robotUUID){
        if(connectedRobots.containsKey(robotUUID))
            return connectedRobots.get(robotUUID);
        return null;
    }

    public static void wwSendCommandSet(String robotUUID, RobotCommandSet commandSet){

        Robot robot = getRobotForUUID(robotUUID);
        if(robot!= null)
            robot.sendCommandSet(commandSet);
    }


    public static void wwLog(int level, String line){
/*
		EMERGENCY = 0,      // Something is very wrong.
		CRITICAL  = 1,      // Something is very wrong.
		ALERT     = 2,      // Something is very wrong.
		ERROR     = 3,      // Something is definitely wrong. This should never happen. Investigate!
		WARNING   = 4,      // Something is probably wrong. An engineer should investigate this.
		NOTICE    = 5,      // Unusual, but not necessarily a problem.
		INFO      = 6,      // Informative and useful in production. nothing is wrong.
		DEBUG     = 7,      // Not useful during production: should not even appear in production logs.
*/
        switch (level) {
            case 0:
            case 1:
            case 2:
            case 3:
                LoggingHelper.e(TAG, line);
                break;
            case 4:
                LoggingHelper.w(TAG, line);
                break;
            case 5:
            case 6:
                LoggingHelper.i(TAG, line);
                break;
            case 7:
                LoggingHelper.d(TAG, line);
                break;
            default:
                LoggingHelper.e(TAG, "unrecognized log level: " + level + " - " + line);
                break;
        }
    }
    /******************************* Localization *******************************/

    public static String getCanonicalTextLanguage()
    {
        return LocaleUtility.getCanonicalTextLanguage();
    }

    /******************************* ROBOT SENSORS *******************************/
    public void onEventBackgroundThread(RobotSensorSetUpdated event) throws JSONException
    {
        Robot robot = event.getRobot();
        RobotSensorSet state = event.getRobotSensorSet();
        didReceiveRobotState(state, robot);
    }

  public void onEventBackgroundThread(GestureEvent event) {
    Robot robot = event.getRobot();
    String identifier = event.getIdentifier();

    try {
      JSONArray niceEvents = new JSONArray();
      JSONObject niceEvent = new JSONObject();
      niceEvent.put("id", event.getIdentifier());
      niceEvent.put("phase", event.getPhase().toString());
      niceEvents.put(niceEvent);

      JSONObject json = new JSONObject();
      json.put("method", "didReceiveRobotEvents");
      json.putOpt("robot", robotToJson(robot));
      json.put("events", niceEvents);
      sendToUnity(json.toString());
    }
    catch (JSONException e) {
      // Error building JSON
      LoggingHelper.e(TAG, "Error building json data to be sent to unity");
    }
  }

    public void onEventMainThread(RobotCommandSetSequenceFinished event) throws JSONException
    {
        handleCommandSetStoppedOrFinished(event);
    }

    public void onEventMainThread(RobotCommandSetSequenceStopped event) throws JSONException
    {
        handleCommandSetStoppedOrFinished(event);
    }

    public void onEventMainThread(ShellCommandExecuted event) throws JSONException
    {
        didExecuteShellCommand(event);
    }

    private void handleCommandSetStoppedOrFinished(RobotCommandSetSequenceActivity event) throws JSONException
    {
        String methodName = "";
        if (event instanceof RobotCommandSetSequenceFinished) {
            methodName = "didFinishCommandSequence";
        }
        else {
            methodName = "didStopCommandSequence";
        }

        int index = loadedAnimations.indexOfValue(event.getSequence());
        if (index < 0) {
            LoggingHelper.e(TAG, "unknown sequence!");
            return;
        }

        int sequenceId = loadedAnimations.keyAt(index);

        JSONObject jso = new JSONObject();
        jso.put   ("method"    , methodName);
        jso.putOpt("robot"     , robotToJson(event.getRobot()));
        jso.put("sequenceId", sequenceId);

        sendToUnity(jso.toString());
    }

    public void onChromeClosed() throws JSONException{
        JSONObject jso = new JSONObject();
        jso.put   ("method"    , "onChromeClosed");
        sendToUnity(jso.toString());
    }

    public void setSoundRecordSlot(Integer id) throws JSONException{
        JSONObject jso = new JSONObject();
        jso.put   ("method"    , "didSoundTranferingFinished");
        jso.put   ("sound_id", id);

        sendToUnity(jso.toString());
    }

    public void didReceiveRobotState(RobotSensorSet state, Robot robot) throws JSONException
    {
        JSONObject json = new JSONObject();
        json.put("method", "didReceiveRobotState");
        json.putOpt("robot", robotToJson(robot));
        json.put("state", robotStateToJson(state));
        //Log.d(TAG, json.toString());
        sendToUnity(json.toString());
    }

    public JSONObject robotStateToJson(RobotSensorSet state) throws JSONException {
        JSONObject ret = new JSONObject();

        com.w2.impl.engine.component.sensors.Beacon beacon =  (com.w2.impl.engine.component.sensors.Beacon)state.getSensor(RobotSensorIdInternal.BEACON);
        if (beacon != null) {
            JSONObject sensorDict = new JSONObject();
            sensorDict.put("left" , beacon.getDataLeft ());  // note HAL uses "dataL", "dataR".
            sensorDict.put("right", beacon.getDataRight());

            ret.putOpt(Integer.toString(RobotSensorIdInternal.BEACON.value) , sensorDict);
        }

        com.w2.api.engine.components.sensors.BodyPose bodyPose = (com.w2.api.engine.components.sensors.BodyPose)state.getSensor(RobotSensorIdInternal.MOTION_BODY_POSE);
        if(bodyPose != null) {
            JSONObject sensorDict = new JSONObject();
            sensorDict.put("x", bodyPose.getX());
            sensorDict.put("y", bodyPose.getY());
            sensorDict.put("radians", bodyPose.getRadians());
            sensorDict.put("watermark", bodyPose.getPoseWatermark());

            ret.putOpt(Integer.toString(RobotSensorIdInternal.MOTION_BODY_POSE.value) , sensorDict);
        }

        Accelerometer accelerometer = (Accelerometer)state.getSensor(RobotSensorIdInternal.ACCELEROMETER);
        if(accelerometer != null) {
            JSONObject sensorDict = new JSONObject();
            sensorDict.put("x", accelerometer.getX());
            sensorDict.put("y", accelerometer.getY());
            sensorDict.put("z", accelerometer.getZ());
            ret.putOpt(Integer.toString(RobotSensorIdInternal.ACCELEROMETER.value), sensorDict);
        }


        Gyroscope gyroscope = (Gyroscope)state.getSensor(RobotSensorIdInternal.GYROSCOPE);
        if(gyroscope != null) {
            JSONObject sensorDict = new JSONObject();
            sensorDict.put("x", gyroscope.getX());
            sensorDict.put("y", gyroscope.getY());
            sensorDict.put("z", gyroscope.getZ());
            ret.putOpt(Integer.toString(RobotSensorIdInternal.GYROSCOPE.value), sensorDict);
        }

        DistanceInternal distanceBack = (DistanceInternal)state.getSensor(RobotSensorIdInternal.DISTANCE_BACK);
        if(distanceBack != null){
            JSONObject sensorDict = new JSONObject();
            sensorDict.put("distance", distanceBack.getDistance());
            sensorDict.put("reflectance", distanceBack.getReflectance());
            ret.putOpt(Integer.toString(RobotSensorIdInternal.DISTANCE_BACK.value), sensorDict);
        }

        DistanceInternal distanceLeft = (DistanceInternal)state.getSensor(RobotSensorIdInternal.DISTANCE_FRONT_LEFT_FACING);
        if(distanceLeft != null){
            JSONObject sensorDict = new JSONObject();
            sensorDict.put("distance", distanceLeft.getDistance());
            sensorDict.put("reflectance", distanceLeft.getReflectance());
            ret.putOpt(Integer.toString(RobotSensorIdInternal.DISTANCE_FRONT_LEFT_FACING.value), sensorDict);
        }

        DistanceInternal distanceRight = (DistanceInternal)state.getSensor(RobotSensorIdInternal.DISTANCE_FRONT_RIGHT_FACING);
        if(distanceRight != null){
            JSONObject sensorDict = new JSONObject();
            sensorDict.put("distance", distanceRight.getDistance());
            sensorDict.put("reflectance", distanceRight.getReflectance());
            ret.putOpt(Integer.toString(RobotSensorIdInternal.DISTANCE_FRONT_RIGHT_FACING.value), sensorDict);
        }

        Encoder encoderLeft = (Encoder)state.getSensor(RobotSensorIdInternal.ENCODER_LEFT_WHEEL);
        if(encoderLeft != null){
            JSONObject sensorDict = new JSONObject();
            sensorDict.put("encoderDistance", encoderLeft.getDistance());
            ret.putOpt(Integer.toString(RobotSensorIdInternal.ENCODER_LEFT_WHEEL.value), sensorDict);
        }

        Encoder encoderRight = (Encoder)state.getSensor(RobotSensorIdInternal.ENCODER_RIGHT_WHEEL);
        if(encoderRight != null){
            JSONObject sensorDict = new JSONObject();
            sensorDict.put("encoderDistance", encoderRight.getDistance());
            ret.putOpt(Integer.toString(RobotSensorIdInternal.ENCODER_RIGHT_WHEEL.value), sensorDict);
        }

        Button mainButton = (Button)state.getSensor(RobotSensorIdInternal.BUTTON_MAIN);
        if(mainButton != null){
            JSONObject sensorDict = new JSONObject();
            int pressState = mainButton.isPressed()? 1:0;
            sensorDict.put("state", pressState);
            ret.putOpt(Integer.toString(RobotSensorIdInternal.BUTTON_MAIN.value), sensorDict);
        }

        Button button1 = (Button)state.getSensor(RobotSensorIdInternal.BUTTON_1);
        if(button1 != null){
            JSONObject sensorDict = new JSONObject();
            int pressState = button1.isPressed()? 1:0;
            sensorDict.put("state", pressState);
            ret.putOpt(Integer.toString(RobotSensorIdInternal.BUTTON_1.value), sensorDict);
        }

        Button button2 = (Button)state.getSensor(RobotSensorIdInternal.BUTTON_2);
        if(button2 != null){
            JSONObject sensorDict = new JSONObject();
            int pressState = button2.isPressed()? 1:0;
            sensorDict.put("state", pressState);
            ret.putOpt(Integer.toString(RobotSensorIdInternal.BUTTON_2.value), sensorDict);
        }

        Button button3 = (Button)state.getSensor(RobotSensorIdInternal.BUTTON_3);
        if(button3 != null){
            JSONObject sensorDict = new JSONObject();
            int pressState = button3.isPressed()? 1:0;
            sensorDict.put("state", pressState);
            ret.putOpt(Integer.toString(RobotSensorIdInternal.BUTTON_3.value), sensorDict);
        }


        MicrophoneInternal microphone = (MicrophoneInternal)state.getSensor(RobotSensorIdInternal.MICROPHONE);
        if(microphone != null){
            JSONObject sensorDict = new JSONObject();
            sensorDict.put("amplitude", microphone.getAmplitude());
            double angle = microphone.getTriangulationAngle();
            if(!Double.isNaN(angle)) {
                sensorDict.put("direction", angle);
                sensorDict.put("voice_detection_confidence", microphone.getTriangulationConfidence());
            }
            sensorDict.put("event", microphone.getClapDetected() ? 1 : 0);
            ret.putOpt(Integer.toString(RobotSensorIdInternal.MICROPHONE.value), sensorDict);
        }

        com.w2.api.engine.components.sensors.HeadPosition headPositionPan = (com.w2.api.engine.components.sensors.HeadPosition)state.getSensor(RobotSensorIdInternal.HEAD_POSITION_PAN);
        if(headPositionPan != null){
            JSONObject sensorDict = new JSONObject();
            sensorDict.put("angle", headPositionPan.getAngle());
            ret.putOpt(Integer.toString(RobotSensorIdInternal.HEAD_POSITION_PAN.value), sensorDict);
        }

        com.w2.api.engine.components.sensors.HeadPosition headPositionTilt = (com.w2.api.engine.components.sensors.HeadPosition)state.getSensor(RobotSensorIdInternal.HEAD_POSITION_TILT);
        if(headPositionTilt != null){
            JSONObject sensorDict = new JSONObject();
            sensorDict.put("angle", headPositionTilt.getAngle());
            ret.putOpt(Integer.toString(RobotSensorIdInternal.HEAD_POSITION_TILT.value), sensorDict);
        }

        Kidnap kidnap = (Kidnap) state.getSensor(RobotSensorIdInternal.KIDNAP);
        if(kidnap != null){
            JSONObject sensorDict = new JSONObject();
            int detectState = kidnap.getDetected()? 1:0;
            sensorDict.put("flag", detectState);
            ret.putOpt(Integer.toString(RobotSensorIdInternal.KIDNAP.value), sensorDict);
        }

        StallBump stallBump = (StallBump) state.getSensor(RobotSensorIdInternal.STALLBUMP);
        if(stallBump != null){
            JSONObject sensorDict = new JSONObject();
            int detectState = stallBump.getDetected()? 1:0;
            sensorDict.put("flag", detectState);
            ret.putOpt(Integer.toString(RobotSensorIdInternal.STALLBUMP.value), sensorDict);
        }

        SoundPlaying soundPlaying = (SoundPlaying) state.getSensor(RobotSensorIdInternal.SOUND_PLAYING);
        if(soundPlaying != null){
            JSONObject sensorDict = new JSONObject();
            int flag = soundPlaying.getFlag()? 1:0;
            sensorDict.put("flag", flag);
            ret.putOpt(Integer.toString(RobotSensorIdInternal.SOUND_PLAYING.value), sensorDict);
        }

        Battery battery = (Battery) state.getSensor(RobotSensorIdInternal.BATTERY);
        if (battery != null) {
            JSONObject sensorDict = new JSONObject();
            sensorDict.put(Battery.CHARGING, battery.getCharging() ? 1 : 0);
            sensorDict.put(Battery.VOLTAGE , battery.getVoltage ());
            sensorDict.put(Battery.LEVEL   , battery.getBatteryLevel().getValue());
            ret.putOpt(Integer.toString(RobotSensorIdInternal.BATTERY.value), sensorDict);
        }

        // make sure that each sensor in the input is reflected in the output
        RobotSensorSetImpl rssi = (RobotSensorSetImpl)state;
        Set<Integer> sensorIDs = rssi.getSensorIDs();
        for (Integer sensorID : sensorIDs) {
          if (!ret.has(sensorID.toString())) {
            LoggingHelper.e(TAG, "unhandled sensor id: " + sensorID.toString());
          }
        }

        return ret;

    }

    public void refreshConnectedRobots(List<Robot> newConnectBotList) throws JSONException{

        for(Robot bot : connectedRobots.values()){
            if(!newConnectBotList.contains(bot)){
                if(wwMessageReceiverName != null)
                    didDisconnectWithRobot(bot);
                else
                    connectedRobots.remove(bot.getRobotId());
            }
        }
        for(int i = 0; i< newConnectBotList.size(); ++i ){
            if(!connectedRobots.containsKey( getAddress( newConnectBotList.get(i)))){
                if(wwMessageReceiverName != null)
                    didConnectWithRobot(newConnectBotList.get(i));
                else
                    connectedRobots.put(getAddress(newConnectBotList.get(i)), newConnectBotList.get(i));
            }
        }

    }



}
