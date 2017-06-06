using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using WW.UGUI;
using TMPro;

namespace Turing{
  public class trObstacleTypeToggle : MonoBehaviour {
    public Button SelectButton;
    public TextMeshProUGUI ValueText;

    public delegate void OnClickDelegate(trObstacleTypeToggle sender);
    public OnClickDelegate OnClicked;

    private trObstacleSeenMode mode = trObstacleSeenMode.SEEN;
    public trObstacleSeenMode Mode{
      get{
        return mode;
      }
      set{
        mode = value;
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
