using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using WW.UGUI;
using System.Collections.Generic;
using TMPro;

namespace Turing{

  public class trBeaconTriggerConfigPnl : trTriggerToggleGroup {
    public trRobotTypeToggleGroup RobotToggle;
    public GameObject ModelView;
    public GameObject Name;
    public GameObject Description;

    public TextMeshProUGUI InFrontLabel;
    public TextMeshProUGUI NotInFrontLabel;
    public TextMeshProUGUI DashSeesLabel;

    List<trTriggerType> exist;
    List<trTriggerType> none;

    private trTransition currentTransition;

    public override void Init () {
      if (!isInited){
        base.Init ();
        pickupTogglesFromTransform(ModelView.transform);

        RobotToggle.OnValueChanged += updateUIForRobotType;

        exist = new List<trTriggerType>();
        exist.Add(trTriggerType.BEACON_BOTH);
        exist.Add(trTriggerType.BEACON_LEFT);
        exist.Add(trTriggerType.BEACON_RIGHT); 
        none = new List<trTriggerType>(); 
        none.Add(trTriggerType.BEACON_NONE);
        AddReverseGroup(exist, none);
        DashSeesLabel.text = wwLoca.Format("@!@<b>Dash</b> Sees:@!@");
        InFrontLabel.text = wwLoca.Format("@!@<b>In-Front</b>@!@");
        NotInFrontLabel.text = wwLoca.Format("@!@<b>Not In-Front</b>@!@");
      }
    }

    void OnDestroy(){
      RobotToggle.OnValueChanged -= updateUIForRobotType;
    }

    void OnEnable(){
      Name.SetActive(false);
      Description.SetActive(false);
    }

    void OnDisable(){
      Name.SetActive(true);
      Description.SetActive(true);
    }

    public override void SetUp(trTransition transition, trTrigger currentTriggerSet){
      // initialize and figure out the robot type first before calling parent setup
      Init(); 
      currentTransition = transition;
      piRobotType robotType = getRobotType(transition.Trigger);
      RobotToggle.SelectedItem = robotType;
      base.SetUp(transition, currentTriggerSet);
    }

    protected override void disableTogglesFromAnotherTrigger(trTrigger currentTriggerSet, trTrigger anotherTriggerSet){
      piRobotType otherSetRobotType = getRobotType(anotherTriggerSet);
      if (otherSetRobotType == RobotToggle.SelectedItem){
        foreach(trTrigger anotherTrigger in anotherTriggerSet.TriggerSet.Triggers){
          trTriggerType type = detailTypeToGeneralType(anotherTrigger.Type);
          //WWLog.logInfo("disabling: (" + otherSetRobotType + ", " + type + ") for " + anotherTrigger.Type);
          ToggleTable[type].IsEnable = false;
          SetToggle(type, false); // automatically set it to false as well
        }
      }
    }

    protected override void updateViewFromTriggerSet(trTriggerSet triggerSet){
      foreach(trTrigger triggerFromSet in triggerSet.Triggers){
        trTriggerType type = detailTypeToGeneralType(triggerFromSet.Type);
        //WWLog.logInfo("setting " + type + " to on, from " + triggerFromSet.Type);
        SetToggle(type, true);
      }
    }

    // override this method to save the selection to trigger
    protected override void saveToTrigger(){
      if(trigger == null){
        return;
      }
      trigger.TriggerSet.ClearTriggers();
      piRobotType robotType = RobotToggle.SelectedItem;
      foreach(trTriggerType type in ToggleTable.Keys){
        if(ToggleTable[type].IsOn){
          trTriggerType detailType = generalTypeToDetailType(type, robotType);
          trigger.TriggerSet.AddTrigger(new trTrigger(detailType));
        }       
      }
      // string debug = "saved beacon: ";
      // foreach(trTrigger triggerInSet in trigger.TriggerSet.Triggers){
      //   debug += triggerInSet.Type.ToString() + ", ";
      // }
      // WWLog.logError(debug);
    }

    void updateUIForRobotType(trRobotTypeToggleGroup toggleGroup){
      disableTogglesFromOtherTransitions(currentTransition, trigger);
    }

    trTriggerType generalTypeToDetailType(trTriggerType type,piRobotType robotType ){
      switch (robotType){
      case piRobotType.UNKNOWN:
        return type;
      case piRobotType.DOT:
        switch(type){
        case trTriggerType.BEACON_NONE:
          return trTriggerType.BEACON_NONE_DOT;
        case trTriggerType.BEACON_LEFT:
          return trTriggerType.BEACON_LEFT_DOT;
        case trTriggerType.BEACON_RIGHT:
          return trTriggerType.BEACON_RIGHT_DOT;
        case trTriggerType.BEACON_BOTH:
          return trTriggerType.BEACON_BOTH_DOT;
        }
        break;
      case piRobotType.DASH:
        switch(type){
        case trTriggerType.BEACON_NONE:
          return trTriggerType.BEACON_NONE_DASH;
        case trTriggerType.BEACON_LEFT:
          return trTriggerType.BEACON_LEFT_DASH;
        case trTriggerType.BEACON_RIGHT:
          return trTriggerType.BEACON_RIGHT_DASH;
        case trTriggerType.BEACON_BOTH:
          return trTriggerType.BEACON_BOTH_DASH;
        }
        break;
      }
      return trTriggerType.NONE;
    }


    piRobotType getRobotType(trTrigger triggerSet){
      trTriggerType type = trTriggerType.BEACON_BOTH;
      if ((triggerSet.Type == trTriggerType.BEACON_SET) && (triggerSet.TriggerSet.Triggers.Count > 0)) {
        type = triggerSet.TriggerSet.Triggers[0].Type;
      } 
      switch(type){
        case trTriggerType.BEACON_BOTH_DOT:
        case trTriggerType.BEACON_LEFT_DOT:
        case trTriggerType.BEACON_NONE_DOT:
        case trTriggerType.BEACON_RIGHT_DOT:
          return piRobotType.DOT;
        case trTriggerType.BEACON_BOTH_DASH:
        case trTriggerType.BEACON_LEFT_DASH:
        case trTriggerType.BEACON_NONE_DASH:
        case trTriggerType.BEACON_RIGHT_DASH:
          return piRobotType.DASH;
        case trTriggerType.BEACON_BOTH:
        case trTriggerType.BEACON_LEFT:
        case trTriggerType.BEACON_NONE:
        case trTriggerType.BEACON_RIGHT:
          return piRobotType.UNKNOWN;
        default:
          return piRobotType.UNKNOWN;
      }
    }

    trTriggerType detailTypeToGeneralType(trTriggerType type){
      trTriggerType ret = trTriggerType.NONE;

      switch(type){
        case trTriggerType.BEACON_BOTH:
        case trTriggerType.BEACON_BOTH_DASH:
        case trTriggerType.BEACON_BOTH_DOT:
          ret = trTriggerType.BEACON_BOTH;
          break;
        case trTriggerType.BEACON_LEFT:
        case trTriggerType.BEACON_LEFT_DASH:
        case trTriggerType.BEACON_LEFT_DOT:
          ret = trTriggerType.BEACON_LEFT;
          break;
        case trTriggerType.BEACON_NONE:
        case trTriggerType.BEACON_NONE_DASH:
        case trTriggerType.BEACON_NONE_DOT:
          ret =  trTriggerType.BEACON_NONE;
          break;
        case trTriggerType.BEACON_RIGHT:
        case trTriggerType.BEACON_RIGHT_DASH:
        case trTriggerType.BEACON_RIGHT_DOT:
          ret =  trTriggerType.BEACON_RIGHT;
          break;
        default: 
          WWLog.logError("type invalid: " + type);
          break;
      }
      return ret;
    }
  }
}