using UnityEngine;
using System.Collections;
using System.Reflection;
using System;

public class ClipboardManagerDesktop : trClipboardManager.piClipboardProtocol {

  public void SaveString (string jsonString) {
    GUIUtility.systemCopyBuffer = jsonString;
  }

  public string LoadString () {
    return GUIUtility.systemCopyBuffer;
  }
}
