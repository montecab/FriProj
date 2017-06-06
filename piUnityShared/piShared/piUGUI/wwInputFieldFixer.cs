using UnityEngine;
using UnityEngine.UI;
using System.Collections;


[RequireComponent (typeof (InputField))]
public class wwInputFieldFixer : MonoBehaviour {

  public float offset = 0;
  private InputField theInputField;
  

  // Use this for initialization
  void Start () {
  
    theInputField = GetComponent<InputField>();
    if (theInputField == null) {
      WWLog.logError("no InputField component. de-activating.");
      gameObject.SetActive(false);
      return;
    }
  }
  
  private bool fixit = true;
  
  void Update() {
    if (theInputField.isFocused) {
      if (fixit) {
        fixit = false;
        GameObject caret = transform.GetChild(0).gameObject;
        if (caret != null) {
          caret.transform.localPosition =  Vector3.Scale(caret.transform.localPosition, new Vector3(1, 0, 1)) + new Vector3(0, offset, 0);
          
        }
        else {
          WWLog.logError("configuration error: null caret");
        }
      }
    }
    else {
      fixit = true;
    }
  }
  
}
