using UnityEngine;
using UnityEngine.UI;
using Turing;
using WW.UGUI;
using System;

namespace Turing{
  public class trHeadTiltEditor : trSingleSliderBehaviorEditor {

    public Image TopDirection;
    public Image DownDirection;
    public Color DirectionActiveColor;
    public Color DirectionInactiveColor;
    public int StraightRange;


    protected override void UpdateUI(){
      base.UpdateUI();
      int behaviorValue = (int) Math.Round(State.GetBehaviorParameterValue(0));
      TopDirection.color = DirectionInactiveColor;
      DownDirection.color = DirectionInactiveColor;
      if (behaviorValue < (StraightRange * -1)){
        TopDirection.color = DirectionActiveColor;      
      }
      else if (behaviorValue > StraightRange){
        DownDirection.color = DirectionActiveColor;
      }
    }
    
    protected override void SetupByState(trState newState) {
      base.SetupByState(newState);
      TestButton.Toggleable = false;
    }
  }  
}

