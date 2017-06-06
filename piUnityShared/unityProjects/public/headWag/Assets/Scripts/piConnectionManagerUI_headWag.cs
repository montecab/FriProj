using UnityEngine;
using System.Collections.Generic;
using SimpleJSON;

/// <summary>
/// 
/// headWag
/// 
/// a super-simple app which wags the robot's head back-and-forth as the robot is manually pushed forward and back.
/// each update, the average encoder distance is obtained from the bot, and if it's different from last time we
/// send a head command then the headPan and headTilt are updated as sin(encoder distance).
/// 
/// </summary>

public class piConnectionManagerUI_headWag : MonoBehaviour {

	private float prevDist = float.NaN;
	
	// wired up in Scene.
	public TextAsset[] robotAnimationsText = new TextAsset[0];
	
	void Start() {
		for (int n = 0; n < robotAnimationsText.Length; ++n) {
			TextAsset ta = robotAnimationsText[n];
			piConnectionManager.Instance.BotInterface.preloadJsonAnimation(ta.text);
		}
	}
	
	void Update() {
		piBotBo bot = piConnectionManager.Instance.AnyConnectedBo;
		if (bot == null) {
			return;
		}
		
		float dist = (bot.WheelLeft.encoderDistance.Value + bot.WheelRight.encoderDistance.Value) / 2;
		
		if (float.IsNaN(prevDist)) {
			prevDist = dist;
		}
		
		if (piMathUtil.withinEpsilon(prevDist, dist)) {
			return;
		}
		
		prevDist = dist;
		
		bot.cmd_headPan (Mathf.Sin(dist * 0.2f) * 90.0f);
		bot.cmd_headTilt(Mathf.Sin(dist * 0.1f) * 20.0f + 10.0f);
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
			string s = "'" + bot.Name + "'" + " - " + bot.connectionState.ToString();
			Color bgOrig = GUI.backgroundColor;
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
		}
		
		
		if (picm.BotsInState(PI.BotConnectionState.CONNECTED).Count > 0) {
			// we're connected, here are some example commands.
			
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
}


