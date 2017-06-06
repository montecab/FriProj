using PI;

public class piBotComponentBase2 : piBotComponentBase {
  // unused abstract boilerplate, left over from simulated bot days
  public override void tick(float dt) {}  
  public override void handleCommand(WW.SimpleJSON.JSONClass jsComponent) {}
  public override WW.SimpleJSON.JSONClass SensorState { get { return null; } }

  public override void handleState(WW.SimpleJSON.JSONClass jsComponent) {}

//  public override string ToString() {return "";}
}
