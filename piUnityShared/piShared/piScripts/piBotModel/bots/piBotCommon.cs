using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using WW.SimpleJSON;
using PI;

public class piBotCommon : piBotBase {

  public float prevHue = 1.0f;
  public float prevVal = 1.0f;
  
  public piBotCommon(string inUUID, string inName, piRobotType inRobotType, JSONClass jsonRobot=null) : base(inUUID, inName, inRobotType, jsonRobot) {}
	
	// convenience accessors.
  public piBotComponentEyeRing       EyeRing            { get{ return (piBotComponentEyeRing      )(components[ComponentID.WW_COMMAND_EYE_RING           ]); }}
  public piBotComponentLightRGB      RGBEarLeft         { get{ return (piBotComponentLightRGB     )(components[ComponentID.WW_COMMAND_RGB_LEFT_EAR       ]); }}
  public piBotComponentLightRGB      RGBEarRight        { get{ return (piBotComponentLightRGB     )(components[ComponentID.WW_COMMAND_RGB_RIGHT_EAR      ]); }}
  public piBotComponentLED           LEDButtonMain      { get{ return (piBotComponentLED          )(components[ComponentID.WW_COMMAND_LED_BUTTON_MAIN    ]); }}
  public piBotComponentSpeaker       Speaker            { get{ return (piBotComponentSpeaker      )(components[ComponentID.WW_COMMAND_SPEAKER            ]); }}
  public piBotComponentAccelerometer Accelerometer      { get{ return (piBotComponentAccelerometer)(components[ComponentID.WW_SENSOR_MOTION_ACCELEROMETER]); }}
  public piBotComponentButton        ButtonMain         { get{ return (piBotComponentButton       )(components[ComponentID.WW_SENSOR_BUTTON_MAIN         ]); }}
  public piBotComponentButton        Button1            { get{ return (piBotComponentButton       )(components[ComponentID.WW_SENSOR_BUTTON_1            ]); }}
  public piBotComponentButton        Button2            { get{ return (piBotComponentButton       )(components[ComponentID.WW_SENSOR_BUTTON_2            ]); }}
  public piBotComponentButton        Button3            { get{ return (piBotComponentButton       )(components[ComponentID.WW_SENSOR_BUTTON_3            ]); }}
  public piBotComponentMicrophone    SoundSensor        { get{ return (piBotComponentMicrophone   )(components[ComponentID.WW_SENSOR_MICROPHONE          ]); }}  // todo: rename to 'microphone'.
  public piBotComponentFlag          SoundPlayingSensor { get{ return (piBotComponentFlag         )(components[ComponentID.WW_SENSOR_SOUND_PLAYING       ]); }}
  public piBotComponentFlag          AnimPlayingSensor  { get{ return (piBotComponentFlag         )(components[ComponentID.WW_SENSOR_ANIMATION_PLAYING   ]); }}
  public piBotComponentBattery       Battery            { get{ return (piBotComponentBattery      )(components[ComponentID.WW_SENSOR_BATTERY             ]); }}
  public piBotComponentSensorRaw     SensorRaw1         { get{ return (piBotComponentSensorRaw    )(components[ComponentID.WW_SENSOR_RAW1                ]); }}
  public piBotComponentSensorRaw     SensorRaw2         { get{ return (piBotComponentSensorRaw    )(components[ComponentID.WW_SENSOR_RAW2                ]); }}

  private const float INVALID_TIME = -1f;
  private float lastTimeVoiceHeard = INVALID_TIME;
  public float LastVoiceHeardSensorDirection = float.NaN;

  private const float MAX_TIME_BETWEEN_VOICES = 3f;
  public  static int  VOICE_CONFIDENCE_THRESHOLD = 10;

  public bool WasVoiceHeardRecently () {
    if (lastTimeVoiceHeard != INVALID_TIME && (Time.time - lastTimeVoiceHeard) <= MAX_TIME_BETWEEN_VOICES && !float.IsNaN(LastVoiceHeardSensorDirection)) {
      return true;
    }
    return false;
  }  

  public void ResetVoiceSensor() {
    lastTimeVoiceHeard = INVALID_TIME;
    LastVoiceHeardSensorDirection = float.NaN;
  }

  public override void handleState(JSONClass jsComponent) {
    base.handleState(jsComponent);
    if (this.SoundSensor.voiceConfidence > VOICE_CONFIDENCE_THRESHOLD) {
      LastVoiceHeardSensorDirection = this.SoundSensor.direction;
      lastTimeVoiceHeard = Time.time;
    }
    else {
      if(!WasVoiceHeardRecently()) {
        ResetVoiceSensor();
      }
    }
  }
  
	protected override void setupComponents() {
		// effectors
		addComponent<piBotComponentEyeRing      >(PI.ComponentID.WW_COMMAND_EYE_RING         );
		addComponent<piBotComponentLightRGB     >(PI.ComponentID.WW_COMMAND_RGB_LEFT_EAR     );
		addComponent<piBotComponentLightRGB     >(PI.ComponentID.WW_COMMAND_RGB_RIGHT_EAR    );
		addComponent<piBotComponentLED          >(PI.ComponentID.WW_COMMAND_LED_BUTTON_MAIN  );
		addComponent<piBotComponentSpeaker      >(PI.ComponentID.WW_COMMAND_SPEAKER          );
		
		// sensors
		addComponent<piBotComponentAccelerometer>(PI.ComponentID.WW_SENSOR_MOTION_ACCELEROMETER);
		addComponent<piBotComponentButton       >(PI.ComponentID.WW_SENSOR_BUTTON_MAIN         );
    addComponent<piBotComponentButton       >(PI.ComponentID.WW_SENSOR_BUTTON_1            );
    addComponent<piBotComponentButton       >(PI.ComponentID.WW_SENSOR_BUTTON_2            );
    addComponent<piBotComponentButton       >(PI.ComponentID.WW_SENSOR_BUTTON_3            );
    addComponent<piBotComponentMicrophone   >(PI.ComponentID.WW_SENSOR_MICROPHONE     	   );
    addComponent<piBotComponentFlag         >(PI.ComponentID.WW_SENSOR_SOUND_PLAYING       );
    addComponent<piBotComponentFlag         >(PI.ComponentID.WW_SENSOR_ANIMATION_PLAYING   );
    addComponent<piBotComponentBattery      >(PI.ComponentID.WW_SENSOR_BATTERY             );
    addComponent<piBotComponentSensorRaw    >(PI.ComponentID.WW_SENSOR_RAW1                );
    addComponent<piBotComponentSensorRaw    >(PI.ComponentID.WW_SENSOR_RAW2                );
	}

  public override void Reset(){
    cmd_rgbLights(0.1f, 0.1f, 0.1f);
    cmd_LEDButtonMain(0);
    cmd_playSound("");

    cmd_eyeRing(0.1f, piBotComponentEyeRing.EYEANIM_FULL_BLINK, new bool[0]);
  }
  
	// BOT COMMANDS
	public void cmd_rgbLights(double red, double green, double blue, uint[] components) {
		logVerbose("red:" + red + " green:" + green + " blue:" + blue + " comps:" + components.ToString());
		if (apiInterface != null) {
			apiInterface.rgbLights(UUID, red, green, blue, components);
		}
	}
	
	public void cmd_rgbLights(double red, double green, double blue) {
    logVerbose("red:" + red + " green:" + green + " blue:" + blue);
		uint[] components = new uint[] {
			(uint)PI.ComponentID.WW_COMMAND_RGB_LEFT_EAR,
			(uint)PI.ComponentID.WW_COMMAND_RGB_RIGHT_EAR,
			(uint)PI.ComponentID.WW_COMMAND_RGB_CHEST,
			(uint)PI.ComponentID.WW_COMMAND_RGB_EYE
		};
		if (apiInterface != null) {
			apiInterface.rgbLights(UUID, red, green, blue, components);
		}
	}

	// note: animations available on the robot: swirl, full_blink, wink.  See WWContentDefinitions in APIObjectiveC for more details
  public void cmd_eyeRing(double brightness, string animationFile, bool[] bitmap) {
    logVerbose("brightness:" + brightness + " animationFile:" + animationFile + " bitmap:" + bitmap.ToString());
		if (apiInterface != null) {
			apiInterface.eyeRing(UUID, brightness, animationFile, bitmap);
		}
	}
	
	public void cmd_LEDButtonMain(double brightness) {
    logVerbose("brightness:" + brightness);
		if (apiInterface != null) {
			apiInterface.ledButtonMain(UUID, brightness);
		}
	}
	
	public void cmd_playSound(string fileName, string directory, double volume) {
    logVerbose("fileName:" + fileName + " directory:" + directory + " volume:" + volume);
		if (apiInterface != null) {
			apiInterface.playSound(UUID, fileName,directory, volume);
		}
	}

	public void cmd_playSound(string fileName) {
    logVerbose("fileName:" + fileName);
		if (apiInterface != null) {
			apiInterface.playSound(UUID, fileName);
		}
	}

	public void cmd_playRecordedSound(uint id){
		if(id <0 || id>9){
			WWLog.logError("Invalid recorded sound id: " + id);
			return;
		}
		string soundName = PI.piBotConstants.USERSOUND_FILE_NAME + id.ToString();
		
		cmd_playSound(soundName);
	}
	
	public void cmd_performJsonAnimation(string jsonAnimation) {
    logVerbose("");
		if (apiInterface != null) {
			apiInterface.performJsonAnimation(UUID, jsonAnimation);
		}
	}

	public void cmd_stopJsonAnimation(string jsonAnimation) {
    logVerbose("");
		if (apiInterface != null) {
			apiInterface.stopJsonAnimation(UUID, jsonAnimation);
		}
	}
  
  public void cmd_startSingleAnim(string jsonAnimation) {
    logVerbose("");
    if (apiInterface != null) {
      apiInterface.startSingleAnim(UUID, jsonAnimation);
    }
  }

  public void informRobotStoppedAnimation(string animString){
    this.cmd_reset();
  }
	
  public void cmd_stopSingleAnim() {
    logVerbose("");
    if (apiInterface != null) {
      apiInterface.stopSingleAnim(UUID);
    }
  }

  // stop these with cmd_stopOnRobotAnimation().
  // there is no mechanism to determine if one of these is currently playing.
  public void cmd_startOnRobotPuppetClip(uint slotIndex) {
    // HACK.
    // this should either go into APIWrapper and API wrapper should use justDirForSlot() and justNameForSlot() etc,
    // or those functions should be piped through from API to Unity.
    cmd_startOnRobotAnimation("SYST", "PUPPET" + slotIndex.ToString());
  }

  public void cmd_stopOnRobotAnimation() {
    cmd_startOnRobotAnimation("", "");
  }

  public void cmd_startOnRobotAnimation(string dir, string name) {
    logVerbose("dir:" + dir + " name:" + name);
    if (apiInterface != null) {
      apiInterface.startOnRobotAnimation(UUID, dir, name);
    }
  }

  public void cmd_soundTransfer(short[] data, string name) {
    logVerbose("dataLength:" + data.Length + " name:" + name);
		if (apiInterface != null) {
			apiInterface.soundTransfer(UUID, data, name);
		}
	}
	
  // todo - should this be a sensor / component ?
	public float fileTransferProgress {
		get {
			if (apiInterface != null) {
				return apiInterface.fileTransferProgress(UUID);
			}
			else {
				return 1.0f;
			}
		}
	}
  
  public void cmd_setBeacon(int value, int periodMS, PI.WWBeaconLevel level) {
    logVerbose("value:" + value + " periodMS:" + periodMS + " level:" + level.ToString());
    
    byte b0 = 0xB0;
    byte b1 = (byte)((uint)level << 4);
    byte b2 = (byte)value;
    byte b3 = (byte)periodMS;
    
    byte [] cmd = new byte[] {b0, b1, b2, b3};
    
    cmd_sendRawData(cmd);
    
    /*
    - (void)sendBeaconMessage
    {
      //WWCommandSet *cmd = [WWCommandSet new];
      NSString *str = [beaconPeriodText text];
      unsigned short beaconPeriod = [str intValue];
      if([str intValue] > 255)
        beaconPeriod = 255;
      str = [beaconMessageText text];
      
      char temp[] = {0xB0, 0x00, 0xFF, 0x00};
      temp[1] = (temp[1] & ~0xF0) | (beaconLevel << 4);
      
      unsigned char beaconMessage = [str intValue];
      temp[2] = beaconMessage;
      temp[3]  = beaconPeriod;
      //WWCommandBeacon *beacon = [[WWCommandBeacon alloc] initWithBeaconPeriodMs:beaconPeriod];
      //[beacon setBeaconLevel:beaconLevel];
      //[beacon setBeaconMessage:beaconMessage];
      //[cmd setBeacon:beacon];
      //[ConnectedRobotManager sendCommandToRobots:cmd];
      for (WWRobot* robot in ConnectedRobotManager.connectedRobots)
      {
        NSData *rawData = [NSData dataWithBytes:temp  length:4];
        [robot _sendCommandPacket:rawData];
      }
    }
    */
  }

  public void stage_Pamplemousse(bool isOn, int id, float weight, byte[] parameters) {
    ComponentID cid = isOn ? ComponentID.WW_COMMAND_PAMPLEMOUSSE_START : ComponentID.WW_COMMAND_PAMPLEMOUSSE_STOP;

    JSONClass jsc = new JSONClass();
    jsc[piJSONTokens.WW_COMMAND_VALUE_ID].AsInt = id;
    if (isOn) {
      JSONArray jsa = new JSONArray();
      for (int n = 0; n < parameters.Length; ++n) {
        jsa[n].AsInt = parameters[n];
      }
      jsc[piJSONTokens.WW_COMMAND_VALUE_WEIGHT    ].AsFloat = weight;
      jsc[piJSONTokens.WW_COMMAND_VALUE_PARAMETERS]         = jsa;
    }

    stage_Command(cid, jsc);
  }


  public void stage_LEDColorEarLeft(Color c) {
    stage_LEDColors(c, new ComponentID[]{
      ComponentID.WW_COMMAND_RGB_LEFT_EAR,
    });
  }

  public void stage_LEDColorEarRight(Color c) {
    stage_LEDColors(c, new ComponentID[]{
      ComponentID.WW_COMMAND_RGB_RIGHT_EAR,
    });
  }

  public void stage_LEDColorEars(Color c) {
    stage_LEDColors(c, new ComponentID[]{
      ComponentID.WW_COMMAND_RGB_LEFT_EAR,
      ComponentID.WW_COMMAND_RGB_RIGHT_EAR,
    });
  }

  public virtual void stage_LEDColorAll(Color c)
  {}

  // utility function
  public void stage_LEDColors(Color c, ComponentID[] components) {
    JSONClass jsc = new JSONClass();
    jsc[piJSONTokens.WW_COMMAND_VALUE_COLOR_RED  ].AsFloat = c.r;
    jsc[piJSONTokens.WW_COMMAND_VALUE_COLOR_GREEN].AsFloat = c.g;
    jsc[piJSONTokens.WW_COMMAND_VALUE_COLOR_BLUE ].AsFloat = c.b;

    foreach (ComponentID cid in components) {
      stage_Command(cid, jsc);
    }
  }
}
