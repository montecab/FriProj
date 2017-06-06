using UnityEngine;
using System.Collections;
using Turing;
using System.Collections.Generic;
using UnityEngine.UI;

public class trTransitionButtonMoveListener : MonoBehaviour {

  private trStateButtonController targetState = null;
  private trStateButtonController sourceState = null;
  private trTransitionButtonController currentTransition = null;   

  public delegate void CreateTransitionDelegate(trStateButtonController startButton, trStateButtonController targetButton, bool isUserAction);
  public CreateTransitionDelegate onCreateTransition;

  public ScrollRect StatesScrollView;
  public trScrollViewLimiter LimiterRect;
  public trDragDeleteController TrashCanCtrl;
  public Dictionary<trState, trStateButtonController> StateToButtonTable;

  void Update(){
    if (currentTransition != null){
      initiateScrollIfNearEdge(currentTransition.transform.position);
    }
  }

  private bool isDragging = false;
  private IEnumerator playConnectionSound() {
    isDragging = true;
    SoundManager.soundManager.PlaySound(SoundManager.trAppSound.CONNECT_STATES_START);
    yield return new WaitForSeconds(4.0f);
    if (isDragging) {
      SoundManager.soundManager.StopAllSound();
      SoundManager.soundManager.PlaySound(SoundManager.trAppSound.CONNECT_STATES_LOOP);
    }
  }
  public void OnTransitionButtonDrag(trStateButtonController state, trTransitionButtonController transition){   
    if (currentTransition == null){
      // initial drag, initialize stuff
      currentTransition = transition;
      sourceState = state;
      StartCoroutine(playConnectionSound());
    } 

    if (transition != null) {
      transition.ProtoCtrl.HideElementInfo();
    }

    trStateButtonController possibleTargetState = getNearestStateButton(transition);
    if (possibleTargetState == null){
      if (targetState != null){
        // previous target state is no longer valid
        targetState.SetSelectedForTransition(false);
        targetState = null;
      }
    }
    else {
      if (!possibleTargetState.Equals(targetState)){
        if (targetState != null){
          // previous target state is no longer valid
          targetState.SetSelectedForTransition(false);
          targetState = null;
        }
        // found a new potential button to create transition with!
        targetState = possibleTargetState;
        targetState.SetSelectedForTransition(true);
      }
    }
  }

  public void OnTransitionButtonDrop(trStateButtonController state, trTransitionButtonController transition){
    isDragging = false;
    SoundManager.soundManager.StopAllSound();
    if (targetState != null){
      if (onCreateTransition != null) {
        onCreateTransition(sourceState, targetState, true);
      }
      SoundManager.soundManager.PlaySound(SoundManager.trAppSound.CONNECT_STATES_END);
      targetState.SetSelectedForTransition(false);
      targetState = null;
    }
    sourceState = null;
    currentTransition = null;
  }


  private trStateButtonController getNearestStateButton(trTransitionButtonController transition){
    trStateButtonController result = null;
    float minDistSqr = float.NaN;
    // find the nearest state for transition
    foreach(trStateButtonController button in StateToButtonTable.Values){
      if (!button.Equals(sourceState)){
        Vector3 vTrnBut = button.transform.position - transition.transform.position;
        vTrnBut.z = 0;  // TUR-1641
        float distSqr = vTrnBut.sqrMagnitude;
        if (float.IsNaN(minDistSqr) ||distSqr < minDistSqr){
          minDistSqr = distSqr;
          result = button;
        }        
      }
    }

    if (result != null) {
      float threshold = getConfigurationTreshold();
      float actualDist = Mathf.Sqrt(minDistSqr);
      if (actualDist > threshold) {
        result = null; // selected button doesn't pass threshold requirement
      }
    }
    
    return result;
  }

  private float getConfigurationTreshold(){
    float result = 0;
    trMultivariate.trAppOptionValue value = trMultivariate.Instance.getOptionValue(trMultivariate.trAppOption.DISTANCE_CREATE_TRANSITION);
    switch(value){
      case trMultivariate.trAppOptionValue.BIG:
        result = 100;
        break;
      case trMultivariate.trAppOptionValue.MEDIUM:
        result = 70;
        break;
      case trMultivariate.trAppOptionValue.SMALL:
        result = 40;
        break;
    }
    return result * StatesScrollView.content.localScale.x;
  }

  void initiateScrollIfNearEdge(Vector3 position){
    Vector2 inMarginDist = Vector2.zero;
    
    if (!TrashCanCtrl.IsInTrashCanArea(position)){
      Vector3 positionInView;
      LimiterRect.PositionInRect(position, out positionInView);
      Vector2 pt = new Vector2(positionInView.x, positionInView.y) + LimiterRect.Rect.min;
      
      inMarginDist = WW.wwUtility.percentageInMargin(pt, LimiterRect.Rect, trProtoController.DRAGGING_MARGIN);
    }
    
    StatesScrollView.velocity = inMarginDist * -trProtoController.DRAGGING_SPEED_MAX;
  }
}
