using UnityEngine;
using WW.SaveLoad;
using WW.SimpleJSON;
using System.IO;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
using WW.UnityEditor_iOS_Xcode;  // piUnityShared - plistparser
#endif

// note, we use Debug.log directly here instead of WWLog
//       because we may be currently building for non-osx, but we're actually in osx.

public static class BuildInfo {

  public const string cTOK_APP_VERSION        = "app_version";

  public static string Summary {
    get {
      return AppVersion + " (" + BuildNumber + ")";
    }
  }
  
  public static string AppVersion {
    get {
      return BuildVersion.VERSION;
    }
  }

  public static string BuildNumber {
    get {
      return BuildVersion.BUILD.ToString();
    }
  }

  public enum VersionCompareResult {
    INVALID,
    LESS_THAN,
    EQUAL,
    GREATER_THAN,
  };

  // returns successful comparison. result is in out param.
  // A <  B :  LESS_THAN
  // A == B :  EQUAL
  // A >  B : GREATER_THAN
  public static VersionCompareResult CompareAppVersions(string A, string B) {
    const int numComponents = 3;
    string[] componentsA = A.Split('.');
    string[] componentsB = B.Split('.');
    if ((componentsA.Length != numComponents) || (componentsB.Length != numComponents)) {
      Debug.LogError(string.Format("invalid version string. either \"{0}\" or \"{1}\"", A, B));
      return VersionCompareResult.INVALID;
    }

    int[] valsA = new int[numComponents];
    int[] valsB = new int[numComponents];
    for (int n = 0; n < numComponents; ++n) {
      int tmp;
      if (!int.TryParse(componentsA[n], out tmp)) {
        Debug.LogError(string.Format("invalid version string: \"{0}\"", A));
        return VersionCompareResult.INVALID;
      }
      valsA[n] = tmp;
      if (!int.TryParse(componentsB[n], out tmp)) {
        Debug.LogError(string.Format("invalid version string: \"{0}\"", B));
        return VersionCompareResult.INVALID;
      }
      valsB[n] = tmp;
    }

    for (int n = 0; n < numComponents; ++n) {
      if (valsA[n] < valsB[n]) {
        return VersionCompareResult.LESS_THAN;
      }
      else if (valsA[n] > valsB[n]) {
        return VersionCompareResult.GREATER_THAN;
      }
    }

    return VersionCompareResult.EQUAL;
  }

#if UNITY_EDITOR
  private struct _testCase {
    public readonly string A;
    public readonly string B;
    public readonly VersionCompareResult result;

    public _testCase(string p1, string p2, VersionCompareResult p3) {
      A      = p1;
      B      = p2;
      result = p3;
    }
  };

  public static void testVersionCompare() {
    _testCase[] cases = new _testCase[] {
      new _testCase("1.2"  , "1.2"   , VersionCompareResult.INVALID     ),
      new _testCase("1.2.3", "1.2"   , VersionCompareResult.INVALID     ),
      new _testCase("1.2.3", "1.2.Z" , VersionCompareResult.INVALID     ),
      new _testCase("1.2.3", "1.2.30", VersionCompareResult.LESS_THAN   ),
      new _testCase("1.2.3", "1.2.2" , VersionCompareResult.GREATER_THAN),
      new _testCase("1.2.3", "1.2.3" , VersionCompareResult.EQUAL       ),
    };

    foreach (_testCase tc in cases) {
      VersionCompareResult result = CompareAppVersions(tc.A, tc.B);
      if (result != tc.result) {
        Debug.LogError("Test Failed! " + tc.A + " ? " + tc.B + ": expected " + tc.result.ToString() + " but got " + result.ToString());
      }
    }

    Debug.Log("BuildInfo.testVersionCompare() - " + cases.Length + " test cases passed");
  }

  public static void test() {
    testVersionCompare();
  }
#endif
}

#if UNITY_EDITOR

public class WWBuildWindow : EditorWindow {
  string appVersion  = "";
  int    buildNumber = 1;
  string username = "";
  string[] scenes = null;
  bool expandScenes = false;
  bool expandRepos = true;
  Dictionary <string, string> theRepos = null;
  Vector2 scrollPos1 = Vector2.zero;
  bool restorePlatformToggleVal = true;
  
  string rootPathOSX = "";
  string rootPathIOS = "";
  string rootPathAndroid = "";
  
  const string cAppName                = "Wonder";  
  const string cLocalBuildSettingsPath = "trLocalBuildSettings.json";
  const string cTOK_INDENT             = "  ";
  const string cTOK_REPL_APP_NAME      = "APP_NAME";
  const string cTOK_REPL_APP_VERSION   = "APP_VERSION";
  const string cTOK_REPL_BUILD_NUMBER  = "BUILD_NUMBER";
  const string cTOK_ANDROID            = "android";
  const string cTOK_IOS                = "ios";
  const string cTOK_OSX                = "osx";
  const string cTOK_BUILD_PATHS        = "build_paths";
  const string cTOK_RESTORE_PLATFORM   = "restore_platform";
  
  
  static string[] cScenes = new string[] {
    "Assets/Scenes/Turing/Start.unity",
    "Assets/Scenes/Turing/Lobby.unity",
    "Assets/Scenes/Turing/Main.unity",
    "Assets/Scenes/Turing/RemoteControl.unity",
    "Assets/Scenes/Turing/Map.unity",
    "Assets/Scenes/Turing/AdminScene.unity",
    "Assets/Scenes/Turing/Vault.unity",
    "Assets/Scenes/Turing/Profiles.unity",
    "Assets/Scenes/Turing/Options.unity",
    "Assets/Scenes/Turing/Community.unity",
    "Assets/Scenes/Turing/AnimShop.unity",
    "Assets/Scenes/Turing/InternalTesting.unity",
  };
  
  const string cGRADLE_TEMPLATE = @"/*
  IMPORTANT IMPORTANT IMPORTANT IMPORTANT IMPORTANT
  this file was generated by the in-unity WW Build-Script.
  See ""BuildSpark.cs"".
  IMPORTANT IMPORTANT IMPORTANT IMPORTANT IMPORTANT
*/
apply plugin: 'com.android.application'
android {
    compileSdkVersion 21
    buildToolsVersion ""21.1.1""

    defaultConfig {
        applicationId ""com.makewonder.wonder""
        minSdkVersion 19
        targetSdkVersion 21
        versionCode BUILD_NUMBER
        versionName ""APP_VERSION""
    }

    buildTypes {
        release {
            minifyEnabled false
            proguardFiles getDefaultProguardFile('proguard-android.txt'), 'proguard-rules.pro'
        }
    }


}

dependencies {
    compile fileTree(dir: 'libs', include: ['*.jar'])
    compile 'com.android.support:appcompat-v7:21.0.2'
    compile 'com.makewonder:ww-chrome-library:1.1-SNAPSHOT'
    compile project(':Wonder')
}
";
  // Add menu named "My Window" to the Window menu
  [MenuItem ("WW/Build Tool")]
  static void Init () {
    
    // Get existing open window or if none, make a new one:
    WWBuildWindow window = (WWBuildWindow)EditorWindow.GetWindow (typeof (WWBuildWindow));
    window.instanceInit();
    window.Show();
  }
  
  void instanceInit() {
    loadLocalBuildSettings();
    appVersion = BuildVersion.VERSION;
    buildNumber = BuildVersion.BUILD;
    scenes      = cScenes;
    username    = System.Environment.UserName;
  }
  
  // for use from command-line
  static public void buildAndroid() {
    WWBuildWindow window = (WWBuildWindow)EditorWindow.GetWindow (typeof (WWBuildWindow));
    window.instanceInit();
    window.BuildItAndroid(false);
  }
  
  // for use from command-line
  static public void buildiOS() {
    WWBuildWindow window = (WWBuildWindow)EditorWindow.GetWindow (typeof (WWBuildWindow));
    window.instanceInit();
    window.BuildItIOS(false);
  }
  
  // for use from command-line
  static public void buildOSX() {
    WWBuildWindow window = (WWBuildWindow)EditorWindow.GetWindow (typeof (WWBuildWindow));
    window.instanceInit();
    window.BuildItOSX(false);
  }
  
  // for use from command-line
  static public void buildAll() {
    WWBuildWindow window = (WWBuildWindow)EditorWindow.GetWindow (typeof (WWBuildWindow));
    window.instanceInit();
    window.BuildThemAll(false);
  }
  
  string LocalBuildSettingsPath {
    get {
      return Application.persistentDataPath + "/" + cLocalBuildSettingsPath;
    }
  }
  void loadLocalBuildSettings() {
    string path = LocalBuildSettingsPath;
    if (!File.Exists(path)) {
      Debug.Log("does not exist: " + cLocalBuildSettingsPath + ". using default local settings");
      return;
    }
    
    JSONClass jsc = JSON.Parse(wwDataSaveLoadManagerStatic.Load(LocalBuildSettingsPath, true)).AsObject;
    if (jsc == null) {
      Debug.LogError("error parsing " + path + ". using default local settings.");
      return;
    }
    
    if (!jsc.ContainsKey(cTOK_BUILD_PATHS)) {
      Debug.LogError("no entry " + cTOK_BUILD_PATHS + ".    " + jsc.ToString());
    }
    else {
      JSONClass bps = jsc[cTOK_BUILD_PATHS].AsObject;
      foreach(string key in bps.Keys) {
        string bp = bps[key];
        switch (key) {
          default:
            Debug.LogError("Unhandled build type: " + key + " --> " + bp);
            break;
          case cTOK_ANDROID:
            rootPathAndroid = bp;
            break;
          case cTOK_IOS:
            rootPathIOS = bp;
            break;
          case cTOK_OSX:
            rootPathOSX = bp;
            break;
        }
      }
    }
    
    if (jsc.ContainsKey(cTOK_RESTORE_PLATFORM)) {
      restorePlatformToggleVal = jsc[cTOK_RESTORE_PLATFORM].AsBool;
    }
  }
  
  void saveLocalBuildSettings() {
    JSONClass jsc = new JSONClass();
    JSONClass bps = new JSONClass();
    bps[cTOK_ANDROID] = rootPathAndroid;
    bps[cTOK_IOS] = rootPathIOS;
    bps[cTOK_OSX] = rootPathOSX;
    jsc[cTOK_BUILD_PATHS] = bps;
    jsc[cTOK_RESTORE_PLATFORM].AsBool = restorePlatformToggleVal;
    wwDataSaveLoadManagerStatic.Save(jsc.ToString(""), LocalBuildSettingsPath);
  }
  
  delegate void BuilderType(bool restorePlatform);
  
  bool isValidFolderPath(string path) {
    if (string.IsNullOrEmpty(path)) {
      return false;
    }
    else {
      return Directory.Exists(path);
    }
  }
  
  void refreshTheRepos() {
    const string theShellCmd = "NotForDistribution/Tools/osx/gitBranches.command";
    
    string stdout = "";
    if (execShellCommand(theShellCmd, out stdout) != 0) {
      theRepos = new Dictionary<string, string> { {"ERROR", "Could not fetch repository branches"} };
    }
    else {
      string[] lines = stdout.Split('\n');
      
      theRepos = new Dictionary<string, string>();
      
      // this is pretty crude.
      foreach(string line in lines) {
        if (line.Contains(" - ")) {
          string[] comps = line.Split(' ');
          theRepos.Add(comps[0], comps[comps.Length - 1]);
        }
      }
      
      if (theRepos.Count == 0) {
        theRepos = new Dictionary<string, string> { {"ERROR", "Could not fetch repository branches"} };
      }
    }
  }
  
  void guiIndent() {
    GUILayout.Space(20);
  }
  
  void OnGUI () {
  
    bool needSaveLocalSettings = false;
  
    GUILayout.Label ("Build Settings", EditorStyles.boldLabel);
    
    scrollPos1 = GUILayout.BeginScrollView(scrollPos1);

    GUI.enabled = false;
    appVersion  = EditorGUILayout.TextField ("App Version" , appVersion);
    buildNumber  = int.Parse(EditorGUILayout.TextField ("Build Number", buildNumber.ToString()));
    username    = EditorGUILayout.TextField ("User Name"   , username);
    GUI.enabled = true;

    // list scenes
    if (scenes == null) {
      GUILayout.Label("NO SCENES!!");
    }
    else {
      expandScenes = EditorGUILayout.Foldout(expandScenes, "Scenes (" + scenes.Length.ToString() + ")");
      if (expandScenes) {
        GUILayout.BeginHorizontal();
        guiIndent();
        GUILayout.BeginVertical();
        foreach (string s in scenes) {
          GUILayout.Label(s);
        }
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
      }
    }
    
    // list repos
    expandRepos = EditorGUILayout.Foldout(expandRepos, "Repository Branches");
    if (expandRepos) {
      GUILayout.BeginHorizontal();
      guiIndent();
      GUILayout.BeginVertical();
      if (GUILayout.Button("Refresh") || (theRepos == null)) {
        refreshTheRepos();
      }
      bool addDivTop = true;
      GUIStyle styleHR = new GUIStyle(GUI.skin.box);
      styleHR.stretchWidth = true;
      styleHR.fixedHeight = 2;
      foreach (string key in theRepos.Keys) {
        if (addDivTop) {
          GUILayout.Box("", styleHR);
        }
        addDivTop = false;
        GUILayout.BeginHorizontal();
        GUILayout.Label(key);
        GUILayout.Label(theRepos[key], GUILayout.Width(200));
        GUILayout.EndHorizontal();
        GUILayout.Box("", styleHR);
      }
      GUILayout.EndVertical();
      GUILayout.EndHorizontal();
    }
    
    
    
    
    // paths
    string s2;
    
    GUILayout.BeginHorizontal();
    if (GUILayout.Button("OSX Path:", GUILayout.Width(100))) {
      getRootPathOSX();
    }
    s2 = isValidFolderPath(rootPathOSX) ? "" : "--INVALID!--";
    GUILayout.Label(s2 + rootPathOSX);
    GUILayout.EndHorizontal();
    
    GUILayout.BeginHorizontal();
    if (GUILayout.Button("IOS Path:", GUILayout.Width(100))) {
      getRootPathIOS();
    }
    s2 = isValidFolderPath(rootPathIOS) ? "" : "--INVALID!--";
    GUILayout.Label(s2 + rootPathIOS);
    GUILayout.EndHorizontal();
    
    GUILayout.BeginHorizontal();
    if (GUILayout.Button("Android Path:", GUILayout.Width(100))) {
      getRootPathAndroid();
    }
    s2 = isValidFolderPath(rootPathAndroid) ? "" : "--INVALID!--";
    GUILayout.Label(s2 + rootPathAndroid);
    GUILayout.EndHorizontal();
    
    // we declare a delegate so we can call it outside the BeginHorizontal,
    // because that would throw an error at the end of building.
    BuilderType builder = null;
    
    GUILayout.BeginHorizontal();
    GUI.enabled = isValidFolderPath(rootPathOSX);
    if (GUILayout.Button("build OSX")) {
      builder = BuildItOSX;
    }
    GUI.enabled = isValidFolderPath(rootPathIOS);
    if (GUILayout.Button("build iOS")) {
      builder = BuildItIOS;
    }
    GUI.enabled = isValidFolderPath(rootPathAndroid);
    if (GUILayout.Button("build Android")) {
      builder = BuildItAndroid;
    }
    GUI.enabled = true;
    GUILayout.EndHorizontal();
    
    GUILayout.BeginHorizontal();
    GUI.enabled = true;
    GUI.enabled &= isValidFolderPath(rootPathOSX);
    GUI.enabled &= isValidFolderPath(rootPathIOS);
    GUI.enabled &= isValidFolderPath(rootPathAndroid);
    if (GUILayout.Button("build all")) {
      builder = BuildThemAll;
    }
    GUI.enabled = true;
    GUILayout.EndHorizontal();
    GUILayout.BeginHorizontal();
    GUILayout.Label("Current Platform:    " + EditorUserBuildSettings.activeBuildTarget.ToString());
    bool tmpBool = restorePlatformToggleVal;
    restorePlatformToggleVal = GUILayout.Toggle(restorePlatformToggleVal, "restore after build");
    needSaveLocalSettings |= (tmpBool != restorePlatformToggleVal);
    GUILayout.EndHorizontal();
    GUILayout.EndScrollView();
    
    // DO NOT PUT ANY GUI STUFF BELOW THIS LINE!
    // because the actual builder code below takes like 30 minutes sometimes.
    
    if (needSaveLocalSettings) {
      saveLocalBuildSettings();
    }
    
    if (builder != null) {
      builder(true);
    }
  }
  
  string DoBuildInfoReplacements(string s) {
    string ret = s;
    ret = ret.Replace(cTOK_REPL_APP_NAME    , cAppName              );
    ret = ret.Replace(cTOK_REPL_APP_VERSION , appVersion            );
    ret = ret.Replace(cTOK_REPL_BUILD_NUMBER, buildNumber.ToString());
    return ret;
  }
  
  void writeTextFile(string path, string contents) {
    Debug.Log("writing file " + path + "\n" + contents);        
    StreamWriter sw = File.CreateText(path);
    sw.Write(contents);
    sw.Close();
  }
  
  void BuildThemAll(bool restorePlatform) {
  
    restorePlatform &= restorePlatformToggleVal;
    BuildTarget btOrig = EditorUserBuildSettings.activeBuildTarget;
    
    BuildItOSX    (false);
    BuildItIOS    (false);
    BuildItAndroid(false);
    
    if (restorePlatform && (EditorUserBuildSettings.activeBuildTarget != btOrig)) {
      EditorUserBuildSettings.SwitchActiveBuildTarget(btOrig);
    }
  }
  
  void getRootPathIOS() {
    string path = EditorUtility.SaveFolderPanel("IOS: Choose Parent Folder of XCode Project. (eg choose \"iOS\")", "", "");
    if (string.IsNullOrEmpty(path)) {
      // cancelled
      return;
    }
    rootPathIOS          = "";
    string pathApp       = path;
    string pathWorkspace = pathApp + "/Unity-iPhone.xcworkspace";
    string pathInfoPlist = pathApp + "/Info.plist";
    
    if (!Directory.Exists(pathWorkspace)) {
      Debug.LogError("Unity-iPhone.xcworkspace missing! you probably selected the wrong folder. If this is an initial export, please do it manually.");
      return;
    }
    
    if (!File.Exists(pathInfoPlist)) {
      Debug.LogError("Info.plist missing! you probably selected the wrong folder. If this is an initial export, please do it manually.");
      return;
    }
    
    rootPathIOS = path;
    saveLocalBuildSettings();
  }
  
  void getRootPathOSX() {
    string path          = EditorUtility.SaveFolderPanel("OSX: Choose Parent Folder of .app", "", "");
    if (string.IsNullOrEmpty(path)) {
      // cancelled
      return;
    }
    rootPathOSX = path + "/" + cAppName + ".app";
    saveLocalBuildSettings();
  }
  
  void getRootPathAndroid() {    
    string path            = EditorUtility.SaveFolderPanel("Android: Choose Parent Folder of build.gradle", "", "");
    rootPathAndroid = "";
    
    string buildGradlePath = path + "/build.gradle";
    
    if (!File.Exists(buildGradlePath)) {
      Debug.LogError("build.gradle missing! you probably selected the wrong folder. If this is an initial export, please do it manually.");
      return;
    }
    
    rootPathAndroid = path;
    saveLocalBuildSettings();
  }
  
  void updatePlistFile(string pathInfoPlist) {
    Debug.Log("updating info.plist: " + pathInfoPlist);
    PlistDocument plist = new PlistDocument();
    plist.ReadFromString(File.ReadAllText(pathInfoPlist));
    PlistElementDict rootDict = plist.root;
    
    rootDict.SetString("CFBundleShortVersionString", appVersion);
    rootDict.SetString("CFBundleVersion"           , buildNumber.ToString());
    
    File.WriteAllText(pathInfoPlist, plist.WriteToString());
  }
  
  void BuildItIOS(bool restorePlatform) {
    if (rootPathIOS == "") {
      getRootPathIOS();
    }
    
    if (rootPathIOS == "") {
      return;
    }
  
    string pathApp       = rootPathIOS;
    string pathInfoPlist = pathApp + "/Info.plist";
    
    BuildOptions opts = BuildOptions.None;
    opts = opts | BuildOptions.ShowBuiltPlayer;
    opts = opts | BuildOptions.SymlinkLibraries;
    opts = opts | BuildOptions.AcceptExternalModificationsToPlayer;
    
    Debug.Log("building " + pathApp);
    
    restorePlatform &= restorePlatformToggleVal;
    BuildTarget btOrig = EditorUserBuildSettings.activeBuildTarget;
    
    BuildPipeline.BuildPlayer(scenes, pathApp, BuildTarget.iOS, opts);
    
    updatePlistFile(pathInfoPlist);
    
    if (restorePlatform && (EditorUserBuildSettings.activeBuildTarget != btOrig)) {
      EditorUserBuildSettings.SwitchActiveBuildTarget(btOrig);
    }
  }
  
  void BuildItOSX(bool restorePlatform) {
    if (rootPathOSX == "") {
      getRootPathOSX();
    }
    
    if (rootPathOSX == "") {
      return;
    }
    
    string pathApp       = rootPathOSX;
    string pathInfoPlist = pathApp + "/Contents/Info.plist";
    
    // Build player.
    BuildOptions opts = BuildOptions.None;
    opts = opts | BuildOptions.ShowBuiltPlayer;
    
    Debug.Log("building " + pathApp);
    
    restorePlatform &= restorePlatformToggleVal;
    BuildTarget btOrig = EditorUserBuildSettings.activeBuildTarget;
    
    BuildPipeline.BuildPlayer(scenes, pathApp, BuildTarget.StandaloneOSXIntel64, opts);
    
    updatePlistFile(pathInfoPlist);
    
    if (restorePlatform && (EditorUserBuildSettings.activeBuildTarget != btOrig)) {
      EditorUserBuildSettings.SwitchActiveBuildTarget(btOrig);
    }
    
  }
  
  int execShellCommandNoVerifyPwd(string cmd, out string stdout) {
    try {
      // Start the child process.
      System.Diagnostics.Process p = new System.Diagnostics.Process();
      // Redirect the output stream of the child process.
      p.StartInfo.UseShellExecute = false;
      p.StartInfo.RedirectStandardOutput = true;
      p.StartInfo.FileName = cmd;
      p.Start();
      // Do not wait for the child process to exit before
      // reading to the end of its redirected stream.
      // p.WaitForExit();
      // Read the output stream first and then wait.
      string output = p.StandardOutput.ReadToEnd();
      p.WaitForExit();
      stdout = output.Trim();
      return p.ExitCode;
    }
    catch (System.Exception e) {
      Debug.LogError("error executing command: \"" + cmd + "\": " + e.ToString());
      stdout = "";
      return -1;
    }
  }
  
  int execShellCommand(string cmd, out string stdout) {
  
      // validate pwd
      string pwd;
    if (execShellCommandNoVerifyPwd("pwd", out pwd) != 0) {
        Debug.LogError("error obtaining current directory.");
        stdout = "";
        return -1;
      }
      
      string[] pwdComponents = pwd.Split('/');
      if (pwdComponents.Length < 2) {
        Debug.LogError("crazy pwd: \"" + pwd + "\"");
        stdout = "";
        return -1;
      }
      
      if (pwdComponents[pwdComponents.Length - 1] != "Go2Unity") {
        Debug.LogError("unexpected pwd: \"" + pwd + "\". expected final component to be \"Go2Unity\".");
        stdout = "";
        return -1;
      }
      
      // okay, we're in the directory we expect.
      
    return execShellCommandNoVerifyPwd(cmd, out stdout);
  }
  
  bool BuiltItAndroidAPIAndChrome() {
    const string theShellCmd = "NotForDistribution/Tools/osx/buildAndroidAPIAndChrome.command";
  
    // pwd is to Go2/Go2Unity
    Debug.Log("Building & installing APIAndroid & ChromeAndroid..");
    
    string result;
    if (execShellCommand(theShellCmd, out result) != 0) {
      Debug.LogError("problem building & installing APIAndroid and/or ChromeAndroid");
      return false;
    }
    
    Debug.Log(result);
    
    return true;
  }
  
  void BuildItAndroid(bool restorePlatform) {
    if (rootPathAndroid == "") {
      getRootPathAndroid();
    }
    
    if (rootPathAndroid == "") {
      return;
    }
    
    if (!BuiltItAndroidAPIAndChrome()) {
      return;
    }

    string path            = rootPathAndroid;    
    string assetsPath      = path + "/Wonder/assets";
    string pathBuildInfo   = path + "/app/build.gradle";
    string buildInfo       = DoBuildInfoReplacements(cGRADLE_TEMPLATE);
    
    
    
    PlayerSettings.Android.bundleVersionCode = buildNumber;
    PlayerSettings.bundleVersion             = appVersion;
    
    // TODO: run NotForDistribution/Tools/osx/buildAndroidAPIAndChrome.command 
    
    
    // Remove assets folder. It's lame that this is necessary.
    Debug.Log("removing " + assetsPath);
    FileUtil.DeleteFileOrDirectory(assetsPath);
    
    // Build player.
    BuildOptions opts = BuildOptions.None;
    opts = opts | BuildOptions.ShowBuiltPlayer;
    opts = opts | BuildOptions.AcceptExternalModificationsToPlayer;
    
    Debug.Log("building " + path);
    
    restorePlatform &= restorePlatformToggleVal;
    BuildTarget btOrig = EditorUserBuildSettings.activeBuildTarget;
    
    BuildPipeline.BuildPlayer(scenes, path, BuildTarget.Android, opts);
    
    if (restorePlatform && (EditorUserBuildSettings.activeBuildTarget != btOrig)) {
      EditorUserBuildSettings.SwitchActiveBuildTarget(btOrig);
    }
    
    writeTextFile(pathBuildInfo, buildInfo);
    
    // TODO: actually build it - skip Android Studio.
  }
}




#endif