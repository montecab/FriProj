using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class controller : MonoBehaviour {

  public Button theButton;
  public string otherSceneName;
  public Text   modeLabel;

  void Start() {
    theButton.onClick.AddListener(onClickTheButton);

    if (modeLabel != null) {
      modeKeeper.Instance.mode = (modeKeeper.Instance.mode) % 3 + 1;
      wwEntryExitTracker wwEET = GetComponent<wwEntryExitTracker>();
      wwEET.forcedName = wwEntryExitTracker.DefaultName + "_mode_" + modeKeeper.Instance.mode;
      wwEET.doEnter();
      modeLabel.text = "mode " + modeKeeper.Instance.mode;
    }
  }

  void onClickTheButton() {
    SceneManager.LoadScene(otherSceneName);
  }
}

public class modeKeeper : Singleton<modeKeeper> {
  public int mode = 0;
}
