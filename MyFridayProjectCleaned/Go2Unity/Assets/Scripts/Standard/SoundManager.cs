using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class SoundManager : MonoBehaviour {

  public enum trAppSound {
    LOBBY_SOUND             = 0,
    CONNECT_STATES_END      = 1,
    CONNECT_STATES_START    = 2,
    TRANSITION_STATES       = 3,
    TRASH                   = 4,
    NEW_STATE               = 5,
    CONNECT_STATES_LOOP     = 6,
    TRANSFER_SM             = 7,  
    UI_SOUND                = 8,
    PLAY_WITH_ME_START      = 9,
    RC_SPEED_ACC1           = 10,
    RC_SPEED_ACC2           = 11,
    RC_SPEED_DEC1           = 12,
    RC_SPEED_DEC2           = 13,
    RC_COLOR_SLIDER_LOOP    = 14,
    RC_COLOR_JOYSTICK_LOOP  = 24,
    RC_EYE_DASH             = 15,
    RC_EYE_DOT              = 16,
    RC_HEAD_PANTILT         = 17,
    RC_DASH_MODE_DRIVE      = 18,
    RC_DASH_MODE_LAUNCHER   = 19,
    RC_DOT_8_BALL_ANSWER    = 20,
    RC_WAVEFORM_SINE        = 21,
    RC_WAVEFORM_STEPPED     = 32,
    RC_WAVEFORM_SOLID       = 33,
    RC_BODY_CONTROL_STICK   = 22,
    RC_LAUNCHER_LOAD        = 28,
    RC_LAUNCHER_LAUNCH      = 29,
    RC_TILT_ALERT           = 30,
    USER_SOUNDS_MIC         = 23,
    SM_TRAY_REVEAL          = 25,
    SM_RUN_OFF              = 26,
    SM_RUN_ON               = 27,
    SM_HINT_AVAILABLE       = 28,

    MAP_BOUNCE              = 31,

    TUTORIAL_HAND_SLIDE     = 34,
    TUTORIAL_HAND_BOUNCE    = 35,

    NEXT_AVAILABLE_ID       = 36,

    INVALID                 = 100000000
  }
    
  [System.Serializable]
  public class SoundProperty
  {
    public trAppSound soundType;
    public AudioClip audioClip;
    public float volume;
    public bool loop;
    public float endtime;
  }
  
  public GameObject soundParentObj;
  public SoundProperty[] soundList;
  
  private Dictionary<trAppSound, SoundProperty> _soundTable = new Dictionary<trAppSound, SoundProperty>();
  
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
    for (int i = 0; i< soundList.Length; i++) {
      _soundTable.Add(soundList[i].soundType, soundList[i]);
    }

    if (soundParentObj == null) {
      WWLog.logInfo("soundManager: setting soundParent to self.");
      soundParentObj = gameObject;
    }
    if (piConnectionManager.Instance != null) {
      piConnectionManager.Instance.OnChromeOpen += MuteAudio;
      piConnectionManager.Instance.OnChromeClose += UnmuteAudio;
    }
  }

  void OnDestroy() {
    if (piConnectionManager.Instance != null) {
      piConnectionManager.Instance.OnChromeOpen -= MuteAudio;
      piConnectionManager.Instance.OnChromeClose -= UnmuteAudio;
    }
  }

  // todo: it seems like PlaySound() should do this logic automatically if it's a looping sound.
  public static void StartOnce(trAppSound sound) {
    if (!soundManager.isSoundPlaying(sound)) {
      soundManager.PlaySound(sound);
    }
  }
  
  // todo: it seems like PlaySound() should do this logic automatically if it's a looping sound.
  public static void StopOnce(trAppSound sound) {
    if (soundManager.isSoundPlaying(sound)) {
      soundManager.StopSound(sound);
    }
  }

  public static void MuteAudio(){
    AudioListener.volume = 0;
    AudioListener.pause = true;
  }

  public static void UnmuteAudio(){
    AudioListener.volume = 1;
    AudioListener.pause = false;
  }

  public void PlaySound(trAppSound soundType)
  {
    SoundProperty soundProperty = getSound(soundType);
    if (soundProperty == null) {
      return;
    }

    GameObject soundObj = new GameObject(soundProperty.soundType.ToString());
    AudioSource soundSource = soundObj.AddComponent<AudioSource>() as AudioSource;
    
    soundObj.transform.parent = soundParentObj.transform;            
    soundObj.transform.position = soundParentObj.transform.position;
    soundSource.clip = soundProperty.audioClip;
    
    soundSource.volume = soundProperty.volume;
    soundSource.loop = soundProperty.loop;

    soundSource.Play();
    if(!soundProperty.loop)
    {
      Destroy(soundObj, soundProperty.endtime);  
    }
  }

  public void StopSound(trAppSound soundType) {
    if (getSound(soundType) == null) {
      // error already printed in getSound.
      return;
    }
    if (soundParentObj.transform.Find (soundType.ToString())) {
      GameObject soundObj = soundParentObj.transform.Find (soundType.ToString()).gameObject;
      soundObj.GetComponent<AudioSource>().Stop ();
      GameObject.Destroy (soundObj);
    }
  }

  public void StopAllSound(){
    foreach (Transform child in soundParentObj.transform) {
      Destroy(child.gameObject);
    }
    return;
  }

  private SoundProperty getSound(trAppSound trAS) {
    if (!_soundTable.ContainsKey(trAS)) {
      WWLog.logError("unknown sound - you probably need to add it to the sound manager for this scene: " + trAS.ToString());
      return null;
    }
    else {
      return _soundTable[trAS];
    }
  }

  public bool isSoundPlaying(trAppSound appSound) {

    SoundProperty soundProperty = getSound(appSound);
    if (soundProperty == null) {
      return false;
    }

    foreach (Transform child in soundParentObj.transform) {
      if(child.gameObject.name == soundProperty.soundType.ToString()){
        return true;
      }
    }
    return false;
  }
  
  public void DestorySoundObj(trAppSound soundType)
  {
    GameObject soundObj = soundParentObj.transform.Find(soundType.ToString()).gameObject;
    GameObject.Destroy(soundObj);
  }
  
  public void SlowDownSound(float pitch)
  {
    foreach(Transform objTransform in transform)
    {
      objTransform.gameObject.GetComponent<AudioSource>().pitch = pitch;
    }
  }

  public void setSoundVolume(trAppSound soundType, float volume){
    foreach(Transform objTransform in transform)
    {
      if(objTransform.gameObject.name == soundType.ToString()){
        objTransform.gameObject.GetComponent<AudioSource>().volume = volume;
      }
    }
  }

  public void FadeOutSound(trAppSound soundType)
  {
    StartCoroutine(FadeOutSoundCor(soundType));
  }
  
  IEnumerator FadeOutSoundCor(trAppSound soundType)
  {
    float count = 0.0f;
    float fadeTime = 1.0f;

    Transform objTransform = soundParentObj.transform.Find(soundType.ToString());
    AudioSource audioSource = objTransform.GetComponent<AudioSource>();
    float startVolume = audioSource.volume;
    while(count<fadeTime)
    {
      count+=Time.deltaTime;
      audioSource.volume = Mathf.Lerp(startVolume,0.0f,count/fadeTime);
      yield return null;
    }
    Destroy(objTransform.gameObject);
    yield return null;
  } 

  public void FadeAdjustVolume(trAppSound soundType, float targetVolume)
  {
    StartCoroutine(FadeAdjustVolumeCor(soundType, targetVolume));
  }
  
  IEnumerator FadeAdjustVolumeCor(trAppSound soundType, float targetVolume)
  {
    float count = 0.0f;
    float fadeTime = 1.0f;

    Transform objTransform = soundParentObj.transform.Find(soundType.ToString());
    AudioSource audioSource = objTransform.GetComponent<AudioSource>();
    float startVolume = audioSource.volume;
    while(count<fadeTime)
    {
      count+=Time.deltaTime;
      audioSource.volume = Mathf.Lerp(startVolume, targetVolume, count/fadeTime);
      yield return null;
    }
    yield return null;
  } 
  
}
