using UnityEngine;
using System.Collections.Generic;

public enum wwLEDColor {
  BLACK,
  WHITE,
  YELLOW,
  GREEN,
  CYAN,
  BLUE,
  MAGENTA,
  ORANGE,
  RED,
}

// RGB values for "WonderWorkshop" versions of various colors,
// calibrated for display on actual robots.
// Note these may not look how you expect when viewed on a regular display device.

public static class wwLEDColors {

  private static Color intColor(int r, int g, int b) {
    return new Color((float)r / 255f, (float)g / 255f, (float)b / 255f);
  }

  private static Dictionary<wwLEDColor, Color> dictForRobot = new Dictionary<wwLEDColor, Color> {
    {wwLEDColor.BLACK  , intColor(  0,   0,   0)},
    {wwLEDColor.WHITE  , intColor(180, 171, 120)},
    {wwLEDColor.YELLOW , intColor(255, 218,   0)},
    {wwLEDColor.GREEN  , intColor( 33, 255,  11)},
    {wwLEDColor.CYAN   , intColor(  0, 167, 255)},
    {wwLEDColor.BLUE   , intColor(  0,   0, 255)},
    {wwLEDColor.MAGENTA, intColor(255,   0, 170)},
    {wwLEDColor.ORANGE , intColor(255,  50,   0)},
    {wwLEDColor.RED    , intColor(255,   0,   0)},
  };
  
  private static Dictionary<wwLEDColor, Color> dictForScreen = new Dictionary<wwLEDColor, Color> {
    {wwLEDColor.BLACK  , intColor(  0,   0,   0)},
    {wwLEDColor.WHITE  , intColor(255, 255, 255)},
    {wwLEDColor.YELLOW , intColor(255, 255,   0)},
    {wwLEDColor.GREEN  , intColor(  0, 255,   0)},
    {wwLEDColor.CYAN   , intColor(  0, 255, 255)},
    {wwLEDColor.BLUE   , intColor(  0,   0, 255)},
    {wwLEDColor.MAGENTA, intColor(255,   0, 255)},
    {wwLEDColor.ORANGE , intColor(255, 200,   0)},
    {wwLEDColor.RED    , intColor(255,   0,   0)},
  };

  private static Dictionary<wwLEDColor, string>dictOfNames = new Dictionary<wwLEDColor, string> {
    {wwLEDColor.BLACK  , "@!@Off@!@"    },
    {wwLEDColor.WHITE  , "@!@White@!@"  },
    {wwLEDColor.YELLOW , "@!@Yellow@!@" },
    {wwLEDColor.GREEN  , "@!@Green@!@"  },
    {wwLEDColor.CYAN   , "@!@Cyan@!@"   },
    {wwLEDColor.BLUE   , "@!@Blue@!@"   },
    {wwLEDColor.MAGENTA, "@!@Magenta@!@"},
    {wwLEDColor.ORANGE , "@!@Orange@!@" },
    {wwLEDColor.RED    , "@!@Red@!@"    },
  };
  
  public static Color UnityColor(this wwLEDColor c){
    return wwCollectionUtils.dictSafeAccess<wwLEDColor, Color>(dictForRobot, c, Color.black);
  }
  
  public static Color UnityColor(this PI.WWBeaconColor c) {
    return UnityColor(wwLEDColors.beaconToLED(c));
  }
  
  public static Color UnityColorForScreen(this wwLEDColor c){
    return wwCollectionUtils.dictSafeAccess<wwLEDColor, Color>(dictForScreen, c, Color.black);
  }
  
  public static string UserFacingName(this wwLEDColor c) {
    return wwCollectionUtils.dictSafeAccess<wwLEDColor, string>(dictOfNames, c, "Mystery");
  }

  public static Color UnityColorForScreen(this PI.WWBeaconColor c) {
    return UnityColorForScreen(wwLEDColors.beaconToLED(c));
  }

  private static Dictionary<PI.WWBeaconColor, wwLEDColor> _beaconToLEDDict = null;
  private static Dictionary<PI.WWBeaconColor, wwLEDColor> BeaconToLEDDict {
    get {
      if (_beaconToLEDDict == null) {
        _beaconToLEDDict = new Dictionary<PI.WWBeaconColor, wwLEDColor> {
          {PI.WWBeaconColor.WW_ROBOT_COLOR_OFF   , wwLEDColor.BLACK},
          {PI.WWBeaconColor.WW_ROBOT_COLOR_WHITE , wwLEDColor.WHITE},
          {PI.WWBeaconColor.WW_ROBOT_COLOR_YELLOW, wwLEDColor.YELLOW},
          {PI.WWBeaconColor.WW_ROBOT_COLOR_GREEN , wwLEDColor.GREEN},
          {PI.WWBeaconColor.WW_ROBOT_COLOR_BLUE  , wwLEDColor.CYAN},
          {PI.WWBeaconColor.WW_ROBOT_COLOR_BLUE2 , wwLEDColor.BLUE},
          {PI.WWBeaconColor.WW_ROBOT_COLOR_PURPLE, wwLEDColor.MAGENTA},
          {PI.WWBeaconColor.WW_ROBOT_COLOR_ORANGE, wwLEDColor.ORANGE},
          {PI.WWBeaconColor.WW_ROBOT_COLOR_RED   , wwLEDColor.RED},
        };
      }
      
      return _beaconToLEDDict;
    }
  }
  
  public static wwLEDColor beaconToLED(PI.WWBeaconColor bc) {
    return wwCollectionUtils.dictSafeAccess<PI.WWBeaconColor, wwLEDColor>(BeaconToLEDDict, bc, wwLEDColor.BLACK, true);
  }
  
  private static Dictionary<wwLEDColor, PI.WWBeaconColor> _LEDToBeaconDict = null;
  private static Dictionary<wwLEDColor, PI.WWBeaconColor> LEDToBeaconDict {
    get {
      if (_LEDToBeaconDict == null) {
        _LEDToBeaconDict = wwCollectionUtils.invertDictionary<PI.WWBeaconColor, wwLEDColor>(BeaconToLEDDict);
      }
      
      return _LEDToBeaconDict;
    }
  }
  
  public static PI.WWBeaconColor LEDToBeacon(wwLEDColor lc) {
    return wwCollectionUtils.dictSafeAccess<wwLEDColor, PI.WWBeaconColor>(LEDToBeaconDict, lc, PI.WWBeaconColor.WW_ROBOT_COLOR_INVALID, true);
  }

  public static bool test() {
    bool passed = true;
    
    passed &= wwLEDColors.beaconToLED(PI.WWBeaconColor.WW_ROBOT_COLOR_GREEN) == wwLEDColor.GREEN;
    passed &= wwLEDColors.LEDToBeacon(wwLEDColor.ORANGE) == PI.WWBeaconColor.WW_ROBOT_COLOR_ORANGE;
    
    if (!passed) {
      WWLog.logError("tests failed");
    }
    
    return passed;
  }
}
