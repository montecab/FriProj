using UnityEngine;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace PIBInterface {

	public class Actions {

		// set the receipient for SendMessageToUnity ---------------------------------
		[DllImport ("__Internal")]
		private extern static void PIInterface_setPIMessageReceiverName([MarshalAs(UnmanagedType.LPStr)] string s);
		
		// robot connection & disconnection ------------------------------------------
		[DllImport ("__Internal")]
		private extern static void PIRobot_startScan();
		[DllImport ("__Internal")]
		private extern static void PIRobot_connect([MarshalAs(UnmanagedType.LPStr)] string robotUUID);
		[DllImport ("__Internal")]
		private extern static void PIRobot_disconnect([MarshalAs(UnmanagedType.LPStr)] string robotUUID);
		
		// commands to robots --------------------------------------------------------
		[DllImport ("__Internal")]
		private extern static void PIRobot_move([MarshalAs(UnmanagedType.LPStr)] string robotUUID,
									double leftWheelVelocity, double rightWheelVelocity);
												
		[DllImport ("__Internal")]
		private extern static void PIRobot_moveWithDuration([MarshalAs(UnmanagedType.LPStr)] string robotUUID,
									double leftWheelVelocity, double rightWheelVelocity, double duration);
												
		[DllImport ("__Internal")]
		private extern static void PIRobot_headTilt([MarshalAs(UnmanagedType.LPStr)] string robotUUID,
									double angle);
												
		[DllImport ("__Internal")]
		private extern static void PIRobot_headPan([MarshalAs(UnmanagedType.LPStr)] string robotUUID,
									double angle);
		
		[DllImport ("__Internal")]
		private extern static void PIRobot_headMove([MarshalAs(UnmanagedType.LPStr)] string robotUUID,
									double panAngle, double tiltAngle);
		
		[DllImport ("__Internal")]
		private extern static void PIRobot_rgb([MarshalAs(UnmanagedType.LPStr)] string robotUUID,
									uint red, uint green, uint blue, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=1)] uint[] components, int componentCount);
		
		[DllImport ("__Internal")]
		private extern static void PIRobot_eyeRing([MarshalAs(UnmanagedType.LPStr)] string robotUUID,
									byte brightness, ushort animationID, ushort loops, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=1)] ushort[]bitmapLEDs, int bitmapLEDCount);
		
		[DllImport ("__Internal")]
		private extern static void PIRobot_playSound([MarshalAs(UnmanagedType.LPStr)] string robotUUID,
		                                             uint soundIndex, uint volume, uint loopCount);
		
		[DllImport ("__Internal")]
		private extern static void PIRobot_led([MarshalAs(UnmanagedType.LPStr)] string robotUUID,
		                                             uint component, uint brightness);
		
		// note: no robot ID
		[DllImport ("__Internal")]
		private extern static uint PIRobot_loadJsonAnimation([MarshalAs(UnmanagedType.LPStr)] string jsonAnimation);
		
		// note: no robot ID
		[DllImport ("__Internal")]
		private extern static void PIRobot_unloadAnimation(uint animId);
		
		[DllImport ("__Internal")]
		private extern static void PIRobot_performAnimation([MarshalAs(UnmanagedType.LPStr)] string robotUUID,
									uint animId);
		                                                     
		private RuntimePlatform platform;
		
		private Dictionary<string, uint> loadedAnimations = new Dictionary<string, uint>();

		public Actions (string piReceiverName) {
			platform = Application.platform;

			if (platform == RuntimePlatform.IPhonePlayer)
				PIInterface_setPIMessageReceiverName(piReceiverName);
		}

		public void startScan() {
			if (platform == RuntimePlatform.IPhonePlayer)
				PIRobot_startScan();
		}

		public void connect(string robotUUID) {
			if (platform == RuntimePlatform.IPhonePlayer)
				PIRobot_connect(robotUUID);
		}
		
		public void disconnect(string robotUUID) {
			if (platform == RuntimePlatform.IPhonePlayer)
				PIRobot_disconnect(robotUUID);
		}
		
		public void move (string robotUUID, double leftWheelVelocity, double rightWheelVelocity) {
			if (platform == RuntimePlatform.IPhonePlayer)
				PIRobot_move(robotUUID, leftWheelVelocity, rightWheelVelocity);
		}

		public void moveWithDuration (string robotUUID, double leftWheelVelocity, double rightWheelVelocity, double duration) {
			if (platform == RuntimePlatform.IPhonePlayer)
				PIRobot_moveWithDuration(robotUUID, leftWheelVelocity, rightWheelVelocity, duration);
		}

		public void headTilt (string robotUUID, double angle) {
			if (platform == RuntimePlatform.IPhonePlayer)
				PIRobot_headTilt (robotUUID, angle);
		}
		
		public void headPan (string robotUUID, double angle) {
			if (platform == RuntimePlatform.IPhonePlayer)
				PIRobot_headPan (robotUUID, angle);
		}
		
		public void headMove (string robotUUID, double panAngle, double tiltAngle) {
			if (platform == RuntimePlatform.IPhonePlayer)
				PIRobot_headMove (robotUUID, panAngle, tiltAngle);
		}

		public void rgbLights(string robotUUID, uint red, uint green, uint blue, uint[] components) {
			if (platform == RuntimePlatform.IPhonePlayer)  {
				PIRobot_rgb(robotUUID, red, green, blue, components, components.Length);
			}
		}
		
		public void eyeRing(string robotUUID, byte brightness, ushort animationID, ushort loops, bool[] bitmap) {
			if (platform == RuntimePlatform.IPhonePlayer)  {
				ushort[] shortArray = new ushort[bitmap.Length];
				for (int n = 0; n < bitmap.Length; ++n) {
					shortArray[n] = (bitmap[n] ? (ushort)1 : (ushort)0);
				}
				PIRobot_eyeRing(robotUUID, brightness, animationID, loops, shortArray, shortArray.Length);
			}
		}
		
		public void ledTail(string robotUUID, byte brightness) {
			if (platform == RuntimePlatform.IPhonePlayer)  {
				PIRobot_led(robotUUID, (uint)PI.ComponentID.COMPONENT_LED_TAIL, brightness);
			}
		}
		
		public void ledButtonMain(string robotUUID, byte brightness) {
			if (platform == RuntimePlatform.IPhonePlayer)  {
				PIRobot_led(robotUUID, (uint)PI.ComponentID.COMPONENT_LED_BUTTON_MAIN, brightness);
			}
		}
		
		public void playSound(string robotUUID, uint soundIndex, uint volume, uint loopCount) {
			if (platform == RuntimePlatform.IPhonePlayer)  {
				PIRobot_playSound(robotUUID, soundIndex, volume, loopCount);
			}
		}
		
		
		// idempotent.
		// note, no robot ID.
		public uint preloadJsonAnimation(string jsonAnimation) {
			if (platform == RuntimePlatform.IPhonePlayer)  {
				if (!loadedAnimations.ContainsKey(jsonAnimation)) {
					uint id = PIRobot_loadJsonAnimation(jsonAnimation);
					if (id == 0) {
						Debug.LogError("could not load animation. length = " + jsonAnimation.Length);
						return 0;
					}
					
					loadedAnimations[jsonAnimation] = id;
				}
				
				return loadedAnimations[jsonAnimation];
			}
			
			return 0;
 		}
 		
 		// note, no robot ID.
 		public void unloadJsonAnimation(string jsonAnimation) {
			if (platform == RuntimePlatform.IPhonePlayer)  {
				if (loadedAnimations.ContainsKey(jsonAnimation)) {
					uint id = PIRobot_loadJsonAnimation(jsonAnimation);
					PIRobot_unloadAnimation(id);
					loadedAnimations.Remove(jsonAnimation);
				}
			}
			
		}

		public void performJsonAnimation(string robotUUID, string jsonAnimation) {
			if (platform == RuntimePlatform.IPhonePlayer)  {
				uint animId = preloadJsonAnimation(jsonAnimation);
				if (animId > 0) {
					PIRobot_performAnimation(robotUUID, animId);
				}
			}
		}
	}
}

