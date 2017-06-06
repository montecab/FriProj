using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

[RequireComponent (typeof(Slider))]
public class trSliderExtendedThumb : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler {

  public GameObject ThumbObjectPrefab;
  public Position PreferedLocation = Position.Left;
  public float ThumbOffset = 10;

  private trThumbWithLabel thumbObject;
  private Slider slider;

  private string initialValue = "";
  private string initialUnitsValue = "";

  private bool isInit = false;

  void Awake(){
    slider = GetComponent<Slider>();
  }

  void OnDisable(){
    if (thumbObject != null){
      Destroy(thumbObject.gameObject);
    }
  }

  public void SetValueAndUnit(string value, string units){
    if (thumbObject != null){
      thumbObject.LabelValue.text = value;
      thumbObject.LabelUnits.text = units;
    } else {
      initialValue = value;
      initialUnitsValue = units;
      isInit = false;
    }
  }

  public void OnPointerDown (PointerEventData eventData) {
    if (PreferedLocation != Position.Automatic && PreferedLocation != Position.Hidden){
      showThumbObject();
    }
  }

  public void OnPointerUp (PointerEventData eventData) {
    hideThumbObject();
  }

  public void OnDrag (PointerEventData eventData) {
    updateThumbPosition();
  }

  private void updateThumbPosition(){
    if (thumbObject != null){
      thumbObject.transform.position = getThumbNewPosition();
    }
  }

  private Vector3 getThumbNewPosition(){
    Vector3 position = slider.handleRect.position;
    Vector2 thumbSize = thumbObject.GetComponent<RectTransform>().GetSize() / 2;
    switch(PreferedLocation){
      case Position.Left:
        position.x -= thumbSize.x + ThumbOffset;
        break;
      case Position.Right:
        position.x += thumbSize.x + ThumbOffset;
        break;
      case Position.Below:
        position.y -= thumbSize.y + ThumbOffset;
        break;
      case Position.Above:
        position.y += thumbSize.y + ThumbOffset;
        break;
    }

    return position;
  }

  void hideThumbObject(){
    if (thumbObject != null){
      Vector3 scale = Vector3.one * (slider.handleRect.GetSize().y / thumbObject.GetComponent<RectTransform>().GetSize().y);
      thumbObject.transform.DOScale(scale, 0.1f);
      thumbObject.transform.DOMove(slider.handleRect.position, 0.1f)
        .OnComplete(() => {
         thumbObject.gameObject.SetActive(false);
        });
    }
  }

  void showThumbObject(){
    if (thumbObject != null){
      thumbObject.gameObject.SetActive(true);
    }
    else{
      GameObject thumb = Instantiate(ThumbObjectPrefab) as GameObject;
      thumbObject = thumb.GetComponent<trThumbWithLabel>();
      thumb.transform.SetParent(transform);
      
      if(!isInit){
        SetValueAndUnit(initialValue, initialUnitsValue);
        isInit = true;
      }   
      
      Vector3 scale = Vector3.one * (slider.handleRect.GetSize().y / thumbObject.GetComponent<RectTransform>().GetSize().y);     
      thumb.transform.localScale = scale;
    }

    thumbObject.gameObject.transform.position = slider.handleRect.position;
    thumbObject.gameObject.transform.DOScale(Vector3.one, 0.1f);
    thumbObject.gameObject.transform.DOMove(getThumbNewPosition(), 0.1f);
  }

  [System.Serializable]
  public enum Position {
    Left, Right, Above, Below, Hidden,
    Automatic //not implemented
  }
}
