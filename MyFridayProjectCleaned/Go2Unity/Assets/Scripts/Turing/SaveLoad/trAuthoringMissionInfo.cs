using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using WW.SaveLoad;
using WW.SimpleJSON;
using WW;

namespace Turing{
  public class trMissionFileInfo{
    public string MissionName; // This will also be the file name, we think there will not be two missions that have the same name. This helps designer to naviagete the files.
    public string UUID;
    public string IconName;
    public trMissionType Type = trMissionType.DASH;
    public int MaxIQPoints = 0; //the max iq points you can get in this mission, only used for going into a mission from authoring panel, we need to recalculate the iq points 

    public trMissionFileInfo(){}
    public trMissionFileInfo(string missionName, string uuid, string iconName, trMissionType type){
      MissionName = missionName;
      UUID = uuid;
      IconName = iconName;
      Type = type;
    }

    public void Update(string missionName, string iconName, int iqPoints, trMissionType type){
      MissionName = missionName;
      IconName = iconName;
      MaxIQPoints = iqPoints;
      Type = type;
    }

    public JSONClass ToJson() {
      JSONClass jsc = new JSONClass();
      jsc[TOKENS.USER_FACING_NAME] = MissionName; 
      jsc[TOKENS.ID] = UUID;
      jsc[TOKENS.ICON_NAME] = IconName; 
      jsc[TOKENS.IQ_POINTS].AsInt = MaxIQPoints;
      jsc[TOKENS.TYPE].AsInt = (int) Type;
    
      return jsc;
    }
    
    public  static trMissionFileInfo FromJson(JSONNode jsc) {
      trMissionFileInfo ret = new trMissionFileInfo();
      ret.MissionName = jsc[TOKENS.USER_FACING_NAME].Value;
      ret.UUID = jsc[TOKENS.ID].Value;
      ret.IconName = jsc[TOKENS.ICON_NAME].Value;
      if(jsc[TOKENS.IQ_POINTS] != null){
        ret.MaxIQPoints = jsc[TOKENS.IQ_POINTS].AsInt;
      }
      if(jsc[TOKENS.TYPE] != null){
        ret.Type = (trMissionType)(jsc[TOKENS.TYPE].AsInt);
      }
      return ret;
    }
  }

  public class trMapArea{
    public string UserFacingName = "";
    public string IconName = "";
    public trMapAreaType Area;

    public trMapArea(string name, string icon, trMapAreaType area){
      UserFacingName = name;
      IconName = icon;
      Area = area;
    }
  }

  public enum trMapAreaType{
    WONDER_WORKSHOP = 0,
    MOUNTAIN = 1,
    LAGOON = 2,
    DARK_FOREST = 3,
    JUNGLE = 4,
    DESERT = 5,
    AFRICAN_GRASSLANDS = 6,
    ARCTIC_WILDERNESS = 7,
    VOLCANIC_PLANE = 8,
    CITY_AT_NIGHT = 9,
    SPACE = 10,
  }

  public class trAuthoringMissionInfo {

    public Dictionary<string, trMissionFileInfo> UUIDToMissionDic = new Dictionary<string, trMissionFileInfo>();

    public Dictionary<string, trMission> MissionDic = new Dictionary<string, trMission>(); // Should only be used in Authoring mode

    private static Dictionary<trMapAreaType, trMapArea> mapAreas = null;
    
    public static Dictionary<trMapAreaType, trMapArea>  MapAreas{
      get{
        if(mapAreas == null){
          mapAreas = new Dictionary<trMapAreaType, trMapArea>();
          mapAreas.Add(trMapAreaType.WONDER_WORKSHOP      , new trMapArea("@!@Wonder Workshop@!@"    , "WW"         , trMapAreaType.WONDER_WORKSHOP));
          mapAreas.Add(trMapAreaType.MOUNTAIN             , new trMapArea("@!@Dragon Reach@!@"       , "Mountain"   , trMapAreaType.MOUNTAIN));
          mapAreas.Add(trMapAreaType.LAGOON               , new trMapArea("@!@Firefly Lagoon@!@"     , "Lagoon"     , trMapAreaType.LAGOON));
          mapAreas.Add(trMapAreaType.DARK_FOREST          , new trMapArea("@!@Castle Creepenstein@!@", "DarkForest" ,trMapAreaType.DARK_FOREST));
          mapAreas.Add(trMapAreaType.JUNGLE               , new trMapArea("@!@Forgotten Jungle@!@"   , "Jungle"     , trMapAreaType.JUNGLE));
          mapAreas.Add(trMapAreaType.DESERT               , new trMapArea("@!@Dry Gulch Desert@!@"   , "Desert"     , trMapAreaType.DESERT));
          mapAreas.Add(trMapAreaType.AFRICAN_GRASSLANDS   , new trMapArea("@!@Big Cat Canyon@!@"     , "Grassland"  , trMapAreaType.AFRICAN_GRASSLANDS));
          mapAreas.Add(trMapAreaType.ARCTIC_WILDERNESS    , new trMapArea("@!@Yeti Pass@!@"          , "Arctic Snow", trMapAreaType.ARCTIC_WILDERNESS));
          mapAreas.Add(trMapAreaType.VOLCANIC_PLANE       , new trMapArea("@!@Mount Ashburn@!@"      , "Volcano"    , trMapAreaType.VOLCANIC_PLANE));
          mapAreas.Add(trMapAreaType.CITY_AT_NIGHT        , new trMapArea("@!@Kong City@!@"          , "City"       , trMapAreaType.CITY_AT_NIGHT));
          mapAreas.Add(trMapAreaType.SPACE                , new trMapArea("@!@Galaxy Lake@!@"        , "space"      , trMapAreaType.SPACE));
        }
        return mapAreas;
      }
    }

    public trProgram MissionMap = trProgram.NewProgram();

    public bool IsLoadUserFolder = true;

    public MissionEditState EditState = MissionEditState.NORMAL;

    private string path = "";
    public string Path {
      get{       
        return path;
      }
      set{       
        path = value;
        PlayerPrefs.SetString(TOKENS.PATH, path);
      }
    }

    public void LoadPath(){
      path = PlayerPrefs.GetString(TOKENS.PATH); 
    }

    private string missionListPath{
      get{
        return Path + "/" + missionListFileName + ".json";
      }
    }

    public const string missionListFileName = "trMissionListInfo";

    public bool IsPathValid(){
      return Directory.Exists(Path) && File.Exists(missionListPath);
    }

    public trMission CurMission;
    public trPuzzle CurPuzzle;
    public trHint CurHint;
    
    private string lastSavedMissionName = null;
    private System.DateTime lastSavedMissionTime = System.DateTime.MinValue;

    public string LastSavedMissionInfo{
      get{
        if((lastSavedMissionName == null) || (lastSavedMissionTime == System.DateTime.MinValue)){
          return "No Challenge Saved Yet";
        }
        return lastSavedMissionName + " saved at " + lastSavedMissionTime.ToString();
      }
    }

    public JSONClass ToJson() {
      JSONClass jsc = new JSONClass();
      JSONArray jarray = new JSONArray();      
      foreach (string s in UUIDToMissionDic.Keys) {
        JSONClass js = new JSONClass();
        js[TOKENS.ID] = s;
        js[TOKENS.MISSION] = UUIDToMissionDic[s].ToJson();
        jarray.Add(js);
      }
      jsc[TOKENS.MISSIONS] =jarray;

      jsc[TOKENS.PROGRAM] = MissionMap.ToJson();
      return jsc;
    }

    public void FromJson(JSONClass jsc) {
      if(IsLoadUserFolder){
        foreach (JSONNode jsM in jsc[TOKENS.MISSIONS].AsArray) {
          UUIDToMissionDic.Add(jsM[TOKENS.ID], trMissionFileInfo.FromJson(jsM[TOKENS.MISSION]));
        }
      }
      MissionMap = trFactory.FromJson<trProgram>(jsc[TOKENS.PROGRAM]);
    }

    public void UpdateCurMission(trMissionFileInfo info){
      if(!UUIDToMissionDic.ContainsValue(info)){
        WWLog.logError("Mission file info doesn't exist: " + info.ToString());
        return;
      }

      string uuid  = info.UUID;
      
      if (!MissionDic.ContainsKey(uuid)) {
        WWLog.logError("MissionDic missing key: " + uuid);
        return;
      }
      
      CurMission = MissionDic[uuid];
    }

    public trMissionFileInfo CreateMission(){
      trMission ret = new trMission();     
      trMissionFileInfo  newFile = new trMissionFileInfo(ret.UserFacingName, ret.UUID, ret.SpriteName, ret.Type);
      UUIDToMissionDic.Add(ret.UUID, newFile);
      CurMission = ret;
      Save();
      return newFile;
    }


    public void Load(){
      LoadPath();
#if UNITY_IPHONE||UNITY_ANDROID
      IsLoadUserFolder = true;
#endif
      if(IsLoadUserFolder){
        LoadUserFacingFolder();
      }else{
        LoadAuthoringFolder();
      }     
    }

    public trMissionFileInfo GetMissionFile(string uuid ){
      if(UUIDToMissionDic.ContainsKey(uuid)){
        return UUIDToMissionDic[uuid];
      }else{
        WWLog.logError("Mission file doesn't exist. UUID: " + uuid);
        return null;
      }
    }
    
    public void LoadAuthoringFolder(){     

      if(!IsPathValid()){
        return;
      }

      DirectoryInfo dir = new DirectoryInfo(Path);
      FileInfo[] files = dir.GetFiles("*.json");

      UUIDToMissionDic.Clear();
      MissionDic.Clear();

      foreach(FileInfo f in files){
        if(f.Name == (missionListFileName + ".json")){
          continue;
        }
        trMission mission = LoadMissionFile(f.Name);
        
        trMissionFileInfo newFile = new trMissionFileInfo();
        string tmp =  mission.UserFacingName + ".json";
        if(f.Name != tmp){
          WWLog.logError("Something is wrong, file name should always equals to mission name. filename = " + f.Name + "  mission name = " + tmp);
        }
        newFile.MissionName = mission.UserFacingName;
        newFile.UUID = mission.UUID;
        newFile.IconName = mission.SpriteName;
        newFile.MaxIQPoints = mission.MaxIQPoints;
        newFile.Type = mission.Type;
        if(!UUIDToMissionDic.ContainsKey(mission.UUID)){
          UUIDToMissionDic.Add(mission.UUID, newFile);
        }
        string uuid = mission.UUID;
        if(!MissionDic.ContainsKey(uuid)){
          MissionDic.Add(uuid, mission);
        }  
        else{
          mission.UUID = wwUID.getUID();
          MissionDic.Add(mission.UUID, mission);
          SaveMission(mission);
          WWLog.logError("Mission " + mission.UserFacingName + " has the same uuid with " + UUIDToMissionDic[uuid].MissionName +". Fixing. If you don't understand what this means, find Leisen.");
        }
      } 

      // order matters, this needs to be last
      string data = wwDataSaveLoadManager.Instance.Load(missionListPath);
      if(data != null){
        JSONClass jsc = (JSON.Parse(data)).AsObject;
        FromJson(jsc);
      }
    }

    public void LoadMission(string uuid){
      if(CurMission != null && uuid == CurMission.UUID){
        return;
      }
      CurMission = LoadMissionFromUUID(uuid);
    }
    
    public trMission LoadMissionFromUUID(string uuid) {
      if(!UUIDToMissionDic.ContainsKey(uuid)){
        WWLog.logError("Cannot find mission with uuid: " + uuid);
        return null;
      }
      if(!IsLoadUserFolder){
        if(MissionDic.ContainsKey(uuid)){
          return MissionDic[uuid];
        }
      }
      string fileName = UUIDToMissionDic[uuid].MissionName + ".json";
      return LoadMissionFile(fileName);
    }


    public trMission LoadMissionFile(string fileName){
      string p = Path + "/" + fileName;
      string data = "";
      if(IsLoadUserFolder){
        string finalFilename = "TuringProto/Missions/" + fileName.Substring(0, fileName.Length - 5);
        TextAsset info = Resources.Load (finalFilename, typeof(TextAsset)) as TextAsset;
        if (info == null) {
          WWLog.logError("could not load file: " + fileName + "    -  " + finalFilename);
        }
        data = info.text;
      }
      else{
        data = wwDataSaveLoadManager.Instance.Load(p);
      }
     
      if(data == null){
        WWLog.logInfo("The file " + fileName +" doesn't exist.");
        return null;
      }
      else{
        JSONNode js = JSON.Parse(data);
        trMission result = trMission.FromJson(js.AsObject);

        if(result.UUID == null){
          result.UUID = wwUID.getUID();
          SaveMission(result);
        }
        return result;
      }    
    }

    public void LoadUserFacingFolder(){
      UUIDToMissionDic.Clear();
      MissionDic.Clear();
      TextAsset Appinfo = Resources.Load ("TuringProto/Missions/" + missionListFileName, typeof(TextAsset)) as TextAsset;
      if(Appinfo != null){
        JSONClass jsc = (JSON.Parse(Appinfo.text)).AsObject;
        FromJson(jsc);
      }

    }

    public void Save(){

      if(!IsPathValid()){
        WWLog.logError(string.Format("Directory[{0}] doesn't exist. Reset path to make it work. ", path));
        return;
      }
      SaveCurMission();

      string data = this.ToJson().ToString("");
      wwDataSaveLoadManager.Instance.Save(data, missionListPath);
    }

    void updateFileInfo(){
      string uuid = CurMission.UUID;
      trMissionFileInfo info = UUIDToMissionDic[uuid];
      if(info.MissionName != CurMission.UserFacingName){
        wwDataSaveLoadManager.Instance.Clear( getRealPath(info.MissionName));
      }
      info.Update(CurMission.UserFacingName, CurMission.SpriteName, CurMission.MaxIQPoints, CurMission.Type);
    }

    public void DeleteMission(trMissionFileInfo mission){
      MissionDic.Remove(mission.UUID);
      UUIDToMissionDic.Remove(mission.UUID);
      wwDataSaveLoadManager.Instance.Clear(getRealPath(mission.MissionName));
      Save();
    }

    public void SaveCurMission(){
      if(CurMission == null){
        return;
      }
     SaveMission(CurMission);
     updateFileInfo();
    }

    public void SaveMission(trMission mission){
      if(!MissionDic.ContainsKey(mission.UUID)){
        MissionDic.Add(mission.UUID, mission);
      }
      string content = mission.ToJson().ToString("");
      wwDataSaveLoadManager.Instance.Save(content,getRealPath(mission.UserFacingName));

      lastSavedMissionName = mission.UserFacingName;
      lastSavedMissionTime = System.DateTime.Now;
    }
    
    string getRealPath(string fileName){
      return Path + "/" + fileName + ".json";
    }

    // returns a dictionary where the keys are all the MapAreas,
    // and each value is a list of the missions in that map area.
    public Dictionary<trMapArea, List<trMissionFileInfo>> buildAreaList() {
      Dictionary<trMapArea, List<trMissionFileInfo>> ret = new Dictionary<trMapArea, List<trMissionFileInfo>>();
      _buildAreaList_Recursive(ret, null);
      return ret;
    }
    
    void _buildAreaList_Recursive(Dictionary<trMapArea, List<trMissionFileInfo>> areaList, trState state) {
      if (state == null) {
        trProgram program = this.MissionMap;
        state = program.StateStart;
        if (state == null) {
          WWLog.logError("MissionMap program has no start state.");
          return;
        }
      }
      
      if (state.Behavior.IsMissionBehavior) {
        trMissionFileInfo missionInfo = state.Behavior.MissionFileInfo;
        if (missionInfo.Type.GetRobotType() == trDataManager.Instance.CurrentRobotTypeSelected) {
          
          trMapAreaType trMAT = (trMapAreaType)state.BehaviorParameterValue;
          trMapArea trMA = trAuthoringMissionInfo.MapAreas[trMAT];
          
          if (!areaList.ContainsKey(trMA)) {
            areaList[trMA] = new List<trMissionFileInfo>();
          }
          List<trMissionFileInfo> missionList = areaList[trMA];
          if (missionList.Contains(missionInfo)) {
            WWLog.logError("unexpected: mission appears twice in mission tree");
            return;
          }
          
          missionList.Add(missionInfo);
        }
      }
      
      // recurse
      foreach(trTransition transition in state.OutgoingTransitions){
        if (transition.StateTarget == null) {
          WWLog.logError("transition has null target. " + state.UserFacingName);
        }
        else {
          _buildAreaList_Recursive(areaList, transition.StateTarget);
        }
      }
    }
    
    // returns true iff this mission UUID has zero outgoing mission connections
    public bool isLeafNodeMission(string UUID) {
      bool ret = false;
      foreach (trMissionFileInfo trMFI in LeafNodeMissions) {
        if (trMFI.UUID == UUID) {
          ret = true;
          break;
        }
      }
      
      return ret;
    }
    
    private HashSet<trMissionFileInfo> leafNodeMissions = null;
    // These are all the missions (either robot) which have no missions leading out of them.
    public HashSet<trMissionFileInfo> LeafNodeMissions {
      get {
        if (leafNodeMissions == null) {
          leafNodeMissions = new HashSet<trMissionFileInfo>();
          _buildLeafNodeMissions_Recursive(leafNodeMissions, null);
          string s = "";
          string delim = "";
          foreach (trMissionFileInfo trMFI in leafNodeMissions) {
            s += delim + "\"" + trMFI.MissionName + "\"";
            delim = ", ";
          }
          WWLog.logInfo("leaf-node missions: " + s);
        }
        return leafNodeMissions;
      }
    }
    
    private void _buildLeafNodeMissions_Recursive(HashSet<trMissionFileInfo> missions, trState state) {
      if (state == null) {
        if (state == null) {
          trProgram program = this.MissionMap;
          state = program.StateStart;
          if (state == null) {
            WWLog.logError("MissionMap program has no start state.");
            return;
          }
        }
      }
      
      if (state.Behavior.IsMissionBehavior) {
        if (state.OutgoingTransitions.Count == 0) {
          // it's a leaf node.
          missions.Add(state.Behavior.MissionFileInfo);
        }
      }
      
      // recurse
      foreach(trTransition transition in state.OutgoingTransitions){
        if (transition.StateTarget == null) {
          WWLog.logError("transition has null target. " + state.UserFacingName);
        }
        else {
          _buildLeafNodeMissions_Recursive(missions, transition.StateTarget);
        }
      }
    }
      
  }
  

  public class trUserProgress{
    public string MissionUUID = "";
    public bool IsCompletedOnce = false;
    public bool IsCompleted = false;
    public bool StartReported = false;
    public float Progress = 0;
    public System.DateTime UnlockTimeUTC = System.DateTime.UtcNow.AddSeconds(-1);

    public static int CurrentVersion = 0;

    public int Version = -1;

    public bool IsUnlocked{
      get{
        return UnlockTimeUTC.CompareTo(System.DateTime.UtcNow) <= 0;
      }
    }

    public bool IsUnlockNow{
      get{
        System.TimeSpan ts = System.DateTime.UtcNow - UnlockTimeUTC;
        if(ts.TotalSeconds<Time.deltaTime && ts.TotalSeconds>=0){
          return true;
        }
        return false;
      }
    }

    public trUserProgress(){}

    public trUserProgress(string uuid, bool isComplete, bool isCompleteOnce){
      MissionUUID = uuid;
      IsCompletedOnce = isCompleteOnce;
      IsCompleted = isComplete;
    }

    public JSONClass ToJson(){
      JSONClass js = new JSONClass();
      js[TOKENS.ID] = MissionUUID;
      js[TOKENS.COMPLETE_ONCE].AsBool = IsCompletedOnce;
      js[TOKENS.COMPLETE].AsBool = IsCompleted;
      js[TOKENS.ACTIVATION_TIME] = UnlockTimeUTC.ToBinary().ToString();
      js[TOKENS.VERSION].AsInt = Version;
      js[TOKENS.PROGRESS].AsFloat = Progress;
      js[TOKENS.START_REPORTED].AsBool = StartReported;
      return js;
    }

    public static trUserProgress FromJson(JSONClass js){
      trUserProgress ret = new trUserProgress();
      ret.MissionUUID = js[TOKENS.ID];
      ret.IsCompletedOnce = js[TOKENS.COMPLETE_ONCE].AsBool;
     
      if(js[TOKENS.VERSION] != null){
        ret.Version = js[TOKENS.VERSION].AsInt ;
      }

      if(js[TOKENS.COMPLETE] != null){
        ret.IsCompleted = js[TOKENS.COMPLETE].AsBool;
      }

      if(js[TOKENS.ACTIVATION_TIME] != null){
        long temp = System.Convert.ToInt64(js[TOKENS.ACTIVATION_TIME]);
        ret.UnlockTimeUTC = System.DateTime.FromBinary(temp);
      }

      if(js[TOKENS.PROGRESS] != null){
        ret.Progress = js[TOKENS.PROGRESS].AsFloat;
      }

      if(js[TOKENS.START_REPORTED] != null){
        ret.StartReported = js[TOKENS.START_REPORTED].AsBool;
      }
      else{
        ret.StartReported = true;
      }

      fixVersion(ret);     
      return ret;
    }

    static void fixVersion(trUserProgress progress){
      if(progress.Version > CurrentVersion){
        WWLog.logError("Something is really wrong. This user progress file has a version that is higher than current version");
        return;
      }
      if(progress.Version == CurrentVersion){
        return;
      }

      if(progress.Version< 0){
        progress.IsCompleted = progress.IsCompletedOnce;
      }

      progress.Version = CurrentVersion;
    }
  }

  public class trUserOverallProgress{

    public Dictionary<string, trUserProgress> UUIDToProgressDic = new Dictionary<string, trUserProgress>();
    public int IQPoints = 0;
    public string RecentPlayedMissionUUID = "";
    public string RecentlySolvedMissionUUID = "";
    private bool userVisitedFreeplay = false;
    public bool UserHasVisitedFreeplay {
      get{
        return IsFreePlayUnlocked && userVisitedFreeplay;
      }
      set{
        userVisitedFreeplay = value;
      }
    }
    private bool userVisitedCommunity = false;
    public bool UserHasVisitedCommunity {
      get{
        return IsCommunityUnlocked && userVisitedCommunity;
      }
      set{
        userVisitedCommunity = value;
      }
    }

    public const string UNLOCK_COMMUNITY_MISSION_UUID_DASH = "39327952_2"; //new press to impress
    public const string UNLOCK_COMMUNITY_MISSION_UUID_DOT = "37585550_2"; //new button beeps
    public const string UNLOCK_FREEPLAY_MISSION_UUID_DASH = "39297011_2"; //new go bot
    public const string UNLOCK_FREEPLAY_MISSION_UUID_DOT = "37415937_2"; //new show and tell

    private bool isCommunityUnlocked = false;
    public bool IsCommunityUnlocked{
      get{
        if(!isCommunityUnlocked){
          isCommunityUnlocked = 
          (
            (UUIDToProgressDic.ContainsKey(UNLOCK_COMMUNITY_MISSION_UUID_DASH) && 
              UUIDToProgressDic[UNLOCK_COMMUNITY_MISSION_UUID_DASH].IsCompletedOnce) ||
            (UUIDToProgressDic.ContainsKey(UNLOCK_COMMUNITY_MISSION_UUID_DOT) &&
              UUIDToProgressDic[UNLOCK_COMMUNITY_MISSION_UUID_DOT].IsCompletedOnce)
          );
        }
        return isCommunityUnlocked;
      }
      set{
        isCommunityUnlocked = value;
      }
    }

    private bool isFreePlayUnlocked = false;
    public bool IsFreePlayUnlocked{
      get{
        if(!isFreePlayUnlocked){
          isFreePlayUnlocked = 
          (
            (UUIDToProgressDic.ContainsKey(UNLOCK_FREEPLAY_MISSION_UUID_DASH) && 
              UUIDToProgressDic[UNLOCK_FREEPLAY_MISSION_UUID_DASH].IsCompletedOnce) ||
            (UUIDToProgressDic.ContainsKey(UNLOCK_FREEPLAY_MISSION_UUID_DOT) && 
              UUIDToProgressDic[UNLOCK_FREEPLAY_MISSION_UUID_DOT].IsCompletedOnce)
          );
        }
        return isFreePlayUnlocked;
      }
      set{
        isFreePlayUnlocked = value;
      }
    } 

    public trUserMissionInfo CurMissionInfo;

    public bool IsCurMissionCompleted{
      get{
        if( CurMissionInfo == null){
          // this happens all the time in FreePlay mode.
          //        WWLog.logWarn("Trying to get if the mission is completed while the current mission doesn't exist");
          return false;
        }
        return IsMissionCompleted(CurMissionInfo.MissionUUID);
      }
    }

    public int IsCurMissionCompleteOnceInt{
      get{
        bool completed = IsCurMissionCompletedOnce;
        int ret = completed? 1:0;
        return ret;
      }
     
    }

    public bool IsCurMissionCompletedOnce{
      get{
        if( CurMissionInfo == null){
          return false;
        }
        return IsMissionCompletedOnce(CurMissionInfo.MissionUUID);
      }
    }

    public void UpdateMissionProgress(string uuid, float progress){
      if(!UUIDToProgressDic.ContainsKey(uuid)){
        WWLog.logError("Cannot find user progress for mission " + uuid);
        return;
      }
      UUIDToProgressDic[uuid].Progress = progress;
    }

    public void RestartCurMission(){
      UUIDToProgressDic[CurMissionInfo.MissionUUID].IsCompleted = false;
      UUIDToProgressDic[CurMissionInfo.MissionUUID].StartReported = false;
      CurMissionInfo.IsShownMissionInfo = false;
      CurMissionInfo.HintIndex = 0;
      CurMissionInfo.PuzzleIndex = 0;
      Save();
    }

    public bool IsMissionCurrentlyInProgress(string uuid) {
      return IsMissionStarted(uuid) && !IsMissionCompleted(uuid);
    }

    public bool IsMissionUnlocked(string uuid){
      if(UUIDToProgressDic.ContainsKey(uuid)){
        return UUIDToProgressDic[uuid].IsUnlocked;
      }
      return false;
    }

    public bool IsMissionTimerStart(string uuid){
      if(UUIDToProgressDic.ContainsKey(uuid)){
        if(!UUIDToProgressDic[uuid].IsUnlocked){
          return true;
        }
      }
      return false;
    }

    public bool IsMissionStarted(string uuid){
      if(IsMissionCompletedOnce(uuid)){
        return true;
      }

      string path = getUserStatePath(uuid);
      return File.Exists(path);
    }

    // returns true iff a "START" event should be reported.
    // note this might be either a first-time START or a replay.
    public bool SetMissionStartReported(string uuid, bool value){
      if(!UUIDToProgressDic.ContainsKey(uuid)){
        return false;
      }

      trUserProgress trUP    = UUIDToProgressDic[uuid];
      bool           prevVal = trUP.StartReported;

      trUP.StartReported = value;

      if (prevVal != value) {
        Save();
      }

      return (value && !prevVal);
    }

    public bool IsMissionCompleted(string uuid){
      if(!UUIDToProgressDic.ContainsKey(uuid)){
        return false;
      }
      return UUIDToProgressDic[uuid].IsCompleted;
    }

    public bool IsMissionCompletedOnce(string uuid){
      if(!UUIDToProgressDic.ContainsKey(uuid)){
        return false;
      }
      return UUIDToProgressDic[uuid].IsCompletedOnce;
    }
    
    public bool IsMissionRecentlySolved(string uuid) {
      if (string.IsNullOrEmpty(uuid)) {
        WWLog.logError("bad mission id.");
        return false;
      }
      else {
        return (RecentlySolvedMissionUUID == uuid);
      }
    }

    private string path;
    
    public void Init(){
      path = Application.persistentDataPath + "/UserOverallProgress.json";
    }

    public JSONClass ToJson() {
      JSONClass jsc = new JSONClass();
      JSONArray jarray = new JSONArray();      
      foreach (string s in UUIDToProgressDic.Keys) {
        JSONClass js = UUIDToProgressDic[s].ToJson();
        jarray.Add(js);
      }
      jsc[TOKENS.MISSIONS] =jarray;
      jsc[TOKENS.IQ_POINTS].AsInt = IQPoints;
      jsc[TOKENS.RECENT_PLAYED] = RecentPlayedMissionUUID;
      jsc[TOKENS.RECENTLY_SOLVED] = RecentlySolvedMissionUUID;
      jsc[TOKENS.FREEPLAY_UNLOCKED].AsBool = IsFreePlayUnlocked;
      jsc[TOKENS.FREEPLAY_VISITED].AsBool = UserHasVisitedFreeplay;
      jsc[TOKENS.COMMUNITY_UNLOCKED].AsBool = IsCommunityUnlocked;
      jsc[TOKENS.COMMUNITY_VISITED].AsBool = UserHasVisitedCommunity;
      return jsc;
    }
    
    public void FromJson(JSONNode jsc) {

      foreach (JSONClass jsM in jsc[TOKENS.MISSIONS].AsArray) {
        trUserProgress newProgress = trUserProgress.FromJson(jsM);
        UUIDToProgressDic.Add(newProgress.MissionUUID, newProgress);
      }
      
      IQPoints = jsc[TOKENS.IQ_POINTS] == null? 0: jsc[TOKENS.IQ_POINTS].AsInt;

      if(jsc[TOKENS.RECENT_PLAYED] != null){
        RecentPlayedMissionUUID = jsc[TOKENS.RECENT_PLAYED];
      }
      
      if(jsc[TOKENS.RECENTLY_SOLVED] != null){
        RecentlySolvedMissionUUID = jsc[TOKENS.RECENTLY_SOLVED];
      }
      
      if(jsc[TOKENS.FREEPLAY_UNLOCKED] != null){
        IsFreePlayUnlocked = jsc[TOKENS.FREEPLAY_UNLOCKED].AsBool;
      }

      if(jsc[TOKENS.FREEPLAY_VISITED] != null){
        UserHasVisitedFreeplay = jsc[TOKENS.FREEPLAY_VISITED].AsBool;
      }
      
      if(jsc[TOKENS.COMMUNITY_UNLOCKED] != null){
        IsCommunityUnlocked = jsc[TOKENS.COMMUNITY_UNLOCKED].AsBool;
      }

      if(jsc[TOKENS.COMMUNITY_VISITED] != null){
        UserHasVisitedCommunity = jsc[TOKENS.COMMUNITY_VISITED].AsBool;
      }
    }

    public void LoadUserState(string uuid){
      if(!UUIDToProgressDic.ContainsKey(uuid)){
        trUserProgress newProgress = new trUserProgress(uuid, false, false);
        UUIDToProgressDic.Add(uuid,newProgress);
        SaveOverallProgress();
      }

      if(CurMissionInfo != null && CurMissionInfo.MissionUUID == uuid){
        return;
      }
      
      trUserMissionInfo trUMI = loadUserMissionInfo(uuid);
      if (trUMI == null) {
        CurMissionInfo = null;
      }
      else {
        CurMissionInfo = trUMI;
        RecentPlayedMissionUUID = uuid;
        Save ();
      }
    }
    
    public static trUserMissionInfo loadUserMissionInfo(string uuid) {
      string statePath = getUserStatePath(uuid);
      if (string.IsNullOrEmpty(statePath)) {
        WWLog.logError("no user progress for mission: " + uuid);
        return null;
        
      }
      
      string data = wwDataSaveLoadManager.Instance.Load(statePath);
      if (data == null) {
        WWLog.logInfo("error loading user progress from mission: " + uuid + "  path: " + statePath);
        return null;
      }
      
      JSONNode js = JSON.Parse(data);
      return trUserMissionInfo.FromJson(js);
    }

    //only clear the program and the progress of this mission, will not modify if it's completed
    public void ClearUserMissionInfo(string uuid){
      if(CurMissionInfo != null && CurMissionInfo.MissionUUID == uuid){
       CurMissionInfo = null;
      }
      wwDataSaveLoadManager.Instance.Clear(getUserStatePath(uuid));
    }

    private static string getUserStatePath(string uuid){
      return Application.persistentDataPath + "/us_" + uuid + ".json";
    }

    public void CompleteMission(string uuid){
      if(!UUIDToProgressDic.ContainsKey(uuid)){
        WWLog.logError("The mission doesn't exist in user progress. adding it. However, this shouldn't happen.");
        trUserProgress newProgress = new trUserProgress(uuid, true, true);
        UUIDToProgressDic.Add(uuid, newProgress);
      }
     
      UUIDToProgressDic[uuid].IsCompletedOnce = true;
      UUIDToProgressDic[uuid].IsCompleted = true;
      RecentlySolvedMissionUUID = uuid;
      SaveOverallProgress();
    }

    public void Load(){
      Init ();
      string data = wwDataSaveLoadManager.Instance.Load(path);
      if(data != null){
        JSONNode js = JSON.Parse(data);
        FromJson(js);
      }
    }

    public void Save(){

      SaveOverallProgress();
      SaveCurMissionInfo();
    }

    public void SaveOverallProgress(){
      string content = this.ToJson().ToString("");
      wwDataSaveLoadManager.Instance.Save(content, path);
    }

    public void MigrateToCurrentAppVersion() {
      const bool force = !true; // debug only
      string currVersion = BuildInfo.AppVersion;
      string prevVersion = PlayerPrefs.GetString(BuildInfo.cTOK_APP_VERSION, "0.0.0");
      BuildInfo.VersionCompareResult vcr = BuildInfo.CompareAppVersions(prevVersion, currVersion);
      if (!force) {
        if (vcr == BuildInfo.VersionCompareResult.EQUAL) {
          // do not migrate
          return;
        }
        else if (vcr == BuildInfo.VersionCompareResult.GREATER_THAN) {
          WWLog.logWarn("current app version is older than previous! curr = " + currVersion + " prev = " + prevVersion);
          return;
        }
      }

      if (UUIDToProgressDic == null) {
        WWLog.logError("Progress dict is null");
        return;
      }

      // comparison was either invalid or "less than", so we migrate.
      new trTelemetryEvent(trTelemetryEventType.CHAL_MIGRATE, true)
        .add(trTelemetryParamType.VER_PREV, prevVersion)
        .add(trTelemetryParamType.VER_CURR, currVersion)
        .emit();

      PlayerPrefs.SetString(BuildInfo.cTOK_APP_VERSION, currVersion);

      // okay - actual migration here:

      foreach(trUserProgress trUP in UUIDToProgressDic.Values) {
        if (IsMissionCurrentlyInProgress(trUP.MissionUUID)) {
          resetMissionProgress(trUP.MissionUUID);
        }
      }
    }

    private void resetMissionProgress(string uuid) {
      WWLog.logInfo("resetting progress for mission " + uuid);
      wwDataSaveLoadManager.Instance.Clear(getUserStatePath(uuid));
    }

    public void Clear(){
      CurMissionInfo = null;
      foreach(trUserProgress progress in UUIDToProgressDic.Values){
        resetMissionProgress(progress.MissionUUID);
      }
      wwDataSaveLoadManager.Instance.Clear(path);
      
      UUIDToProgressDic.Clear();
      IQPoints = 0;
      RecentPlayedMissionUUID = "";
      RecentlySolvedMissionUUID = "";
      IsFreePlayUnlocked = false;
      UserHasVisitedFreeplay = false;
      IsCommunityUnlocked = false;
      UserHasVisitedCommunity = false;
      
      Load();
      trDataManager.Instance.MissionMng.Load();
    }

    public void SaveCurMissionInfo(){
      if(CurMissionInfo == null){
        return;
      }
      SaveMissionInfo(CurMissionInfo);
    }

    public void SaveMissionInfo(trUserMissionInfo info){

      string content = CurMissionInfo.ToJson().ToString("");
      wwDataSaveLoadManager.Instance.Save(content, getUserStatePath(info.MissionUUID));
    }

    public void CreateMissionInfo(trProgram program, string missionUUID){
      CurMissionInfo = new trUserMissionInfo();
      CurMissionInfo.Program = program;
      CurMissionInfo.MissionUUID = missionUUID;
      if(!UUIDToProgressDic.ContainsKey(missionUUID)){
        trUserProgress newProgress = new trUserProgress(missionUUID, false, false);
        UUIDToProgressDic.Add(missionUUID, newProgress);
      }
      Save();
    }
  }

  public class trUserMissionInfo{
    public string MissionUUID = "";
    public trProgram Program = trProgram.NewProgram();
    public int ResetProgramPuzzleIndex = -1;
    private trProgram programForReset = null;
    public trProgram ProgramForReset{
      set{
        programForReset = value;
      }
      get{
        if(programForReset == null){
          programForReset = Program.DeepCopy();
        }
        return programForReset;
      }
    }
    public int PuzzleIndex = 0;

    //Made hint index default value to 0 because:
    //We want to animate hint button if the user is not making progress for some time and there is an available hint.
    //For every hint we only want to show the animation once. trDataManager.Instance.MissionMng.ActivateNextHint() 
    //gives infomation about if it's the first time activating hint.
    //If tht default value is 1, we dont have the animation for the first hint.
    public int HintIndex = 0;

    public bool IsShownMissionInfo = false;

    public bool IsFirstPuzzle{
      get{
        return PuzzleIndex == 0;
      }
    }

    public void UpdateResetProgram(){
      if(ResetProgramPuzzleIndex < PuzzleIndex){
        ProgramForReset = Program.DeepCopy();
        ResetProgramPuzzleIndex = PuzzleIndex;
      }
    }

    public JSONClass ToJson(){
      JSONClass js = new JSONClass();
      js[TOKENS.ID] = MissionUUID;
      js[TOKENS.PUZZLE_INDEX].AsInt = PuzzleIndex;
      js[TOKENS.HINT_INDEX].AsInt = HintIndex;
      js[TOKENS.PROGRAM] = Program.ToJson();
      js[TOKENS.PUZZLE_ID_RESET].AsInt = ResetProgramPuzzleIndex;
      if(ProgramForReset != null){
        js[TOKENS.PROGRAM_FOR_RESET] = ProgramForReset.ToJson();
      }
      js[TOKENS.SHOW_MISSION].AsBool = IsShownMissionInfo;
      return js;
    }

    public static trUserMissionInfo FromJson(JSONNode js){
      trUserMissionInfo info = new trUserMissionInfo();
      info.MissionUUID = js[TOKENS.ID].Value;
      info.PuzzleIndex = js[TOKENS.PUZZLE_INDEX].AsInt;
      info.HintIndex = js[TOKENS.HINT_INDEX].AsInt;
      info.Program =trFactory.FromJson<trProgram>(js[TOKENS.PROGRAM]); 
      if(js[TOKENS.PUZZLE_ID_RESET] != null){
        info.ResetProgramPuzzleIndex = js[TOKENS.PUZZLE_ID_RESET].AsInt;
      }
      if(js[TOKENS.PROGRAM_FOR_RESET] != null){
        info.ProgramForReset = trFactory.FromJson<trProgram>(js[TOKENS.PROGRAM_FOR_RESET]); 
      }
      info.IsShownMissionInfo = js[TOKENS.SHOW_MISSION].AsBool;
      return info;
    }

  }


  public class trProgramIngredientInfo{

    public Dictionary<string, int> BehaviorTable  = new Dictionary<string, int>();
    public Dictionary<trTriggerType, int> TriggerTable = new Dictionary<trTriggerType, int>();

    public void CalculateIngredients(trProgram targetProgram){
      BehaviorTable.Clear();
      TriggerTable.Clear();
      
      foreach(trState state in targetProgram.UUIDToStateTable.Values){
        if(state == targetProgram.StateStart){
          continue;
        }
        string beh = state.Behavior.UUID;
        if(BehaviorTable.ContainsKey(beh)){
          BehaviorTable[beh] ++;
        }
        else{
          BehaviorTable.Add(beh, 1);
        }
      }
      
      foreach(trTransition transition in targetProgram.UUIDToTransitionTable.Values){
        trTriggerType type = transition.Trigger.Type;
        if(TriggerTable.ContainsKey(type)){
          TriggerTable[type] ++;
        }
        else{
          TriggerTable.Add(type, 1);
        }
      }
    }

    public int getIngredientsCount(){
      return BehaviorTable.Keys.Count + TriggerTable.Keys.Count;
    }
  }

  public enum MissionEditState{
    NORMAL, // not in edit mode
    EDIT,
    EDIT_HINT_PROGRAM,
  }

  public class trMissionManager{
    public trAuthoringMissionInfo AuthoringMissionInfo = new trAuthoringMissionInfo();
    public trUserOverallProgress UserOverallProgress = new trUserOverallProgress();


    private trProgramIngredientInfo ingredientInfo = new trProgramIngredientInfo();
    public trProgramIngredientInfo CurPuzzleIngredientInfo {
      get{
        ingredientInfo.CalculateIngredients(GetCurPuzzle().Hints[GetCurPuzzle().Hints.Count -1].Program);
        return ingredientInfo;
      }
    }


    public void Load(){
      AuthoringMissionInfo.Load();
      UserOverallProgress.Load();
      UserOverallProgress.MigrateToCurrentAppVersion();
      UpdateOverallProgressBasedOnMap();

      // set first mission to be recent played mission if it's not stored
      if(UserOverallProgress.RecentPlayedMissionUUID == ""){
        UserOverallProgress.RecentPlayedMissionUUID = 
          AuthoringMissionInfo.MissionMap.StateStart.OutgoingTransitions[0].StateTarget.Behavior.MissionFileInfo.UUID;
      }
    }

    public bool isAnyChallengeStarted(){
      foreach(string uuid in UserOverallProgress.UUIDToProgressDic.Keys){
        if(UserOverallProgress.UUIDToProgressDic[uuid].StartReported){
          return true;
        }
      }
      return false;
    }

    public bool isUnlockFreePlayMission(trUserMissionInfo mission){
      return (mission != null) && (string.Equals(mission.MissionUUID, trUserOverallProgress.UNLOCK_FREEPLAY_MISSION_UUID_DASH) || 
        string.Equals(mission.MissionUUID, trUserOverallProgress.UNLOCK_FREEPLAY_MISSION_UUID_DOT));      
    }

    public bool isUnlockCommunityMission(trUserMissionInfo mission){
      return (mission != null) && (string.Equals(mission.MissionUUID, trUserOverallProgress.UNLOCK_COMMUNITY_MISSION_UUID_DASH) || 
        string.Equals(mission.MissionUUID, trUserOverallProgress.UNLOCK_COMMUNITY_MISSION_UUID_DOT));
    }

    public void UpdateOverallProgressBasedOnMap(){
      foreach(trState state in AuthoringMissionInfo.MissionMap.UUIDToStateTable.Values){
        bool isCompleted = !state.Behavior.IsMissionBehavior;
        string uuid = "";
        
        if(state.Behavior.IsMissionBehavior){       
          uuid  = state.Behavior.MissionFileInfo.UUID;
          isCompleted |= (UserOverallProgress.UUIDToProgressDic.ContainsKey(uuid)
                          &&UserOverallProgress.UUIDToProgressDic[uuid].IsCompletedOnce);
        }   
        
        if(isCompleted){          
          foreach(trTransition transition in state.OutgoingTransitions){
            uuid = transition.StateTarget.Behavior.MissionFileInfo.UUID;
            if(!UserOverallProgress.UUIDToProgressDic.ContainsKey(uuid)){
              trUserProgress progress = new trUserProgress(uuid, false, false);
              UserOverallProgress.UUIDToProgressDic.Add(uuid, progress);
            }
          }
        }

      }
    }

    // unlock one path that leads to the mission with uuid
    // this is used in authoring panel only
    public void UpdateUserOverallProgress(string uuid){
      Dictionary<string, string> parentTable = new Dictionary<string, string>();
      foreach( trTransition transition in AuthoringMissionInfo.MissionMap.UUIDToTransitionTable.Values){
        if(transition.StateSource.Behavior.IsMissionBehavior
           &&transition.StateTarget.Behavior.IsMissionBehavior){
           if(!parentTable.ContainsKey(transition.StateTarget.Behavior.MissionFileInfo.UUID)){
            parentTable.Add(transition.StateTarget.Behavior.MissionFileInfo.UUID,
                            transition.StateSource.Behavior.MissionFileInfo.UUID);
           }
           else{
            WWLog.logError("Mission "+ transition.StateTarget.Behavior.MissionFileInfo.MissionName +" has two parent challenges.Please make sure it's intentional.");
          }
        }
      }

      Dictionary<string, trUserProgress> newProgressDic = new Dictionary<string, trUserProgress>();
      UserOverallProgress.IQPoints = 0;
      UserOverallProgress.IsFreePlayUnlocked = false;

      newProgressDic.Add(uuid, new trUserProgress(uuid, false, false));
      
      if(parentTable.ContainsKey(uuid)){
        string curMission = parentTable[uuid] ;
        trUserProgress progress = new trUserProgress(curMission, true, true);
        newProgressDic.Add(curMission, progress);
        UserOverallProgress.IQPoints += AuthoringMissionInfo.UUIDToMissionDic[curMission].MaxIQPoints;

        while(parentTable.ContainsKey(curMission)){
          curMission = parentTable[curMission];
          trUserProgress newProgress = new trUserProgress(curMission, true, true);
          newProgressDic.Add(curMission, newProgress);
          UserOverallProgress.IQPoints += AuthoringMissionInfo.UUIDToMissionDic[curMission].MaxIQPoints;
        }
      }
      //add those that connect to startstate
      foreach(trTransition transition in AuthoringMissionInfo.MissionMap.StateStart.OutgoingTransitions){
        string uid = transition.StateTarget.Behavior.MissionFileInfo.UUID;
        if(!newProgressDic.ContainsKey(uid)){
          newProgressDic.Add(uid, UserOverallProgress.UUIDToProgressDic[uid]);
        }
      }
      
      foreach(string id in UserOverallProgress.UUIDToProgressDic.Keys){
        if(!newProgressDic.ContainsKey(id)){
          UserOverallProgress.ClearUserMissionInfo(id);
        }
      }

      

      UserOverallProgress.UUIDToProgressDic = newProgressDic;
      UserOverallProgress.ClearUserMissionInfo(uuid);
      UserOverallProgress.SaveOverallProgress();
    }

    public void LoadMission(string uuid){
      AuthoringMissionInfo.LoadMission(uuid);
      if(AuthoringMissionInfo.CurMission == null){
        WWLog.logWarn("No mission is available.");
        return;
      }

      UserOverallProgress.LoadUserState(uuid);

      if(UserOverallProgress.CurMissionInfo == null){
        CreateMissionInfo();    
      }

      piRobotType missionRobotType = AuthoringMissionInfo.CurMission.Type.GetRobotType();

      if( missionRobotType != UserOverallProgress.CurMissionInfo.Program.RobotType){
        WWLog.logInfo("Update challenge robot type from " + UserOverallProgress.CurMissionInfo.Program.RobotType + " to " + missionRobotType);
        UserOverallProgress.CurMissionInfo.Program.RobotType = missionRobotType;
        UserOverallProgress.CurMissionInfo.ProgramForReset = UserOverallProgress.CurMissionInfo.Program;

      }
    }

    public int TotalMissionsCount(){
      return AuthoringMissionInfo.MissionMap.UUIDToStateTable.Count - 1;
    }

    public int CompletedMissionsCount(){
      int count = 0;
      foreach(trUserProgress progress in UserOverallProgress.UUIDToProgressDic.Values){
        if (progress.IsCompletedOnce){
          count++;
        }
      }
      return count;
    }

    public bool IsAnyMissionUnlockedNow(){
      foreach(trUserProgress progress in UserOverallProgress.UUIDToProgressDic.Values){
        if(!progress.IsCompletedOnce && progress.IsUnlockNow){
          return true;
        }
      }
      return false;
    }


    public bool IsAnyMissionUnlockedButNotStarted(){
      foreach(string uuid in UserOverallProgress.UUIDToProgressDic.Keys){
        if( UserOverallProgress.UUIDToProgressDic[uuid].IsCompletedOnce){
          continue;
        }

        if(UserOverallProgress.IsMissionUnlocked(uuid)
           && !UserOverallProgress.IsMissionStarted(uuid)){
          return true;
        }
      }
      return false;
    }

    public bool CreateMissionInfo(){
      if(AuthoringMissionInfo.CurMission.Puzzles.Count >0
         &&AuthoringMissionInfo.CurMission.Puzzles[0].Hints.Count >0){

        trProgram startProgram = AuthoringMissionInfo.CurMission.Puzzles[0].Hints[0].Program.DeepCopy();
        UserOverallProgress.CreateMissionInfo(startProgram, AuthoringMissionInfo.CurMission.UUID);
        return true;
      }  
      return false;
    }
    
    public bool ActivateNextHint(){
      if(AuthoringMissionInfo.EditState == MissionEditState.EDIT ||
         AuthoringMissionInfo.EditState == MissionEditState.EDIT_HINT_PROGRAM){
        WWLog.logError("Shouldn't happen during admin control");
        return false;
      }
      if(UserOverallProgress.CurMissionInfo.HintIndex == GetCurPuzzle().Hints.Count -2){
        return false;
      }
      UserOverallProgress.CurMissionInfo.HintIndex++;
      UserOverallProgress.SaveCurMissionInfo();
      return true;
    }

    public bool ActivateNextPuzzle(int incorrect_runs, int incompleteRuns, int seconds_spent){
      if(AuthoringMissionInfo.EditState == MissionEditState.EDIT ||
         AuthoringMissionInfo.EditState == MissionEditState.EDIT_HINT_PROGRAM){
        WWLog.logError("Shouldn't happen during admin control");
        return false;
      }

      if(UserOverallProgress.UUIDToProgressDic[UserOverallProgress.CurMissionInfo.MissionUUID].IsCompleted){
        return false;
      }
     
      int BQ_earned = 0;
      bool isReplay = UserOverallProgress.UUIDToProgressDic[UserOverallProgress.CurMissionInfo.MissionUUID].IsCompletedOnce;

      if(!isReplay){
        UserOverallProgress.IQPoints += GetCurPuzzle().IQPoints; // order matters !
        BQ_earned = GetCurPuzzle().IQPoints;
      }

      new trTelemetryEvent(trTelemetryEventType.CHAL_FINISH_STEP, true)
        .add(trTelemetryParamType.CHALLENGE, trDataManager.Instance.MissionMng.GetCurMission().UserFacingName)
        .add(trTelemetryParamType.STEP, trDataManager.Instance.MissionMng.UserOverallProgress.CurMissionInfo.PuzzleIndex)
        .add (trTelemetryParamType.INCORRECT_RUNS, incorrect_runs)
        .add (trTelemetryParamType.INCOMPLETE_RUNS, incompleteRuns)
        .add(trTelemetryParamType.TIME, seconds_spent)
        .add(trTelemetryParamType.BQ_EARNED, BQ_earned)
        .add(trTelemetryParamType.IS_REPLAY, trDataManager.Instance.MissionMng.UserOverallProgress.IsCurMissionCompleteOnceInt)
        .emit();
      
      if(UserOverallProgress.CurMissionInfo.PuzzleIndex == AuthoringMissionInfo.CurMission.Puzzles.Count -1){
        string uuid = UserOverallProgress.CurMissionInfo.MissionUUID;
        UserOverallProgress.CompleteMission(uuid);

        trTelemetryEventType trTET = isReplay ? trTelemetryEventType.CHAL_FINISH_REPLAY : trTelemetryEventType.CHAL_FINISH_FIRSTPLAY;


        new trTelemetryEvent(trTET, true)
          .add(trTelemetryParamType.CHALLENGE, trDataManager.Instance.MissionMng.GetCurMission().UserFacingName)
          .emit();

        //set unlock time for child missions
        foreach(trState state in AuthoringMissionInfo.MissionMap.UUIDToStateTable.Values){
          if(state.Behavior.IsMissionBehavior && state.Behavior.MissionFileInfo.UUID == uuid){
            foreach(trTransition transition in state.OutgoingTransitions){
              string unlockedUuid = transition.StateTarget.Behavior.MissionFileInfo.UUID;
              if(!UserOverallProgress.UUIDToProgressDic.ContainsKey(unlockedUuid)){
                trUserProgress newProgress = new trUserProgress(unlockedUuid, false, false);
                if(transition.Trigger.Type == trTriggerType.TIME){
                  newProgress.UnlockTimeUTC = System.DateTime.UtcNow.AddSeconds((double)transition.Trigger.ParameterValue);
                  handleLocalNotificationtion(newProgress.UnlockTimeUTC);
                }
                UserOverallProgress.UUIDToProgressDic.Add(unlockedUuid, newProgress);
                UserOverallProgress.SaveOverallProgress();
              }

            }
          }
        }
        return false;
      }

      UserOverallProgress.CurMissionInfo.PuzzleIndex++;
      UserOverallProgress.CurMissionInfo.HintIndex = 0;
      float progress =  (float)UserOverallProgress.CurMissionInfo.PuzzleIndex/(float)AuthoringMissionInfo.CurMission.Puzzles.Count;
      UserOverallProgress.UpdateMissionProgress(UserOverallProgress.CurMissionInfo.MissionUUID, progress);
      UserOverallProgress.Save();

      return true;
    }

    public void RestartCurMission(){
      UserOverallProgress.CurMissionInfo.Program = AuthoringMissionInfo.CurMission.Puzzles[0].Hints[0].Program.DeepCopy();
      UserOverallProgress.CurMissionInfo.ProgramForReset = AuthoringMissionInfo.CurMission.Puzzles[0].Hints[0].Program.DeepCopy();
      UserOverallProgress.RestartCurMission();
    }

    void handleLocalNotificationtion(System.DateTime utcTime){
      piConnectionManager.Instance.scheduleLocalNotification("Wonder", "New challenge is available! Go check!", "", utcTime);
    }

    public trPuzzle GetCurPuzzle(){
      if(AuthoringMissionInfo.EditState == MissionEditState.EDIT ||
         AuthoringMissionInfo.EditState == MissionEditState.EDIT_HINT_PROGRAM){
        return AuthoringMissionInfo.CurPuzzle;
      }
      else{
        if(UserOverallProgress.CurMissionInfo == null){
          return null;
        }

        if(UserOverallProgress.CurMissionInfo.PuzzleIndex >AuthoringMissionInfo.CurMission.Puzzles.Count-1){
          WWLog.logWarn("Saved puzzle index is invalid ");
          UserOverallProgress.CurMissionInfo.PuzzleIndex = 0;
        }
        return AuthoringMissionInfo.CurMission.Puzzles[UserOverallProgress.CurMissionInfo.PuzzleIndex];
      }
    }

    public trHint GetCurHint(){
      if(AuthoringMissionInfo.EditState == MissionEditState.EDIT ||
         AuthoringMissionInfo.EditState == MissionEditState.EDIT_HINT_PROGRAM){
        return AuthoringMissionInfo.CurHint;
      }
      else{
        if(UserOverallProgress.CurMissionInfo.HintIndex > GetCurPuzzle().Hints.Count-1){
          WWLog.logWarn("Saved hint index is invalid ");
          UserOverallProgress.CurMissionInfo.HintIndex = 0;
        }
        return GetCurPuzzle().Hints[UserOverallProgress.CurMissionInfo.HintIndex];
      }
    }

    public trMission GetCurMission(){
      return AuthoringMissionInfo.CurMission;
    }

    public trProgram GetTargetProgram(){

      return GetCurPuzzle().Hints[GetCurPuzzle().Hints.Count -1].Program;

    }

    public trProgram GetCurProgram(){
      if(AuthoringMissionInfo.EditState == MissionEditState.EDIT_HINT_PROGRAM){
        return AuthoringMissionInfo.CurHint.Program;
      }
      else if(AuthoringMissionInfo.EditState == MissionEditState.EDIT){
        return AuthoringMissionInfo.MissionMap;
      }
      else{
        return UserOverallProgress.CurMissionInfo.Program;
      }
    }

    public void SaveCurProgram(){
      if(AuthoringMissionInfo.EditState == MissionEditState.EDIT ||
         AuthoringMissionInfo.EditState == MissionEditState.EDIT_HINT_PROGRAM){
        AuthoringMissionInfo.Save();
        UpdateOverallProgressBasedOnMap();
      }
      else{
        UserOverallProgress.SaveCurMissionInfo();
        if(trHintManager.Instance != null){
          trHintManager.Instance.TinkeredProgram();
        }

      }
    }
  }
}
