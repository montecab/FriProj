using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace Turing{
  public class trRCDotMagic8BallController : MonoBehaviour {
    
    public Button AskButton;
    public TextMeshProUGUI ResultText;

    public struct Option{
      public string soundName;
      public string userFacingName;
     
      public Option(string sound, string name){
        soundName = sound;
        userFacingName = name;
      }
    }

    private Option[] options;

    void Start(){
      options = new Option[]{
        new Option("WITHMAYBE", wwLoca.Format("@!@Maybe@!@")),
        new Option("WITHYES", wwLoca.Format("@!@Yes@!@")),
        new Option("WITHNO", wwLoca.Format("@!@No@!@")),
        new Option("NOTSURTHIS", wwLoca.Format("@!@Not Sure@!@")),
        new Option("WHEE_DOT", wwLoca.Format("@!@Whee!@!@")),
        new Option("NICE_ONE", wwLoca.Format("@!@Nice One!@!@")),
        new Option("YES_DOT", wwLoca.Format("@!@Yes!@!@")),
        new Option("NOTCOMPUTE", wwLoca.Format("@!@Not Compute@!@"))

      };
      ResultText.text = wwLoca.Format("@!@Ask me@!@");
      AskButton.onClick.AddListener(onAskButtonClick);
    }

    private const float cTextDelaySeconds = 1f;

    void onAskButtonClick(){
      trCurRobotController.Instance.CheckOpenChrome();
      piBotCommon robot = (piBotCommon)trCurRobotController.Instance.CurRobot;
      if(robot == null){
        return;
      }
      int random = Random.Range(0, options.Length);
      ResultText.text = options[random].userFacingName;
      ResultText.gameObject.transform.localScale = Vector3.zero;

      StartCoroutine(onScreenAnswer());  
      StartCoroutine(robotReaction(random));
    }

    IEnumerator onScreenAnswer() {
      yield return new WaitForSeconds(cTextDelaySeconds);
      ResultText.gameObject.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
      SoundManager.soundManager.PlaySound(SoundManager.trAppSound.RC_DOT_8_BALL_ANSWER);
    }

    IEnumerator robotReaction(int random){
      piBotCommon robot = (piBotCommon)trCurRobotController.Instance.CurRobot;
      if(robot != null){
        robot.cmd_playSound(options[random].soundName);
        robot.cmd_eyeRing(1.0, "swirl", piMathUtil.deserializeBoolArray(0));      
      }
      bool hasFinished = false;
      bool hasStartedSound = false;

      while(robot!= null && !hasFinished){
        if (!hasStartedSound) {
          hasFinished = false;
          hasStartedSound = robot.SoundPlayingSensor.flag;
        }
        else {
          hasFinished = !robot.SoundPlayingSensor.flag;
        }
        yield return null;
      }

      if(robot != null){
        robot.cmd_eyeRing(1.0, "", piMathUtil.deserializeBoolArray(0));      
      }
    }
  }
}

