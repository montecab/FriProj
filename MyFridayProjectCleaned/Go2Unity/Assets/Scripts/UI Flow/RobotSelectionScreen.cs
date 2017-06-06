using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class RobotSelectionScreen : MonoBehaviour {

  public GameObject RobotButtonPrefab;
  public GameObject RobotButtonsParent;
  public SegmentedController RobotButtonsController;

  private int robotCnt;
  private List<GameObject> robotButtons = new List<GameObject>();


  void Start(){
    robotCnt = piConnectionManager.Instance.BotsInState(PI.BotConnectionState.CONNECTED).Count;

    if(robotCnt == 0){
      piConnectionManager.Instance.showChromeButton();
      piConnectionManager.Instance.openChrome();
    }
    else{
      refreshRobotList();
    }
  }

  void Update(){

    int currentCnt = piConnectionManager.Instance.BotsInState(PI.BotConnectionState.CONNECTED).Count;

    //prototype code... Will need to change to refresh every time closing Chrome
    if(currentCnt == robotCnt){
      return;
    }
    else{
      refreshRobotList();
    }

  }

  void refreshRobotList(){
    for(int i = 0; i< robotButtons.Count; ++i){
      Destroy(robotButtons[i]);
    }
    robotButtons.Clear();
    robotCnt = piConnectionManager.Instance.BotsInState(PI.BotConnectionState.CONNECTED).Count;
    for(int i = 0 ; i< robotCnt; ++i ){
      GameObject newRobotBtn = Instantiate(RobotButtonPrefab, Vector3.zero, RobotButtonsParent.transform.rotation) as GameObject;
      newRobotBtn.transform.parent = RobotButtonsParent.transform;

      Segment newRbtBtnSegment = newRobotBtn.GetComponent<Segment>();
      RobotButtonsController.Segments.Add(newRbtBtnSegment);
      newRbtBtnSegment.SegmentsController = RobotButtonsController;

      newRobotBtn.GetComponentInChildren<Text>().text = piConnectionManager.Instance.BotsInState(PI.BotConnectionState.CONNECTED)[i].Name;

      RobotButtonsController.Refresh();
    }
  }
}
