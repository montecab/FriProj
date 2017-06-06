using UnityEngine;
using System.Collections;

public static class ClipboardManagerFactory {

  public static trClipboardManager.piClipboardProtocol getPlatformRelatedManager() {
    trClipboardManager.piClipboardProtocol result = null;
#if UNITY_IOS && !UNITY_EDITOR
    result = new ClipboardManagerIos();
#elif UNITY_ANDROID && !UNITY_EDITOR
    result = new ClipboardManagerAndroid();
#else
    result = new ClipboardManagerDesktop();
#endif
    return result;
  }
}
