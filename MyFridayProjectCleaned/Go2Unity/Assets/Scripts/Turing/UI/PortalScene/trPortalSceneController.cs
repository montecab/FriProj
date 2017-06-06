using UnityEngine;
using System.Collections;

public class trPortalSceneController : MonoBehaviour {
  public void openDefaultBrowser(){
    string url = "http://makewonder.com";
    Application.OpenURL(url);
  }
}
