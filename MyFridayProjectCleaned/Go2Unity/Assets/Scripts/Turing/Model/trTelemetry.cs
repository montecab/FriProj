using UnityEngine;
using System.Collections.Generic;

// wrapper around telemetry.
// for example usage, see "ExampleUsage" method in trTelemetry class.

namespace Turing {

  public enum trTelemetryEventType {
    // code generated here:
    // https://docs.google.com/spreadsheets/d/14zM3EYvUYkV6ix61SPzMYUiORstqzCHJndEAADLKrzU/edit#gid=0

    UNKNOWN,
    
    // Challenge System
    CHAL_AREA,             // when the user click on a map zone
    CHAL_START_FIRSTPLAY,  // when the user first opens a challenge
    CHAL_RESUME_FIRSTPLAY, // when the user resumes a challenge they have not yet completed
    CHAL_START_REPLAY,     // when the user begins replaying a challenge
    CHAL_RESUME_REPLAY,    // when the user resumes a challenge they're replaying
    CHAL_BEGIN_STEP,       // when the user gets the first introduction to a new step
    CHAL_FINISH_STEP,      // whenever the user completes a step
    CHAL_FINISH_FIRSTPLAY, // when the user gets the 'you completed it' dialog for the entire challenge, first time
    CHAL_FINISH_REPLAY,    // when the user gets the 'you completed it' dialog for the entire challenge, replay
    CHAL_HINT,             // whenever the user taps the hint button
    REWARD_UNLOCKED,       // when the user unlocks a reward
    CHAL_RESET,            // when the user resets the challenge
    CHAL_SM_START,         // whenever the user Starts a SM (challenge-mode only)
    CHAL_DEL_ACTION,
    CHAL_DEL_CUE,
    CHAL_SAVE,             // whenever the user adds a new copy of a challenge to their My Programs
    CHAL_RUN,              // user test drive a challenge program before remixing
    CHAL_MIGRATE,          // when challenge progress is being migrated from a previous app version
    
    // Free Play
    FP_SM_START, // whenever the user Starts a SM (freeplay only)
    FP_SM_STOP,
    FP_TRANSFER, // whenever the user starts a transfer to robot
    FP_PROGRAM_NEW, // when the user taps the new program button
    FP_PROGRAM_DEL, // when the user deletes a program
    FP_SET_ACTION, // when the user drags an action item from the tray
    FP_SET_CUE,
    FP_DEL_ACTION,
    FP_DEL_CUE,
    FP_STATE_CHANGE, // when the state machine state changes
    FP_PARAM_ACTION, // an action parameter has been edited
    FP_PARAM_CUE,    // a cue parameter has been edited

    // video playback
    STORY_PROFILE_VIEWCLIP,

    // misc
    ERR_PROGRAM_VALIDATION,     // when a trProgram fails validation
    PROGRAM_INCOMPATIBLE,       // when a user tries to download a program with newer version

    // Community
    CMNTY_LOAD, // community loads a new list (based on category & robot type)
    CMNTY_ITEM, // user clicks a comunity program
    CMNTY_RUN, // user tests a program in community
    CMNTY_SAVE, // user downloads a community program

    // Sharing
    SHR_UPLOAD, // user uploaded a program
    SHR_REDEEM_BTN, // user clicked on redeem button
    SHR_RUN, // user tested a shared program 
    SHR_SAVE, // user downloaded a shared program
    SHR_REDEEM_KEY, // user tried to redeem a key
    SHR_UPLOAD_BTN, // user clicked share button

    // Video Hints
    VIDEO_HINT_PLAY, // user plays a video hint
    VIDEO_HINT_SKIP, // user plays and closes a video hint before it ends

    // Undo/redo
    FP_UNDO, // when a user click on undo in freeplay
    FP_REDO, // when a user click on redo in freeplay
    CHAL_UNDO, // when a user click on undo in challenge
    CHAL_REDO, // when a user click on redo in challenge

    //FTUE
    WONDER_FTUE_SHOW,
    WONDER_FTUE_SKIP,
    WONDER_FTUE_FREEPLAY,
    WONDER_FTUE_FINISH,
  }
  
  public enum trTelemetryParamType {
    AREA,
    BEHAVIOR_CURR,
    BEHAVIOR_PREV,
    BQ,
    BQ_EARNED,
    CATEGORY,
    CHALLENGE,
    CONTEXT,
    DETAIL,
    DURATION,
    FILE_ID,
    FILE_NAME,
    HINT,
    ID,
    INCOMPLETE_RUNS,
    INCORRECT_RUNS,
    IS_REPLAY,
    NUM_PROGS,
    NUM_STATES,
    NUM_TRANSITIONS,
    REWARD_ID,
    ROBOT_TYPE,
    STEP,
    STORY_ID,
    SUCCESS,
    TIME,
    TOKEN,
    TRIGGER,
    TYPE,
    TYPE_PREV,
    VER_CURR,
    VER_NEW,
    VER_PREV,
  }
  
  public class trTelemetryEvent {
    public trTelemetryEventType eventType;
    public Dictionary<string, string> paramDict = new Dictionary<string, string>();
    public const int MAX_PARAMS = 10;
    
    public trTelemetryEvent(trTelemetryEventType trTET, bool includeStandardParameters) {
      eventType = trTET;
      
      if (includeStandardParameters) {
        addStandardParameters();
      }
    }
    
    public trTelemetryEvent addStandardParameters() {
      add(trTelemetryParamType.BQ, trDataManager.Instance.GetIQPoints());
      return this;
    }
    
    public trTelemetryEvent add(trTelemetryParamType trTPT, string val) {
      string sTPT = trTPT.ToString();
      if (paramDict.ContainsKey(sTPT)) {
        WWLog.logWarn("over-writing existing key in telemetry event: " + sTPT);
      }
      
      if (paramDict.Count >= MAX_PARAMS) {
        WWLog.logError("maximum params exceeded: max is " + MAX_PARAMS + ". omitting " + sTPT);
      }
      else {
        paramDict[sTPT] = val;
      }
      
      return this;
    }
    
    public trTelemetryEvent add<T>(trTelemetryParamType trTPT, T val) {
      return add(trTPT, val.ToString());
    }
    
    public void emit() {
      // this is how we switch between back-end service providers.
      // probably would be cleaner to have trDataManager have a trTelemetry instance or something,
      // but i think that would actually mean more typing for ppl trying to use the telemetry service.
      trTelemetry_Flurry.Instance.Emit(this);
    }
    
    public override string ToString ()
    {
      string s = "";
      string delim = "";
      s += "{";
      foreach (string sTPT in paramDict.Keys) {
        s += delim;
        s += "\"" + sTPT + "\":";
        s += "\"" + paramDict[sTPT] + "\"";
        delim = ", ";
      }
      s += "}";
     
      return eventType.ToString() + " " + s;
    }
  }
}
