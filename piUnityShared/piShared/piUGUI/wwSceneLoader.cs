using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class wwSceneLoader : MonoBehaviour {
  public string sceneName;

  public Button theButton;

  void Start() {
    theButton.onClick.AddListener(onClickTheButton);
  }

  void onClickTheButton() {
    SceneManager.LoadScene(sceneName);
  }
}
