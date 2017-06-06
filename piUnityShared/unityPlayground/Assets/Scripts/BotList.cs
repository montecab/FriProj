using UnityEngine;
using System.Collections;

public class BotList : MonoBehaviour {

	private float _lastUpdateTime = 0;

	// Update is called once per frame
	void Update () {
		float secondsSinceLastUpdate = Time.time - _lastUpdateTime;
		if (secondsSinceLastUpdate < 1.0f) {
			return;
		}
	
		_lastUpdateTime = Time.time;
		
		string body = "";
		foreach (piBotBo bot in piConnectionManager.Instance.BotsInState(PI.BotConnectionState.CONNECTED)) {
			body += "\n" + bot.Name + "\n" + bot.SensorState.ToString();
		}
		
		GetComponent<UILabel>().text = body;
	}
}
