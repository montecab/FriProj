using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using PI;

public abstract class piBotBase {

	private string uuId;
	private string name;
	public PI.BotConnectionState connectionState;
	
	public PIBInterface.Actions apiInterface;

	public Dictionary<PI.ComponentID, piBotComponentBase> components = new Dictionary<ComponentID, piBotComponentBase>();
	
	public piBotBase(string inUUID, string inName) {
		uuId = inUUID;
		name = inName;
		connectionState = BotConnectionState.UNKNOWN;
		setupComponents();
	}
	
	public string UUID {
		get {
			return uuId;
		}
	}
	
	public string Name {
		get {
			return name;
		}
	}
	
	public virtual void tick(float dt) {
		foreach (piBotComponentBase component in components.Values) {
			component.tick(dt);
		}
	}
	
	protected componentT addComponent<componentT>(PI.ComponentID id) where componentT:piBotComponentBase {
		componentT component = System.Activator.CreateInstance<componentT>();
		component.componentId = id;
		components[id] = component;
		return component;
	}
	
	protected virtual void setupComponents(){}
	
	/*
		gets json object w/ the form:
		"componentID" : {
			"param": "value"
		}
	*/
	private piBotComponentBase validateComponentID(int componentID) {
		if (!Enum.IsDefined(typeof(PI.ComponentID), componentID)) {
			Debug.LogError("unknown component ID: " + componentID);
			componentID = (int)(PI.ComponentID.COMPONENT_UNKNOWN);
		}
		
		PI.ComponentID cId = (PI.ComponentID)componentID;
		
		if (!components.ContainsKey(cId)) {
			Debug.LogError("component not present in robot: " + cId.ToString());
			return null;
		}
		
		return components[cId];
	}
	
	// when acting as a simulated robot
	public void handleCommand(SimpleJSON.JSONClass jsComponent) {
		foreach(string key in jsComponent.Keys) {
			piBotComponentBase component = validateComponentID(int.Parse(key));
			if (component != null) {
				component.handleCommand(jsComponent[key].AsObject);
			}
		}
	}
	
	// when acting as a proxy real robot
	public void handleState(SimpleJSON.JSONClass jsComponent) {
		foreach(string key in jsComponent.Keys) {
			piBotComponentBase component = validateComponentID(int.Parse(key));
			if (component != null) {
				component.handleState(jsComponent[key].AsObject);
			}
		}
	}
	

	// BOT COMMANDS
	public void cmd_connect() {
		if (this.apiInterface != null) {
			apiInterface.connect(UUID);
		}
	}
	
	public void cmd_disconnect() {
		if (this.apiInterface != null) {
			apiInterface.disconnect(UUID);
		}
	}
}









