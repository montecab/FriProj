using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace Turing{
  public class trRCSoundButtonController : MonoBehaviour {
    public string soundName;
  	
    void Start(){
      this.GetComponent<Button>().onClick.AddListener(onClicked);
    }

    void onClicked(){
      piBotCommon bot = (piBotBo)trCurRobotController.Instance.CurRobot;
      trCurRobotController.Instance.CheckOpenChrome();
      if(bot != null){
        bot.cmd_playSound(soundName);
      }
    }
  }
}
