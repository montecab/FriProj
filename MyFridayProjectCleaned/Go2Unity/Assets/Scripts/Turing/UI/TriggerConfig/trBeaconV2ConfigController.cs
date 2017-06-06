using UnityEngine;
using UnityEngine.UI;
using Turing;
using PI;
using System.Collections.Generic;
using System;

public class trBeaconV2ConfigController : trTriggerConfigCustomControllerBase {

  public Toggle matchAye;
  public Toggle matchNay;
  
  public Toggle typeAny;
  public Toggle typeDash;
  public Toggle typeDot;
  
  public Toggle distAny;
  public Toggle distNear;
  public Toggle distMiddle;
  public Toggle distFar;
  
  public Toggle receiversAny;
  public Toggle receiversLeft;
  public Toggle receiversBoth;
  public Toggle receiversRight;
  
  public Toggle colorAny;
  public Toggle colorWhite;
  public Toggle colorRed;
  public Toggle colorOrange;
  public Toggle colorYellow;
  public Toggle colorGreen;
  public Toggle colorCyan;
  public Toggle colorBlue;
  public Toggle colorMagenta;
  public Toggle colorBlack;
  
  public Toggle idAny;
  public Toggle idSpecific;
  public Text   idSpecificLabel;
  public Text   idCurrent;
  public Button idLatchCurrent;
  
  public void Start() {
    registerCallbacks();
  }
  
  public void Update() {
    updateCurrentSeenRobotID();
  }
  
  public override void SetUp(trTransition tran, trTrigger trig) {
  
    // TODO: this seems gnarly, in terms of logic.
    //       we should clarify what's going on w/ the trigger configure panel.
    if (tran != null) {
      trig.BeaconV2Params = tran.Trigger.BeaconV2Params;
    }
    
    base.SetUp(tran, trig);
    
    if (trigger.BeaconV2Params == null) {
      trigger.BeaconV2Params = new trTriggerBeaconV2.Parameters_t();
    }

    paramsToUI();
  }
  
  public trTriggerBeaconV2.Parameters_t Parameters {
    get {
      return trigger.BeaconV2Params;
    }
  }
  
  private Dictionary<Toggle, bool            > _dictUIMatch;
  private Dictionary<Toggle, piRobotType     > _dictUIRobotType;
  private Dictionary<Toggle, WWBeaconLevel   > _dictUIBeaconLevel;
  private Dictionary<Toggle, WWBeaconReceiver> _dictUIReceivers;
  private Dictionary<Toggle, WWBeaconColor   > _dictUIColor;
  private List      <Toggle                  > _listUIID;
  
  
  private Dictionary<Toggle, bool         > DictUIMatch {
    get {
      if (_dictUIMatch == null) {
        _dictUIMatch = new Dictionary<Toggle, bool> {
          { matchAye  , true },
          { matchNay  , false},
        };
      }
      
      return _dictUIMatch;
    }
  }
    
  private Dictionary<Toggle, piRobotType  > DictUIRobotType {
    get {
      if (_dictUIRobotType == null) {
        _dictUIRobotType = new Dictionary<Toggle, piRobotType> {
          { typeAny , piRobotType.UNKNOWN },
          { typeDash, piRobotType.DASH    },
          { typeDot , piRobotType.DOT     },
        };
      }
      
      return _dictUIRobotType;
    }
  }
  
  private Dictionary<Toggle, WWBeaconLevel> DictUIBeaconLevel {
    get {
      if (_dictUIBeaconLevel == null) {
        _dictUIBeaconLevel = new Dictionary<Toggle, WWBeaconLevel> {
          { distAny   , WWBeaconLevel.BEACON_LEVEL_OFF         },
          { distNear  , WWBeaconLevel.BEACON_LEVEL_LOW         },
          { distMiddle, WWBeaconLevel.BEACON_LEVEL_MEDIUM      },
          { distFar   , WWBeaconLevel.BEACON_LEVEL_MEDIUM_HIGH },
        };
      }
      
      return _dictUIBeaconLevel;
    }
  }
  
  private Dictionary<Toggle, WWBeaconReceiver> DictUIReceivers {
    get {
      if (_dictUIReceivers == null) {
        _dictUIReceivers = new Dictionary<Toggle, WWBeaconReceiver> {
          { receiversAny  , WWBeaconReceiver.WW_BEACON_RECEIVER_UNKNOWN },
          { receiversLeft , WWBeaconReceiver.WW_BEACON_RECEIVER_LEFT    },
          { receiversBoth , WWBeaconReceiver.WW_BEACON_RECEIVER_LEFT | WWBeaconReceiver.WW_BEACON_RECEIVER_RIGHT },
          { receiversRight, WWBeaconReceiver.WW_BEACON_RECEIVER_RIGHT  },
        };
      }
      
      return _dictUIReceivers;
    }
  }
  
  private Dictionary<Toggle, WWBeaconColor> DictUIColor {
    get {
      if (_dictUIColor == null) {
        _dictUIColor = new Dictionary<Toggle, WWBeaconColor> {
          { colorAny    , WWBeaconColor.WW_ROBOT_COLOR_INVALID },
          { colorWhite  , WWBeaconColor.WW_ROBOT_COLOR_WHITE   },
          { colorRed    , WWBeaconColor.WW_ROBOT_COLOR_RED     },
          { colorOrange , WWBeaconColor.WW_ROBOT_COLOR_ORANGE  },
          { colorYellow , WWBeaconColor.WW_ROBOT_COLOR_YELLOW  },
          { colorGreen  , WWBeaconColor.WW_ROBOT_COLOR_GREEN   },
          { colorCyan   , WWBeaconColor.WW_ROBOT_COLOR_BLUE    },
          { colorBlue   , WWBeaconColor.WW_ROBOT_COLOR_BLUE2   },
          { colorMagenta, WWBeaconColor.WW_ROBOT_COLOR_PURPLE  },
          { colorBlack  , WWBeaconColor.WW_ROBOT_COLOR_OFF     },
        };
      }
      
      return _dictUIColor;
    }
  }
  
  private List<Toggle> ListUIID {
    get {
      if (_listUIID == null) {
        _listUIID = new List<Toggle> {
          idAny,
          idSpecific,
        };
      }
      return _listUIID;
    }
  }
  
  private void userDidSomething(bool unused) {
    if (_safeSettingCount <= 0) {
      UIToParams(Parameters);
    }
  }
  
  private void registerCallbacksForToggles(List<Toggle> items) {
    foreach (Toggle tog in items) {
      tog.onValueChanged.AddListener(userDidSomething);
    }
  }
  
  private void registerCallbacks() {
    registerCallbacksForToggles(new List<Toggle>(DictUIMatch      .Keys));
    registerCallbacksForToggles(new List<Toggle>(DictUIRobotType  .Keys));
    registerCallbacksForToggles(new List<Toggle>(DictUIBeaconLevel.Keys));
    registerCallbacksForToggles(new List<Toggle>(DictUIReceivers  .Keys));
    registerCallbacksForToggles(new List<Toggle>(DictUIColor      .Keys));
    registerCallbacksForToggles(ListUIID                                );
    idLatchCurrent.onClick.AddListener(onClickLatchID);
  }
  
  
  private void paramToUI<T>(Dictionary<Toggle, T> dict, T val) where T : IComparable {
    foreach (Toggle tog in dict.Keys) {
      safeSetToggle(tog, (val.CompareTo(dict[tog]) == 0));
    }
    
    int onCount = 0;
    foreach (Toggle tog in dict.Keys) {
      onCount += tog.isOn ? 1 : 0;
    }
    if (onCount != 1) {
      WWLog.logError("one toggle should have been activated, but " + onCount + " were. val = " + val.ToString());
    }
  }
  
  private void paramsToUI() {
    trTriggerBeaconV2.Parameters_t p = Parameters;
    
    paramToUI<bool            >(DictUIMatch      , p.match             );
    paramToUI<piRobotType     >(DictUIRobotType  , p.otherType         );
    paramToUI<WWBeaconLevel   >(DictUIBeaconLevel, p.otherDistanceLevel);
    paramToUI<WWBeaconReceiver>(DictUIReceivers  , p.selfReceivers     );
    paramToUI<WWBeaconColor   >(DictUIColor      , p.otherColor        );
    
    safeSetToggle(idAny     , (p.otherID == 0));
    safeSetToggle(idSpecific, (p.otherID != 0));

    if (p.otherID != 0) {
      idSpecificLabel.text = p.otherID.ToString();
    }
  }
  
  private T UIToParam<T>(Dictionary<Toggle, T> dict) {
    foreach (Toggle tog in dict.Keys) {
      if (tog.isOn) {
        return dict[tog];
      }
    }
    
    WWLog.logError("no toggle is active. type: " + typeof(T).ToString());
    return default(T);
  }
  
  private void UIToParams(trTriggerBeaconV2.Parameters_t p) {
    p.otherType          = UIToParam<piRobotType     >(DictUIRobotType  );
    p.otherDistanceLevel = UIToParam<WWBeaconLevel   >(DictUIBeaconLevel);
    p.selfReceivers      = UIToParam<WWBeaconReceiver>(DictUIReceivers  );
    p.otherColor         = UIToParam<WWBeaconColor   >(DictUIColor      );
    p.match              = UIToParam<bool            >(DictUIMatch      );
    
    if (idSpecific.isOn) {
      if (!uint.TryParse(idSpecificLabel.text, out p.otherID)) {
        p.otherID = 0;
        safeSetToggle(idAny     , true );
        safeSetToggle(idSpecific, false);
      }
    }
    else {
      p.otherID = 0;
    }
  }
  
  private void updateCurrentSeenRobotID() {
    uint value = 0;
    if (ProtoController && (ProtoController.CurRobot != null)) {
      if (ProtoController.CurRobot.robotType == piRobotType.DASH) {
        piBotBo robot = (piBotBo)ProtoController.CurRobot;
        value = robot.BeaconV2.CurrentData.robotID;
      }
    }
    
    if (value == 0) {
      idCurrent.text = "--";
    }
    else {
      idCurrent.text = value.ToString();
    }
  }
  
  private void onClickLatchID() {
    idSpecificLabel.text = idCurrent.text;
    safeSetToggle(idAny     , false);
    safeSetToggle(idSpecific, true );
    UIToParams(Parameters);
  }
  
  // ARG. this is because unity will issue the Something Changed callback when we set IsOn.
  private int _safeSettingCount = 0;
  private void safeSetToggle(Toggle toggle, bool on) {
    _safeSettingCount += 1;
    toggle.isOn = on;
    _safeSettingCount -= 1;
    
    if (_safeSettingCount < 0) {
      WWLog.logError("something went wrong. resetting _safeSettingCount to 0");
      _safeSettingCount = 0;
    }
  }
}




