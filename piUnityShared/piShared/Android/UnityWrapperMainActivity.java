package com.makewonder;


import android.content.res.Configuration;
import android.os.Bundle;

import android.support.v4.app.FragmentManager;
import android.support.v7.app.ActionBarActivity;

import android.view.KeyEvent;
import android.view.MotionEvent;
import android.app.AlertDialog;
import android.content.DialogInterface;

import android.view.Window;
import android.webkit.DownloadListener;

import com.makewonder.sharedService.DownloadManager;
import com.makewonder.sharedService.wwDownloadListener;
import com.unity3d.player.UnityPlayer;
import com.w2.api.engine.constants.RobotType;
import com.w2.api.engine.errorhandling.APIException;
import com.w2.api.engine.events.EventBusFactory;
import com.w2.api.engine.events.RobotScannedEvent;
import com.w2.api.engine.events.connection.RobotConnected;
import com.w2.api.engine.events.connection.RobotDisconnected;
import com.w2.api.engine.robots.Robot;
import com.w2.api.engine.robots.RobotManager;
import com.w2.api.engine.robots.RobotManagerFactory;
import com.w2.libraries.chrome.dialogfragments.RobotManagementViewDialogFragment;
import com.w2.libraries.chrome.events.ChromeDialogShow;
import com.w2.libraries.chrome.events.EventDialogClosed;
import com.w2.libraries.chrome.events.RobotAutoConnect;
import com.w2.libraries.chrome.events.SoundRecordedSlot;
import com.w2.libraries.chrome.events.WWEventBus;
import com.w2.libraries.chrome.flurry.WWFlurry;
import com.w2.libraries.chrome.localization.LocaDownloader;
import com.w2.libraries.chrome.update.UpdateManager;
import com.w2.libraries.chrome.update.UpdateManagerCallback;
import com.w2.libraries.chrome.utils.WWConfiguration;
import com.w2.libraries.chrome.views.RobotManagementToggleFragment;
import com.w2.libraries.chrome.voicerecorder.DialogFragmentRecordSound;
import com.w2.logging.LoggingHelper;
import com.w2.libraries.chrome.events.PrivacyDialogShow;

import cz.msebera.android.httpclient.Header;
import org.json.JSONException;

import java.io.File;
import java.io.IOException;
import java.util.Collections;
import java.util.List;

import de.greenrobot.event.EventBus;
import de.greenrobot.event.util.AsyncExecutor;


public class UnityWrapperMainActivity extends ActionBarActivity {

  public static UnityWrapperMainActivity sMainActivity;

  public static UnityWrapperMainActivity getInstance() {
    return sMainActivity;
  }

  protected UnityPlayer mUnityPlayer;
  protected RobotManagementToggleFragment mRobotToggleFragment;
  protected EventBus systemBus;
  protected EventBus chromeEventBus;
  protected RobotManager mRobotManager;
  protected AlertDialog.Builder mSystemDialogBuilder;


  @Override
  protected void onCreate(Bundle savedInstanceState) {

    sMainActivity = this;
    super.onCreate(savedInstanceState);

    WWConfiguration.initialize(this);

    //ChromeView
    wwUnityWrapper.sChromeInterface = new ChromeInterface() {
      @Override
      public void openChrome() {
        m_openChrome();
      }

      @Override
      public void hideChromeButton() {
        m_hideChromeButton();
      }

      @Override
      public void showChromeButton() {
        m_showChromeButton();
      }

      @Override
      public void openVoiceRecording() {
        m_openVoiceRecording();
      }

      @Override
      public void openPrivacyDialog() { m_openPrivacyDialog(); }

      @Override
      public void showConnectToRobotDialog(int robotType) {
        m_showConnectToRobotDialog(robotType);
      }

      @Override
      public void showSystemDialog(String title, String message, String buttonText) {
        m_showSystemDialog(title, message, buttonText);
      }

      @Override
      public void startDownloadLocaFiles(String appName, String version, boolean zip, String fileName, String path ){
        m_startDownloadLocaFiles(appName, version, zip, fileName, path);
      }
    };
    m_hideChromeButton();

    wwUnityWrapper.sLocalNotificationInterface = new LocalNotificationInterface() {
      @Override
      public void scheduleNotification(String title, String body, String jsonParams, long millisecondsSince1970) {
        LocalNotificationAlarm alarm = new LocalNotificationAlarm(getApplicationContext());
        alarm.sendPushNotification(title, body, jsonParams, millisecondsSince1970);
      }
    };
  }

  public void startScan() throws APIException {
    if(mRobotManager != null){
      mRobotManager.startScan();
    }
  }

  public void stopScan() throws APIException {
    if(mRobotManager != null){
      mRobotManager.stopScan();
    }
  }

  public RobotManager getRobotManager(){
    return mRobotManager;
  }

  /**
   * Method invoked by the systemBus {@link de.greenrobot.event.EventBus}.
   *
   * @param event
   */
  public void onEventMainThread(RobotConnected event) throws JSONException {
    Robot robot = event.getRobot();
    wwUnityWrapper.getInstance().didConnectWithRobot(robot);
  }

  public void onEventMainThread(RobotScannedEvent event) throws JSONException {
    Robot robot = event.getRobot();
    wwUnityWrapper.getInstance().didDiscoverWithRobot(robot);
  }


  public void onEventMainThread(RobotDisconnected event) throws JSONException {
    Robot robot = event.getRobot();
    wwUnityWrapper.getInstance().didDisconnectWithRobot(robot);
  }

  public void onEventMainThread(EventDialogClosed event) throws JSONException {
    refreshConnectedRobots();
  }

  public void onEventMainThread(RobotAutoConnect event) throws JSONException {

    refreshConnectedRobots();
    wwUnityWrapper.getInstance().onChromeClosed();

  }

  public void onEventMainThread(SoundRecordedSlot event) throws JSONException {
    List<Integer> updatedSlots = event.getSlots();
    for(Integer slot : updatedSlots) {
      wwUnityWrapper.getInstance().setSoundRecordSlot(slot);
    }
  }

  void refreshConnectedRobots() throws JSONException {
    List<Robot> result = Collections.EMPTY_LIST;
    try {
      result = RobotManagerFactory.getRobotManager(this).getAllConnectedRobots();
    } catch(APIException e) {
      WWFlurry.onError("APIException", e.getDescription(), e);
      LoggingHelper.logException(e);
    }
    wwUnityWrapper.getInstance().refreshConnectedRobots(result);
  }

  //chrome

  private  void m_startDownloadLocaFiles(final String appName,final String version, final boolean zip, final String fileName,final String path ){
    Runnable runnable = new Runnable() {
      @Override
      public void run() {
        LocaDownloader downloader = new LocaDownloader(UnityWrapperMainActivity.sMainActivity, appName, version, true, fileName, path) {
          @Override
          public void onSuccess() {
            LoggingHelper.i("Unity", "Download Success.");
          }

          @Override
          public void onFailure(int errCode) {
            LoggingHelper.i("Unity", "Download failed, err code: " + errCode);
          }
        };
      }
    };
    this.runOnUiThread(runnable);
  }

  private void m_openPrivacyDialog(){
    if(chromeEventBus != null){
        chromeEventBus.post(new PrivacyDialogShow());
    }
  }

  private void m_showSystemDialog(String title, String message, String buttonText) {
    mSystemDialogBuilder = new AlertDialog.Builder(sMainActivity);
    mSystemDialogBuilder.setTitle(title);
    mSystemDialogBuilder.setMessage(message)
            .setCancelable(true)
            .setNeutralButton(buttonText, new DialogInterface.OnClickListener() {
              public void onClick(DialogInterface dialog, int id) {
                // if this button is clicked, just close
                // the dialog box and do nothing
                dialog.dismiss();
              }
            });
    this.runOnUiThread(new Runnable() {
      @Override
      public void run() {
        AlertDialog alertDialog = mSystemDialogBuilder.create();
        alertDialog.requestWindowFeature(Window.FEATURE_NO_TITLE);
        alertDialog.show();
      }
    });
  }


  public void m_hideChromeButton() {
    if(mRobotToggleFragment != null) {
      FragmentManager fragmentManager = this.getSupportFragmentManager();
      fragmentManager.beginTransaction()
              .hide(mRobotToggleFragment)
              .commit();
    }
  }

  public void m_showChromeButton() {
    if(mRobotToggleFragment != null) {
      FragmentManager fragmentManager = this.getSupportFragmentManager();
      fragmentManager.beginTransaction()
              .show(mRobotToggleFragment)
              .commit();
    }
  }

  public void m_openVoiceRecording() {
    DialogFragmentRecordSound.startFromActivity(UnityWrapperMainActivity.this, "PATH");
  }

  public void m_openChrome() {
    RobotManagementViewDialogFragment.showRobotDiscoveryDialog(UnityWrapperMainActivity.this);
  }

  public void m_showConnectToRobotDialog(int robotType) {
    RobotType type = RobotType.EnumFromValue(robotType);
    if (type != RobotType.UNKNOWN){
      WWEventBus.getInstance().post(new ChromeDialogShow("Connect to a", type, 3000));
    }
  }


  // Quit Unity
  @Override
  protected void onDestroy() {
    mUnityPlayer.quit();
    super.onDestroy();
  }

  // Pause Unity
  @Override
  protected void onPause() {
    super.onPause();
    mUnityPlayer.pause();
  }

  // Resume Unity
  @Override
  protected void onResume() {
    super.onResume();
    mUnityPlayer.resume();

    this.systemBus = EventBusFactory.getSystemBus();
    if(!this.systemBus.isRegistered(this))
      this.systemBus.register(this);

    this.chromeEventBus = WWEventBus.getInstance();
    if(!this.chromeEventBus.isRegistered(this)) {
      this.chromeEventBus.register(this);
    }

    try {
      if(mRobotManager == null)
        mRobotManager = RobotManagerFactory.getRobotManager(this);
    } catch(Exception e) {
      System.out.println("IN here");
    }

  }

  public void onEvent(EventDialogClosed event)throws  JSONException{
    wwUnityWrapper.getInstance().onChromeClosed();
  }

  // This ensures the layout will be correct.
  @Override
  public void onConfigurationChanged(Configuration newConfig) {
    super.onConfigurationChanged(newConfig);
    mUnityPlayer.configurationChanged(newConfig);
  }

  // Notify Unity of the focus change.
  @Override
  public void onWindowFocusChanged(boolean hasFocus) {
    super.onWindowFocusChanged(hasFocus);
    mUnityPlayer.windowFocusChanged(hasFocus);
  }

  // For some reason the multiple keyevent type is not supported by the ndk.
  // Force event injection by overriding dispatchKeyEvent().
  @Override
  public boolean dispatchKeyEvent(KeyEvent event) {
    if(event.getAction() == KeyEvent.ACTION_MULTIPLE)
      return mUnityPlayer.injectEvent(event);
    return super.dispatchKeyEvent(event);
  }

  // Pass any events not handled by (unfocused) views straight to UnityPlayer
  @Override
  public boolean onKeyUp(int keyCode, KeyEvent event) {
    return mUnityPlayer.injectEvent(event);
  }

  @Override
  public boolean onKeyDown(int keyCode, KeyEvent event) {
    return mUnityPlayer.injectEvent(event);
  }

  @Override
  public boolean onTouchEvent(MotionEvent event) {
    return mUnityPlayer.injectEvent(event);
  }

  /*API12*/
  public boolean onGenericMotionEvent(MotionEvent event) {
    return mUnityPlayer.injectEvent(event);
  }


}
