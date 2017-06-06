using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class _LeoFollow : PhotonView {

	public bool isFollowingRight;
	public GameObject target;
	
	// Update is called once per frame
	void FixedUpdate () {
		if (!isMine)
			return;
		
		if (target == null) {
			target = isFollowingRight ? GameObject.FindGameObjectWithTag ("RightController") : GameObject.FindGameObjectWithTag ("LeftController");
			return;
		}

		transform.position = target.transform.position;
		transform.rotation = target.transform.rotation;
	}

	void WriteToDebug(string s) {
		GameObject.FindGameObjectWithTag ("DebugText").GetComponent<Text> ().text = s;
	}
}
