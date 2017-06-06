using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace Turing{
  public class trOkCancelDialog : trNormalDialogBase {

    public Transform ButtonsHolder;

    public Button CancelButton;
    public UnityEngine.Events.UnityAction OnCancelButtonClicked;

    void Start(){
      if (CancelButton != null) {
        CancelButton.onClick.AddListener(onCancelButtonPress);
      }
      SetupDialog();
    }

    void onCancelButtonPress(){
      gameObject.SetActive(false);
      if (OnCancelButtonClicked != null) OnCancelButtonClicked();
    }

    protected override void onBackgroundPress() {
      onCancelButtonPress();
    }

    public void AddButton(Button button){
      button.transform.SetParent(ButtonsHolder.transform);
    }

    public void RemoveButton(Button button){
      Destroy(button.gameObject);
    }
  }
}
