using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

namespace Turing{
  public class trMissionEvaluationController: MonoBehaviour{
    public trProtoController ProtoCtrl;

    private List<GameObject> ShownErrorObjs = new List<GameObject>();
    private bool isIngredientsWrong = false;
    private trStateMachineEvaluation trSME;
    private trState curState = null;

    public const float ERROR_SHOW_TIME = 2.0f;
   
    private List<GameObject> ErrorObjPool = new List<GameObject>();

    public GameObject ErroRingPrefab;
    public Transform ErrorObjParent;

    public int IncorrectRuns = 0;
    public int TotalRuns = 0;

    private bool isFinishMission = false;
    private bool isEnd = false;

    private Dictionary<trState, int> StateRunningCountDic = new Dictionary<trState, int>();

    private float timeStarted = 0;
    public int SecondsSpent{
      get{
        return (int)(Time.fixedTime - timeStarted);
      }
    }


    void Start(){
      if(!trDataManager.Instance.IsInNormalMissionMode
         ||trDataManager.Instance.MissionMng.UserOverallProgress.IsCurMissionCompleted){
        this.gameObject.SetActive(false);
        return;
      }
      ProtoCtrl.RunButtonClickedListener += onRunStop;
      timeStarted = Time.fixedTime;
    }



    void onRunStop(bool isRunning){
      if(trDataManager.Instance.IsInNormalMissionMode){
        StateRunningCountDic.Clear();
        foreach(trState state in trDataManager.Instance.GetCurProgram().UUIDToStateTable.Values){
          StateRunningCountDic.Add(state, 0);
        }
        isFinishMission = false;
        curState = null;

        if(isRunning){
          new trTelemetryEvent(trTelemetryEventType.CHAL_SM_START, true)
            .add(trTelemetryParamType.CHALLENGE, trDataManager.Instance.MissionMng.GetCurMission().UserFacingName)
            .add(trTelemetryParamType.STEP, trDataManager.Instance.MissionMng.UserOverallProgress.CurMissionInfo.PuzzleIndex)
            .emit();

          bool isCorrect = showStateMachineEvaluationInfo();
          if(!isCorrect){
            IncorrectRuns ++;
          }
          TotalRuns ++;
          if(isIngredientsWrong){
            for(int i = 0; i< ShownErrorObjs.Count; ++i){
              StartCoroutine(showAndHideAfterSeconds(ShownErrorObjs[i], ERROR_SHOW_TIME));
            }
            ProtoCtrl.toggleRunningState();
          }
        }
//        if(isRunning){
//          setShowErrorInfo(true);
//        }else{
//          setShowErrorInfo(false);
//        }
      }
    }


    void checkMissionFinished(){
      if(trDataManager.Instance.MissionMng.UserOverallProgress.IsCurMissionCompleted){
        return;
      }
      if(!trSME.P1UnmatchedInfo.IsSame){
        return;
      }
   
      if(trDataManager.Instance.IsInNormalMissionMode){

        if(trSME.P1ToP2StateDict.ContainsKey(curState)){
          if(!StateRunningCountDic.ContainsKey(curState)){
            StateRunningCountDic.Add(curState, 1);
          }else{
            StateRunningCountDic[curState] += 1;
          }
        }
        
        bool isComplete = true;
        foreach(trState state in trSME.P1ToP2StateDict.Keys){
          if(!StateRunningCountDic.ContainsKey(state)
             ||StateRunningCountDic[state] < trSME.P1ToP2StateDict[state].ActivationCount){
            isComplete = false;
            break;
          }
        }
        
        if(isComplete){
          isFinishMission = true;
        }
      }

    }

    // this used to be Update(). see TUR-1503.    
    public void onRobotState(){
      if(!trDataManager.Instance.IsInNormalMissionMode || isEnd){
        return;
      }

      if ( isFinishMission) {
        bool isRealFinished = (curState != null) && curState.Behavior.isFinishedForChallengeCompletion(ProtoCtrl.CurRobot);
        isRealFinished = isRealFinished || (curState != trDataManager.Instance.GetCurProgram().StateCurrent);
        if (isRealFinished){
          float delayTime = 2f;
          if(trBehaviorTypeMethods.IsAnimation(curState.Behavior.Type)){
            delayTime = 0.5f;
          }
          StartCoroutine(showPuzzleCompletePanel(delayTime));
          isEnd = true;
        }
        return;
      }   

      if(!ProtoCtrl.IsRunning){
        return;
      }

      if(curState != trDataManager.Instance.GetCurProgram().StateCurrent){ 
        showErrorObjWhenRunning();
        curState = trDataManager.Instance.GetCurProgram().StateCurrent;
        checkMissionFinished();
      }
    }

    IEnumerator showPuzzleCompletePanel(float delayTime){   
      yield return new WaitForSeconds(delayTime);
      CompletePuzzle();
    }

    public void CompletePuzzle(){
      ProtoCtrl.TutorialCtrl.Stop();
      ProtoCtrl.PuzzleInfoPnlCtrl.CompleteStep();
      if(ProtoCtrl.IsRunning){
        ProtoCtrl.toggleRunningState();
      }
      ProtoCtrl.ShowPuzzleCompletePanel();
    }

    void showErrorObjWhenRunning(){
      if(trSME.P1UnmatchedInfo.IsSame){
        return;
      }

      if(trSME.P1UnmatchedInfo.WrongStates != null && trSME.P1UnmatchedInfo.WrongStates.Contains(trDataManager.Instance.GetCurProgram().StateCurrent)){
        GameObject obj = ProtoCtrl.StateEditCtrl.StateToButtonTable[trDataManager.Instance.GetCurProgram().StateCurrent].ErrorImage.gameObject;
        ProtoCtrl.toggleRunningState();
        StartCoroutine(showAndHideAfterSeconds(obj, ERROR_SHOW_TIME));
      }
      else if(trSME.P1UnmatchedInfo.WrongTransitions != null){
        for(int i = 0; i< trSME.P1UnmatchedInfo.WrongTransitions.Count; ++i){
          if((trSME.P1UnmatchedInfo.WrongTransitions[i].StateSource == curState || trSME.P1UnmatchedInfo.WrongTransitions[i].StateSource.IsOmniState)
             &&trSME.P1UnmatchedInfo.WrongTransitions[i].StateTarget ==trDataManager.Instance.GetCurProgram().StateCurrent ){
            trTransition wrongTrans = trSME.P1UnmatchedInfo.WrongTransitions[i];
            GameObject transErrorObj = ProtoCtrl.StateEditCtrl.TransitionToArrowTable[wrongTrans].TransitionToTriggerHolder[wrongTrans].ErrorImage.gameObject;
            ProtoCtrl.toggleRunningState();
            StartCoroutine(showAndHideAfterSeconds(transErrorObj, ERROR_SHOW_TIME));
            break;
          }
        }

      }
    }

    GameObject getErrorObj(){
      if(ErrorObjPool.Count == ShownErrorObjs.Count){
        GameObject obj = Instantiate(ErroRingPrefab) as GameObject;
        ErrorObjPool.Add(obj);
        obj.transform.SetParent(ErrorObjParent, false);
      }
      return ErrorObjPool[ShownErrorObjs.Count];
    }

    IEnumerator showAndHideAfterSeconds(GameObject obj, float seconds){
      ProtoCtrl.PlayFailureAnimation();
      obj.SetActive(true);
      yield return new WaitForSeconds(seconds);
      obj.SetActive(false);
    }

    bool showStateMachineEvaluationInfo(){
      curState = null;
      ShownErrorObjs.Clear();
      isIngredientsWrong = false;
      trSME = new trStateMachineEvaluation();
      trProgram p1 = ProtoCtrl.CurProgram;
      trProgram p2 = trDataManager.Instance.MissionMng.GetTargetProgram();
      bool isSame = trSME.IsEquivalent(p1, p2);

      if(!ProtoCtrl.PuzzleInfoPnlCtrl.IsIngredientsRight){
        ProtoCtrl.PuzzleInfoPnlCtrl.DisplayErrorTooltip();
//        foreach(trIngredientInfoController ctrl in ProtoCtrl.PuzzleInfoPnlCtrl.WrongElements){
//          GameObject errorObj = getErrorObj();
//          errorObj.transform.position = ctrl.gameObject.transform.position;
//          ShownErrorObjs.Add(errorObj);
//          isIngredientsWrong = true;
//          trSME.P1UnmatchedInfo.IsSame = false;
//          return false;
//        }
        isIngredientsWrong = true;
        trSME.P1UnmatchedInfo.IsSame = false;
        return false;
      }
      //Debug.LogError(trSME.P1UnmatchedInfo.ToString());
//      if(!isSame){
//        if(trSME.P1UnmatchedInfo.WrongStates != null){
//          GameObject stateErrorObj = ProtoCtrl.StateEditCtrl.StateToButtonTable[trSME.P1UnmatchedInfo.WrongStates].ErrorImage.gameObject;
//          ShownErrorObjs.Add(stateErrorObj);
//        }
//        else if(trSME.P1UnmatchedInfo.WrongTransitions != null){
//          trTransition wrongTrans = trSME.P1UnmatchedInfo.WrongTransitions;
//          GameObject transErrorObj = ProtoCtrl.StateEditCtrl.TransitionToArrowTable[wrongTrans].TransitionToTriggerHolder[wrongTrans].ErrorImage.gameObject;
//          ShownErrorObjs.Add(transErrorObj);
//        }
//      }
      
      return isSame;  
    }
  	
  }


}
