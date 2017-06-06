using UnityEngine;
using UnityEngine.UI;
using Turing;
using WW.UGUI;

public class trModalPanelBase : MonoBehaviour {

  public Button SaveButton;
  public Image HeadingImage;
  public Button OverallBackground;
  public Button FakeCatcher;

  public delegate void OnDialogDissmissDelegate();
  public OnDialogDissmissDelegate OnDialogDissmissed;

  protected virtual void SetupUI () {
    // required items setup
    SaveButton.onClick.AddListener(OnSaveButtonTap);

    if (OverallBackground != null) OverallBackground.onClick.AddListener(Close);
    if (FakeCatcher != null) FakeCatcher.onClick.AddListener(Close);
  }

  protected virtual void UpdateUI() {
    // where child classes can override code
  }

  void Start () {
    SetupUI();
  }

  public void Close(){
    DismissModal(true);
  }

  protected virtual void DismissModal(bool notify) {
    gameObject.SetActive(false);
    if (OnDialogDissmissed != null && notify){
      OnDialogDissmissed();
    }
  }

  protected virtual void OnSaveButtonTap() {
    //Debug.Log("save button pressed");
    DismissModal(true);
  }
}
