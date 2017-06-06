using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Turing;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using WW.UGUI;
using DG.Tweening;
using System.Linq;

namespace Turing{
  public class trTransitionArrowController : trButtonControllerBase {

    public Dictionary<trTransition, trTriggerButtonViewHolder> TransitionToTriggerHolder = new Dictionary<trTransition, trTriggerButtonViewHolder>();

    public delegate void TransitionDelegate(trTransitionArrowController ctrl, trTransition transition);
    public TransitionDelegate DeleteTransition;

    public delegate void TriggerButtonDelegate(trTransitionArrowController ctrl, trTransition transition);
    public TriggerButtonDelegate TriggerButtonClicked;

    public RectTransform ButtonContainer;
    public trTransitionLine TransitionLine;
    public GameObject TriggerButtonPrefab;

    public Image SpritesArrowContainer; // Note: this is used for authoring panel programs since we don't want 3d stuff in authoring panel(it will need camera which makes a mess)

    public trStateButtonController SourceButton;
    public trStateButtonController TargetButton;

    public float TargetStateScaleSize = 1;

    public Animator TriggerFiredAnimator;

    private float buttonWidth;

    private float dragThreshold = 0;

    private bool isTwoArrows = false;


    //parameter used to check if the layout needs to be updated
    private Vector3 preSourceLocalPos = Vector3.zero;
    private Vector3 preTargetLocalPos = Vector3.zero;
    private bool preIsTwoTransition = false;
    private int preTriggerNum = 0;
    private float lastEndpointDragTime = 0;


#region System
    void Start(){
      dragThreshold = getConcreteValueForTransitionTreshold();

      //fix rendering order for lines and triggers
      TransitionLine.transform.SetParent(transform.parent);
      TransitionLine.transform.localPosition = this.transform.localPosition;
      TransitionLine.transform.SetAsFirstSibling();
    }

    void Update(){

      if(SourceButton == null || TargetButton == null) return;

      DoLayoutUpdate(false);
      
      if (!SourceButton.IsDraggingState && !TargetButton.IsDraggingState && TargetStateScaleSize > 1){
        updateStateScale(TargetButton.gameObject.transform, TargetStateScaleSize);
      }
    }

    void DoLayoutUpdate(bool force){
      
      isTwoArrows = isTwoTransition();
      
      bool stuffHasChanged = force;
      
      stuffHasChanged = stuffHasChanged || (isTwoArrows != preIsTwoTransition);
      stuffHasChanged = stuffHasChanged || (TransitionToTriggerHolder.Count != preTriggerNum);

      if (!stuffHasChanged) {
        if((SourceButton.lastPositionChangeTime < lastEndpointDragTime) && (TargetButton.lastPositionChangeTime < lastEndpointDragTime)){
          return;
        }
        lastEndpointDragTime = Time.time;
      }

      Vector3 sourcePos  = this.transform.parent.InverseTransformPoint(SourceButton.transform.position);
      sourcePos.z = 0;
      Vector3 targetPos  = this.transform.parent.InverseTransformPoint(TargetButton.transform.position);
      targetPos.z = 0;
      stuffHasChanged = stuffHasChanged || !piMathUtil.withinEpsilon((sourcePos - preSourceLocalPos).sqrMagnitude);
      stuffHasChanged = stuffHasChanged || !piMathUtil.withinEpsilon((targetPos - preTargetLocalPos).sqrMagnitude);

      if(stuffHasChanged){
        preSourceLocalPos = sourcePos;
        preTargetLocalPos = targetPos;
        preIsTwoTransition = isTwoArrows;
        preTriggerNum = TransitionToTriggerHolder.Count;
        
        LayoutTransition();
      }
    }

#endregion

#region Set Up
    
    public void AddTransition(trTransition transition, bool isShowObscure = false){
      if (TransitionToTriggerHolder.ContainsKey(transition)){
        return;
      }
      transition.OnTransitionActivated = playTransitionActivationAnimation;
      
      trTriggerButtonViewHolder triggerButton = createTriggerButton(transition, isShowObscure);
      TransitionToTriggerHolder.Add(transition, triggerButton);

    }
    
    public void RemoveTransition(trTransition transition, bool notify=true){
      if (!TransitionToTriggerHolder.ContainsKey(transition)){
        WWLog.logError("Trying to remove non-existing transition");
        return;
      }
      
      transition.OnTransitionActivated = null;
      
      trTriggerButtonViewHolder viewHolder = TransitionToTriggerHolder[transition];
      
      foreach(trTriggerButtonViewHolder vholder in TransitionToTriggerHolder.Values){
        if (vholder.index > viewHolder.index){
          vholder.index--;
        }
      }
      
      Destroy(viewHolder.gameObject);
      TransitionToTriggerHolder.Remove(transition);
      if (TransitionToTriggerHolder.Count == 0){
        Destroy(TransitionLine.gameObject);
        Destroy(gameObject);
      }
      
      if(DeleteTransition != null && notify){
        DeleteTransition(this, transition);
      }
    }
    
    trTriggerButtonViewHolder createTriggerButton(trTransition transition, bool isShowObscure){
      GameObject button = Instantiate(TriggerButtonPrefab);
      
      trTriggerButtonViewHolder viewHolder = button.GetComponent<trTriggerButtonViewHolder>();
      viewHolder.IsShowObscure = isShowObscure;
      viewHolder.onStartDrag += onStartDrag;
      viewHolder.onDrag += onDrag;
      viewHolder.onEndDrag += onEndDrag;
      viewHolder.onPointerDown += onPointerDown;
      viewHolder.onPointerUp += onPointerUp;

      viewHolder.TriggerButton.onClick.AddListener(delegate {
        trTransition local = transition;
        onTriggerButtonClicked(local);
      });
      
      viewHolder.index = TransitionToTriggerHolder.Keys.Count + 1;
      viewHolder.TransitionData = transition;
      
      viewHolder.SetUpView();
      button.transform.SetParent(ButtonContainer.transform);
      button.GetComponent<RectTransform>().SetDefaultScale();
      viewHolder.ParentCtrl = this;
      return viewHolder;
    }
    
   
    
    public void SetupTransitionView(trTransition transition){
      trTriggerButtonViewHolder viewHolder = TransitionToTriggerHolder[transition];
      viewHolder.SetUpView();
    }
    
    public void SetUp(trTransition transition, trStateButtonController sourceBtn, trStateButtonController targetBtn, bool isShowObscure = false){
      
      SourceButton = sourceBtn;
      TargetButton = targetBtn;
      
      TransitionLine.PtA = SourceButton;
      TransitionLine.PtB = TargetButton;
      
      AddTransition(transition, isShowObscure);
    }

    public trTriggerButtonViewHolder ButtonViewHolderByTransition(trTransition trT) {
      if (TransitionToTriggerHolder.ContainsKey(trT)) {
        return TransitionToTriggerHolder[trT];
      }
      else {
        WWLog.logError("no such transition here: " + trT.ToString());
        return null;
      }
    }
#endregion

    public override void EnableUserInteraction(bool interaction){
      base.EnableUserInteraction(interaction);
      foreach(trTriggerButtonViewHolder holder in TransitionToTriggerHolder.Values){
        holder.enabled = interaction;
      }
    }

#region Buttons Handlers
    void onTriggerButtonClicked(trTransition transition){
      if (TriggerButtonClicked != null){
        if (!elementInfoLongPressHappened) {
          TriggerButtonClicked(this, transition);
        }
      }
    }
    
    public void OnDeleteButtonClicked(trTransition transition){
      RemoveTransition(transition);
    }
#endregion

#region Trigger Button Long Press

    public void onPointerDown(trTriggerButtonViewHolder sender, PointerEventData eventData){
      if (!CanUserInteract() || (ProtoCtrl == null)) {
        return;
      }

      StartElementInfoLongPress(sender.TransitionData);
    }

    public void onPointerUp(trTriggerButtonViewHolder sender, PointerEventData eventData){
      if (!CanUserInteract() || (ProtoCtrl == null)) {
        return;
      }

      bool closeIt = (trElementInfoPanelController.behaviorOnPointerUp == trElementInfoPanelController.BehaviorOnPointerUp.CLOSE);
      CancelElementInfoLongPress(closeIt);
    }

    private trTransition longPressTransition;
    private bool elementInfoLongPressCanProceed;
    private bool elementInfoLongPressHappened = false;
    void StartElementInfoLongPress(trTransition trTrn) {
      longPressTransition = trTrn;
      elementInfoLongPressCanProceed = true;
      elementInfoLongPressHappened = false;
      StartCoroutine(crElementInfoLongPress());
    }

    void CancelElementInfoLongPress(bool closeElementInfo) {
      longPressTransition = null;
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
          if (longPressTransition != null) {
            ProtoCtrl.TryShowElementInfo(longPressTransition);
          }
          else {
            WWLog.logError("hm. no transition.");
          }
        }
      }
    }


#endregion

#region Trigger Button Drag Handlers
   
    void onStartDrag(trTriggerButtonViewHolder sender, PointerEventData eventData){
      if (!CanUserInteract()) { 
        return;
      }

      //change parent so that state button is on top of trash can
      Vector3 position = sender.transform.position;
      sender.itemDragged.transform.SetParent(ProtoCtrl.StateEditCtrl.StateDragPanel.transform, true);
      sender.itemDragged.transform.position = position;
    }
    
    void onEndDrag(trTriggerButtonViewHolder sender, PointerEventData eventData){
      if (!CanUserInteract()) { 
        return;
      }

      if(ProtoCtrl.TrashCanCtrl.IsInTrashCanArea(sender.itemDragged.transform.position)){
        // telemetry
        if (trDataManager.Instance.IsInNormalMissionMode) {
          new trTelemetryEvent(trTelemetryEventType.CHAL_DEL_CUE, true)
            .add(trTelemetryParamType.ROBOT_TYPE, ProtoCtrl.CurProgram.RobotType)
            .add(trTelemetryParamType.TYPE, sender.TransitionData.Trigger.Type)
            .add(trTelemetryParamType.CHALLENGE, trDataManager.Instance.MissionMng.GetCurMission().UserFacingName)
            .add(trTelemetryParamType.STEP, trDataManager.Instance.MissionMng.UserOverallProgress.CurMissionInfo.PuzzleIndex)
            .emit();
        }
        else {
          new trTelemetryEvent(trTelemetryEventType.FP_DEL_CUE, true)
            .add(trTelemetryParamType.ROBOT_TYPE, trDataManager.Instance.GetCurProgram().RobotType)
            .add(trTelemetryParamType.TYPE, sender.TransitionData.Trigger.Type)
            .emit();
        }
        RemoveTransition(sender.TransitionData);
        ProtoCtrl.StateEditCtrl.UpdateUndoRedoUserAction();
      }
      else{
        sender.itemDragged.gameObject.transform.SetParent(ButtonContainer.transform, false);
        sender.gameObject.transform.localScale = Vector3.one;
        sender.GoBackToStartLocalPos();
      }
      ProtoCtrl.TrashCanCtrl.SetAnimation(false);
    }
    
    void onDrag(trTriggerButtonViewHolder sender, PointerEventData eventData){
      if (!CanUserInteract()) { 
        return;
      }

      ProtoCtrl.TrashCanCtrl.IsInTrashCanArea(sender.itemDragged.transform.position);
    }
    public override void OnBeginDrag (PointerEventData eventData){}

    public override void OnEndDrag (PointerEventData eventData){}

    public override void OnDrag (PointerEventData eventData){}

#endregion

#region Layout Updating

    public void SetActiveDropOn(bool active){
      if(active){
        this.GetComponent<Animator>().enabled = false;
        TransitionLine.WidthOverall = 1.5f;
      }else{
        TransitionLine.WidthOverall = 0.5f;
        this.GetComponent<Animator>().enabled = true;
      }
    }

    public void LayoutTransition(){
      if (SourceButton == null || TargetButton == null) return;
      
      float sfTrg = scaleFactorTrigger();
      float sfDsp = sfTrg;
      
      TransitionLine.Displacement = sfDsp * getMaxDisplacementSize();
      TransitionLine.DoUpdate(false);  // force the internal state stuff to update.
    
		  setupTriggerButtons();

    }

    private float scaleFactorTrigger() {
      return scaleFactor(dragThreshold / 2, dragThreshold / 1);
    }
    
    private float scaleFactorCap() {
      return scaleFactor(dragThreshold / 4, dragThreshold / 2);
    }

    bool isTwoTransition(){
      for(int i=0; i<TargetButton.StateData.OutgoingTransitions.Count; i++){
        if(TargetButton.StateData.OutgoingTransitions[i].StateTarget == SourceButton.StateData){
          return true;
        }
      }
      return false;
    }
    
    float getMaxDisplacementSize(){
      return isTwoArrows ? 70 : 20;
    }

    void setCurveLineLocalScale(float scaleCap, float scaleTriggerButton){

      Vector3 shrinkVectorTrigger = Vector2.one * scaleTriggerButton;
      
      bool shouldShowGeneralIcon = (scaleTriggerButton < 0.5f);
      if (shouldShowGeneralIcon){
        shrinkVectorTrigger = Vector2.one * 0.5f;
      }
      
      foreach(trTriggerButtonViewHolder holder in TransitionToTriggerHolder.Values){

        holder.BehaviorContainer.SetActive(!shouldShowGeneralIcon);
      }
      
      // Update Trigger Icon
      ButtonContainer.localScale = shrinkVectorTrigger;
    }

    // TODO performance: this code runs all the time.
    void setupTriggerButtons(){

      if(TransitionToTriggerHolder.Count == 0){
        return;
      }

      float sourceButtonWidth = SourceButton.GetComponent<RectTransform>().GetWidth();
      Vector3 sourcePos  = preSourceLocalPos;
      Vector3 targetPos  = preTargetLocalPos;
      float lineTotal = (sourcePos - targetPos).magnitude;
      lineTotal = (lineTotal==0)?0.00001f:lineTotal; // avoid dividing by zero

      float triggerWidth = TransitionToTriggerHolder.First().Value.GetComponent<RectTransform>().GetWidth()*0.8f;
      float scaleFac = (lineTotal - sourceButtonWidth*1.4f)/(TransitionToTriggerHolder.Count*triggerWidth);
      scaleFac = Mathf.Clamp01(scaleFac);

      float thresh = 0.35f;

      bool shouldShowGeneralIcon = (scaleFac < thresh);
      if (shouldShowGeneralIcon){
        ButtonContainer.localScale = thresh*Vector3.one;
      }else{
        ButtonContainer.localScale = scaleFac*Vector3.one;
      }   
      triggerWidth *= scaleFac;

      foreach(trTriggerButtonViewHolder holder in TransitionToTriggerHolder.Values){
        if (!holder.isDragging){
          holder.BehaviorContainer.SetActive(!shouldShowGeneralIcon);
          
          float t = (float)(TransitionToTriggerHolder.Count - holder.index) * triggerWidth/lineTotal; // last added transition is close to source
          t += sourceButtonWidth/lineTotal; // avoid dingle
          
          Vector3 v1 = TransitionLine.transform.TransformPoint(TransitionLine.LocalPositionForNormalizedDistance(t       ));
          Vector3 v2 = TransitionLine.transform.TransformPoint(TransitionLine.LocalPositionForNormalizedDistance(t + 0.1f));
          v2.z = v1.z; // avoid wrong rotation around x,y axis
          holder.TriggerButton.transform.position = v1;
          holder.TriggerButton.transform.rotation = Quaternion.FromToRotation(Vector3.right, v2 - v1);          
          holder.LabelContainer.rotation = Quaternion.identity;
          holder.BehaviourImage.gameObject.transform.rotation = Quaternion.identity;
          holder.Questionmark.rotation = Quaternion.identity;
          holder.TriggerButton.transform.SetAsFirstSibling();
        }
      }
    }

    private float scaleFactor(float threshMin, float threshMax) {
      if (SourceButton == null || TargetButton == null) {
        return 1.0f;
      }
      
      Vector2 stateButtonSize = SourceButton.transform.GetComponent<RectTransform>().GetSize() / 2;
      Vector2 sourceDiff = SourceButton.transform.position - TargetButton.transform.position;
      float distance = sourceDiff.magnitude - stateButtonSize.magnitude;
      
      if (distance < threshMax) {
        return Mathf.Clamp(Mathf.InverseLerp(threshMin, threshMax, distance), 0, 1);
      }
      return 1.0f; // else
    }
#endregion

#region Mission
    public void ShowIncorrect(trTransition transition){
      StartCoroutine(showIncorrect(TransitionToTriggerHolder[transition]));
    }
    
    IEnumerator showIncorrect(trTriggerButtonViewHolder holder){
      this.transform.DOKill();
      this.transform.DOShakeScale(0.2f);
      holder.TriggerButton.GetComponent<Image>().color = Color.red;
      yield return new WaitForSeconds(0.2f);
      holder.TriggerButton.GetComponent<Image>().color = Color.white;
    }
#endregion

#region Internal
    public void SetEnableDeleteFocus(bool isEnabled){
      foreach(trTriggerButtonViewHolder holder in TransitionToTriggerHolder.Values){
        holder.TriggerButton.gameObject.SetActive(!isEnabled);
      }
    }

    private void playTransitionActivationAnimation(){
      SoundManager.soundManager.PlaySound(SoundManager.trAppSound.TRANSITION_STATES);
      TransitionLine.trigger();
      if (TriggerFiredAnimator != null){
        TriggerFiredAnimator.Play("Transition");
      }
    }

    void updateStateScale(Transform targetTransform, float scale){
      if (targetTransform.localScale.x != scale){
        Vector3 scale3 = targetTransform.localScale;
        scale3.x = scale;
        scale3.y = scale;
        scale3.z = scale;
        targetTransform.localScale = scale3;
      }
    }

    private float getConcreteValueForTransitionTreshold(){
      trMultivariate.trAppOptionValue optionValue = trMultivariate.Instance.getOptionValue(trMultivariate.trAppOption.TRANSITION_SIMPLIFICATION_THRESHOLD);
      float result = 0;
      switch (optionValue){
      case trMultivariate.trAppOptionValue.SMALL:
        result = 70;
        break;
      case trMultivariate.trAppOptionValue.MEDIUM:
        result = 90;
        break;
      case trMultivariate.trAppOptionValue.BIG:
        result = 110;
        break;
      }
      return result;
    }
#endregion
  }
}
