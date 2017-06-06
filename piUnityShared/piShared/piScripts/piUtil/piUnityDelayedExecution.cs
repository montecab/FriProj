using UnityEngine;
using System.Collections;

/***
  *
  * ugh, something of a nightmare.
  * to get synthetic shell command responses with delay,
  * it's necessary to do this dance to get the responce on the main unity thread.
 ***/


public class piUnityDelayedExecution : Singleton<piUnityDelayedExecution> {

	public delegate void Callback0();
  public delegate void Callback1<T>(T arg1);
  
	public void delayedExecution0(Callback0 callback, float seconds) {
		StartCoroutine(_delay0(callback, seconds));
	}
	
  public void delayedExecution1<T>(Callback1<T> callback, float seconds, T arg1) {
    StartCoroutine(_delay1<T>(callback, seconds, arg1));
  }
  
  public void delayedExecution(System.Action<string, string> callback, float seconds, string arg1, string arg2) {
		StartCoroutine(_delay(callback, seconds, arg1, arg2));
	}
	
  IEnumerator _delay0(Callback0 callback, float seconds) {
    yield return new WaitForSeconds(seconds);
    callback();
  }
  
  IEnumerator _delay1<T>(Callback1<T> callback, float seconds, T arg1) {
    yield return new WaitForSeconds(seconds);
    callback(arg1);
  }
  
  IEnumerator _delay(System.Action<string, string> callback, float seconds, string arg1, string arg2) {
		yield return new WaitForSeconds(seconds);
		callback(arg1, arg2);
	}
	
}
