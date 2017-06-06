using UnityEngine;
using UnityEngine.UI;
using PI;

[RequireComponent(typeof(Button))]
public class AspectButton : MonoBehaviour {

  public int width;
  public int height;
  public AspectRatioFitter fitter;

  void Start () {
    if (fitter == null) {
      WWLog.logError("no fitter. turning off.");
      gameObject.SetActive(false);
    }

    GetComponent<Button>().onClick.AddListener(onClick);
  }

  void onClick() {
    assertAspect();
  }

  void assertAspect() {
    fitter.aspectRatio = (float)width / (float)height;
  }
}
