using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Turing{
  public class trTutorialTriggerCalloutController : MonoBehaviour, IPointerClickHandler {

    private static int[] DELAY_VALUES = new int[] {8, 11, 15, 20, 30, 60};
    private const float CHALLENGE_DELAY = 4.0f;
    private float delayToShow = DELAY_VALUES[0];

    public GameObject TriggerCalloutPanel;  
    public GameObject DashImage;
    public GameObject DotImage;
    public GameObject TriggerObjPrefab;
    public GridLayoutGroup Grid;

    private trStateMachineRunningController _protoCtrl;
    private trState curState = null;
    private float runningTimer = 0;
    private List<trTrigger> triggers = new List<trTrigger>();
    private List<GameObject> triggerObjectPool = new List<GameObject>();
    private List<trTriggerCalloutViewController> triggerViewsCtrl = new List<trTriggerCalloutViewController>();

    private int DelayIndex{
      get {
        return PlayerPrefs.GetInt(TOKENS.ACTIVATION_TIME, 0);
      }
      set {
        PlayerPrefs.SetInt(TOKENS.ACTIVATION_TIME, value);
      }
    }

    public void SetupView(trStateMachineRunningController protoCtrl){
      _protoCtrl = protoCtrl;
      curState = null;
      runningTimer = 0;
      if(trDataManager.Instance.IsInNormalMissionMode){
        delayToShow = CHALLENGE_DELAY;
      }
      else{
        delayToShow = DELAY_VALUES[DelayIndex];
      }
    }

    void Start(){
      if(_protoCtrl==null){
        trStateMachineRunningController protoCtrl = FindObjectOfType<trStateMachineRunningController>();
        SetupView(protoCtrl);
      }
    }

  	void Update () {
      if(_protoCtrl.IsRunning){
        if(_protoCtrl.CurProgram.StateCurrent != curState){
          runningTimer = 0;
          curState = _protoCtrl.CurProgram.StateCurrent;
          if (TriggerCalloutPanel.activeSelf && !trDataManager.Instance.IsInNormalMissionMode){
            pickupNewTimerInterval();
          }
          _setActive(false);
        }
        runningTimer += Time.deltaTime;
        if(runningTimer > delayToShow && curState != null && curState.OutgoingTransitions.Count >0 ){
          triggers.Clear();
          foreach(trTransition transition in curState.OutgoingTransitions){
            if(transition.Trigger.Type.isRelatedToRobot()){
              triggers.Add(transition.Trigger);
            }           
          }

          if(triggers.Count>0){
            _setActive(true);
            DashImage.SetActive(trDataManager.Instance.CurrentRobotTypeSelected == piRobotType.DASH);
            DotImage.SetActive(trDataManager.Instance.CurrentRobotTypeSelected == piRobotType.DOT);
            setView();
          }
          
        }
      }
      else{
        _setActive(false);
        curState = null;
      }
  	}

    void setView(){
      foreach(Transform child in Grid.transform){
        child.gameObject.SetActive(false);
      }
      if(triggers.Count == 2 ){
        Grid.cellSize = new Vector2(200, 200);
        Grid.constraintCount = 1;
      }
      else if(triggers.Count == 1){
        Grid.cellSize = new Vector2(300, 300);
        Grid.constraintCount = 1;
      }
      else{
        Grid.cellSize = new Vector2(120, 120);
        Grid.constraintCount = 2;
      }
      triggerViewsCtrl.Clear();
      for(int i = 0; i< triggers.Count; ++i){
        GameObject newTrigger = GetTriggerObj();
        newTrigger.SetActive(true);
        trTriggerCalloutViewController ctrl = newTrigger.GetComponent<trTriggerCalloutViewController>();
        ctrl.TriggerImg.sprite = trIconFactory.GetIcon(triggers[i].Type);
        ctrl.Label.text = triggers[i].UserFacingNameLocalized;
        triggerViewsCtrl.Add(ctrl);
      }

    }

    public void OnPointerClick (PointerEventData eventData) {
      runningTimer = 0;
      if(!trDataManager.Instance.IsInNormalMissionMode){
        pickupNewTimerInterval();
      }     
      _setActive(false);
    }

    void _setActive(bool isActive) {
      if (TriggerCalloutPanel.activeSelf & isActive)
      {
        // If the trigger callout panel is already shown on screen, ignore and return
        return;
      }
      if (isActive) {
        SoundManager.soundManager.StopAllSound();
        SoundManager.soundManager.PlaySound(SoundManager.trAppSound.PLAY_WITH_ME_START);
        TriggerCalloutPanel.SetActive(true);
      }
      else {
        TriggerCalloutPanel.SetActive(false);
      }
    }

    void pickupNewTimerInterval(){
      if (DelayIndex + 1 < DELAY_VALUES.Length){
        DelayIndex++;
      }
      delayToShow = DELAY_VALUES[DelayIndex];
    }

    GameObject GetTriggerObj(){
      if(triggerObjectPool.Count == triggerViewsCtrl.Count){
        GameObject trigger = Instantiate(TriggerObjPrefab) as GameObject;
        trigger.transform.SetParent(Grid.transform, false);
        triggerObjectPool.Add(trigger);
      }
      return triggerObjectPool[triggerViewsCtrl.Count];
    }
  }
 
}
