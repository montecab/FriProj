using PI;

public class piBotComponentBattery : piBotComponentBase {

  public enum Level {
    WW_BATTERY_LEVEL_CRITICAL = 0,
    WW_BATTERY_LEVEL_LOW      = 1,
    WW_BATTERY_LEVEL_WARNING  = 2,
    WW_BATTERY_LEVEL_NORMAL   = 3,
  }

  public bool  charging = false;
  public Level level    = Level.WW_BATTERY_LEVEL_NORMAL;
  public float volts    = 5f;

  // unused abstract boilerplate, left over from simulated bot days
  public override void tick(float dt) {}  
  public override void handleCommand(WW.SimpleJSON.JSONClass jsComponent) {}
  public override WW.SimpleJSON.JSONClass SensorState { get { return null; } }

  public override void handleState(WW.SimpleJSON.JSONClass jsComponent) {
    charging =         jsComponent[piJSONTokens.WW_SENSOR_VALUE_BATTERY_CHARGING].AsBool;
    level    = (Level)(jsComponent[piJSONTokens.WW_SENSOR_VALUE_BATTERY_LEVEL   ].AsInt);
    volts    =        (jsComponent[piJSONTokens.WW_SENSOR_VALUE_BATTERY_VOLTAGE ].AsFloat) / 1000f;
  }

  public override string ToString() {
    return "charging: " + charging + " level: " + level.ToString() + " voltage: " + volts.ToString("0.00") + "v";
  }
}
