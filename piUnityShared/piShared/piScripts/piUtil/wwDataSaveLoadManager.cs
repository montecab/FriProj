using UnityEngine;
using System.Collections;
using System.IO;
using WW.SimpleJSON;

namespace WW.SaveLoad{
  public class wwDataSaveLoadManager: Singleton<wwDataSaveLoadManager> {    

    public void Save(string content, string path){    
      StreamWriter stream = File.CreateText (path);
      stream.Write (content);
      stream.Close (); 
    }
    
    public string Load(string path){
      if (File.Exists (path)) {
        WWLog.logDebug("loading from: " + path);
        StreamReader stream = new StreamReader(path);
        if (stream != null) {
          string contents = stream.ReadToEnd();
          stream.Close();
          if (!string.IsNullOrEmpty(contents)) {
            return contents;
          }
          else {
            WWLog.logError("bad contents: " + path);
          }
        }
        else {
          WWLog.logError("failed to load path: " + path);
        }
      }
      WWLog.logWarn("failed to find file: " + path);
      return null;
    }
    
    public void Clear(string path){
      if (File.Exists (path)) {
        File.Delete(path);
        WWLog.logInfo("Data Cleared: " + path);
      }
      else{
        WWLog.logInfo("Trying to clear data not existed: " + path);
      }
    }

  }

  public class wwDataSaveLoadManagerStatic {    
    
    public static void Save(string content, string path){    
      StreamWriter stream = File.CreateText (path);
      stream.Write (content);
      stream.Close (); 
    }
    
    public static bool Exists(string path) {
      return File.Exists(path);
    }
    
    public static string Load(string path, bool silent = false){
      if (Exists (path)) {
        if (!silent) {
          WWLog.logDebug("loading from: " + path);
        }
        StreamReader stream = new StreamReader(path);
        if (stream != null) {
          string contents = stream.ReadToEnd();
          stream.Close();
          if (!string.IsNullOrEmpty(contents)) {
            return contents;
          }
          else {
            if (!silent) {
              WWLog.logError("bad contents: " + path);
            }
          }
        }
        else {
          if (!silent) {
            WWLog.logError("failed to load path: " + path);
          }
        }
      }
      if (!silent) {
        WWLog.logWarn("failed to find file: " + path);
      }
      return null;
    }
    
    public static void Clear(string path){
      if (File.Exists (path)) {
        File.Delete(path);
        WWLog.logInfo("Data Cleared: " + path);
      }
      else{
        WWLog.logInfo("Trying to clear data not existed: " + path);
      }
    }
    
  }
  
}
