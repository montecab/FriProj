using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class trDraggingScript : MonoBehaviour, IDragHandler {

  public void OnDrag (PointerEventData eventData)
  {
    transform.localPosition += new Vector3(eventData.delta.x, eventData.delta.y, 0);
  }
	
}
