using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Turing{
  public class trRCAnimationButtonController : MonoBehaviour {
    public Button AnimButton;
    public uint AnimId;

    // Use this for initialization
    void Start () {
      AnimButton.onClick.AddListener(handleAnimation);
    }
    
    // Update is called once per frame
    void handleAnimation () {
      piBotCommon robot = (piBotCommon) trCurRobotController.Instance.CurRobot;
      if(robot == null){
        trCurRobotController.Instance.CheckOpenChrome();
        return;
      }

      trMoodyAnimation animation = trMoodyAnimations.Instance.getAnimation(AnimId);
      if (animation != null) {
        List<trMoodType> availableMoods = animation.AvailableMoods;
        trMoodType moodType = (availableMoods.Count>0)?availableMoods[0]:trMood.DefaultMood;
        string animName = animation.filename;      
        string animJson = trMoodyAnimations.Instance.getJsonForAnim(animName, moodType);
        robot.cmd_startSingleAnim(animJson);
      }
    }
  }
}


