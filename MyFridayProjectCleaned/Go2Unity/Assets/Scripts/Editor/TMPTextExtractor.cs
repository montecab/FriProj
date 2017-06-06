#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;

public class TMPTextExtractor : EditorWindow {

  public class POString
  {
    public string content;
    public string comment;
    public POString(string ct, string cm){
      content = ct;
      comment = cm;
    }
  }
  private static readonly string TAG_NO_TRANSLATION = "DoNotTranslate";
  private bool checkAllScenes = true;
  private bool checkAllPrefabs = true;
  private Vector2 scrollPosEditor = Vector2.zero;
  private Vector2 scrollPosForPrefabWindow = Vector2.zero;
  private Vector2 scrollPosForSceneWindow = Vector2.zero;
  private List<POString> searchResultPrefabs;
  private Dictionary<string, List<POString>> searchResultScenes;
  private List<string> allScenesToCheck = new List<string>(){
    "Assets/Scenes/Turing/Community.unity",
    "Assets/Scenes/Turing/Lobby.unity",
    "Assets/Scenes/Turing/Main.unity",
    "Assets/Scenes/Turing/Map.unity",
    "Assets/Scenes/Turing/Options.unity",
    "Assets/Scenes/Turing/Profiles.unity",
    "Assets/Scenes/Turing/RemoteControl.unity",
    "Assets/Scenes/Turing/Vault.unity",
  };
  private bool[] showScenes = new bool[8];
  private string csvPath = "/Resources/Strings/wonder_ui_static_en_US.csv";
  private static string poPath = "";

  private static string poPathDefault{
    get{
      return  "/Resources/Strings/wonder/"+BuildInfo.AppVersion+ "/wonder_ui_static_en_US.po.txt";
    }
  }
  private bool isExtracted = false;

  [MenuItem("WW/Localization/TMP_Text Extractor", false, 0)]
  private static void Init () {
    TMPTextExtractor window = (TMPTextExtractor) EditorWindow.GetWindow( typeof( TMPTextExtractor ) );
    poPath = poPathDefault;
    window.Show();
  }

  private void OnGUI(){
    if (searchResultPrefabs == null || searchResultScenes == null){
      Reset();
    }
    scrollPosEditor = GUILayout.BeginScrollView(scrollPosEditor);
    GUILayout.Space(20);
    GUILayout.BeginHorizontal();
    Color defaultColor = GUI.color;
    GUI.color = Color.green;
    if (GUILayout.Button("Extract")){
      Reset();
      if (checkAllPrefabs){
        searchResultPrefabs = GetPOStringsFromPrefabs();
        isExtracted = true;
      }
      if (checkAllScenes){
        IEnumerator e = ExtractScenes();
        while(e.MoveNext());
        isExtracted = true;
      }
      EditorUtility.DisplayDialog("Extraction complete",
        "Prefabs extracted: (" + searchResultPrefabs.Count + ") strings\n" + "Scenes extracted:" + searchResultScenes.Count,
        "OK");
    }
    GUILayout.EndHorizontal();
    GUI.color = defaultColor;

    GUILayout.Space(20);
    GUILayout.Label("Options:");
    GUILayout.BeginHorizontal();
    checkAllScenes = GUILayout.Toggle(checkAllScenes, "All scenes in list");
    if (checkAllScenes){
      if (GUILayout.Button("+", GUILayout.Width(20))){
        allScenesToCheck.Add("");
      }

      GUILayout.EndHorizontal();
      List<string> removedScenes = new List<string>();
      for (int i = 0; i < allScenesToCheck.Count; i++){
        GUILayout.BeginHorizontal();
        allScenesToCheck[i] = GUILayout.TextField(allScenesToCheck[i]);
        if (GUILayout.Button("-", GUILayout.Width(20))){
          removedScenes.Add(allScenesToCheck[i]);
        }
        GUILayout.EndHorizontal();
      }
      for (int i = 0; i < removedScenes.Count; i++){
        allScenesToCheck.Remove(removedScenes[i]);
      }
    } else{
      GUILayout.EndHorizontal(); 
    }

    checkAllPrefabs = GUILayout.Toggle(checkAllPrefabs, "All prefabs in project");
    GUILayout.Space(20);

    if (isExtracted && (searchResultPrefabs.Count>0 || searchResultScenes.Keys.Count>0)){
      GUILayout.Label ("Result Prefabs: (" + searchResultPrefabs.Count + ") strings");
      if (checkAllPrefabs) {
        scrollPosForPrefabWindow = GUILayout.BeginScrollView (scrollPosForPrefabWindow, GUILayout.MaxHeight (300));
        string tmp = "";
        foreach (POString item in searchResultPrefabs) {
          tmp += item.content+"\n";
        }
        GUILayout.TextArea (tmp);
        GUILayout.EndScrollView ();
      }
      GUILayout.Label ("Result Scenes:");
      if (checkAllScenes) {
        scrollPosForSceneWindow = GUILayout.BeginScrollView (scrollPosForSceneWindow, GUILayout.MaxHeight (500));
        int counter = 0;
        if (searchResultScenes.Keys.Count > 0) {
          foreach (string key in searchResultScenes.Keys) {
            showScenes [counter] = GUILayout.Toggle (showScenes [counter], key + " (" + searchResultScenes [key].Count + ") strings");
            if (showScenes [counter]) {
              string tmp = "";
              foreach (POString item in searchResultScenes[key]) {
                tmp += item.content+"\n";
              }
              GUILayout.TextArea (tmp);
            }
            counter++;
          }
        }
        GUILayout.EndScrollView ();
      }

      GUILayout.Space (20);
      if (GUILayout.Button ("Export to CSV")) {
        SaveAsCSV();
        EditorUtility.DisplayDialog ("Export complete", "Exported to:" + Application.dataPath+csvPath, "OK");
        EditorUtility.RevealInFinder(Application.dataPath+csvPath);
      }
      GUILayout.BeginHorizontal();
      GUILayout.Label(Application.dataPath);
      csvPath = GUILayout.TextField(csvPath);
      GUILayout.EndHorizontal();

      GUILayout.Space (20);
      if (GUILayout.Button ("Export to PO")) {
        SaveAsPO();
        EditorUtility.DisplayDialog("Export complete", "Exported to:" + Application.dataPath+poPath, "OK");
        EditorUtility.RevealInFinder(Application.dataPath+poPath);
      }
      GUILayout.BeginHorizontal();
      GUILayout.Label(Application.dataPath);
      poPath = GUILayout.TextField(poPath);
      GUILayout.EndHorizontal();
    }
    GUILayout.EndScrollView();
  }

  private IEnumerator ExtractScenes(){
    float sceneCount = 0;
    foreach (string scene in allScenesToCheck){  //Each Scene
      bool isSceneValid = true;
      float objCount = 0;
      List<POString> results = new List<POString>();
      try{
        EditorSceneManager.OpenScene(scene, OpenSceneMode.Single);
      } catch (System.Exception ex){
        Debug.LogError("Invalid Scene name: " + scene + " " + ex.ToString());
        EditorUtility.ClearProgressBar();
        isSceneValid = false;
      }
      if (isSceneValid){
        GameObject[] allGameObjects = GetRootGameObjectsInScene ();
        foreach (GameObject go in allGameObjects) { //Each GameObject
          FindTMPCompRecursively(go, scene, results);
          EditorUtility.DisplayProgressBar("Extraction Progress","Extracting "+scene, 
            (sceneCount+(objCount/(float)allGameObjects.Length))/(float)allScenesToCheck.Count);
          objCount++;
          yield return null;
        }
        searchResultScenes.Add (scene, results);
        sceneCount++; 
      }
    }
    EditorUtility.ClearProgressBar();
  }

  public static List<POString> GetPOStringsFromScene(string sceneName){
    List<POString> results = new List<POString>();
    bool isSceneValid = true;
    try{
      EditorSceneManager.OpenScene(sceneName, OpenSceneMode.Single);
    } catch (System.Exception ex){
      Debug.LogError("Invalid Scene name: " + sceneName + " " + ex.ToString());
      EditorUtility.ClearProgressBar();
      isSceneValid = false;
    }
    if (isSceneValid){
      GameObject[] allGameObjects = GetRootGameObjectsInScene ();
      foreach (GameObject go in allGameObjects) { //Each GameObject
        FindTMPCompRecursively(go, sceneName, results);
      }
    }
    return results;
  }

  public static List<POString> GetPOStringsFromPrefabs(){
    List<POString> poStrings = new List<POString>();
    List<string> allPrefabs = GetAllPrefabs();
    foreach (string prefab in allPrefabs){
      GameObject go = (GameObject)AssetDatabase.LoadMainAssetAtPath(prefab);
      FindTMPCompRecursively(go, prefab, poStrings);
    }
    return poStrings;
  }

  private static void FindTMPCompRecursively (GameObject root, string path, List<POString> results){
    if (root.tag != TAG_NO_TRANSLATION) {
      TMPro.TMP_Text[] components = root.GetComponents<TMPro.TMP_Text>();
      foreach (TMPro.TMP_Text comp in components) { //Each TMP_Text
        if (!string.IsNullOrEmpty(comp.literalText) && !IsOnlyNonAlphabet(comp.literalText)) {
          results.Add(new POString(comp.literalText.Replace(System.Environment.NewLine, "\\n"), path+"/"+GetGameObjectPath(comp.transform)));
        }
      }
      foreach (Transform item in root.transform) {
        FindTMPCompRecursively(item.gameObject, path, results);  
      }
    }
  }

  private static bool IsOnlyNonAlphabet (string input){
    bool result = true;
    for (int i = 0; i < input.Length; i++) {
      result = result && !System.Char.IsLetter(input[i]);
    }
    return result;
  }

  private static string GetGameObjectPath(Transform transform){
    string path = transform.name;
    while (transform.parent != null){
     transform = transform.parent;
     path = transform.name + "/" + path;
    }
    return path;
  }

  private static GameObject[] GetRootGameObjectsInScene (){
    List<GameObject> results = new List<GameObject> ();
    GameObject[] allGameObjects = UnityEngine.Object.FindObjectsOfType<GameObject> ();
    foreach (var item in allGameObjects) {
      if (item.transform.parent == null) {
        results.Add(item);
      }
    }
    return results.ToArray();
  }

  public static string StringToCSVCell(string str){
    bool mustQuote = (str.Contains(",") || str.Contains("\"") || str.Contains("\r") || str.Contains("\n"));
    if (mustQuote){
        StringBuilder sb = new StringBuilder();
        sb.Append("\"");
        foreach (char nextChar in str)
        {
            sb.Append(nextChar);
            if (nextChar == '"')
                sb.Append("\"");
        }
        sb.Append("\"");
        return sb.ToString();
    }
    return str;
  }

  public static List<string> GetAllPrefabs () {
    string[] temp = AssetDatabase.GetAllAssetPaths();
    List<string> result = new List<string>();
    foreach ( string s in temp ) {
      if ( s.Contains( ".prefab" ) ) result.Add( s );
    }
    return result;
  }

  public static List<string> GetAllScenes () {
    string[] temp = AssetDatabase.GetAllAssetPaths();
    List<string> result = new List<string>();
    foreach ( string s in temp ) {
      if ( s.Contains( ".unity" ) ) result.Add( s );
    }
    return result;
  }

  private void Reset(){
    searchResultPrefabs = new List<POString>();
    searchResultScenes = new Dictionary<string, List<POString>>();
  }

  private void SaveAsCSV (){
    StringBuilder sb = new StringBuilder ();  
    sb.AppendLine("-----------------------------------Prefabs-----------------------------------");
    foreach (POString item in searchResultPrefabs) {
      sb.AppendLine(StringToCSVCell(item.content)+", "+item.comment);
    }
    foreach (string key in searchResultScenes.Keys) {
      sb.AppendLine("-----------------------------------"+key+"-----------------------------------");
      foreach (POString item in searchResultScenes[key]) {
        sb.AppendLine(StringToCSVCell(item.content)+", "+item.comment);
      }
    }
    File.WriteAllText(Application.dataPath+csvPath, sb.ToString());                 
  }

  private void SaveAsPO(){
    wwPO po = new wwPO();
    foreach (POString text in searchResultPrefabs){
      string inlineText = text.content;
      string comment = text.comment;
      po.AddEntry(inlineText, inlineText, new List<string>(){comment});
    }
    foreach (string key in searchResultScenes.Keys){
      foreach (POString text in searchResultScenes[key]){
        string inlineText = text.content;
        string comment = text.comment;
        po.AddEntry(inlineText, inlineText, new List<string>(){comment});
      }
    }
    File.WriteAllText(Application.dataPath+poPath, po.ToString());
  }

}
#endif
