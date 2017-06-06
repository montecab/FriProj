using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using WW;
using TMPro;

namespace Turing{
  public class trCodeRedeemPanelController : trUIController {
    public Button GetProgramButton;
    public TextMeshProUGUI GetProgramButtonLabel;
    public TextMeshProUGUI DescriptionLabel;
    public TMP_InputField CodeInput;
    public trSharedItemDetailPanelController DetailPanelCtrl;

    private bool tryAgain;

    // Use this for initialization
    void Start () {
      GetProgramButton.onClick.AddListener(onGetProgramButtonClicked);
      CodeInput.onValueChanged.AddListener(onCodeInputEdit);
      CodeInput.onEndEdit.AddListener(onCodeInputComplete);
    }

    void OnDestroy(){
      GetProgramButton.onClick.RemoveListener(onGetProgramButtonClicked);
      CodeInput.onValueChanged.RemoveListener(onCodeInputEdit);
      CodeInput.onEndEdit.RemoveListener(onCodeInputComplete);
    }

    protected override void OnEnable(){
      new trTelemetryEvent(trTelemetryEventType.SHR_REDEEM_BTN, true)
        .emit();
      resetUIWithAutoFocusOnInputField(true);
      uGUIPanelTween.Instance.TweenOpen(this.transform);
      base.OnEnable();
    }

    void onCodeInputEdit(string text) {
      GetProgramButton.interactable = (text.Length >= trSharingConstants.MIN_SHARING_TOKEN_LENGTH);
    }

    void onCodeInputComplete(string text) {
      // immediately submit
    }

    void resetUIWithAutoFocusOnInputField(bool autoFocusOnInput) {
      DescriptionLabel.text = wwLoca.Format("@!@Looks like a friend gave you a key.@!@");
      tryAgain = false;
      
      GetProgramButtonLabel.text = wwLoca.Format("@!@Get Program@!@");
      GetProgramButtonLabel.color = new Color(0.28f, 0.2f, 0.49f, 1.0f);
      GetProgramButton.interactable = false;

      CodeInput.text = "";
      CodeInput.gameObject.SetActive(true);
      if (autoFocusOnInput) {
        CodeInput.Select();
      }
    }

    void dismissRedeemPanel() {      
      resetUIWithAutoFocusOnInputField(false);
      uGUIPanelTween.Instance.TweenClose(this.transform);
    }

    void showInvalidCodeUI() {
      GetProgramButtonLabel.text = wwLoca.Format("@!@Try Again@!@");
      DescriptionLabel.text = wwLoca.Format("@!@Wrong key entered.  Please try again!@!@");
      GetProgramButtonLabel.color = new Color(1.0f, 0.28f, 0.34f, 1.0f);
      CodeInput.gameObject.SetActive(false);
      tryAgain = true;
    }

    protected override void onBackButtonClicked(){
      dismissRedeemPanel();
    }

    protected override void onCloseButtonClicked(){
      dismissRedeemPanel();
    }

    void onGetProgramButtonClicked(){
      if (tryAgain) {
        resetUIWithAutoFocusOnInputField(true);
      }
      else {
        DetailPanelCtrl.BackButtonListener = null;
        DetailPanelCtrl.DownloadAndShowSharedItem(CodeInput.text);
        DetailPanelCtrl.OnPayloadFinishListener = onRequestFinish;
      }
    }

    void onRequestFinish(HTTPManager.RequestInfo info){
      DetailPanelCtrl.OnPayloadFinishListener = null;

      if(info.responseCode == 404){
        showInvalidCodeUI();
      }
      else {
        dismissRedeemPanel();     
      }

      new trTelemetryEvent(trTelemetryEventType.SHR_REDEEM_KEY, true)
        .add(trTelemetryParamType.TOKEN, CodeInput.text)
        .add(trTelemetryParamType.SUCCESS, info.success.ToString())
        .emit();
    }
  }
}

