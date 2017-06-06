using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

using PI;

public abstract class piBotComponentBase {

	public PI.ComponentID componentId = PI.ComponentID.COMPONENT_UNKNOWN;

	public abstract void tick(float dt);
	
	// when acting as a simulated robot
	public abstract void handleCommand(SimpleJSON.JSONClass jsComponent);
	
	// when acting as a proxy real robot
	public abstract void handleState(SimpleJSON.JSONClass jsComponent);
}

