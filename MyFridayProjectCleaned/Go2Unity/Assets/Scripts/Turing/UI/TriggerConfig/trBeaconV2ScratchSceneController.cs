using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Turing;
using PI;

public class trBeaconV2ScratchSceneController : MonoBehaviour {

  public trBeaconV2ConfigController configController;
  public Text output;
  
  private trTrigger    trig;
  private trTransition tran;
  
  // Use this for initialization
  void Start () {
  
    trig = new trTrigger   (trTriggerType.BEACON_V2);
    tran = new trTransition();
    tran.Trigger = trig;
    
    trig.BeaconV2Params.otherColor         = WWBeaconColor.WW_ROBOT_COLOR_GREEN;
    trig.BeaconV2Params.otherDistanceLevel = WWBeaconLevel.BEACON_LEVEL_MEDIUM;
    trig.BeaconV2Params.otherType          = piRobotType.DOT;
    trig.BeaconV2Params.otherID            = 10;
    trig.BeaconV2Params.selfReceivers      = WWBeaconReceiver.WW_BEACON_RECEIVER_LEFT | WWBeaconReceiver.WW_BEACON_RECEIVER_RIGHT;
    
    configController.SetUp(tran, trig);
    configController.gameObject.SetActive(true);
  }
  
  // Update is called once per frame
  void Update () {
    output.text = configController.Parameters.ToJson().ToString();
  }
}
