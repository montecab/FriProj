using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using WW.SimpleJSON;

namespace Turing{
  public class trMission: trBase{
    public string UserFacingName = "NewMission";
    public string IntroDescription = "This is a description.";
    private bool ready_to_translate = false; // not used by wonder directly, but needs to be preserved for translation flow.
    public bool IsIntroOutroMode = false; //TODO:UI: deprecated, this is always false
    public string OutroDescription = "";
    public string IntroSprite = "scene_1_Heeyorg";
    public string OutroSprite = "scene_1_Heeyorg";

    public string SpriteName = "sprite";
    public List<trPuzzle> Puzzles = new List<trPuzzle>();
    public bool IsTutorialMission = false;
    public string StartVideo = "";
    public string EndVideo = "";

    private trMissionType type;
    public trMissionType Type{
      set{
        type = value;
        UpdateRobotType();
      }
      get{
        return type;
      }
    }

    public int MaxIQPoints{
      get{
        int ret = 0; 
        for(int i = 0; i< Puzzles.Count; ++i){
          ret += Puzzles[i].IQPoints;
        }
        return ret;
      }
    }

    public trMission(){
      SpriteName = trIconFactory.ChallengeIconNames[0];
    }

    public void UpdateRobotType(){
      piRobotType robotType = type.GetRobotType();
      for(int i = 0; i<Puzzles.Count; ++i){
        for(int j = 0; j < Puzzles[i].Hints.Count; ++j){
          if(Puzzles[i].Hints[j].Program.RobotType != robotType){
            Puzzles[i].Hints[j].Program.RobotType = robotType;
          }
        }
      }
    }

    public static trMission FromJson(JSONClass jsc){
      trMission ret = new trMission();
      ret.OutOfJson(jsc);
      return ret;
    }

    protected override void IntoJson(JSONClass jsc) {

      jsc[TOKENS.USER_FACING_NAME] = UserFacingName;
      jsc[TOKENS.DESCRIPTION] = IntroDescription;
      jsc[TOKENS.ICON_NAME] = SpriteName;
      jsc[TOKENS.TUTORIAL_MISSION].AsBool = IsTutorialMission;
      jsc[TOKENS.TYPE].AsInt = (int) Type;
      jsc[TOKENS.VIDEO_START] = StartVideo;
      jsc[TOKENS.VIDEO_END] = EndVideo;
      jsc[TOKENS.INTROOUTRO_MODE].AsBool = IsIntroOutroMode;

      if (ready_to_translate) {
        jsc[TOKENS.READY_TO_TRANSLATE].AsBool = ready_to_translate;
      }

      if(IsIntroOutroMode){
        jsc[TOKENS.OUTRO_DESCRIPTION] = OutroDescription;
        jsc[TOKENS.SPRITE_INTRO] = IntroSprite;
        jsc[TOKENS.SPRITE_OUTRO] = OutroSprite;
      }

      JSONArray jssteps = new JSONArray();      
      foreach (trPuzzle puzzle in Puzzles) {
        jssteps.Add(puzzle.ToJson());
      }
      jsc[TOKENS.PUZZLES] =jssteps;

      base.IntoJson(jsc);
    }
    
    protected override void OutOfJson(JSONClass jsc) {
      UserFacingName = jsc[TOKENS.USER_FACING_NAME];
      IntroDescription = jsc[TOKENS.DESCRIPTION];
      SpriteName = jsc[TOKENS.ICON_NAME];

      if(jsc[TOKENS.INTROOUTRO_MODE] != null){
        IsIntroOutroMode = jsc[TOKENS.INTROOUTRO_MODE].AsBool;
        if(IsIntroOutroMode){
          if(jsc[TOKENS.OUTRO_DESCRIPTION] != null){
            OutroDescription = jsc[TOKENS.OUTRO_DESCRIPTION];
          }
          if(jsc[TOKENS.SPRITE_INTRO] != null){
            IntroSprite = jsc[TOKENS.SPRITE_INTRO];
          }
          if(jsc[TOKENS.SPRITE_OUTRO] != null){
            OutroSprite = jsc[TOKENS.SPRITE_OUTRO];
          }
        }
      }

      if(jsc[TOKENS.TUTORIAL_MISSION] != null){
        IsTutorialMission = jsc[TOKENS.TUTORIAL_MISSION].AsBool;
      }       

      if(jsc[TOKENS.TYPE] != null){
        Type = (trMissionType)(jsc[TOKENS.TYPE].AsInt);
      }

      if(jsc[TOKENS.VIDEO_START] != null){
        StartVideo = jsc[TOKENS.VIDEO_START];
      }

      if(jsc[TOKENS.VIDEO_END] != null){
        EndVideo = jsc[TOKENS.VIDEO_END];
      }

      if(jsc[TOKENS.READY_TO_TRANSLATE] != null){
        ready_to_translate = jsc[TOKENS.READY_TO_TRANSLATE].AsBool;
      }

      foreach (JSONClass jsM in jsc[TOKENS.PUZZLES].AsArray) {
        trPuzzle puzzle = trPuzzle.FromJson(jsM);
        Puzzles.Add(puzzle);
      }
      base.OutOfJson(jsc);
    }
  }

  // not using piRobotType here because we want to add more type easily like launcher
  public enum trMissionType{
    DASH = 0,
    DOT = 1
  }

  static class trMissionTypeMethods{
    public static piRobotType GetRobotType(this trMissionType type){
      switch(type){
      case trMissionType.DASH:
        return piRobotType.DASH;
      case trMissionType.DOT:
        return piRobotType.DOT;
      }
      return piRobotType.UNKNOWN;
    }
  }

  public class trPuzzle{
    public string UserFacingName = "NewPuzzle";
    public string Description = "This is a description.";
    public string IntroductionText = "This is a introduction.";
    public string IntroductionIconName = "ui_eli_0";
    public bool IsShowIntroduction = true;
    public bool IsLoadStartProgram = false; // if the puzzle starts with the first hint's program
    public bool IsExportToRobotTutorial = false;
    public bool IsCenterProgramOnStart = false;
    public List<trHint> Hints = new List<trHint>();
    public Dictionary<string, trBehavior> UUIDToBehaviorDic = new Dictionary<string, trBehavior>();
    private bool ready_to_translate;  // not used by wonder directly, but needs to be preserved for translation flow.

    public int IQPoints = 10;

    public trPuzzle(){}

    public trHint LastHint{
      get{
        return Hints[Hints.Count - 1];
      }
    }

    public trHint FirstHint{
      get{
        return Hints[0];
      }
    }

    public JSONClass ToJson() {
      JSONClass jsc = new JSONClass();
      jsc[TOKENS.USER_FACING_NAME] = UserFacingName;
      jsc[TOKENS.DESCRIPTION] = Description;
      jsc[TOKENS.LOAD_START_PROGRAM].AsBool = IsLoadStartProgram;
      jsc[TOKENS.INTRODUCTION] = IntroductionText;
      jsc[TOKENS.SHOW_INTRODUCTION].AsBool = IsShowIntroduction;
      jsc[TOKENS.TRANSFER_ROBOT_TUT].AsBool = IsExportToRobotTutorial;
      jsc[TOKENS.ICON_NAME] = IntroductionIconName;
      jsc[TOKENS.CENTER_PROGRAM].AsBool = IsCenterProgramOnStart;

      JSONArray jshints = new JSONArray();      
      foreach (trHint hint in Hints) {
        jshints.Add(hint.ToJson());
      }
      jsc[TOKENS.HINTS] =jshints;
      jsc[TOKENS.IQ_POINTS].AsInt = IQPoints ;

      JSONArray jsB = new JSONArray();      
      foreach (trBehavior behavior in UUIDToBehaviorDic.Values) {
        jsB.Add(behavior.ToJson());
      }
      jsc[TOKENS.BEHAVIORS] = jsB;

      if (ready_to_translate) {
        jsc[TOKENS.READY_TO_TRANSLATE].AsBool = ready_to_translate;
      }

      return jsc;
    }
    
    public static trPuzzle FromJson(JSONNode jsc) {
      trPuzzle puzzle = new trPuzzle();
      puzzle.UserFacingName = jsc[TOKENS.USER_FACING_NAME];
      puzzle.Description = jsc[TOKENS.DESCRIPTION];
      puzzle.Hints.Clear();

      foreach (JSONClass jsM in jsc[TOKENS.HINTS].AsArray) {
        trHint hint = trHint.FromJson(jsM);
        puzzle.Hints.Add(hint);
      }
      puzzle.IQPoints = jsc[TOKENS.IQ_POINTS] == null? 10: jsc[TOKENS.IQ_POINTS].AsInt;

      if(jsc[TOKENS.LOAD_START_PROGRAM] != null){
        puzzle.IsLoadStartProgram = jsc[TOKENS.LOAD_START_PROGRAM].AsBool;
      }

      if(jsc[TOKENS.ICON_NAME] != null){
        puzzle.IntroductionIconName = jsc[TOKENS.ICON_NAME];
      }

      if(jsc[TOKENS.INTRODUCTION] != null){
        puzzle.IntroductionText = jsc[TOKENS.INTRODUCTION];
      }

      if(jsc[TOKENS.SHOW_INTRODUCTION] != null){
        puzzle.IsShowIntroduction = jsc[TOKENS.SHOW_INTRODUCTION].AsBool;
      }

      if(jsc[TOKENS.TRANSFER_ROBOT_TUT] != null){
        puzzle.IsExportToRobotTutorial = jsc[TOKENS.TRANSFER_ROBOT_TUT].AsBool;
      }

      if(jsc[TOKENS.BEHAVIORS] != null){
        foreach(JSONClass jsB in jsc[TOKENS.BEHAVIORS].AsArray){
          trBehavior behavior = trFactory.FromJson<trBehavior>(jsB);
          puzzle.UUIDToBehaviorDic.Add(behavior.UUID, behavior);
        }
      }else{
        if(puzzle.Hints.Count < 2){
          WWLog.logError("Not Enough Hints to calculate necessary behaviors");
          return puzzle;
        }
        foreach(trBehavior beh in puzzle.Hints[puzzle.Hints.Count - 1].Program.UUIDToBehaviorTable.Values){
          puzzle.UUIDToBehaviorDic.Add(beh.UUID, beh);
        }
      }

      if(jsc[TOKENS.CENTER_PROGRAM] != null){
        puzzle.IsCenterProgramOnStart = jsc[TOKENS.CENTER_PROGRAM].AsBool; 
      }

      if(jsc[TOKENS.READY_TO_TRANSLATE] != null){
        puzzle.ready_to_translate = jsc[TOKENS.READY_TO_TRANSLATE].AsBool;
      }

      return puzzle;
    }
  }

  public class trHint{
    private trProgram program = null;
    public trProgram Program{
      set{
        program = value;
      }
      get{
        if(program == null){
          program = trProgram.NewProgram();
        }
        return program;
      }
    }
    public string UserFacingName = "NewHint";
    public string Description = "";
    public int SubstractIQPoints = 0; // The points that will be substracted from IQ points if this hint is reviewed
    public bool IsCheckingStatePara = false;
    public bool IsCheckingTriggerPara = false;
    public string VideoPath = "";
    private bool ready_to_translate = false; // not used by wonder directly, but needs to be preserved for translation flow.

    public JSONClass ToJson() {
      JSONClass jsc = new JSONClass();
      jsc[TOKENS.USER_FACING_NAME] = UserFacingName;
      jsc[TOKENS.PROGRAM] = Program.ToJson();
      jsc[TOKENS.IQ_POINTS].AsInt = SubstractIQPoints;
      jsc[TOKENS.DESCRIPTION] = Description;
      jsc[TOKENS.CHECK_STATE_PARA].AsBool = IsCheckingStatePara;
      jsc[TOKENS.CHECK_TRIGGER_PARA].AsBool = IsCheckingTriggerPara;
      jsc[TOKENS.FILE_NAME] = VideoPath;
      if (ready_to_translate) {
        jsc[TOKENS.READY_TO_TRANSLATE].AsBool = ready_to_translate;
      }
      return jsc;
    }
    
    public static trHint FromJson(JSONClass jsc) {
      trHint hint = new trHint();
      hint.OutOfJson(jsc);
      return hint;
    }

    public void OutOfJson(JSONClass jsc) {
      UserFacingName = jsc[TOKENS.USER_FACING_NAME];
      Program = trFactory.FromJson<trProgram>(jsc[TOKENS.PROGRAM]);
      if(jsc[TOKENS.IQ_POINTS] != null){
        SubstractIQPoints = jsc[TOKENS.IQ_POINTS].AsInt;
      }
      if(jsc[TOKENS.DESCRIPTION] != null){
        Description = jsc[TOKENS.DESCRIPTION];
      }
      if(jsc[TOKENS.CHECK_STATE_PARA] != null){
        IsCheckingStatePara = jsc[TOKENS.CHECK_STATE_PARA].AsBool;
      }
      if(jsc[TOKENS.CHECK_TRIGGER_PARA] != null){
        IsCheckingTriggerPara = jsc[TOKENS.CHECK_TRIGGER_PARA].AsBool;
      }
      if(jsc[TOKENS.FILE_NAME] != null){
        VideoPath = jsc[TOKENS.FILE_NAME];
      }
      if(jsc[TOKENS.READY_TO_TRANSLATE] != null){
        ready_to_translate = jsc[TOKENS.READY_TO_TRANSLATE].AsBool;
      }
    }
  }
}
