using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PoseTestController : MonoBehaviour {


  public Text uiCmd;
    
  private float unitDistLin = 50.0f;
  private float unitDistAng = 90.0f;
  private float unitDistTim =  1.0f;
  
  public void fwd() {
    SetPoseRelative(1.0f, 0.0f, 0.0f, 1.0f);
  }
  
  public void bak() {
    SetPoseRelative(-1.0f, 0.0f, 0.0f, 1.0f);
  }
  
  public void lft() {
    SetPoseRelative(0.0f, 0.0f, 1.0f, 1.0f);
  }

  public void rgt() {
    SetPoseRelative(0.0f, 0.0f, -1.0f, 1.0f);
  }

  public void stp() {
    SetPoseRelative(0.0f, 0.0f, 0.0f, 0.5f);
  }
  
  public void clr() {
    piBotBo bot = piConnectionManager.Instance.AnyConnectedBo;
    if (bot == null) {
      return;
    }
    bot.cmd_rgbLights(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
  }
  
  public void SetPoseRelative(float dX, float dY, float dTheta, float dTime) {
    dX     *= unitDistLin;
    dY     *= unitDistLin;
    dTheta *= unitDistAng;
    dTime  *= unitDistTim;
    
    piBotBo bot = piConnectionManager.Instance.AnyConnectedBo;
    if (bot == null) {
      return;
    }
    
    PI.WWPoseMode      mode = PI.WWPoseMode.WW_POSE_MODE_RELATIVE_MEASURED;
    PI.WWPoseDirection dir  = PI.WWPoseDirection.WW_POSE_DIRECTION_INFERRED;
    PI.WWPoseWrap      wrap = PI.WWPoseWrap.WW_POSE_WRAP_ON;
    
    string floatFmt = "0.00";
    string cmd = "";
    string delim = "\n";
    cmd += delim + "dX:     " + dX    .ToString(floatFmt);
    cmd += delim + "dY:     " + dY    .ToString(floatFmt);
    cmd += delim + "dTheta: " + dTheta.ToString(floatFmt);
    cmd += delim + "dTime:  " + dTime .ToString(floatFmt);
    cmd += delim + "mode:   " + mode  .ToString();
    cmd += delim + "dir:    " + dir   .ToString();
    cmd += delim + "wrap:   " + wrap  .ToString();
    
    uiCmd.text = cmd;
    
    bot.cmd_poseParam(dX, dY, dTheta, dTime, mode, dir, wrap);
  }
}
