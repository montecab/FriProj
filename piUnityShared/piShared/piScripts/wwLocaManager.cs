using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using WW.SaveLoad;

public class wwLocaManager{
  public const string LOCA_LOOKUP_LANG_UNKNOWN = "LOCA_LOOKUP_LANG_UNKNOWN";  // app attempted to load an unknown language (error)
  public const string LOCA_LOOKUP_LANG_LOADED = "LOCA_LOOKUP_LANG_LOADED";    // app loaded some language. (too frequent ? once per session or more)
  public const string LOCA_LOOKUP_STRING_MISS = "LOCA_LOOKUP_STRING_MISS";    // app missed a string lookup
  public const string LOCA_LANG = "device_lang";
  public const string LOCA_LOAD_FROM_SOURCE = "load_from_resource";
  public const string LOCA_MISSING_KEY = "missing_string";
  public const string LOCA_COMPONENT = "component";

  public static readonly string DO_NOT_TRANSLATE = "DoNotTranslate";

  private static readonly string token_extraction        = "@!@";
  private static readonly string token_extraction_plural = "@#@";
  public  static readonly string token_do_not_translate  = "@noloc@";
  public  static readonly string defaultLangage          = "en_US";

  private wwPO po;
  private string language = "uninitialized";
  private string appName;
  private string version;
  private string savePath;

  public static string overrideLanguage = null;

  private HashSet<string> poFilesIngested;

  public wwLocaManager(){
    WWLog.logInfo("current system language = " + getSystemTextLanguage());
  }

  public static string getSystemTextLanguage() {
    if (string.IsNullOrEmpty(overrideLanguage)) {
      return PIBInterface.Actions.getCanonicalTextLanguage();
    }
    else {
      return overrideLanguage;
    }
  }

  public void Init(string appName, string version, string path){
    this.appName = appName;
    this.version = version;
    this.savePath = path;
    piConnectionManager.Instance.startDownloadLocaFiles(appName, version, true, "", savePath);
  }

  public void SetLanguage(string lang, string poResourceFolder, string downloadPath) {
    if (lang == language) {
      return;
    }

    if (ParsePOFolder(lang, poResourceFolder, downloadPath)) {
      language = lang;
    }
    else {
      Dictionary<string, string> paramDict = new Dictionary<string, string>();
      paramDict.Add(LOCA_LANG, language);
      paramDict.Add(LOCA_COMPONENT, appName);
      FlurryAgent.Instance.logEvent(LOCA_LOOKUP_LANG_UNKNOWN, paramDict);

      if (lang == defaultLangage) {
        // uh oh.
        WWLog.logError("the default language has no PO files! \"" + lang + "\"");
      }
      else {
        // let's try the default language.
        WWLog.logWarn("unhandled language: \"" + lang + "\", trying \"" + defaultLangage + "\".");
        SetLanguage(defaultLangage, poResourceFolder, downloadPath);
      }
    }
  }

  public string Format(int pluralVal, string key, params object[] args){
    bool unused;
    return Format(pluralVal, key, out unused, args);
  }

  public string Format(int pluralVal, string key, out bool isFound, params object[] args){
    isFound = false;
    warnIfNoContents();
    key = key.Replace(token_extraction, "");
	key = key.Trim(new char[] { ' ','\t'});
    if (string.IsNullOrEmpty(key)) {
      isFound = true;
      return "";
    }
    string result = key;
    string[] array = result.Split(new string[]{ token_extraction_plural }, System.StringSplitOptions.None);
    if (array.Length == 2){
      if (po.dictionary.ContainsKey(array[0])){
        result = po.dictionary[array[0]].translations[po.formula.GetPluralIndex(pluralVal)];
        result = string.Format(result, args);
        isFound = true;
      } else{
        string errMsg = "Can't find key:\"" + array[0] + "\"";
        if (wwDoOncePerTypeVal<string>.doIt(errMsg)) {
          WWLog.logError(errMsg);
        }
		    result = string.Format(result, args);
        Dictionary<string, string> paramDict = new Dictionary<string, string>();
        paramDict.Add(LOCA_LANG, language);
        paramDict.Add(LOCA_MISSING_KEY, key);
        paramDict.Add(LOCA_COMPONENT, appName);
        FlurryAgent.Instance.logEvent(LOCA_LOOKUP_STRING_MISS, paramDict);
      }
    }
    else {
      WWLog.logError("Invalid plural key: " + key);
    }
    return result;
  }

  public string Format(string key, params object[] args){
    bool unused;
    return Format(key, out unused, args);
  }

  private HashSet<string> doNotTranslate = new HashSet<string>();

  public string Format(string key, out bool isFound, params object[] args){
    isFound = false;
    warnIfNoContents();
    if (!string.IsNullOrEmpty (key)) {
      key = key.Replace(token_extraction, "");
	  key = key.Trim(new char[] { ' ','\t'});
      if (key.StartsWith(token_do_not_translate)) {
        key = key.Substring(token_do_not_translate.Length);
        doNotTranslate.Add(key);
      }
    }
    if (string.IsNullOrEmpty(key)) {
      isFound = true;
      return "";
    }
    string result = key;
    if (po.dictionary.ContainsKey(key)){
      result = po.dictionary[key].translations[0];
      result = string.Format(result, args);
      isFound = true;
    } else {
      if (doNotTranslate.Contains(key)) {
        isFound = true;
      }
      else {
        string errMsg = "Can't find key: \"" + key + "\"";
        if (wwDoOncePerTypeVal<string>.doIt(errMsg)) {
          WWLog.logError(errMsg);
        }
        Dictionary<string, string> paramDict = new Dictionary<string, string>();
        paramDict.Add(LOCA_LANG, language);
        paramDict.Add(LOCA_MISSING_KEY, key);
        paramDict.Add(LOCA_COMPONENT, appName);
        FlurryAgent.Instance.logEvent(LOCA_LOOKUP_STRING_MISS, paramDict);
      }
      result = string.Format(result, args);
    }
    return result;
  }

  private void warnIfNoContents() {
	if (poFilesIngested!=null && poFilesIngested.Count == 0) {
      if (wwDoOncePerTypeVal<string>.doIt("warn if no po files ingested")) {
        WWLog.logError("looking up key but no PO files have been ingested.");
      }
    }
  }

  // returns true if the file is injested.
  private bool ParsePOFileFromResource(string pathFile){
    bool ret = false;
    if (poFilesIngested.Contains(pathFile)) {
      WWLog.logWarn("already ingested \"" + pathFile + "\". skipping.");
    }
    else {
      WWLog.logInfo("loading PO file: " + pathFile);
      TextAsset file = Resources.Load(pathFile, typeof(TextAsset)) as TextAsset;
      wwPO temp = new wwPO();
      temp.Parse(file.text);
//      temp.PrintAllKeys(); // For debugging
      po.Merge(temp);
      poFilesIngested.Add(pathFile);
      ret = true;
    }

    return ret;
  }

  private bool ParsePOFileFromSavedPath(string path){
    bool ret = false;
    if (poFilesIngested.Contains(path)) {
      WWLog.logWarn("already ingested \"" + path + "\". skipping.");
    }
    else {
      WWLog.logInfo("loading PO file: " + path);
      string data = wwDataSaveLoadManager.Instance.Load(path);
      if(data == null){
        WWLog.logWarn("Failed at loading file" + path);
        return false;
      }
      wwPO temp = new wwPO();
      temp.Parse(data);
      po.Merge(temp);
      poFilesIngested.Add(path);
      ret = true;
    }

    return ret;
  }

  private bool ParsePOFolder(string lang, string poResourceFolder, string downloadPath) {
    po = new wwPO();
    poFilesIngested = new HashSet<string>();

    bool isCacheVersionMatched = piConnectionManager.Instance.isCacheVersionMatched(appName, version);
    bool fromDownload = false;
    //Try to load from saved path
    DirectoryInfo dir = new DirectoryInfo(downloadPath);
    if(isCacheVersionMatched && dir.Exists){
      FileInfo[] pofiles = dir.GetFiles("*_" + lang + ".po.txt");

      foreach(FileInfo f in pofiles){
        ParsePOFileFromSavedPath(f.FullName);
      }
    }

    fromDownload = poFilesIngested.Count >0;

    Dictionary<string, string> flurryParamDict = new Dictionary<string, string>();
    flurryParamDict.Add(LOCA_LANG, lang);
    flurryParamDict.Add(LOCA_COMPONENT, appName);

    if(!fromDownload){
      // TODO: this reads _all_ the files in the folder, including the ones not for the requested language. TUR-2371
      Object[] files = Resources.LoadAll(poResourceFolder);
      foreach (Object file in files) {
        if (file.name.EndsWith("_" + lang + ".po")) {
          ParsePOFileFromResource(poResourceFolder+"/"+version+"/"+file.name);
        }
      }        
      flurryParamDict.Add(LOCA_LOAD_FROM_SOURCE, true.ToString());
    }
    else{
      flurryParamDict.Add(LOCA_LOAD_FROM_SOURCE, false.ToString());
    }


    int numInjested = poFilesIngested.Count;
    string path = fromDownload?downloadPath : poResourceFolder;
    string s = "Parsed folder: \"" + path + "\", " + numInjested + " files and " + po.dictionary.Count + " strings injested for language \"" + lang + "\"";
    if (numInjested > 0) {
      FlurryAgent.Instance.logEvent(LOCA_LOOKUP_LANG_LOADED, flurryParamDict);
      WWLog.logInfo(s);
    }
    else {
      // not an error because this might just be an unsupported language.
      WWLog.logWarn(s);
    }

    return (numInjested > 0);
  }

  public void TestKeySearchPerformance(){
    List<string> testStrings = new List<string>();
    foreach (string item in po.dictionary.Keys){
      testStrings.Add(item);
    }
    Stopwatch watch = new Stopwatch();
    watch.Reset();
    watch.Start();
    foreach (var item in testStrings){
      Format(item);
    }
    watch.Stop();
    WWLog.log(WWLog.logLevel.INFO, "Searched "+po.dictionary.Keys.Count+" keys, took :"+watch.ElapsedMilliseconds+" ms");
  }

}
