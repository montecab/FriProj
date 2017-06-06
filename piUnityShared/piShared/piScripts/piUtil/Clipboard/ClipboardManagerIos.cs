using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public class ClipboardManagerIos : trClipboardManager.piClipboardProtocol {

  [DllImport("__Internal")]
  private static extern void _SetClipboard(string value);

  [DllImport("__Internal")]
  private static extern string _GetClipboard();

  public void SaveString (string jsonString) {
    WWLog.logError("not tested");
    _SetClipboard(jsonString);
  }

  public string LoadString () {
    WWLog.logError("not tested");
    return _GetClipboard();
  }
}
