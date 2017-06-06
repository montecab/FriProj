using UnityEngine;
using System.Collections;

public class IdleRotationScript : MonoBehaviour {

	public float RotationSpeed = 100;

	bool IsRotating = true;

	public void Play () {
		IsRotating = true;
	}

	public void Stop () {
		IsRotating = false;
	}
	
	// Update is called once per frame
	void Update () {
		if(gameObject.activeSelf && IsRotating) {
			gameObject.transform.Rotate(new Vector3(0,0,-RotationSpeed / 100));
		}
	}
}
