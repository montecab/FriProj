using UnityEngine;
using UnityEngine.UI;
using Turing;
using WW.UGUI;
using System;
using System.Collections.Generic;

namespace Turing {

  public class trTankModeEditor : trDoubleSliderBehaviorEditor {

    public List<Image> DirectionImages = new List<Image>();
    public Color DirectionActiveColor;
    public Color DirectionInactiveColor;

    trTankModeDirection calculateDirection(){
      // truncate to integer for easy calculation
      int leftWheelSeed = (int) Math.Round(State.GetBehaviorParameterValue(0));
      int rightWheelSpeed = (int) Math.Round(State.GetBehaviorParameterValue(1));

      // figure out the direction
      if (leftWheelSeed == rightWheelSpeed){
        if (leftWheelSeed == 0) {
          return trTankModeDirection.STOP;
        }
        else if (leftWheelSeed > 0) {
          return trTankModeDirection.FORWARD_FACE_STRAIGHT;
        }
        else {
          return trTankModeDirection.BACKWARD_FACE_STRAIGHT;
        }
      }
      else if (leftWheelSeed == 0){
        return (rightWheelSpeed > 0) ? trTankModeDirection.FORWARD_FACE_LEFT : trTankModeDirection.BACKWARD_FACE_LEFT;
      }
      else if (rightWheelSpeed == 0){
        return (leftWheelSeed > 0) ? trTankModeDirection.FORWARD_FACE_RIGHT : trTankModeDirection.BACKWARD_FACE_RIGHT;
      }
      else{
        // guaranteed leftWheelSpeed != rightWheelSpeed && both != 0
        if ((leftWheelSeed > 0) && (rightWheelSpeed > 0)) {
          return ((leftWheelSeed - rightWheelSpeed) > 0) ? trTankModeDirection.FORWARD_FACE_RIGHT : trTankModeDirection.FORWARD_FACE_LEFT;
        }
        else if ((leftWheelSeed < 0) && (rightWheelSpeed < 0)){
          return ((leftWheelSeed - rightWheelSpeed) > 0) ? trTankModeDirection.BACKWARD_FACE_LEFT : trTankModeDirection.BACKWARD_FACE_RIGHT;
        }
        else {
          // guaranteed that one value is negative while the other is positive, so consider it a spin
          return (leftWheelSeed > 0) ? trTankModeDirection.SPIN_RIGHT : trTankModeDirection.SPIN_LEFT;
        }
      }
    }

    void displayDirection(trTankModeDirection direction){
      // reset direction colors
      foreach(Image directionImage in DirectionImages){
        directionImage.color = DirectionInactiveColor;
      }
      switch (direction){
        case trTankModeDirection.FORWARD_FACE_STRAIGHT:
          DirectionImages[0].color = DirectionActiveColor;
          break;
        case trTankModeDirection.BACKWARD_FACE_STRAIGHT:
          DirectionImages[1].color = DirectionActiveColor;
          break;
        case trTankModeDirection.FORWARD_FACE_LEFT:
          DirectionImages[2].color = DirectionActiveColor;
          break;
        case trTankModeDirection.FORWARD_FACE_RIGHT:
          DirectionImages[3].color = DirectionActiveColor;
          break;
        case trTankModeDirection.BACKWARD_FACE_LEFT:
          DirectionImages[4].color = DirectionActiveColor;
          break;
        case trTankModeDirection.BACKWARD_FACE_RIGHT:
          DirectionImages[5].color = DirectionActiveColor;
          break;
        case trTankModeDirection.SPIN_LEFT:
          DirectionImages[6].color = DirectionActiveColor;
          DirectionImages[7].color = DirectionActiveColor;
          DirectionImages[9].color = DirectionActiveColor;
          DirectionImages[10].color = DirectionActiveColor;
          break;
        case trTankModeDirection.SPIN_RIGHT:
          DirectionImages[6].color = DirectionActiveColor;
          DirectionImages[7].color = DirectionActiveColor;
          DirectionImages[8].color = DirectionActiveColor;
          DirectionImages[11].color = DirectionActiveColor;
          break;
      }
    }

    protected override void UpdateUI(){
      base.UpdateUI();
      trTankModeDirection direction = calculateDirection();
      displayDirection(direction);
    }
  }  
}

