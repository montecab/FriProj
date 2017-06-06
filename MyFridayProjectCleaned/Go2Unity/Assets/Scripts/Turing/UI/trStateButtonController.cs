using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Turing;
using WW.UGUI;
using UnityEngine.Events;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;

namespace Turing{
  public class trStateButtonController : trButtonControllerBase, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, IDropHandler {

    public trButtonTween TweenCtrl;
    private trState stateData;
    public trState StateData{
      set{
        stateData = value;
        SetUp();
      }
      get{
        return stateData;
      }
    }
    public TextMeshProUGUI NameLabel;


    public GameObject CoverDarkenImage;
    public GameObject RunFocus;
    public Image StateImage;

    public trTransitionButtonController TransitionContainer;
    public Image TreatmentRing;
    public Image ErrorImage;

   
    public GameObject ActivationTimeConfigPanel;
    public Button PlusButton;
    public Button MinusButton;
    public Text ActivationTimeLabel;
    public Toggle CheckParameterToggle;
    public Button PropagateButton;
    public Toggle IsObscuredToggle;
    public bool IsObscureAllowed = false;
    public Image ObscureImage;

    public bool IsDraggingState = false;

    public delegate void StateButtonDelegate(trStateButtonController ctrl);
    public StateButtonDelegate ClickListeners;
    public StateButtonDelegate onDragStateButton;
    public StateButtonDelegate onDropStateButton;


    public GameObject IndexContainer;
    public Text IndexText;

   

    public GameObject FunctionRunningIndicator;

    public trStateMachinePanelBase StateMachinePnlCtrl;
    public GameObject EyeRingtMiniPrefab;

    private trEyeRingView eyeRingView;

    public float lastPositionChangeTime;

    public trBehavior BehaviorData{
      set{
        if(stateData == null){
          WWLog.logError("Tried to set behavior data while there is no state data set");
          return;
        }
        if(ProtoCtrl != null){
          if (stateData.Behavior == null || stateData.Behavior.Type != value.Type) {
          
            if (!trDataManager.Instance.IsMissionMode) {
              trTelemetryEvent trTE = new trTelemetryEvent(trTelemetryEventType.FP_SET_ACTION, true)
                .add(trTelemetryParamType.ROBOT_TYPE, ProtoCtrl.CurProgram.RobotType)
                .add(trTelemetryParamType.TYPE      , value.Type.Consolidated())
                .add(trTelemetryParamType.TYPE_PREV , stateData.Behavior == null ? "none" : stateData.Behavior.Type.Consolidated());
              if (value.Type.IsColor()) {
                trTE.add(trTelemetryParamType.DETAIL, value.Type.ToString());
              }
              trTE.emit();
            }
          }
          if(stateData.Behavior != value){  
            ProtoCtrl.CurProgram.setStateBehaviour(stateData, value);
          }
        }   
        SetUpView();
       
      }
      get{
        if(stateData == null){
          WWLog.logError("Tried to get behavior data while there is no state data set");
          return null;
        }
        return stateData.Behavior;
      }

    }

    void Start(){
      PlusButton.onClick.AddListener(onClickPlusButton);
      MinusButton.onClick.AddListener(onClickMinusButton);
      CheckParameterToggle.onValueChanged.AddListener(onCheckParaToggled);
      PropagateButton.onClick.AddListener(onClickPropagateButton);
      IsObscuredToggle.onValueChanged.AddListener(onIsHideToggled) ;
      lastPositionChangeTime = Time.time;
    }

    void OnDestroy(){
      PlusButton.onClick.RemoveListener(onClickPlusButton);
      MinusButton.onClick.RemoveListener(onClickMinusButton);
      CheckParameterToggle.onValueChanged.RemoveListener(onCheckParaToggled);
      PropagateButton.onClick.RemoveListener(onClickPropagateButton);
      IsObscuredToggle.onValueChanged.RemoveListener(onIsHideToggled) ;
    }

    void onIsHideToggled(bool isOn){
      StateData.IsObscured = isOn;
      trDataManager.Instance.SaveCurProgram();
    }

    void onClickPropagateButton(){
      ProtoCtrl.missionAuthoringPanel.PropgCtrl.showPropagatePanel(StateData);
    }

    void onCheckParaToggled(bool isOn){
      StateData.IsCheckingParameter = isOn;
      trDataManager.Instance.SaveCurProgram();
    }

    void onClickPlusButton(){
      if(BehaviorData.Type == trBehaviorType.OMNI){
        return;
      }
      StateData.ActivationCount ++;
      updateActivationTimeLabel();
      trDataManager.Instance.SaveCurProgram();
    }

    void onClickMinusButton(){
      StateData.ActivationCount --;
      updateActivationTimeLabel();
      trDataManager.Instance.SaveCurProgram();
    }

    void updateActivationTimeLabel(){
      ActivationTimeLabel.text = StateData.ActivationCount.ToString();
    }

    public void UpdateLayoutPos(){
      this.gameObject.transform.localPosition = new Vector3(StateData.LayoutPosition.x, 
                                                            StateData.LayoutPosition.y,
                                                            this.gameObject.transform.localPosition.z);
    }

    public override void SetUpView(){
      base.SetUpView();
      IndexContainer.SetActive(false);

      if (this.BehaviorData.Type != trBehaviorType.START_STATE) {
        StateImage.sprite = trIconFactory.GetIcon(stateData);
      }
      UpdateTextLabel();
      if(BehaviorData.IsMissionBehavior){
        setActiveBehaviorLabel(true);
        IndexContainer.SetActive(true);
        IndexText.text = stateData.BehaviorParameterValue.ToString();
      }
      //MoodContainer.SetUpView();

      if(BehaviorData.Type == trBehaviorType.SOUND_USER || BehaviorData.Type == trBehaviorType.DO_NOTHING){
        IndexContainer.SetActive(true);
        Vector3 position = IndexContainer.transform.localPosition;
        Vector2 size = GetComponent<RectTransform>().GetSize();
        position.x = IndexContainer.GetComponent<RectTransform>().GetSize().x / 2;
        position.y = 0;

        if (BehaviorData.Type == trBehaviorType.SOUND_USER){
          IndexText.text = trRobotSounds.Instance.GetUserFacingIndex(StateData).ToString();
          position.x = 0;
          position.y = -(size.y - IndexContainer.GetComponent<RectTransform>().GetSize().y) * 0.97f;
        } 
        IndexContainer.transform.localPosition = position;
      } 
      else if (BehaviorData.isAnimation()) {
        string path = trMoodyAnimations.Instance.getAnimationIconPath(StateData);
        if (path != null) {
          StateImage.sprite = Resources.Load <Sprite>(path);
        }
      }
      else if(BehaviorData.Type == trBehaviorType.EYERING){
        if(eyeRingView == null){
          AttachAdditionalViewIfNeeded();
        }
        else{
          eyeRingView.SerializedValue = (int) StateData.BehaviorParameterValue;
        }
      }

//      trMultivariate.trAppOptionValue value = trMultivariate.Instance.getOptionValue(trMultivariate.trAppOption.EXTRA_START_BUTTON);
//      if(value == trMultivariate.trAppOptionValue.NO && IsStartButton()){
//        RunImageNew.SetActive(true);
//        StopImage = StopImageNew;
//        RunFocus = RunFocusNew;
//        TransitionContainer.transform.position = RunStateTransitionContainerPos.transform.position; 
//        StateImage.gameObject.SetActive(false);
//      }

      updateActivationTimeLabel();
      if(BehaviorData.IsParameterized){
        CheckParameterToggle.gameObject.SetActive(true);
        CheckParameterToggle.isOn = StateData.IsCheckingParameter;
      }
      else{
        CheckParameterToggle.gameObject.SetActive(false);
      }

      IsObscuredToggle.isOn = StateData.IsObscured;

      bool showObscredImg = IsObscureAllowed && StateData.IsObscured;
      ObscureImage.gameObject.SetActive(showObscredImg);
      StateImage.gameObject.SetActive(!showObscredImg);
      if(eyeRingView != null){
        eyeRingView.gameObject.SetActive(!showObscredImg);
      }
      if(IndexContainer.gameObject.activeSelf && showObscredImg){
        IndexContainer.gameObject.SetActive(false);
      }

      if(BehaviorData.Type == trBehaviorType.FUNCTION_END){
        TransitionContainer.gameObject.SetActive(false);
      }
    }

    public void UpdateTextLabel(){
      if (BehaviorData.IsParameterized){
        BehaviorData.RunningParamValue = stateData.BehaviorParameterValue;
        NameLabel.literalText = string.Format("{0} {1}", BehaviorData.UserFacingNameLocalized, trStringFactory.GetParaString(stateData));
      } else {
        NameLabel.literalText = BehaviorData.UserFacingNameLocalized;
      }
    }

    public void SetUp(){
      if(stateData == null){
        WWLog.logWarn("state data is null");
        return;
      }
      CheckSaveLayoutPosition();
    }

    private bool IsAnyConnectedTransition(){
      if(StateData.OutgoingTransitions.Count > 0){
        return true;
      }
      foreach(trTransition transition in trDataManager.Instance.GetCurProgram().UUIDToTransitionTable.Values){
        if(transition.StateTarget == stateData){
          return true;
        }
      }
      return false;
    }

    public bool IsStartButton(){
      return StateData.Behavior.Type == trBehaviorType.START_STATE;
    }



    public void SetEnableOmniState(bool isEnabled){
      if (!isEnabled && stateData.Behavior.Type == trBehaviorType.OMNI){
        BehaviorData = new trBehavior(trBehaviorType.DO_NOTHING);
      }
    }

    public void SetRunFocus(bool isFocus){
      RunFocus.SetActive(isFocus);
      if(!IsStartButton()){
        CoverDarkenImage.SetActive(!isFocus);
      }else{
        CoverDarkenImage.SetActive(false);
      }
    }
      
    public void SetEnableRunningUI(bool isEnable){
      if(!isEnable){
        SetNormal();
      }
      else{
        CoverDarkenImage.SetActive(true);
      }        
    }

    public override void EnableUserInteraction(bool interaction){
      // enable/disable user interactions
      base.EnableUserInteraction(interaction);  
      if(BehaviorData.Type != trBehaviorType.FUNCTION_END){
        TransitionContainer.gameObject.SetActive(interaction);
      }        
    }

    public void SetNormal(){
      RunFocus.SetActive(false);
      CoverDarkenImage.SetActive(false);
      FunctionRunningIndicator.SetActive(false);
    }


    public void ShowIncorrect(){
      StartCoroutine(showIncorrect());
    }
    
    IEnumerator showIncorrect(){
      this.ErrorImage.gameObject.SetActive(true);
      yield return new WaitForSeconds(1.5f);
      this.ErrorImage.gameObject.SetActive(false);
    }
    
    public bool CheckSaveLayoutPosition(){
      if(ProtoCtrl == null){
        return false;
      }
     
      Vector3 localPos = this.transform.localPosition;
      Vector2 newPos = new Vector2(localPos.x, localPos.y);

      if((newPos - StateData.LayoutPosition).magnitude > 3.0f){
        StateData.LayoutPosition = newPos;
        return true;
      }

      return false;
    }

    void setActiveBehaviorLabel(bool toEnable){
      NameLabel.gameObject.SetActive(toEnable);
    }

    public void SetSelectedForTransition(bool selected){
      animateStateTouchFocusChanged(selected);
    }


    public void SetActiveAtvTimeConfigPnl(bool active){
      ActivationTimeConfigPanel.SetActive(active);
    }
      
    void animateStateTouchFocusChanged(bool touched){
      if (touched){
        TweenCtrl.TweenDragged();
        TreatmentRing.gameObject.SetActive(true);
        TreatmentRing.GetComponent<trButtonTween>().TweenDragged();
      } else {
        TweenCtrl.TweenDrop();
        TreatmentRing.GetComponent<trButtonTween>().TweenDrop();
        TreatmentRing.gameObject.SetActive(false);
      }
    }

    #region Handling user interactions
    public override void OnBeginDrag (PointerEventData eventData){
      if (ProtoCtrl != null && CanUserInteract()){
        base.OnBeginDrag(eventData);
        //change parent so that state button is on top of trash can
        gameObject.transform.SetParent(ProtoCtrl.StateEditCtrl.StateDragPanel.transform);
        IsDraggingState = true;
        lastPositionChangeTime = Time.time;
      }
      CancelElementInfoLongPress(false);
    }

    public override void OnDrag (PointerEventData eventData){
      if (ProtoCtrl != null && CanUserInteract()){
        base.OnDrag(eventData);
        gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x,
                                                          gameObject.transform.localPosition.y,
                                                          0);
        if(!IsStartButton()){
          ProtoCtrl.TrashCanCtrl.IsInTrashCanArea(gameObject.transform.position);
        }

        if (onDragStateButton != null){
          onDragStateButton(this);
        }
        lastPositionChangeTime = Time.time;

//        if(!IsAnyConnectedTransition()){
//          ProtoCtrl.StateEditCtrl.CheckDropOnTransition(gameObject.transform.position);
//        }        
      }
    }
    public override void OnEndDrag (PointerEventData eventData){
      if (ProtoCtrl != null &&CanUserInteract()){
        if(ProtoCtrl.TrashCanCtrl.IsInTrashCanArea(gameObject.transform.position)){
          if(IsStartButton()){
            ProtoCtrl.StateEditCtrl.RemoveAllTransitions(stateData);
            GoBackToStartWorldPos();
            gameObject.transform.SetParent(ProtoCtrl.StateEditCtrl.StatePanel.transform);
            gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x,
                                                             gameObject.transform.localPosition.y,
                                                             -10);
          }
          else{
            ProtoCtrl.DeleteState(stateData);
            SoundManager.soundManager.PlaySound(SoundManager.trAppSound.TRASH);
            ProtoCtrl.StateEditCtrl.UpdateUndoRedoUserAction();
          }
          if (onDropStateButton != null){
            onDropStateButton(this);
          }
        }
        else{
          if (ProtoCtrl.AllowedScrollRect != null && !ProtoCtrl.AllowedScrollRect.IsPointInLimitRect(gameObject.transform.position)){
            GoBackToStartWorldPos();
            TweenCtrl.Shake();
          } 

          if (onDropStateButton != null){
            onDropStateButton(this);
          }

          gameObject.transform.SetParent(ProtoCtrl.StateEditCtrl.StatePanel.transform);
          gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x,
                                                            gameObject.transform.localPosition.y,
                                                            -10);
          CheckSaveLayoutPosition();
        }
        ProtoCtrl.TrashCanCtrl.SetAnimation(false);
        base.OnEndDrag (eventData);
        IsDraggingState = false;
        lastPositionChangeTime = Time.time;

//        if(!IsAnyConnectedTransition()){
//          ProtoCtrl.StateEditCtrl.TryInsertState(gameObject.transform.position, this);
//        } 
      }
    }


    public void OnDrop (PointerEventData eventData){

      GameObject obj = eventData.pointerDrag;
      bool canDropOmniState = (trMultivariate.Instance.getOptionValue(trMultivariate.trAppOption.OMNI_DISABLE_IN_TRAY) != trMultivariate.trAppOptionValue.YES);
      if(obj!= null){
        trBehaviorButtonController behaviorButton = obj.GetComponent<trBehaviorButtonController>();

        if(behaviorButton != null && behaviorButton.IsActive){
          if(behaviorButton.BehaviorData.Type.IsMood()
             &&trDataManager.Instance.GetCurProgram().IsStateAllowedToSetMood(StateData)){
            stateData.Mood = behaviorButton.BehaviorData.Type.ToMood();
            SetUpView();
          }
          else if(IsStartButton()){
            TweenCtrl.Shake();
            return;
          }
          else if ((canDropOmniState || behaviorButton.BehaviorData.Type != trBehaviorType.OMNI && BehaviorData.Type != trBehaviorType.OMNI)){
            if (behaviorButton.BehaviorData.isAnimation()) {
              stateData.Mood = behaviorButton.BehaviorData.Animation.AvailableMoods[0];
            }
            RemoveAdditionalView();
            if(BehaviorData != behaviorButton.BehaviorData){
              BehaviorData = behaviorButton.BehaviorData;
              if(!ProtoCtrl.StateEditCtrl.ShowBehaviourConfigurationPanel(this, true)){
                ProtoCtrl.StateEditCtrl.UpdateUndoRedoUserAction();
              }
              ProtoCtrl.StateEditCtrl.UpdateTriggerWarningShowing(StateData);
            }
           
            AttachAdditionalViewIfNeeded();

          } 
          else if (BehaviorData.Type == trBehaviorType.DO_NOTHING && behaviorButton.BehaviorData.Type != trBehaviorType.OMNI) {
            TweenCtrl.Shake();
          }

          if (behaviorButton.BehaviorData.Type == trBehaviorType.OMNI || BehaviorData.Type == trBehaviorType.OMNI){
            ProtoCtrl.SetOmniState(stateData);
          } 
          else if (stateData.IsOmniState){
            ProtoCtrl.SetOmniState(null);
          }

          CheckSaveLayoutPosition();
          trDataManager.Instance.SaveCurProgram();
        }

        // trMoodButtonController moodButton = obj.GetComponent<trMoodButtonController>();
        // if(moodButton != null && moodButton!= MoodContainer){
        //   if(trDataManager.Instance.GetCurProgram().IsStateAllowedToSetMood(StateData)){
        //     StateData.Mood = moodButton.StateCtrl.stateData.Mood;
        //     moodButton.StateCtrl.stateData.Mood = trMoodType.NO_CHANGE;
        //     trDataManager.Instance.SaveCurProgram();
        //     SetUpView();
        //   }         
        // }
      }
    }

    public void AttachAdditionalViewIfNeeded(){
      if(stateData == null || stateData.Behavior == null){
        return;
      }
      if (StateData.Behavior.isEyeRingControlBehaviour()){
        if (eyeRingView == null){
          GameObject newEyering = Instantiate(EyeRingtMiniPrefab) as GameObject;
          newEyering.transform.SetParent(this.transform, false);
          newEyering.GetComponent<RectTransform>().offsetMax = Vector2.zero;
          newEyering.GetComponent<RectTransform>().offsetMin = Vector2.zero;
          newEyering.transform.localScale = Vector3.one * 0.95f;
          newEyering.GetComponentInChildren<Image>().gameObject.SetActive(false);
          eyeRingView = newEyering.GetComponent<trEyeRingView>();
        }
        eyeRingView.ToggleEnabled = false;
        eyeRingView.SerializedValue = (int)this.StateData.BehaviorParameterValue;
      } 

      if(stateData.Behavior.Type == trBehaviorType.DO_NOTHING){
        IndexText.text = StateMachinePnlCtrl.NextValueForBehaviourType(this.StateData.Behavior.Type).ToString();
      }
    }  


    public void RemoveAdditionalView(){
      if (eyeRingView != null){
        Destroy(eyeRingView.gameObject);
        eyeRingView = null;
      }
    }

    public void PopLabel(){
      NameLabel.transform.DOKill();
      NameLabel.gameObject.SetActive(true);
      NameLabel.transform.localScale = Vector3.one;
      NameLabel.transform.DOShakeScale(0.5f).OnComplete(()=>{NameLabel.gameObject.SetActive(false);});
    }

    public void OnPointerClick(PointerEventData eventData){	
    // this is called also after drag/drop, so prevent double calling!
      if (!eventData.dragging){
        if (!elementInfoLongPressHappened) {
          // if a long-press opened the element info panel, don't call click listeners.

          SoundManager.soundManager.PlaySound(SoundManager.trAppSound.UI_SOUND);
          if (ClickListeners != null) {
            ClickListeners(this);          
          }
        }
      }
    }

    public void OnPointerDown(PointerEventData eventData){
      if (CanUserInteract()){
        if(ProtoCtrl == null){ // this is because we use state button for map generation 
          return;
        }

        if (!ProtoCtrl.IsRunning){
          SetSelectedForTransition(true);
        }
        
        if(!BehaviorData.IsMissionBehavior){
          setActiveBehaviorLabel(true);
        }

        StartElementInfoLongPress();
      }     
    }
    
    public void OnPointerUp(PointerEventData eventData){
      if (CanUserInteract()){
        if(ProtoCtrl == null){ // this is because we use state button for map generation 
          return;
        }
        SetSelectedForTransition(false);
        if(!BehaviorData.IsMissionBehavior){
          setActiveBehaviorLabel(false);
        }        
      }

      bool closeIt = (trElementInfoPanelController.behaviorOnPointerUp == trElementInfoPanelController.BehaviorOnPointerUp.CLOSE);
      CancelElementInfoLongPress(closeIt);
    }
    #endregion

    private bool elementInfoLongPressCanProceed;
    private bool elementInfoLongPressHappened = false;
    void StartElementInfoLongPress() {
      elementInfoLongPressCanProceed = true;
      elementInfoLongPressHappened = false;
      StartCoroutine(crElementInfoLongPress());
    }

    void CancelElementInfoLongPress(bool closeElementInfo) {
      elementInfoLongPressCanProceed = false;

      if (ProtoCtrl && closeElementInfo) {
        ProtoCtrl.HideElementInfo();
      }
    }

    IEnumerator crElementInfoLongPress() {
      yield return new WaitForSeconds(trElementInfoPanelController.longPressDuration);
      if (elementInfoLongPressCanProceed) {
        elementInfoLongPressHappened = true;
        if (ProtoCtrl) {
          ProtoCtrl.TryShowElementInfo(stateData);
        }
      }
    }
  }
}
