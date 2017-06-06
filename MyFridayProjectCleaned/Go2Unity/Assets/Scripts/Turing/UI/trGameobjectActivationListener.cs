using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Turing;
using UnityEngine.Events;

public class trGameobjectActivationListener : MonoBehaviour {

  private Dictionary<int, bool> savedViewState = new Dictionary<int, bool>();

  public trMultivariate.trAppOption OptionToListen;
  public trMultivariate.trAppOptionValue PositiveOption;
  public trMultivariate.trAppOptionValue NegativeOption;

  [System.Serializable]
  public class OnValueChangedEvent : UnityEvent<bool>{};

  public OnValueChangedEvent OnValueChanged = new OnValueChangedEvent();

	void Awake(){
    trMultivariate.Instance.ValueChanged += onOptionValueChanged;
  }

  void Start(){
    trMultivariate.trAppOptionValue value = trMultivariate.Instance.getOptionValue(OptionToListen);
    if (value == PositiveOption || value == NegativeOption){
      setObjectActive(value == PositiveOption);
    }
  }

  void OnDestroy(){
    if (trMultivariate.Instance != null){
      trMultivariate.Instance.ValueChanged -= onOptionValueChanged;
    }
  }

  void onOptionValueChanged (trMultivariate.trAppOption option, trMultivariate.trAppOptionValue newValue) {
    if (option == OptionToListen && (newValue == PositiveOption || newValue == NegativeOption)){
      setObjectActive(newValue == PositiveOption);
    }
  }

  void setObjectActive(bool active){
    if (active){
      restoreState(transform);
    } else {
      savedViewState.Clear();
      saveState(transform, false);
    }
    OnValueChanged.Invoke(active);
  }

  void saveState(Transform instance, bool activate){
    savedViewState[instance.gameObject.GetInstanceID()] = instance.gameObject.activeSelf;
    if (instance != transform){
      instance.gameObject.SetActive(activate);
    }
    if (instance.childCount > 0){
      for (int i = 0; i < instance.childCount; i++){
        saveState(instance.GetChild(i), activate);
      }
    }
  }

  void restoreState(Transform instance){
    int key = instance.gameObject.GetInstanceID();
    if (savedViewState.ContainsKey(key)){
      instance.gameObject.SetActive(savedViewState[key]);
    }
    if (instance.childCount > 0){
      for (int i = 0; i < instance.childCount; i++){
        restoreState(instance.GetChild(i));
      }
    }
  }
}
