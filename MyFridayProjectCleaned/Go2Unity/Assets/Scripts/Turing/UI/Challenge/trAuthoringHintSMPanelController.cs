using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using WW.SimpleJSON;
using DG.Tweening;

namespace Turing{
  public class trAuthoringHintSMPanelController : MonoBehaviour {

    public Text NameText;
    public Button OpenAdminPanelButton;
    public Button ActivationCountConfigButton;
    public Button ToClipboardButton;
    public Button FromClipboardButton;
    public InputField DescriptionInput;
    public Button CheckBehaviorCompatibilityButton;
    public Text ErrorText;
    public Button PropagateConfigButton;
    public Button ObscureConfigButton;
    public Button PreviewButton;
    public trAuthoringPropagateController PropgCtrl;
    public trVideoSelectBtnController VideoButton;

    private trProtoController _protoCtrl;
    bool isInPropagateMode = false;
    bool isInActivationCountMode = false;
    bool isInBehaviorAppMode = false;
    bool isInObscureMode = false;
    bool isInPreviewMode = false;

    public void SetupView(trProtoController protoCtrl){
      _protoCtrl = protoCtrl;
      setView();
      OpenAdminPanelButton.onClick.AddListener(()=>onButtonClicked());
      ActivationCountConfigButton.onClick.AddListener(()=>onActivationCountConfigBtnClicked());
      ToClipboardButton.onClick.AddListener(()=>onClickedToClipboard());
      FromClipboardButton.onClick.AddListener(()=>onClickedFromClipboard());
      DescriptionInput.onEndEdit.AddListener(onDescriptionFinished);
      CheckBehaviorCompatibilityButton.onClick.AddListener(onCheckCompatibilityButtonClicked);
      PropagateConfigButton.onClick.AddListener(onPropagateConfigButtonClicked);
      ObscureConfigButton.onClick.AddListener(onObscureConfigButtonClicked);
      PreviewButton.onClick.AddListener(onPreviewButtonClicked);
      VideoButton.OnSave = onVideoChanged;
    }

    void onVideoChanged(trVideoSelectBtnController ctrl){
      trDataManager.Instance.AuthoringMissionInfo.CurHint.VideoPath = ctrl.VideoName.text;
      trDataManager.Instance.MissionMng.AuthoringMissionInfo.SaveCurMission();
    }

    void onObscureConfigButtonClicked(){
      isInObscureMode = !isInObscureMode;
      foreach(trStateButtonController button in _protoCtrl.StateEditCtrl.StateToButtonTable.Values){
        button.IsObscuredToggle.gameObject.SetActive(isInObscureMode);
      }

      foreach(trTransitionArrowController arrow in _protoCtrl.StateEditCtrl.TransitionToArrowTable.Values){
        foreach(trTriggerButtonViewHolder button in arrow.TransitionToTriggerHolder.Values){
          button.ObscureToggle.gameObject.SetActive(isInObscureMode);
        }       
      }
    }

    void onPreviewButtonClicked(){
      isInPreviewMode = !isInPreviewMode;
      foreach(trStateButtonController button in _protoCtrl.StateEditCtrl.StateToButtonTable.Values){
        button.IsObscureAllowed = isInPreviewMode;
        button.SetUpView();
      }

      foreach(trTransitionArrowController arrow in _protoCtrl.StateEditCtrl.TransitionToArrowTable.Values){
        foreach(trTriggerButtonViewHolder button in arrow.TransitionToTriggerHolder.Values){
          button.IsShowObscure = isInPreviewMode;
          button.SetUpView();
        }       
      }
    }

    void onPropagateConfigButtonClicked(){
      isInPropagateMode = !isInPropagateMode;
      foreach(trStateButtonController button in _protoCtrl.StateEditCtrl.StateToButtonTable.Values){
        button.PropagateButton.gameObject.SetActive(isInPropagateMode);
      }

      foreach(trTransitionArrowController arrow in _protoCtrl.StateEditCtrl.TransitionToArrowTable.Values){
        foreach(trTriggerButtonViewHolder button in arrow.TransitionToTriggerHolder.Values){
          button.PropagateButton.gameObject.SetActive(isInPropagateMode);
        }       
      }
    }

    void onCheckCompatibilityButtonClicked(){
      string errorInfo = "";

      foreach(trState state in trDataManager.Instance.GetCurProgram().UUIDToStateTable.Values){
        if(state.Behavior.Type == trBehaviorType.START_STATE){
          continue;
        }
        if(state.Behavior.Type == trBehaviorType.OMNI
           && state.ActivationCount >0){
          errorInfo += "Omni state has activation count more than 0 ";
        }
      }

      if(errorInfo != null){
        ErrorText.text = errorInfo;
        ErrorText.gameObject.SetActive(true);
      }
    }

    void onStateParaToggleChange(bool isOn){
      trDataManager.Instance.MissionMng.AuthoringMissionInfo.CurHint.IsCheckingStatePara = isOn;
      trDataManager.Instance.MissionMng.AuthoringMissionInfo.SaveCurMission();
    }

    void onTriggerParaToggleChange(bool isOn){
      trDataManager.Instance.MissionMng.AuthoringMissionInfo.CurHint.IsCheckingTriggerPara = isOn;
      trDataManager.Instance.MissionMng.AuthoringMissionInfo.SaveCurMission();
    }

    void onDescriptionFinished(string s){
      if(trDataManager.Instance != null){
        trDataManager.Instance.MissionMng.AuthoringMissionInfo.CurHint.Description = s;
        trDataManager.Instance.MissionMng.AuthoringMissionInfo.SaveCurMission();
      }
    }

    void onClickedToClipboard() {
      trClipboardManager.ClipboardValue = _protoCtrl.CurProgram.ToJson().ToString("");
    }
    
    void onClickedFromClipboard() {
      try{
        trFactory.ForgetItems();
        string programJson = trClipboardManager.ClipboardValue;
        JSONNode jsn = JSON.Parse(programJson);
        trProgram program = trFactory.FromJson<trProgram>(jsn);
       
        trDataManager.Instance.MissionMng.AuthoringMissionInfo.CurHint.Program = program;
        trDataManager.Instance.SaveCurProgram();
        _protoCtrl.LoadProgram();
        
      } catch (System.Exception e){
        WWLog.logError(e.ToString());
      }
    }

    void onBehaviorAppearConfigBtnClicked(){
      isInBehaviorAppMode = !isInBehaviorAppMode;
      foreach(trBehaviorButtonController button in _protoCtrl.BehaviorPanelCtrl.BehaviorToButtonDic.Values){
        button.SetActiveToggle(isInBehaviorAppMode);
      }
    }

    void onActivationCountConfigBtnClicked(){
      isInActivationCountMode = !isInActivationCountMode;
      foreach(trStateButtonController button in _protoCtrl.StateEditCtrl.StateToButtonTable.Values){
        button.SetActiveAtvTimeConfigPnl(isInActivationCountMode);
        button.IsObscuredToggle.gameObject.SetActive(isInActivationCountMode);
      }
    }

    void onButtonClicked(){
      trDataManager.Instance.AuthoringMissionCtrl.SetActive(true);
    }

    void setView(){
      if(trDataManager.Instance.MissionMng.AuthoringMissionInfo.EditState != MissionEditState.EDIT_HINT_PROGRAM){
        return;
      }
      trMission mission = trDataManager.Instance.AuthoringMissionInfo.CurMission;
      trPuzzle puzzle = trDataManager.Instance.AuthoringMissionInfo.CurPuzzle;
      trHint hint = trDataManager.Instance.AuthoringMissionInfo.CurHint;
      int puzzleId = mission.Puzzles.IndexOf(puzzle) + 1;
      int hintId = puzzle.Hints.IndexOf(hint) +1;
      NameText.text = mission.UserFacingName  
        + "<color=yellow> -> </color>" + "[" + puzzleId.ToString() + "] " + puzzle.UserFacingName
          + "<color=yellow> -> </color>" + "[" + hintId.ToString() + "] " + hint.UserFacingName;
      DescriptionInput.text = hint.Description;
      VideoButton.VideoName.text = hint.VideoPath;
    }
  }
}
