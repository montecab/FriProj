using UnityEngine;
using UnityEngine.EventSystems;

public class PinchZoom : MonoBehaviour
{
  public GameObject ZoomObj;

  // where the zoom should be limited in, the pivot is (0.5,0.5), 
  //this gameobject should have the same parent with zoomobj 
  //so we can make sure it doesn't include any scale effect
  public RectTransform ZoomArea; 

  public float ZOOM_MAX = 1.5f;
  public float ZOOM_MIN = 0.5f;

  public trFingerOnObject FingerOnObjectCtrl;

  private float zoomFactor = 1;
  
  private Vector2 prevTouchPos0;
  private Vector2 prevTouchPos1;
  
  float minX, minY, maxX, maxY;

  void Start(){
    zoomFactor = ZoomObj.transform.localScale.x;
  }

  void setContentZoom(Vector3 zoom){
    ZoomObj.transform.localScale = zoom;
    LimitZoomArea();
  }

  void Update()
  {
    bool isTouchRight = Input.touchCount == 2;
    if(FingerOnObjectCtrl != null){
      isTouchRight = isTouchRight && FingerOnObjectCtrl.IsTwoFingerOnObject;
    }
     
    if (isTouchRight
        && Input.GetTouch (0).phase == TouchPhase.Moved 
        && Input.GetTouch (1).phase == TouchPhase.Moved) {

      bool isTwoFingerOnRequiredObject = EventSystem.current.currentSelectedGameObject == null ;


      if(!isTwoFingerOnRequiredObject){
        return;
      }
      
      Vector2 currPos0 = Input.GetTouch(0).position;  
      Vector2 currPos1 = Input.GetTouch(1).position;  
      Vector2 prevPos0 = prevTouchPos0;
      Vector2 prevPos1 = prevTouchPos1;
      
      Vector2 currDist = currPos0 - currPos1; // distance between current  finger touches
      Vector2 prevDist = prevPos0 - prevPos1; // distance between previous finger touches
      
      float deltDist = currDist.magnitude - prevDist.magnitude;
      
      zoomFactor = Mathf.Clamp(zoomFactor + (deltDist / (Screen.width / 2.0f)), ZOOM_MIN , ZOOM_MAX);
      setContentZoom(Vector3.one * zoomFactor);
      
      LimitZoomArea();
    }
    
    if (isTouchRight) {
      prevTouchPos0 = Input.GetTouch(0).position;
      prevTouchPos1 = Input.GetTouch(1).position;
    }
  
#if !UNITY_ANDROID && !UNITY_IPHONE
    if(Input.GetKey(KeyCode.UpArrow)){
      zoomFactor = ZoomObj.transform.localScale.x - 0.01f;
      zoomFactor = Mathf.Clamp(zoomFactor, ZOOM_MIN, ZOOM_MAX);
      setContentZoom(Vector3.one * zoomFactor);
      LimitZoomArea();
    } else if(Input.GetKey(KeyCode.DownArrow)){
      zoomFactor = ZoomObj.transform.localScale.x + 0.01f;
      zoomFactor = Mathf.Clamp(zoomFactor, ZOOM_MIN, ZOOM_MAX);
      setContentZoom(Vector3.one * zoomFactor);
      LimitZoomArea();
    }
#endif

  }


  public void LimitZoomArea(){
    if(ZoomArea != null){
      minX = ZoomArea.transform.localPosition.x + ZoomArea.GetWidth()/2.0f - ZoomObj.GetComponent<RectTransform>().GetWidth() * zoomFactor/2.0f;
      maxX = ZoomArea.transform.localPosition.x - ZoomArea.GetWidth()/2.0f + ZoomObj.GetComponent<RectTransform>().GetWidth() * zoomFactor/2.0f;
      minY = ZoomArea.transform.localPosition.y + ZoomArea.GetHeight()/2.0f - ZoomObj.GetComponent<RectTransform>().GetHeight() * zoomFactor/2.0f;
      maxY = ZoomArea.transform.localPosition.y - ZoomArea.GetHeight()/2.0f + ZoomObj.GetComponent<RectTransform>().GetHeight() * zoomFactor/2.0f;

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
      
      float x = ZoomObj.transform.localPosition.x;
      float y = ZoomObj.transform.localPosition.y;

      x = Mathf.Clamp(x, minX, maxX);
      y = Mathf.Clamp(y, minY, maxY);
      ZoomObj.transform.localPosition = new Vector3(x, y, ZoomObj.transform.localPosition.z);
    }   
  }
}