using UnityEngine;
using System.Collections.Generic;

public class mainCtlr : MonoBehaviour {

	public enum expressionModeT : int {
		EYE_ON_OFF  = 0,
		TOP_BUTTON_ON_OFF,
		EARS_ON_OFF,
		EYE_UP_DOWN,
		LIGHTS_BLUE_ORANGE,
		HEAD_UP_DOWN,
		HEAD_LEFT_RIGHT,
		SPIN_LEFT_RIGHT,
		LAST_ENTRY		// must be last.
	}
	
	private bool multiBotExpressionSame = true;

	public  UILabel gui_BinaryValue;
	public  UILabel gui_DecimalValue;
	public  GameObject gui_Welcome;
	public  GameObject gui_ConnectionManager;
	
	private int numBinaryDigits = 1;
	private int binaryValue = 0;
	private expressionModeT expressionMode = expressionModeT.EARS_ON_OFF;
	private int autoIncrement = 0;
	
	private const float tiltMax  =  25;
	private const float tiltMin  = -10;
	private const float panMax   =  90;
	private const float panMin   = -90;
	private const float wheelMin = -80;
	private const float wheelMax =  80;
	
	private uint[] earComponents = new uint[] {
		(uint)PI.ComponentID.COMPONENT_RGB_LEFT_EAR,
		(uint)PI.ComponentID.COMPONENT_RGB_RIGHT_EAR,
	};
	
	private uint[] allRGBComponents = new uint[] {
		(uint)PI.ComponentID.COMPONENT_RGB_LEFT_EAR,
		(uint)PI.ComponentID.COMPONENT_RGB_RIGHT_EAR,
		(uint)PI.ComponentID.COMPONENT_RGB_CHEST,
		(uint)PI.ComponentID.COMPONENT_RGB_EYE,
	};
	
	
	// Use this for initialization
	void Start () {
		expressValue();
		if (gui_Welcome != null) {
			gui_Welcome.SetActive(true);
		}
	}
	
	// Update is called once per frame
	private float prevTime = float.NaN;
	void Update () {
		if (float.IsNaN(prevTime)) {
			prevTime = Time.time;
		}
		
		if (autoIncrement != 0) {
			float tNow = Time.time;
			if ((int)(tNow * autoIncrement) > (int)(prevTime * autoIncrement)) {
				valueIncrement();
				prevTime = tNow;
			}
		}
	}
	
	public void expressValueOnRobot(int binVal, piBotBo bot, int botIndex) {
	
		int robotDigitIndex = botIndex % numBinaryDigits;
		bool robotDigit = ((binVal & (1 << robotDigitIndex)) == 0 ? false : true);
		
		expressionModeT em = ExpressionMode;
		if (!multiBotExpressionSame) {
			em = (expressionModeT)(botIndex % (int)expressionModeT.LAST_ENTRY);
		}
	
		switch (em) {
			case expressionModeT.EYE_ON_OFF: {
				bool[] leds = new bool[16];
				for (int n = 0; n < leds.Length; ++n) {
					leds[n] = robotDigit;
				}
				bot.cmd_eyeRing(255, 0xffff, 0, leds);
				break;
			}
			case expressionModeT.TOP_BUTTON_ON_OFF: {
			bot.cmd_LEDButtonMain(robotDigit ? (byte)0xff : (byte)0x00);
				break;
			}
			case expressionModeT.EARS_ON_OFF: {
				uint r = (uint)(robotDigit ? 0xff : 0x00);
				uint g = (uint)(robotDigit ? 0xff : 0x00);
				uint b = (uint)(robotDigit ? 0xff : 0x00);
				bot.cmd_rgbLights(r, g, b, earComponents);
				break;
			}
			case expressionModeT.EYE_UP_DOWN: {
				bool[] leds = new bool[16];
				for (int n = 0; n < leds.Length / 2; ++n) {
					leds[n] = robotDigit;
				}
				for (int n = leds.Length / 2; n < leds.Length; ++n) {
					leds[n] = !robotDigit;
				}
				bot.cmd_eyeRing(255, 0xffff, 0, leds);
				break;
			}
			case expressionModeT.LIGHTS_BLUE_ORANGE: {
				uint r = (uint)(robotDigit ? 0xff : 0x11);
				uint g = (uint)(robotDigit ? 0x33 : 0xff);
				uint b = (uint)(robotDigit ? 0x00 : 0xdd);
				bot.cmd_rgbLights(r, g, b, allRGBComponents);
				break;
			}
			case expressionModeT.HEAD_UP_DOWN: {
				bot.cmd_headMove(0, (robotDigit ? tiltMax : tiltMin));
				break;
			}
			case expressionModeT.HEAD_LEFT_RIGHT: {
				bot.cmd_headMove((robotDigit ? panMax : panMin), 0);
				break;
			}
			case expressionModeT.SPIN_LEFT_RIGHT: {
				if (robotDigit) {
					bot.cmd_move(wheelMax, wheelMin);
				}
				else {
					bot.cmd_move(wheelMin, wheelMax);
				}
				break;
			}
		}
	}
	
	public void returnToHomeState(piBotBo bot) {
		bot.cmd_move(0, 0);
		bot.cmd_headMove(0, 0);
		bot.cmd_rgbLights(0, 0, 0, allRGBComponents);
		bot.cmd_eyeRing(255, 0xffff, 0, new bool[16]);	
		bot.cmd_LEDButtonMain(0);
	}
	
	public void returnToHomeStates() {
		List<piBotBo> bots = piConnectionManager.Instance.BotsInState(PI.BotConnectionState.CONNECTED);
		for (int n = 0; n < bots.Count; ++n) {
			returnToHomeState(bots[n]);
		}
	}
	
	public void expressValueOnRobots(int binVal) {
		List<piBotBo> bots = piConnectionManager.Instance.BotsInState(PI.BotConnectionState.CONNECTED);
		for (int n = 0; n < bots.Count; ++n) {
			expressValueOnRobot(binVal, bots[n], n);
		}
	}
	
	public void expressValueOnScreen() {
		string s = "";
		int val = Value;
		for (int n = 0; n < numBinaryDigits; ++n) {
			s = (val % 2 == 0 ? "0" : "1") + s;
			val = val >> 1;
		}
		gui_BinaryValue.text = s;
		gui_DecimalValue.text = Mathf.Min (MaxVal, binaryValue).ToString();
	}
	
	public void expressValue() {
		expressValueOnScreen();
		expressValueOnRobots(binaryValue);
	}
	
	public int Value {
		get {
			return binaryValue;
		}
		set {
			binaryValue = value;
			expressValue();
		}
	}
	
	public int MaxVal {
		get {
			return (1 << numBinaryDigits) - 1;
		}
	}
	
	public void valueChange(int delta) {
		int val = Value;
		val += delta;
		if (val < 0) {
			val = MaxVal;
		}
		else if (val > MaxVal) {
			val = 0;
		}
		Value = val;
	}
	
	public void valueIncrement() {
		valueChange (1);
	}
	
	public void valueDecrement() {
		valueChange (-1);
	}
	
	public void valueReset() {
		Value = 0;
	}
	
	public void setNumDigits(int num) {
		numBinaryDigits = num;
		expressValue();
	}
	
	public void setNumDigits1() {
		setNumDigits(1);
	}
	
	public void setNumDigits2() {
		setNumDigits(2);
	}
	
	public void setNumDigits3() {
		setNumDigits(3);
	}
	
	public void setNumDigits4() {
		setNumDigits(4);
	}
	
	public void setNumDigits5() {
		setNumDigits(5);
	}
	
	public void setNumDigits6() {
		setNumDigits(6);
	}
	
	public expressionModeT ExpressionMode {
		get {
			return expressionMode;
		}
		set {
			returnToHomeStates();
			expressionMode = value;
			expressValueOnRobots(binaryValue);
		}
	}
	
	public void changedExpressionMode(UIPopupList widget) {
		expressionModeT em = expressionModeT.EARS_ON_OFF;
		
		switch (widget.value) {
			case "Eye On/Off":
				em = expressionModeT.EYE_ON_OFF;
				break;
			case "Top Button On/Off":
				em = expressionModeT.TOP_BUTTON_ON_OFF;
				break;
			case "Ears On/Off":
				em = expressionModeT.EARS_ON_OFF;
				break;
			case "Eye Up/Down":
				em = expressionModeT.EYE_UP_DOWN;
				break;
			case "Lights Blue/Orange":
				em = expressionModeT.LIGHTS_BLUE_ORANGE;
				break;
			case "Head Up/Down":
				em = expressionModeT.HEAD_UP_DOWN;
				break;
			case "Head Left/Right":
				em = expressionModeT.HEAD_LEFT_RIGHT;
				break;
			case "Spin Left/Right":
				em = expressionModeT.SPIN_LEFT_RIGHT;
				break;
		}
		
		ExpressionMode = em;
	}
	
	public void toggleMultiBotMode(UILabel label) {
		multiBotExpressionSame = !multiBotExpressionSame;
		label.text = "multi: " + (multiBotExpressionSame ? "same" : "different");
		expressValueOnRobots(binaryValue);
	}
	
	public void toggleAutoIncrement(UILabel label) {
		prevTime = Time.time;
		autoIncrement = (autoIncrement + 1) % (4 + 1);
		label.text = "auto: " + autoIncrement;
	}
	
	public void onWelcomeButton(UISprite widget) {
		widget.gameObject.SetActive(false);
		gui_ConnectionManager.SetActive(true);
	}
}




