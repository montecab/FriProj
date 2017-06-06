using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System.Collections;

namespace Turing{

  public class trMomentaryButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {

    public delegate void MomentaryButtonDelegate ();

    public MomentaryButtonDelegate OnPointerDownEvent;
    public MomentaryButtonDelegate OnPointerUpEvent;

    public float minimumPressTime;

    bool waitingForMinimumPressTime;
    float pressTime;

    public void OnPointerDown(PointerEventData eventData){
      if (!waitingForMinimumPressTime) {
        waitingForMinimumPressTime = true;
        pressTime = Time.time + minimumPressTime;
        StartCoroutine(futurePress());
      }
    }

    public void OnPointerUp(PointerEventData eventData){
      waitingForMinimumPressTime = false;
      if (OnPointerUpEvent != null){
        OnPointerUpEvent();
      }
    }

    IEnumerator futurePress() {
      while (waitingForMinimumPressTime && (Time.time < pressTime)) {
        yield return new WaitForEndOfFrame();
      }
      if (waitingForMinimumPressTime) {
        if (OnPointerDownEvent != null){
          OnPointerDownEvent();
        }
      }
    }
  }
}
