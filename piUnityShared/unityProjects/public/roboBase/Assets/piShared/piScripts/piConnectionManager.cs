using UnityEngine;
using System.Collections.Generic;
using SimpleJSON;

/// <summary>
///
/// piConnectionManager interfaces with the PIBInterface class to manage connections with robots.
/// It tracks discovered robot/s (just a single robot as of this writing), and facilitates connecting & disconnecting.
/// It provides no GUI, but see piConnectionManagerExampleUI for an example.
/// This script must be attached to a gameObject, and will force that game object's name.
///
/// TODO:
/// - extend the class to connect to multiple robots.
/// - as part of the previous, robot commands will need to be per-robot.
/// 
/// </summary>

public class piConnectionManager : MonoBehaviour {

	private static string singletonName = "ThePIConnectionManager";
	private static piConnectionManager instance;
	public static piConnectionManager Instance {
		get {
			if (instance == null) {
				instance = FindObjectOfType(typeof(piConnectionManager)) as piConnectionManager;
				if (instance == null) {
					GameObject go = new GameObject(singletonName);
					instance = go.AddComponent<piConnectionManager>();
				}
			}
			return instance;
		}
	}
	
	public PIBInterface.Actions BotInterface {
		get {
			if (botInterface == null) {
				botInterface = new PIBInterface.Actions(singletonName);
			}
			return botInterface;
		}
	}
	
	public static List<piAPIMessageDelegateInterface> listeners = new List<piAPIMessageDelegateInterface>();
	
	private Dictionary<string, piBotBo>knownBots = new Dictionary<string, piBotBo>();

	private PIBInterface.Actions botInterface = null;
	

	public void Awake() {
		this.name = singletonName;
	}
	
	public void Start() {
		startScan();
	}
	
	public Dictionary<string, piBotBo>KnownBots {
		get {
			return knownBots;
		}
	}
	
	public int NumKnownBots {
		get {
			return knownBots.Count;
		}
	}
	
	public List<piBotBo> BotsInState(PI.BotConnectionState state) {
		List<piBotBo> ret = new List<piBotBo>();
		foreach (piBotBo bot in knownBots.Values) {
			if (bot.connectionState == state) {
				ret.Add(bot);
			}
		}
		
		return ret;
	}
	
	// legacy routine
	public piBotBo AnyConnectedBo {
		get {
			foreach (piBotBo bot in knownBots.Values) {
				if (bot.connectionState == PI.BotConnectionState.CONNECTED) {
					return bot;
				}
			}
			return null;
		}
	}
	
	public piBotBo KnownBot(string uuId) {
		if (knownBots.ContainsKey(uuId)) {
			return knownBots[uuId];
		}
		else {
			return null;
		}
	}
	
	private piBotBo findOrCreateBot(JSONClass jsonRobot) {
		string uuId = jsonRobot["uuId"];
		piBotBo bot = KnownBot(uuId);
		if (bot == null) {
			bot = new piBotBo(uuId, jsonRobot["name"]);
			bot.apiInterface = this.BotInterface;
		}
		return bot;
	}
	
	
	void onRobotManager_didDiscoverRobot(JSONClass message) {
		JSONClass jsonRobot = message["robot"].AsObject;
		piBotBo bot = findOrCreateBot(jsonRobot);
		if (bot.connectionState == PI.BotConnectionState.CONNECTED) {
			// we're good.
		}
		else {
			bot.connectionState = PI.BotConnectionState.DISCOVERED;
		}
		knownBots[bot.UUID] = bot;
	}
	
	void onRobotManager_didSelectRobot(JSONClass message) {
		JSONClass jsonRobot = message["robot"].AsObject;
		piBotBo bot = findOrCreateBot(jsonRobot);
		bot.connectionState = PI.BotConnectionState.SELECTED;
	}
	
	void onRobotManager_didConnectWithRobot(JSONClass message) {
		JSONClass jsonRobot = message["robot"].AsObject;
		piBotBo bot = findOrCreateBot(jsonRobot);
		bot.connectionState = PI.BotConnectionState.CONNECTED;
		bot.tareWheels();
	}
	
	void onRobotManager_didFailToConnectWithRobot(JSONClass message) {
		JSONClass jsonRobot = message["robot"].AsObject;
		piBotBo bot = findOrCreateBot(jsonRobot);
		bot.connectionState = PI.BotConnectionState.FAILEDTOCONNECT;
	}
	
	void onRobotManager_didDisconnectWithRobot(JSONClass message) {
		JSONClass jsonRobot = message["robot"].AsObject;
		piBotBo bot = findOrCreateBot(jsonRobot);
		bot.connectionState = PI.BotConnectionState.DISCONNECTED;
	}
	
	void onRobotManager_didReceiveRobotState(JSONClass message) {
		JSONClass state = message["state"].AsObject;
		
		JSONClass jsonRobot = message["robot"].AsObject;
		piBotBo bot = findOrCreateBot(jsonRobot);
		if (bot.connectionState != PI.BotConnectionState.CONNECTED) {
			Debug.LogWarning("unexpected: received message for robot in state:" + bot.connectionState);
		}
		bot.handleState(state);
	}
	
	void onRobotManager_didStopCommandSequence(JSONClass message) {
		// todo: probably need to refactor this stuff out
		JSONClass jsonRobot = message["robot"].AsObject;
		piBotBo bot = findOrCreateBot(jsonRobot);

		int sequenceId = message["sequenceId"].AsInt;
		
		string anim = BotInterface.findAnimForAnimID((uint)sequenceId);

		// todo: add an event-listener system.
		if (bot == null || anim == null) {
			// no-op. this is just to take care of 'unused variable' compiler warns.
		}
	}

	void onRobotManager_didFinishCommandSequence(JSONClass message) {
		// todo: probably need to refactor this stuff out
		JSONClass jsonRobot = message["robot"].AsObject;
		piBotBo bot = findOrCreateBot(jsonRobot);

		int sequenceId = message["sequenceId"].AsInt;

		string anim = BotInterface.findAnimForAnimID((uint)sequenceId);
		
		// todo: add an event-listener system.
		if (bot == null || anim == null) {
			// no-op. this is just to take care of 'unused variable' compiler warns.
		}
	}

	public void _injectOnPIRobotManagerDelegate(string jsonString) {
		onPIRobotManagerDelegate(jsonString);
	}
	
	void onPIRobotManagerDelegate(string jsonString) {
//		Debug.Log("Unity Got Message: " + jsonString);

		JSONClass message = JSON.Parse(jsonString).AsObject;

		switch(message["method"]) {
		default:
			Debug.LogError("Unknown method: " + message["method"]);
			break;
		case "didDiscoverRobot":
			onRobotManager_didDiscoverRobot(message);
			break;
		case "didSelectRobot":
			onRobotManager_didSelectRobot(message);
			break;
		case "didConnectWithRobot":
			onRobotManager_didConnectWithRobot(message);
			break;
		case "didFailToConnectWithRobot":
			onRobotManager_didFailToConnectWithRobot(message);
			break;
		case "didDisconnectWithRobot":
			onRobotManager_didDisconnectWithRobot(message);
			break;
		case "didReceiveRobotState":
			onRobotManager_didReceiveRobotState(message);
			break;
		case "didStopCommandSequence":
			onRobotManager_didStopCommandSequence(message);
			break;
		case "didFinishCommandSequence":
			onRobotManager_didFinishCommandSequence(message);
			break;
		}
		
		foreach (piAPIMessageDelegateInterface listener in listeners) {
			listener.onPIMessage(message);
		}
	}

	public void connectBot(piBotBase bot) {
		bot.connectionState = PI.BotConnectionState.CONNECTING;
		BotInterface.connect(bot.UUID);
	}
	
	public void disconnectBot(piBotBase bot) {
		bot.connectionState = PI.BotConnectionState.DISCONNECTING;
		BotInterface.disconnect(bot.UUID);
	}
	
	public void disconnectAll() {
		foreach (piBotBase bot in knownBots.Values) {
			switch (bot.connectionState) {
				default: {
					// do nothing
					break;
				}
				case PI.BotConnectionState.CONNECTED:
				case PI.BotConnectionState.CONNECTING: {
					disconnectBot(bot);
					break;
				}
			}
		}
	}
	
	public void startScan() {
		BotInterface.startScan();
	}
	
}


