using UnityEngine;
using System.Collections;

public class ClipboardManagerAndroid : trClipboardManager.piClipboardProtocol {

#if UNITY_ANDROID
  private AndroidJavaObject _getClipboardManagerObject() {
    AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
    AndroidJavaObject context = jc.GetStatic<AndroidJavaObject>("currentActivity");
    string contextClipboardServiceName = "clipboard";
    return context.Call<AndroidJavaObject>("getSystemService", contextClipboardServiceName);
  }

  private AndroidJavaObject _converTextToClipData(string text) {
    AndroidJavaClass jc = new AndroidJavaClass("android.content.ClipData");
    AndroidJavaObject result = jc.CallStatic<AndroidJavaObject>("newPlainText", "Programm", text);
    return result;
  }

  // these are to facilitate passing variables between threads, sort of.
  private bool   _finishedLoadString;
  private string _theLoadString;
  private string _theSaveString;

  private void _SaveString() {
    AndroidJavaObject clipboard = _getClipboardManagerObject();
    AndroidJavaObject clipData = _converTextToClipData(_theSaveString);
    clipboard.Call("setPrimaryClip", clipData);
  }

  private void _LoadString() {
    AndroidJavaObject clipboard = _getClipboardManagerObject();
    AndroidJavaObject clipdata = clipboard.Call<AndroidJavaObject>("getPrimaryClip");
    AndroidJavaObject clipItem = clipdata.Call<AndroidJavaObject>("getItemAt", 0);
    _theLoadString = clipItem.Call<string>("getText");
    _finishedLoadString = true;
  }
#endif
  
  public void SaveString (string jsonString) {
#if UNITY_ANDROID
    _theSaveString = jsonString;
    AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
    AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
    activity.Call("runOnUiThread", new AndroidJavaRunnable(_SaveString));
#endif
  }

  public string LoadString () {
#if UNITY_ANDROID
    float tExpire = Time.realtimeSinceStartup + 0.25f;
    _finishedLoadString = false;
    _theLoadString = "";

    AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
    AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
    activity.Call("runOnUiThread", new AndroidJavaRunnable(_LoadString));

    while (!_finishedLoadString && (Time.realtimeSinceStartup < tExpire)) {
      // hard loop
    }
    if (!_finishedLoadString) {
      WWLog.logError("timed out fetching system clipboard");
    }
    return _theLoadString;
#else
    return null;
#endif
  }
}
