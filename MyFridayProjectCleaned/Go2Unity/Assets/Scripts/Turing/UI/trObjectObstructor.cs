using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class trObjectObstructor : MonoBehaviour {

	public List<trObstructorListener> Listeners = new List<trObstructorListener>();

  void OnEnable(){
    foreach(trObstructorListener listener in Listeners){
      listener.AddObstructer();
    }
  }

  void OnDisable(){
    foreach(trObstructorListener listener in Listeners){
      listener.RemoveObstructor();
    }
  }
}
