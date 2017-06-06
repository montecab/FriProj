using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using WW.UGUI;
using UnityEngine.UI;

namespace Turing{
  public class trBMakerTabController : uGUIGridPanelBase {

  	public Button CreateButton;
    public Image DeleteButtonInactive;
    public trProtoController ProtoCtrl;

  

    void Start(){
      CreateButton.onClick.AddListener(()=>onCreateButtonClicked());
    }    

    public void DeleteBehavior(trBehaviorButtonController button){
      Children.Remove(button.gameObject);
      trDataManager.Instance.AppUserSettings.RemoveBehavior(button.BehaviorData);

      if(ProtoCtrl.CurProgram.UUIDToBehaviorTable.ContainsKey(button.BehaviorData.UUID)){
        ProtoCtrl.CurProgram.RemoveBehavior(button.BehaviorData);
        ProtoCtrl.StateEditCtrl.UpdateStates();
        trDataManager.Instance.SaveCurProgram();
      }     

      Destroy(button.gameObject);
     
    }

    void onCreateButtonClicked(){

      trBehavior behavior = new trBehavior(trBehaviorType.MAPSET);
      behavior.UUID = wwUID.getUID();

      behavior.MapSet.Name = "NewBehavior";
      behavior.MapSet.IconName = trIconFactory.UserIconNames[0];

      trDataManager.Instance.AppUserSettings.AddBehavior(behavior);

      trBehaviorButtonController button =  ProtoCtrl.BehaviorPanelCtrl.CreateButton(behavior, this);
      ProtoCtrl.BehaviorMakerPanelCtrl.SetActive(button);

      ScrollRect scrollRect = GetComponentInChildren<ScrollRect>();
      scrollRect.normalizedPosition = Vector2.one;
      scrollRect.velocity = new Vector2(-1000, -1000);
    }

  }
}
