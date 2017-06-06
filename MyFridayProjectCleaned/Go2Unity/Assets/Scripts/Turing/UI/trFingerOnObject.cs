using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;


//this script is to help decide when to pinch, we want to pinch zoom only when two fingers are on the bg canvas not on any element

public class trFingerOnObject : MonoBehaviour, IPointerDownHandler {

  private List<int> fingerIds = new List<int>();

  public bool IsTwoFingerOnObject{
    get{
      return fingerIds.Count == 2 && Input.touchCount ==2;
    }
  }

  void Update(){
    //Remove finger ids using this dumb solution because scrolling the object is considered as PointerUp event
    for(int i = fingerIds.Count - 1; i >= 0; i--){
      bool isFound = false;
      foreach(Touch touch in Input.touches){
        if(touch.fingerId == fingerIds[i]){
          isFound = true;
          break;
        }
      }
      if(!isFound){
        fingerIds.RemoveAt(i);

      }
    }
  }

  #region IPointerDownHandler implementation
  public void OnPointerDown (PointerEventData eventData)
  {
    if(!fingerIds.Contains(eventData.pointerId)){
      fingerIds.Add(eventData.pointerId);
    }
  }
  #endregion
}
