using UnityEngine;
using System.Collections.Generic;
using WW.SimpleJSON;
using WW.SaveLoad;


namespace Turing {

  public enum trFeatureVisibility {
    PUBLIC,
    INTERNAL,
    OFF,
  }

    
  public class trMultivariate : Singleton<trMultivariate> {

    public static readonly string DEBUG_TRANSLATION_TOKEN = "^---^";

    public enum trAppOption {
      NULL_OPTION,
      TRANSITION_SIMPLIFICATION_THRESHOLD,
      DISTANCE_CREATE_TRANSITION, 
      EXPORT_PANEL,
      OMNI_DISABLE_IN_TRAY,
      EXTRA_START_BUTTON,
      ZOOM_CENTER_WITH_START_STATE,
      AVOID_OVERLAP_WHEN_ZOOM,
      RUN_SPARK_BEHAVIOR,
      POWER_ON_STATE_MACHINE,
      CLIPBOARD,
      UNLOCK_SHARING,
      UNLOCK_FREEPLAY,
      UNLOCK_FUNCTIONS,
      UNLOCK_ALL_REWARDS,
      UNLOCK_ALL_CUE_REWARDS,
      UNLOCK_CONTROLLER,
      UNLOCK_EXPERIMENTAL_TRIGGERS,
      UNLOCK_PUPPET,
      UNLOCK_ANIM_SHOP,
      UNLOCK_COMMUNITY,
      UNLOCK_INTERNAL_ANIMATIONS,
      MULTIBOT_TRIGGERS,
      BUMPER_MODE,
      TRANSITION_LINE_QUALITY,
      SHOW_FPS_METER,
      PERF_STATES,
      PERF_TRIGGERS,
      PERF_LINES,
      FORCE_BAD_FRAMERATE_IN_EDITOR,
      MUTE_SOUND_AND_ANIMS,
      SHOW_LAUNCH_ACTIONS,
      USE_POSE_BASED_LINANG,
      VAULT_ALLOW_INTERNAL,
      STATE_MACHINE_GRID,
      ELEMENT_INFO_PANEL,
      SCREENSHOTS,
      RESET_TUTORIAL_SLIDES_DATA,
      REGENERATE_ALL_THUMBNAILS,
      SHOW_VIDEO_TIMESTAMP,
      LANGUAGE,
      DEBUG_TRANSLATION_MODE,
      DEBUG_MODE,
      SHOW_TOUCH_CURSOR,
    }
    
    public enum trAppOptionValue {
      UNDEFINED,
      YES,
      NO,
      NONE,
      MEDIUM,
      SMALL,
      BIG,
      SHOW,
      HIDE,
      NO_FRILLS_IMMEDIATE,
      NO_FRILLS_BUTTON,
      EYERING,
      PRODUCTION,
      CURRENT,
      VERY_VERY_LOW,
      VERY_LOW,
      LOW,
      NORMAL,
      VERY_HIGH,
      de_DE,
      en_US,
      ko_KR,
      zh_CN,
      UNHANDLED,
      SYSTEM,
    }
    
    private Dictionary<trAppOption, List<trAppOptionValue>> optionDefinitions = new Dictionary<trAppOption, List<trAppOptionValue>>();
    private Dictionary<trAppOption, trAppOptionValue      > optionSettings    = new Dictionary<trAppOption, trAppOptionValue>();

    private string localFilename = "";
    
    private bool isInit = false;

    public delegate void OnOptionValueChangedDelegate(trAppOption option, trAppOptionValue newValue);
    public OnOptionValueChangedDelegate ValueChanged;
    
    void init(){
      if(!isInit){
        isInit = true;

        localFilename = Application.persistentDataPath + "/trMultivariateSettings.json";
        // first item for a given option is the default.

        addOptionValue(trAppOption.TRANSITION_SIMPLIFICATION_THRESHOLD, trAppOptionValue.SMALL       );
        addOptionValue(trAppOption.TRANSITION_SIMPLIFICATION_THRESHOLD, trAppOptionValue.MEDIUM, true);
        addOptionValue(trAppOption.TRANSITION_SIMPLIFICATION_THRESHOLD, trAppOptionValue.BIG         );

        addOptionValue(trAppOption.LANGUAGE, trAppOptionValue.SYSTEM, true);
        addOptionValue(trAppOption.LANGUAGE, trAppOptionValue.de_DE);
        addOptionValue(trAppOption.LANGUAGE, trAppOptionValue.en_US);
        addOptionValue(trAppOption.LANGUAGE, trAppOptionValue.ko_KR);
        addOptionValue(trAppOption.LANGUAGE, trAppOptionValue.zh_CN);
        addOptionValue(trAppOption.LANGUAGE, trAppOptionValue.UNHANDLED);

        addOptionValue(trAppOption.DISTANCE_CREATE_TRANSITION, trAppOptionValue.SMALL       );
        addOptionValue(trAppOption.DISTANCE_CREATE_TRANSITION, trAppOptionValue.MEDIUM, true);
        addOptionValue(trAppOption.DISTANCE_CREATE_TRANSITION, trAppOptionValue.BIG         );
        
        addOptionValue(trAppOption.EXPORT_PANEL, trAppOptionValue.HIDE);
        addOptionValue(trAppOption.EXPORT_PANEL, trAppOptionValue.SHOW);

        addOptionValue(trAppOption.OMNI_DISABLE_IN_TRAY, trAppOptionValue.YES, true);
        addOptionValue(trAppOption.OMNI_DISABLE_IN_TRAY, trAppOptionValue.NO);

        addOptionValue(trAppOption.EXTRA_START_BUTTON, trAppOptionValue.YES);
        addOptionValue(trAppOption.EXTRA_START_BUTTON, trAppOptionValue.NO, true);

        addOptionValue(trAppOption.ZOOM_CENTER_WITH_START_STATE, trAppOptionValue.YES);
        addOptionValue(trAppOption.ZOOM_CENTER_WITH_START_STATE, trAppOptionValue.NO, true);

        addOptionValue(trAppOption.AVOID_OVERLAP_WHEN_ZOOM, trAppOptionValue.YES);
        addOptionValue(trAppOption.AVOID_OVERLAP_WHEN_ZOOM, trAppOptionValue.NO, true);
        
        addOptionValue(trAppOption.RUN_SPARK_BEHAVIOR, trAppOptionValue.HIDE);
        addOptionValue(trAppOption.RUN_SPARK_BEHAVIOR, trAppOptionValue.SHOW);
        
        addOptionValue(trAppOption.POWER_ON_STATE_MACHINE, trAppOptionValue.NO_FRILLS_IMMEDIATE);
        addOptionValue(trAppOption.POWER_ON_STATE_MACHINE, trAppOptionValue.NO_FRILLS_BUTTON);
        addOptionValue(trAppOption.POWER_ON_STATE_MACHINE, trAppOptionValue.EYERING);
        addOptionValue(trAppOption.POWER_ON_STATE_MACHINE, trAppOptionValue.PRODUCTION, true);
        addOptionValue(trAppOption.POWER_ON_STATE_MACHINE, trAppOptionValue.CURRENT);

        addOptionValue(trAppOption.CLIPBOARD, trAppOptionValue.HIDE, true);
        addOptionValue(trAppOption.CLIPBOARD, trAppOptionValue.SHOW);
        
        addOptionValue(trAppOption.UNLOCK_SHARING, trAppOptionValue.YES);
        addOptionValue(trAppOption.UNLOCK_SHARING, trAppOptionValue.NO, true);
        
        addOptionValue(trAppOption.UNLOCK_FREEPLAY, trAppOptionValue.YES);
        addOptionValue(trAppOption.UNLOCK_FREEPLAY, trAppOptionValue.NO, true);

        addOptionValue(trAppOption.UNLOCK_COMMUNITY, trAppOptionValue.YES);
        addOptionValue(trAppOption.UNLOCK_COMMUNITY, trAppOptionValue.NO, true);

        addOptionValue(trAppOption.UNLOCK_FUNCTIONS, trAppOptionValue.YES);
        addOptionValue(trAppOption.UNLOCK_FUNCTIONS, trAppOptionValue.NO, true);

        addOptionValue(trAppOption.UNLOCK_ALL_REWARDS, trAppOptionValue.YES);
        addOptionValue(trAppOption.UNLOCK_ALL_REWARDS, trAppOptionValue.NO, true);
        
        addOptionValue(trAppOption.UNLOCK_ALL_CUE_REWARDS, trAppOptionValue.YES);
        addOptionValue(trAppOption.UNLOCK_ALL_CUE_REWARDS, trAppOptionValue.NO, true);
        
        addOptionValue(trAppOption.UNLOCK_CONTROLLER, trAppOptionValue.YES, true);
        addOptionValue(trAppOption.UNLOCK_CONTROLLER, trAppOptionValue.NO);

        addOptionValue(trAppOption.UNLOCK_INTERNAL_ANIMATIONS, trAppOptionValue.YES);
        addOptionValue(trAppOption.UNLOCK_INTERNAL_ANIMATIONS, trAppOptionValue.NO, true);

        
        addOptionValue(trAppOption.UNLOCK_EXPERIMENTAL_TRIGGERS, trAppOptionValue.YES);
        addOptionValue(trAppOption.UNLOCK_EXPERIMENTAL_TRIGGERS, trAppOptionValue.NO, true);
        
        addOptionValue(trAppOption.MULTIBOT_TRIGGERS, trAppOptionValue.YES);
        addOptionValue(trAppOption.MULTIBOT_TRIGGERS, trAppOptionValue.NO, true);
        
        addOptionValue(trAppOption.BUMPER_MODE, trAppOptionValue.YES, true);
        addOptionValue(trAppOption.BUMPER_MODE, trAppOptionValue.NO);

        addOptionValue(trAppOption.DEBUG_TRANSLATION_MODE, trAppOptionValue.YES);
        addOptionValue(trAppOption.DEBUG_TRANSLATION_MODE, trAppOptionValue.NO, true);

        addOptionValue(trAppOption.TRANSITION_LINE_QUALITY, trAppOptionValue.VERY_VERY_LOW);
        addOptionValue(trAppOption.TRANSITION_LINE_QUALITY, trAppOptionValue.VERY_LOW);
        addOptionValue(trAppOption.TRANSITION_LINE_QUALITY, trAppOptionValue.LOW);
        addOptionValue(trAppOption.TRANSITION_LINE_QUALITY, trAppOptionValue.NORMAL, true);
        addOptionValue(trAppOption.TRANSITION_LINE_QUALITY, trAppOptionValue.VERY_HIGH);

        addOptionValue(trAppOption.SHOW_FPS_METER, trAppOptionValue.YES);
        addOptionValue(trAppOption.SHOW_FPS_METER, trAppOptionValue.NO, true);

        addOptionValue(trAppOption.SHOW_VIDEO_TIMESTAMP, trAppOptionValue.YES);
        addOptionValue(trAppOption.SHOW_VIDEO_TIMESTAMP, trAppOptionValue.NO, true);

        addOptionValue(trAppOption.PERF_STATES, trAppOptionValue.SHOW, true);
        addOptionValue(trAppOption.PERF_STATES, trAppOptionValue.HIDE);

        addOptionValue(trAppOption.PERF_TRIGGERS, trAppOptionValue.SHOW, true);
        addOptionValue(trAppOption.PERF_TRIGGERS, trAppOptionValue.HIDE);

        addOptionValue(trAppOption.PERF_LINES, trAppOptionValue.SHOW, true);
        addOptionValue(trAppOption.PERF_LINES, trAppOptionValue.HIDE);
        
        addOptionValue(trAppOption.FORCE_BAD_FRAMERATE_IN_EDITOR, trAppOptionValue.YES, true);
        addOptionValue(trAppOption.FORCE_BAD_FRAMERATE_IN_EDITOR, trAppOptionValue.NO);
        
        addOptionValue(trAppOption.MUTE_SOUND_AND_ANIMS, trAppOptionValue.NO, true);
        addOptionValue(trAppOption.MUTE_SOUND_AND_ANIMS, trAppOptionValue.YES);

        addOptionValue(trAppOption.SHOW_LAUNCH_ACTIONS, trAppOptionValue.SHOW);
        addOptionValue(trAppOption.SHOW_LAUNCH_ACTIONS, trAppOptionValue.HIDE, true);
        
        addOptionValue(trAppOption.USE_POSE_BASED_LINANG, trAppOptionValue.YES);
        addOptionValue(trAppOption.USE_POSE_BASED_LINANG, trAppOptionValue.NO, true);

        addOptionValue(trAppOption.VAULT_ALLOW_INTERNAL, trAppOptionValue.YES);
        addOptionValue(trAppOption.VAULT_ALLOW_INTERNAL, trAppOptionValue.NO, true);

        addOptionValue(trAppOption.STATE_MACHINE_GRID, trAppOptionValue.YES, true);
        addOptionValue(trAppOption.STATE_MACHINE_GRID, trAppOptionValue.NO);

        addOptionValue(trAppOption.UNLOCK_PUPPET, trAppOptionValue.YES);
        addOptionValue(trAppOption.UNLOCK_PUPPET, trAppOptionValue.NO, true);

        addOptionValue(trAppOption.UNLOCK_ANIM_SHOP, trAppOptionValue.YES);
        addOptionValue(trAppOption.UNLOCK_ANIM_SHOP, trAppOptionValue.NO, true);

        addOptionValue(trAppOption.ELEMENT_INFO_PANEL, trAppOptionValue.YES);
        addOptionValue(trAppOption.ELEMENT_INFO_PANEL, trAppOptionValue.NO, true);

        addOptionValue(trAppOption.SCREENSHOTS, trAppOptionValue.YES);
        addOptionValue(trAppOption.SCREENSHOTS, trAppOptionValue.NO, true);

        addOptionValue(trAppOption.RESET_TUTORIAL_SLIDES_DATA, trAppOptionValue.YES);
        addOptionValue(trAppOption.RESET_TUTORIAL_SLIDES_DATA, trAppOptionValue.NO, true);

        addOptionValue(trAppOption.REGENERATE_ALL_THUMBNAILS, trAppOptionValue.YES);
        addOptionValue(trAppOption.REGENERATE_ALL_THUMBNAILS, trAppOptionValue.NO, true);

        addOptionValue(trAppOption.DEBUG_MODE, trAppOptionValue.YES);
        addOptionValue(trAppOption.DEBUG_MODE, trAppOptionValue.NO, true);

        addOptionValue(trAppOption.SHOW_TOUCH_CURSOR, trAppOptionValue.YES);
        addOptionValue(trAppOption.SHOW_TOUCH_CURSOR, trAppOptionValue.NO, true);

        Load();

        WWLog.logDebug("current app options: " + ToJSON().ToString());
      }
    }

    private void Load(){
      string data = wwDataSaveLoadManager.Instance.Load(localFilename);
      if (data != null){
        FromJson(JSON.Parse(data));
      }
    }

    private void Save(){
      wwDataSaveLoadManager.Instance.Save(ToJSON().ToString(), localFilename);
    }
    
    public JSONClass ToJSON() {
      JSONClass jsc = new JSONClass();
      foreach (trAppOption opt in System.Enum.GetValues(typeof(trAppOption))) {
        if (optionSettings.ContainsKey(opt)) {
          jsc[opt.ToString()] = getOptionValue(opt).ToString();
        }
      }
      return jsc;
    }

    private void FromJson(JSONNode js){
      foreach (trAppOption opt in System.Enum.GetValues(typeof(trAppOption))) {
        trAppOptionValue optionValue;
        if (piStringUtil.ParseStringToEnum<trAppOptionValue>(js[opt.ToString()], out optionValue)) {
          optionSettings[opt] = optionValue;
        }
      }
    }

    void addOptionValue(trAppOption opt, trAppOptionValue val) {
      addOptionValue(opt, val, false);
    }
        
    void addOptionValue(trAppOption opt, trAppOptionValue val, bool andSet) {
      if (!optionDefinitions.ContainsKey(opt)) {
        optionDefinitions[opt] = new List<trAppOptionValue>();
      }
      List<trAppOptionValue> vals = optionDefinitions[opt];
      if (vals.Contains(val)) {
        WWLog.logError("duplicate option value. opt = " + opt.ToString() + "  val = " + val.ToString());
      }
      else {
        vals.Add(val);
      }
      
      if (andSet) {
        optionSettings[opt] = val;
      }
    }
    
    public bool isUsedOption(trAppOption opt) {
      return optionSettings.ContainsKey(opt);
    }
    
    public trAppOptionValue getOptionValue(trAppOption opt) {
      init();
      
      if (!optionSettings.ContainsKey(opt)) {
        // set the current setting to the first available option.
        if (!optionDefinitions.ContainsKey(opt)) {
          WWLog.logError("unknown option: " + opt.ToString());
          return trAppOptionValue.UNDEFINED;
        }
        List<trAppOptionValue> vals = optionDefinitions[opt];
        if (vals.Count == 0) {
          WWLog.logError("no values defined for option: " + opt.ToString());
          return trAppOptionValue.UNDEFINED;
        }
        
        optionSettings[opt] = vals[0];
      }
      
      return optionSettings[opt];
    }
    
    public List<trAppOptionValue> getPossibleValues(trAppOption trAO) {
      init();
      
      if (!optionDefinitions.ContainsKey(trAO)) {
        WWLog.logError("unknown app option: " + trAO.ToString());
        return new List<trAppOptionValue>();
      }
      
      List<trAppOptionValue> possibleVals = optionDefinitions[trAO];
      
      return possibleVals;
    }
    
    public void setOptionValue(trAppOption opt, trAppOptionValue val) {
      // validate value
      // this calls init() and also validates the option itself.
      if (!getPossibleValues(opt).Contains(val)) {
        WWLog.logError("invalid value for option! opt = " + opt.ToString() + "  val = " + val.ToString());
        return;
      }
      
      WWLog.logInfo("setting app option. opt = " + opt.ToString() + " val = " + val.ToString());
      bool shouldSave = (val != optionSettings[opt]);
      optionSettings[opt] = val;      
      if (shouldSave){
        Save();
      }
      if (ValueChanged != null){
        ValueChanged(opt, val);
      }
    }
    
    public void incrementOptionValue(trAppOption trAO) {
      List<trAppOptionValue> possibleValues = getPossibleValues(trAO);
      trAppOptionValue currentValue = getOptionValue(trAO);
      int index = possibleValues.IndexOf(currentValue);
      if (index < 0) {
        WWLog.logError("inconsistent state. for " + trAO.ToString() + ", current value not found: " + currentValue.ToString());
        return;
      }
      
      int newIndex = (index + 1) % possibleValues.Count;
      setOptionValue(trAO, possibleValues[newIndex]);
    }
    
    public static bool isYES(trAppOption trAO) {
      return (Instance.getOptionValue(trAO) == trAppOptionValue.YES);
    }
    public static bool isYESorSHOW(trAppOption trAO) {
      trAppOptionValue trAOV = Instance.getOptionValue(trAO);
      return ((trAOV == trAppOptionValue.YES) || (trAOV == trAppOptionValue.SHOW));
    }
  }
}
