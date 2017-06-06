using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using WW.UGUI;
using TMPro;

// todo: this needs to be refactored into a generic dropdown class along with trObstacleTypeToggleGroup
namespace Turing{
  public class trRobotTypeToggleGroup : MonoBehaviour { 

    public List<trRobotTypeToggle> toggles;
    public delegate void OnValueChangedDelegate(trRobotTypeToggleGroup sender);
    public OnValueChangedDelegate OnValueChanged;

    private piRobotType selectedItem = piRobotType.UNKNOWN; // default
    public piRobotType SelectedItem{
      get{
        return selectedItem;
      }
      set{
        initialize();
        selectedItem = value;
        updateUIAndCloseMenu();
      }
    }

    private List<trRobotTypeToggle> items = new List<trRobotTypeToggle>();
    protected bool isInitialized;

    void Start(){
      initialize();
    }

    void initialize () {
      if (!isInitialized){
        int index = 0;
        foreach(piRobotType type in System.Enum.GetValues(typeof(piRobotType))){
          if(index>=toggles.Count){
            break;
          }
          trRobotTypeToggle itemButton = toggles[index];
          itemButton.RobotType = type;
          itemButton.ValueText.text = displayStringForItem(type);
          itemButton.OnClicked += onItemPressed;
          items.Add(itemButton);
          index++;
        }
        isInitialized = true;
      }
    }

    void onItemPressed(trRobotTypeToggle item){
      WWLog.logDebug("selected robot type: " + item.RobotType);
      SelectedItem = item.RobotType;
      foreach (trRobotTypeToggle toggle in toggles){
        foreach (Transform child in toggle.transform){
          child.gameObject.SetActive(toggle==item);
        }
      }
      if (OnValueChanged != null) OnValueChanged(this);
    }

    void updateUIAndCloseMenu(){
      foreach (trRobotTypeToggle toggle in toggles){
        foreach (Transform child in toggle.transform){
          child.gameObject.SetActive((toggle.RobotType == selectedItem));
        }
      }
    }

    string displayStringForItem(piRobotType type){      
      switch (type){
        case piRobotType.UNKNOWN:
          return wwLoca.Format("@!@Dash or Dot@!@");
        case piRobotType.DASH:
          return "Dash";          
        case piRobotType.DOT:
          return "Dot";          
        }
      return "";
    }
  }
}