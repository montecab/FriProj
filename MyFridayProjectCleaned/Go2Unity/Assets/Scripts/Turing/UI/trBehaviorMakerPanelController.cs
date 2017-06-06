using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using WW.UGUI;
using WW.SimpleJSON;

namespace Turing{
  public class trBehaviorMakerPanelController : uGUISegmentedController {

    public Button RunButton;
    public Image  StopImage;
    public Text   RunButtonInfo;

    public Button DoneButton;
    public Button BehaviorInfoButton;
    public Button ModeButton;
    public Button ResetButton;

    public Button ToClipboardButton;
    public Button FromClipboardButton;

    public GameObject ActivateCover;

    public Text   InfoText;
    public Image  InfoImage;
    public Text ModeButtonText;

    public GameObject ActuatorButtonParent;
    public GameObject ActuatorButtonPrefab;

    public trActuatorPanelControllerBase WheelsPanel;
    public trActuatorPanelControllerBase HeadPanel;
    public trActuatorPanelControllerBase RGBPanel;
    public trActuatorPanelControllerBase LEDPanel;
    
    public trActuatorPanelControllerBase CurPanel;

    public trSensorConfigurePanelController SensorConfigurePanel;
    public trBehaviorInfoConfigurePanelController InfoConfigurePanel;

    public trProtoController ProtoCtrl;

    private bool isSimpleMode = false;

    private bool isRunning = false;

    private Dictionary<trButtonBase, trActuatorPanelControllerBase> buttonToPanelDic = new Dictionary<trButtonBase, trActuatorPanelControllerBase>();

    private trBehaviorButtonController curBehaviorButton;
    public trBehaviorButtonController CurBehaviorButton{
      set{
        if(curBehaviorButton == value){
          return;
        }

        curBehaviorButton = value;
        behaviorData = curBehaviorButton.BehaviorData;
      }
      get{
        return curBehaviorButton;
      }
    }

   // private trBehavior behaviorData; // oxe: why "behaviorData" instead of just "behavior" ?
    private trBehavior behaviorData{
      set{

        if(value.Type != trBehaviorType.MAPSET){
          WWLog.logError("The behavior type is not mapper.");
          return;
        }

        SetUpView();
        trMapSet mapSet = behaviorData.MapSet;
        if (mapSet == null) {
          WWLog.logError("null MapSet in behavior: " + " (" + behaviorData.Type.ToString() + ")");
          return;
        }

        for(int i=0; i< ActuatorPanels.Count; ++i){
          for(int j = 0; j< ActuatorPanels[i].MapperCtrls.Count; ++j){
            trMap map= findMap(mapSet, ActuatorPanels[i].MapperCtrls[j].ActuatorType);
            ActuatorPanels[i].MapperCtrls[j].MapData = map;
          }
        }

        Segments[0].OnClickSegment();
       
      }
      get{
        return curBehaviorButton.BehaviorData;
      }
    }

    private List<trActuatorPanelControllerBase> ActuatorPanels = new List<trActuatorPanelControllerBase>();

    private bool isInit = false;

    private trMapperPanelController getMapperCtrl(trActuatorType type ){
      for(int i = 0; i< ActuatorPanels.Count; ++i){
        for(int j = 0; j< ActuatorPanels[i].MapperCtrls.Count; ++j){
          if(ActuatorPanels[i].MapperCtrls[j].ActuatorType == type){
            return ActuatorPanels[i].MapperCtrls[j];
          }
        }
      }
      return null;
    }
    
  	// Use this for initialization
  	void Awake () {
  	  RunButton.onClick.AddListener(()=>onRunButtonClicked());
      DoneButton.onClick.AddListener(()=>onDoneButtonClicked());
      BehaviorInfoButton.onClick.AddListener(()=>onBehaviorInfoButtonClicked());
      ModeButton.onClick.AddListener(()=>onModeButtonClicked());
      ResetButton.onClick.AddListener(()=>onResetButtonClicked());

      ToClipboardButton.onClick.AddListener(() => saveToClipboard());
      FromClipboardButton.onClick.AddListener(() => loadFromClipboard());

      SensorConfigurePanel.InitView();
      InitView();

  	}

    public void SetInfoView(){
      InfoText.text = behaviorData.MapSet.Name;
      InfoImage.sprite = trIconFactory.GetIcon(behaviorData.MapSet.IconName);
    }


    public void SetUpView(){
      InitView();
      SetInfoView();
      setRunButtonView();
    }

    void setRunButtonView(){
      RunButton.enabled = !ProtoCtrl.IsRunning;
      RunButton.image.color = ProtoCtrl.IsRunning? Color.gray: Color.white;
      RunButtonInfo.gameObject.SetActive(ProtoCtrl.IsRunning);
    }

    public void SetActive(trBehaviorButtonController ctrl){
      CurBehaviorButton = ctrl;
      this.gameObject.SetActive(true);
    }

    public void SetMapsActive(bool active){
      for(int i = 0; i< CurPanel.MapperCtrls.Count; ++i){
        if(findMap(behaviorData.MapSet, CurPanel.MapperCtrls[i].ActuatorType) != null){
          CurPanel.MapperCtrls[i].MapData.Active = active;
        }else{
          trMap newMap = new trMap() ;
          newMap.Actuator = new trActuator(CurPanel.MapperCtrls[i].ActuatorType);
          newMap.Sensor = new trSensor(trSensorType.TIME_IN_STATE);
          newMap.Sensor.ParameterValue = 5.0f;
          behaviorData.MapSet.Maps.Add(newMap);
          CurPanel.MapperCtrls[i].MapData = newMap;
          CurPanel.MapperCtrls[i].MapData.Active = active;
        }
      }
    }

    trMap findMap(trMapSet mapSet, trActuatorType type){
      for(int i = 0; i< mapSet.Maps.Count; ++i){
        if(type == mapSet.Maps[i].Actuator.Type){
          return mapSet.Maps[i];
        }
      }
      return null;
    }

    void Update(){
      if(isRunning){
        if(behaviorData.MapSet != null && ProtoCtrl.CurRobot != null){
          behaviorData.MapSet.onRobotState(ProtoCtrl.CurRobot);
        }
      }
    }

    #region Clipboard Handlers
    void saveToClipboard(){
      string json = behaviorData.ToJson().ToString("");
      trClipboardManager.ClipboardValue = json;
    }

    void loadFromClipboard(){
      string savedData = trClipboardManager.ClipboardValue;
      JSONNode node = JSON.Parse(savedData);
      trBehavior fromClipboard = trFactory.FromJson<trBehavior>(node);
      curBehaviorButton.BehaviorData = fromClipboard;
      behaviorData = fromClipboard;
    }
    #endregion
  	
    #region Button listeners
    void onRunButtonClicked(){
      isRunning = !isRunning;
      StopImage.gameObject.SetActive(isRunning);

      if(!isRunning && ProtoCtrl.CurRobot != null){
        ProtoCtrl.CurRobot.cmd_reset();
      }
    }

    void onDoneButtonClicked(){
      if(isRunning){
        onRunButtonClicked();
      }
      this.gameObject.SetActive(false);
      curBehaviorButton.SetUpView();
      trDataManager.Instance.SaveBehavior();
      ProtoCtrl.StateEditCtrl.UpdateStates();
    }

    void onBehaviorInfoButtonClicked(){
      InfoConfigurePanel.gameObject.SetActive(true);
      InfoConfigurePanel.BehaviorData = behaviorData;
    }

    void onModeButtonClicked(){
      isSimpleMode = !isSimpleMode;
      string buttonText = isSimpleMode? "COMPLEX" :"SIMPLE";
      ModeButtonText.text = buttonText;
      CurPanel.SetMode(isSimpleMode);
      CurPanel.ResetActuators();
      trDataManager.Instance.SaveBehavior();
    }


    void onActivateButtonClicked(uGUISwitch button){
      ActivateSegment(button.gameObject.GetComponentInParent<uGUITabButtonController>());
      setActiveButton(button);
      trDataManager.Instance.SaveBehavior();
    }

    void onResetButtonClicked(){
      if(CurPanel != null){
        CurPanel.Reset();
      }
      trDataManager.Instance.SaveBehavior();
    }
    #endregion

    public override void ActivateSegment (uGUISegment seg)
    {
      base.ActivateSegment (seg);
      trButtonBase button = seg.gameObject.GetComponent<trButtonBase>();
      CurPanel = buttonToPanelDic[button];

      trSwitchHolder swithHolder = seg.gameObject.GetComponent<trSwitchHolder>();
      setActiveButton(swithHolder.ActiveSwitch);

      //NOTE: ignore since we are removing simple mode
//      isSimpleMode = CurPanel.IsSimpleMode;
//      buttonText = isSimpleMode? "COMPLEX" :"SIMPLE";
//      ModeButtonText.text = buttonText;
//      CurPanel.SetMode(isSimpleMode);
    }

    void setActiveButton(uGUISwitch button){
      ActivateCover.SetActive(!button.IsActive);
      SetMapsActive(button.IsActive);
    }

    void InitView(){
      if(isInit){
        return;
      }
      createActuatorButton(trActuatorType.WHEEL_L    , WheelsPanel);
      createActuatorButton(trActuatorType.HEAD_PAN   , HeadPanel);
      createActuatorButton(trActuatorType.RGB_ALL_HUE, RGBPanel);
      createActuatorButton(trActuatorType.LED_TOP    , LEDPanel);
      
      WheelsPanel.MapperCtrls[0].ActuatorType = trActuatorType.WHEEL_L;
      WheelsPanel.MapperCtrls[1].ActuatorType = trActuatorType.WHEEL_R;

      HeadPanel.MapperCtrls[0].ActuatorType = trActuatorType.HEAD_PAN;
      HeadPanel.MapperCtrls[1].ActuatorType = trActuatorType.HEAD_TILT;

      RGBPanel.MapperCtrls[0].ActuatorType = trActuatorType.RGB_ALL_HUE;
      RGBPanel.MapperCtrls[1].ActuatorType = trActuatorType.RGB_ALL_VAL;

      LEDPanel.MapperCtrls[0].ActuatorType = trActuatorType.LED_TOP;
      LEDPanel.MapperCtrls[1].ActuatorType = trActuatorType.LED_TAIL;

      ActuatorPanels.Add(WheelsPanel);
      ActuatorPanels.Add(HeadPanel);
      ActuatorPanels.Add(RGBPanel);
      ActuatorPanels.Add(LEDPanel);
      
      isInit = true;
    }

    void createActuatorButton(trActuatorType type, trActuatorPanelControllerBase content){
      GameObject newButton = Instantiate(ActuatorButtonPrefab, ActuatorButtonPrefab.transform.position, ActuatorButtonPrefab.transform.rotation) as GameObject;
      newButton.transform.SetParent(ActuatorButtonParent.transform, false);
      trButtonBase buttonbase = newButton.GetComponent<trButtonBase>();
      buttonbase.Img.sprite = trIconFactory.GetIcon(type);

      uGUISegment segment = newButton.GetComponent<uGUITabButtonController>();
      segment.SegmentsController = this;
      Segments.Add(segment);

      uGUISwitch activeSwitch = newButton.GetComponent<trSwitchHolder>().ActiveSwitch;
      activeSwitch.onClick.AddListener(delegate {
        uGUISwitch local = activeSwitch;
        onActivateButtonClicked(local);
      });

      segment.Contents.Add(content.gameObject);

      buttonToPanelDic.Add(buttonbase, content);
    }
    
    
    public void onClickResetTime() {
      trBehavior trBHV = behaviorData;
      if (trBHV == null) {
        WWLog.logError("no behavior.");
        return;
      }
      if (trBHV.MapSet == null) {
        WWLog.logError("behavior has no mapset.");
        return;
      }
      
      trBHV.MapSet.resetTime();
    }    
  }
}
