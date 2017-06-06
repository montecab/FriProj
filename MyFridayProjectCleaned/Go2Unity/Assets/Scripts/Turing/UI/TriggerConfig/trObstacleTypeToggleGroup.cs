using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using WW.UGUI;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using TMPro;

namespace Turing{

  public enum trObstacleSeenMode{
    SEEN     = 0,
    NOT_SEEN = 1,
  }

  public class trObstacleTypeToggleGroup : MonoBehaviour {

    public GameObject RowItemPrefab;
    public List<trObstacleTypeToggle> toggles;
    public delegate void OnValueChangedDelegate(trObstacleTypeToggleGroup sender);
    public OnValueChangedDelegate OnValueChanged;

    private trObstacleSeenMode selectedItem = trObstacleSeenMode.SEEN;
    public trObstacleSeenMode SelectedItem{
      get {
        return selectedItem;
      }
      set {
        initialize();
        selectedItem = value;
        updateUIAndCloseMenu();
      }
    }

    protected bool isInitialized;

    void Start(){
      initialize();
    }

    void initialize(){   
      if (!isInitialized){
        int index = 0;
        foreach(trObstacleSeenMode mode in System.Enum.GetValues(typeof(trObstacleSeenMode))){
          trObstacleTypeToggle itemButton = toggles[index];
          itemButton.Mode = mode;
          itemButton.ValueText.text = displayStringForItem(mode);
          itemButton.OnClicked += onItemPressed;
          index++;
        }
        isInitialized = true;
      }
    }
    
    void onItemPressed(trObstacleTypeToggle item) {
      WWLog.logDebug("selected mode: " + item.Mode);
      SelectedItem = item.Mode;
      if (OnValueChanged != null) OnValueChanged(this);
      foreach (trObstacleTypeToggle toggle in toggles){
        foreach (Transform child in toggle.transform){
          child.gameObject.SetActive(toggle==item);
        }
      }
    }

    void updateUIAndCloseMenu(){
      foreach (trObstacleTypeToggle toggle in toggles){
        foreach (Transform child in toggle.transform){
          child.gameObject.SetActive((toggle.Mode == selectedItem));
        }
      }
    }
    
    string displayStringForItem(trObstacleSeenMode type){
      switch (type){
        case trObstacleSeenMode.SEEN:
          return wwLoca.Format("@!@SEEN@!@");
        case trObstacleSeenMode.NOT_SEEN:
          return wwLoca.Format("@!@NOT SEEN@!@");
        default:
          return "";
      }
    }
  }
}
