using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Turing;

namespace Turing{
  public class trUIController : MonoBehaviour {

    [SerializeField]
    private Button backButton;
    [SerializeField]
    private Button closeButton;

    public WW.BackButtonController.InputEventHandler BackBtnCallback = null; //For pop-up dialog/window that doesn't have uiController.
    public WW.BackButtonController.InputEventHandler CloseBtnCallback = null;

    protected virtual void Awake (){
      if (backButton != null) {
        backButton.onClick.AddListener (onBackButtonClicked);
      }
      if (closeButton != null) {
        closeButton.onClick.AddListener (onCloseButtonClicked);
      }
    }

    protected virtual void OnEnable (){
      WW.BackButtonController.Instance.AddListener(onBackButtonClicked);
    }

    protected virtual void OnDisable (){
      if (WW.BackButtonController.Instance != null) {
        WW.BackButtonController.Instance.RemoveListener(onBackButtonClicked);
      }
    }

    protected virtual void onBackButtonClicked (){
      if (BackBtnCallback != null) {
        BackBtnCallback.Invoke();
      }
    }

    protected virtual void onCloseButtonClicked (){
      if (BackBtnCallback != null) {
        BackBtnCallback.Invoke();
      }
    }
  }
}


