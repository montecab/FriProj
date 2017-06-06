using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/*
    attach this script to a GameObject with a Graphic component (eg, image, text),
    and choose a robot LED color value from the drop-down.
*/

[RequireComponent(typeof(Graphic))]
public class wwRobotColorizer : MonoBehaviour {

  public Graphic    target;
  public wwLEDColor color;
  
  void update() {
    if (target == null) {
      WWLog.logError("no target");
    }
    else {
      float a = target.color.a;
      Color c = wwLEDColors.UnityColorForScreen(color);
      c.a = a;
      target.color = c;
    }
  }

  void Start () {
    update();
  }
  
  void OnValidate() {
    update();
  }
}
