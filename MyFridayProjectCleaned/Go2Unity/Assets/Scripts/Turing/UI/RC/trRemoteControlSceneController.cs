using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Turing{
  public class trRemoteControlSceneController : trUIController {

    public piBotBase CurRobot{
      get{
        return trCurRobotController.Instance.CurRobot;
      }
    }
   
    public GameObject[] DashOnlyPanels;
    public GameObject[] DotOnlyPanels;

    private static trRemoteControlSceneController singleton;
    // singleton only for this scene
    public static trRemoteControlSceneController Instance
    {
      get
      {
        if(singleton==null)
        {
          singleton = new trRemoteControlSceneController();
        }
        return singleton;
      }
    }

    protected override void Awake () {    
      if(singleton != null)
      {
        Destroy(this.gameObject);
        return;
      }
      singleton = this;
      base.Awake();
    }

  	void Start () {
      trDataManager.Instance.Init();
      trCurRobotController.Instance.CheckRobotType = true;
      trCurRobotController.Instance.CheckConnectRobot();
      trCurRobotController.Instance.CheckRobotType = false;
      trCurRobotController.Instance.onConnectCurRobot    += onConnectRobot;

      setUpPanelsForRobotType(trDataManager.Instance.CurrentRobotTypeSelected);
  	}

    protected override void OnDisable(){
      if (trCurRobotController.Instance){
        trCurRobotController.Instance.onConnectCurRobot    -= onConnectRobot;
      }
      base.OnDisable();
    }

    protected override void onBackButtonClicked (){
      trNavigationRouter.Instance.ShowSceneWithName(trNavigationRouter.SceneName.LOBBY);
    }

    void onConnectRobot(piBotBase robot) {
      
      if (CurRobot == null) {
        return;
      }

      piRobotType type = CurRobot == null? piRobotType.DASH : CurRobot.robotType;
      setUpPanelsForRobotType(type);
    }

    void setUpPanelsForRobotType(piRobotType type){
      bool isDash = type == piRobotType.DASH;
      setActiveForPanels(isDash, DashOnlyPanels);
      setActiveForPanels(!isDash, DotOnlyPanels);
    }

    void setActiveForPanels(bool isActive, GameObject[] panels){
      for(int i = 0; i< panels.Length; ++i){
        panels[i].SetActive(isActive);
      }
    }
  }
}
