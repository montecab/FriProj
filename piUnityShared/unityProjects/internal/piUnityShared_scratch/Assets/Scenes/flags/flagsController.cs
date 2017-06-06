using UnityEngine;
using UnityEngine.UI;

public class flagsController : MonoBehaviour {
  public Button buttonPlaySound;
  public Button buttonPlayAnim;
  public Image  imagePlayingSound;
  public Image  imagePlayingAnim;

  void Start() {
    buttonPlaySound.onClick.AddListener(onClickPlaySound);
    buttonPlayAnim.onClick.AddListener(onClickPlayAnim);
  }

  void Update() {
    piBotCommon robot = (piBotCommon)piConnectionManager.Instance.FirstConnectedRobot;
    if (robot == null) {
      imagePlayingSound.color = Color.gray;
      imagePlayingAnim.color  = Color.gray;
    }
    else {
      imagePlayingSound.color = robot.SoundPlayingSensor.flag ? Color.green : Color.red;
      imagePlayingAnim.color  = robot.AnimPlayingSensor .flag ? Color.green : Color.red;
    }
  }

  void onClickPlaySound() {
    piBotCommon robot = (piBotCommon)piConnectionManager.Instance.FirstConnectedRobot;
    if (robot != null) {
      robot.cmd_playSound("TRUMPET_01");
    }
  }

  void onClickPlayAnim() {
    piBotCommon robot = (piBotCommon)piConnectionManager.Instance.FirstConnectedRobot;
    if (robot != null) {
      robot.cmd_startOnRobotPuppetClip(0);
    }
  }
}
