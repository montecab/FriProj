using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class trObstructorListener : MonoBehaviour {

	private int obstructorsCount = 0;

  public void AddObstructer(){
    obstructorsCount++;
    if (obstructorsCount > 0){
      setObjectActive(false);
    }
  }

  public void RemoveObstructor(){
    obstructorsCount--;
    if (obstructorsCount <= 0){
      setObjectActive(true);
      if (obstructorsCount < 0) {
        WWLog.logError("obstructor count is negative.");
      }
    }
  }

  void setObjectActive(bool active){
    if (active){
      obstructorsCount = 0;      
      transform.localPosition = new Vector3(0, 0, 20);
    }
    else {
      // "hide" the object by moving it way out of the camera's view frustum.
      // this is probably less effective at improving performance than using SetActive(false),
      // but that method introduces other difficulties. see TUR-1350.
      transform.localPosition = new Vector3(0, 0, 10000);
    }
  }
}
