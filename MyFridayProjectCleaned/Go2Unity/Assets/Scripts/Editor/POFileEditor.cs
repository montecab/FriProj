#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;

public class POFileEditor : EditorWindow {

  private wwPO po = null;
  private string poSavePath = "/Resources/Strings/output.po.txt";
  private TextAsset poTextAsset = null;
  private Vector2 scrollPos = Vector2.zero;
  private Color defaultColor;
  private bool isLoaded = false;
  private bool isDirty = false;

  [MenuItem("WW/Localization/PO File Editor", false, 1)]
  private static void Init () {
    POFileEditor window = (POFileEditor) EditorWindow.GetWindow( typeof( POFileEditor ) );
    window.Show();
  }

  private void OnGUI(){
    GUILayout.Space(20);
    if (GUILayout.Button("Load PO file")){
      LoadPOFile();
    }
    GUILayout.BeginHorizontal();
    GUILayout.Label("Drag PO file into the field:");
    poTextAsset = (TextAsset)EditorGUILayout.ObjectField(poTextAsset, typeof(TextAsset), false);
    GUILayout.EndHorizontal();

    if (po != null){
      GUILayout.Space(20);
      GUILayout.Label("PO Dictionary:");
      scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.MaxHeight(Screen.height * 0.8f));
      foreach (string key in po.dictionary.Keys){
        GUILayout.BeginHorizontal();
        GUILayout.TextField(key, GUILayout.Width(Screen.width / 2));
        DrawPOEntry(po.dictionary[key]);
        GUILayout.EndHorizontal();
        GUILayout.Space(5);
      }
      GUILayout.EndScrollView();

      if (isLoaded && GUI.changed && !isDirty){
        isDirty = true;
      }
      GUILayout.Space(20);
      GUI.enabled = isDirty;
      GUI.color = Color.red;
      if (GUILayout.Button("Save PO file")){
        SavePOFile();
        EditorUtility.DisplayDialog("Save complete", "Saved to:" + Application.dataPath + poSavePath, "OK");
      }
      GUI.color = defaultColor;
      GUI.enabled = true;
      GUILayout.BeginHorizontal();
      GUILayout.Label(Application.dataPath);
      poSavePath = GUILayout.TextField(poSavePath);
      GUILayout.EndHorizontal();
      if (!isLoaded){
        isLoaded = true;
      }
    }
  }

  private void DrawPOEntry(wwPOEntry entry){
    GUILayout.BeginVertical();
    for (int i = 0; i < entry.comments.Count; i++){
      entry.comments[i] = GUILayout.TextField(entry.comments[i], GUILayout.Width(Screen.width / 2));
    }
    for (int i = 0; i < entry.translations.Count; i++){
      entry.translations[i] = GUILayout.TextField(entry.translations[i], GUILayout.Width(Screen.width/2));
    }
    GUILayout.EndVertical();
  }

  private void LoadPOFile(){
    if (poTextAsset != null){
      po = new wwPO();
      po.Parse(poTextAsset.text);
      defaultColor = GUI.backgroundColor;
      isDirty = false;
      EditorUtility.DisplayDialog("Load Complete", "Loaded:"+poTextAsset.name, "OK");
    }
  }

  private void SavePOFile(){
    if (po != null){
      WW.SaveLoad.wwDataSaveLoadManagerStatic.Save(po.ToString(), Application.dataPath+poSavePath);
    }
  }
}
#endif
