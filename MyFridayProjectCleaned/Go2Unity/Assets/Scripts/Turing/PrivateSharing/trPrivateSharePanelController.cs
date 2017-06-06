using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;
using WW;
using TMPro;
using WW.SimpleJSON;
namespace Turing{
   public class trPrivateSharePanelController : MonoBehaviour {

    public GameObject SharePanel;
    public Button UploadOKButton;
    public Button UploadCancelButton;
    public TextMeshProUGUI CodeLabel;
    public TextMeshProUGUI CodePanelLabel;
    public GameObject CodePanel;
    public Button CodePanelOKButton;
    public Button BackgroundButton;

    public void SetupView(){
      UploadOKButton.onClick.AddListener(onCopyButtonClicked);
      UploadCancelButton.onClick.AddListener(onBackButtonClicked);
      CodePanelOKButton.onClick.AddListener(onBackButtonClicked);
      BackgroundButton.onClick.AddListener(onBackButtonClicked);
      displayGenerateTokenPanel();
      new trTelemetryEvent(trTelemetryEventType.SHR_UPLOAD_BTN, true)
        .add(trTelemetryParamType.ROBOT_TYPE, trDataManager.Instance.GetCurProgram().RobotType.ToString())
        .emit();
      WW.BackButtonController.Instance.AddListener(onBackButtonClicked);
    }

    void onBackButtonClicked(){
      SharePanel.SetActive(false);
      CodePanel.SetActive(false);
      this.gameObject.SetActive(false);
      WW.BackButtonController.Instance.RemoveListener(onBackButtonClicked);
    }

    void displayGenerateTokenPanel() {
      CodeLabel.gameObject.SetActive(false);
      UploadOKButton.interactable = false;
      onGetCode(); // fetch the code automatically on display
    }

    void onGetCode(){
      HTTPManager.Instance.downloadUrl(trSharingConstants.GetSharedItemRESTUrl(), onGetCodeFinished, trSharingConstants.DOWNLOAD_TIMEOUT);
      trDataManager.Instance.InternetWarningManager.SetEnableSpinner(true);
      //TODO: add metrics
    }

    void onGetCodeFinished(HTTPManager.RequestInfo info){
      if(!info.isDone){
        return;
      }
      if(info.success){
        // parse token
        JSONNode js = JSON.Parse(info.DownloadedText);
        FilePayload payload = new FilePayload();
        payload.FromJson(js.AsObject);
        CodeLabel.text = payload.Token;

        // enable UI
        CodeLabel.gameObject.SetActive(true);
        UploadOKButton.interactable = true;
        uGUIPanelTween.Instance.TweenOpen(SharePanel.transform);
      }
      else{
        trDataManager.Instance.InternetWarningManager.SetEnableWarningDialog(info);
      }
      trDataManager.Instance.InternetWarningManager.SetEnableSpinner(false);
    }

    void onCopyButtonClicked(){
      string url = trSharingConstants.GetSharedItemRESTUrl(CodeLabel.text);
      trProgram program = trDataManager.Instance.GetCurProgram();
      string origName = program.UserFacingName;
      program.UserFacingName = CodeLabel.text; // COPPA compliant
      if(program != null){
        HTTPManager.Instance.uploadJSON(url, program.ToJson(), onUploadProgram,  trSharingConstants.DOWNLOAD_TIMEOUT);
        trDataManager.Instance.InternetWarningManager.SetEnableSpinner(true);
      }
      program.UserFacingName = origName;
    }

    void onUploadProgram(HTTPManager.RequestInfo info){
      if(!info.isDone){
        return;
      }
      if(info.success){
        trClipboardManager.ClipboardValue  = CodeLabel.text;
        uGUIPanelTween.Instance.TweenOpen(CodePanel.transform);
        CodePanelLabel.text = CodeLabel.text;

      }
      else{       
        trDataManager.Instance.InternetWarningManager.SetEnableWarningDialog(info);
      }

      new trTelemetryEvent(trTelemetryEventType.SHR_UPLOAD, true)
        .add(trTelemetryParamType.TOKEN, CodeLabel.text)
        .add(trTelemetryParamType.SUCCESS, info.success.ToString())
        .emit();
      SharePanel.SetActive(false);
      trDataManager.Instance.InternetWarningManager.SetEnableSpinner(false);
    }
  }
}
