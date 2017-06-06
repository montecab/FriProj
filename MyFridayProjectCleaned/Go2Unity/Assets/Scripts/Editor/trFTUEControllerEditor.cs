using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(trFTUEController))]
public class trFTUEControllerEditor : Editor {

  private static readonly string RESOURCES_PATH = "Assets/Resources/";
  private FTUEType _type = FTUEType.CHROME;

  public override void OnInspectorGUI()
  {
    base.OnInspectorGUI();
    EditorGUILayout.Space();
    Color oriColor = GUI.color;

    EditorGUILayout.Space();
    _type = (FTUEType) EditorGUILayout.EnumPopup("FTUE type:", _type);
    GUI.color = Color.green;
    if(GUILayout.Button("Load "+_type.ToString()+" settings")){
      trFTUEController myTarget = (trFTUEController)target;
      FTUEModel model = Resources.Load<FTUEModel>(FTUEManager.FTUE_MODELS_PATH+_type.ToString());
      myTarget.FromFTUEModel(model);
    }
    EditorGUILayout.Space();
    GUI.color = Color.red;
    if(GUILayout.Button("Save "+_type.ToString()+" settings")){
      trFTUEController myTarget = (trFTUEController)target;
      FTUEModel model = ScriptableObject.CreateInstance<FTUEModel>();
      model.ftueType = _type;
      model = myTarget.ToFTUEModel(model);
      AssetDatabase.CreateAsset(model, RESOURCES_PATH+FTUEManager.FTUE_MODELS_PATH+_type.ToString()+".asset");
      AssetDatabase.SaveAssets();
      EditorGUIUtility.PingObject(model);
    }
    GUI.color = oriColor;
  }
}
