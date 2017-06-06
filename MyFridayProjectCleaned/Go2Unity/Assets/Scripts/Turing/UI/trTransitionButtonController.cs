using UnityEngine;
using System.Collections;
using WW.UGUI;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Turing{
  public class trTransitionButtonController : trButtonControllerBase, IPointerDownHandler{
    public trStateButtonController StateCtrl;
    public GameObject TransitionDottedLine;

    public delegate void TransitionButtonDelegate(trStateButtonController state, trTransitionButtonController ctrl);
    public TransitionButtonDelegate ClickListeners;
    public TransitionButtonDelegate onSelectTransitionButton;
    public TransitionButtonDelegate onDragTransitionButton;
    public TransitionButtonDelegate onDropTransitionButton;

    public GameObject DingleHandle;

    private Vector3 originalPosition;

    public override void SetUpView(){
      base.SetUpView();
    }

    void OnEnable(){
      if(DingleHandle){
         DingleHandle.SetActive(true);
      }
    }

    void OnDisable(){
      if(DingleHandle){
        DingleHandle.SetActive(false);
       }
    }

    public override void EnableUserInteraction(bool interaction){
      base.EnableUserInteraction(interaction);
      enabled = interaction;
    }

    public override void OnBeginDrag(PointerEventData eventData){
      if (CanUserInteract()){
        base.OnBeginDrag(eventData);
        originalPosition = transform.localPosition; // remember original position.  This has to be done before the SetParent since we are using localPosition      
        gameObject.transform.SetParent(ProtoCtrl.StateEditCtrl.StateDragPanel.transform); //change parent so that state button is on top of trash can
        enableUserInteractionForOtherStates(false);
        TransitionDottedLine.SetActive(true);
        DingleHandle.SetActive(false);
      }
    }

    public override void OnDrag (PointerEventData eventData){
      if (CanUserInteract()){
        base.OnDrag(eventData);
        gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x,
                                                        gameObject.transform.localPosition.y,
                                                        0);

        if (onDragTransitionButton != null) onDragTransitionButton(StateCtrl, this);
       
        ProtoCtrl.TrashCanCtrl.IsInTrashCanArea(gameObject.transform.position);
      }
    }

    public override void OnEndDrag (PointerEventData eventData){
      if (CanUserInteract()){
        if(ProtoCtrl.TrashCanCtrl.IsInTrashCanArea(gameObject.transform.position)){
          StateCtrl.StateData.Mood = trMoodType.NO_CHANGE;
          trDataManager.Instance.SaveCurProgram();
        }
        ProtoCtrl.TrashCanCtrl.SetAnimation(false);
        DingleHandle.SetActive(true);
        gameObject.transform.SetParent(StateCtrl.gameObject.transform, false);
        gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x,
                                                          gameObject.transform.localPosition.y,
                                                         0);
        
        base.OnEndDrag (eventData);
        enableUserInteractionForOtherStates(true);
        TransitionDottedLine.SetActive(false);

        if (onDropTransitionButton != null) onDropTransitionButton(StateCtrl, this);

        SetUpView();
        transform.localPosition = originalPosition; // go back to its original position
        transform.localScale = Vector3.one;
      }
    }

    private void enableUserInteractionForOtherStates(bool toEnable){
      foreach(trStateButtonController stateButton in ProtoCtrl.StateEditCtrl.StateToButtonTable.Values){
        if (!stateButton.Equals(StateCtrl)) {
          stateButton.EnableUserInteraction(toEnable);
        }
      }
    }
        
    public void OnPointerDown (PointerEventData eventData){
      // Add this so clicking on transition button doesn't make state button tween
    }
  }
}
