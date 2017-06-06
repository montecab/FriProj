using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using WW.SaveLoad;
using WW.SimpleJSON;

namespace Turing{
  public class trUserFunctions {

    public Dictionary<string, trFunctionBehavior> UuidToBehaviorDic = new Dictionary<string, trFunctionBehavior>();

    private string path;

    private int version = -1;
     

    public static int CurrentVersion = 0;

    public void Init(){
      path = Application.persistentDataPath + "/trUserFunctions.json";
    }

    string getSavePath(string functionUUID){
      return Application.persistentDataPath +  "/fn_" + functionUUID + ".json";
    }

    public List<trFunctionBehavior> GetFunctionsWithRobotType(piRobotType type){
      List<trFunctionBehavior> ret = new List<trFunctionBehavior>();
      foreach(trFunctionBehavior beh in UuidToBehaviorDic.Values){
        if(beh.RobotType == type){
          ret.Add(beh);
        }
      }
      return ret;
    }

    public void UpdateBehaviors(trProgram program){
      foreach(trBehavior behavior in program.UUIDToBehaviorTable.Values){
        if(behavior.Type == trBehaviorType.FUNCTION){
          //program behavior overwrites so the program will work as previously
          if(UuidToBehaviorDic.ContainsKey(behavior.UUID)){
            UuidToBehaviorDic[behavior.UUID] = (trFunctionBehavior)behavior;
            SaveFunction((trFunctionBehavior)behavior);
          }
          else{
            UuidToBehaviorDic.Add(behavior.UUID,(trFunctionBehavior)behavior);
            SaveFunction((trFunctionBehavior)behavior);
          }
        }
       
      }
      Save();
    }


    public void AddBehavior(trFunctionBehavior behavior){
      UuidToBehaviorDic.Add(behavior.UUID, behavior);
      SaveFunction(behavior);
      Save();
    }

    public void RemoveBehavior(trBehavior behavior){
      if(UuidToBehaviorDic.ContainsKey(behavior.UUID)){
        UuidToBehaviorDic.Remove(behavior.UUID);
        wwDataSaveLoadManager.Instance.Clear(getSavePath(behavior.UUID));
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
    }
      
    public void SaveFunction(trFunctionBehavior func){
      string data = func.ToJson().ToString();
      wwDataSaveLoadManager.Instance.Save(data, getSavePath(func.UUID));
    }

    public void Save(){
      Init();
      string data = this.ToJson().ToString();
      wwDataSaveLoadManager.Instance.Save(data, path);
    }

    public JSONClass ToJson(){
      JSONClass js = new JSONClass();
      JSONArray array = new JSONArray();
      foreach(trFunctionBehavior behavior in UuidToBehaviorDic.Values){
        JSONClass jsc = new JSONClass();
        jsc[TOKENS.ID] = behavior.UUID;
        array[array.Count] = jsc;
      }
      js[TOKENS.BEHAVIORS] = array;
      js[TOKENS.VERSION].AsInt = version;
      return js;
    }

    public void FromJson(JSONNode jsc){
      if(jsc[TOKENS.VERSION] != null){
        version = jsc[TOKENS.VERSION].AsInt;
      }

      if(version < 0){
        foreach(JSONClass jsc2 in jsc[TOKENS.BEHAVIORS  ].AsArray) {
          trFunctionBehavior beh = trFactory.FromJson<trFunctionBehavior>(jsc2);       
          UuidToBehaviorDic.Add(beh.UUID, beh);
          SaveFunction(beh);
        }
        Save();
      }
      else{
        foreach(JSONClass jsc2 in jsc[TOKENS.BEHAVIORS  ].AsArray) {
          string uuid = jsc2[TOKENS.ID];
          string data = wwDataSaveLoadManager.Instance.Load(getSavePath(uuid));
          JSONNode js = JSON.Parse(data);
          trFunctionBehavior beh = trFactory.FromJson<trFunctionBehavior>(js);       
          UuidToBehaviorDic.Add(beh.UUID, beh);
        }
      }

      if(version != CurrentVersion){
        version = CurrentVersion;
        Save();
      }
     
    }
  }

}
