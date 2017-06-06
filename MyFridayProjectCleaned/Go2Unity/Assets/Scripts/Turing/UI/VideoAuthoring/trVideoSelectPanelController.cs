using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

namespace Turing{
  public class trVideoSelectPanelController : MonoBehaviour {
    public GameObject TogglePrefab;
    public ToggleGroup VideoToggleGroup;
    public string CurrentSelectedVideo{
      get{
        foreach(string key in videoToToggleTable.Keys){
          if(videoToToggleTable[key].VideoToggle.isOn){
            return key;
          }
        }
        return "";
      }

      set{
        initView();
        foreach(string key in videoToToggleTable.Keys){
          bool ison = key == value;
          videoToToggleTable[key].VideoToggle.isOn = ison;
        }
      }
    }

    public Button ConfirmButton;

    public Action<string> OnConfirm;

    private bool isInited = false;

    private Dictionary<string, trVideoToggleController>  videoToToggleTable = new Dictionary<string, trVideoToggleController>();

    // Use this for initialization
    void Start () {
      trDataManager.Instance.Init();
      ConfirmButton.onClick.AddListener(onConfirmButtonClicked);
      initView();

    }

    void initView(){
      if(isInited)
        return;
      isInited = true;
      createToggle("");
      foreach(string video in trDataManager.Instance.VideoManager.Videos){
        createToggle(video);
      }
    }

    void createToggle(string s){
      GameObject newToggle = Instantiate(TogglePrefab) as GameObject;
      newToggle.transform.SetParent(VideoToggleGroup.transform, false);
      trVideoToggleController ctrl = newToggle.GetComponent<trVideoToggleController>();
      ctrl.VideoToggle.group = VideoToggleGroup;
      ctrl.Label.text = s;
      videoToToggleTable.Add(s, ctrl);
    }

    void onConfirmButtonClicked(){
      if(OnConfirm != null){
        OnConfirm(CurrentSelectedVideo);
      }
      this.gameObject.SetActive(false);
    }
      
  }

}
