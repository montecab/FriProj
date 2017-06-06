using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class trTooManyRobotPanelController : MonoBehaviour {

  public Button OKButton;
  public trProtoController ProtoCtrl;

  void Start(){
    OKButton.onClick.AddListener(()=>onOKButtonClicked());
  }

  void onOKButtonClicked(){
    #if UNITY_IOS || UNITY_ANDROID
    piConnectionManager.Instance.openChrome();
    #else
    ProtoCtrl.RobotListCtrl.OnClickToggleList();
    #endif

    this.gameObject.SetActive(false);
  }
}
