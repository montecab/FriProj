using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using WW.SimpleJSON;
using WW.SaveLoad;
using System.IO;


namespace Turing{
  public class trVideoManager{

    public Dictionary<trTriggerType, trVideoInfo> triggerVideos = new Dictionary<trTriggerType, trVideoInfo>();
    public Dictionary<trBehaviorType, trVideoInfo> behaviorVideos = new Dictionary<trBehaviorType, trVideoInfo>();
    public Dictionary<string, trSubtitleInfo> subtitles = new Dictionary<string, trSubtitleInfo>();
    public HashSet<string> Videos = new HashSet<string>();

    private string videoDocPath = "";
    private string videosFolderPath = "";

    public void Load(){
      if(trDataManager.Instance.AuthoringMissionInfo.IsLoadUserFolder){
        LoadUserFacingVideo();
      }else{
        LoadAuthoringVideos();
      }
      LoadSubtitles();
    }

    public void Save(){
      wwDataSaveLoadManager.Instance.Save(this.ToJson().ToString(),videoDocPath );
    }

    public void LoadUserFacingVideo(){
      TextAsset Appinfo = Resources.Load ("TuringProto/HintVideos", typeof(TextAsset)) as TextAsset;
      if(Appinfo != null){
        JSONClass jsc = (JSON.Parse(Appinfo.text)).AsObject;
        FromJson(jsc);
      }
    }

    public void LoadSubtitles(){
      TextAsset Appinfo = Resources.Load ("TuringProto/Subtitles", typeof(TextAsset)) as TextAsset;
      if(Appinfo != null){
        JSONClass jsc = (JSON.Parse(Appinfo.text)).AsObject;
        subtitles.Clear();
        foreach (JSONNode jsM in jsc[TOKENS.SUBTITLES].AsArray){
          trSubtitleInfo info = trSubtitleInfo.FromJson(jsM);
          subtitles.Add(info.FileName, info);
        }
      }
    }

    public string GetVideoPath(string fileName){
      if(trDataManager.Instance.AuthoringMissionInfo.IsLoadUserFolder){
        return fileName;
      }
      else{
        return videosFolderPath + "/" + fileName;
      }
    }

    public void LoadAuthoringVideos(){
      if(!trDataManager.Instance.MissionMng.AuthoringMissionInfo.IsPathValid()){
        return;
      }
        
      Videos.Clear();

      DirectoryInfo dir = new DirectoryInfo(trDataManager.Instance.MissionMng.AuthoringMissionInfo.Path);

      DirectoryInfo dirV = dir.Parent;
      videoDocPath = dirV.FullName + "/HintVideos.json";
      string data = wwDataSaveLoadManager.Instance.Load(videoDocPath);
      if(data != null){
        JSONClass jsc = (JSON.Parse(data)).AsObject;
        FromJson(jsc);
      }

      DirectoryInfo videoDir = dir.Parent.Parent.Parent;
      videosFolderPath = videoDir.FullName + "/StreamingAssets";
      videoDir = new DirectoryInfo(videosFolderPath);
      FileInfo[] files = videoDir.GetFiles("*.mov");

      foreach(FileInfo file in files){
        if(Videos.Contains(file.Name)){
          WWLog.logError("Duplicate video named: " + file.Name + ". Please rename.");
        }
        else{
          Videos.Add(file.Name);

        }
      }

      files = videoDir.GetFiles("*.mp4");
      foreach(FileInfo file in files){
        if(Videos.Contains(file.Name)){
          WWLog.logError("Duplicate video named: " + file.Name + ". Please rename.");
        }
        else{
          Videos.Add(file.Name);

        }
      }
    }

    void validate(){
      foreach(trVideoInfo videoInfo in triggerVideos.Values){
        if(!Videos.Contains(videoInfo.FileName)){
          WWLog.logError("Video " + videoInfo.FileName + " doesn't exist. Please check.") ;
        }
      }
    }


    public JSONClass ToJson() {
      JSONClass jsc = new JSONClass();
      JSONArray jarray = new JSONArray();      
      foreach (trVideoInfo t in triggerVideos.Values) {
        jarray.Add(t.ToJson());
      }
      jsc[TOKENS.TRIGGER_VIDEOS] =jarray;

      jarray = new JSONArray();      
      foreach (trVideoInfo t in behaviorVideos.Values) {
        jarray.Add(t.ToJson());
      }
      jsc[TOKENS.BEHAVIOR_VIDEOS] =jarray;

      jarray = new JSONArray();
      foreach (trSubtitleInfo t in subtitles.Values){
        jarray.Add(t.ToJson());
      }
      jsc[TOKENS.SUBTITLES] = jarray;

      return jsc;
    }

    public void FromJson(JSONClass jsc) {
      triggerVideos.Clear();  
      foreach (JSONNode jsM in jsc[TOKENS.TRIGGER_VIDEOS].AsArray) {
        trVideoInfo info = trVideoInfo.FromJson(jsM);
        triggerVideos.Add(info.TriggerType, info);
      }

      behaviorVideos.Clear();
      foreach (JSONNode jsM in jsc[TOKENS.BEHAVIOR_VIDEOS].AsArray) {
        trVideoInfo info = trVideoInfo.FromJson(jsM);
        behaviorVideos.Add(info.BehaviorType, info);
      }
    }

  }

  public class trVideoInfo{
    public string FileName;
    public trTriggerType TriggerType = trTriggerType.NONE;
    public trBehaviorType BehaviorType = trBehaviorType.MOVE_F1; // use deprecate behavior type for default behavior

    public static trVideoInfo FromJson(JSONNode js){
      trVideoInfo ret = new trVideoInfo();
      ret.FileName = js[TOKENS.FILE_NAME];
      if(js[TOKENS.TRIGGER] != null){
        ret.TriggerType = (trTriggerType)(js[TOKENS.TRIGGER].AsInt);
      }
      if(js[TOKENS.BEHAVIOR_ID] != null){
        ret.BehaviorType = (trBehaviorType)(js[TOKENS.BEHAVIOR_ID].AsInt);
      }

      return ret;
    }

    public JSONClass ToJson() {
      JSONClass jsc = new JSONClass();
      jsc[TOKENS.FILE_NAME] = FileName; 
      if(TriggerType != trTriggerType.NONE){
        jsc[TOKENS.TRIGGER].AsInt = (int)TriggerType;
      }
      if(BehaviorType != trBehaviorType.MOVE_F1){
        jsc[TOKENS.BEHAVIOR_ID].AsInt = (int)BehaviorType;
      }

      return jsc;
    }
  }

  public class trSubtitleInfo{
    public string FileName;
    public List<trCaptionInfo> Captions = new List<trCaptionInfo>();

    public JSONClass ToJson() {
      JSONClass jsc = new JSONClass();
      jsc[TOKENS.FILE_NAME] = FileName;
      JSONArray jsArray = new JSONArray();      
      for(int i = 0; i<Captions.Count; ++i){
        jsArray.Add(Captions[i].ToJson());
      }
      jsc[TOKENS.CAPTIONS] = jsArray;
      return jsc;
    }
    
    public static trSubtitleInfo FromJson(JSONNode js){
      trSubtitleInfo subtitle = new trSubtitleInfo();
      subtitle.OutOfJson(js);
      return subtitle;
    }
    
    public void OutOfJson(JSONNode js) {
      FileName = js[TOKENS.FILE_NAME];
      foreach (JSONClass jsM in js[TOKENS.CAPTIONS].AsArray) {
        trCaptionInfo caption = trCaptionInfo.FromJson(jsM);
        Captions.Add(caption);
      }
    }
  }

  public class trCaptionInfo{
    public string Caption;
    public float StartTime;
    public float EndTime;

    public trCaptionInfo(){}

    public trCaptionInfo(string c, float st, float et){
      Caption = c;
      StartTime = st;
      EndTime = et;
    }

    public JSONClass ToJson() {
      JSONClass jsc = new JSONClass();
      jsc[TOKENS.CAPTION] = Caption;
      jsc[TOKENS.CAPTION_START].AsFloat = StartTime;
      jsc[TOKENS.CAPTION_END].AsFloat = EndTime;
      return jsc;
    }

    public static trCaptionInfo FromJson(JSONClass js){
      trCaptionInfo caption = new trCaptionInfo();
      caption.OutOfJson(js);
      return caption;
    }

    public void OutOfJson(JSONClass jsc) {
      Caption = jsc[TOKENS.CAPTION];
      StartTime = jsc[TOKENS.CAPTION_START].AsFloat;
      EndTime = jsc[TOKENS.CAPTION_END].AsFloat;
    }
  }
}

