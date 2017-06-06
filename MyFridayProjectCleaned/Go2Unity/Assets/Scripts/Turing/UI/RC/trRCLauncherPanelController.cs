using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;


namespace Turing{
  public class trRCLauncherPanelController : MonoBehaviour {


    public Button LauncherLoadLeftButton;
    public Button LauncherLoadRightButton;
    public Button LauncherFlingButton;
    public Button LauncherTossButton;
    public Button HelpButton;

    public GameObject LauncherWarningDialog;
    private bool hasShownLauncherWarningDialog;

    public GameObject InfoPanel;
    public Button InfoPanelHelpButton;
    public Button InfoPanelDoneButton;


    private piBotBo robot{
      get{
        return (piBotBo)(trCurRobotController.Instance.CurRobot);
      }
    }

    // Use this for initialization
    void Start () {
      LauncherFlingButton.onClick.AddListener(onLauncherFlingClicked);
      LauncherLoadLeftButton.onClick.AddListener(onLauncherLoadLeftClicked);
      LauncherLoadRightButton.onClick.AddListener(onLauncherLoadRightClicked);
      LauncherTossButton.onClick.AddListener(onLauncherTossButtonClicked);

      InfoPanelDoneButton.onClick.AddListener(onDoneButtonClicked);
      InfoPanelHelpButton.onClick.AddListener(onHelpButtonClicked);
      HelpButton.onClick.AddListener(onHelpButtonClicked);

    }

    void onHelpButtonClicked(){
      LauncherWarningDialog.gameObject.SetActive(true);
    }

    void onDoneButtonClicked(){
      InfoPanel.gameObject.SetActive(false);
    }

    void onLauncherTossButtonClicked(){
      trCurRobotController.Instance.CheckOpenChrome();
      if(robot != null){
        robot.cmd_launcher_fling(0);
      }
    }

    void onLauncherLoadLeftClicked(){
      trCurRobotController.Instance.CheckOpenChrome();
      if(robot != null){
        robot.cmd_launcher_reload_left();
      }
      SoundManager.soundManager.PlaySound(SoundManager.trAppSound.RC_LAUNCHER_LOAD);
    }
    
    void onLauncherLoadRightClicked(){
      trCurRobotController.Instance.CheckOpenChrome();
      if(robot != null){
        robot.cmd_launcher_reload_right();
      }
      SoundManager.soundManager.PlaySound(SoundManager.trAppSound.RC_LAUNCHER_LOAD);
    }
    
    void onLauncherFlingClicked(){
      trCurRobotController.Instance.CheckOpenChrome();
      if(robot != null){
        robot.cmd_launcher_fling(1.0f);
      }
      SoundManager.soundManager.PlaySound(SoundManager.trAppSound.RC_LAUNCHER_LAUNCH);
    }
  }

}
