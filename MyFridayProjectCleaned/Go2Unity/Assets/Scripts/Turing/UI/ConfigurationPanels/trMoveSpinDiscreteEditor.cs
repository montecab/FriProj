using UnityEngine;
using UnityEngine.UI;
using Turing;
using System;
using System.Collections.Generic;
using WW.UGUI;

namespace Turing {
  public class trMoveSpinDiscreteEditor : trSingleSliderBehaviorEditor {

    public List<Image> DirectionImages = new List<Image>();
    public Color DirectionActiveColor;
    public Color DirectionInactiveColor;
    public int StopRange;

    protected override void SetupByState(trState newState) {
      base.SetupByState(newState);
      TestButton.Toggleable = false;
    }

    protected override void UpdateUI(){
      base.UpdateUI();
      // reset direction colors
      foreach(Image directionImage in DirectionImages){
        directionImage.color = DirectionInactiveColor;
      }
      float degree = State.GetBehaviorParameterValue(0);
      if (degree > StopRange){
        DirectionImages[0].color = DirectionActiveColor;
        DirectionImages[1].color = DirectionActiveColor;
        DirectionImages[3].color = DirectionActiveColor;
        DirectionImages[4].color = DirectionActiveColor;
      }
      else if (degree < -StopRange) {
        DirectionImages[0].color = DirectionActiveColor;
        DirectionImages[1].color = DirectionActiveColor;
        DirectionImages[2].color = DirectionActiveColor;
        DirectionImages[5].color = DirectionActiveColor;        
      }
    }
  }  
}

