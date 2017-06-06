using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Turing;

public class trTransferButtonConfirmationDialog : MonoBehaviour {

  public const string kDashAnimationFileName = "dash_happy_spark_smTranfer";
  public const string kDotAnimationFileName = "dot_happy_spark_smTranfer";

  public Button StartTransferButton;
  public Button CancelButton;
  public Transform ButtonsHolder;
  public trOkCancelDialog transferingSuccessDialog;

  private trProtoController _protoCtrl;
  private trTransferButtonController _transferButton;

  public void SetupView(trProtoController protoCtrl, trTransferButtonController transferBtnCtrl){
    _protoCtrl = protoCtrl;
    _transferButton = transferBtnCtrl;
    StartTransferButton.onClick.AddListener(onStartTransferButtonClick);
    _transferButton.StatusDelegates = onTransferButtonStatusChanged;
    CancelButton.interactable = true;
  }

  private void onStartTransferButtonClick(){
    _transferButton.ProgramToSend = trDataManager.Instance.GetCurProgram();
    _transferButton.onClick();
    CancelButton.interactable = false;
  }

  private IEnumerator showTransferSuccessDialog() {
    //We are faking a transferring animation here. After the transfer is done for about 4 seconds, we play a progress animation on the bot
    // and play a sound on the ipad before showing the confirmation dialog
    piBotBo currentRobot = (piBotBo)_protoCtrl.CurRobot;
    if(currentRobot!=null){
      string animFileToPlay = currentRobot.robotType == piRobotType.DASH ? kDashAnimationFileName : kDotAnimationFileName;
      string animJson = trMoodyAnimations.Instance.getJsonForAnim(animFileToPlay);
      currentRobot.cmd_startSingleAnim(animJson);
      
      yield return new WaitForSeconds(0.2f);

      SoundManager.soundManager.PlaySound(SoundManager.trAppSound.TRANSFER_SM);
      yield return new WaitForSeconds(4.2f);
      // Done with the fake progress animation and sound

      // Now show the success dialog
      transferingSuccessDialog.OnCancelButtonClicked = delegate() {
        transferingSuccessDialog.SetActive(false);
        gameObject.SetActive(false);
      };
      transferingSuccessDialog.OnOKButtonClicked = delegate() {
        piConnectionManager.Instance.BotInterface.disconnectRobot(_protoCtrl.CurRobot.UUID);
        transferingSuccessDialog.SetActive(false);
        gameObject.SetActive(false);
      };
      if (configureResultDialog(transferingSuccessDialog)) {
        transferingSuccessDialog.SetActive(true);
      }
    }
    else{
      gameObject.SetActive(false);
    }
  }
  
  private void onTransferButtonStatusChanged (trTransferStatus status) {
    if(status == trTransferStatus.IDLE_WITH_SUCCESS){
      StartCoroutine(showTransferSuccessDialog());
    }
  }
  
  private bool configureResultDialog(trOkCancelDialog dialog){
    if (_protoCtrl.CurRobot == null) {
      WWLog.logError("robot has become null. disconnected ?");
      return false;
    }
    else {
      string robotName = _protoCtrl.CurRobot.Name;
      if (TMPro.TMP_TextUtilities.HasUnsupportedCharacters(robotName)) {
        WWLog.logDebug("Robot name has unsupported characters.  Replacing with Dash/Dot");
        robotName = wwLoca.Format(trCurRobotController.Instance.CurRobot.robotType == piRobotType.DASH ? "Dash" : "Dot");
      }
      dialog.DescriptionText =  wwLoca.Format(
        "@!@Your program has been transferred to '{0}'.\n\nDisconnect '{1}' and press the Top Button to run your program!\n\nDisconnect now?@!@",
        robotName, robotName);
      return true;
    }
  }

}
