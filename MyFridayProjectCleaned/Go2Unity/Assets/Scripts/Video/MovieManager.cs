using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using System.Collections.Generic;
using WW.SimpleJSON;
using Turing;

[System.Serializable]
public class ClipInfo {
  public string Caption;
  public float  Duration;

  public const string CAPTION = "caption";
  public const string DURATION = "duration";

  public JSONClass ToJson() {
    JSONClass jsc = new JSONClass();
    jsc[CAPTION] = Caption;
   
    jsc[DURATION].AsFloat = Duration;
    return jsc;
  }

  public static ClipInfo FromJson(JSONClass js){
    ClipInfo clip = new ClipInfo();
    clip.OutOfJson(js);
    return clip;
  }

  public void OutOfJson(JSONClass jsc) {
    Caption = jsc[CAPTION];
    Duration = jsc[DURATION].AsFloat;
  }
}

public class MovieInfo{
  public string Name;
  public string UUID;
  public string IconName;
  public string ImageName;
  public string FileName;
  public List<ClipInfo> Clips = new List<ClipInfo>();

  public const string NAME = "name";
  public const string CLIPS = "clips";
  public const string UID = "uuid";
  public const string ICON_NAME = "icon";
  public const string IMAGE_NAME = "image";
  public const string FILENAME = "filename";

  public JSONClass ToJson() {
    JSONClass jsc = new JSONClass();
    jsc[NAME] = Name;
    jsc[UID] = UUID;
    jsc[ICON_NAME] = IconName;
    jsc[IMAGE_NAME] = ImageName;
    jsc[FILENAME] = FileName;

    JSONArray jsArray = new JSONArray();      
    for(int i = 0; i<Clips.Count; ++i){
      jsArray.Add(Clips[i].ToJson());
    }
    jsc[CLIPS] = jsArray;
    return jsc;
  }
  
  public static MovieInfo FromJson(JSONClass js){
    MovieInfo movie = new MovieInfo();
    movie.OutOfJson(js);
    return movie;
  }
  
  public void OutOfJson(JSONClass jsc) {
    Name = jsc[NAME];
    UUID = jsc[UID];
    FileName = jsc[FILENAME];
    IconName = jsc[ICON_NAME];
    ImageName = jsc[IMAGE_NAME];
    foreach (JSONClass jsM in jsc[CLIPS].AsArray) {
      ClipInfo clip = ClipInfo.FromJson(jsM);
      Clips.Add(clip);
    }
  }
}

public class MovieManager : Singleton<MovieManager>{
  private string path = "";

  public Dictionary<string, MovieInfo> UUIDToMovieTable = new Dictionary<string, MovieInfo>();

  public const string MOVIES = "movies";

  public void Load(string p = ""){
    if(!string.IsNullOrEmpty(p)){
      path = p;
    }

    TextAsset info = Resources.Load (path, typeof(TextAsset)) as TextAsset;
    if(info != null){
      JSONClass jsc = (JSON.Parse(info.text)).AsObject;
      FromJson(jsc);
    }
  }

  public void FromJson(JSONClass js){
    foreach (JSONClass jsM in js[MOVIES].AsArray) {
      MovieInfo movie = MovieInfo.FromJson(jsM);
      UUIDToMovieTable.Add(movie.UUID, movie);
    }
  }

  public JSONClass ToJson(){
    JSONClass js = new JSONClass();
    JSONArray jsArray = new JSONArray();      
    foreach(string uuid in UUIDToMovieTable.Keys){
      jsArray.Add(UUIDToMovieTable[uuid].ToJson());
    }
    js[MOVIES] = jsArray;
    return js;
  }

  public MovieInfo GetMovieInfo(string uuid){
    return UUIDToMovieTable.ContainsKey(uuid) ? UUIDToMovieTable[uuid] : null;
  }

}


