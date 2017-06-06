using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace Turing {
  public class trMapperPanelController : MonoBehaviour {
    public Button SensorButton ;
    public Button InvertButton;
    public Image InverImage;
    public Image SensorImage;
    public Image ActuatorImage;
    public Text SensorText;
    public trActuatorConfigPanelController ActuatorConfigCtrl;
    public RectTransform SensorCoordinateArea;
    public trDoubleSliderController SensorSlider;    
    public Slider SensorShowSlider;
    public Slider ActuatorShowSlider;

    public RectTransform ActuatorShowPoint;

    public trCoordinateAxis XAxisInfo;
    public trCoordinateAxis YAxisInfo;

    public trProtoController ProtoCtrl;

    private trMap map;
    public trMap MapData{
      set{
        if(value == null){
          this.gameObject.SetActive(false);
          map = value;
          return;
        }

        this.gameObject.SetActive(true);
        if(map == value){
          return;
        }

        map = value;
        SetView();
      }

      get{
        return map;
      }
    }

    public Text ActuatorLabel;

    private trActuatorType actuatorType;
    public trActuatorType ActuatorType{
      set{
        actuatorType = value;
        ActuatorLabel.text = actuatorType.GetUserFacingName();
        ActuatorConfigCtrl.SetUpSnapPoint(getSnapPointNum(actuatorType));
        ActuatorImage.sprite = trIconFactory.GetIcon(actuatorType);
        YAxisInfo.NormValueToLabelTable = trStringFactory.GetAxisInfo(actuatorType);
        YAxisInfo.UpdateView();
      }
      get{
        return actuatorType;
      }
    }

    public trSensor SensorData{
      set{
        if(MapData.Sensor != null&&
           MapData.Sensor == value){
          return;
        }
        MapData.Sensor = value;
        setSensorView();
        trDataManager.Instance.SaveBehavior();
      }
      get{
        return MapData.Sensor;
      }
    }

    private float saveTime =  float.NaN;

    public void UpdateSensorData(trSensor sensor){
      MapData.Sensor.Type = sensor.Type;
      MapData.Sensor.ParameterValue = sensor.ParameterValue;
      setSensorView();
      trDataManager.Instance.SaveBehavior();
    }

    float getMidSnapValue(trActuatorType type){
      switch(type){
      case trActuatorType.WHEEL_L:
      case trActuatorType.WHEEL_R:
      case trActuatorType.HEAD_PAN:
        return 0.5f;
      case trActuatorType.HEAD_TILT:
        return 0.2f;
      case trActuatorType.RGB_ALL_HUE:
      case trActuatorType.RGB_ALL_VAL:
      case trActuatorType.LED_TOP:
      case trActuatorType.LED_TAIL:
        return float.NaN;
      }
      return 0.5f;
    }

    int getSnapPointNum(trActuatorType type){
      switch(type){
      case trActuatorType.WHEEL_L:
      case trActuatorType.WHEEL_R:
        return 5;
      case trActuatorType.HEAD_PAN:
        return 19;
      case trActuatorType.HEAD_TILT:
        return 9;
      case trActuatorType.RGB_ALL_HUE:
      case trActuatorType.RGB_ALL_VAL:
        return 99;
      case trActuatorType.LED_TOP:
      case trActuatorType.LED_TAIL:
        return 9;
      }
      return 0;
    }

    int getSnapPointNum(trSensorType type){
//      switch(type){
//      case trSensorType.:
//      
//       
//      }
      return 19;
    }

    void FixedUpdate(){

      showIndicator();

      if(!float.IsNaN(saveTime) ){
        saveTime += Time.fixedDeltaTime;
        if(saveTime > 1.0f){
          save ();
          saveTime = float.NaN;
        }
      }
    }


    void showIndicator(){
      bool isValid = ProtoCtrl.CurRobot != null && MapData!= null &&SensorData != null;
      if(isValid){
        ActuatorShowPoint.gameObject.SetActive(true);
        SensorShowSlider.gameObject.SetActive(true);

        SensorData.Robot = (piBotBo)ProtoCtrl.CurRobot;
        SensorShowSlider.value = SensorData.ValueNormalized;
        Color color;
        if(SensorData.ValueNormalized <= MapData.RangeSensor.Max 
           &&SensorData.ValueNormalized >= MapData.RangeSensor.Min){
          color = Color.yellow;
        }else{
          color = wwColorUtil.orange;
        }
        SensorShowSlider.handleRect.GetComponent<Image>().color = color;
        
        float acvalue = MapData.NormalizedSensorValueToNormalizedActuatorValue(SensorData.ValueNormalized);
        Vector2 v = new Vector2(MapData.RangeSensor.Normalize(SensorData.ValueNormalized), acvalue);
        ActuatorShowPoint.transform.SetAsLastSibling();
        Vector3 worldPos = ActuatorShowPoint.transform.position;
        worldPos.y = ActuatorConfigCtrl.GetComponent<RectTransform>().XYRatioToWorldPos(v).y;
        worldPos.x = SensorCoordinateArea.XYRatioToWorldPos(new Vector2(SensorData.ValueNormalized, 0)).x;
        ActuatorShowPoint.transform.position = worldPos;
        setActuatorShowPointView(v.y);
      }
      else{
        ActuatorShowPoint.gameObject.SetActive(false);
        SensorShowSlider.gameObject.SetActive(false);
      }
    }

    void setActuatorShowPointView(float v){
      if(ActuatorType == trActuatorType.RGB_ALL_HUE){
        Color c = trActuator.NormalizedValueToColor(v, 1.0f);
        ActuatorShowPoint.GetComponent<trButtonBase>().Img.color = c;
      }
    }
   
    void setSensorView(){
//      bool isLabelActive = trSensor.Parameterized(SensorData.Type);
//      SensorText.gameObject.SetActive(isLabelActive);
//      SensorText.text = trStringFactory.GetParaString(SensorData);
      SensorImage.sprite = trIconFactory.GetIcon(SensorData.Type);
      SensorSlider.SetUp(0, 1, float.NaN, getSnapPointNum(SensorData.Type));
      XAxisInfo.NormValueToLabelTable = trStringFactory.GetAxisInfo(SensorData);
      XAxisInfo.UpdateView();
    }

    void setInvertView(){
//      Vector3 rot = MapData.InvertSensor? new Vector3(0,0,180) : Vector3.zero;
//      InvertButton.transform.rotation = Quaternion.Euler(rot);
//
//      Color color = MapData.InvertSensor? Color.red: Color.green;
//      InverImage.color = color;
      Sprite sprite = MapData.InvertSensor? InvertButton.spriteState.pressedSprite : InvertButton.spriteState.highlightedSprite;
      InverImage.sprite = sprite;
    }

    void setActuatorRangeView(){
      SetMode(MapData.SimpleMode);
      ActuatorConfigCtrl.AcPoints = MapData.ActuatorPoints;
      ActuatorConfigCtrl.IsInverse = MapData.InvertSensor;
    }

    void setSensorRangeView(){
      SensorSlider.IsPushOtherHandle = false;
      SensorSlider.LowValue = MapData.RangeSensor.Min;
      SensorSlider.HighValue = MapData.RangeSensor.Max;
      SensorSlider.IsPushOtherHandle = true;
    }

    void SetView(){
      if(MapData == null){
        WWLog.logError("Mapdata is null");
        return;
      }
      //Debug.LogError (MapData.Sensor.Type +": "+ MapData.RangeSensor.Min + ", " + MapData.RangeSensor.Max);
      setSensorView();
      setInvertView();
      setActuatorRangeView();
      setSensorRangeView();
    }

    public void Reset(){
      ActuatorConfigCtrl.Reset();
      SensorSlider.Reset();
      if(MapData.InvertSensor){
        onInvertButtonClicked();
      }
    }

  	// Use this for initialization
  	void Start () {
  	  SensorButton.onClick.AddListener(()=>onSensorButtonClicked());
      InvertButton.onClick.AddListener(()=>onInvertButtonClicked());
      SensorSlider.SliderLow.onValueChanged.AddListener(onSensorSliderLowValueChanged);
      SensorSlider.SliderHigh.onValueChanged.AddListener(onSensorSliderHighValueChanged);
  	}

    public void SetMode(bool isSimple){
      //Need to delete this once we devcide to remove simple mode
//      InvertButton.gameObject.SetActive(!isSimple);
//      SensorSlider.gameObject.SetActive(!isSimple);
//      SensorButton.gameObject.SetActive(!isSimple);
//      SensorShowSlider.gameObject.SetActive(!isSimple);
//      ActuatorConfigCtrl.IsSimpleMode = isSimple;
//      if(MapData != null){
//        MapData.SimpleMode = isSimple;
//      }
//     
    }

    void save(){
        trDataManager.Instance.SaveBehavior();
    }

    void onSensorSliderLowValueChanged(float x){
      if(MapData == null){
        return;
      }
      MapData.RangeSensor.setMinMax(SensorSlider.LowValue, MapData.RangeSensor.Max);
      saveTime = 0;
    }

    void onSensorSliderHighValueChanged(float x){
      if(MapData == null){
        return;
      }
      MapData.RangeSensor.setMinMax(MapData.RangeSensor.Min, SensorSlider.HighValue);
      saveTime = 0;
    }
  	
    void onSensorButtonClicked(){
      if(MapData == null){
        return;
      }
      ProtoCtrl.BehaviorMakerPanelCtrl.SensorConfigurePanel.SetActive(true);
      ProtoCtrl.BehaviorMakerPanelCtrl.SensorConfigurePanel.CurMapper = this;
    }

    void onInvertButtonClicked(){
      if(MapData == null){
        return;
      }
      MapData.InvertSensor = !(MapData.InvertSensor);
      ActuatorConfigCtrl.IsInverse = MapData.InvertSensor;
      setInvertView();
      trDataManager.Instance.SaveBehavior();
    }
  }
}
