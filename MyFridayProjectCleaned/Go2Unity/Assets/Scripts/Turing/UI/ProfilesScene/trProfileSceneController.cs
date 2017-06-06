using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using TMPro;

namespace Turing{
  public class trProfileSceneController : trUIController {

    public TextMeshProUGUI IQPoints;
    public TextMeshProUGUI ChallengeProgress;
    public TextMeshProUGUI ChallengesCompleteText;
    public TextMeshProUGUI CuesText;
    public TextMeshProUGUI MemoriesText;
    public GameObject ItemCuePrefab;
    public GameObject ItemMemoryPrefab;
    public GridLayoutGroup RewardDurablesContainer;
    public GridLayoutGroup StoriesContainer;
    public trVideoPlayerPanelController VideoPanelCtrl;
    public Transform TooltipLayer;
    public int StoryPulseViewLimit = 1;

    private Dictionary<string, trReward> unlockedStoryRewards = new Dictionary<string, trReward>();
    private trReward currentStoryPlayingReward;

    void Start () {
      trDataManager.Instance.Init();
      updateUI();
    }

    protected override void onBackButtonClicked (){
      trNavigationRouter.Instance.ShowSceneWithName(trNavigationRouter.SceneName.LOBBY);
    }

    void updateUI(){
      IQPoints.text = getTotalIQPoints();
      ChallengeProgress.text = getChallengeProgressText();
      ChallengesCompleteText.text = wwLoca.Format("@!@Challenges completed@!@");
      MemoriesText.text = wwLoca.Format("@!@MEMORIES@!@");
      CuesText.text = wwLoca.Format("@!@CUES@!@");
      updateRewardDurablesContainerUI();
      updateStoriesContainerUI();
    }

    string getChallengeProgressText(){
      int totalMissions = trDataManager.Instance.MissionMng.TotalMissionsCount();
      int completedMissions = trDataManager.Instance.MissionMng.CompletedMissionsCount();
      return completedMissions.ToString() + " / " + totalMissions.ToString();
    }

    string getTotalIQPoints(){
      return trDataManager.Instance.GetIQPoints().ToString();
    }

    void onMovieButtonPressed(trProfileMemoryController ctrl){
      if (unlockedStoryRewards.ContainsKey(ctrl.Data)){
        currentStoryPlayingReward = unlockedStoryRewards[ctrl.Data];        
        MovieInfo movie = currentStoryPlayingReward.StoryAnimation();
        if ((movie != null) && (!VideoPanelCtrl.gameObject.activeInHierarchy)){
          VideoPanelCtrl.gameObject.SetActive(true);
          VideoPanelCtrl.Play(movie.FileName);
          updateStoryPlayCount();
          new trTelemetryEvent(trTelemetryEventType.STORY_PROFILE_VIEWCLIP, true)
            .add(trTelemetryParamType.STORY_ID, movie.UUID)
            .emit();
        }
      }
    }

    void updateStoryPlayCount(){
//      VideoPanelCtrl.gameObject.SetActive(false);
      incrementStoryPlayCount(currentStoryPlayingReward.StoryAnimation());
      if (currentStoryPlayingReward != null){
        foreach(Transform child in StoriesContainer.transform){
          trProfileMemoryController itemCtrl = child.gameObject.GetComponent<trProfileMemoryController>();
          if ((itemCtrl != null) && itemCtrl.Data.Equals(currentStoryPlayingReward.UUID)) {
            child.gameObject.GetComponentInChildren<Animation>().enabled = false; // turn off animation
          }
        }
        currentStoryPlayingReward = null;        
      }
    }

    void incrementStoryPlayCount(MovieInfo movie, int increment=1){   
      string movieKey = string.Format("{0}_{1}", TOKENS.STORY_PLAY_COUNT, movie.UUID);    
      int moviePlayCount = getStoryPlayCount(movie);
      moviePlayCount += increment;
      WWLog.logDebug("played video for " + moviePlayCount + " times");
      PlayerPrefs.SetString(movieKey, moviePlayCount.ToString());
    }

    int getStoryPlayCount(MovieInfo movie){
      string movieKey = string.Format("{0}_{1}", TOKENS.STORY_PLAY_COUNT, movie.UUID);      
      int moviePlayCount = 0; 
      if (!string.IsNullOrEmpty(PlayerPrefs.GetString(movieKey))){
        moviePlayCount = Int32.Parse(PlayerPrefs.GetString(movieKey));
      }
      return moviePlayCount;
    }

    void updateRewardDurablesContainerUI(){
      // clear each durable item
      foreach(Transform child in RewardDurablesContainer.transform){
        Destroy(child.gameObject);
      }

      // reload durables

      // many durables appear in more than one reward
      Dictionary<Sprite, trProfileItemController> durableSpritesAlreadyInList = new Dictionary<Sprite, trProfileItemController>();

      foreach(trReward reward in trRewardsManager.Instance.AvailableRewards){
        bool isUnlocked = trRewardsManager.Instance.IsRewardUnlocked(reward);
        foreach(trRewardDurable durable in reward.Durables){
          if (!durableSpritesAlreadyInList.ContainsKey(durable.DisplayImage())) {
            
            GameObject item = Instantiate(ItemCuePrefab) as GameObject;
            trProfileItemController itemCtrl = item.GetComponent<trProfileItemController>();
            itemCtrl.Icon.sprite = durable.DisplayImage();      
            itemCtrl.Locked = !isUnlocked;
            itemCtrl.TooltipLayer = TooltipLayer;
            itemCtrl.TooltipText = durable.DisplayTextLocalized();
            addGameObjectToTransform(item, RewardDurablesContainer.transform);
            durableSpritesAlreadyInList.Add(durable.DisplayImage(), itemCtrl);
          }
          else{
            if(isUnlocked){
              durableSpritesAlreadyInList[durable.DisplayImage()].Locked = !isUnlocked;
            }
          }
        }
      }
    }

    void updateStoriesContainerUI(){
     // clear each reward item
      foreach(Transform child in StoriesContainer.transform){
        Destroy(child);
      }
      unlockedStoryRewards.Clear();

      // reload stories

      // many durables appear in more than one reward
      HashSet<string> durablePayloadsAlreadyInList = new HashSet<string>();

      foreach(trReward reward in trRewardsManager.Instance.UnlockedStoryRewards){
        if (!durablePayloadsAlreadyInList.Contains(reward.Payload)) {
          durablePayloadsAlreadyInList.Add(reward.Payload);
          unlockedStoryRewards[reward.UUID] = reward;
          GameObject item = Instantiate(ItemMemoryPrefab) as GameObject;
          trProfileMemoryController itemCtrl = item.GetComponent<trProfileMemoryController>();
          itemCtrl.Icon.sprite = reward.DisplayImage();
          itemCtrl.Data = reward.UUID;
          itemCtrl.Text.text = wwLoca.Format(reward.Description);
          itemCtrl.OnPressed += onMovieButtonPressed;
          item.GetComponentInChildren<Animation>().enabled = (getStoryPlayCount(reward.StoryAnimation()) < StoryPulseViewLimit);
          addGameObjectToTransform(item, StoriesContainer.transform);
          Debug.Log("adding unlocked story reward: " + reward.UUID);
        }
      }
    }

    void addGameObjectToTransform(GameObject obj, Transform parentTransform){
      obj.transform.SetParent(parentTransform);
      obj.GetComponent<RectTransform>().SetDefaultScale();
      RectTransform rectTransform = obj.GetComponent<RectTransform>();
      Vector3 position = rectTransform.localPosition;
      position.z = 0;
      rectTransform.localPosition = position;
    }
  }  
}

