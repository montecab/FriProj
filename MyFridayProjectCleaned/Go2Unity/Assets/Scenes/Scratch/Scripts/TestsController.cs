using UnityEngine;
using UnityEngine.UI;

public class TestsController : MonoBehaviour {
  public Button buttonTest;

  void Start() {
    buttonTest.onClick.AddListener(onClickTests);
  }

  void onClickTests() {
    #if UNITY_EDITOR
      piTest.doTests();
      BuildInfo.test();
    #endif
  }
}
