using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class wwEntryExitTracker : MonoBehaviour {

  private const string cTokDuration = "DURATION";
  private const string cTokScene    = "SCENE";
  private const string eventIdPreamble = "SCENE_";
  private const string eventIdEnter    = "ENTER_";
  private const string eventIdExit     = "EXIT_";
  private static System.DateTime timeEntry = System.DateTime.MinValue;
  private static System.DateTime timeExit  = System.DateTime.MinValue;

  public string forcedName = null; // set this in the scene if you want. otherwise the scene name will be used.
  public bool autoEnter = true;
  
	static string SignalEnter {
		get {
			return eventIdPreamble + eventIdEnter + DefaultName;
		}
	}

	static string SignalExit {
		get {
			return eventIdPreamble + eventIdExit + DefaultName;
		}
	}

	public static string DefaultName {
		get {
			return UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
		}
	}

  public static void DoEnter(string name = null, Dictionary<string, string> parameters = null) {
    timeEntry = System.DateTime.Now;
    string signal = (name == null) ? SignalEnter : eventIdPreamble + eventIdEnter + name;
    EmitSignal(signal, parameters);
  }

  public static void DoExit(string name = null, Dictionary<string, string> parameters = null) {
    //Setup Duration
    timeExit = System.DateTime.Now;
    if ((timeExit != System.DateTime.MinValue) && timeEntry != System.DateTime.MinValue) {
      double duration = (timeExit - timeEntry).TotalSeconds;
      parameters = new Dictionary<string, string> ();
      parameters.Add(cTokDuration, duration.ToString("0.0"));
      timeExit = System.DateTime.MinValue;
      timeEntry = System.DateTime.MinValue;
    }
    string signal = (name == null) ? SignalExit : eventIdPreamble + eventIdExit + name;
    EmitSignal(signal, parameters);
  }
    
	public static void EmitSignal(string s, Dictionary<string, string> parameters) {
		if (parameters == null) {
			parameters = new Dictionary<string, string>();
		}
		parameters.Add(cTokScene, DefaultName);

		WWLog.logInfo("emitting event: " + s + " params: " + piStringUtil.dictionaryToString<string, string>(parameters));
		if (FlurryAgent.Instance != null) {
			FlurryAgent.Instance.logEvent(s, parameters);
		}
		else {
			WWLog.logError("Can't use FlurryAgent.");
		}
	}

  // Backward compatibility (original non-static functions that might be used in other unity projects)
  void Start() {
    if (autoEnter) {
      DoEnter(forcedName);
    }
    enabled = false;  // don't bother calling update, etc.
  }

  void OnDestroy() {
    DoExit(forcedName);
  }

  virtual public void emitSignal(string s, Dictionary<string, string> parameters) {
    EmitSignal (s, parameters);
  }

  public void doEnter()
  {
    DoEnter ();
  }

  public void doExit()
  {
    DoExit ();
  }

}
	
