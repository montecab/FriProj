using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using TMPro;

namespace Turing{
  public class trMissionCompletePanelController : trModalPanelBase {

    [Header("UI Components")]
    [SerializeField]
    private TextMeshProUGUI _missionCompleteTitle;
    [SerializeField]
    private GameObject _durablePrefab;
    [SerializeField]
    private Button _continueButton;
    [SerializeField]
    private Button _playButton;

    [Header("UI Layouts")]
    [SerializeField]
    private trMissionCompleteContent _cueMemoryUI;
    [SerializeField]
    private trMissionCompleteContent _cueUI;
    [SerializeField]
    private trMissionCompleteContent _memoryUI;
    [SerializeField]
    private trMissionCompleteContent _textUI;

    private trProtoController _protoCtrl;
    private trUserMissionInfo _completedMission;
    private trVideoPlayerPanelController _videoPanelCtrl;
    private TextMeshProUGUI _missionCompleteDescription;
    private GameObject _durableParent;
    private Image _storyImage;
    private TextMeshProUGUI _storyDescription;
    private Button _storyPlayButton;
    private trReward reward;

    public void SetupView(trProtoController protoCtrl, trUserMissionInfo missionInfo){
      _completedMission = missionInfo;
      _protoCtrl = protoCtrl;
      _videoPanelCtrl  = protoCtrl.VideoPanelCtrl;
      // setup callbacks
      if (trDataManager.Instance.MissionMng.GetCurMission ().EndVideo != "") {
        _protoCtrl.VideoPanelCtrl.Play (trDataManager.Instance.MissionMng.GetCurMission ().EndVideo, trDataManager.Instance.MissionMng.GetCurMission ().UserFacingName);
        _protoCtrl.VideoPanelCtrl.PanelCloseListner = onVideoFinish;
      } else {
        _protoCtrl.PlayCompletedAnimation (); // mission completed, play the associated animation
      }
      _continueButton.onClick.AddListener (onContinueButtonClicked);

      // Depend on the reward type, enable the correct layout and setup ui references
      reward = trRewardsManager.Instance.GrantRewardForIQPoints (trDataManager.Instance.GetIQPoints ());
      trMissionCompleteContent content = null;
      if (reward == null) {
        content = _textUI;
      } else {
        if (reward.HasStory ()) {
          content = _memoryUI;
        } else {
          content = _cueUI;
        }
      }
      content.gameObject.SetActive (true);
      SetupUIReferences (content);

      if (reward != null) {
        WWLog.logDebug ("reward approved: " + reward.UUID);        
        new trTelemetryEvent (trTelemetryEventType.REWARD_UNLOCKED, true)
          .add (trTelemetryParamType.CHALLENGE, trDataManager.Instance.MissionMng.GetCurMission ().UserFacingName)
            .add (trTelemetryParamType.STEP, trDataManager.Instance.MissionMng.UserOverallProgress.CurMissionInfo.PuzzleIndex)
            .add (trTelemetryParamType.REWARD_ID, reward.UUID)
            .add (trTelemetryParamType.IS_REPLAY, trDataManager.Instance.MissionMng.UserOverallProgress.IsCurMissionCompleteOnceInt)
            .emit ();
        updateUIWithReward ();
      } else {
        _missionCompleteTitle.text = wwLoca.Format ("@!@Congratulations!@!@");
        _missionCompleteDescription.text = wwLoca.Format ("@!@You solved it!\nReady for the next challenge?@!@");
      }

      // Setup PlayButton
      if (trDataManager.Instance.MissionMng.isUnlockFreePlayMission (_completedMission) ||
          trDataManager.Instance.MissionMng.isUnlockCommunityMission (_completedMission) ||
          reward != null) {      
        _playButton.gameObject.SetActive (true);
        _playButton.onClick.AddListener (onPlayButtonClicked);
        TextMeshProUGUI buttonLabel = _playButton.GetComponentInChildren<TextMeshProUGUI> ();
        if (reward != null) {
          buttonLabel.text = wwLoca.Format ("@!@Inventor's Log@!@");
          _playButton.gameObject.SetActive(false);
        }
        else if (trDataManager.Instance.MissionMng.isUnlockFreePlayMission (_completedMission)) {
          buttonLabel.text = wwLoca.Format ("@!@Free Play@!@");
          _missionCompleteDescription.text = wwLoca.Format ("@!@<b>Free Play</b> is now open! Go there and invent something new.\nYou can unlock new abilities to use in Free Play by solving more challenges in the Scroll Quest.@!@");
          _continueButton.gameObject.SetActive(false);
        } 
        else {
          buttonLabel.text = wwLoca.Format ("@!@Wonder Cloud@!@");
          _missionCompleteDescription.text = wwLoca.Format ("@!@\n<b>Wonder Cloud</b> is now open!\nGo check out what others have built with Wonder!@!@");
        }
      }
      else {
        _playButton.gameObject.SetActive (false);        
      }  

      // Setup ContinueButton
      bool isLeafNodeMission = trDataManager.Instance.AuthoringMissionInfo.isLeafNodeMission (trDataManager.Instance.MissionMng.GetCurMission ().UUID);
      TextMeshProUGUI continueButtonLabel = _continueButton.transform.GetComponentInChildren<TextMeshProUGUI> ();
      if (isLeafNodeMission) {
        _missionCompleteDescription.text += wwLoca.Format ("@!@\n\nThat's the last one in this path!@!@");
        continueButtonLabel.text = wwLoca.Format ("@!@Back to Map@!@");
      } else {
        continueButtonLabel.text = wwLoca.Format ("@!@Continue Quest@!@");
      }
    }

    private void onVideoFinish(){
      _protoCtrl.PlayCompletedAnimation();
      _protoCtrl.VideoPanelCtrl.PanelCloseListner = null;
    }

    private void SetupUIReferences (trMissionCompleteContent content){
      this._missionCompleteTitle = content.missionCompleteTitle;
      this._missionCompleteDescription = content.missionCompleteDescription;
      this._durableParent = content.rewardHolder;
      this._storyDescription = content.storyDescription;
      this._storyImage = content.storyImage;
      this._storyPlayButton = content.storyPlayButton;
    }

    private void updateUIWithReward() {      
      if (reward != null){
        // we would only display either a story reward or one with durables, but not both.
        if (reward.HasStory()){
          _storyDescription.text = wwLoca.Format(reward.Description);
          _storyImage.gameObject.SetActive(true);
          _storyImage.sprite = reward.DisplayImage();
          _storyPlayButton.onClick.AddListener(()=>{
            MovieInfo movie = reward.StoryAnimation();
            if (movie != null){
              _videoPanelCtrl.gameObject.SetActive(true);
              _videoPanelCtrl.Play(movie.FileName);
              new trTelemetryEvent(trTelemetryEventType.STORY_PROFILE_VIEWCLIP, true)
                .add(trTelemetryParamType.STORY_ID, movie.UUID)
                .emit();
            }
          });
        }
        else {
          //DurableParent.SetActive(true);
          foreach(Transform child in _durableParent.transform){
            Destroy(child.gameObject);
          }
          // only display at most 2 durables
          for (int i = 0; i < Math.Min(reward.Durables.Count, 2); i++){
            var durableGO = (GameObject) Instantiate(_durablePrefab, Vector3.zero, Quaternion.identity);
            trRewardDurableIconController durableScript = durableGO.GetComponent<trRewardDurableIconController>();
            durableScript.Durable = reward.Durables[i];
            durableGO.transform.SetParent(_durableParent.transform, false);
          }
        }
      }
    }

    private void onPlayButtonClicked (){
      if (reward != null) {
        trNavigationRouter.Instance.ShowSceneWithName(trNavigationRouter.SceneName.PROFILES);
      }
      else if (trDataManager.Instance.MissionMng.isUnlockFreePlayMission(_completedMission)){
        trNavigationRouter.Instance.ShowSceneWithName(trNavigationRouter.SceneName.LOBBY);
      }
      else { // we only show this when it is free play or community
        trNavigationRouter.Instance.ShowSceneWithName(trNavigationRouter.SceneName.COMMUNITY);
      }
    }

    private void onContinueButtonClicked(){      
      trNavigationRouter.Instance.ShowSceneWithName(trNavigationRouter.SceneName.MAP, trMissionListController.RunMode.Continue.ToString());
    }

    protected override void SetupUI(){
      base.SetupUI();
    }
  }
}