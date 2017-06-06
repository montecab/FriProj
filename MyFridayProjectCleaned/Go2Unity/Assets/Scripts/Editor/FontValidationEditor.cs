#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;

public class FontValidationEditor : EditorWindow {

  private static string PATH_PO_FOLDER{
    get{
      return "Strings/" + BuildInfo.AppVersion;
    }
  }
  private static readonly string PREFAB_FONT_VALIDATION = "Assets/Prefabs/TuringProto/FontValidation.prefab";

  [MenuItem("WW/Localization/Validate characters in Font", false, 2)]
  private static void ValidateCharactersInFont(){
    TMPro.TextMeshProUGUI.missingCharacters = "";
    Object prefab = AssetDatabase.LoadAssetAtPath(PREFAB_FONT_VALIDATION, typeof(GameObject));
    GameObject gameObject = Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;
    TMPro.TextMeshProUGUI TMPText = gameObject.GetComponentInChildren<TMPro.TextMeshProUGUI>();
    Object[] files = Resources.LoadAll(PATH_PO_FOLDER);
    foreach (Object file in files){
      if (file.name.EndsWith(".po")){
        Debug.Log("Checking PO file:" + file.name);
        TextAsset testAsset = Resources.Load(PATH_PO_FOLDER + "/" + file.name, typeof(TextAsset)) as TextAsset;
        wwPO temp = new wwPO();
        temp.Parse(testAsset.text);
        foreach (string key in temp.dictionary.Keys){
          wwPOEntry entry = temp.dictionary[key];
          foreach (string translation in entry.translations){
            TMPText.text = translation;
            TMPText.Rebuild(UnityEngine.UI.CanvasUpdate.PreRender);
          }
        }
      }
    }
    DestroyImmediate(gameObject);
    if (TMPro.TextMeshProUGUI.missingCharacters.Length > 0){
      Debug.Log(TMPro.TextMeshProUGUI.missingCharacters);
      Debug.LogWarning("Missing characters from the font! To fix the issue:\n"+
                       "1. Copy the missing characters above.\n"+
                       "2. Window->TextMeshPro - Font Assset Creator\n"+
                       "3. Drag the font file into [Font source] and set [Atlas Resolution] to 2048x2048\n"+
                       "4. Set [Character Set] to \"Custom Characters\" and drag the font asset into [Select Font Asset]\n"+
                       "4. Add the missing characters to the Custom Character List\n"+
                       "5. [Generate Font Atlas](5-10 mins) and [Save TextMeshPro Font Asset](Replace the original one)\n");
    } else{
      Debug.Log("Validation complete. No missing characeters found");
    }
  }
}
#endif
