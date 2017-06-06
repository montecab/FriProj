using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using Turing;

public class trStatePanelDropController : MonoBehaviour, IDropHandler, IPointerClickHandler {

 
  public trProtoController ProtoCtrl;

  #region IDropHandler implementation
 
  public void OnDrop (PointerEventData eventData)
  {
    GameObject obj = eventData.pointerDrag;
    if(obj!= null){      
      trBehaviorButtonController behaviorButton = obj.GetComponent<trBehaviorButtonController>();
      if(behaviorButton != null && behaviorButton.IsActive){   
        Vector3 globalMousePos;
        if(ProtoCtrl.AllowedScrollRect == null){
          ProtoCtrl.StateEditCtrl.AddState(eventData.position, behaviorButton.BehaviorData);
        }
        else if (ProtoCtrl.AllowedScrollRect.CoordinateInScrollView(eventData.position, out globalMousePos))
        {
          ProtoCtrl.StateEditCtrl.AddState(new Vector2(globalMousePos.x, globalMousePos.y), behaviorButton.BehaviorData);
        }

      }
    }
  }

  #endregion

  #region IPointerClickHandler implementation

  public void OnPointerClick (PointerEventData eventData)
  {
    if(!eventData.dragging){
      //ProtoCtrl.StateEditCtrl.ShowAllTransitions();
      ProtoCtrl.StopRunningState();
    }
  }

  #endregion
}
