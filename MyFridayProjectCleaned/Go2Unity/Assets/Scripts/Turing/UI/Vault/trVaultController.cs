using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using TMPro;
using Turing;
using WW.SimpleJSON;

namespace Turing {
  public class trVaultController : MonoBehaviour {

    public GameObject                    MainUI;
    public trVaultScrollQuestsController ScrollQuestsController;
    public trVaultMyProgramsController MyProgramsController;
    public TextMeshProUGUI TitleLabel;

    public Button BackButton;
    public Button ScrollQuestsButton;
    public Button MyProgramsButton;
    public Button CreateNewProgramButton;

    public enum DisplayMode {
      MainMenu = 0, 
      ScrollQuests = 1,
      MyPrograms = 2,
      NewProgram = 3
    }
    private DisplayMode currentDisplayMode;
    trAppSaveInfo trASI;

    void Start () {
  	  Setup();

      string s = trNavigationRouter.Instance.GetTransitionParameterForScene();
      if (s != null) {
        piStringUtil.ParseStringToEnum<DisplayMode>(s, out currentDisplayMode);   
        setDisplayMode(currentDisplayMode);     
      }
      else {
        setDisplayMode(DisplayMode.MainMenu); // by default
      }
  	}

    void Setup() {  
      trDataManager.Instance.Init();

      trASI = trDataManager.Instance.AppSaveInfo;
      ScrollQuestsController.SQProgress = trDataManager.Instance.MissionMng.UserOverallProgress;
      MyProgramsController.AppSaveInfo = trASI;

      BackButton.onClick.AddListener(onClickBack);
      MyProgramsButton.onClick.AddListener(delegate{setDisplayMode(DisplayMode.MyPrograms);});
      ScrollQuestsButton.onClick.AddListener(delegate{setDisplayMode(DisplayMode.ScrollQuests);});
      CreateNewProgramButton.onClick.AddListener(onClickNewProgram);
    }
    
    void onClickBack() {
      if (currentDisplayMode == DisplayMode.MainMenu){  
        // exit out of this scene 
        trNavigationRouter.Instance.ShowSceneWithName(trNavigationRouter.SceneName.LOBBY);
      }
      else {
        setDisplayMode(DisplayMode.MainMenu);
      }
    }

    void setDisplayMode(DisplayMode mode) {      
      switch (mode) {
        case DisplayMode.MainMenu:
          MainUI.SetActive(true);
          MyProgramsController.gameObject.SetActive(false);
          ScrollQuestsController.gameObject.SetActive(false);
          TitleLabel.text = wwLoca.Format("@!@File menu@!@");
          break;
        case DisplayMode.MyPrograms:
          MainUI.SetActive(false);
          TitleLabel.text = wwLoca.Format("@!@My programs@!@");
          MyProgramsController.ShowPanel();
          break;
        case DisplayMode.ScrollQuests:
          MainUI.SetActive(false);
          TitleLabel.text = wwLoca.Format("@!@Scroll quests@!@");
          ScrollQuestsController.ShowPanel();
          break;
      }
      currentDisplayMode = mode;
    }
    
    void onClickNewProgram() {
      trASI.CreateProgram(trDataManager.Instance.CurrentRobotTypeSelected);

      new trTelemetryEvent(trTelemetryEventType.FP_PROGRAM_NEW, true)
        .add(trTelemetryParamType.ROBOT_TYPE, trDataManager.Instance.CurrentRobotTypeSelected)
        .add(trTelemetryParamType.NUM_PROGS, trASI.Programs.Count)
        .emit();

      trNavigationRouter.Instance.ShowSceneWithName(trNavigationRouter.SceneName.MAIN, trProtoController.RunMode.FreePlay.ToString());
    }

  }
}