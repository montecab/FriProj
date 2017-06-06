using UnityEngine;
using System.Collections;

using PI;

public class AccelerationToColorController : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	
	private float accelerometerValueToBrightness(float val) {
		return Mathf.Clamp(val, 0.0f, 1.0f);
	}
	
	// Update is called once per frame
	void Update () {
	
		uint[] rgbComponents = new uint[] {
			(uint)ComponentID.WW_COMMAND_RGB_RIGHT_EAR,
			(uint)ComponentID.WW_COMMAND_RGB_LEFT_EAR,
			(uint)ComponentID.WW_COMMAND_RGB_EYE,
			(uint)ComponentID.WW_COMMAND_RGB_CHEST,
		};
		
		foreach (piBotBo bot in piConnectionManager.Instance.BotsInState(PI.BotConnectionState.CONNECTED)) {
			// turn off eye ring
			bot.cmd_eyeRing(255, "", new bool[] {
				false, false, false, false,
				false, false, false, false,
				false, false, false, false,});
			
			// chest lights indicate XYZ
			float r = accelerometerValueToBrightness(bot.Accelerometer.x);
			float g = accelerometerValueToBrightness(bot.Accelerometer.y);
			float b = accelerometerValueToBrightness(bot.Accelerometer.z);
			bot.cmd_rgbLights(r, g, b, rgbComponents);
			
			// top button indicates Z
			bot.cmd_LEDButtonMain(accelerometerValueToBrightness(bot.Accelerometer.z * -1));
		}
	}
}
