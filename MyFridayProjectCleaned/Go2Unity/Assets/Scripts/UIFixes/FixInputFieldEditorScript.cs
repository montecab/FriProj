
#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
public class FixInputFieldEditorScript: EditorWindow 
{
  static int go_count = 0, components_count = 0;

  [MenuItem("Window/Find Missing Scripts (All)")]
  static void FindInAll()
  {
    go_count = 0;
    components_count = 0;
    foreach (var root in SceneRoots()) 
    {
      //Debug.Log(root);
      FindInGO(root);
    }
  }

  static void FindInGO(GameObject g)
  {
    go_count++;
    Component[] components = g.GetComponents<Component>();
    for (int i = components.Length - 1; i >=0; i--)
    {
      components_count++;
      if (components[i] is InputField)
      {
        InputField input = (InputField)components[i];
        Text t = input.textComponent;
        Graphic placeholder = input.placeholder;
        InputField.LineType lineT = input.lineType;
        DestroyImmediate(components[i]);
        InputFieldFixDoublePaste fixedInput = g.AddComponent<InputFieldFixDoublePaste>();
        fixedInput.textComponent = t;
        fixedInput.placeholder = placeholder;
        fixedInput.lineType = lineT;
        Debug.Log ("changed an Inputfield script attached in position: " + i, g);
      }
    }
    // Now recurse through each child GO (if there are any):
    foreach (Transform childT in g.transform)
    {
      //Debug.Log("Searching " + childT.name  + " " );
      FindInGO(childT.gameObject);
    }
  }

  static IEnumerable<GameObject> SceneRoots()
  {
    var prop = new HierarchyProperty(HierarchyType.GameObjects);
    var expanded = new int[0];
    while (prop.Next(expanded)) {
      yield return prop.pptrValue as GameObject;
    }
  }
}

#endif