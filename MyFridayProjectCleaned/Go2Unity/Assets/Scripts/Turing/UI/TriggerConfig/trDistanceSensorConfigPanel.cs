using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Turing{
  public class trDistanceSensorConfigPanel : trTriggerToggleGroup {

    public GameObject SeenView;
    public GameObject NotSeenView;
    //public Button ToggleSeenMode;
    public trObstacleTypeToggleGroup ObstacleToggleGroup;
    public GameObject Name;
    public GameObject Description;

    List<trTriggerType> seenDis;
    List<trTriggerType> notSeeDis;

    void updateViewForSeenMode(trObstacleTypeToggleGroup group){
      //Debug.Log("value set to: " + mode);
      SeenView.SetActive(group.SelectedItem == trObstacleSeenMode.SEEN);
      NotSeenView.SetActive(group.SelectedItem == trObstacleSeenMode.NOT_SEEN);
    }

    public override void Init () {
      if (!isInited){
        base.Init ();

        pickupTogglesFromTransform(SeenView.transform);
        pickupTogglesFromTransform(NotSeenView.transform);

        ObstacleToggleGroup.OnValueChanged += updateViewForSeenMode;

        seenDis = new List<trTriggerType>();
        seenDis.Add(trTriggerType.DISTANCE_CENTER_NEAR);
        seenDis.Add(trTriggerType.DISTANCE_RIGHT_NEAR);
        seenDis.Add(trTriggerType.DISTANCE_LEFT_NEAR);
        seenDis.Add(trTriggerType.DISTANCE_LEFT_FAR);
        seenDis.Add(trTriggerType.DISTANCE_RIGHT_FAR);
        seenDis.Add(trTriggerType.DISTANCE_CENTER_FAR);
        seenDis.Add(trTriggerType.DISTANCE_REAR_NEAR);
        seenDis.Add(trTriggerType.DISTANCE_REAR_FAR);
        
        notSeeDis = new List<trTriggerType>();
        notSeeDis.Add(trTriggerType.DISTANCE_CENTER_NONE);
        notSeeDis.Add(trTriggerType.DISTANCE_REAR_NONE);

        AddReverseGroup(seenDis, notSeeDis);
        AddReverseGroup(notSeeDis, seenDis);
      }
    }

    void OnDestroy(){
      ObstacleToggleGroup.OnValueChanged -= updateViewForSeenMode;
    }

    void OnEnable(){
      Name.SetActive(false);
      Description.SetActive(false);
    }

    void OnDisable(){
      Name.SetActive(true);
      Description.SetActive(true);
    }

    protected override void setView (){
      base.setView ();
      // loop through all the registered toggle areas and deduce the mode from it.
      // assume that either a toggle from seen or unseen group will be set, but not both
      trObstacleSeenMode viewMode = trObstacleSeenMode.SEEN;
      foreach(trTriggerType type in ToggleTable.Keys){
        if(ToggleTable[type].IsOn){
          if(notSeeDis.Contains(type)){           
            viewMode = trObstacleSeenMode.NOT_SEEN;
            break;
          }
        }
      }
      ObstacleToggleGroup.SelectedItem = viewMode;
      updateViewForSeenMode(ObstacleToggleGroup);
    }

  }
}
