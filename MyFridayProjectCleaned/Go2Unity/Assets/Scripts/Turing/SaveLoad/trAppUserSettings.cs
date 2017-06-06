using UnityEngine;
using System.Collections;
using WW.SimpleJSON;
using System.Collections.Generic;
using WW.SaveLoad;

namespace Turing{
  public class trAppUserSettings {
    public Dictionary<string, trBehavior> UuidToBehaviorDic = new Dictionary<string, trBehavior>();

    private string path;

    public void Init(){
      path = Application.persistentDataPath + "/trAppUserSettings.json";
    }

    public void UpdateBehaviors(trProgram program){
     foreach(trBehavior behavior in program.UUIDToBehaviorTable.Values){
        if(!behavior.IsMissionBehavior && !UuidToBehaviorDic.ContainsKey(behavior.UUID)){
          UuidToBehaviorDic.Add(behavior.UUID, behavior);
        }
        //program behavior overwrites so the program will work as previously
        if(UuidToBehaviorDic.ContainsKey(behavior.UUID)){
          UuidToBehaviorDic[behavior.UUID] = behavior;
        }
      }
      Save();
    }


    public void AddBehavior(trBehavior behavior){
      if(behavior.IsMissionBehavior){
        WWLog.logError("Trying to save mission behavior in user setting. Very wrong!");
        return;
      }
      UuidToBehaviorDic.Add(behavior.UUID, behavior);
      Save();
    }

    public void RemoveBehavior(trBehavior behavior){
      if(UuidToBehaviorDic.ContainsKey(behavior.UUID)){
        UuidToBehaviorDic.Remove(behavior.UUID);
      }
      else{
        WWLog.logError("Trying to remove a behavior that's not existed.");
      }

      Save();
    }

    public void Load(){
      Init();
      string data = wwDataSaveLoadManager.Instance.Load(path);
      if(data == null){
        Save();
      }
      else{
        JSONNode js = JSON.Parse(data);
        FromJson(js);
      }     

      foreach(trBehavior behavior in trBehavior.DefaultBehaviors.Values){
        if(behavior.Type  == trBehaviorType.MAPSET){
          if(!UuidToBehaviorDic.ContainsKey(behavior.UUID)){
            UuidToBehaviorDic.Add(behavior.UUID, behavior);
          }
        }       
      }
      Save();
    }

    public void Save(){
      Init();
      string data = this.ToJson().ToString();
      wwDataSaveLoadManager.Instance.Save(data, path);
    }

    public JSONClass ToJson(){
      JSONClass js = new JSONClass();
      JSONArray array = new JSONArray();
      foreach(trBehavior behavior in UuidToBehaviorDic.Values){
        array[array.Count] = behavior.ToJson();
      }
      js[TOKENS.BEHAVIORS] = array;
      return js;
    }
    
    public void FromJson(JSONNode jsc){
      foreach(JSONClass jsc2 in jsc[TOKENS.BEHAVIORS  ].AsArray) {
        trBehavior beh = trFactory.FromJson<trBehavior>(jsc2);
        if(!beh.IsMissionBehavior && beh.Type == trBehaviorType.MAPSET){
          UuidToBehaviorDic.Add(beh.UUID, beh);
        }
      }
    }

   
  }
}

