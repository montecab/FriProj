using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class VideoPlayer : MonoBehaviour {

  public Button PlayButton;
  public Button PlayFullButton;
  public Button PlayMinimalBUtton;

  public Button AnimButton;

	// Use this for initialization
	void Start () {
    PlayButton.onClick.AddListener(()=>onPlayButtonClicked());
    PlayFullButton.onClick.AddListener(()=>onPlayFullButtonClicked());
    PlayMinimalBUtton.onClick.AddListener(()=>onPlayMinimalButtonClicked());
    AnimButton.onClick.AddListener(()=>onPlayAnimButtonClicked());
    piConnectionManager.Instance.showChromeButton();

    Application.runInBackground = true;
	}
	

  void FixedUpdate(){
   int time = (int)Time.fixedTime;
    if(time%2 == 0){
//      WWLog.logInfo("unity running");
      foreach(piBotBo robot in piConnectionManager.Instance.BotsInState(PI.BotConnectionState.CONNECTED)){
        float angle = time%4 == 0? 120 :-120;
        robot.cmd_headPan(angle);
      }
    }
  }

  void onPlayAnimButtonClicked(){
    foreach(piBotBo robot in piConnectionManager.Instance.BotsInState(PI.BotConnectionState.CONNECTED)){
      piRobotAnimSoundManager.Instance.playRobotAnimation(robot, "braggart");
    }
  }

  void onPlayButtonClicked(){
    #if UNITY_IPHONE|| UNITY_ANDROID
    piConnectionManager.Instance.hideChromeButton();
    Handheld.PlayFullScreenMovie("Story.m4v", Color.black, FullScreenMovieControlMode.CancelOnInput);
    #endif
  }

  void onPlayFullButtonClicked(){
    #if UNITY_IPHONE|| UNITY_ANDROID
    Handheld.PlayFullScreenMovie("Story.m4v", Color.black, FullScreenMovieControlMode.Full);
    #endif
  }

  void onPlayMinimalButtonClicked(){
    #if UNITY_IPHONE|| UNITY_ANDROID
    Handheld.PlayFullScreenMovie("Story.m4v", Color.black, FullScreenMovieControlMode.Minimal);
    #endif
  }
}
