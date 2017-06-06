using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using Turing;
using System;

// todo: de-duplicate all this code w/ trIconFactory.

public class trImageFactory {
  
  static private Dictionary<string        , string> storyImages = null;
  static private Dictionary<string        , Sprite> spriteDic         =  new Dictionary<string, Sprite>();
  
  static public string[] StoryImageNames = new string[]{
    "Reward_1_image",
    "Reward_2_image",
    "Reward_3_image",
    "Reward_4_image",
    "Reward_5_image",
    "reward_default_image"
  };

  static private void loadSprites<T>(Dictionary<T, string> dic){
    foreach(string path in dic.Values){
      Sprite sprite = Resources.Load<Sprite>(path);
      if(spriteDic.ContainsKey(path)){
        continue;
      }
      spriteDic.Add(path, sprite);
      if (sprite == null) {
        WWLog.logError("could not find sprite for " + path);
      }
    }
  }
  
  static private Sprite getSprite<T>(Dictionary<T, string> dict, T value) {
    if (dict == null) {
      return null;
    }
    
    string path;
    
    if (!dict.ContainsKey(value)) {
      if (wwDoOncePerTypeVal<T>.doIt(value)) {
        WWLog.logError("no image for " + typeof(T).Name + "." + value.ToString() + " - using unknown.");
      }
      path = "Sprites/Triggers/eventUnknown";
    }
    else {
      path = dict[value];
    }
    
    if(spriteDic.ContainsKey(path)){
      return spriteDic[path];
    }
    else{
      Sprite ret = Resources.Load<Sprite>(path);
      spriteDic.Add(path, ret);
      if (ret == null) {
        WWLog.logError("could not find sprite for " + path);
      }
      
      return ret;
    }
   
  }
  
  static public Sprite GetStoryImage(string value){
    createStoryImagesDic();    
    return getSprite(storyImages, value);
  }

  static private void createStoryImagesDic(){
    if(storyImages == null){
      storyImages = new Dictionary<string, string>();
      string prefix = "Sprites/StoryImages/";
      for(int i = 0; i < StoryImageNames.Length; ++i){
        string name = StoryImageNames[i];
        storyImages[name] = prefix + name;
      }
    }
  }

}


