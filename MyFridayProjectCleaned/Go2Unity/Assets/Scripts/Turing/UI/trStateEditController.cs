using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Turing;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;

namespace Turing {
  public class trStateEditController : trStateMachinePanelBase {

    public GameObject StateDragPanel;
    public trProtoController ProtoCtrl;
    public trStateMoveListener StateMoveListener;
    public trTransitionButtonMoveListener TransitionButtonMoveListener;

    [SerializeField]
    private GameObject _simpleBehaviorPanelPrefab;
    [SerializeField]
    private Transform _simpleBehaviorPanelHolder;
    private trSimpleBehaviorController _simpleBehaviorPanel;

    private float _lastBumperUpdateTime;
    private float _lastStateMovedTime;

    public enum UIState{
      DELETE,
      CHOOSE_START_STATE,
      CHOOSE_OMNI_STATE,
      STATE_EDIT
    }

    private UIState uiState = UIState.STATE_EDIT;

    public UIState MyUIState{
      set{
        if(uiState == value){
          return;
        }

        switch(value){
        case UIState.DELETE:

          break;
        }
        uiState = value;
      }
      get{
        return uiState;
      }
    }

    public void UpdateStates(){
      foreach(trStateButtonController button in StateToButtonTable.Values){
        if(!ProtoCtrl.CurProgram.UUIDToBehaviorTable.ContainsKey(button.BehaviorData.UUID)){
          if(trDataManager.Instance.AppUserSettings.UuidToBehaviorDic.ContainsValue(button.BehaviorData)){
            WWLog.logWarn("Something is wrong. The program should contain the behavior.");
          }

          button.BehaviorData = trBehavior.GetDefaultBehavior(trBehaviorType.DO_NOTHING);

        }
        button.SetUpView();
      }
      trDataManager.Instance.SaveCurProgram();
    }

    void Start(){
      if(_simpleBehaviorPanel==null){
        GameObject obj = Instantiate(_simpleBehaviorPanelPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        obj.transform.SetParent(_simpleBehaviorPanelHolder, false);
        _simpleBehaviorPanel = obj.GetComponent<trSimpleBehaviorController>();
      }
      _simpleBehaviorPanel.ProtoController = ProtoCtrl;
      StateMoveListener.StateToButtonTable = StateToButtonTable;
      //StateMoveListener.onCreateTransition = CreateTransition;
      TransitionButtonMoveListener.StateToButtonTable = StateToButtonTable;
      TransitionButtonMoveListener.onCreateTransition = CreateTransition;
    }

    void Update() {
      foreach(trStateButtonController trSBC in StateToButtonTable.Values){
        if(trSBC.lastPositionChangeTime > _lastStateMovedTime){
          _lastStateMovedTime = trSBC.lastPositionChangeTime;
        }
      }
    }
    
    // we use FixedUpdate for this because we're basically doing cheap Euler integration on state position,
    // so we want to get a constant movement rate across platforms.
    void FixedUpdate() {
      if (trMultivariate.Instance.getOptionValue(trMultivariate.trAppOption.BUMPER_MODE) == trMultivariate.trAppOptionValue.YES) {
        // don't do bumper stuff for edit modes.
        if (trDataManager.Instance.AuthoringMissionInfo.EditState == MissionEditState.NORMAL) {
          if (_lastStateMovedTime >= _lastBumperUpdateTime) {
            UpdateBumperStates();
          }
        }
      }
    }

    private const float gridSpacing  =  22.5f;
    private const float bumperRadius = 165.0f;
    private bool isPreviousLayoutChanged = false;
    private trStateButtonController previousDraggedStateBtn = null;
    private int notChangingFrameCount = 0;
    private trStateButtonController[] _sbcs        = new trStateButtonController[0];
    private Vector3                [] _localPosOld = new Vector3[0];
    private Vector3                [] _localPosNew = new Vector3[0];
    void UpdateBumperStates() {
      // copy StateToButtonTable.Values into a randomly-accessible array.
      // this is because StateToButtonTable.Values cannot be accesses with [].
      // TODO: how can we avoid doing this ?

      // also, we copy the localPosition into an array and do all our math on the array,
      // and only assign a new localPosition if it's changed significantly.

      int stateNum = StateToButtonTable.Values.Count;

      if (_sbcs.Length != stateNum) {
        _sbcs        = new trStateButtonController[stateNum];
        _localPosOld = new Vector3                [stateNum];
        _localPosNew = new Vector3                [stateNum];
      }

      int q = 0;
      foreach (trStateButtonController trSBC in StateToButtonTable.Values) {
        _sbcs       [q] = trSBC;
        _localPosOld[q] = trSBC.transform.localPosition;
        _localPosNew[q] = _localPosOld[q];
        q += 1;
      }

      bool isChanged = false;
      trStateButtonController draggingState = null;


      if (trMultivariate.isYESorSHOW(trMultivariate.trAppOption.STATE_MACHINE_GRID)) {
        // each state is attracted to grid points
        for (int n = 0; n < stateNum; ++n) {
          trStateButtonController trSBC = _sbcs[n];
          if (!trSBC.IsDraggingState) {
            // find nearest grid point:
            Vector2 gpPos = _localPosNew[n];
            gpPos.x = gridSpacing * Mathf.Round(gpPos.x / gridSpacing);
            gpPos.y = gridSpacing * Mathf.Round(gpPos.y / gridSpacing);

            _localPosNew[n] = Vector2.Lerp(_localPosNew[n], gpPos, 0.2f);
          }
        }
      }


      // each state repells each state
      for (int n = 0; n < _sbcs.Length; ++n) {
        trStateButtonController trSBC1 = _sbcs[n];
        if (!trSBC1.IsDraggingState) {
          for (int m = n + 1; m < _sbcs.Length; ++m) {
            trStateButtonController trSBC2 = _sbcs[m];
            if (!trSBC2.IsDraggingState) {
              Vector3 vs1s2 = _localPosNew[m] - _localPosNew[n];
              float   ds1s2 = vs1s2.magnitude;              
              
              if (ds1s2 < bumperRadius) {
                float foo = Mathf.Lerp(5.0f, 0.1f, ds1s2 / bumperRadius); // this seems to have no effect.
                Vector3 vs1s2N = vs1s2 / ds1s2;
                _localPosNew[n] += vs1s2N * -foo;
                _localPosNew[m] += vs1s2N *  foo;
              }
            }
          }
        }
      }


      for (int n = 0; n < _sbcs.Length; ++n) {
        trStateButtonController trSBC1 = _sbcs[n];

        float sqrMag = (_localPosNew[n] - _localPosOld[n]).sqrMagnitude;
        if (sqrMag > 0.02f) {
          // if we've significantly changes the position, then go ahead and actually assign it to the transform.
          trSBC1.transform.localPosition = _localPosNew[n];
          trSBC1.lastPositionChangeTime = Time.time;
        }

        if (!trSBC1.IsDraggingState) {
          isChanged = isChanged || trSBC1.CheckSaveLayoutPosition();
        }
        else{
          draggingState = trSBC1;
        }
      }

      //Save program after dragging and bumping 
      if(previousDraggedStateBtn!= null&&StateToButtonTable.ContainsValue(previousDraggedStateBtn)  && draggingState == null && isPreviousLayoutChanged ){
        notChangingFrameCount = isChanged ? 0: notChangingFrameCount +1;
        if(notChangingFrameCount > 10){          //if the layout is not changed for 10 frame
          
          UpdateUndoRedoUserAction();
          notChangingFrameCount = 0;
          isPreviousLayoutChanged = false;
          previousDraggedStateBtn = null;
          _lastBumperUpdateTime = Time.time;
          trDataManager.Instance.SaveCurProgram();
        }
      }

      if(!StateToButtonTable.ContainsValue(previousDraggedStateBtn)){
        previousDraggedStateBtn = null;
      }

      if(draggingState != null){
        previousDraggedStateBtn = draggingState;
      }

      if(previousDraggedStateBtn!= null && isChanged){
        isPreviousLayoutChanged = true;
      }

    }

    public void UpdateTriggerWarningShowing(trState state){
      foreach(trTransition trans in state.OutgoingTransitions){
        trTransitionArrowController arrowCtrl = TransitionToArrowTable[trans];
        foreach(trTriggerButtonViewHolder holder in arrowCtrl.TransitionToTriggerHolder.Values){
          holder.SetUpView();
        }
      }
    }

    public override void SetUpView (trProgram program)
    {
      //not centering states for tutorial challenges because they messed up finger setup 
      StatePanel.transform.localPosition = program.ScrollPosition;
      bool isCentered = trDataManager.Instance.IsInFreePlayMode;
      isCentered = isCentered ||(trDataManager.Instance.IsInNormalMissionMode
        && trDataManager.Instance.MissionMng.GetCurPuzzle().IsCenterProgramOnStart);
      if(isCentered){
        StatePanel.transform.localPosition = Vector2.zero;
        program.CenterStatesOnCanvas();
      }
      base.SetUpView (program);
      UpdateUndoRedoUserAction();
    }




    public override void Reset(){
      ProtoCtrl.HideElementInfo();
      base.Reset();
    }
      
    public override void SetRunMode(bool runMode){
      base.SetRunMode(runMode);
      if (runMode) {
        ProtoCtrl.HideElementInfo();
      }
    }

    public void DeleteTransition(trTransitionArrowController arrow, trTransition transition){
      ProtoCtrl.CurProgram.RemoveTransition(transition);
      SoundManager.soundManager.PlaySound(SoundManager.trAppSound.TRASH);
      TransitionToArrowTable.Remove(transition);
      RemoveUnusedElements();
      trDataManager.Instance.SaveCurProgram();
    }

    private void onTriggerButtonClicked(trTransitionArrowController arrow, trTransition transition){
      if (ProtoCtrl.IsRunning){
        if (!trDataManager.Instance.IsMissionMode || trDataManager.Instance.MissionMng.AuthoringMissionInfo.EditState != MissionEditState.NORMAL){
          ProtoCtrl.CurProgram.SetState(transition.StateTarget, ProtoCtrl.CurRobot);
          if (transition.OnTransitionActivated != null){
            transition.OnTransitionActivated();
          }
        }
      } 
      else {
        SoundManager.soundManager.PlaySound(SoundManager.trAppSound.UI_SOUND);
        showTriggerConfigurationPanel(arrow, transition);
      }
    }

    public void UpdateUndoRedoUserAction(){
      if(ProtoCtrl.UndoRedoCtrl != null){
        ProtoCtrl.UndoRedoCtrl.Save(ProtoCtrl.CurProgram);
      }
    }

    public void UpdateUndoRedoView(trProgram target){

      ProtoCtrl.HideElementInfo();
      
      foreach(string uuid in target.UUIDToStateTable.Keys){
        if(CurProgram.UUIDToStateTable.ContainsKey(uuid)){
          trState state = CurProgram.UUIDToStateTable[uuid];
          bool isSameBeh = state.Behavior.UUID == target.UUIDToStateTable[uuid].Behavior.UUID;         
          if(!isSameBeh){
            CurProgram.setStateBehaviour(state, target.UUIDToStateTable[uuid].Behavior);
          }
          bool isParaDiff = state.IsBehaviorParameterDifferent(target.UUIDToStateTable[uuid]); // order matters!
          state.Copy(target.UUIDToStateTable[uuid]);
          StateToButtonTable[state].SetUpView();
          StateToButtonTable[state].UpdateLayoutPos();
          if(isParaDiff){
            StateToButtonTable[state].PopLabel();
          }
        }
        else{
          trState s = new trState();
          trState st = target.UUIDToStateTable[uuid];
          s.UUID = st.UUID;
          s.Copy(st);
          CurProgram.AddState(s);
          CreateStateButton(s);
        }
      }

      // avoiding modifying dictionary when iterating through it
      string[] uuids = new string[CurProgram.UUIDToStateTable.Count];
      CurProgram.UUIDToStateTable.Keys.CopyTo(uuids, 0); 
      foreach(string uuid in uuids){
        if(!target.UUIDToStateTable.ContainsKey(uuid)){
          DeleteState(CurProgram.UUIDToStateTable[uuid]);
        }
      }

      foreach(string uuid in target.UUIDToTransitionTable.Keys){
        if(CurProgram.UUIDToTransitionTable.ContainsKey(uuid)){
          trTransition trans = CurProgram.UUIDToTransitionTable[uuid];
          bool isParaDifferent = trans.Trigger.IsParaDifferent(target.UUIDToTransitionTable[uuid].Trigger);
          trans.Copy(target.UUIDToTransitionTable[uuid]);
          TransitionToArrowTable[trans].TransitionToTriggerHolder[trans].SetUpView();
          if(isParaDifferent){
            TransitionToArrowTable[trans].TransitionToTriggerHolder[trans].PopLabel();
          }
        }
        else{
          trTransition t = new trTransition();
          trTransition tt = target.UUIDToTransitionTable[uuid];
          t.UUID = uuid;
          t.Copy(tt);
          t.StateSource = CurProgram.UUIDToStateTable[tt.StateSource.UUID]; //add checking 
          t.StateTarget = CurProgram.UUIDToStateTable[tt.StateTarget.UUID]; //add checking
          CurProgram.AddTransition(t);
          CreateTransitionArrow(t);
        }
      }

      // avoiding modifying dictionary when iterating through it
      uuids = new string[CurProgram.UUIDToTransitionTable.Count];
      CurProgram.UUIDToTransitionTable.Keys.CopyTo(uuids, 0); 

      foreach(string uuid in uuids){
        if(!target.UUIDToTransitionTable.ContainsKey(uuid)){
          trTransition trans = CurProgram.UUIDToTransitionTable[uuid];
          TransitionToArrowTable[trans].RemoveTransition(trans);
        }
      }

      trDataManager.Instance.SaveCurProgram();

    }

    public void ShowTriggerConfigurationPanel(trTransition trTrn) {
      // find the arrow
      if (!TransitionToArrowTable.ContainsKey(trTrn)) {
        WWLog.logError("transition not in arrow table! " + trTrn.ToString());
        return;
      }

      showTriggerConfigurationPanel(TransitionToArrowTable[trTrn], trTrn, false);
    }

    void showTriggerConfigurationPanel(trTransitionArrowController arrow, trTransition transition, bool isCreateTransition = false){
      ProtoCtrl.HideElementInfo();
      ProtoCtrl.TriggerConfigurePanel.SetConfiguration(arrow, transition, isCreateTransition);
      ProtoCtrl.TriggerConfigurePanel.OnDismiss -= onTriggerPanelVisibile;
      ProtoCtrl.TriggerConfigurePanel.OnDismiss += onTriggerPanelVisibile;
      if (ProtoCtrl.OnConfigPanelVisibilityChanged != null){
        ProtoCtrl.OnConfigPanelVisibilityChanged(true);
      }
    }

    void onTriggerPanelVisibile(trTrigger trigger, bool visible){
      if (ProtoCtrl.OnConfigPanelVisibilityChanged != null){
        ProtoCtrl.OnConfigPanelVisibilityChanged(false);
      }
    }

    public void CreateTransition(trStateButtonController sourceButton, trStateButtonController targetButton, bool isUserAction = true){
      if (ProtoCtrl.IsRunning) return;
      if (targetButton.StateData.Behavior.Type == trBehaviorType.OMNI){
        targetButton.ShowIncorrect();
        return;
      }

      if (sourceButton.StateData.OutgoingTransitions.Count >= trToFirmware.cOutgoingTransitionsPerState) {
        ProtoCtrl.ShowExceedTransitionsDialog();
        return;
      }

      trTransition newTransition = new trTransition();
      newTransition.StateSource = sourceButton.StateData;
      newTransition.StateTarget = targetButton.StateData;

      ProtoCtrl.CurProgram.UUIDToTransitionTable.Add(newTransition.UUID, newTransition);

      if(sourceButton.StateData.AddOutgoingTransition(newTransition)){
        trTransitionArrowController arrowCtrl = CreateTransitionArrow(newTransition);
        if(isUserAction){
          showTriggerConfigurationPanel(arrowCtrl, newTransition, true);
        }
      }
    }

    public void DeleteStateButton(trState state, bool isSave = true){
      Destroy(StateToButtonTable[state].gameObject);
      StateToButtonTable.Remove(state);
      ProtoCtrl.CurProgram.UUIDToStateTable.Remove(state.UUID);

      if(isSave){
        trDataManager.Instance.SaveCurProgram();
      }
    }
      

    public override trStateButtonController CreateStateButton(trState state){
      trStateButtonController button = base. CreateStateButton(state);
      button.ProtoCtrl = ProtoCtrl; // order matters! this should be first
      button.ClickListeners += ClickOneState;
      button.onDragStateButton += StateMoveListener.OnStateDrag;
      button.onDropStateButton += StateMoveListener.OnStateButtonDrop;
      button.TransitionContainer.ProtoCtrl = ProtoCtrl;
      button.TransitionContainer.onDragTransitionButton += TransitionButtonMoveListener.OnTransitionButtonDrag;
      button.TransitionContainer.onDropTransitionButton += TransitionButtonMoveListener.OnTransitionButtonDrop;

      // todo: this should be refactored into trStateButtonController itself, with helper method to denote if this is 
      // editable or not.  If not, then transitions are not created.
//      button.onDragListener += onStateButtonDrag;
      return button;
    }
    public void AddState(Vector2 pos, trBehavior behavior) {
      // todo for @igorl: this should be refactored to use CreateStateButton(state) method
      SoundManager.soundManager.PlaySound(SoundManager.trAppSound.NEW_STATE);

      Vector3 posV = new Vector3(pos.x, pos.y, 0);
      GameObject newStateBtn = Instantiate(StateButtonPrefab, posV, Quaternion.identity) as GameObject;
      newStateBtn.GetComponent<trButtonTween>().TweenDrop();
      
      trStateButtonController button = newStateBtn.GetComponent<trStateButtonController>();

      button.transform.SetParent(StatePanel.transform);      
      Vector3 posL = newStateBtn.transform.localPosition;
      posL.z = StateButtonPrefab.transform.localPosition.z;
      newStateBtn.transform.localPosition = posL;   
      button.ProtoCtrl = ProtoCtrl;
      button.StateMachinePnlCtrl = this;

      trState newState = new trState();
      newState.UserFacingName = "S" + StateToButtonTable.Count.ToString();;
      button.StateData = newState;
     
      if(!behavior.Type.IsMood()){
        button.BehaviorData = behavior;
      }
      else{
        button.BehaviorData = trBehavior.GetDefaultBehavior(trBehaviorType.DO_NOTHING);
        newState.Mood = behavior.Type.ToMood();
        button.SetUpView();
      }
      button.AttachAdditionalViewIfNeeded();
      button.ClickListeners += ClickOneState;

      button.onDragStateButton += StateMoveListener.OnStateDrag;
      button.onDropStateButton += StateMoveListener.OnStateButtonDrop;

      button.TransitionContainer.ProtoCtrl = ProtoCtrl;
      button.TransitionContainer.onDragTransitionButton += TransitionButtonMoveListener.OnTransitionButtonDrag;
      button.TransitionContainer.onDropTransitionButton += TransitionButtonMoveListener.OnTransitionButtonDrop;   

      StateToButtonTable.Add(button.StateData, button);
      ProtoCtrl.CurProgram.AddState(newState);
      
      if (behavior.Type == trBehaviorType.OMNI){
        ProtoCtrl.SetOmniState(newState);
      }

      if(!ShowBehaviourConfigurationPanel(button, true)){ // save action to UndoRedo history if it doesn't need config
        UpdateUndoRedoUserAction();
        trDataManager.Instance.SaveCurProgram();
      }     
    }

    public void TryInsertState(Vector3 pos, trStateButtonController button){
      trTransition transition = CheckDropOnTransition(pos);
      
      if(transition!= null){
        trState start = transition.StateSource;
        trState target = transition.StateTarget;
        foreach(trTransition tran in start.OutgoingTransitions){
          tran.StateTarget = button.StateData;
          TransitionToArrowTable[tran].SetUp(tran, TransitionToArrowTable[tran].SourceButton, button);
        }
        trTransition newTran = new trTransition();
        newTran.StateSource = button.StateData;
        newTran.StateTarget = target;
        button.StateData.AddOutgoingTransition(newTran);
        newTran.Trigger = new trTrigger(trTriggerType.BEHAVIOR_FINISHED);
        CreateTransitionArrow(newTran);
        trDataManager.Instance.GetCurProgram().UUIDToTransitionTable.Add(newTran.UUID, newTran);
        
        TransitionToArrowTable[transition].SetActiveDropOn(false);
        trDataManager.Instance.SaveCurProgram();
      }
    }

    public void ShowBehaviourConfigurationPanel(trState state, bool isNewBehavior=false){
      if (!StateToButtonTable.ContainsKey(state)) {
        WWLog.logError("state not in statetobutton table: " + state.ToString());
        return;
      }

      ShowBehaviourConfigurationPanel(StateToButtonTable[state], isNewBehavior);
    }

    public bool ShowBehaviourConfigurationPanel(trStateButtonController button, bool isNewBehavior=false){
      trState stateData = button.StateData;

      if (ProtoCtrl != null) {
        ProtoCtrl.HideElementInfo();
      }

      if(stateData.Behavior.IsMissionBehavior){
        return false;
      }
      float delay = isNewBehavior ? 0.3f : 0f;

      if (!stateData.Behavior.IsParameterized) {
        return false;
      }

      bool hasConfigPanel = false;

      if ((stateData.Behavior.Type == trBehaviorType.MOVE_CONT_SPIN) ||
          (stateData.Behavior.Type == trBehaviorType.MOVE_CONT_STRAIGHT) || 
          (stateData.Behavior.Type == trBehaviorType.MOVE_DISC_STRAIGHT) ||
          (stateData.Behavior.Type == trBehaviorType.MOVE_DISC_TURN) ||
          (stateData.Behavior.Type == trBehaviorType.HEAD_PAN) ||
          (stateData.Behavior.Type == trBehaviorType.HEAD_TILT) || 
          (stateData.Behavior.Type == trBehaviorType.EYERING) || 
          (stateData.Behavior.Type == trBehaviorType.LAUNCH_FLING)) {  
          _simpleBehaviorPanel.IsNewBehavior = isNewBehavior;
          _simpleBehaviorPanel.State = stateData;
          _simpleBehaviorPanel.OnDismiss = delegate(trState state, bool success) {
          tryEmitParameterSignal(state, success);
          if (stateData.Behavior.Type == trBehaviorType.EYERING){
            button.GetComponentInChildren<trEyeRingView>().SerializedValue = (int)state.BehaviorParameterValue;
          }
          else {
            button.UpdateTextLabel();            
          }

          if (ProtoCtrl.OnConfigPanelVisibilityChanged != null){
            ProtoCtrl.OnConfigPanelVisibilityChanged(false);
          }
          button.enabled = true;
        };
        button.enabled = false;
        uGUIPanelTween.Instance.TweenOpen(_simpleBehaviorPanel.gameObject.transform, delay);
        hasConfigPanel = true;
      } 
      else if ((stateData.Behavior.isSoundBehaviour()) || (stateData.Behavior.Type == trBehaviorType.PUPPET)){
        ProtoCtrl.SoundConfigurePanel.SetStateData(button, trDataManager.Instance.CurrentRobotTypeSelected);
        uGUIPanelTween.Instance.TweenOpen(ProtoCtrl.SoundConfigurePanel.gameObject.transform, delay);
        ProtoCtrl.SoundConfigurePanel.OnEditingFinished = delegate(trState state, bool success) {
          tryEmitParameterSignal(state, success);
          button.UpdateTextLabel();
          Text customTextLabel = button.StateImage.GetComponentInChildren<Text>();
          if (customTextLabel != null){
            customTextLabel.text = button.StateData.StateConfigText(ProtoCtrl.CurRobot);
          }
          if (ProtoCtrl.OnConfigPanelVisibilityChanged != null){
            ProtoCtrl.OnConfigPanelVisibilityChanged(false);
          }
          button.enabled = true;
        };
        button.enabled = false;
        hasConfigPanel = true;
        ProtoCtrl.SoundConfigurePanel.IsNewBehavior = isNewBehavior;
      } 
      else if (stateData.Behavior.isAnimation()){       
        if (!isNewBehavior || stateData.Behavior.Animation.AvailableMoods.Count > 1) {
          ProtoCtrl.AnimationConfigurePanel.SetStateData(button,  trDataManager.Instance.CurrentRobotTypeSelected);
          uGUIPanelTween.Instance.TweenOpen(ProtoCtrl.AnimationConfigurePanel.gameObject.transform, delay);
          ProtoCtrl.AnimationConfigurePanel.OnEditingFinished = delegate(trState state, bool success) {
            tryEmitParameterSignal(state, success);
            button.UpdateTextLabel();
            Text customTextLabel = button.StateImage.GetComponentInChildren<Text>();
            if (customTextLabel != null){
              customTextLabel.text = button.StateData.StateConfigText(ProtoCtrl.CurRobot);
            }
            if (ProtoCtrl.OnConfigPanelVisibilityChanged != null){
              ProtoCtrl.OnConfigPanelVisibilityChanged(false);
            }
          };
          ProtoCtrl.AnimationConfigurePanel.IsNewBehavior = isNewBehavior;
          hasConfigPanel = true;
        }       
      }    
      else {
        WWLog.logError(string.Format("Can't find config panel for behaviour {0}", stateData.Behavior));
      }
      if (ProtoCtrl.OnConfigPanelVisibilityChanged != null){
        ProtoCtrl.OnConfigPanelVisibilityChanged(true);
      }
      return hasConfigPanel;
    }

    public void DeleteState(trState state){
    
      if (trDataManager.Instance.IsInNormalMissionMode) {
        new trTelemetryEvent(trTelemetryEventType.CHAL_DEL_ACTION, true)
          .add(trTelemetryParamType.ROBOT_TYPE, ProtoCtrl.CurProgram.RobotType)
          .add(trTelemetryParamType.TYPE, state.Behavior.Type.Consolidated())
          .add(trTelemetryParamType.CHALLENGE, trDataManager.Instance.MissionMng.GetCurMission().UserFacingName)
          .add(trTelemetryParamType.STEP, trDataManager.Instance.MissionMng.UserOverallProgress.CurMissionInfo.PuzzleIndex)
          .emit();
      }
      else {
        new trTelemetryEvent(trTelemetryEventType.FP_DEL_ACTION, true)
          .add(trTelemetryParamType.ROBOT_TYPE, ProtoCtrl.CurProgram.RobotType)
          .add(trTelemetryParamType.TYPE, state.Behavior.Type.Consolidated())
          .emit();
      }
      
      
      ProtoCtrl.CurProgram.RemoveState(state);
      RemoveUnusedElements();
      trDataManager.Instance.SaveCurProgram();
    }

    public trTransitionArrowController ArrowByTransition(trTransition transition) {
      trTransitionArrowController ret = null;
      if (transition != null) {
        if (!TransitionToArrowTable.TryGetValue(transition, out ret)) {
          WWLog.logError("no such transition: " + transition.ToString());
        }
      }
      return ret;
    }


    public void RemoveAllTransitions(trState state){
      ProtoCtrl.CurProgram.RemoveAllStateTransitions(state);
      RemoveUnusedElements();
      trDataManager.Instance.SaveCurProgram();
    }

    private void RemoveUnusedElements(){
      Dictionary<trTransitionArrowController, List<trTransition>> toRemove = new Dictionary<trTransitionArrowController, List<trTransition>>();
      foreach(var item in TransitionToArrowTable.Values){
        foreach(trTransition transition in item.TransitionToTriggerHolder.Keys){
          if (!ProtoCtrl.CurProgram.UUIDToTransitionTable.ContainsValue(transition)){
            List<trTransition> listTransitions = null;
            if (toRemove.ContainsKey(item)){
              listTransitions = toRemove[item];
            }
            else {
              listTransitions = new List<trTransition>();
              toRemove[item] = listTransitions;
            }
            if(!listTransitions .Contains(transition)){
              listTransitions.Add(transition);
            }  

          }
        }
      }


      foreach(trTransitionArrowController arrow in toRemove.Keys){
        List<trTransition> transitions = toRemove[arrow];
        foreach(trTransition item in transitions){
          TransitionToArrowTable.Remove(item);
          arrow.RemoveTransition(item, false);
        }
      }
      
      List<trState> removedStates = new List<trState>();
      
      foreach (var item in StateToButtonTable.Keys){
        if (!ProtoCtrl.CurProgram.UUIDToStateTable.ContainsValue(item)){
          Destroy(StateToButtonTable[item].gameObject);
          removedStates.Add(item);
        }
      }
      
      foreach(var item in removedStates){
        StateToButtonTable.Remove(item);
      }
    }
   
    public void ClickOneState(trStateButtonController button){
      trState state = button.StateData;

      if(state.Behavior.Type == trBehaviorType.MISSION){
        ProtoCtrl.MissionAreaConfigCtrl.ChooseArea(button);
        return;
      }

      if(ProtoCtrl.IsRunning){
        if (!trDataManager.Instance.IsMissionMode){
          if (state.Behavior.Type == trBehaviorType.START_STATE) {
            ProtoCtrl.CurRobot.Reset();
          }
          bool setCurState = true;
          if(state.Behavior.Type == trBehaviorType.FUNCTION){
            if(state == ProtoCtrl.CurProgram.StateCurrent){
              ProtoCtrl.LoadFunction((trFunctionBehavior)(state.Behavior));
              setCurState = false;
            }
          }
          if(setCurState){
            ProtoCtrl.SetCurState(state);
          }         
        }
      } 
      else {
        if(state.Behavior.Type == trBehaviorType.FUNCTION){
          ProtoCtrl.LoadFunction((trFunctionBehavior)(state.Behavior));
        }
        else {
          ShowBehaviourConfigurationPanel(state);
        }
      }
    }

    public void SetEnableOmniStateButton(trState state, bool isEnabled){
      if(state == null){
        return;
      }
      trStateButtonController button; 
      StateToButtonTable.TryGetValue(state, out button);
      if(button!= null){
        button.SetEnableOmniState(isEnabled);
      }
    }

    public trTransition CheckDropOnTransition(Vector3 worldPos){
      trTransition ret = null;
      foreach(trTransition transition in TransitionToArrowTable.Keys){
        trTransitionArrowController arrow = TransitionToArrowTable[transition];
        if(ret != null){
          arrow.SetActiveDropOn(false);
          continue;
        }
        RectTransform trans = arrow.GetComponent<RectTransform>();
        Vector2 vec = trans.GetXYRatioWithWorldPos(worldPos);
        bool isTransition = arrow.TransitionLine.Displacement ==0 && vec.x <= 1 && vec.x >= 0 &&vec.y <= 1.5f && vec.y >= -0.5f;
        isTransition |= arrow.TransitionLine.Displacement !=0 && vec.x <= 1 && vec.x >= 0 &&vec.y <= 2.5f && vec.y >= 0.5f;
        if(isTransition){
          arrow.SetActiveDropOn(true);
          ret = transition;
        }else{
          arrow.SetActiveDropOn(false);
        }
      } 
      return ret;
    }


    public override trTransitionArrowController CreateTransitionArrow(trTransition transition){

      trTransitionArrowController arrowCtrl = base.CreateTransitionArrow(transition);
      if (arrowCtrl.ProtoCtrl == null){
        arrowCtrl.DeleteTransition += DeleteTransition;
        arrowCtrl.TriggerButtonClicked += onTriggerButtonClicked;
        arrowCtrl.ProtoCtrl = ProtoCtrl;
      }

      return arrowCtrl;
    }

    public void ResetArrows(){
     foreach(trTransitionArrowController arrow in TransitionToArrowTable.Values){
        Destroy(arrow.gameObject);
      }
      TransitionToArrowTable.Clear();
    }
    
//    void onStateButtonDrag(trStateButtonController button, PointerEventData eventData){
//      GameObject belowObject = eventData.pointerDrag;
//      if (belowObject != null){
//        trStateButtonController[] belowButtons = belowObject.GetComponents<trStateButtonController>();
//        WWLog.logDebug(string.Format("size {0}", belowButtons.Length));
//        if (belowButtons.Length > 1){
//          WWLog.logDebug("Catched");
//        }
//      }
//    }

    private void tryEmitParameterSignal(trState state, bool emit) {
      if (!emit) {
        // this currently never happens - there's no 'cancel' on parameterization dialogs.
        return;
      }

      if (!trDataManager.Instance.IsInNormalMissionMode) {
        new trTelemetryEvent(trTelemetryEventType.FP_PARAM_ACTION, true)
          .add(trTelemetryParamType.ROBOT_TYPE, ProtoCtrl.CurProgram.RobotType)
          .add(trTelemetryParamType.TYPE      , state.Behavior.Type.Consolidated())
          .add(trTelemetryParamType.DETAIL    , state.Behavior.TelemetryDetail(state))
          .emit();
      }
    }
  }
}
