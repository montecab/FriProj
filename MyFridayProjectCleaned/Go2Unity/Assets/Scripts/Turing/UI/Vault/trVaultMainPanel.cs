using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace Turing{
  public class trVaultMainPanel : MonoBehaviour{

    [SerializeField]
    private Button _createProgramButton;

    [SerializeField]
    private Button _enterKeyButton;

    [SerializeField]
    private GameObject _dashProgramList;

    [SerializeField]
    private GameObject _dotProgramList;

    [SerializeField]
    private RectTransform _dashScrollTransform;

    [SerializeField]
    private RectTransform _dotScrollTransform;

    [SerializeField]
    private Transform _buttonsPanel;

    public UnityAction createProgramAction{
      set {
        _createProgramButton.onClick.AddListener(value);
      }
    }

    public UnityAction enterKeyAction{
      set {
        _enterKeyButton.onClick.AddListener(value);
      }
    }

    public GameObject programListForRobotType(piRobotType robotType){
      if(robotType == piRobotType.DASH){
        return _dashProgramList;
      }
      return _dotProgramList;
    }

    public RectTransform scrollTransformForRobotType(piRobotType robotType){
      if(robotType == piRobotType.DASH){
        return _dashScrollTransform;
      }
      return _dotScrollTransform;
    }

    public void setupButtonsForRobot(piRobotType robotType){
      RectTransform curScrollPanel = scrollTransformForRobotType(robotType);
      _buttonsPanel.SetParent(curScrollPanel.transform, false);
      _buttonsPanel.SetAsFirstSibling();
    }
  }
}
