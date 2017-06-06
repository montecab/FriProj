using UnityEngine;
using System.Collections;

/*
	Simple controller which expects to be attached to a UIPanel.
	when beginDismiss() is called, the controller waits for dismissDelay seconds,
	and then fades out the entire UIPanel over fadeTime seconds, and then goes inactive.
	Note, if you have the UIPanel inactive in the Hierarchy by default,
	you should set it active in the Start() of your main game controller.
*/
	

public class SplashController : MonoBehaviour {

	public float dismissDelay = 0.5f;
	public float fadeTime     = 0.5f;
	
	private bool _dismissing;
	private float _dismissStartTime;
	
	void Start () {
		gameObject.SetActive(true);
		_dismissing = false;
	}
	
	void Update () {
		if (!_dismissing) {
			return;
		}
		
		float dt = Time.time - _dismissStartTime;
		if (dt > (fadeTime + dismissDelay)) {
			gameObject.SetActive(false);
			_dismissing = false;
			return;
		}
		
		if (dt > dismissDelay) {
			float f = (dt - dismissDelay) / fadeTime;
			GetComponent<UIPanel>().alpha = (1.0f - f);
		}
	}
	
	public void beginDismiss() {
		_dismissing = true;
		_dismissStartTime = Time.time;
	}
}
