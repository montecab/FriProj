using UnityEngine;
using System.Collections;
using WW.UGUI;
using System.Collections.Generic;
using WW;
using UnityEngine.UI;
using TMPro;

namespace Turing{
  public class trTriggerToggleGroup : uGUIToggleGroup<trTriggerType> {

    public trTriggerType Type;
    public GameObject WarningLabel;

    protected trTrigger trigger;
    public trTrigger Trigger{
      set{
        if(!value.Type.IsTriggerSet()){
          WWLog.logError("Setting a trigger which is not a trigger set is not allowed");
          return;
        }
        trigger = value;      
        setView();
      }
      get{
        return trigger;
      }
    }

    public override void Init(){
      if (!isInited){
        base.Init();
        if(!Type.IsTriggerSet()){
          WWLog.logError("Invalid trigger type." + Type.ToString());
        }        
        WarningLabel.GetComponent<TextMeshProUGUI>().text = "Nothing Selected!";      
      }
    }

    protected virtual void saveToTrigger(){
      if(trigger == null){
        return;
      }
      trigger.TriggerSet.ClearTriggers();
      foreach(trTriggerType type in ToggleTable.Keys){
        if(ToggleTable[type].IsOn){
          trigger.TriggerSet.AddTrigger(new trTrigger(type));
        }       
      }
    }

    public void SetActive(bool isEnabled){
      if(this.gameObject.activeSelf == isEnabled){
        return;
      }
      this.gameObject.SetActive(isEnabled);
      if(!isEnabled){
        saveToTrigger(); // save to trigger since we are exiting the control
      }
    }

    protected override void onAnyToggleChanged (bool val){
      base.onAnyToggleChanged (val);
      setWarningView();
    }

    protected void pickupTogglesFromTransform(Transform parent){
      foreach(Transform child in parent){
        trTriggerToggle toggle = child.GetComponent<trTriggerToggle>();
        if((toggle != null) && !ToggleTable.ContainsValue(toggle)){
            Register(toggle); 
        }
      }
    }

    // this method is called by trTriggerConfigPanelController and guarantee to be setup
    public virtual void SetUp(trTransition transition, trTrigger currentTriggerSet){      
      Init();

      bool anotherSameTriggerSetExists = disableTogglesFromOtherTransitions(transition, currentTriggerSet);
      if(!anotherSameTriggerSetExists && currentTriggerSet.TriggerSet.Triggers.Count == 0){
        // no triggers set, so set default
        currentTriggerSet.TriggerSet = trTriggerSet.DefaultValue(currentTriggerSet.Type);
      }
      
      Trigger = currentTriggerSet;
    }

    protected virtual bool disableTogglesFromOtherTransitions(trTransition transition, trTrigger currentTriggerSet){      
      // by default enable all toggles
      foreach(trTriggerType type in ToggleTable.Keys){
        ToggleTable[type].IsEnable = true;
      }
      bool anotherSameTriggerSetExists = false;
      foreach(trTransition anotherTransition in transition.StateSource.OutgoingTransitions){
        if((anotherTransition.Trigger != null) && (anotherTransition.Trigger.Type == currentTriggerSet.Type) && (anotherTransition.Trigger != transition.Trigger)){
          anotherSameTriggerSetExists = true;
          disableTogglesFromAnotherTrigger(currentTriggerSet, anotherTransition.Trigger);
        }
      }
      return anotherSameTriggerSetExists;
    }

    protected virtual void disableTogglesFromAnotherTrigger(trTrigger currentTriggerSet, trTrigger anotherTriggerSet){
      // another transition that is the same trigger set as this, disable values that are already taken
      foreach(trTrigger triggerInOtherSet in anotherTriggerSet.TriggerSet.Triggers){
        if (ToggleTable.ContainsKey(triggerInOtherSet.Type)){
          ToggleTable[triggerInOtherSet.Type].IsEnable = false;
          SetToggle(triggerInOtherSet.Type, false);
        }
      }
    }

    protected virtual void setView(){
      // by default set everything to false
      foreach(trTriggerType type in ToggleTable.Keys){
        ToggleTable[type].IsOn = false;
      }

      updateViewFromTriggerSet(trigger.TriggerSet);      
      setWarningView();
    }

    protected virtual void updateViewFromTriggerSet(trTriggerSet triggerSet){
      foreach(trTrigger triggerFromSet in triggerSet.Triggers){
        SetToggle(triggerFromSet.Type, true);
      }
    }

    protected void setWarningView(){
      foreach(trTriggerType type in ToggleTable.Keys){
        if(ToggleTable[type].IsOn){
          WarningLabel.SetActive(false);
          return;
        }
      }
      WarningLabel.SetActive(true);
    }

    // void onBackButtonClicked(){
    //   if(this.gameObject.activeSelf){
    //     BackButton.gameObject.SetActive(false);
    //     SetActive(false);
    //   }
    // }
  }
}
