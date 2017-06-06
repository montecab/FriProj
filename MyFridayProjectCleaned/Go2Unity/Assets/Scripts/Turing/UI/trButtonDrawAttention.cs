using UnityEngine;
using System.Collections;

public class trButtonDrawAttention : MonoBehaviour {

	public bool IsActive;
  private const float TIMER = 1.1f;

  void Update(){
    if(IsActive){
      float v = Time.fixedTime % TIMER;
      v =  (1 - Mathf.Abs(v - TIMER/2.0f)/(TIMER/2.0f)) *0.1f +1;
      this.transform.localScale = Vector3.one * v;
    }   
  }
}
