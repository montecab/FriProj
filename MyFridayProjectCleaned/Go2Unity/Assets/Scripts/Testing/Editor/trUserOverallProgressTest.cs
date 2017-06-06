using UnityEngine;
using System.Collections;
using NUnit.Framework;
using Turing;
using WW.SaveLoad;

[TestFixture]
public class trUserOverallProgressTest{

  private trUserOverallProgress _progress;
  private trMissionManager _missionManager;
  private const string NO_SUCH_MISSION = "No such mission";

  [SetUp]
  public void SetUp(){
    trDataManager.Instance.Init();
    _missionManager = new trMissionManager();
    _missionManager.Load();
    _progress = _missionManager.UserOverallProgress;
    _progress.Clear();
    _progress.Save();
  }

  [Test]
  public void InitializationTest(){
    Assert.AreEqual(0, _progress.IQPoints);
    Assert.False(_progress.UserHasVisitedFreeplay);
    Assert.False(_progress.UserHasVisitedCommunity);
    Assert.False(_progress.IsCommunityUnlocked);
    Assert.False(_progress.IsFreePlayUnlocked);
    Assert.False(_progress.IsMissionCurrentlyInProgress(NO_SUCH_MISSION));
    Assert.False(_progress.IsMissionStarted(NO_SUCH_MISSION));
    Assert.False(_progress.IsMissionCompleted(NO_SUCH_MISSION));
    Assert.False(_progress.IsMissionRecentlySolved(NO_SUCH_MISSION));
  }

  [Test]
  public void UnlockDashFreeplayTest(){
    const string uuid = trUserOverallProgress.UNLOCK_FREEPLAY_MISSION_UUID_DASH;
    _progress.LoadUserState(uuid);
    Assert.False(_progress.IsFreePlayUnlocked);
    _progress.CompleteMission(uuid);
    Assert.True(_progress.IsFreePlayUnlocked);
  }

  [Test]
  public void UnlockDotFreeplayTest(){
    const string uuid = trUserOverallProgress.UNLOCK_FREEPLAY_MISSION_UUID_DOT;
    _progress.LoadUserState(uuid);
    Assert.False(_progress.IsFreePlayUnlocked);
    _progress.CompleteMission(uuid);
    Assert.True(_progress.IsFreePlayUnlocked);
  }

  [Test]
  public void UnlockDashCommunityTest(){
    const string uuid = trUserOverallProgress.UNLOCK_COMMUNITY_MISSION_UUID_DASH;
    _progress.LoadUserState(uuid);
    Assert.False(_progress.IsCommunityUnlocked);
    _progress.CompleteMission(uuid);
    Assert.True(_progress.IsCommunityUnlocked);
  }

  [Test]
  public void UnlockDotCommunityTest(){
    const string uuid = trUserOverallProgress.UNLOCK_COMMUNITY_MISSION_UUID_DOT;
    _progress.LoadUserState(uuid);
    Assert.False(_progress.IsCommunityUnlocked);
    _progress.CompleteMission(uuid);
    Assert.True(_progress.IsCommunityUnlocked);
  }

  [Test]
  public void MissionCompleteTest(){
    const string uuid = trUserOverallProgress.UNLOCK_COMMUNITY_MISSION_UUID_DOT;
    _missionManager.LoadMission(uuid);
    Assert.False(_progress.IsCurMissionCompleted);
    Assert.False(_progress.IsMissionCompleted(uuid));
    Assert.False(_progress.IsCurMissionCompletedOnce);
    Assert.False(_progress.IsMissionCompletedOnce(uuid));
    _progress.CompleteMission(uuid);
    Assert.True(_progress.IsCurMissionCompleted);
    Assert.True(_progress.IsMissionCompleted(uuid));
    Assert.True(_progress.IsCurMissionCompletedOnce);
    Assert.True(_progress.IsMissionCompletedOnce(uuid));

    const string uuid2 = trUserOverallProgress.UNLOCK_FREEPLAY_MISSION_UUID_DASH;
    _missionManager.LoadMission(uuid2);
    Assert.False(_progress.IsCurMissionCompleted);
    Assert.False(_progress.IsMissionCompleted(uuid2));
    Assert.False(_progress.IsCurMissionCompletedOnce);
    Assert.False(_progress.IsMissionCompletedOnce(uuid2));
  }

  [Test]
  public void LoadUserStateTest(){
    const string uuid = trUserOverallProgress.UNLOCK_COMMUNITY_MISSION_UUID_DOT;
    _missionManager.LoadMission(uuid);
    Assert.IsNotNull(_progress.CurMissionInfo);
    Assert.AreEqual(uuid, _progress.CurMissionInfo.MissionUUID);
  }

  [Test]
  public void MissionUnlockTest(){
    const string uuid = trUserOverallProgress.UNLOCK_COMMUNITY_MISSION_UUID_DOT;
    _missionManager.LoadMission(uuid);
    _progress.UUIDToProgressDic[uuid].UnlockTimeUTC = System.DateTime.UtcNow.AddSeconds(3600);
    Assert.False(_progress.IsMissionUnlocked(uuid));
    Assert.True(_progress.IsMissionTimerStart(uuid));
    _progress.UUIDToProgressDic[uuid].UnlockTimeUTC = System.DateTime.UtcNow.AddSeconds(-1);
    Assert.True(_progress.IsMissionUnlocked(uuid));
  }

  [Test]
  public void MissionStartedTest(){
    const string uuid = trUserOverallProgress.UNLOCK_COMMUNITY_MISSION_UUID_DOT;
    Assert.False(_progress.IsMissionStarted(uuid));
    _missionManager.LoadMission(uuid);
    Assert.True(_progress.IsMissionStarted(uuid));
  }

  [Test]
  public void RestartCurMissionTest(){
    const string uuid = trUserOverallProgress.UNLOCK_COMMUNITY_MISSION_UUID_DOT;
    _missionManager.LoadMission(uuid);
    Assert.False(_progress.IsCurMissionCompleted);
    _progress.CompleteMission(uuid);
    Assert.True(_progress.IsCurMissionCompleted);
    _progress.RestartCurMission();
    Assert.False(_progress.IsCurMissionCompleted);
  }

  [Test]
  public void MissionInProgressTest(){
    const string uuid = trUserOverallProgress.UNLOCK_COMMUNITY_MISSION_UUID_DOT;
    _missionManager.LoadMission(uuid);
    Assert.True(_progress.IsMissionCurrentlyInProgress(uuid));
    _progress.CompleteMission(uuid);
    Assert.False(_progress.IsMissionCurrentlyInProgress(uuid));
  }

  [Test]
  public void MigrationTest(){
    const string uuid = trUserOverallProgress.UNLOCK_COMMUNITY_MISSION_UUID_DOT;
    _missionManager.LoadMission(uuid);
    const string uuid2 = trUserOverallProgress.UNLOCK_FREEPLAY_MISSION_UUID_DASH;
    _missionManager.LoadMission(uuid2);
    Assert.True(_progress.IsMissionCurrentlyInProgress(uuid));
    Assert.True(_progress.IsMissionCurrentlyInProgress(uuid2));
    PlayerPrefs.SetString(BuildInfo.cTOK_APP_VERSION, "0.0.0");
    _progress.MigrateToCurrentAppVersion();
    Assert.False(_progress.IsMissionCurrentlyInProgress(uuid));
    Assert.False(_progress.IsMissionCurrentlyInProgress(uuid2));
  }
}
