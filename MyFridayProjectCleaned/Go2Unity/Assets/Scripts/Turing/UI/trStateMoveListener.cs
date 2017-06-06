using UnityEngine;
using System.Collections;
using Turing;
using System.Collections.Generic;
using UnityEngine.UI;

public class trStateMoveListener : MonoBehaviour {

  public Dictionary<trState, trStateButtonController> StateToButtonTable;
	//private trStateButtonController targetState = null;
  private trStateButtonController sourceState = null;
  //private float distanceToCreateTransition;

  public delegate void CreateTransitionDelegate(trStateButtonController startButton, trStateButtonController targetButton);
  public CreateTransitionDelegate onCreateTransition;

  public ScrollRect StatesScrollView;
  public trScrollViewLimiter LimiterRect;
  public trDragDeleteController TrashCanCtrl;

  void Update(){
    if (sourceState != null){
      initiateScrollIfNearEdge(sourceState.transform.position);
    }
  }

  public void OnStateDrag(trStateButtonController button){
    if (sourceState == null) {
      sourceState = button;
    }

//    sourceState.ProtoCtrl.HideElementInfo();

    // distanceToCreateTransition = getConfigurationTreshold();

    // if (sourceState == null){
    //   sourceState = button;
    // }

    // trStateButtonController foundButton = getNearestStateButton(button, distanceToCreateTransition);
    // if (foundButton == null){
    //   if (targetState != null){
    //     targetState.SetSelectedForTransition(false);
    //   }
    // } else {
    //   if (targetState != foundButton){
    //     if (targetState != null){
    //       targetState.SetSelectedForTransition(false);
    //     }
    //     foundButton.SetSelectedForTransition(true);
    //   }
    // }
    // targetState = foundButton;
    // setupLinkImagePosition(button, targetState);
  }

  public void OnStateButtonDrop(trStateButtonController button){
    // sourceState = null;

    // if (targetState != null){
    //   if (onCreateTransition != null){
    //     onCreateTransition(button, targetState);
    //   }
    //   targetState.SetSelectedForTransition(false);
    //   button.GoBackToStartPos();
    //   targetState = null;
    // }
    // button.CreateLinkImage.gameObject.SetActive(false);
    
    trStateButtonController foundButton = getNearestStateButton(button, 40);
    if (foundButton != null) {
      Debug.Log("on top of button, go back to original place");
      sourceState.GoBackToStartWorldPos();
    }
    sourceState = null;
  }

  // private void setupLinkImagePosition(trStateButtonController source, trStateButtonController target){
  //   bool isLinkImageVisible = (target != null);
  //   source.CreateLinkImage.gameObject.SetActive(isLinkImageVisible);
  //   if (isLinkImageVisible){
  //     source.CreateLinkImage.transform.localPosition = -(source.transform.position - target.transform.position) / 2;
  //   }
  // }


  private trStateButtonController getNearestStateButton(trStateButtonController source, float threshold){
    trStateButtonController result = null;
    float minDistanceSquare = float.NaN;
    foreach(trStateButtonController button in StateToButtonTable.Values){
      if (button == source) continue;

      float distance = (button.transform.position - source.transform.position).sqrMagnitude;
      if (float.IsNaN(minDistanceSquare) ||distance < minDistanceSquare){
        minDistanceSquare = distance;
        result = button;
      }
    }

    if (Mathf.Sqrt(minDistanceSquare) > threshold){
      result = null;
    }
    return result;
  }

  private float getConfigurationTreshold(){
    float result = 0;
    trMultivariate.trAppOptionValue value = trMultivariate.Instance.getOptionValue(trMultivariate.trAppOption.DISTANCE_CREATE_TRANSITION);
    switch(value){
      case trMultivariate.trAppOptionValue.BIG:
        result = 110;
        break;
      case trMultivariate.trAppOptionValue.MEDIUM:
        result = 80;
        break;
      case trMultivariate.trAppOptionValue.SMALL:
        result = 50;
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
