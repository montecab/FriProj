using UnityEngine;
using System.Collections;

public class PlaygroundPanel : MonoBehaviour {

	public SuperBasicButton buttonA;
	
	public UISprite avatarBody;
	public UISprite avatarHead;
	
	// Use this for initialization
	void Start () {
	
		buttonA.onClick = onButtonA;
		
		Debug.Log("sssss hello quack ");
		
		
	}
	
	// Update is called once per frame
	void Update () {
		piBotBo bot = piConnectionManager.Instance.AnyConnectedBo;
		
		if (bot != null) {
			Vector3    pos = new Vector3(bot.BodyPoseSensor.x, bot.BodyPoseSensor.y, 0);
			pos *= 10.0f;
			Quaternion rot = Quaternion.Euler(0, 0, bot.BodyPoseSensor.radians * Mathf.Rad2Deg);
			avatarBody.transform.localPosition = pos;
			avatarBody.transform.localRotation = rot;
			
			rot = Quaternion.Euler(0, 0, bot.HeadPanSensor.angle.valTarget * Mathf.Rad2Deg);
			avatarHead.transform.localRotation = rot;
		}
	
	}
	
	void onButtonA() {
		foreach (piBotBo bot in piConnectionManager.Instance.BotsInState(PI.BotConnectionState.CONNECTED)) {
			bot.cmd_reset();
			bot.cmd_poseSetGlobal(0, 0, 0, 0);
		}
	}
}
