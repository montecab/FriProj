using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

using PI;

public abstract class piBotComponentBase {

	public PI.ComponentID componentId = PI.ComponentID.WW_UNKNOWN;

	public abstract void tick(float dt);
	
	// when acting as a simulated robot
	public abstract void handleCommand(WW.SimpleJSON.JSONClass jsComponent);
	
	// when acting as a proxy real robot
	public abstract void handleState(WW.SimpleJSON.JSONClass jsComponent);

	// returns null if this component does not have a sensor aspect, or if it's not yet implemented.
  public abstract WW.SimpleJSON.JSONClass SensorState { get; }
}


