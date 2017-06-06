using UnityEngine;
using System.Collections.Generic;
using WW.SimpleJSON;
using System;

namespace Turing {

  // please keep these alphabetical
  public static class TOKENS {
    public const string ACTIVATION_TIME         = "activation_time";
    public const string ACTIVE                  = "active";
    public const string ACTUATOR_RANGE          = "actuator_range";
    public const string ACTUATOR_POINT          = "actuator_point";
    public const string ACTUATOR_TYPE           = "actuator_type";
    public const string BEHAVIORS               = "behaviors";
    public const string BEHAVIOR_ID             = "behavior_id";
    public const string BEHAVIOR_VIDEOS         = "behavior_videos";
    public const string CENTER_PROGRAM          = "center_program";
    public const string COLOR                   = "color";
    public const string COMMENT                 = "comment";
    public const string COMPLETE                = "completed";
    public const string COMPLETE_ONCE           = "completed_once";
    public const string CHECK_STATE_PARA        = "check_state_para";
    public const string CHECK_TRIGGER_PARA      = "check_trigger_para";
    public const string COMMUNITY_UNLOCKED      = "community_unlocked";
    public const string COMMUNITY_VISITED       = "community_visited";
    public const string DESCRIPTION             = "description";
    public const string DISTANCE_LEVEL          = "distance_level";
    public const string FILE_NAME               = "file_name";
    public const string READY_TO_TRANSLATE      = "ready_to_translate";
    public const string FREEPLAY_UNLOCKED       = "freeplay_unlocked";
    public const string FREEPLAY_VISITED        = "freeplay_visited";
    public const string FUNCTIONS               = "functions";
    public const string FUNCTION_PROGRAM        = "function_program";
    public const string HINTS                   = "hints";
    public const string HINT_INDEX              = "hint_index";
    public const string ICON_NAME               = "icon_name";
    public const string ID                      = "id";
    public const string INTRODUCTION            = "introduction";
    public const string INTROOUTRO_MODE         = "introoutro";
    public const string IQ_POINTS               = "iq_points";
    public const string LAYOUT_POSITION         = "layout_position";
    public const string LOADED_TIME             = "loaded_time";
    public const string LOAD_START_PROGRAM      = "load_startSM";
    public const string MAPS                    = "maps";
    public const string MAP_SET                 = "map_set";
    public const string MATCH                   = "match";
    public const string MISSION                 = "mission";
    public const string MISSIONS                = "missions";
    public const string MOOD                    = "mood";
    public const string MOODY_ANIMATION         = "moody_animation";
    public const string OBSCURED                = "obscured";
    public const string OUTGOING_TRANSITIONS    = "outgoing_transitions";
    public const string OUTRO_DESCRIPTION       = "outro_description";
    public const string PARAMETERS              = "parameters";
    public const string PARAMETER_VALUE         = "parameter_value";
    public const string PARAMETER_VALUES        = "parameter_values";
    public const string PARENT_TOKEN            = "parent_token";
    public const string PATH                    = "path";
    public const string PROGRAM                 = "program";
    public const string PROGRAM_FOR_RESET       = "profram_reset";
    public const string PROGRESS                = "progress";
    public const string PUZZLE_ID_RESET         = "puzzle_index_reset";
    public const string PUZZLES                 = "puzzles";
    public const string PUZZLE_INDEX            = "puzzle_index";
    public const string RECEIVERS               = "receivers";
    public const string RECENT_PLAYED           = "recent_played";
    public const string RECENTLY_SOLVED         = "recently_solved";
    public const string ROBOT_TYPE              = "robot_type";
    public const string SENSOR_INVERT           = "sensor_invert";
    public const string SENSOR_RANGE            = "sensor_range";
    public const string SENSOR_TYPE             = "sensor_type";
    public const string SHOW_MISSION            = "show_mission";
    public const string SHOW_INTRODUCTION       = "show_introduction";
    public const string SIMPLE_MODE             = "simple_mode";
    public const string SPRITE_INTRO            = "sprite_intro";
    public const string SPRITE_OUTRO            = "sprite_outro";
    public const string START_REPORTED          = "start_reported";
    public const string STATES                  = "states";   
    public const string STATE_OMNI_ID           = "state_omni_id";
    public const string STATE_SOURCE_ID         = "state_source_id";
    public const string STATE_START_ID          = "state_start_id";
    public const string STATE_TARGET_ID         = "state_target_id";
    public const string THUMBNAIL_DIRTY         = "thumbnail_dirty";
    public const string TRANSITIONS             = "transitions";
    public const string TRIGGER                 = "trigger";
    public const string TRIGGERS                = "triggers";
    public const string TRIGGER_SET             = "trigger_set";
    public const string TRIGGER_VIDEOS          = "trigger_videos";
    public const string TRANSFER_ROBOT_TUT      = "transfer_robot_tut";
    public const string TUTORIAL_MISSION        = "tutorial_mission";
    public const string TYPE                    = "type";
    public const string USER_FACING_NAME        = "user_facing_name";
    public const string VERSION                 = "version";
    public const string VIDEO_START             = "video_start";
    public const string VIDEO_END               = "video_end";
    public const string X                       = "x";
    public const string Y                       = "y";
    public const string SUBTITLES               = "subtitles";
    public const string CAPTIONS                = "captions";
    public const string CAPTION                 = "caption";
    public const string CAPTION_START           = "caption_start";
    public const string CAPTION_END             = "caption_end";

    //RewardsDefinitions
    public const string INTERNAL_NAME           = "internal_name";
    public const string IQ_POINTS_REQUIRED      = "min_points";
    public const string PAYLOAD                 = "payload";
    public const string DURABLES                = "durables";
    public const string CATEGORY                = "category";
    public const string STORY_PLAY_COUNT        = "anim_video_view_count";
  }

  public static class trFactory  {
  
    private static Dictionary<string, trBase> itemDictionary = new Dictionary<string, trBase>();

    private static T tryDeserialize<T>(JSONClass jsc) where T : trBase, new() {
      return trBase.FromJson<T>(jsc);
    }
  
    public static T FromJson<T>(JSONNode jsn) where T:trBase, new() {
      if (!(jsn is JSONClass)) {
        WWLog.logError("invalid json type send to FromJson: " + jsn.GetType().ToString());
        return null;
      }
      
      JSONClass jsc = (JSONClass)jsn;
      
      
      T ret = tryDeserialize<T>(jsc);
      
      if (ret != null) {
        if (!HasItem(ret.UUID)) {
          AddItem(ret);
        }
      }
      
      return ret;
    }
    
    public static JSONClass ToJson(Vector2 vec) {
      JSONClass jsc = new JSONClass();
      jsc[TOKENS.X].AsFloat = vec.x;
      jsc[TOKENS.Y].AsFloat = vec.y;
      return jsc;
    }
    
    public static Vector2 FromJson(JSONClass jsc) {
      return new Vector2(jsc[TOKENS.X].AsFloat, jsc[TOKENS.Y].AsFloat);
    }
    
    public static void ForgetItems() {
      itemDictionary.Clear();
    }
    
    public static void AddItem(trBase item) {
      if (itemDictionary.ContainsKey(item.UUID)) {
        WWLog.logWarn("attempt to add item to dictionary twice. type = " + item.GetType().Name + "  ID = " + item.UUID);
      }
      else {
        itemDictionary.Add(item.UUID, item);        
      }
    }
    
    public static bool HasItem(string UUID) {
      return itemDictionary.ContainsKey(UUID);
    }
    
    public static T GetItem<T>(string UUID) where T:trBase {
      if (HasItem(UUID)) {
        return itemDictionary[UUID] as T;
      }
      else {
        WWLog.logWarn("item not found. type = " + typeof(T).Name + "  ID = " + UUID);
        return null;
      }
    }
  }
}
