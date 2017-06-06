/// <summary>
/// Script Name: piRobotAnimSoundManager.cs
/// Author: Leisen Huang
/// Description:
/// This is a singleton class. 
/// This script mantains the robot animations&sound executions. The developer can change the animations 
/// in the JSON file. 
/// 
////****NOTE****/
/// Set-up:
/// This script requires asset in the resource folder.
/// There should be a RobotSoundAnimationInfo JSON file inside Resource/RobotResourcces folder.
/// Please reference Init() to see how the animation&sounds set up and change according 
/// to your need.
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using WW.SimpleJSON;

// todo: convert this to use piBotBase instead of piBotBo.

public class piRobotAnimSoundManager : Singleton<piRobotAnimSoundManager> {
	//RobotAnimation Property
	[System.Serializable]
	public class RobotAnimationProperty{
		public string name;
		public TextAsset file;
		public float duration;
	}
	public List<RobotAnimationProperty> animations = new List<RobotAnimationProperty>();

	//the table for looking for the animation in the animation list with the name of it
	private Dictionary<string, int> _animationTable = new Dictionary<string, int> ();
	private HashSet<string> _preloadAnimationTable = new HashSet<string>();
	
	//this is used for tracking which bot is playing which sound
	//string is the robot's uuid, int is the array index of sound
	private Dictionary<string, int>  _robotAnimationPairTable = new Dictionary<string, int>();

	//Robot Anim&Sound Combination Property
	[System.Serializable]
	public class RobotAnimSoundProperty{
		public string name;
		public string sound_name;
		public string animation_name;
		public float sound_offset;
		public float animation_offset;
	}
	public List<RobotAnimSoundProperty> animSounds = new List<RobotAnimSoundProperty>();
	
	
	//the table for looking for the sound in the sound list with the name of it
	private Dictionary<string, int> _animSoundTable = new Dictionary<string, int> ();

	//RobotSound Property
	[System.Serializable]
	public class RobotSoundProperty{
		public string name;
		public string filename;
		public string directory;
		//public int id; // This is the index number of the sound on the robot
		public float duration;
	}
	public List<RobotSoundProperty> sounds = new List<RobotSoundProperty>();
	
	//the table for looking for the sound in the sound list with the name of it
	private Dictionary<string, int> _soundTable = new Dictionary<string, int> ();
	
	//this is used for tracking which bot is playing which sound
	//string is the robot's uuid, int is the array index of sound
	private Dictionary<string, int>  _robotsoundpairTable = new Dictionary<string, int>();

	private bool soundPlaying = false;

//	//RobotAnimation Property
//	[System.Serializable]
//	public class RobotEventProperty{
//		public string Name;
//		public string TutorialIcon;
//	}
//	public List<RobotEventProperty> Events = new List<RobotEventProperty>();
//
//	//the table for looking for the event in the event list with the name of it
//	private Dictionary<string, int> _eventTable = new Dictionary<string, int> ();

  private void Awake(){
    Init();
  }

	private bool _isInit = false;
	private void Init(){
		if (_isInit) return;
    string animInfoPath = "RobotResources/RobotSoundAnimationInfo";
    TextAsset RobotInfo = Resources.Load (animInfoPath, typeof(TextAsset)) as TextAsset;
    if (RobotInfo == null) {
      WWLog.logWarn("could not load " + animInfoPath);
      _isInit = true;
      this.enabled = false;
      return;
    }
		JSONNode myNewNode = JSON.Parse(RobotInfo.text);
		JSONArray AnimationInfo = myNewNode[piRobotConstants.JSONKEY_ANIMATION].AsArray;

		for (int i = 0; i < AnimationInfo.Count; i++) {
			RobotAnimationProperty newAniamtion = new RobotAnimationProperty ();
			newAniamtion.name = AnimationInfo [i] [piRobotConstants.JSONKEY_ANIMATION_NAME];
			newAniamtion.file = Resources.Load ("RobotResources/Animations/" + AnimationInfo [i] [piRobotConstants.JSONKEY_ANIMATION_ID], typeof(TextAsset)) as TextAsset;
			newAniamtion.duration = float.Parse( AnimationInfo [i] [piRobotConstants.JSONKEY_ANIMATION_DURATION]);
			animations.Add (newAniamtion);
			//Debug.Log(newAniamtion.name);

			_animationTable.Add(newAniamtion.name, i);
		}

//		WWLog.logInfo("preload " + AnimationInfo.Count + " animations..");
//		for (int i = 0; i < animations.Count; i++) {
//			piConnectionManager.Instance.BotInterface.preloadJsonAnimation (animations[i].file.text);
//		}
//
//		WWLog.logInfo(".. done preloading " + AnimationInfo.Count + " animations.");
//		
		JSONArray AnimSoundInfo = myNewNode[piRobotConstants.JSONKEY_ANIM_SOUND].AsArray;
		
		for (int i = 0; i < AnimSoundInfo.Count; i++) {
			RobotAnimSoundProperty newAnimSound = new RobotAnimSoundProperty();
			newAnimSound.name = AnimSoundInfo[i][piRobotConstants.JSONKEY_ANIM_SOUND_NAME];
			newAnimSound.sound_name = AnimSoundInfo[i][piRobotConstants.JSONKEY_ANIM_SOUND_SOUND_NAME];
			newAnimSound.animation_name = AnimSoundInfo[i][piRobotConstants.JSONKEY_ANIM_SOUND_ANIMATION_NAME];
			newAnimSound.sound_offset = float.Parse( AnimSoundInfo[i][piRobotConstants.JSONKEY_ANIM_SOUND_SOUND_OFFSET]);
			newAnimSound.animation_offset = float.Parse(AnimSoundInfo[i][piRobotConstants.JSONKEY_ANIM_SOUND_ANIM_OFFSET]);
			animSounds.Add(newAnimSound);
			_animSoundTable.Add(newAnimSound.name, i);
		}

		JSONArray SoundInfo = myNewNode[piRobotConstants.JSONKEY_SOUND].AsArray;
		
		for (int i = 0; i < SoundInfo.Count; i++) {
			RobotSoundProperty newsound = new RobotSoundProperty();
			newsound.name = SoundInfo[i][piRobotConstants.JSONKEY_SOUND_NAME];
			newsound.filename = SoundInfo[i][piRobotConstants.JSONKEY_SOUND_FILENAME];
			newsound.directory = SoundInfo[i][piRobotConstants.JSONKEY_SOUND_DIRECTORY];
			//newsound.id = int.Parse(SoundInfo[i][piRobotConstants.JSONKEY_SOUND_ID]);
			newsound.duration = float.Parse(SoundInfo[i][piRobotConstants.JSONKEY_SOUND_DURATION]);
			sounds.Add(newsound);
			_soundTable.Add(newsound.name, i);
		}
		_isInit = true;
	}


	//********************************************************
	//				ROBOT ANIMATION
	//********************************************************
	//start playing animation immediately
	public void playRobotAnimation(piBotBo robot, string name){
		if (robot == null) {WWLog.logError("Robot desnt exist."); return;}
		playRobotAnimation (robot, name, 0);
	}
	
	// let one robot play an animation after the offset
	public void playRobotAnimation(piBotBo robot, string name, float offset){

		if (_animationTable.ContainsKey (name) == false) {
			WWLog.logError ("Animation named: " + name + " doesn't exist.");
			return;
		}
		if(robot == null) {WWLog.logError ("Robot doesnt exist.");return;}
		
		if (robot is piBotBoFake) {
			WWLog.logError("Skipping playing animation \"" + name + "\" on fake bot. " + robot.Name);
			return;
		}
		
		stopRobotAnimation (robot);
		
		int index = (int) _animationTable [name];

		if(_preloadAnimationTable.Contains(name)  == false){
			//WWLog.logInfo("start preload");
			piConnectionManager.Instance.BotInterface.preloadJsonAnimation (animations[index].file.text);
			//WWLog.logInfo("end preload");
			_preloadAnimationTable.Add(name);
		}
		
    if(_robotAnimationPairTable.ContainsKey(robot.UUID)){
      _robotAnimationPairTable[robot.UUID] = index;
    }
    else{
      _robotAnimationPairTable.Add (robot.UUID, index);
    }
		
		
		StartCoroutine ( PlayRobotAnimation(robot, animations[index], offset));
	}
	
	IEnumerator PlayRobotAnimation(piBotBo robot, RobotAnimationProperty anim, float offset){
		yield return new WaitForSeconds (offset);
		
		//check if the robot is already playing another aniamtion during the offset
		if (isRobotAnimationPlaying(robot, anim.name)){
			robot.cmd_performJsonAnimation (anim.file.text);
//			yield return new WaitForSeconds (anim.duration);
//			if (isRobotAnimationPlaying(robot, anim.name))
//				stopRobotAnimation(robot);
		}
	}
	
	public void informRobotStoppedAnimation(piBotBo robot, string animString){
		if (_robotAnimationPairTable.ContainsKey (robot.UUID)) {
			robot.cmd_reset();

			_robotAnimationPairTable.Remove(robot.UUID);

		}
	}
	
	public void stopRobotAnimation(piBotBo robot){
		
		if (robot == null) {WWLog.logError("Robot desnt exist."); return;}
		
		if (_robotAnimationPairTable.ContainsKey (robot.UUID)) {
			int index = _robotAnimationPairTable[robot.UUID];
			robot.cmd_stopJsonAnimation(animations[index].file.text);
		}
	}
	
	//if one robot is playing animation
	public bool isRobotAnimationPlaying(piBotBo robot)
	{
		if (robot == null) {WWLog.logError("Robot desnt exist."); return false;}
		if (_robotAnimationPairTable.ContainsKey (robot.UUID)) {
			return true;
		}
		return false;
	}
	
	//if the robot is playing the specific animation
	public bool isRobotAnimationPlaying(piBotBo robot, string anim_name)
	{
		if (robot == null) {WWLog.logError("Robot desnt exist."); return false;}
		if (_robotAnimationPairTable.ContainsKey (robot.UUID)) {
			int index = _robotAnimationPairTable[robot.UUID];
			if(animations[index].name == anim_name)
				return true;
		}
		return false;
	}

	public bool isRobotAnimationExisted(string name){
		if (_animationTable.ContainsKey (name)) {
			return true;
		}
		return false;
	}
  
  public RobotAnimationProperty getAnimationProperty(string name) {
    if (!_animationTable.ContainsKey(name)) {
      WWLog.logError("unknown animation: " + name);
      return null;
    }
    
    int index = _animationTable[name];
    return animations[index];
  }
  
  public float getDurationAnimation(string name) {
    if (string.IsNullOrEmpty(name)) {
      return 0;
    }
    
    RobotAnimationProperty rap = getAnimationProperty(name);
    if (rap == null) {
      WWLog.logError("unknown animation: " + name + ", using duration 0");
      return 0;
    }
    
    return rap.duration;
  }


	//********************************************************
	//				ROBOT ANIMATION&SOUND COMBINATION
	//********************************************************
	// let one robot play a combination of anim&sound
	public void playRobotAnimSound(piBotBo robot, string name){
		if (_animSoundTable.ContainsKey (name) == false) {
			WWLog.logError ("AnimSound named: " + name + " doesn't exist.");
			return;
		}
		if(robot == null) {WWLog.logError ("Robot doesnt exist.");return;}
		
		stopRobotAnimSound (robot);
		
		int index = (int) _animSoundTable [name];
		RobotAnimSoundProperty curAnimSound = animSounds [index];
		
		bool hasSound = (curAnimSound.sound_name     != "");
		bool hasAnim  = (curAnimSound.animation_name != "");

		if (hasAnim) {
			playRobotAnimation(robot, curAnimSound.animation_name, curAnimSound.sound_offset);
		}
		if (hasSound) {
			playRobotSound(robot, curAnimSound.sound_name, curAnimSound.sound_offset);
		}
	}

	public void stopRobotAnimSound(piBotBo robot){
		
		if (robot == null) {WWLog.logError("Robot desnt exist."); return;}
		
		stopRobotSound (robot);
		stopRobotAnimation (robot);
	}
	
	//if one robot is playing sound or animation
	public bool isRobotAnimSoundPlaying(piBotBo robot)
	{
		if (robot == null) {WWLog.logError("Robot desnt exist."); return false;}
		if (isRobotSoundPlaying(robot)
		    ||isRobotAnimationPlaying(robot)) {
			return true;
		}
		return false;
	}
	
	//if the robot is playing the specific animation and sound
	//****NOTE**** This function may not give the expected result while the robot stop play an AnimNSound and 
	//			   play another animation or sound which are the child of the animNSound
	public bool isRobotAnimSoundPlaying(piBotBo robot, string anim_sound_name)
	{
		if (robot == null) {WWLog.logError("Robot desnt exist."); return false;}

		if (_animSoundTable.ContainsKey (anim_sound_name)) {
			int index = _animSoundTable[anim_sound_name];
			if(isRobotSoundPlaying(robot, animSounds[index].sound_name)
			   ||isRobotAnimationPlaying(robot, animSounds[index].animation_name))
				return true;
		}
		return false;
	}


	
	//********************************************************
	//				ROBOT SOUND
	//********************************************************
	// let one robot play a sound after the offset
	public void playRobotSound(piBotBo robot, string name, float offset=0){
		if (_soundTable.ContainsKey (name) == false) {
			WWLog.logError ("Sound named: " + name + " doesn't exist.");
			return;
		}
		if(robot == null) {WWLog.logError("Robot doesnt exist.");return;}
		
		stopRobotSound (robot);
		
		int index = (int) _soundTable [name];
		
    if(_robotsoundpairTable.ContainsKey(robot.UUID)){
      _robotsoundpairTable[robot.UUID] = index;
    }
    else{
      _robotsoundpairTable.Add (robot.UUID, index);
    }		
		
		StartCoroutine (PlayRobotSound (robot, sounds[index], offset));
	}
	
	IEnumerator PlayRobotSound(piBotBo robot, RobotSoundProperty sound, float offset){
		yield return new WaitForSeconds (offset);
		
		//check if the robot is already playing another sound during the offset
		if (isRobotSoundPlaying(robot,sound.name)){
			robot.cmd_playSound (sound.filename);
//			yield return new WaitForSeconds (sound.duration);
//			if (isRobotSoundPlaying(robot,sound.name))
//				stopRobotSound(robot);
		}
	}

	 
	public void stopRobotSound(piBotBo robot){
		//Debug.Log("stop called");
		if (robot == null) {WWLog.logError ("Robot doesnt exist.");return;}
		if (_robotsoundpairTable.ContainsKey (robot.UUID)) {
			_robotsoundpairTable.Remove(robot.UUID);
		}
	}


  void Update(){
    foreach (piBotBo bot in piConnectionManager.Instance.BotsInState(PI.BotConnectionState.CONNECTED)) {
			if(bot.SoundPlayingSensor.flag != soundPlaying){
				if(soundPlaying){
					stopRobotSound(bot);
				}
				soundPlaying = bot.SoundPlayingSensor.flag;
//				if (_robotsoundpairTable.ContainsKey (bot.UUID)) {
//					int index = _robotsoundpairTable[bot.UUID];
//					WWLog.logDebug(sounds[index].name + " playing?" + bot.SoundPlayingSensor.flag);
//       			 }
			}
	  
	 
//      if (bot.SoundPlayingSensor.soundFinished) {
//        stopRobotSound(bot);
//      }
    }
  }
	
	//if one robot is playing sound
	public bool isRobotSoundPlaying(piBotBo robot)
	{
		if (robot == null) {WWLog.logError("Robot desnt exist."); return false;}
		if (_robotsoundpairTable.ContainsKey (robot.UUID)) {
			return true;
		}
		return false;
	}
	
	//if the robot is playing the specific sound
	public bool isRobotSoundPlaying(piBotBo robot, string sound_name)
	{
		if (robot == null) {WWLog.logError("Robot desnt exist."); return false;}
		if (_robotsoundpairTable.ContainsKey (robot.UUID)) {
			int index = _robotsoundpairTable[robot.UUID];
			if(sounds[index].name == sound_name)
				return true;
		}
		return false;
	}

	public bool isRobotSoundExisted(string name){
		if (_soundTable.ContainsKey (name)) {
			return true;
		}
		return false;
	}

}
