using UnityEngine;
using UnityEngine.UI;
using Turing;
using WW.UGUI;
using System;

namespace Turing {
  public class trMoveStraightEditor : trSingleSliderBehaviorEditor {

    public Image UpDirection;
    public Image DownDirection;
    public Color DirectionActiveColor;
    public Color DirectionInactiveColor;


    protected override void UpdateUI(){
      base.UpdateUI();
      int behaviorValue = (int) Math.Round(State.GetBehaviorParameterValue(0));
      UpDirection.color = DirectionInactiveColor;
      DownDirection.color = DirectionInactiveColor;
      if (behaviorValue > 0){
        UpDirection.color = DirectionActiveColor;
      }
      else if (behaviorValue < 0){
        DownDirection.color = DirectionActiveColor;
      }
    }

    protected override void SetupByState(trState newState){
      base.SetupByState(newState);
      TestButton.Toggleable = newState.Behavior.Type == trBehaviorType.MOVE_CONT_STRAIGHT;
    }
  }  
}

