using UnityEngine;
using System.Collections;

public class piRobotConstants{

	//animation json keys
	public const string JSONKEY_ANIMATION					= "animations";
	public const string JSONKEY_ANIMATION_NAME				= "name";
	public const string JSONKEY_ANIMATION_ID				= "animId";
	public const string JSONKEY_ANIMATION_DURATION			= "duration";

	//sound json keys
	public const string JSONKEY_SOUND						= "sounds";
	public const string JSONKEY_SOUND_NAME					= "name";
	public const string JSONKEY_SOUND_FILENAME				= "filename";
	public const string JSONKEY_SOUND_DIRECTORY  			= "directory";
	public const string JSONKEY_SOUND_ID					= "soundId";
	public const string JSONKEY_SOUND_DURATION				= "duration";

	//anim sound combination json keys
	public const string JSONKEY_ANIM_SOUND					= "anim_sound";
	public const string JSONKEY_ANIM_SOUND_NAME				= "name";
	public const string JSONKEY_ANIM_SOUND_ANIMATION_NAME	= "anim_name";
	public const string JSONKEY_ANIM_SOUND_SOUND_NAME		= "sound_name";
	public const string JSONKEY_ANIM_SOUND_SOUND_OFFSET		= "sound_offset";
	public const string JSONKEY_ANIM_SOUND_ANIM_OFFSET		= "anim_offset";

	public static string getRecordedSoundName(uint id){
		if(id> 9){
			WWLog.logError("Invalide recorded sound id : " + id);
			return "";
		}
		return PI.piBotConstants.USERSOUND_FILE_NAME + id.ToString();
			
	}

	public static bool isRecodedSoundIndexValid(int id){
		if(id<0 || id > 9){
			WWLog.logError("Invalide recorded sound id : " + id);
			return false;
		}
			
		return true;
	}
}
