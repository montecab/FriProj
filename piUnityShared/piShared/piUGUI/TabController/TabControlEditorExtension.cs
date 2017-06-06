using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(wwTabControl))]
public class TabControlEditorInspector: UnityEditor.Editor {

  [UnityEditor.MenuItem("GameObject/UI/WW/TabControl")]
  static void CreateTabControl() {
    GameObject control = wwTabControl.CreateDefaultTabControl();

    if (UnityEditor.Selection.activeTransform != null) {
      control.transform.SetParent(UnityEditor.Selection.activeTransform, false);
      control.GetComponent<RectTransform>().SetPositionOfPivot(Vector3.zero);
      control.GetComponent<RectTransform>().SetDefaultScale();
    }
  }

  public override void OnInspectorGUI () {
    UnityEditor.EditorGUILayout.LabelField("    Top Level Objects Linking", UnityEditor.EditorStyles.boldLabel);
    UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("containerPanel"), true);
    UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("tabsPanel"), true);
    UnityEditor.EditorGUILayout.LabelField("    Configuration File Settings", UnityEditor.EditorStyles.boldLabel);
    UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("SourceJsonFile"), true);
    UnityEditor.EditorGUILayout.LabelField("    Mechanics", UnityEditor.EditorStyles.boldLabel);
    UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("SwipeSnap"), true);
    UnityEditor.EditorGUILayout.LabelField("    Default UI Settings", UnityEditor.EditorStyles.boldLabel);
    UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("DefaultTabColor"), true);
    UnityEditor.EditorGUILayout.LabelField("        Tab Button Settings", UnityEditor.EditorStyles.boldLabel);
    UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("TabButtonTintColor"), true);
    UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("selectedTabIndex"), true);
    UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("TabButtonMaskSprite"), true);
    UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("TabButtonSpacing"), true);

    if (GUILayout.Button("Regenerate From JSON")) {
      _RegenerateFromJson();
    }

    if (GUILayout.Button("Add Empty Tab")) {
      _AddNewTab();
    }

    UnityEditor.EditorGUILayout.LabelField("    Content", UnityEditor.EditorStyles.boldLabel);
    UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("tabItems"), true);
    serializedObject.ApplyModifiedProperties();
    UnityEditor.EditorGUILayout.LabelField("    Behavior", UnityEditor.EditorStyles.boldLabel);
    UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("OnSelectedEvent"), true);
  }

  private void _RegenerateFromJson() {
    wwTabControl tabControl =  (serializedObject.targetObject as wwTabControl);

    if (tabControl.SourceJsonFile == null) {
      UnityEditor.EditorUtility.DisplayDialog("No parameter", "You didn't assign SourceJsonFile parameter", "Close");
      return;
    }

    UnityEditor.SerializedProperty tabs = serializedObject.FindProperty("tabItems");

    for (int i = 0; i < tabs.arraySize; i++) {
      GameObject contentPanel = tabs.GetArrayElementAtIndex(i).FindPropertyRelative("itemContentPanel").objectReferenceValue as GameObject;
      Button button = tabs.GetArrayElementAtIndex(i).FindPropertyRelative("activationButton").objectReferenceValue as Button;
      DestroyImmediate(contentPanel);
      DestroyImmediate(button.gameObject);
      tabs.GetArrayElementAtIndex(i).FindPropertyRelative("contentItems").arraySize = 0;
    }

    tabs.arraySize = 0;
    serializedObject.ApplyModifiedProperties();
    tabControl.RegenerateControl(clear: false);
    List<wwTabItem> local = new List<wwTabItem>(tabControl.TabsObjects);

    foreach (wwTabItem tabItem in local) {
      tabs.arraySize++;
      serializedObject.ApplyModifiedProperties();
      tabs.GetArrayElementAtIndex(tabs.arraySize - 1).FindPropertyRelative("itemContentPanel").objectReferenceValue = tabItem.ContentPanel;
      tabs.GetArrayElementAtIndex(tabs.arraySize - 1).FindPropertyRelative("activationButton").objectReferenceValue = tabItem.TabButton;
      serializedObject.ApplyModifiedProperties();
    }
  }

  private void _AddNewTab() {
    wwTabControl tabControl =  (serializedObject.targetObject as wwTabControl);
    wwTabItem tabItem = tabControl.CreateDefaultTabItem(null, tabControl.TabsObjects.Count);
    UnityEditor.SerializedProperty tabs = serializedObject.FindProperty("tabItems");
    tabs.arraySize++;
    serializedObject.ApplyModifiedProperties();
    tabs.GetArrayElementAtIndex(tabs.arraySize - 1).FindPropertyRelative("itemContentPanel").objectReferenceValue = tabItem.ContentPanel;
    tabs.GetArrayElementAtIndex(tabs.arraySize - 1).FindPropertyRelative("activationButton").objectReferenceValue = tabItem.TabButton;
    serializedObject.ApplyModifiedProperties();
  }
}

#endif
