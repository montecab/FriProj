using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using WW.UGUI;
using TMPro;

// todo: this needs to be refactored into a generic class along with trObstacleTypeToggle
namespace Turing{
  public class trRobotTypeToggle : MonoBehaviour {
    public Button SelectButton;
    public TextMeshProUGUI ValueText;

    public delegate void OnClickDelegate(trRobotTypeToggle sender);
    public OnClickDelegate OnClicked;

    private piRobotType robotType = piRobotType.UNKNOWN;
    public piRobotType RobotType{
      get{
        return robotType;
      }
      set{
        robotType = value;
      }
    }

    void Start(){
      SelectButton.onClick.AddListener(onSelectButtonPress);
    }

    void OnDestroy(){
      SelectButton.onClick.RemoveAllListeners();
    }

    void onSelectButtonPress(){
      if (OnClicked != null) OnClicked(this);
    }
  }
}
