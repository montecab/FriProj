using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using WW.SimpleJSON;
using WW.SaveLoad;

namespace Turing{
  public class trAppSaveInfo {

    private const string firstProgramName = "@!@My First Program@!@";

    public List<trProgram> Programs = new List<trProgram>();
  	public trProgram CurProgram {
      get {
        if (trDataManager.Instance.CurrentRobotTypeSelected == piRobotType.DOT) {
          return CurProgramForDot;
        }
        else {
          return CurProgramForDash;
        }
      }
      set {
        if (value.RobotType == piRobotType.DOT) {
          CurProgramForDot = value;
        }
        else {
          CurProgramForDash = value;
        }
      }
    }

    public trProgram CurProgramForDash {
      get {
        return curProgramForDash;
      }
      set {
        trProgram toBeSetProgram = value;
        initializeProgram(toBeSetProgram);
        curProgramForDash = toBeSetProgram;
      }
    }
    public trProgram curProgramForDash;

    public trProgram CurProgramForDot {
      get {
        return curProgramForDot;
      }
      set {
        trProgram toBeSetProgram = value;
        initializeProgram(toBeSetProgram);
        curProgramForDot = toBeSetProgram;
      }
    }
    public trProgram curProgramForDot;

    // If the file has not been loaded from disk yet, do it now.
    public void initializeProgram(trProgram program) {
      if(!program.IsInitialized) {
        //Load the json from disk
        string filePath = getSavePath(program.UUID);
        string programData = wwDataSaveLoadManager.Instance.Load(filePath);
        program.LoadFromProgramJson(programData);
      }
    }
    
    // Up for deprecation. 
    public const string TOKEN_PROGRAMS = "programs"; 

    public const string TOKEN_PROGRAMS_INFO = "programs_info";
    public const string TOKEN_CUR_PROGRAM = "cur_program";
    public const string TOKEN_CUR_PROGRAM_DASH = "cur_program_dash";
    public const string TOKEN_CUR_PROGRAM_DOT = "cur_program_dot";
    
    private Dictionary<string, trProgram> uuidToProgramTable = new Dictionary<string, trProgram>();

    private string appSaveInfoPath;

    public void Init(){
      appSaveInfoPath= Application.persistentDataPath + "/trAppSaveInfo.json";
    }

    public void Save(){
      
      if (CurProgram != null) {
        CurProgram.TouchRecentLoadedTime();
        foreach (trProgram trPRG in Programs) {
          if (trPRG.UUID == CurProgram.UUID) {
            trPRG.TouchRecentLoadedTime();
          }
        }
      }
      
      string data = this.ToJson().ToString("");
      wwDataSaveLoadManager.Instance.Save(data, appSaveInfoPath);
    }

    public void Load(){
      string data = wwDataSaveLoadManager.Instance.Load(appSaveInfoPath);
      if(data == null){
        CreateProgram(piRobotType.DASH, wwLoca.Format(firstProgramName));
        CreateProgram(piRobotType.DOT , wwLoca.Format(firstProgramName));
      }
      else{
        JSONNode js = JSON.Parse(data);
        FromJson(js);
      }     
    }

    public void ResetCurrentProgram(piRobotType robotType) {
      trProgram newCurrentProgram = null;
      foreach (trProgram program in Programs) {
        if(program.RobotType == robotType) {
          newCurrentProgram = program;
        }
      }
      if (newCurrentProgram == null) {
        CreateProgram(robotType, wwLoca.Format(firstProgramName));
      }
      else {
        if (robotType == piRobotType.DOT) {
          CurProgramForDot = newCurrentProgram;
        }
        else {
          CurProgramForDash = newCurrentProgram;
        }
      }
    }


    public void RemoveProgram(trProgram program, trProgram backupCurrentProgram){
      if(Programs.Contains(program)){
        Programs.Remove(program);
        uuidToProgramTable.Remove(program.UUID);
        string path = getSavePath(program.UUID);
        wwDataSaveLoadManager.Instance.Clear(path);
        wwDataSaveLoadManager.Instance.Clear(program.ThumbnailPath);
        
        if(CurProgram == program ){
          if (backupCurrentProgram != null) {
            if (!Programs.Contains(backupCurrentProgram)) {
              WWLog.logError("backup program is no good: unknown program");
              backupCurrentProgram = null;
            }
            else if (backupCurrentProgram.RobotType != CurProgram.RobotType) {
              WWLog.logError("backup program is no good: wrong type");
              backupCurrentProgram = null;
            }
          }
          
          if (backupCurrentProgram != null) {
            CurProgram = backupCurrentProgram;
          }
          else {
            ResetCurrentProgram(program.RobotType);
          }
        }

        Save();
        
        new trTelemetryEvent(trTelemetryEventType.FP_PROGRAM_DEL, true)
          .add(trTelemetryParamType.ROBOT_TYPE, program.RobotType)
          .add(trTelemetryParamType.NUM_PROGS, Programs.Count)
          .emit();
        
      }
    }
    
    public void AddMissionProgramAndSetCurProgram(trProgram program, string missionName){
      int number = 1;
      foreach(trProgram p in uuidToProgramTable.Values){
        if(p.RobotType == program.RobotType){
          string subName = p.UserFacingName;
          // get name up to the last underscore
          int lastIndexOfUnderscore = p.UserFacingName.LastIndexOf("_");
          int tmpNumber = 1;
          if (lastIndexOfUnderscore > 0) {
            string numberString = subName.Substring(lastIndexOfUnderscore + 1);
            if (int.TryParse(numberString, out tmpNumber)) {
              subName = subName.Substring(0, lastIndexOfUnderscore);
            }
          }
          if (subName == missionName) {
            number = Mathf.Max(tmpNumber + 1, number);
          }
        }
      }

      program.UserFacingName = missionName;
      if (number > 1) {
        program.UserFacingName += "_" + number.ToString();
      }
      
      program.TouchRecentLoadedTime();      
      
      AddProgram(program);
      CurProgram = program;                                                                                                                                                                                                                                                                                                                   
      Save();
    }

    public void AddProgram(trProgram program){
      if(uuidToProgramTable.ContainsKey(program.UUID)){
        WWLog.logError("Trying to add a program which already exist");
        return;
      }
      Programs.Add(program);
      SaveProgram(program);
      uuidToProgramTable.Add(program.UUID, program);
      Save();
    }

    public trProgram CreateProgram() {
      return CreateProgram(trDataManager.Instance.CurrentRobotTypeSelected);
    }

    public trProgram CreateProgram(piRobotType robotType, string userFacingName = null){
      trProgram newProgram = trProgram.NewProgram(robotType);
      if (!string.IsNullOrEmpty(userFacingName)) {
        newProgram.UserFacingName = userFacingName;
      }
      
      if(newProgram.RobotType == piRobotType.DASH) {
        CurProgramForDash = newProgram;
      }
      else if (newProgram.RobotType == piRobotType.DOT) {
        CurProgramForDot = newProgram;
      }
        
      SaveProgram(newProgram);
      Programs.Add(newProgram);
      uuidToProgramTable.Add(newProgram.UUID, newProgram);
      Save();

      return newProgram;
    }

    public void SaveCurProgram(){
      SaveProgram(CurProgram);
    }

    public void Clear(){
      wwDataSaveLoadManager.Instance.Clear(appSaveInfoPath);
    }

    public void SaveProgram(trProgram program){
      if(program == null){
        WWLog.logError("program is null");
        return;
      }

      program.ThumbnailDirty = true;
      Save(); // save thumbnail info
      string data = program.ToJson().ToString("");
      wwDataSaveLoadManager.Instance.Save(data, getSavePath(program.UUID));
    }

    // This should not effect any external users but effects internal users who have programs which were saved using the old format
    // The old format saved only the program ids and no other meta info in the traAppSaveInfo file which forced us to load all the programs
    private void fromJsonLegacy(JSONNode js) {
      JSONArray array = js[TOKEN_PROGRAMS].AsArray;
      for(int i = 0; i< array.Count; ++i){
        string fileName = array[i];
        string path = getSavePath(fileName);
        string programData = wwDataSaveLoadManager.Instance.Load(path);
        if(programData != null){
          JSONNode jsn = JSON.Parse(programData);
          trProgram newProgram = trFactory.FromJson<trProgram>(jsn);
          Programs.Add(newProgram);
          if(!uuidToProgramTable.ContainsKey(newProgram.UUID) ){
            uuidToProgramTable.Add(newProgram.UUID, newProgram);
          }
        }
        
      }
      trProgram legacyCurrent = null;
      if (js[TOKEN_CUR_PROGRAM] != null) {
        string curProgramUUID = js[TOKEN_CUR_PROGRAM];
        legacyCurrent = uuidToProgramTable[curProgramUUID];
      }
      if (js[TOKEN_CUR_PROGRAM_DASH] != null && uuidToProgramTable.ContainsKey(js[TOKEN_CUR_PROGRAM_DASH])) {
        CurProgramForDash = uuidToProgramTable[js[TOKEN_CUR_PROGRAM_DASH]];
      }
      if (js[TOKEN_CUR_PROGRAM_DOT] != null && uuidToProgramTable.ContainsKey(js[TOKEN_CUR_PROGRAM_DOT])) {
        CurProgramForDot = uuidToProgramTable[js[TOKEN_CUR_PROGRAM_DOT]];
      }
      
      if(CurProgramForDash == null && legacyCurrent != null) {
        CurProgramForDash = legacyCurrent;
      }
      
      if (CurProgramForDash == null) {
        CreateProgram(piRobotType.DASH);
      }
      if (CurProgramForDot == null) {
        CreateProgram(piRobotType.DOT); 
      }
      this.Save();
    }
    
    public void FromJson(JSONNode js){
      if (js[TOKEN_PROGRAMS_INFO] == null) {
        fromJsonLegacy(js);
        return;
      }

      JSONArray allProgramsInfo = js[TOKEN_PROGRAMS_INFO].AsArray;

      string currentDashProgramUUID = js[TOKEN_CUR_PROGRAM_DASH];
      string currentDotProgramUUID = js[TOKEN_CUR_PROGRAM_DOT];

      for(int i = 0; i< allProgramsInfo.Count; ++i){
        JSONNode programInfo = allProgramsInfo[i];
        trProgram newProgram = new trProgram();
        newProgram.UserFacingName = trProgram.sanitizeFilename(programInfo[TOKENS.USER_FACING_NAME]);
        newProgram.UUID = programInfo[TOKENS.ID];
        newProgram.RobotType = (piRobotType)(programInfo[TOKENS.ROBOT_TYPE].AsInt);
        if (programInfo.AsObject.ContainsKey(TOKENS.LOADED_TIME)) {
          newProgram.RecentLoadedTime = long.Parse(programInfo[TOKENS.LOADED_TIME]);
        }

        if(programInfo[TOKENS.THUMBNAIL_DIRTY] != null){
          newProgram.ThumbnailDirty = programInfo[TOKENS.THUMBNAIL_DIRTY].AsBool;
        }
        Programs.Add(newProgram);
        if (newProgram.UUID == currentDashProgramUUID) {
          CurProgramForDash = newProgram;
        }
        else if (newProgram.UUID == currentDotProgramUUID) {
          CurProgramForDot = newProgram;
        }
        if(!uuidToProgramTable.ContainsKey(newProgram.UUID) ){
          uuidToProgramTable.Add(newProgram.UUID, newProgram);
        }
      }
      if (CurProgramForDash == null) {
        CreateProgram(piRobotType.DASH);
      }
      if (CurProgramForDot == null) {
        CreateProgram(piRobotType.DOT); 
      }
    }
    

    string getSavePath(string fileName){
      return Application.persistentDataPath +  "/" + fileName + ".json";
    }

    public JSONClass ToJson(){
      JSONClass js = new JSONClass();
      JSONArray array = new JSONArray();
      foreach(trProgram program in Programs){
        JSONClass programInfo = new JSONClass();
        programInfo[TOKENS.USER_FACING_NAME] = trProgram.sanitizeFilename(program.UserFacingName);
        programInfo[TOKENS.ID] = program.UUID;
        programInfo[TOKENS.ROBOT_TYPE].AsInt = (int)program.RobotType;
        programInfo[TOKENS.LOADED_TIME] = program.RecentLoadedTime.ToString();
        programInfo[TOKENS.THUMBNAIL_DIRTY].AsBool = program.ThumbnailDirty;
        array[array.Count] = programInfo;
      }
      js[TOKEN_PROGRAMS_INFO] = array;

      if (CurProgramForDash != null) {
        js[TOKEN_CUR_PROGRAM_DASH] = CurProgramForDash.UUID;
      }
      if (CurProgramForDot != null) {
        js[TOKEN_CUR_PROGRAM_DOT] = CurProgramForDot.UUID;
      }
      return js;
    }
  }
}
