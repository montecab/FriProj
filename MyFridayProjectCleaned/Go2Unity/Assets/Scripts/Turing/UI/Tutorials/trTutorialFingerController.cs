using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using DG.Tweening;

namespace Turing{
  public class trTutorialFingerController : MonoBehaviour {
    public Image FingerObj;
    public GameObject TriggerPanel;
    public CanvasGroup FingerGroup;

    private enum AnimState{
      DRAG, CLICK, NONE
    }
    private AnimState animState = AnimState.NONE;

 
    public trProtoController ProtoCtrl;

    private bool isStop = false;
   
    private trTutorialErrorInfo curErrorInfo;
    private GameObject emptyObj;

    private trHint curHint;

    private Transform lastDragFromPos;
    private Transform lastDragToPos;
    private Transform lastClickPos;
    private float lockTimer = LOCK_TIME; // Allow some thinking time
   
    private const float LOCK_TIME = 3f;
    private const float DESCRIPTION_TIME = 3.0f;
    private bool force = false;

    public enum TutorialState{
      NONE,
      MISSING_STATE_DRAG_BEHAVIOR,
      CHANGE_BEHAVIOR_DRAG_BEHAVIOR,
      WRONG_PARA_STATE,
      REDUNDANT_STATE,
      WRONG_TRANSITION,
      MISSING_TRANSITION,
      REDUNDANT_TRANSITION,
      RUN_BUTTON

    }

    private TutorialState uiState = TutorialState.NONE;

    public Image TriggerImage;

    
    void Start(){
      if(!isTutorialChallenge()){
        return;
      }
      trDataManager.Instance.OnSaveCurProgram += saveProgram;
      ProtoCtrl.RunButtonClickedListener += checkStopTutorial;
    }

    void OnDestroy(){
      if(!isTutorialChallenge()){
        return;
      }
      if(trDataManager.Instance != null){
        trDataManager.Instance.OnSaveCurProgram -= saveProgram;
      }     
    }

		private bool isTutorialChallenge(){
      if(trDataManager.Instance == null){
        return false;
      }
      if(trDataManager.Instance.MissionMng.AuthoringMissionInfo.EditState != MissionEditState.NORMAL
         ||!trDataManager.Instance.IsMissionMode
         || trDataManager.Instance.MissionMng.UserOverallProgress.IsCurMissionCompleted
         ||!trDataManager.Instance.MissionMng.AuthoringMissionInfo.CurMission.IsTutorialMission){
        return false;
      }
      return true;
		}

    public void FirstTimeCheckTutorialProgress(){
      StartCoroutine(delayCheck(LOCK_TIME));
    }

    private IEnumerator delayCheck(float seconds){
      yield return new WaitForSeconds(seconds);
      CheckTutorialProgress(false);
    }


    private void Update(){
      if(!isTutorialChallenge() || isStop){
        return;
      }
      if(ProtoCtrl == null){
        return;
      }

      if(force){
        startShowTut();
        force = false;
      }

      if(!ProtoCtrl.IsRunning){       
        if( lockTimer< LOCK_TIME){
          lockTimer += Time.deltaTime;
          if(lockTimer > LOCK_TIME){
            CheckTutorialProgress(false);
          }
        }

        if(curErrorInfo != null && curErrorInfo.MissingState != null){
          bool isTabRight = ProtoCtrl.BehaviorPanelCtrl.IsTabWithBehaviorActivated(curErrorInfo.MissingState.Behavior);
          if(!isTabRight){
            if(uiState == TutorialState.MISSING_STATE_DRAG_BEHAVIOR || uiState == TutorialState.CHANGE_BEHAVIOR_DRAG_BEHAVIOR){
              ProtoCtrl.BehaviorPanelCtrl.ActivateTabWithBehavior(curErrorInfo.MissingState.Behavior);
            }
          }
        }
      }

    }

    public void Stop(){
      StopFingerAnim();
      isStop = true;
    }

    private void checkStopTutorial(bool isRunning ){
      if(!isTutorialChallenge()){
        return;
      }   

      if(isRunning){
        reset();
      }else{
        CheckTutorialProgress(false, true);
      }
    } 

    private void reset(){
      StopFingerAnim();
      StopAllCoroutines();
    }

    private void saveProgram(){
      CheckTutorialProgress(true);
    }

    public void CheckFTUEProgress(){
      if(!isTutorialChallenge() || ProtoCtrl.IsRunning || isStop){
        return;
      }
      trTutorialErrorInfo errorInfo = getCurrentErrorInfo();
      if(!errorInfo.Equals(curErrorInfo)){
        StopFingerAnim ();
        curErrorInfo = errorInfo;
        startShowTut();
      }
    }

    private void CheckTutorialProgress(bool isSaveProgram , bool force = false){
      if(!isTutorialChallenge() ||ProtoCtrl.IsRunning||isStop){
        return;
      }

      if(isSaveProgram && lockTimer < LOCK_TIME){
        return;
      }

      trTutorialErrorInfo errorInfo = getCurrentErrorInfo();

      if(!errorInfo.Equals(curErrorInfo)){
        StopFingerAnim ();
        if(isSaveProgram){
          lockTimer = 0;
          return;
        }
        curErrorInfo = errorInfo;
        startShowTut();
      }
      else if(force){
        startShowTut();
      }
    }

    public bool IsReadyToRun {
      get {
        if(!isTutorialChallenge() ||curErrorInfo == null){
          return false;
        }
        return (curErrorInfo.HintId == -1);
      }
    }

    private void startShowTut(){
      if(IsReadyToRun){
        uiState = TutorialState.RUN_BUTTON;
        startClickStartButtonTut();
        return;
      } else {     
        curHint = trDataManager.Instance.MissionMng.GetCurPuzzle().Hints[curErrorInfo.HintId];
      }

      if(curErrorInfo.MissingState != null && curErrorInfo.RedundentState != null){
        startChangeStateBehavTut();
        uiState = TutorialState.MISSING_STATE_DRAG_BEHAVIOR;
        return;
      }

      if(curErrorInfo.MissingState != null){
        startMissingStateTut();
        uiState = TutorialState.CHANGE_BEHAVIOR_DRAG_BEHAVIOR;
        return;
      }

      if(curErrorInfo.WrongParaState != null){
        startChangeStateParaTut();
        uiState = TutorialState.WRONG_PARA_STATE;
        return;
      }
      
      if( curErrorInfo.MissingTransitionTarget != null
         && curErrorInfo.MissingTransitionSource != null){
        startMissingTransitionTut();
        uiState = TutorialState.MISSING_TRANSITION;
        return;
      }

      if(curErrorInfo.RedundentState != null){
        startRedundentStateTut();
        uiState = TutorialState.REDUNDANT_STATE;
        return;
      }

      if(curErrorInfo.WrongTriggerTransition != null){
        startClickWrongTriggerTut();
        uiState = TutorialState.WRONG_TRANSITION;
        return;
      }

      if(curErrorInfo.WrongParaTransition != null){
        startClickWrongTriggerTut();
        uiState = TutorialState.WRONG_TRANSITION;
        return;
      }

      if(curErrorInfo.RedundentTransiton != null){
        startRedundentTransitionTut();
        uiState = TutorialState.REDUNDANT_TRANSITION;
        return;
      }
    }
    
    private trTutorialErrorInfo getCurrentErrorInfo(){
       
      trTutorialErrorInfo result  = new trTutorialErrorInfo();
      trPuzzle curPuzzle = trDataManager.Instance.MissionMng.GetCurPuzzle();
      result.HintId = -1;
      for(int i = 0; i< curPuzzle.Hints.Count; ++i){
        trTutorialErrorInfo error = new trTutorialErrorInfo();
        getNextTutorialElement(curPuzzle.Hints[i], ref error);
        if(error.IsEmpty){
          result.HintId = i +1;
        }
      }

      if(result.HintId == -1){
        result.HintId = curPuzzle.Hints.Count - 1;
      }
      else if(result.HintId == curPuzzle.Hints.Count){
        result.HintId = -1;
        return result;
      }


      getNextTutorialElement(curPuzzle.Hints[result.HintId], ref result);
      return result;

    }

    private void startDragTriggerParaSliderTut(){
      Transform sliderHandle;
      sliderHandle = ((trParaSliderController)(ProtoCtrl.TriggerConfigurePanel.TriggerParaCtrl.CurParaPanel)).MSlider.handleRect.transform;

      if(emptyObj == null){
        emptyObj = new GameObject();
      }
      emptyObj.transform.position = sliderHandle.transform.position + new Vector3(20.0f, 0, 0);
      StartDrag(sliderHandle, emptyObj.transform);
    }

    private void startClickStartButtonTut(){
      Transform pos = ProtoCtrl.RunButton.gameObject.transform;
      ProtoCtrl.playButtonIndicator.SetActive(true);
      StartClick(pos, 250f);
    }

    private void startClickWrongTriggerTut(){
      if(curErrorInfo.WrongTriggerTransition != null && ProtoCtrl.StateEditCtrl.TransitionToArrowTable.ContainsKey(curErrorInfo.WrongTriggerTransition)){
        StartClick(ProtoCtrl.StateEditCtrl.TransitionToArrowTable[curErrorInfo.WrongTriggerTransition].TransitionToTriggerHolder[curErrorInfo.WrongTriggerTransition].TriggerButton.transform);
      }
      else if(curErrorInfo.WrongParaTransition != null && ProtoCtrl.StateEditCtrl.TransitionToArrowTable.ContainsKey(curErrorInfo.WrongParaTransition)){
        StartClick(ProtoCtrl.StateEditCtrl.TransitionToArrowTable[curErrorInfo.WrongParaTransition].TransitionToTriggerHolder[curErrorInfo.WrongParaTransition].TriggerButton.transform);
      }
    }

    private void startMissingTransitionTut(){
      if (ProtoCtrl.StateEditCtrl.StateToButtonTable.ContainsKey(curErrorInfo.MissingTransitionSource) && 
          ProtoCtrl.StateEditCtrl.StateToButtonTable.ContainsKey(curErrorInfo.MissingTransitionTarget)){
        Transform fromPos = ProtoCtrl.StateEditCtrl.StateToButtonTable[curErrorInfo.MissingTransitionSource].TransitionContainer.gameObject.transform;
        Transform toPos = ProtoCtrl.StateEditCtrl.StateToButtonTable[curErrorInfo.MissingTransitionTarget].gameObject.transform;
        StartDrag(fromPos, toPos);
      }
    }

    private void startRedundentTransitionTut(){
      Transform fromPos = ProtoCtrl.StateEditCtrl.TransitionToArrowTable[curErrorInfo.RedundentTransiton].TransitionToTriggerHolder[curErrorInfo.RedundentTransiton].BehaviourImage.gameObject.transform;
      StartDrag(fromPos,ProtoCtrl.TrashCanCtrl.gameObject.transform);
    }

    private void startRedundentStateTut(){
      Transform fromPos = ProtoCtrl.StateEditCtrl.StateToButtonTable[curErrorInfo.RedundentState].gameObject.transform;
      StartDrag(fromPos, ProtoCtrl.TrashCanCtrl.gameObject.transform);
    }

    private void startChangeStateBehavTut(){
      Transform fromPos = null;

      if(!ProtoCtrl.BehaviorPanelCtrl.IsTabWithBehaviorActivated(curErrorInfo.MissingState.Behavior)){
        ProtoCtrl.BehaviorPanelCtrl.ActivateTabWithBehavior(curErrorInfo.MissingState.Behavior);
      }

      foreach(trBehavior beh in ProtoCtrl.BehaviorPanelCtrl.BehaviorToButtonDic.Keys){
        if(curErrorInfo.MissingState.Behavior.UUID == beh.UUID){
          fromPos = ProtoCtrl.BehaviorPanelCtrl.BehaviorToButtonDic[beh].gameObject.transform;
        }
      }
      if(fromPos == null){
        WWLog.logError("Cannot find beahvior "+ curErrorInfo.MissingState.Behavior.Type.ToString() +  " in behavior panel ");
        return;
      }
      
      Transform toPos = ProtoCtrl.StateEditCtrl.StateToButtonTable[curErrorInfo.RedundentState].gameObject.transform;
      StartDrag(fromPos,toPos);
    }

    private void startChangeStateParaTut(){
      Transform pos = ProtoCtrl.StateEditCtrl.StateToButtonTable[curErrorInfo.WrongParaState].gameObject.transform;
      StartClick(pos);
    }

    private void startMissingStateTut(){
      Transform fromPos = null;
      //if current tab is not the tab of missing state
      if(!ProtoCtrl.BehaviorPanelCtrl.IsTabWithBehaviorActivated(curErrorInfo.MissingState.Behavior)){
        ProtoCtrl.BehaviorPanelCtrl.ActivateTabWithBehavior(curErrorInfo.MissingState.Behavior);
        force = true; // force showing tut in the next frame so the layout of behaviors are done at that point
        return;
      }

      foreach(trBehavior beh in ProtoCtrl.BehaviorPanelCtrl.BehaviorToButtonDic.Keys){
        if(curErrorInfo.MissingState.Behavior.UUID == beh.UUID){
          fromPos = ProtoCtrl.BehaviorPanelCtrl.BehaviorToButtonDic[beh].gameObject.transform;
        }
      }
      if(fromPos == null){
        WWLog.logError("Cannot find beahvior "+ curErrorInfo.MissingState.Behavior.Type.ToString() +  " in behavior panel ");
        return;
      }
      
      if(emptyObj == null){
        emptyObj = new GameObject();
      }
      emptyObj.transform.SetParent(ProtoCtrl.StateEditCtrl.StatePanel.transform, false);
      Vector2 relPosition = curErrorInfo.MissingState.LayoutPosition - curHint.Program.StateStart.LayoutPosition; // the position relative to start state
      Vector2 localPos = trDataManager.Instance.GetCurProgram().StateStart.LayoutPosition + relPosition;
      emptyObj.transform.localPosition = new Vector3(localPos.x, localPos.y, 0);
      StartDrag(fromPos, emptyObj.transform);
    }


    private void getNextTutorialElement(trHint hint, ref trTutorialErrorInfo errorinfo){
     
      List<trState> noMatchTargetStates = new List<trState>();
      List<trState> noMatchSourceStates = new List<trState>();

      Dictionary<trState, trState> matchStates = new Dictionary<trState, trState>();
      
      foreach(trState targetState in hint.Program.UUIDToStateTable.Values){
        noMatchTargetStates.Add(targetState);
      }
      
      foreach(trState sourceState in trDataManager.Instance.GetCurProgram().UUIDToStateTable.Values){
        noMatchSourceStates.Add(sourceState);
      }

      foreach(trState s1 in hint.Program.UUIDToStateTable.Values){
        foreach(trState s2 in trDataManager.Instance.GetCurProgram().UUIDToStateTable.Values){
          if(!noMatchSourceStates.Contains(s2)){
            continue;
          }
          bool tmpParaSame = false;
          if(!s2.IsSimilarTo(s1, ref tmpParaSame)){
            continue;
          }

          noMatchSourceStates.Remove(s2);
          noMatchTargetStates.Remove(s1);

          matchStates.Add(s1, s2);
          break;
        }
      }

      if(noMatchTargetStates.Count == 0 && noMatchSourceStates.Count >0){
        errorinfo.RedundentState = noMatchSourceStates[0];
        return;
      }
      
      if(noMatchTargetStates.Count>0 && noMatchSourceStates.Count == 0){
        errorinfo.MissingState = noMatchTargetStates[0];
        return;
      }
      
      if(noMatchTargetStates.Count>0 && noMatchSourceStates.Count > 0){
        trState state = noMatchTargetStates[0];

        for(int j = 0; j<noMatchSourceStates.Count; ++j){
          if(state.Behavior.UUID == noMatchSourceStates[j].Behavior.UUID){
            errorinfo.WrongParaState = noMatchSourceStates[j];
            return;
          }
        }
        
        errorinfo.MissingState = state;
        errorinfo.RedundentState = noMatchSourceStates[0];
      }

      // check if we need to change the state's behavior
      // if find a redundent state in source state machine 
      // which has the missing target state's transitioins,
      // then we just need to change the state's behavior
      if(errorinfo.MissingState != null && errorinfo.RedundentState != null){ 
        if(errorinfo.MissingState.OutgoingTransitions.Count != errorinfo.RedundentState.OutgoingTransitions.Count){ 
          errorinfo.MissingState = null;
        }else{
          HashSet<trTransition> tranHash = new HashSet<trTransition>();
          foreach(trTransition targetTran in errorinfo.MissingState.OutgoingTransitions){
            bool isFound = false;
            foreach(trTransition sourceTran in errorinfo.RedundentState.OutgoingTransitions){
              if(tranHash.Contains(sourceTran)){
                continue;
              }
              if(sourceTran.Trigger.Type == targetTran.Trigger.Type
                 && sourceTran.StateTarget.Behavior.UUID == targetTran.StateTarget.Behavior.UUID){
               
                  isFound = true;
                  tranHash.Add(sourceTran);
                  break;
                                
              }
            }
            if(!isFound){
              errorinfo.MissingState = null;
            }
            
          }
        }
      }

      if(errorinfo.MissingState != null || errorinfo.RedundentState != null){
        return;
      }

      List<trTransition> noMatchTargetTrans = new List<trTransition>();
      List<trTransition> noMatchSourceTrans = new List<trTransition>();

      foreach(trTransition targetTran in hint.Program.UUIDToTransitionTable.Values){
        noMatchTargetTrans.Add(targetTran);
      }

      foreach(trTransition sourceTran in trDataManager.Instance.GetCurProgram().UUIDToTransitionTable.Values){
        noMatchSourceTrans.Add(sourceTran);
      }

      foreach(trTransition targetTran in hint.Program.UUIDToTransitionTable.Values){
        foreach(trTransition sourceTran in trDataManager.Instance.GetCurProgram().UUIDToTransitionTable.Values){
          if(!noMatchSourceTrans.Contains(sourceTran)){
            continue;
          }

          if(matchStates[targetTran.StateSource] == sourceTran.StateSource
             && matchStates[targetTran.StateTarget] == sourceTran.StateTarget
             &&targetTran.Trigger.Type == sourceTran.Trigger.Type){
            bool isFound = true;
            if(trTrigger.Parameterized(sourceTran.Trigger.Type)){
              if( !piMathUtil.withinSpecifiedEpsilon(sourceTran.Trigger.NormalizedValue, targetTran.Trigger.NormalizedValue, 0.1f)){
                isFound = false;
              }
            }
            if(isFound){
              noMatchSourceTrans.Remove(sourceTran);
              noMatchTargetTrans.Remove(targetTran);
              break;
            }
          }
        }
      }

      if(noMatchTargetTrans.Count == 0 && noMatchSourceTrans.Count >0){
        errorinfo.RedundentTransiton = noMatchSourceTrans[0];
        return;
      }

      if(noMatchTargetTrans.Count>0 && noMatchSourceTrans.Count == 0){
        errorinfo.MissingTransitionSource = matchStates[noMatchTargetTrans[0].StateSource];
        errorinfo.MissingTransitionTarget = matchStates[noMatchTargetTrans[0].StateTarget];
        return;
      }

      if(noMatchTargetTrans.Count>0 && noMatchSourceTrans.Count > 0){
        for(int i = 0; i<noMatchTargetTrans.Count; ++i){
          for(int j = 0; j<noMatchSourceTrans.Count; ++j){
            if(matchStates[noMatchTargetTrans[i].StateSource] == noMatchSourceTrans[j].StateSource
               && matchStates[noMatchTargetTrans[i].StateTarget] == noMatchSourceTrans[j].StateTarget){
              if(noMatchTargetTrans[i].Trigger.Type != noMatchSourceTrans[j].Trigger.Type){
                errorinfo.WrongTriggerTransition = noMatchSourceTrans[j];
              }
              else{
                errorinfo.WrongParaTransition = noMatchSourceTrans[j];
              }
              return;
            }
          }

          errorinfo.RedundentTransiton = noMatchSourceTrans[0];
        }
        return;
      }

    }   
   

    private void StopFingerAnim(){
      uiState = TutorialState.NONE;
      if(FingerObj == null){
        return;
      }
      animState = AnimState.NONE;
      StopCoroutine("fingerClick");
      StopCoroutine("fingerDrag");
      FingerGroup.alpha = 1;
      FingerObj.transform.DOKill();
      FingerObj.gameObject.SetActive(false);
      ProtoCtrl.playButtonIndicator.SetActive(false);
    }

    private void StartClick(Transform pos, float rotateAngle = 0f){
      if(pos == null){
        return;
      }

      FingerObj.transform.DOKill();
      FingerObj.gameObject.SetActive(true);
      FingerObj.rectTransform.localRotation = Quaternion.Euler(new Vector3(0, 0, rotateAngle));

      animState = AnimState.CLICK;
      lastClickPos = pos;
      StartCoroutine(fingerClick(pos));
    }

    IEnumerator fingerClick(Transform pos){

      if(animState != AnimState.CLICK){
        yield break;
      }

      if(pos != lastClickPos){
        yield break;
      }
      
      if(!this.gameObject.activeSelf){
        yield break;
      }

      FingerObj.transform.position = pos.position;
      FingerObj.transform.localScale = Vector3.one * 1.2f;
      FingerObj.transform.DOScale( Vector3.one, 0.5f);
      yield return new WaitForSeconds(0.4f);
      if (!ProtoCtrl.IsAnyModalPanelOpen) {
        // TUR-2201: disable this sound too.  
        //SoundManager.soundManager.PlaySound(SoundManager.trAppSound.TUTORIAL_HAND_BOUNCE);
      }
      yield return new WaitForSeconds(0.4f);
      FingerObj.transform.DOScale( Vector3.one *1.2f, 0.2f);
      yield return new WaitForSeconds(0.2f);
      StartCoroutine(fingerClick(pos));
    }
  
    public void StartDrag(Transform start, Transform end){
      if(start == null || end == null){
        return;
      }
      lastDragFromPos = start;
      lastDragToPos = end;

      FingerObj.transform.DOKill();
      FingerObj.gameObject.SetActive(true);
      animState = AnimState.DRAG;
      StartCoroutine(fingerDrag(start, end));
    }

    IEnumerator fingerDrag(Transform from, Transform to){

      if(animState != AnimState.DRAG){
        yield break;
      }

      if(from != lastDragFromPos || to != lastDragToPos){
        yield break;
      }

      if(!this.gameObject.activeSelf){
        yield break;
      }

      if(from == null || to == null){
        yield break;
      }

      FingerObj.transform.position = from.position;
      FingerObj.transform.localScale = Vector3.one * 1.2f;

      FingerObj.transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack);
      yield return new WaitForSeconds(0.2f);
      if (!ProtoCtrl.IsAnyModalPanelOpen) {
        // TUR-2201: turning off sound for when tutorial hand is sliding, as this is super annoying for challenges.
        //SoundManager.soundManager.PlaySound(SoundManager.trAppSound.TUTORIAL_HAND_SLIDE);
      }
      if(to == null){
        yield break;
      }
      FingerObj.transform.DOMove(to.position, 1.5f).SetEase(Ease.InOutQuart);
      yield return new WaitForSeconds(1.8f);
      FingerObj.transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InQuad);
      yield return new WaitForSeconds(0.2f);
      FingerGroup.alpha = 1;
      StartCoroutine(fingerDrag(from, to));
    }
  }


  public class trTutorialErrorInfo{
    public trState MissingState = null;
    public trState WrongParaState = null;
    public trTransition RedundentTransiton = null;
    public trTransition WrongTriggerTransition = null;
    public trTransition WrongParaTransition = null;
    public trState RedundentState = null;
    public trState MissingTransitionSource = null;
    public trState MissingTransitionTarget = null;
    
    public int HintId;

    public bool IsEmpty{
      get{
        if(this.MissingState != null){
          return false;
        }
        
        if(this.WrongParaState != null){
          return false;
        }
        
        if(this.RedundentTransiton != null){
          return false;
        }
        
        if(this.WrongTriggerTransition != null){
          return false;
        }
        
        if(this.RedundentState != null){
          return false;
        }
        
        if(this.MissingTransitionTarget != null){
          return false;
        }
        
        if(this.MissingTransitionSource != null){
          return false;
        }
        
        if(this.WrongParaTransition != null){
          return false;
        }

        return true;

      }
    }

    public bool Equals(trTutorialErrorInfo info){
      if(info == null){
        return false;
      }

      if(info.HintId != this.HintId){
        return false;
      }

      if(info.MissingState != this.MissingState){
        return false;
      }

      if(info.WrongParaState != this.WrongParaState){
        return false;
      }
      
      if(info.RedundentTransiton != this.RedundentTransiton){
        return false;
      }
      
      if(info.WrongTriggerTransition != this.WrongTriggerTransition){
        return false;
      }
      
      if(info.RedundentState != this.RedundentState){
        return false;
      }

      if(info.MissingTransitionTarget != this.MissingTransitionTarget){
        return false;
      }

      if(info.MissingTransitionSource != this.MissingTransitionSource){
        return false;
      }

      if(info.WrongParaTransition != this.WrongParaTransition){
        return false;
      }

      return true;
    }
  }
}
