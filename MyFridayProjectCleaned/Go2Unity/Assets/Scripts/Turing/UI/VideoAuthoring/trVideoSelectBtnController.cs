using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using System;

namespace Turing{
  public class trVideoSelectBtnController : MonoBehaviour {
    public Button SelectButton;
    public TextMeshProUGUI VideoName;
    public TextMeshProUGUI ItemName; //trigger, behavior...

    public trVideoSelectPanelController VideoSelectPanelCtrl;
    public Action<trVideoSelectBtnController>  OnSave;

    void Start(){
      SelectButton.onClick.AddListener(onSelectBtnClicked);
    }

    void onSelectBtnClicked(){
      VideoSelectPanelCtrl.gameObject.SetActive(true);
      VideoSelectPanelCtrl.CurrentSelectedVideo = VideoName.text;
      VideoSelectPanelCtrl.OnConfirm = changeVideo;
    }

    void changeVideo(string s){
      VideoName.text = s;
      if(OnSave != null){
        OnSave(this);
      }
    }
  }

}
