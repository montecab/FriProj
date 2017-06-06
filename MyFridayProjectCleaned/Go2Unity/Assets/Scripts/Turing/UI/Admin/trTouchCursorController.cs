using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Turing{
  public class trTouchCursorController : MonoBehaviour{

    private Image[] _images;
    private bool _allDisabled;
    private Vector3 _canvasSize;

    void Awake(){
      trMultivariate.Instance.ValueChanged += onOptionValueChanged;
    }

    void Start() {
      _images = GetComponentsInChildren<Image>();
      disableAllImages();
      trMultivariate.trAppOptionValue value = trMultivariate.Instance.getOptionValue(trMultivariate.trAppOption.SHOW_TOUCH_CURSOR);
      onOptionValueChanged(trMultivariate.trAppOption.SHOW_TOUCH_CURSOR, value);
      _canvasSize = GetComponent<RectTransform>().sizeDelta;
    }

    void Update(){
      if (_allDisabled && (Input.touchCount == 0)) {
        return;
      }
        
      if (Input.touchCount == 0) {
        disableAllImages();
        return;
      }

      for (int i=0; i<_images.Length; i++) {
        if (Input.touchCount > i) {
          _images[i].enabled = true;
          Vector3 pos = Camera.main.ScreenToViewportPoint(Input.GetTouch(i).position);
          Vector3 anchoredPos = Vector3.Scale(pos, _canvasSize);
          (_images[i].transform as RectTransform).anchoredPosition3D = anchoredPos;
        } else {
          _images[i].enabled = false;
        }
      }
      _allDisabled = false;
    }

    void OnDestroy(){
      trMultivariate.Instance.ValueChanged -= onOptionValueChanged;
    }

    private void disableAllImages(){
      for (int i = 0; i < _images.Length; i++){
        _images[i].enabled = false;
      }
      _allDisabled = true;
    }

    private void onOptionValueChanged (trMultivariate.trAppOption option, trMultivariate.trAppOptionValue newValue) {
      if (option == trMultivariate.trAppOption.SHOW_TOUCH_CURSOR) {
        gameObject.SetActive(newValue == trMultivariate.trAppOptionValue.YES);
      }
    }

  }
}
