using UnityEngine;
using System.Collections;
using Turing;
using WW.SimpleJSON;
using WW.SaveLoad;
using System.Collections.Generic;

public class trRewardsManager : Singleton<trRewardsManager> {

  private const string RewardsListPath = "TuringProto/trRewardsList";

  private Dictionary<string, trReward> availableRewards = new Dictionary<string, trReward>();
  private HashSet<string> receivedRewards = new HashSet<string>();
  private Dictionary<trRewardDurableCategory, HashSet<string>> lockedRewardDurables = new Dictionary<trRewardDurableCategory, HashSet<string>>();
  private string receivedRewardsFileName;

  public List<trReward> AvailableRewards{
    get{
      return new List<trReward>(availableRewards.Values);
    }
  }  
  public List<trReward> UnlockedStoryRewards{
    get{
      List<trReward> unlockStoryRewards = new List<trReward>();
      foreach(trReward reward in availableRewards.Values){
        //Debug.Log("reward " + reward.UUID + " is unlocked: " + IsRewardUnlocked(reward) + "and has story: " + reward.HasStory());
        if (IsRewardUnlocked(reward) && reward.HasStory()){
          unlockStoryRewards.Add(reward);
        }
      }
      return unlockStoryRewards;
    }
  }

  public void Init(){
    receivedRewardsFileName = Application.persistentDataPath + "/trReceivedRewardsArray.json";    
    loadAllAvailableRewards(); 
    loadReceivedRewards();
    GrantAllRewardsForIQPoints(trDataManager.Instance.GetIQPoints());
    MovieManager.Instance.Load("TuringProto/Movies"); // instantiate the movies
  }

  public trReward GrantRewardForIQPoints(int points){
    trReward rewardToGrant = GetRewardForIQPoints(points);
    if (rewardToGrant != null){
      if (receivedRewards.Contains(rewardToGrant.UUID)) {
        rewardToGrant = null; // already granted the reward, so return null
      }
      else {
        markRewardAsGrant(rewardToGrant); // not there, granted      
      }
    }
    return rewardToGrant;
  }

  public bool IsRewardUnlocked(trReward reward){
    return (reward != null) && (receivedRewards.Contains(reward.UUID) || (trMultivariate.Instance.getOptionValue(trMultivariate.trAppOption.UNLOCK_ALL_REWARDS) == trMultivariate.trAppOptionValue.YES));
  }

  public bool IsAvailableRobotSound(uint soundId){
    if (trDataManager.Instance.IsInNormalMissionMode) {
      return true;
    }
    return isAvailableByCategoryAndId(trRewardDurableCategory.SOUND_ROBOT, soundId.ToString());
  }

  public bool IsAvailableBehavior(trBehavior behavior){
    if (trDataManager.Instance.IsInNormalMissionMode) {
      return true;
    }
    return isAvailableByCategoryAndId(trRewardDurableCategory.BEHAVIOR, behavior.UUID);
  }

  public bool IsAvailableRobotAnim(uint animId){
    if (trDataManager.Instance.IsInNormalMissionMode) {
      return true;
    }
    return isAvailableByCategoryAndId(trRewardDurableCategory.ANIM_ROBOT, animId.ToString());
  }

  public bool IsAvailableTrigger(trTrigger trigger){
    if (trDataManager.Instance.IsInNormalMissionMode) {
      return true;
    }
    return isAvailableByCategoryAndId(trRewardDurableCategory.CUE, trigger.Type.ToString());
  }

  private bool isAvailableByCategoryAndId(trRewardDurableCategory category, string id){
    if (trMultivariate.Instance.getOptionValue(trMultivariate.trAppOption.UNLOCK_ALL_REWARDS) == trMultivariate.trAppOptionValue.YES) {
      return true;
    }
    
    if (category == trRewardDurableCategory.CUE) {
      if (trMultivariate.Instance.getOptionValue(trMultivariate.trAppOption.UNLOCK_ALL_CUE_REWARDS) == trMultivariate.trAppOptionValue.YES) {
        return true;
      }
    }
    
    if (trDataManager.Instance.IsInNormalMissionMode) {
      return true;
    }
    
    bool isAvailable = true;
    if (lockedRewardDurables.ContainsKey(category) && lockedRewardDurables[category].Contains(id)) {
      isAvailable = false;
    }   
    return isAvailable;
  }

  public trReward GetRewardForIQPoints(int points){
    trReward potentialReward = null;
    foreach(trReward reward in availableRewards.Values){
      if (reward.IQPointsRequired <= points) {
        if ((potentialReward == null) || (!reward.Equals(potentialReward) && (reward.IQPointsRequired > potentialReward.IQPointsRequired))) {
          potentialReward = reward;
        }
      }
    }
    return potentialReward;
  }

  private void markRewardAsGrant(trReward reward){
    if (!IsRewardUnlocked(reward)){
      receivedRewards.Add(reward.UUID);
      unlockDurablesForReward(reward);
      saveReceivedRewards();
    }
  }

  private trReward getAvailableRewardById(string rewardId){
    trReward result = null;
    if ((rewardId != null) && availableRewards.ContainsKey(rewardId)){
      result = availableRewards[rewardId];
    }
    return result;
  }

  private void unlockDurablesForReward(trReward reward){
    if (reward != null) {
      foreach(trRewardDurable durable in reward.Durables){
        lockedRewardDurables[durable.Category].Remove(durable.Payload);
        //Debug.Log("unlocked " + durable.Category.ToString() + " for id: " + durable.Payload);
      }
    }
    else {
      WWLog.logError("null reward");
    }
  }

  private void loadAllAvailableRewards(){
    TextAsset rewardsList = Resources.Load (RewardsListPath, typeof(TextAsset)) as TextAsset;
    JSONNode js = JSON.Parse(rewardsList.text);
    foreach(JSONNode node in js.AsArray){
      trReward reward = trFactory.FromJson<trReward>(node);
      // get all the durables in the cache
      foreach(trRewardDurable durable in reward.Durables){
        HashSet<string> lockedDurableIds = (lockedRewardDurables.ContainsKey(durable.Category)) ? lockedRewardDurables[durable.Category] : new HashSet<string>();        
        lockedDurableIds.Add(durable.Payload);
        lockedRewardDurables[durable.Category] = lockedDurableIds;
      }
      availableRewards[reward.UUID] = reward;

    }
  }

  private void loadReceivedRewards(){
    string data = wwDataSaveLoadManager.Instance.Load(receivedRewardsFileName);
    if (data != null){
      JSONNode node = JSON.Parse(data);
      foreach(JSONNode rewardId in node.AsArray){
        trReward reward = getAvailableRewardById(rewardId.Value);
        if (reward != null){
          receivedRewards.Add(reward.UUID);
          unlockDurablesForReward(reward);   
        }
      }
    }
    WWLog.logDebug("loaded granted rewards from: " + receivedRewardsFileName);
  }

  public void GrantAllRewardsForIQPoints(int points){
    foreach(trReward reward in availableRewards.Values){
      if ((reward.IQPointsRequired <= points) && !receivedRewards.Contains(reward.UUID)){
        receivedRewards.Add(reward.UUID);
        unlockDurablesForReward(reward);
      }
    }
    saveReceivedRewards();
  }

  void lockAllDurables(){
    lockedRewardDurables.Clear();
    foreach(trReward reward in availableRewards.Values){
      foreach(trRewardDurable durable in reward.Durables){
        HashSet<string> lockedDurableIds = (lockedRewardDurables.ContainsKey(durable.Category)) ? lockedRewardDurables[durable.Category] : new HashSet<string>();        
        lockedDurableIds.Add(durable.Payload);
        lockedRewardDurables[durable.Category] = lockedDurableIds;
      }
    }

  }

  
  public void clearAllRewards() {
    receivedRewards.Clear();
    saveReceivedRewards();
    lockAllDurables();
  }

  private void saveReceivedRewards(){
    JSONArray json = new JSONArray();
    foreach(string rewardId in receivedRewards){
      json.Add(rewardId);
    }
    wwDataSaveLoadManager.Instance.Save(json.ToString(), receivedRewardsFileName);
  }
}
