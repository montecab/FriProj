using UnityEngine;
using UnityEngine.UI;
using Turing;
using WW.UGUI;
using System;


public class wwToggleableButton : MonoBehaviour {

  public Sprite ActiveImage;
  public Sprite InactiveImage;
  public Button ToggleButton;
  public bool Toggleable;

  public delegate void ToggleableButtonDelegate(bool toggleState);
  public ToggleableButtonDelegate OnValueChanged;
  public bool isActive;

  void Awake(){
    ToggleButton.onClick.AddListener(onButtonPress);
    OverrideValue(isActive, false);
  }

  void Init(){
    OverrideValue(isActive, false);
  }

  void onButtonPress(){
    if (Toggleable){
      OverrideValue(!isActive);
    }
    else {
      if (OnValueChanged != null) OnValueChanged(true); // always true
    }
  }

  public void OverrideValue(bool toggleValue, bool triggerCallback=true) {
    //Debug.LogError("override value: " + toggleValue);
    isActive = toggleValue;
    ToggleButton.image.sprite = isActive ? ActiveImage : InactiveImage;
    if (triggerCallback && (OnValueChanged != null)) {
      OnValueChanged(isActive);
    }
  }

  public void Reset(){
    isActive = false;
    ToggleButton.image.sprite = InactiveImage;
  }
}

