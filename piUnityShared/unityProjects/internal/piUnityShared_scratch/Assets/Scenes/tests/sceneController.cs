using UnityEngine;
using UnityEngine.UI;
using PI;

public class sceneController : MonoBehaviour {
  public Button buttonTest;
  public Button buttonShowModal;

  void Start() {
    buttonTest     .onClick.AddListener(onClickTest);
    buttonShowModal.onClick.AddListener(onClickShowModal);
  }

  void onClickTest() {
    piTest.doTests();
  }

  void onClickShowModal() {
    wwCrudeModal.showModal(buttonShowModal.transform.GetComponentInParent<Canvas>(), "hello");
  }

}
