using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using TMPro;

namespace Turing{
  public class trVideoPlayerPanelController : MonoBehaviour {
    public monoflow.MPMP MediaPlayer;
    public Button CloseButton;
    public Action PanelCloseListner;
    public Button PlayButton;
    public GameObject PauseImages;
    public GameObject PlayImages;
    public AspectRatioFitter ARFitter;

    public TextMeshProUGUI TextCaption;

    private trSubtitleInfo subtitleInfo;
    private IEnumerator playCoroutine = null;
    private IEnumerator subtitleCoroutine = null;
    private string curPath = "";
    private string curContext = ""; //video for what (eg. challenge)

    void Start () {
      CloseButton.onClick.AddListener(onCloseButtonClicked);
      MediaPlayer.looping = false;
      MediaPlayer.autoPlay = false;
      PlayButton.onClick.AddListener(onPlayButtonClicked);

      //this requires Android API 23, but we are on 19, so we cannot use it for Android
      #if !UNITY_ANDROID
      MediaPlayer.OnPlaybackCompleted = OnPlayCompleted;
      #endif
    }

    void OnPlayCompleted(monoflow.MPMP player){
      MediaPlayer.Pause();
      SetActive(false);
    }

    void onPlayButtonClicked(){
      if(MediaPlayer.IsPaused()){
        MediaPlayer.Play();
      }
      else{
        MediaPlayer.Pause();
      }
      updatePlayButtonView();
    }

    void onCloseButtonClicked(){
      if(!String.IsNullOrEmpty(curContext)){
        new trTelemetryEvent(trTelemetryEventType.VIDEO_HINT_SKIP, true)
          .add(trTelemetryParamType.CONTEXT, curContext)
          .add(trTelemetryParamType.FILE_NAME, curPath)
          .emit();
      }
      SetActive(false);
    }

    public void Play(string path, string context){
      curPath = path;
      curContext = context;
      new trTelemetryEvent(trTelemetryEventType.VIDEO_HINT_PLAY, true)
        .add(trTelemetryParamType.CONTEXT, context)
        .add(trTelemetryParamType.FILE_NAME, path)
        .emit();
      Play();
    }

    public void Play(string path){
      curPath = path;
      curContext = "";
      Play();
    }

    public void Play(){
      if (!this.gameObject.activeSelf){
        this.gameObject.SetActive(true);
      }
      MediaPlayer.videoPath = trDataManager.Instance.VideoManager.GetVideoPath(curPath);
      if (playCoroutine != null){
        StopCoroutine(playCoroutine);
      }
      playCoroutine = play();
      TextCaption.text = "";
      StartCoroutine(playCoroutine);
    }

    IEnumerator displaySubtitle(){
      int currentIndex = -1;
      while (MediaPlayer.GetCurrentPosition() < 1f){
        float time = (float)(MediaPlayer.GetCurrentPosition() * MediaPlayer.GetDuration());
        if (currentIndex != -1 && time < subtitleInfo.Captions[currentIndex].StartTime &&
           time >= subtitleInfo.Captions[currentIndex].EndTime){
           yield return null;
        }
        for (int i = 0; i < subtitleInfo.Captions.Count; i++){
          trCaptionInfo info = subtitleInfo.Captions[i];
          if(time>=info.StartTime && time<info.EndTime && currentIndex!=i){
            currentIndex = i;
            TextCaption.text = wwLoca.Format(subtitleInfo.Captions[i].Caption);
          }
        }
        yield return null;
      }
    }

    IEnumerator play ()
    {
      string filePath = System.IO.Path.Combine (Application.streamingAssetsPath, curPath);
      #if UNITY_ANDROID
      WWW www = new WWW (filePath);
      yield return www;
      if (string.IsNullOrEmpty(www.error)){
      #else //UNITY_IOS, UNITY_EDITOR
      System.IO.FileInfo fileInfo = new System.IO.FileInfo(filePath);
      if (fileInfo.Exists){
      #endif
        MediaPlayer.Load();
        while (MediaPlayer.IsLoading()){
          yield return null;
        }

        //adjust aspect ration for video
        Vector2 videoSize = MediaPlayer.GetNativeVideoSize();
        float aspectRatio = videoSize.x / videoSize.y;
        float screenAspectRatio = (float)(Screen.width) / (float)(Screen.height);   
        ARFitter.aspectMode = screenAspectRatio > aspectRatio ? AspectRatioFitter.AspectMode.HeightControlsWidth : AspectRatioFitter.AspectMode.WidthControlsHeight;
        ARFitter.aspectRatio = aspectRatio;

        MediaPlayer.Play();
        MediaPlayer.SetSeeking(true);
        MediaPlayer.SeekTo(0);
        MediaPlayer.SetSeeking(false);
        updatePlayButtonView();
        yield return null;

        #if UNITY_ANDROID  
        //wait for 0.5 second to make sure seekTo is executed
        yield return new WaitForSeconds(0.5f);
        #endif

        //subtitle
        if (trDataManager.Instance.VideoManager.subtitles.ContainsKey(curPath)){
          subtitleInfo = trDataManager.Instance.VideoManager.subtitles[curPath];
          if (subtitleCoroutine != null){
            StopCoroutine(subtitleCoroutine);
          }
          subtitleCoroutine = displaySubtitle();
          StartCoroutine(subtitleCoroutine);
        } else{
          Debug.LogWarning("Can't find subtitle:"+curPath);
        }

        #if UNITY_ANDROID  
        //Android GetCurrentPosition()stop at very random location when ending(0.995 ~ 1.0)
        //This is hacking but I cannot find a better way since we cannot use the OnPlayCompleted callback with our Android level 19
        while(MediaPlayer.GetCurrentPosition() <= 0.995f){
          yield return null;
        }
        OnPlayCompleted(MediaPlayer);
        #endif
      } else {
        WWLog.logError("Can't find video at:"+filePath);
        this.gameObject.SetActive(false);
      }
    }

    void updatePlayButtonView(){
      PlayImages.SetActive(MediaPlayer.IsPaused());
      PauseImages.SetActive(!MediaPlayer.IsPaused());
    }

    public void SetActive(bool active){
      this.gameObject.SetActive(active);
      if(!active && PanelCloseListner != null){
        PanelCloseListner();
      }
    }

    void OnEnable(){
      piConnectionManager.Instance.hideChromeButton();
    }

    void OnDisable(){
      MediaPlayer.Pause();
      if(piConnectionManager.Instance != null){
        piConnectionManager.Instance.showChromeButton();
      }
    }

    #if !UNITY_IOS && !UNITY_ANDROID
    private void OnGUI(){
      if (Turing.trMultivariate.Instance.getOptionValue(Turing.trMultivariate.trAppOption.SHOW_VIDEO_TIMESTAMP)==Turing.trMultivariate.trAppOptionValue.YES){
        float percentage = ((int)Math.Floor(MediaPlayer.GetCurrentPosition()*MediaPlayer.GetDuration()*100))/100f;
        GUI.Label(new Rect(0,0,200,100), "Progress: "+percentage.ToString()+" secs");
      }
    }
    #endif

  }

}
