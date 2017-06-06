using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

namespace Turing{
  public class trLobbySceneController : MonoBehaviour {

    public Button       FreePlayButton;
    public Button       ChallengesButton;
    public Button       DashButton;
    public Button       DotButton;
    public Button       EliProfileButton;
    public Button       RCButton;
    public Button       CommunityButton;
    public EventTrigger SecretButton;
    private bool        secretButtonPressed = false;
    public GameObject   ButtonContainer;

    public Button          PrivacyButtonShow;

    public Image FreeplayImg;
    public Image CommunityImg;
    public Sprite FreeplayInactiveSprite;
    public Sprite CommunityInactiveSprite;

    private const string   PrivacyPolicyPath = "TuringProto/PrivacyPolicy";

    public GameObject TutorialCanvasPrefab;
    public GameObject TutorialCanvasHolder;
    private GameObject tutorial = null;
    private event piConnectionManager.OnChromeClosedDelegate onChromeClose = null;

    public GameObject FreePlayParticle;
    public GameObject CommunityParticle;

    public List<uint>DashSounds;
    public List<uint>DotSounds;

    public trNormalDialogBase WarningDialog;
    public trRobotSelectionController RobotSelectionController;

    private enum RobotSelectionIntent {
      FREE_PLAY      = 0,
      MAP            = 1,
      UNKNOWN        = 2,
      REMOTE_CONTROL = 3,
      VAULT          = 4,
    }
    private RobotSelectionIntent robotSelectionIntent = RobotSelectionIntent.UNKNOWN;
    private const string kPrefLastSelectedRobotType = "LAST_SELECTED_ROBOT_TYPE";

    private const float SECRET_ALPHA_HIGH = 1.0f;
    private const float SECRET_ALPHA_LOW  = 0.5f;
    
    void Start(){
      trDataManager.Instance.Init();
      FreePlayButton  .onClick.AddListener(onFreePlayButtonClicked);
      ChallengesButton.onClick.AddListener(onChallengesButtonClicked);
      DashButton      .onClick.AddListener(onDashButtonClick);
      DotButton       .onClick.AddListener(onDotButtonClick);
      RCButton        .onClick.AddListener(onRCButtonClicked);
      CommunityButton .onClick.AddListener(onCommunityButtonClicked);
      EliProfileButton.onClick.AddListener(onEliProfileButtonClick);
      WarningDialog  .OnOKButtonClicked = onFreePlayDialogOkBtnClicked;

      RCButton      .gameObject.SetActive(trMultivariate.isYESorSHOW(trMultivariate.trAppOption.UNLOCK_CONTROLLER));
            
      EventTrigger.Entry ete;
      ete = new EventTrigger.Entry();
      ete.eventID = EventTriggerType.PointerDown;
      ete.callback.AddListener(onPointerDownSecretButton);
      SecretButton.triggers.Add(ete);
      ete = new EventTrigger.Entry();
      ete.eventID = EventTriggerType.PointerUp;
      ete.callback.AddListener(onPointerUpSecretButton);
      SecretButton.triggers.Add(ete);
      SecretButton.GetComponent<Graphic>().CrossFadeAlpha(0, 0, true);
      
      
      setupParticles();
      setupLockedAreaLabels();
      setupPrivacyPanel();

      SoundManager.soundManager.PlaySound(SoundManager.trAppSound.LOBBY_SOUND);
      RobotSelectionController.OnDismiss += robotTypeSelected;

      //FTUE
      piConnectionManager.Instance.showChromeButton();
      bool isWonderFTUE = !trDataManager.Instance.MissionMng.isAnyChallengeStarted() && !isFreePlayUnlocked();
      bool isChromeFTUE = piConnectionManager.Instance.isAutoConnectInfoEmpty();
      bool isConnectFTUEDone = FTUEManager.Instance.ShouldDisplayFTUE(FTUEType.CHROME); //This is for user who finished chrome ftue but quit the app before starting challenges.
      piConnectionManager.Instance.setFTUEMode(isWonderFTUE && isChromeFTUE && isConnectFTUEDone);
      if(isWonderFTUE || FTUEManager.Instance.ShouldDisplayFTUE(FTUEType.LOBBY_FREEPLAY)){
        if(tutorial==null){
          tutorial = Instantiate(TutorialCanvasPrefab, Vector3.zero, Quaternion.identity) as GameObject;
          tutorial.transform.SetParent(TutorialCanvasHolder.transform, false);
          tutorial.SetActive(false);
        }
        if(FTUEManager.Instance.currentFTUE==0){ //Only emit this when user enters for the first time
          new trTelemetryEvent(trTelemetryEventType.WONDER_FTUE_SHOW, true).emit();
        }
        CheckFTUEDisplay();
      }
      else{
        if(FTUEManager.Instance.currentFTUE==0){ //Only emit this when user enters for the first time
          new trTelemetryEvent(trTelemetryEventType.WONDER_FTUE_SKIP, true).emit();
        }
        FTUEManager.Instance.SkipFTUE();
      }
    }

    private void CheckFTUEDisplay(){
      if(FTUEManager.Instance.ShouldDisplayFTUE(FTUEType.CHROME)){
        if(piConnectionManager.Instance.isAutoConnectInfoEmpty()){
          tutorial.SetActive(true);
          trFTUEController ftueCtrl = tutorial.GetComponent<trFTUEController>();
          ftueCtrl.SetupView(FTUEType.CHROME);
          onChromeClose = ()=>{
            if(piConnectionManager.Instance.GetConnectedRobotType()!= piConnectionManager.ConnectedRobotType.UNKNOWN){
              FTUEManager.Instance.MoveToNextFTUE();
              CheckFTUEDisplay();
            }
          };
          piConnectionManager.Instance.OnChromeClose += onChromeClose;
          #if UNITY_EDITOR  //We don't have chrome in editor, so close the FTUE automatically after 5 secs
          StartCoroutine(WaitAndExecute(5f, ()=>{
            FTUEManager.Instance.MoveToNextFTUE();
            CheckFTUEDisplay();  
          }));
          #endif
        }
        else{
          FTUEManager.Instance.MoveToNextFTUE();
          CheckFTUEDisplay();  
        }
      }
      else if(FTUEManager.Instance.ShouldDisplayFTUE(FTUEType.LOBBY_SCROLLQUEST)){
        piConnectionManager.Instance.OnChromeClose -= onChromeClose;
        tutorial.SetActive(true);
        piConnectionManager.Instance.hideChromeButton();
        trFTUEController ftueCtrl = tutorial.GetComponent<trFTUEController>();
        ftueCtrl.SetupView(FTUEType.LOBBY_SCROLLQUEST,onChallengesButtonClicked);
      }
      else if(FTUEManager.Instance.ShouldDisplayFTUE(FTUEType.LOBBY_FREEPLAY) && isFreePlayUnlocked()){
        tutorial.SetActive(true);
        piConnectionManager.Instance.hideChromeButton();
        trFTUEController ftueCtrl = tutorial.GetComponent<trFTUEController>();
        ftueCtrl.SetupView(FTUEType.LOBBY_FREEPLAY,onFreePlayButtonClicked);
      }
    }

    #if UNITY_EDITOR
    private IEnumerator WaitAndExecute(float time, UnityEngine.Events.UnityAction action){
      yield return new WaitForSeconds(time);
      action();
    }
    #endif
    
    // todo: refactor this. it's too complicated and redundant.
    private void robotTypeSelected(bool selected, piRobotType selectedType) {
      if (selected) {
        trDataManager.Instance.CurrentRobotTypeSelected = selectedType;
        PlayerPrefs.SetInt(kPrefLastSelectedRobotType, (int)selectedType);
        PlayerPrefs.Save();
        switch(robotSelectionIntent) {
          case RobotSelectionIntent.FREE_PLAY:
            showFreePlay();
            break;
          case RobotSelectionIntent.MAP:
            showChallengeMap();
            break;
          case RobotSelectionIntent.REMOTE_CONTROL:
            showRemoteControl();
            break;
          case RobotSelectionIntent.VAULT:
            showVault();
            break;
          default:
            Debug.LogError("The robot type selection UI was shown without an intent.");
            break;
        }
        robotSelectionIntent = RobotSelectionIntent.UNKNOWN;
      }
      else {
        RobotSelectionController.gameObject.SetActive(false);
      }
    }

    void showRemoteControl(){
      trNavigationRouter.Instance.ShowSceneWithName(trNavigationRouter.SceneName.REMOTE_CONTROL);
    }

    void setupLockedAreaLabels(){
      float lockedAlpha = 0.5f;
      float unlockedAlpha = 1.0f;
      TextMeshProUGUI freePlayLabel = FreePlayButton.GetComponentInChildren<TextMeshProUGUI>();
      if (freePlayLabel != null){
        freePlayLabel.color = wwColorUtil.ColorWithAlpha(freePlayLabel.color, isFreePlayUnlocked() ? unlockedAlpha : lockedAlpha);
      }
      TextMeshProUGUI communityLabel = CommunityButton.GetComponentInChildren<TextMeshProUGUI>();
      if (communityLabel != null){
        communityLabel.color = wwColorUtil.ColorWithAlpha(communityLabel.color, isCommunityUnlocked() ? unlockedAlpha : lockedAlpha);
      }

      if(!isFreePlayUnlocked()){
        FreeplayImg.sprite = FreeplayInactiveSprite;
      }
      if(!isCommunityUnlocked()){
        CommunityImg.sprite = CommunityInactiveSprite;
      }

    }

    void onFreePlayDialogOkBtnClicked(){
      WarningDialog.gameObject.SetActive(false);
    }
    
    void setupParticles() {
      FreePlayParticle.SetActive(trDataManager.Instance.MissionMng.UserOverallProgress.IsFreePlayUnlocked
        &&!trDataManager.Instance.MissionMng.UserOverallProgress.UserHasVisitedFreeplay);
      CommunityParticle.SetActive(trDataManager.Instance.MissionMng.UserOverallProgress.IsCommunityUnlocked
        &&!trDataManager.Instance.MissionMng.UserOverallProgress.UserHasVisitedCommunity);

      bool mute = shouldMuteParticles();
      muteParticleSystem(FreePlayParticle.GetComponent<ParticleSystem>(), mute);
      muteParticleSystem(CommunityParticle.GetComponent<ParticleSystem>(), mute);
    }
    
    void muteParticleSystem(ParticleSystem ps, bool mute) {
      bool emit = !mute;
      if (mute) {
        ps.Clear();
      }

      // grr, unity. i've filed a bug about this. - oxe
      // this won't compile:
      // ps.emission.enabled = emit;

      // but this seems to actually work:
      ParticleSystem.EmissionModule tmpEM = ps.emission;
      tmpEM.enabled = emit;
    }
    
    bool shouldMuteParticles() {
      bool ret = false;
      ret = ret || RobotSelectionController.gameObject.activeSelf;
      
      return ret;
    }
    
    void onFreePlayButtonClicked(){
      if (secretCodeMode("LFP ")) {
        return;
      }
      
      if(isFreePlayUnlocked()){
        trDataManager.Instance.MissionMng.UserOverallProgress.UserHasVisitedFreeplay = true;
        trDataManager.Instance.MissionMng.UserOverallProgress.Save();
        showVault();
      }
      else {
        string description = wwLoca.Format("@!@Young robots need training! Finish a few Scroll Quest challenges to unlock Free Play!@!@");
        showWarningDialog(wwLoca.Format("@!@Free Play Locked!@!@"), description);        
      }
    }

    void showWarningDialog(string title, string description){
      WarningDialog.TitleText = title;
      WarningDialog.DescriptionText = description;
      WarningDialog.gameObject.SetActive(true);      
    }

    private bool isFreePlayUnlocked(){
      bool isUnlock = trMultivariate.isYES(trMultivariate.trAppOption.UNLOCK_FREEPLAY);
      isUnlock = isUnlock ||trDataManager.Instance.MissionMng.UserOverallProgress.IsFreePlayUnlocked;
      return isUnlock;
    }

    private bool isCommunityUnlocked(){
      bool isUnlock = trMultivariate.isYES(trMultivariate.trAppOption.UNLOCK_COMMUNITY);
      isUnlock = isUnlock || trDataManager.Instance.MissionMng.UserOverallProgress.IsCommunityUnlocked;
      return isUnlock;      
    }

    private void showFreePlay() {
      trNavigationRouter.Instance.ShowSceneWithName(trNavigationRouter.SceneName.MAIN, trProtoController.RunMode.FreePlay.ToString());
    }
    
    private void showVault() {
      trNavigationRouter.Instance.ShowSceneWithName(trNavigationRouter.SceneName.VAULT);
    }

    void onCommunityButtonClicked(){
      if (isCommunityUnlocked()) {
        trDataManager.Instance.MissionMng.UserOverallProgress.UserHasVisitedCommunity = true;
        trDataManager.Instance.MissionMng.UserOverallProgress.Save();
        trNavigationRouter.Instance.ShowSceneWithName(trNavigationRouter.SceneName.COMMUNITY);        
      }
      else {
        string description = wwLoca.Format("@!@Young robots need training! Finish a few Scroll Quest challenges to unlock the Wonder Cloud.@!@");
        showWarningDialog(wwLoca.Format("@!@Wonder Cloud Locked!@!@"), description);
      }
    }

    void onRCButtonClicked(){
      showRobotSelectionScreenWithDestination(RobotSelectionIntent.REMOTE_CONTROL);
    }

    void onChallengesButtonClicked(){
      showRobotSelectionScreenWithDestination(RobotSelectionIntent.MAP);
    }

    private void showRobotSelectionScreenWithDestination(RobotSelectionIntent destinationScreen) {
      robotSelectionIntent = destinationScreen;
      RobotSelectionController.gameObject.SetActive(true);
    }
    private void showChallengeMap () {
      trNavigationRouter.Instance.ShowSceneWithName(trNavigationRouter.SceneName.MAP);
    }
  
    trRobotSound getRandomSoundByRobotType(piRobotType type){
      List<uint> robotSounds = (type == piRobotType.DOT) ? DotSounds : DashSounds;
      uint randomSoundId = robotSounds[Random.Range(0, robotSounds.Count)];
      WWLog.logDebug("trying to play sound: " + randomSoundId);
      return trRobotSounds.Instance.GetSound(randomSoundId, type);
    }

    void onDashButtonClick(){
      // don't use dash in secret code mode because he's too close to the left border.
      
      piConnectionManager.ConnectedRobotType connectedType = piConnectionManager.Instance.GetConnectedRobotType();
      switch(connectedType){
        case piConnectionManager.ConnectedRobotType.BOTH:
        case piConnectionManager.ConnectedRobotType.DASH_ONLY:
          trRobotSound sound = getRandomSoundByRobotType(piRobotType.DASH);
          foreach (piBotCommon bot in piConnectionManager.Instance.BotsInState(PI.BotConnectionState.CONNECTED)) {
            if ((sound != null) && (bot.robotType == piRobotType.DASH)){
              bot.cmd_playSound(sound.filename);
            }
          }
          break;
        case piConnectionManager.ConnectedRobotType.UNKNOWN:
          piConnectionManager.Instance.openChrome();
          break;
      }
    }
    
    void onDotButtonClick(){
      if (secretCodeMode("LDOT ")) {
        return;
      }
    
      piConnectionManager.ConnectedRobotType connectedType = piConnectionManager.Instance.GetConnectedRobotType();
      switch(connectedType){
        case piConnectionManager.ConnectedRobotType.BOTH:
        case piConnectionManager.ConnectedRobotType.DOT_ONLY:
          trRobotSound sound = getRandomSoundByRobotType(piRobotType.DOT);
          foreach (piBotCommon bot in piConnectionManager.Instance.BotsInState(PI.BotConnectionState.CONNECTED)) {
            if ((sound != null) && (bot.robotType == piRobotType.DOT)){
              bot.cmd_playSound(sound.filename);
            }
          }
          break;
        case piConnectionManager.ConnectedRobotType.UNKNOWN:
          piConnectionManager.Instance.openChrome();
          break;
      }
    }

    void onEliProfileButtonClick(){
      if (secretCodeMode("LIL ")) {
        return;
      }
      trNavigationRouter.Instance.ShowSceneWithName(trNavigationRouter.SceneName.PROFILES, "Eli's Programs");
    }
    
    void setupPrivacyPanel() {
      PrivacyButtonShow   .onClick.AddListener(onClickButtonPrivacyPolicy);
    }
    
    void onClickButtonPrivacyPolicy() {
      piConnectionManager.Instance.openPrivacyWindow();
    }
    
    void flashSecretButton() {
      Graphic sbg = SecretButton.GetComponent<Graphic>();
      Color c = sbg.color;
      if (SecretButton.transform.localScale.x > 1.1f) {
        c.a = SECRET_ALPHA_LOW;
        SecretButton.transform.localScale = Vector3.one;
      }
      else {
        c.a = SECRET_ALPHA_HIGH;
        SecretButton.transform.localScale = Vector3.one * 1.5f;
      }
      
//    sbg.color = c;
    }
    
    bool secretCodeMode(string val) {
      if (secretButtonPressed) {
        flashSecretButton();
        trSecretAdminController.Instance.addToKeyPhrase(val);
        return true;
      }
      else {
        return false;
      }
    }
    
    void fadeSecretButton() {
      SecretButton.transform.localScale = Vector3.one;
      float a = secretButtonPressed ? 1.0f : 0.0f;
      float t = secretButtonPressed ? 0.6f : 1.0f;
      SecretButton.GetComponent<Graphic>().CrossFadeAlpha(a, t, true);
    }
    
    bool UseTouch {
      get {
        return
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
          false
#else
          true
#endif
          ;
      }
      
    }
    
    // in editor / desktop, you click the secret button once then click the other buttons,
    // on multi-touch devices you hold-down the secret button while tapping the other buttons.
    void onPointerDownSecretButton(BaseEventData unused) {
      if (UseTouch) {
        secretButtonPressed = true;
      }
      fadeSecretButton();
      piUnityDelayedExecution.Instance.delayedExecution0(trSecretAdminController.Instance.clearKeyPhrase, 0.01f);
    }
    
    void onPointerUpSecretButton(BaseEventData unused) {
      if (UseTouch) {
        secretButtonPressed = false;
      }
      else {
        secretButtonPressed = !secretButtonPressed;
      }

      fadeSecretButton();
    }
  }
}
