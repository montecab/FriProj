using UnityEngine;
using UnityEngine.UI;
using Turing;
using WW.UGUI;
using System;

namespace Turing{
  public class trHeadPanEditor : trSingleSliderBehaviorEditor {

    public Image LeftDirection;
    public Image RightDirection;
    public Color DirectionActiveColor;
    public Color DirectionInactiveColor;
    public int StraightRange;

    protected override void UpdateUI(){
      base.UpdateUI();
      int behaviorValue = (int) Math.Round(State.GetBehaviorParameterValue(0));
      LeftDirection.color = DirectionInactiveColor;
      RightDirection.color = DirectionInactiveColor;
      if (behaviorValue < (StraightRange * -1)){
        RightDirection.color = DirectionActiveColor;      
      }
      else if (behaviorValue > StraightRange){
        LeftDirection.color = DirectionActiveColor;
      }
    }

    protected override void SetupByState(trState newState) {
      base.SetupByState(newState);
      TestButton.Toggleable = false;
    }
  }

}
