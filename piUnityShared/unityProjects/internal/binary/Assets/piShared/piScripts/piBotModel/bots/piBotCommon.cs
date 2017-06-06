using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using PI;

public class piBotCommon : piBotBase {

	public piBotCommon(string inUUID, string inName) : base(inUUID, inName) {}
	
	// convenience accessors.
	public piBotComponentEyeRing       EyeRing      { get{ return (piBotComponentEyeRing      )(components[ComponentID.COMPONENT_EYE_RING         ]); }}
	public piBotComponentLightRGB      RGBEarLeft   { get{ return (piBotComponentLightRGB     )(components[ComponentID.COMPONENT_RGB_LEFT_EAR     ]); }}
	public piBotComponentLightRGB      RGBEarRight  { get{ return (piBotComponentLightRGB     )(components[ComponentID.COMPONENT_RGB_RIGHT_EAR    ]); }}
	public piBotComponentLED           LEDButtonMain{ get{ return (piBotComponentLED          )(components[ComponentID.COMPONENT_LED_BUTTON_MAIN  ]); }}
	public piBotComponentSpeaker       Speaker      { get{ return (piBotComponentSpeaker      )(components[ComponentID.COMPONENT_SPEAKER          ]); }}
	public piBotComponentAccelerometer Accelerometer{ get{ return (piBotComponentAccelerometer)(components[ComponentID.COMPONENT_ACCELEROMETER    ]); }}
	public piBotComponentButton        ButtonMain   { get{ return (piBotComponentButton       )(components[ComponentID.COMPONENT_BUTTON_MAIN      ]); }}
	public piBotComponentButton        Button1      { get{ return (piBotComponentButton       )(components[ComponentID.COMPONENT_BUTTON_1         ]); }}
	public piBotComponentButton        Button2      { get{ return (piBotComponentButton       )(components[ComponentID.COMPONENT_BUTTON_2         ]); }}
	public piBotComponentButton        Button3      { get{ return (piBotComponentButton       )(components[ComponentID.COMPONENT_BUTTON_3         ]); }}
	
	protected override void setupComponents() {
		// effectors
		addComponent<piBotComponentEyeRing      >(PI.ComponentID.COMPONENT_EYE_RING         );
		addComponent<piBotComponentLightRGB     >(PI.ComponentID.COMPONENT_RGB_LEFT_EAR     );
		addComponent<piBotComponentLightRGB     >(PI.ComponentID.COMPONENT_RGB_RIGHT_EAR    );
		addComponent<piBotComponentLED          >(PI.ComponentID.COMPONENT_LED_BUTTON_MAIN  );
		addComponent<piBotComponentSpeaker      >(PI.ComponentID.COMPONENT_SPEAKER          );
		
		// sensors
		addComponent<piBotComponentAccelerometer>(PI.ComponentID.COMPONENT_ACCELEROMETER    );
		addComponent<piBotComponentButton       >(PI.ComponentID.COMPONENT_BUTTON_MAIN      );
		addComponent<piBotComponentButton       >(PI.ComponentID.COMPONENT_BUTTON_1         );
		addComponent<piBotComponentButton       >(PI.ComponentID.COMPONENT_BUTTON_2         );
		addComponent<piBotComponentButton       >(PI.ComponentID.COMPONENT_BUTTON_3         );
	}
	
	
	
	// BOT COMMANDS
	public void cmd_rgbLights(uint red, uint green, uint blue, uint[] components) {
		if (apiInterface != null) {
			apiInterface.rgbLights(UUID, red, green, blue, components);
		}
	}
	
	public void cmd_eyeRing(byte brightness, ushort animationID, ushort loops, bool[] bitmap) {
		if (apiInterface != null) {
			apiInterface.eyeRing(UUID, brightness, animationID, loops, bitmap);
		}
	}
	
	public void cmd_LEDButtonMain(byte brightness) {
		if (apiInterface != null) {
			apiInterface.ledButtonMain(UUID, brightness);
		}
	}
	
	public void cmd_playSound(uint soundIndex, uint volume, uint loopCount) {
		if (apiInterface != null) {
			apiInterface.playSound(UUID, soundIndex, volume, loopCount);
		}
	}
	
	public void cmd_performJsonAnimation(string jsonAnimation) {
		if (apiInterface != null) {
			apiInterface.performJsonAnimation(UUID, jsonAnimation);
		}
	}
}
