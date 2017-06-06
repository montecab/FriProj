using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using WW.UGUI;
using UnityEngine.UI;

namespace Turing{
  public class trSensorConfigurePanelController : uGUISegmentedController {
    
    private trMapperPanelController curMapper;
    public trMapperPanelController CurMapper{
      set{
        if(curMapper == value){
          return;
        }
        curMapper = value;
        SetUpView();
      }
      get{
        return curMapper;
      }
    }
    
    public GameObject ButtonPrefab;
    public GameObject ButtonParent;
    
    public Dictionary<trSensorType, trSensorButtonController> sensorTypeToButtonTable = new Dictionary<trSensorType, trSensorButtonController>();
    
    public trSensorParaPanelController SensorParaCtrl;
    public RectTransform SelectionPanel;

    private float lastClickTime = float.NaN;
    private bool isInit = false;

    public void Start(){
      piConnectionManager.Instance.OnConnectRobot += HandleRobotConnectionChanged;
      piConnectionManager.Instance.OnDisconnectRobot += HandleRobotConnectionChanged;
    }

    void HandleRobotConnectionChanged (piBotBase robot) {
      piBotBase configRobot = robot;
      if (robot.connectionState == PI.BotConnectionState.DISCONNECTED){
        configRobot = piConnectionManager.Instance.AnyConnectedBo;
      }
      regenerateSensorsView(configRobot);
      SetUpView();
    }

    public void OnDestroy(){
      if (piConnectionManager.Instance != null){
        piConnectionManager.Instance.OnConnectRobot -= HandleRobotConnectionChanged;
        piConnectionManager.Instance.OnDisconnectRobot -= HandleRobotConnectionChanged;
      }
    }
    
    public void SetUpView(){
      InitView();
      setUpParameterSensorData();
      trSensor curSensor = CurMapper.SensorData;
      if(curSensor != null && sensorTypeToButtonTable.ContainsKey(curSensor.Type)){
        if(trSensor.Parameterized(curSensor.Type)){
          sensorTypeToButtonTable[curSensor.Type].SensorData.ParameterValue = curSensor.ParameterValue;
        }
        ActivateSegment(sensorTypeToButtonTable[curSensor.Type]);
      }else{
        ActivateSegment(Segments[0]);
      }
      
    }

    void setUpParameterSensorData(){
      foreach(trSensorType type in sensorTypeToButtonTable.Keys){
        if(trSensor.Parameterized(type)){
          sensorTypeToButtonTable[type].SensorData.ParameterValue = 0;
        }
      }
    }
    
    public override void ActivateSegment (uGUISegment seg)
    {
      //Double tap to close the panel
      if(float.IsNaN(lastClickTime)){
        lastClickTime = Time.fixedTime;
      }
      else{
        if(seg == activatedSegment){
          if(Time.fixedTime - lastClickTime < 0.5f){
            SetActive(false);
            lastClickTime = float.NaN;
          }else{
            lastClickTime = Time.fixedTime;
          }
        }else{
          lastClickTime = Time.fixedTime;
        }
      }

      trSensor sensor= ((trSensorButtonController)seg).SensorData;
      SensorParaCtrl.CurSensor = sensor;
      
      base.ActivateSegment (seg);
    }

//    void shrinkUIForPara(trSensor sensor){
//      GridLayoutGroup grid = ButtonParent.GetComponent<GridLayoutGroup>();
//      int col = 0;
//      if(trSensor.Parameterized(sensor.Type)){
//        col = 4;
//      }else{
//        col = 7;      
//      }
//      
//      grid.constraintCount = col;      
//      float width = grid.cellSize.x * col + grid.spacing.x * (col - 1) + grid.padding.left + grid.padding.right + 30;
//      SelectionPanel.sizeDelta = new Vector2(width, 0);
//    }
    
    public void OnBackGroundCLicked(){
      SetActive(false);
    }
    
    public void SetActive(bool enabled){
      this.gameObject.SetActive(enabled);
      if(!enabled){
        if(activatedSegment != null){
          trSensorButtonController sensor = (trSensorButtonController)activatedSegment;
          CurMapper.UpdateSensorData(sensor.SensorData);
        }
      }
    }
    
    public void InitView(){
      if(isInit){
        return;
      }

      if(Segments.Count > 0){
        foreach(uGUISegment seg in Segments){
          Destroy(seg.gameObject);
        }
      }

      regenerateSensorsView(piConnectionManager.Instance.FirstConnectedRobot);

      isInit = true;
      
    }

    private void regenerateSensorsView(piBotBase robot){

      foreach(var item in sensorTypeToButtonTable.Values){
        Destroy(item.gameObject);
      }
      sensorTypeToButtonTable.Clear();

      List<trSensor> sensors = new List<trSensor>();
      trSensor newSensor;

      foreach (trSensorType type in System.Enum.GetValues(typeof(trSensorType))) {
        if (trSensor.ShowToUser(type, robot)) {
          newSensor = new trSensor(type);
          newSensor.UUID = "SENSOR_" + type.ToString();
          sensors.Add(newSensor);
        }
      }
      
      for(int i = 0; i< sensors.Count; ++i){
        GameObject newButton = Instantiate(ButtonPrefab, ButtonPrefab.transform.position, Quaternion.identity) as GameObject;
        newButton.transform.SetParent(ButtonParent.transform, false);
        trSensorButtonController ctrl =  newButton.GetComponent<trSensorButtonController>();
        ctrl.SensorData = sensors[i];
        ctrl.SegmentsController = this;
        AddSegment(ctrl);
        sensorTypeToButtonTable.Add(sensors[i].Type, ctrl);
        
      }
    }
  }
}
