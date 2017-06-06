using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;


[RequireComponent (typeof (EventSystem))]
public class wwEventSystemFitter : MonoBehaviour {
	// Use this for initialization
	void Start () {
    //change drag threshold based on the resolution so click is not considered as dragging on high resolution devices
    this.gameObject.GetComponent<EventSystem>().pixelDragThreshold = Screen.height/20;
	}
}
