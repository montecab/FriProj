using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class wwGameObjectActivatorButton : MonoBehaviour {
  public enum Action {
    Activate,
    Deactivate,
    Toggle,
  };
  public enum When {
    OnClick,
    OnStart,
  };

  public GameObject target;
  public Action     action;
  public When       when;

  private Button theButton;

  void Start() {
    if (when == When.OnClick) {
      theButton = gameObject.GetComponent<Button>();
      theButton.onClick.AddListener(onClickTheButton);
    }
    else if (when == When.OnStart) {
      doAction();
    }
  }

  void onClickTheButton() {
    doAction();
  }

  void doAction() {
    if (target == null) {
      WWLog.logError("target is null");
      return;
    }

    switch (action) {
      default: {
        WWLog.logError("Unhandled action: " + action.ToString());
        break;
      }
      case Action.Activate: {
        target.SetActive(true);
        break;
      }
      case Action.Deactivate: {
        target.SetActive(false);
        break;
      }
      case Action.Toggle: {
        target.SetActive(!target.activeSelf);
        break;
      }
    }
  }
}
