using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Turing;
using DG.Tweening;

public class trCenterScrollOnDoubleTab : MonoBehaviour, IPointerClickHandler {

  private float firstClickTime = float.NaN;
  private const float ACTIVATION_TIME = 0.4f; //seconds

  public ScrollRect TargetScrollRect;
  public trProtoController ProtoController;

  private IEnumerator currentEnumerator;

  public void OnPointerClick (PointerEventData eventData) {
    if (currentEnumerator != null){
      StopCoroutine(currentEnumerator);
      currentEnumerator = null;
    }

    if (float.IsNaN(firstClickTime) || Time.fixedTime - firstClickTime > ACTIVATION_TIME){
      firstClickTime = Time.fixedTime;
      currentEnumerator = toggleBehaviourPanelDelayed();
      StartCoroutine(currentEnumerator);
    } else {
      centerStateMachine();
      firstClickTime = float.NaN;
    }
  }

  IEnumerator toggleBehaviourPanelDelayed(){
    yield return new WaitForSeconds(ACTIVATION_TIME);
    ProtoController.BehaviorPanelCtrl.UsageController.ForceToggle();
    ProtoController.HideElementInfo();
  }

  public void centerStateMachine(bool isImmediate = false){
    float minX = float.NaN;
    float maxX = float.NaN;
    float minY = float.NaN;
    float maxY = float.NaN;
    foreach(trStateButtonController state in ProtoController.StateEditCtrl.StateToButtonTable.Values){
      if(float.IsNaN(minX) || minX > state.gameObject.transform.localPosition.x){
        minX = state.gameObject.transform.localPosition.x;
      }

      if(float.IsNaN(minY) ||minY > state.gameObject.transform.localPosition.y){
        minY = state.gameObject.transform.localPosition.y;
      }

      if(float.IsNaN(maxX) ||maxX < state.gameObject.transform.localPosition.x){
        maxX = state.gameObject.transform.localPosition.x;
      }

      if(float.IsNaN(maxY) ||maxY < state.gameObject.transform.localPosition.y){
        maxY = state.gameObject.transform.localPosition.y;
      }
    }

    Vector3 center = new Vector3((minX + maxX)/2.0f, (minY + maxY)/2.0f, 0);
    if(!isImmediate){
      this.gameObject.transform.DOKill();
      this.gameObject.transform.DOLocalMove(-center, 0.2f, true);
    }
    else{
      this.gameObject.transform.localPosition = -center;
    }

  }
	
}
