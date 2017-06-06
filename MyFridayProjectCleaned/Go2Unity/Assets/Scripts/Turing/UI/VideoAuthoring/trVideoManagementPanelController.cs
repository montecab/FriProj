using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

namespace Turing{
  public class trVideoManagementPanelController : MonoBehaviour {
    public GameObject VideoSelectionPrefab;
    public VerticalLayoutGroup ListGroup;

    public trVideoSelectPanelController SelectPanel;

    public Button TriggerButton;
    public Button BehaviorButton;

    public RectTransform TriggerPanel;
    public RectTransform BehaviorPanel;

    public ScrollRect ScrollCtrl;

    private Dictionary<trVideoSelectBtnController, trTriggerType> triggerBtns = new Dictionary< trVideoSelectBtnController, trTriggerType>();
    private Dictionary<trVideoSelectBtnController, trBehaviorType> behaviorBtns = new Dictionary<trVideoSelectBtnController, trBehaviorType>();
   
    private bool isTriggerPanelActive = true;

    // Use this for initialization
    void Start () {
      trDataManager.Instance.Init();
      initView();
      TriggerButton.onClick.AddListener(onTriggerButtonClicked);
      BehaviorButton.onClick.AddListener(onBehaviorButtonClicked);
      updatePanels();
    }

    void onTriggerButtonClicked(){
      if(!isTriggerPanelActive){
        isTriggerPanelActive = true;
        updatePanels();
      }
    }

    void onBehaviorButtonClicked(){
      if(isTriggerPanelActive){
        isTriggerPanelActive = false;
        updatePanels();
      }
    }

    void updatePanels(){
      TriggerButton.image.color = isTriggerPanelActive? Color.green: Color.gray;
      BehaviorButton.image.color = isTriggerPanelActive? Color.gray: Color.green;
      TriggerPanel.gameObject.SetActive(isTriggerPanelActive);
      BehaviorPanel.gameObject.SetActive(!isTriggerPanelActive);
      ScrollCtrl.content = isTriggerPanelActive? TriggerPanel:BehaviorPanel;
    }



    void initView(){
     
      foreach(trTriggerType type in Enum.GetValues(typeof(trTriggerType))){
        if(trTrigger.ShowToUser(type, piRobotType.DASH) || trTrigger.ShowToUser(type, piRobotType.DOT)){
          GameObject newSelectBtn = Instantiate(VideoSelectionPrefab) as GameObject;
          newSelectBtn.transform.SetParent(TriggerPanel.transform, false);
           
          trVideoSelectBtnController ctrl = newSelectBtn.GetComponent<trVideoSelectBtnController>();
          ctrl.VideoSelectPanelCtrl = SelectPanel;
          ctrl.ItemName.text = type.ToString();
          ctrl.VideoName.text = "";
          ctrl.OnSave = save;
          if(trDataManager.Instance.VideoManager.triggerVideos.ContainsKey(type)){
            ctrl.VideoName.text = trDataManager.Instance.VideoManager.triggerVideos[type].FileName;
          }

          triggerBtns.Add( ctrl,type);
        }
      }

      foreach(trBehaviorType type in Enum.GetValues(typeof(trBehaviorType))){
        if(type.IsShowToUser()){
          GameObject newSelectBtn = Instantiate(VideoSelectionPrefab) as GameObject;
          newSelectBtn.transform.SetParent(BehaviorPanel.transform, false);

          trVideoSelectBtnController ctrl = newSelectBtn.GetComponent<trVideoSelectBtnController>();
          ctrl.VideoSelectPanelCtrl = SelectPanel;
          ctrl.ItemName.text = type.ToString();
          ctrl.VideoName.text = "";
          ctrl.OnSave = save;
          if(trDataManager.Instance.VideoManager.behaviorVideos.ContainsKey(type)){
            ctrl.VideoName.text = trDataManager.Instance.VideoManager.behaviorVideos[type].FileName;
          }

          behaviorBtns.Add( ctrl,type);
        }
      }
    }

    void save(trVideoSelectBtnController ctrl){
     

      if(triggerBtns.ContainsKey(ctrl)){
        trTriggerType type = triggerBtns[ctrl];
        if(trDataManager.Instance.VideoManager.triggerVideos.ContainsKey(type)){
          if(ctrl.VideoName.text == "" ){
            trDataManager.Instance.VideoManager.triggerVideos.Remove(type);
          }
          else{
            trDataManager.Instance.VideoManager.triggerVideos[type].FileName = ctrl.VideoName.text;
          }

        }
        else{
          trVideoInfo newInfo = new trVideoInfo();
          newInfo.TriggerType = type;
          newInfo.FileName = ctrl.VideoName.text;
          trDataManager.Instance.VideoManager.triggerVideos.Add(type, newInfo);
        }
      }

      if(behaviorBtns.ContainsKey(ctrl)){
        trBehaviorType type = behaviorBtns[ctrl];
        if(trDataManager.Instance.VideoManager.behaviorVideos.ContainsKey(type)){
          if(ctrl.VideoName.text == "" ){
            trDataManager.Instance.VideoManager.behaviorVideos.Remove(type);
          }
          else{
            trDataManager.Instance.VideoManager.behaviorVideos[type].FileName = ctrl.VideoName.text;
          }

        }
        else{
          trVideoInfo newInfo = new trVideoInfo();
          newInfo.BehaviorType = type;
          newInfo.FileName = ctrl.VideoName.text;
          trDataManager.Instance.VideoManager.behaviorVideos.Add(type, newInfo);
        }
      }
     
      trDataManager.Instance.VideoManager.Save();
    }
  }
}

