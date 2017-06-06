using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;
using WW;
using TMPro;

namespace Turing{
  public class trHintPanelController : trStateMachinePanelBase {

    public UnityAction onCloseButtonClicked;

    [SerializeField]
    private Button _closeButton;
    [SerializeField]
    private TextMeshProUGUI _descriptionText;
    [SerializeField]
    private Button _videoButton;
    [SerializeField]
    private TextMeshProUGUI _videoText;
    [SerializeField]
    private trProtoController _protoCtrl;
    public trProtoController protoCtrl{get{return _protoCtrl;} set{_protoCtrl = value;}}

    private trHint curHint;

    private void Start(){
      _closeButton.onClick.AddListener (onCloseButtonClicked);
      _videoButton.onClick.AddListener(onVideoButtonClicked);
      _videoText.text = wwLoca.Format("@!@Need more help?\nWatch tutorial@!@");
    }

    private void onVideoButtonClicked(){
      _protoCtrl.VideoPanelCtrl.SetActive(true);
      _protoCtrl.VideoPanelCtrl.Play(trDataManager.Instance.MissionMng.GetCurHint().VideoPath, trDataManager.Instance.MissionMng.GetCurMission().UserFacingName);
    }

    public override void SetUpView (trProgram program)
    {
      IsShowObscure = true;
      IsDisableInteraction = true;
      base.SetUpView (program);
    }

    private void OnEnable(){
      //Added this and made hint index default value to 0 because:
      //We want to animate hint button if the user is not making progress for some time and there is an available hint.
      //For every hint we only want to show the animation once. trDataManager.Instance.MissionMng.ActivateNextHint() 
      //gives infomation about if it's the first time activating hint.
      //If tht default value is 1, we dont have the animation for the first hint.
      if(trDataManager.Instance.MissionMng.UserOverallProgress.CurMissionInfo.HintIndex == 0){
        trDataManager.Instance.MissionMng.ActivateNextHint();
      }
      trHint hint = trDataManager.Instance.MissionMng.GetCurHint();
      if(hint != curHint){
        SetUpView(hint.Program);
        bool hasVideo = hint.VideoPath != "";
        #if UNITY_ANDROID
        hasVideo = false;
        #endif
        _videoButton.gameObject.SetActive(hasVideo);
        wwUtility.FitContent2DShallow(StatePanel, 20, 20);
        curHint = hint;
        string hintText = wwLoca.Format("@!@<b>HINT:</b>@!@");
        _descriptionText.text = hintText +" "+ wwLoca.Format(curHint.Description);
      }
    }
  }
}
