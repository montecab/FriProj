using UnityEngine;
using UnityEngine.UI;

/*
Simple behavior that can be attached to a (square) UI item
and which will cause it to apear to roll back and forth when moved along X.
Like a wheel on a bicycle. 
*/

public class wwGUIRoller : MonoBehaviour {
  public float radius = -1;

  private RectTransform rt;
  private Vector3 prevPos;

  void Start() {
    rt = GetComponent<RectTransform>();
    prevPos = rt.localPosition;
  }

  void Update() {
    float r = radius;
    if (r <= 0) {
      r = rt.GetWidth() / 2f;
    }

    float dX = rt.localPosition.x - prevPos.x;

    float dTheta = dX / r * 360f / 4f;
    if (Mathf.Abs(dX) > 0.1f) {
      rt.localEulerAngles += dTheta * Vector3.back;
      prevPos = rt.localPosition;
    }
  }
}
