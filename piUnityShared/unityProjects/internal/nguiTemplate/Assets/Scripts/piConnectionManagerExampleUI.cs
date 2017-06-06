using UnityEngine;
using System.Collections.Generic;
using SimpleJSON;

public class piConnectionManagerExampleUI : MonoBehaviour {

	// wired up in Scene.
	public TextAsset[] robotAnimationsText = new TextAsset[0];
	
	int eyePattern           = 0;		
	int brightnessTail       = 0;
	int brightnessButtonMain = 0;

	public bool collapseGUI           = true;	// iff true, collapse & expand the whole GUI
	public bool showExampleCommands   = true;   // iff true, show example robot commands (motors, lights etc)
	public bool showSensors           = true;   // iff true, show the current sensor state for connected bots
	public bool showExampleAnimations = true;	// iff true, show example bot animations
	
	private bool collapsedGUI = false;
	
	
	void Start() {
		for (int n = 0; n < robotAnimationsText.Length; ++n) {
			TextAsset ta = robotAnimationsText[n];
			piConnectionManager.Instance.BotInterface.preloadJsonAnimation(ta.text);
		}
	}
	
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
		GUI.skin.box.alignment = TextAnchor.MiddleCenter;
		
		Dictionary<string, piBotBo> knownBots = picm.KnownBots;
		
		int numConnectedBots = picm.BotsInState(PI.BotConnectionState.CONNECTED).Count;
		
		Color bgOrig = GUI.backgroundColor;
		if (numConnectedBots > 0) {
			GUI.backgroundColor = Color.green;
		}
		else if (knownBots.Count > 0) {
			GUI.backgroundColor = Color.blue;
		}
		
		if (collapseGUI && collapsedGUI) {
			if (GUI.Button(new Rect(cursor.x, cursor.y, buttonSize.x / 4, buttonSize.y), "bots")) {
				collapsedGUI = false;
			}
			GUI.backgroundColor = bgOrig;
			return;
		}
		
		if (knownBots.Count > 0) {
			if (collapseGUI) {
				if (GUI.Button(new Rect(cursor.x, cursor.y, buttonSize.x / 4, buttonSize.y), "hide")) {
					collapsedGUI = true;
					return;
				}
				cursor.y += buttonSize.y;
			}
			GUI.backgroundColor = bgOrig;
		}
		else {
			if (GUI.Button(new Rect(cursor.x, cursor.y, buttonSize.x, buttonSize.y), "calling all robots..")) {
				collapsedGUI = true;
				return;
			}
			cursor.y += buttonSize.y;
		}
		
		GUI.backgroundColor = bgOrig;
		// show a button per discovered bot.
		foreach (string key in knownBots.Keys) {
			piBotBo bot = knownBots[key];
			string s = "'" + bot.Name + "'" + " - " + bot.connectionState.ToString();
			if (bot.connectionState == PI.BotConnectionState.CONNECTED) {
				GUI.backgroundColor = Color.green;
			}
			else {
				GUI.backgroundColor = Color.blue;
			}
			if (GUI.Button(new Rect(cursor.x, cursor.y, buttonSize.x, buttonSize.y), s)) {
				onButton_BotButton(bot);
			}
			GUI.backgroundColor = bgOrig;
			
			cursor.y += buttonSize.y;
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
		
		
		if (picm.BotsInState(PI.BotConnectionState.CONNECTED).Count > 0 && showExampleCommands) {
			// we're connected, here are some example commands.
			
			// wheels
			WN = 3;
			wn = 0;
			if (GUI.Button(new Rect(cursor.x + buttonSize.x / WN * (wn++), cursor.y, buttonSize.x / WN, buttonSize.y), "frwd")) {
				foreach (piBotBo bot in picm.BotsInState(PI.BotConnectionState.CONNECTED)) {
					bot.cmd_move(40.0f, 40.0f);
				}
			}
			if (GUI.Button(new Rect(cursor.x + buttonSize.x / WN * (wn++), cursor.y, buttonSize.x / WN, buttonSize.y), "stop")) {
				foreach (piBotBo bot in picm.BotsInState(PI.BotConnectionState.CONNECTED)) {
					bot.cmd_move(0.0f, 0.0f);
				}
			}
			if (GUI.Button(new Rect(cursor.x + buttonSize.x / WN * (wn++), cursor.y, buttonSize.x / WN, buttonSize.y), "back")) {
				foreach (piBotBo bot in picm.BotsInState(PI.BotConnectionState.CONNECTED)) {
					bot.cmd_move(-40.0f, -40.0f);
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
			if (GUI.Button(new Rect(cursor.x + buttonSize.x / 2 * 0, cursor.y, buttonSize.x / 2, buttonSize.y), "ears")) {
				uint[] components = new uint[] {(uint)PI.ComponentID.COMPONENT_RGB_LEFT_EAR, (uint)PI.ComponentID.COMPONENT_RGB_RIGHT_EAR};
				byte r = (byte)Random.Range(0, 255);
				byte g = (byte)Random.Range(0, 255);
				byte b = (byte)Random.Range(0, 255);
				foreach (piBotBo bot in picm.BotsInState(PI.BotConnectionState.CONNECTED)) {
					bot.cmd_rgbLights(r, g, b, components);
				}
			}
			// eye
			if (GUI.Button(new Rect(cursor.x + buttonSize.x / 2 * 1, cursor.y, buttonSize.x / 2, buttonSize.y), "eye")) {
				bool[] leds = new bool[PI.piBotConstants.eyeRingNumLEDs];
				int tmp = eyePattern;
				for (int n = leds.Length - 1; n >= 0 ; --n) {
					leds[n] = (tmp % 2 == 0 ? false : true);
					tmp = tmp >> 1;
				}
				foreach (piBotBo bot in picm.BotsInState(PI.BotConnectionState.CONNECTED)) {
					bot.cmd_eyeRing(255, (ushort)piBotComponentEyeRing.EyeAnimationIndex.EYEANIM_BITMAP, 1, leds);
				}
				eyePattern = eyePattern + 1;
			}
			cursor.y += buttonSize.y;
			
			// leds
			int dBrightness = 64;
			if (GUI.Button(new Rect(cursor.x + buttonSize.x / 2 * 0, cursor.y, buttonSize.x / 2, buttonSize.y), "LED tail")) {
				brightnessTail += dBrightness;
				if (brightnessTail == 256) {
					brightnessTail = 255;
				}
				if (brightnessTail > 256) {
					brightnessTail = 0;
				}
				foreach (piBotBo bot in picm.BotsInState(PI.BotConnectionState.CONNECTED)) {
					bot.cmd_LEDTail((byte)brightnessTail);
				}
			}
			if (GUI.Button(new Rect(cursor.x + buttonSize.x / 2 * 1, cursor.y, buttonSize.x / 2, buttonSize.y), "LED button")) {
				brightnessButtonMain += dBrightness;
				if (brightnessButtonMain == 256) {
					brightnessButtonMain = 255;
				}
				if (brightnessButtonMain > 256) {
					brightnessButtonMain = 0;
				}
				foreach (piBotBo bot in picm.BotsInState(PI.BotConnectionState.CONNECTED)) {
					bot.cmd_LEDButtonMain((byte)brightnessButtonMain);
				}
			}
			cursor.y += buttonSize.y;
			
			// sounds
			WN = 3;
			wn = 0;
			if (GUI.Button(new Rect(cursor.x + buttonSize.x / WN * (wn++), cursor.y, buttonSize.x / WN, buttonSize.y), "snd 1")) {
				foreach (piBotBo bot in picm.BotsInState(PI.BotConnectionState.CONNECTED)) {
					bot.cmd_playSound(1, 255, 1);
				}
			}
			if (GUI.Button(new Rect(cursor.x + buttonSize.x / WN * (wn++), cursor.y, buttonSize.x / WN, buttonSize.y), "snd 4")) {
				foreach (piBotBo bot in picm.BotsInState(PI.BotConnectionState.CONNECTED)) {
					bot.cmd_playSound(4, 255, 1);
				}
			}
			if (GUI.Button(new Rect(cursor.x + buttonSize.x / WN * (wn++), cursor.y, buttonSize.x / WN, buttonSize.y), "snd 5")) {
				foreach (piBotBo bot in picm.BotsInState(PI.BotConnectionState.CONNECTED)) {
					bot.cmd_playSound(5, 255, 1);
				}
			}
			cursor.y += buttonSize.y;

			// tare
			if (GUI.Button(new Rect(cursor.x, cursor.y, buttonSize.x, buttonSize.y), "tare wheel encoders")) {
				foreach (piBotBo bot in picm.BotsInState(PI.BotConnectionState.CONNECTED)) {
					bot.tareWheels();
				}
			}
			cursor.y += buttonSize.y;
		}
			
		// new Column
		cursor.y = cursorInitial.y;
		cursor.x += buttonSize.x;
		
		if (showSensors) {
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
		}
			
		
		// new Column
		cursor.y = cursorInitial.y;
		cursor.x += labelSize.x;
		
		if (showExampleAnimations && numConnectedBots > 0) {
			// Robot Animations
			int numColumns = 1;
			for (int n = 0; n < robotAnimationsText.Length; ++n) {
				TextAsset ta = robotAnimationsText[n];
				if (GUI.Button(new Rect(cursor.x + buttonSize.x / numColumns * (n % numColumns), cursor.y, buttonSize.x / numColumns, buttonSize.y), ta.name)) {
					Debug.Log("Playing Animation: " + ta.name);
					foreach (piBotBo bot in picm.BotsInState(PI.BotConnectionState.CONNECTED)) {
						bot.cmd_performJsonAnimation(ta.text);
					}
				}
				if (n % numColumns == numColumns - 1) {
					cursor.y += buttonSize.y;
				}
			}
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
}


