using UnityEngine;
using System.Collections.Generic;
using SimpleJSON;

public class piConnectionManagerExampleUI : MonoBehaviour {

	private float prevTime  = float.NaN;
	private float prevEncDL = float.NaN;
	private float prevEncDR = float.NaN;
	private float prevSentWheelVelL = 0;
	private float prevSentWheelVelR = 0;
	private float wheelVelL = 0;
	private float wheelVelR = 0;
	private float wheelVelFac = 2;
	
	void Start() {
	}
	
	void OnEnable() {
		InvokeRepeating("sampleWheels", 0, 0.1f);
	}
	
	void OnDisable() {
		CancelInvoke("sampleWheels");
	}
	
	void Update() {
		if ((masterBot != null) && (masterBot.connectionState != PI.BotConnectionState.CONNECTED)) {
			masterBot = null;
		}
	}
	
	piBotBo masterBot = null;
	
	bool AllowDebugButtons {
		get {
			bool showDebugButtonsInUnityPlayer = true;
			bool showDebugButtons = showDebugButtonsInUnityPlayer && (Application.platform != RuntimePlatform.IPhonePlayer);
			return showDebugButtons;
		}
	}
	
	void OnGUI() {
		float designedForWidth = 640.0f;
		piConnectionManager picm = piConnectionManager.Instance;
		
		Vector2 margin;
		margin.x = 10;
		margin.y = 10;
	
		Vector2 cursorInitial = margin;
		
		Vector2 cursor = cursorInitial;
		
		Vector2 buttonSize;
		buttonSize.x = (designedForWidth - (margin.x * 2)) / 3;
		buttonSize.y = 40;
		
		Vector2 labelSize;
		labelSize.x = buttonSize.x;
		labelSize.y = 20;
		
		int WN = 1;
		int wn = 0;
		
		float scaleFac = Screen.width / designedForWidth;
		
		GUIUtility.ScaleAroundPivot(Vector2.one * scaleFac, Vector2.zero);
		GUI.skin.label.wordWrap = true;
			
		// Connection and Robot Controls
				
		if (GUI.Button(new Rect(cursor.x, cursor.y, buttonSize.x, buttonSize.y), StatusString)) {
			onButton_TheButton();
		}
		cursor.y += buttonSize.y;
		
		// show a button per discovered bot.
		Dictionary<string, piBotBo> knownBots = picm.KnownBots;
		foreach (string key in knownBots.Keys) {
			piBotBo bot = knownBots[key];
			
			float wf = (bot.connectionState == PI.BotConnectionState.CONNECTED ? 0.75f : 1.0f);
			
			string s = "'" + bot.Name + "'" + " - " + bot.connectionState.ToString();
			Color bgOrig = GUI.backgroundColor;
			if (bot.connectionState == PI.BotConnectionState.CONNECTED) {
				GUI.backgroundColor = Color.green;
			}
			else {
				GUI.backgroundColor = Color.blue;
			}
			if (GUI.Button(new Rect(cursor.x, cursor.y, buttonSize.x * wf, buttonSize.y), s)) {
				onButton_BotButton(bot);
			}
			
			if (bot.connectionState == PI.BotConnectionState.CONNECTED) {
				if (masterBot == bot) {
					GUI.backgroundColor = Color.red;
				}
				if (GUI.Button(new Rect(cursor.x + buttonSize.x * wf, cursor.y, buttonSize.x * (1 - wf), buttonSize.y), "MSTR")) {
					onButton_MasterBotButton(bot);
				}
			}
			
			GUI.backgroundColor = bgOrig;
			
			
			
			cursor.y += buttonSize.y;
			
			if (masterBot == bot) {
				
				if (GUI.Button(new Rect(cursor.x + buttonSize.x / 4.0f * 0, cursor.y, buttonSize.x / 4.0f, buttonSize.y), "1x")) {
					wheelVelFac = 1;
				}
				if (GUI.Button(new Rect(cursor.x + buttonSize.x / 4.0f * 1, cursor.y, buttonSize.x / 4.0f, buttonSize.y), "2x")) {
					wheelVelFac = 2;
				}
				if (GUI.Button(new Rect(cursor.x + buttonSize.x / 4.0f * 2, cursor.y, buttonSize.x / 4.0f, buttonSize.y), "4x")) {
					wheelVelFac = 4;
				}
				if (GUI.Button(new Rect(cursor.x + buttonSize.x / 4.0f * 3, cursor.y, buttonSize.x / 4.0f, buttonSize.y), "8x")) {
					wheelVelFac = 8;
				}
				cursor.y += buttonSize.y;
			}
		}
		
		
		if (AllowDebugButtons) {
			if (GUI.Button(new Rect(cursor.x, cursor.y, buttonSize.x, buttonSize.y), "fake dscvr")) {
				onButton_FakeDiscover();
			}
			
			cursor.y += buttonSize.y;
			
			if (picm.BotsInState(PI.BotConnectionState.CONNECTED).Count > 0) {
				if (GUI.Button(new Rect(cursor.x, cursor.y, buttonSize.x, buttonSize.y), "fake encoders")) {
					onButton_FakeEncoders();
				}
				cursor.y += buttonSize.y;
			}
			if (picm.BotsInState(PI.BotConnectionState.CONNECTED).Count > 0) {
				if (GUI.Button(new Rect(cursor.x, cursor.y, buttonSize.x, buttonSize.y), "fake acc")) {
					onButton_FakeAcceleration();
				}
				cursor.y += buttonSize.y;
			}
		}
		
		
		if (picm.BotsInState(PI.BotConnectionState.CONNECTED).Count > 0) {
			// we're connected, here are some example commands.
			
			// wheels
			WN = 1;
			wn = 0;
			if (GUI.Button(new Rect(cursor.x + buttonSize.x / WN * (wn++), cursor.y, buttonSize.x / WN, buttonSize.y), "stop")) {
				foreach (piBotBo bot in picm.BotsInState(PI.BotConnectionState.CONNECTED)) {
					bot.cmd_move(0.0f, 0.0f);
				}
			}
			cursor.y += buttonSize.y;
			
			// head
			if (GUI.Button(new Rect(cursor.x + buttonSize.x / 2 * 0, cursor.y, buttonSize.x / 2, buttonSize.y), "look UR")) {
				foreach (piBotBo bot in picm.BotsInState(PI.BotConnectionState.CONNECTED)) {
					bot.cmd_headMove(120, 20);
				}
			}
			if (GUI.Button(new Rect(cursor.x + buttonSize.x / 2 * 1, cursor.y, buttonSize.x / 2, buttonSize.y), "look home")) {
				foreach (piBotBo bot in picm.BotsInState(PI.BotConnectionState.CONNECTED)) {
					bot.cmd_headMove(0, 0);
				}
			}
			cursor.y += buttonSize.y;
			
			// ears
			if (GUI.Button(new Rect(cursor.x + buttonSize.x / 2 * 0, cursor.y, buttonSize.x / 2, buttonSize.y), "ears red")) {
				uint[] components = new uint[] {(uint)piRobotConstants.PIComponentId.COMPONENT_RGB_LEFT_EAR, (uint)piRobotConstants.PIComponentId.COMPONENT_RGB_RIGHT_EAR};
				foreach (piBotBo bot in picm.BotsInState(PI.BotConnectionState.CONNECTED)) {
					bot.cmd_rgbLights(255, 0, 0, components);
				}
			}
			if (GUI.Button(new Rect(cursor.x + buttonSize.x / 2 * 1, cursor.y, buttonSize.x / 2, buttonSize.y), "ears blue")) {
				uint[] components = new uint[] {(uint)piRobotConstants.PIComponentId.COMPONENT_RGB_LEFT_EAR, (uint)piRobotConstants.PIComponentId.COMPONENT_RGB_RIGHT_EAR};
				foreach (piBotBo bot in picm.BotsInState(PI.BotConnectionState.CONNECTED)) {
					bot.cmd_rgbLights(0, 0, 255, components);
				}
			}
			cursor.y += buttonSize.y;
			
			// eye
			if (GUI.Button(new Rect(cursor.x + buttonSize.x / 2 * 0, cursor.y, buttonSize.x / 2, buttonSize.y), "eye on")) {
				foreach (piBotBo bot in picm.BotsInState(PI.BotConnectionState.CONNECTED)) {
					bool[] leds = new bool[PI.piBotConstants.eyeRingNumLEDs];
					for (int n = 0; n < leds.Length; ++n) {
						leds[n] = true;
					}
					bot.cmd_eyeRing(255, (ushort)piBotComponentEyeRing.EyeAnimationIndex.EYEANIM_BITMAP, 1, leds);
				}
			}
			if (GUI.Button(new Rect(cursor.x + buttonSize.x / 2 * 1, cursor.y, buttonSize.x / 2, buttonSize.y), "eye off")) {
				foreach (piBotBo bot in picm.BotsInState(PI.BotConnectionState.CONNECTED)) {
					bool[] leds = new bool[PI.piBotConstants.eyeRingNumLEDs];
					for (int n = 0; n < leds.Length; ++n) {
						leds[n] = false;
					}
					bot.cmd_eyeRing(255, (ushort)piBotComponentEyeRing.EyeAnimationIndex.EYEANIM_BITMAP, 1, leds);
				}
			}
			cursor.y += buttonSize.y;
			
			if (GUI.Button(new Rect(cursor.x, cursor.y, buttonSize.x, buttonSize.y), "tare wheel encoders")) {
				foreach (piBotBo bot in picm.BotsInState(PI.BotConnectionState.CONNECTED)) {
					bot.tareWheels();
				}
			}
			cursor.y += buttonSize.y;
			
			// new Column
			cursor.y = cursorInitial.y;
			cursor.x += buttonSize.x;
			
			foreach (piBotBo bot in picm.BotsInState(PI.BotConnectionState.CONNECTED)) {

				int tmpFS = GUI.skin.label.fontSize;
				Color tmp = GUI.color;				
				GUI.color = Color.green;				
				GUI.Label(new Rect(cursor.x, cursor.y, labelSize.x, labelSize.y), bot.Name);
				cursor.y += labelSize.y;
				GUI.skin.label.fontSize = 8;
				GUI.Label(new Rect(cursor.x, cursor.y, labelSize.x, labelSize.y), bot.UUID);
				cursor.y += labelSize.y;
				GUI.color = tmp;
				GUI.skin.label.fontSize = tmpFS;
				
				// encoders
				GUI.Label(new Rect(cursor.x + labelSize.x / 2 * 0, cursor.y, labelSize.x / 2, labelSize.y), "L: " + bot.WheelLeft .encoderDistance.Value.ToString("0.00"));
				GUI.Label(new Rect(cursor.x + labelSize.x / 2 * 1, cursor.y, labelSize.x / 2, labelSize.y), "R: " + bot.WheelRight.encoderDistance.Value.ToString("0.00"));
				cursor.y += labelSize.y;
	
				// accelerometer
				GUI.Label(new Rect(cursor.x + labelSize.x / 3 * 0, cursor.y, labelSize.x / 2, labelSize.y), "X: " + bot.Accelerometer.x.ToString("0.00"));
				GUI.Label(new Rect(cursor.x + labelSize.x / 3 * 1, cursor.y, labelSize.x / 3, labelSize.y), "Y: " + bot.Accelerometer.y.ToString("0.00"));
				GUI.Label(new Rect(cursor.x + labelSize.x / 3 * 2, cursor.y, labelSize.x / 3, labelSize.y), "Z: " + bot.Accelerometer.z.ToString("0.00"));
				cursor.y += labelSize.y;
	
				// buttons
				GUI.Label(new Rect(cursor.x + labelSize.x / 4 * 0, cursor.y, labelSize.x / 4, labelSize.y), "BM: " + (int)bot.ButtonMain.state);
				GUI.Label(new Rect(cursor.x + labelSize.x / 4 * 1, cursor.y, labelSize.x / 4, labelSize.y), "B1: " + (int)bot.Button1   .state);
				GUI.Label(new Rect(cursor.x + labelSize.x / 4 * 2, cursor.y, labelSize.x / 4, labelSize.y), "B2: " + (int)bot.Button2   .state);
				GUI.Label(new Rect(cursor.x + labelSize.x / 4 * 3, cursor.y, labelSize.x / 4, labelSize.y), "B3: " + (int)bot.Button3   .state);
				cursor.y += labelSize.y;
				
				// distance sensors front
				GUI.Label(new Rect(cursor.x + labelSize.x / 2 * 0, cursor.y, labelSize.x / 2, labelSize.y), "FL:" + bot.DistanceSensorFrontLeft .distance.ToString("0"));
				GUI.Label(new Rect(cursor.x + labelSize.x / 2 * 1, cursor.y, labelSize.x / 2, labelSize.y), "±:"  + bot.DistanceSensorFrontLeft .margin  .ToString("0"));
				cursor.y += labelSize.y;
				GUI.Label(new Rect(cursor.x + labelSize.x / 2 * 0, cursor.y, labelSize.x / 2, labelSize.y), "FR:" + bot.DistanceSensorFrontRight.distance.ToString("0"));
				GUI.Label(new Rect(cursor.x + labelSize.x / 2 * 1, cursor.y, labelSize.x / 2, labelSize.y), "±:"  + bot.DistanceSensorFrontRight.margin  .ToString("0"));
				cursor.y += labelSize.y;
				
				// distance sensor rear
				GUI.Label(new Rect(cursor.x + labelSize.x / 2 * 0, cursor.y, labelSize.x / 4, labelSize.y), "T:"  + bot.DistanceSensorTail      .distance.ToString("0"));
				GUI.Label(new Rect(cursor.x + labelSize.x / 2 * 1, cursor.y, labelSize.x / 4, labelSize.y), "±:"  + bot.DistanceSensorTail      .margin  .ToString("0"));
				cursor.y += labelSize.y;
				
				cursor.y += labelSize.y;
			}
				
			
			// new Column
			cursor.y = cursorInitial.y;
			cursor.x += labelSize.x;
		}
	}
	
	private string StatusString {
		get {
			piConnectionManager picm = piConnectionManager.Instance;
			string s = "";
			
			s = "connected: " + picm.BotsInState(PI.BotConnectionState.CONNECTED).Count + " / " + picm.NumKnownBots;
			
			return s;
		}
	}
	
	public void onButton_TheButton() {
		int n = 1;
		foreach (piBotBase bot in piConnectionManager.Instance.KnownBots.Values) {
			Debug.Log(n.ToString("00") + " " + bot.UUID + "  " + bot.Name + "  " + bot.connectionState.ToString());
		}
	}
	
	void onButton_BotButton(piBotBase bot) {
		switch(bot.connectionState) {
			case PI.BotConnectionState.UNKNOWN: {
				break;
			}
			case PI.BotConnectionState.FAILEDTOCONNECT:
			case PI.BotConnectionState.DISCONNECTED:
			case PI.BotConnectionState.DISCOVERED: {
				piConnectionManager.Instance.connectBot(bot);
				break;
			}
			case PI.BotConnectionState.CONNECTED: {
				piConnectionManager.Instance.disconnectBot(bot);
				break;
			}
			case PI.BotConnectionState.CONNECTING: {
				if (AllowDebugButtons) {
					string jsonString = @"
						{
							""method"":""didConnectWithRobot"",
							""robot"":
							{
								""name"":""" + bot.Name + @""",
								""uuId"":""" + bot.UUID + @""",
								""type"" : 1
							}
						}
						";
					piConnectionManager.Instance._injectOnPIRobotManagerDelegate(jsonString);
				}
				break;
			}
			case PI.BotConnectionState.DISCONNECTING: {
				if (AllowDebugButtons) {
					string jsonString = @"
							{
								""method"":""didDisconnectWithRobot"",
								""robot"":
								{
									""name"":""" + bot.Name + @""",
									""uuId"":""" + bot.UUID + @""",
									""type"" : 1
								}
							}
							";
					piConnectionManager.Instance._injectOnPIRobotManagerDelegate(jsonString);
				}
				break;
			}
		}
	}
	
	
	private static int fakeUUIDNum = 1;
	public void onButton_FakeDiscover() {
		string jsonString = @"
			{
				""method"":""didDiscoverRobot"",
				""robot"":
				{
					""name"":""fake"",
					""uuId"":""AAAAAAAA-BBBB-CCCC-DDDD-EEEEEEEEEE " + fakeUUIDNum + @""",
					""type"" : 1
				}
			}
			";
		
		fakeUUIDNum += 1;
		piConnectionManager.Instance._injectOnPIRobotManagerDelegate(jsonString);
	}
	public void onButton_FakeConnect() {
		string jsonString = @"
			{
				""method"":""didConnectWithRobot"",
				""robot"":
				{
					""name"":""fake"",
					""uuId"":""fake_uuid"",
					""type"" : 1
				}
			}
			";
		
		piConnectionManager.Instance._injectOnPIRobotManagerDelegate(jsonString);
	}
	
	static float _ewl = 100;
	static float _ewr = 100;
	public void onButton_FakeEncoders() {
		piBotBase bot = piConnectionManager.Instance.AnyConnectedBo;
		if (masterBot != null) {
			bot = masterBot;
		}
		if (bot == null) {
			Debug.LogError("unexpected: clicked fakeencoder button, but no available bot");
			return;
		}
		_ewl += 2.0f;
		_ewr += 1.0f;
		string jsonString = @"
			{
			  ""method"" : ""didReceiveRobotState"",
			  ""state"" : {
			    ""300"" : {
			      ""encoderDistance"" : " + _ewl + @"
			    },
			    ""301"" : {
			      ""encoderDistance"" : " + _ewr + @"
			    }
			  },
			  ""robot"" : {
			    ""name"" : """ + bot.Name + @""",
			    ""uuId"" : """ + bot.UUID + @""",
			    ""type"" : 1
			  }
			}
		";
		piConnectionManager.Instance._injectOnPIRobotManagerDelegate(jsonString);		
	}
	
	static float _accTheta = 0;
	public void onButton_FakeAcceleration() {
		piBotBase bot = piConnectionManager.Instance.AnyConnectedBo;
		if (bot == null) {
			Debug.LogError("unexpected: clicked fakeencoder button, but no available bot");
			return;
		}
		_accTheta += Mathf.PI * 2.0f / 12.0f;
		string jsonString = @"
			{
			  ""method"" : ""didReceiveRobotState"",
			  ""state"" : {
			    ""400"" : {
			      ""x"" : " + Mathf.Cos(_accTheta) + @",
			      ""y"" : 0,
			      ""z"" : " + Mathf.Sin(_accTheta) + @"
			    }
			  },
			  ""robot"" : {
			    ""name"" : """ + bot.Name + @""",
			    ""uuId"" : """ + bot.UUID + @""",
			    ""type"" : 1
			  }
			}
		";
		piConnectionManager.Instance._injectOnPIRobotManagerDelegate(jsonString);		
	}
	
	
	void onButton_MasterBotButton(piBotBo bot) {
		masterBot = bot;
	}
	
	void sampleWheels() {
		if ((masterBot == null) || (masterBot.connectionState != PI.BotConnectionState.CONNECTED)) {
			return;
		}

		if (float.IsNaN(prevEncDL)) {
			prevEncDL = masterBot.WheelLeft.encoderDistance.Value;
			prevEncDR = masterBot.WheelLeft.encoderDistance.Value;
		}

		if (float.IsNaN(prevTime)) {
			prevTime = Time.time;
			return;
		}
		
		float dt = Time.time - prevTime;
		
		float dWheelL = masterBot.WheelLeft .encoderDistance.Value - prevEncDL;
		float dWheelR = masterBot.WheelRight.encoderDistance.Value - prevEncDR;
		
		wheelVelL = dWheelL / dt;
		wheelVelR = dWheelR / dt;
		
		float dWVL = wheelVelL - prevSentWheelVelL;
		float dWVR = wheelVelR - prevSentWheelVelR;
		
		if (!piMathUtil.withinEpsilon(dWVL) || !piMathUtil.withinEpsilon(dWVR)) {
			Debug.Log("setting wheel vel to " + wheelVelL + ", " + wheelVelR);
			foreach (piBotBo otherBot in piConnectionManager.Instance.BotsInState(PI.BotConnectionState.CONNECTED)) {
				if (otherBot != masterBot) {
					otherBot.cmd_move(wheelVelL * wheelVelFac, wheelVelR * wheelVelFac);
				}
			}
			
			prevSentWheelVelL = wheelVelL;
			prevSentWheelVelR = wheelVelR;
		}
		
		prevEncDL = masterBot.WheelLeft .encoderDistance.Value;
		prevEncDR = masterBot.WheelRight.encoderDistance.Value;
		prevTime  = Time.time;
	}
}


