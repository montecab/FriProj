using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;

[RequireComponent (typeof(CanvasGroup))]
public class trEyeRingView : MonoBehaviour {

  public Toggle[] EyeSegments;

  public OnValueChanged OnChanged;

  private int serializedValue = 0;
  public int SerializedValue {
    get {
      return serializedValue;
    }
    set {
      serializedValue = value;
      updateView();
    }
  }

  private bool isToggleEnabled = true;
  public bool ToggleEnabled{
    get {
      return isToggleEnabled;
    }
    set {
      isToggleEnabled = value;
      updateTogglesEnable();
    }
  }

	void Awake(){
    for (int i = 0; i < EyeSegments.Length; i++){
      EyeSegments[i].onValueChanged.AddListener(onToggleChanged);
    }
    updateSerializedValue();
  }

  void Start(){
    Vector2 viewSize = GetComponent<RectTransform>().GetSize();
    float minSide = Mathf.Min(viewSize.x, viewSize.y);
    float radius = minSide / 2;

    foreach(Toggle toggle in EyeSegments){
      Vector3 position = toggle.transform.localPosition;

      float angle = position.z * Mathf.Deg2Rad;
      position.x = Mathf.RoundToInt(radius * Mathf.Sin(angle));
      position.y = Mathf.RoundToInt(radius * Mathf.Cos(angle) - radius);

      toggle.transform.localPosition = position;
    }
  }

  void OnDestroy(){
    if(EyeSegments!=null){
      for (int i = 0; i < EyeSegments.Length; i++){
        EyeSegments[i].onValueChanged.RemoveListener(onToggleChanged);
      }
    }
  }

  private void updateTogglesEnable(){
    foreach(Toggle toggle in EyeSegments){
      toggle.interactable = isToggleEnabled;
    }
    CanvasGroup group = GetComponent<CanvasGroup>();
    group.interactable = isToggleEnabled;
    group.blocksRaycasts = isToggleEnabled;
  }

  private void updateView() {
    bool[] segments = piMathUtil.deserializeBoolArray(serializedValue);
    if (segments.Length != EyeSegments.Length){
      WWLog.logError("deserialization error");
    }
    for (int i = 0; i < segments.Length; i++){
      Toggle toggle = EyeSegments[i];
      toggle.isOn = segments[i];
      toggle.interactable = isToggleEnabled;
    }
  }

  private void onToggleChanged(bool value){
    updateSerializedValue();
    if (OnChanged != null){
      OnChanged.Invoke(this);
    }
  }

  private void updateSerializedValue(){
    bool[] segments = new bool[EyeSegments.Length];
    for (int i = 0; i < EyeSegments.Length; i++){
      segments[i] = EyeSegments[i].isOn;
    }
    
    serializedValue = piMathUtil.serializeBoolArray(segments);
  }

  [System.Serializable]
  public class OnValueChanged : UnityEvent<trEyeRingView>{};
}
