using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(wwSceneComment))]
public class wwEditorScripts : Editor {
  public override void OnInspectorGUI ()
  {
    wwSceneComment item = (wwSceneComment)target;

    EditorStyles.textField.wordWrap = true;
    EditorStyles.textArea.wordWrap = true;

    EditorGUILayout.LabelField("Comment");
    item.comment = EditorGUILayout.TextArea(item.comment);

    if (string.IsNullOrEmpty(item.addedBy)) {
      item.addedBy = System.Environment.UserName;
    }
    EditorGUILayout.LabelField("Added By");
    item.addedBy = EditorGUILayout.TextField(item.addedBy);

  }
}
