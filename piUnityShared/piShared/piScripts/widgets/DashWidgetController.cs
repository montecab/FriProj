using UnityEngine;
using UnityEngine.UI;

namespace WW {

  public class DashWidgetController : MonoBehaviour {
    public Transform trnBodyTheta;
    public Transform trnBodyForward;
    public Transform trnHead;
    public Graphic   bodyImage;
    public Graphic   headUp;
    public Graphic   headLevel;
    public Graphic   headDown;
    public Button    buttonTare;

    private float bodyForward = 0f;
    private float bodyRadians;
    private float headPanRadians;
    private float headTiltRadians;

    private float bodyForwardPrev;

    private const  float cTiltLevelMin  = -10f * Mathf.Deg2Rad;
    private const  float cTiltLevelMax  =  10f * Mathf.Deg2Rad;
    private static Color cDisabledColor = new Color(1f, 1f, 1f, 0.25f);

    void Start() {
      piBotComponentMotorWheel.speedFilteredMax = 20f;
    }

    public float BodyForward {
      get {
        return bodyForward;
      }
      set {
        bodyForward = value;
        trnBodyForward.localPosition = Vector3.right * bodyForward;
      }
    }

    public float BodyRadians {
      get {
        return bodyRadians;
      }

      set {
        bodyRadians = value;
        trnBodyTheta.localEulerAngles = Vector3.forward * Mathf.Rad2Deg * bodyRadians;
      }
    }

    public float HeadPanRadians {
      get {
        return headPanRadians;
      }

      set {
        headPanRadians = value;
        trnHead.localEulerAngles = new Vector3(0f, 0f, Mathf.Rad2Deg * headPanRadians);
      }
    }

    public float HeadTiltRadians {
      get {
        return headTiltRadians;
      }

      set {
        headTiltRadians = value;

        headUp   .enabled = false;
        headLevel.enabled = false;
        headDown .enabled = false;

        if (headTiltRadians < cTiltLevelMin) {
          headUp.enabled = true;
        }
        else if (headTiltRadians > cTiltLevelMax) {
          headDown.enabled = true;
        }
        else {
          headLevel.enabled = true;
        }
      }
    }

    private void setColor(Color c) {
      bodyImage.color = c;
      headUp   .color = c;
      headLevel.color = c;
      headDown .color = c;
    }

    public void update(piBotBase robot) {
      if (robot == null || (robot.robotType != piRobotType.DASH)) {
        setColor(cDisabledColor);
        return;
      }

      piBotBo bot = (piBotBo)robot;

      setColor(Color.white);


      BodyRadians     = bot.BodyPoseSensor.radians;
      BodyForward     = bot.LinearSpeedFiltered * 0.8f;
      HeadPanRadians  = bot.HeadPanSensor .angle.valTarget;
      HeadTiltRadians = bot.HeadTiltSensor.angle.valTarget;
    }
  }
}