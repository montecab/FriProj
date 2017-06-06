using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using PI;

public class piBotYanna : piBotCommon {

	public piBotYanna(string inUUID, string inName) : base(inUUID, inName) {}

	public piBotComponentLightRGB   RGBEye   { get{ return (piBotComponentLightRGB  )(components[ComponentID.COMPONENT_RGB_EYE        ]); }}

	protected override void setupComponents() {
		base.setupComponents();
		
		// effectors
		addComponent<piBotComponentLightRGB  >(PI.ComponentID.COMPONENT_RGB_EYE          );
	}	
}
