using UnityEngine;
using System.Collections;

public static class trClipboardManager {

  public interface piClipboardProtocol {
    void SaveString(string jsonString);
    string LoadString();
  }

  public static string ClipboardValue {
    set {
      ClipboardManagerFactory.getPlatformRelatedManager().SaveString(value);
    }

    get {
      return ClipboardManagerFactory.getPlatformRelatedManager().LoadString();
    }
  }
}
