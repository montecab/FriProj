using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;

public class TouchScrollRect : ScrollRect{

  [SerializeField]
  private GameObject _zoomObject;

  // where the zoom should be limited in, the pivot is (0.5,0.5),
  //this gameobject should have the same parent with zoomobj
  //so we can make sure it doesn't include any scale effect
  [SerializeField]
  private RectTransform _zoomArea;
  [SerializeField]
  private trFingerOnObject _fingerOnCtrlObject;
  [SerializeField]
  private float _zoomMax = 1.5f;
  [SerializeField]
  private float _zoomMin = 0.5f;

  private float _zoomFactor = 1;

  private Vector2 _touchPos0;
  private Vector2 _touchPos1;

  private Vector2 _dragPosition;
  private Vector2 _currentTouchPosition;
  private Vector2 _lastTouchPosition;
  private int _lastTouchCount;

  private int _dragCount;

  private float minX, minY, maxX, maxY;


  protected override void Start(){
    _zoomFactor = _zoomObject.transform.localScale.x;
    base.Start();
  }

  void ProcessTouches(){
    int touchCount = Mathf.Clamp(Input.touchCount, 0, 2);

    if(touchCount == 0){
      _lastTouchCount = 0;
      return;
    }

    if(touchCount == 1){
      _currentTouchPosition = Input.GetTouch(0).position;
    } else{
      _currentTouchPosition = (Input.GetTouch(0).position + Input.GetTouch(1).position) * 0.5f;
    }

    // initialize the dragPosition if the user wasn't touching last frame
    if(_lastTouchCount == 0){
      _dragPosition = _currentTouchPosition;
    }
      
    // don't immediately move dragPosition if the touchCount changes
    if(touchCount == _lastTouchCount){
      _dragPosition += _currentTouchPosition - _lastTouchPosition;
    } 

    // update the state
    _lastTouchCount = touchCount;
    _lastTouchPosition = _currentTouchPosition;
  }


  void Update(){

    #if !UNITY_ANDROID && !UNITY_IPHONE
    if(Input.GetKey(KeyCode.UpArrow)){
      _zoomFactor = Mathf.Clamp(_zoomFactor - (10 / (Screen.width / 2.0f)), _zoomMin, _zoomMax);
      _zoomObject.transform.localScale = Vector3.one * _zoomFactor;
      LimitZoomArea();
    } else if(Input.GetKey(KeyCode.DownArrow)){
      _zoomFactor = Mathf.Clamp(_zoomFactor + (10 / (Screen.width / 2.0f)), _zoomMin, _zoomMax);
      _zoomObject.transform.localScale = Vector3.one * _zoomFactor;
      LimitZoomArea();
    }
    #endif

    ProcessTouches();

    bool notTouchingObject = true;

    if (EventSystem.current != null) {
    	notTouchingObject = EventSystem.current.currentSelectedGameObject == null;
    }

    if(_fingerOnCtrlObject != null){
      notTouchingObject &= _fingerOnCtrlObject.IsTwoFingerOnObject;
    }
      
    if(_lastTouchCount == 2){
      if(notTouchingObject
         && Input.GetTouch(0).phase == TouchPhase.Moved
         && Input.GetTouch(1).phase == TouchPhase.Moved){

        Vector2 currPos0 = Input.GetTouch(0).position;  
        Vector2 currPos1 = Input.GetTouch(1).position;  
        Vector2 prevPos0 = _touchPos0;
        Vector2 prevPos1 = _touchPos1;

        Vector2 currDist = currPos0 - currPos1; // distance between current  finger touches
        Vector2 prevDist = prevPos0 - prevPos1; // distance between previous finger touches

        float deltDist = currDist.magnitude - prevDist.magnitude;

        _zoomFactor = Mathf.Clamp(_zoomFactor + (deltDist / (Screen.width / 2.0f)), _zoomMin, _zoomMax);
        _zoomObject.transform.localScale = Vector3.one * _zoomFactor;
      }

      _touchPos0 = Input.GetTouch(0).position;
      _touchPos1 = Input.GetTouch(1).position;
    }
  }

  // OnBeginDrag is called for each multi-touch
  // For the first touch, replace the position of the touch with our custom location
  // and pass the eventData to the base class
  public override void OnBeginDrag(PointerEventData eventData){
    _dragCount += 1;
    if(_dragCount > 1){
      return;
    }
    if(_lastTouchCount == 0){
      ProcessTouches();
    }
    if (_lastTouchCount != 0) {
      eventData.position = _dragPosition;
    }
    base.OnBeginDrag(eventData);
  }

  // Replace the position of the touch with our custom location
  // and pass the eventData to the base class
  public override void OnDrag(PointerEventData eventData){
    if(_lastTouchCount == 0){
      ProcessTouches();
    }
    if (_lastTouchCount != 0) {
      eventData.position = _dragPosition;
    }
    base.OnDrag(eventData);
  }

  // OnEndDrag is called for each multi-touch
  // For the final touch, replace the position of the touch with our custom location
  // and pass the eventData to the base class
  public override void OnEndDrag(PointerEventData eventData){
    _dragCount -= 1;
    if(_dragCount > 0){
      return;
    }
    if (_lastTouchCount != 0) {
      eventData.position = _dragPosition;
    }
    base.OnEndDrag(eventData);
  }

  private void LimitZoomArea(){
    if(_zoomArea != null){
      minX = _zoomArea.transform.localPosition.x + _zoomArea.GetWidth() / 2.0f - _zoomObject.GetComponent<RectTransform>().GetWidth() * _zoomFactor / 2.0f;
      maxX = _zoomArea.transform.localPosition.x - _zoomArea.GetWidth() / 2.0f + _zoomObject.GetComponent<RectTransform>().GetWidth() * _zoomFactor / 2.0f;
      minY = _zoomArea.transform.localPosition.y + _zoomArea.GetHeight() / 2.0f - _zoomObject.GetComponent<RectTransform>().GetHeight() * _zoomFactor / 2.0f;
      maxY = _zoomArea.transform.localPosition.y - _zoomArea.GetHeight() / 2.0f + _zoomObject.GetComponent<RectTransform>().GetHeight() * _zoomFactor / 2.0f;

      if(minX > maxX){
        float tmp = maxX;
        maxX = minX;
        minX = tmp;
      }

      if(minY > maxY){
        float tmp = maxY;
        maxY = minY;
        minY = tmp;
      }

      //      Debug.LogError("minX: " + minX + ", maxX: " + maxX + ", minY: " + minY + ", maxY: " + maxY );

      float x = _zoomObject.transform.localPosition.x;
      float y = _zoomObject.transform.localPosition.y;

      x = Mathf.Clamp(x, minX, maxX);
      y = Mathf.Clamp(y, minY, maxY);
      _zoomObject.transform.localPosition = new Vector3(x, y, _zoomObject.transform.localPosition.z);
    }   
  }

}
