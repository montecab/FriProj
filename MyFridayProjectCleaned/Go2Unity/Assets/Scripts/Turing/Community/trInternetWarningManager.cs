using UnityEngine;
using System.Collections;
using TMPro;
using WW;

public class trInternetWarningManager : MonoBehaviour {

  public GameObject WarningDialog;
  public TextMeshProUGUI WarningText;

  public GameObject Spinner;

  //from chris: TUR-2018
  private const string NO_INTERNET_TITLE = "Your Internet Connection";
  private const string NO_INTERNET_MESSAGE = "Please make sure your device is connected to the Internet.";
  private const string SERVER_FAILURE_TITLE = "Temporary Server Failure";
  private const string SERVER_FAILURE_MESSAGE = "One of our servers is misbehaving. Please try again later.";

  public void SetEnableSpinner(bool active){
    Spinner.gameObject.SetActive(active);
  }

  public void SetEnableWarningDialog(HTTPManager.RequestInfo info){
    if(info.isTimeout|| info.responseCode == 0){
      piConnectionManager.Instance.showSystemDialog(NO_INTERNET_TITLE, NO_INTERNET_MESSAGE);
    }   
    else if(info.responseCode >= 500 && info.responseCode < 600){
      piConnectionManager.Instance.showSystemDialog(SERVER_FAILURE_TITLE, SERVER_FAILURE_MESSAGE);
    }
  }
}
