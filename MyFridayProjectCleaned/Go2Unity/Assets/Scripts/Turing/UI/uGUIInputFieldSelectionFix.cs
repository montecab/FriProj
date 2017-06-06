using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class uGUIInputFieldSelectionFix : InputField {

  //FIX TUR-317
  //To fix this issue http://issuetracker.unity3d.com/issues/gui-dot-textfield-cursor-position-y-offset-is-wrong-if-font-size-is-changed
  public override void OnSelect (BaseEventData eventData){
    base.OnSelect(eventData);
    GameObject inputCaret = transform.GetChild(0).gameObject;
    if (inputCaret != null && inputCaret.name == "InputField Input Caret"){
      Vector3 localPosition = inputCaret.transform.localPosition;
      localPosition.y = -getOffsetForHeight(inputCaret.GetComponent<RectTransform>().GetSize().y);
      inputCaret.transform.localPosition = localPosition;
    }
  }

  float getOffsetForHeight(float height){
    float result = 3.25f - height * 0.05f;
    return result * height;
  }
}
