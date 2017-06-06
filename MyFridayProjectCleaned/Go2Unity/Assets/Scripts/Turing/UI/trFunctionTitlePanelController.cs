using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace Turing{
  public class trFunctionTitlePanelController : MonoBehaviour {
    public InputField FunctionTitleInput;
    public Button BackButton;
    public trProtoController ProtoCtrl;

    public trFunctionBehavior FunctionBeh;

    // Use this for initialization
    void Start () {
      FunctionTitleInput.onEndEdit.AddListener(onFunctionTitleChange);
      BackButton.onClick.AddListener(onBackButtonClicked);
    }

    public void SetActive(bool active, trFunctionBehavior beh = null){
      this.gameObject.SetActive(active);
      FunctionBeh = beh;
      if(FunctionBeh!= null){
        SetUpView();
      }
    }

    public void SetUpView(){
      FunctionTitleInput.text = FunctionBeh.UserFacingNameLocalized;
    }

    void onBackButtonClicked(){
      ProtoCtrl.ResumeProgram();
    }

    void onFunctionTitleChange(string s){
      FunctionBeh.FunctionProgram.UserFacingName = s;
      trDataManager.Instance.SaveCurProgram();
    }
  }
}


