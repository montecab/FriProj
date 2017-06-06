using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class trMoveDemoController : MonoBehaviour {

  public Button StartButton;
  public Button StopButton;

  public float StoppingAccelerationLinear  = 50;
  public float StoppingAccelerationAngular = 10;
  
  void Awake(){
    StartButton.onClick.AddListener(() => onButtonClick(true));
    StopButton.onClick.AddListener(() => onButtonClick(false));
  }

	// Use this for initialization
	void Start () {
	  StartButton.enabled = true;
    StopButton.enabled = false;
	}

  private void onButtonClick(bool isStartButton){
    StartButton.enabled = !isStartButton;
    StopButton.enabled = isStartButton;

    if (isStartButton){
      foreach (piBotBo robot in getAllConnectedRobots()){
        robot.cmd_bodyMotion(20, 5);
      }
    } else {
      if (StoppingAccelerationLinear < 0){
        foreach (piBotBo robot in getAllConnectedRobots()){
          robot.cmd_bodyMotionStop();
        }
      } else {
        foreach(piBotBo robot in getAllConnectedRobots()){
          robot.cmd_bodyMotionWithAcceleration(0, 0, StoppingAccelerationLinear, StoppingAccelerationAngular);
        }
      }
    }
  }

  private List<piBotBo> getAllConnectedRobots(){
    return piConnectionManager.Instance.BotsInState(PI.BotConnectionState.CONNECTED);
  }
}
