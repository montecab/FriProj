using UnityEngine;
using System.Collections;

namespace Turing{
  public class trHintManager : Singleton<trHintManager> {

    private float timer = 0 ;

    private float updateTime = float.NaN;
  	
    private bool isTimerStop = true;

    public const float TINKER_TIME_COUNT = 5.0f;
    public const float HINT_ACTIVATE_TIME = 15.0f;

    public delegate void HintDelegate();
    public HintDelegate OnHint;

  	// Update is called once per frame
  	void FixedUpdate () {
      if(!trDataManager.Instance.IsInNormalMissionMode){
        return;
      }

      if(Time.fixedTime - updateTime > TINKER_TIME_COUNT){
        isTimerStop = true;
      }

      if(!isTimerStop){
        timer += Time.fixedDeltaTime;
        if(timer >HINT_ACTIVATE_TIME){
          activateHint();
          timer = 0;
        }
      }
  	}

    void activateHint(){
      bool isNextHintAvailable = trDataManager.Instance.MissionMng.ActivateNextHint();
      if(isNextHintAvailable && OnHint != null){
        OnHint();
      }
    }

    public void TinkeredProgram(){
      updateTime = Time.fixedTime;
      isTimerStop = false;
    }
  }
}
