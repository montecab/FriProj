using UnityEngine;
using System.Collections;
using WW.UGUI;
using UnityEngine.UI;
using TMPro;

namespace Turing{
  public class trRobotSelectionController : trUIController {

    public Button SelectDashButton;
    public Button SelectDotButton;   
    public Button DismissButton;
    public TextMeshProUGUI ConnectedDashName;
    public TextMeshProUGUI ConnectedDotName;
    public GameObject SelectionModal;
    public float DismissModalTime = 0.2f;

    private piRobotType selectedType = piRobotType.UNKNOWN;
    private bool shouldAutoSelectRobot = true;

    public delegate void RobotSelectionDelegate(bool selected, piRobotType selectedType);
    public RobotSelectionDelegate OnDismiss;
    
    void Start(){
      // first check if we can just bypass selection altogether
      autoSelectRobotType();
      if (selectedType == piRobotType.UNKNOWN) {
        // callback setup
        SelectDashButton.onClick.AddListener(onSelectionPressedDash);
        SelectDotButton.onClick.AddListener(onSelectionPressedDot);
        DismissButton.onClick.AddListener(onBackButtonClicked);
        // not showing connected robot names yet since the work is hard, punting (TUR-950)
        ConnectedDashName.gameObject.SetActive(false);
        ConnectedDotName.gameObject.SetActive(false);

        // now update UI as needed
        updateUI();
      }
    }

    void Update(){
      autoSelectRobotType();
    }

    void autoSelectRobotType(){
      if (shouldAutoSelectRobot){
        switch(piConnectionManager.Instance.GetConnectedRobotType()){
          case piConnectionManager.ConnectedRobotType.DASH_ONLY:
            selectedType = piRobotType.DASH;
            break;
          case piConnectionManager.ConnectedRobotType.DOT_ONLY:
            selectedType = piRobotType.DOT;
            break;
        }
        if (selectedType != piRobotType.UNKNOWN){
          WWLog.logDebug("auto selected robot type: " + selectedType);
          doSelectAndDismissWithDelay(selectedType, false);
        }        
      }
    }
    
    void updateUI(){     
      WWLog.logDebug("updating UI for some reason");
      SelectionModal.SetActive(true); 
      SelectDashButton.GetComponent<Image>().color = new Color(0, 0, 0, 0);
      SelectDotButton.GetComponent<Image>().color = new Color(0, 0, 0, 0);
      
      switch(selectedType) {
        case piRobotType.DASH:
          SelectDashButton.GetComponent<Image>().color = Color.white;
          break;
        case piRobotType.DOT:
          SelectDotButton .GetComponent<Image>().color = Color.white;
          break;
      }
    }

    void onSelectionPressedDash(){
      doSelectAndDismissWithDelay(piRobotType.DASH, true);
    }
    
    void onSelectionPressedDot(){
      doSelectAndDismissWithDelay(piRobotType.DOT, true);
    }
    
    void doSelectAndDismissWithDelay(piRobotType robotType, bool dismissWithDelay) {
      selectedType = robotType;
      
      if (dismissWithDelay){
        updateUI();
        shouldAutoSelectRobot = false; // already selected but we are deferring dismiss, so make sure we are not trying to auto-select again
        piUnityDelayedExecution.Instance.delayedExecution1<bool>(DismissModal, DismissModalTime, true);
      }
      else {
        DismissModal(true);
      }
    }

    protected override void onBackButtonClicked(){
      DismissModal(false);
    }

    public void DismissModal(bool selected){
      if (OnDismiss != null){
        OnDismiss(selected, selected ? selectedType : piRobotType.UNKNOWN);
      }
    }
  }
}
