using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class SoundManager : MonoBehaviour {
		
	[System.Serializable]
	public class SoundProperty
	{
		public string name;
		public AudioClip audioClip;
		public float volume;
		public bool loop;
	}
	
	public GameObject soundParentObj;
	public SoundProperty[] soundList;
	private Dictionary<string, SoundProperty> _soundTable = new Dictionary<string, SoundProperty>();

	
	private static SoundManager singleton;
    public static SoundManager soundManager
    {
		get
        {
			if(singleton==null)
			{
				singleton = new SoundManager();
			}
			return singleton;
		}
    }
	
	// Use this for initialization
	void Awake () {		
        if(singleton != null)
        {
            Destroy(this.gameObject);
            return;
        }
        
        singleton = this;
	}


	void Start(){
		for (int i = 0; i< soundList.Length; i++) {
			_soundTable.Add(soundList[i].name, soundList[i]);
		}
	}
	
	
	public void PlaySound(string soundName)
	{
		StopAllSound ();

		if (isSoundExisted (soundName) == false) {
			Debug.LogWarning("Unity sound called " + soundName + "doesn't exist in the sound list");
			return;
		}

		SoundProperty soundProperty = _soundTable [soundName];


		GameObject soundObj = new GameObject(soundProperty.name);
		AudioSource soundSource = soundObj.AddComponent("AudioSource") as AudioSource;
		
		soundObj.transform.parent = soundParentObj.transform;            
		soundObj.transform.position = soundParentObj.transform.position;
		soundSource.clip = soundProperty.audioClip;
		
		soundSource.volume = soundProperty.volume;
		soundSource.loop = soundProperty.loop;
		

		soundSource.Play();
		if(!soundProperty.loop)
		{
			Destroy(soundObj, soundProperty.audioClip.length);  
		}
			
	}

	public void StopAllSound(){
		foreach (Transform child in soundParentObj.transform) {
			Destroy(child.gameObject);
		}
		return;
	}

	public bool isSoundExisted(string name){
		return _soundTable.ContainsKey (name);
	}
	
	public bool isSoundPlaying(string soundName){
		foreach (Transform child in soundParentObj.transform) {
			if(child.gameObject.name == soundName){
				return true;
			}
		}
		return false;
	}
	
	public void StopSound(string soundName)
	{
		if (soundParentObj.transform.FindChild (soundName)) {
				GameObject soundObj = soundParentObj.transform.FindChild (soundName).gameObject;
				soundObj.audio.Stop ();
				GameObject.Destroy (soundObj);
		}
	}
	
	public void DestorySoundObj(string soundName)
	{
		GameObject soundObj = soundParentObj.transform.FindChild(soundName).gameObject;
		GameObject.Destroy(soundObj);
	}
	
	public void SlowDownSound(float pitch)
	{
		foreach(Transform objTransform in transform)
		{
			objTransform.gameObject.audio.pitch = pitch;
		}
	}
	
	public void FadeOutSound(string soundName)
	{
		StartCoroutine(FadeOutSoundCor(soundName));
	}
	
	IEnumerator FadeOutSoundCor(string soundName)
	{
		float count = 0.0f;

		Transform objTransform = soundParentObj.transform.FindChild(soundName);
		float startVolume = objTransform.audio.volume;
		while(count<3.0f)
		{
			count+=Time.deltaTime;
			objTransform.gameObject.audio.volume = Mathf.Lerp(startVolume,0.0f,count/3.0f);
			yield return null;
		}
		Destroy(objTransform.gameObject);
		
		yield return null;
		
	} 
}
