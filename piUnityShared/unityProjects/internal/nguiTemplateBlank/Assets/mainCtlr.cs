using UnityEngine;
using System.Collections;

public class mainCtlr : MonoBehaviour {

	private float wL = 0;
	private float wR = 0;
	private float hP = 0;
	private float hT = 0;
	private bool updateWheels = true;
	private bool updateHead   = true;
	
	public float zeroSnapEpsilon = 0.05f;
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (updateWheels) {
			updateWheels = false;
			foreach(piBotBo bot in piConnectionManager.Instance.BotsInState(PI.BotConnectionState.CONNECTED)) {
				bot.cmd_move(wL, wR);
			}
		}
		
		if (updateHead) {
			updateHead = false;
			foreach(piBotBo bot in piConnectionManager.Instance.BotsInState(PI.BotConnectionState.CONNECTED)) {
				bot.cmd_headMove(hP, hT);
			}
		}
	}
	
	private void zeroSnap(UIScrollBar bar, float snapPoint, float epsilon) {
		if (Mathf.Abs(snapPoint - bar.value) <= epsilon) {
			bar.value = snapPoint;
		}
	}
	
	public void onSlider_WheelLeft(UIScrollBar bar) {
		zeroSnap(bar, 0.5f, zeroSnapEpsilon);
		// the bar value is zero at the top and one at the bottom.
		wL = Mathf.Lerp(-100, 100, 1.0f - bar.value);
		updateWheels = true;
	}
	
	public void onSlider_WheelRight(UIScrollBar bar) {
		zeroSnap(bar, 0.5f, zeroSnapEpsilon);
		// the bar value is zero at the top and one at the bottom.
		wR = Mathf.Lerp(-100, 100, 1.0f - bar.value);
		updateWheels = true;
	}
	
	public void onSlider_HeadPan(UIScrollBar bar) {
		zeroSnap(bar, 0.5f, zeroSnapEpsilon);
		// the bar value is zero at the top and one at the bottom.
		hP = Mathf.Lerp(-120, 120, 1.0f - bar.value);
		updateHead = true;
	}
	
	public void onSlider_HeadTilt(UIScrollBar bar) {
		zeroSnap(bar, 0.5f, zeroSnapEpsilon);
		// the bar value is zero at the top and one at the bottom.
		hT = Mathf.Lerp(-10, 35, 1.0f - bar.value);
		updateHead = true;
	}
}
