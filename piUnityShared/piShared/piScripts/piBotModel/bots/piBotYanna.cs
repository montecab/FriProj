using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using WW.SimpleJSON;

using PI;

public class piBotYanna : piBotCommon {

  public piBotYanna(string inUUID, string inName) : base(inUUID, inName, piRobotType.DOT) {}

	public piBotComponentLightRGB   RGBEye   { get{ return (piBotComponentLightRGB  )(components[ComponentID.WW_COMMAND_RGB_EYE        ]); }}

	protected override void setupComponents() {
		base.setupComponents();
		
		// effectors
		addComponent<piBotComponentLightRGB  >(PI.ComponentID.WW_COMMAND_RGB_EYE          );
	}

  public override void stage_LEDColorAll(Color c) {
    stage_LEDColors(c, new ComponentID[]{
      ComponentID.WW_COMMAND_RGB_LEFT_EAR,
      ComponentID.WW_COMMAND_RGB_RIGHT_EAR,
      ComponentID.WW_COMMAND_RGB_EYE,
    });
  }

  public void stage_LEDColorCenter(Color c) {
    stage_LEDColors(c, new ComponentID[]{
      ComponentID.WW_COMMAND_RGB_EYE,
    });
  }

}
