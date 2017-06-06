using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class wwAverage {
  private float[] items;
  private uint windowSize = 3;
  private int NextItemIndex = 0;

  private bool shouldUpdateAverageValue = false;
  private float prevAverageValue = .0f;

  public wwAverage (uint windowSize) {
    this.WindowSize = windowSize;
  }

  uint WindowSize {
    get {
      return this.windowSize;
    }
    set {
      if (value > 0) {
        windowSize = value;
        items = new float[windowSize];
        NextItemIndex = 0;
      }
      else {
        WWLog.logError("invalid windowSize: " + value);
      }
    }
  }

  public void AddNewValue(float value){

    if (NextItemIndex >= windowSize){
      NextItemIndex = 0;
    }

    items[NextItemIndex] = value;
  
    NextItemIndex++;
    shouldUpdateAverageValue = true;
  }

  public float GetAverageValue(){
    if (shouldUpdateAverageValue){
      recalculateRecentAverageValue();
    }
    return prevAverageValue;
  }
  
  public void SetAverageValue(float value) {
    for (int n = 0; n < items.Length; ++n) {
      items[n] = value;
    }
    prevAverageValue = value;
  }

  private void recalculateRecentAverageValue(){
    double sum = 0;
    foreach(float value in items){
      sum += value;
    }
    prevAverageValue = (float)(sum / items.Length);
  }
}
