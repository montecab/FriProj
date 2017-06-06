using UnityEngine;
using System.Collections;
using WW.UGUI;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Turing{
  public class trButtonControllerBase :  uGUIDragDrop {
    public trProtoController ProtoCtrl;
    private bool canUserInteract = true; // user can interact by default

    public virtual void SetUpView(){
      canUserInteract = true;
    }

    public virtual void EnableUserInteraction(bool interaction){
      canUserInteract = interaction;
    }

    protected bool CanUserInteract(){
      return canUserInteract;
    }

    // #region IPointerHandler, IDropHandler, IDragHandler methods to be implemented!
    // public void OnPointerClick(PointerEventData eventData){
    //   // this is called also after drag/drop, so prevent double calling!
    //   if (CanUserInteract() && !eventData.dragging){
    //     OnButtonPointerClick(eventData);
    //   } 
    // } 
    // protected virtual void OnButtonPointerClick(PointerEventData eventData){
    //   Debug.Log("OnPointerClick triggered but not implemented by child class");
    // }

    // public void OnPointerDown(PointerEventData eventData){
    //   if (CanUserInteract()) OnButtonPointerDown(eventData);
    // } 
    // protected virtual void OnButtonPointerDown(PointerEventData eventData){  
    //   Debug.Log("OnPointerDown triggered but not implemented by child class");    
    // }

    // public void OnPointerUp(PointerEventData eventData){
    //   if (CanUserInteract()) OnButtonPointerUp(eventData);
    // } 
    // protected virtual void OnButtonPointerUp(PointerEventData eventData){
    //   Debug.Log("OnPointerUp triggered but not implemented by child class");   
    // } 

    // public void OnDrop(PointerEventData eventData){
    //   if (CanUserInteract()) OnButtonDrop(eventData);
    // } 
    // protected virtual void OnButtonDrop(PointerEventData eventData){    
    //   Debug.Log("OnDrop triggered but not implemented by child class");     
    // }

    // public void OnBeginDrag(PointerEventData eventData){
    //   if (CanUserInteract()) {
    //     base.OnBeginDrag(eventData);
    //     OnButtonBeginDrag(eventData);
    //   }
    // }
    // protected virtual void OnButtonBeginDrag(PointerEventData eventData){
    //   Debug.Log("OnBeginDrag triggered but not implemented by child class");           
    // }

    // public void OnDrag(PointerEventData eventData){
    //   if (CanUserInteract()) {
    //     base.OnDrag(eventData);
    //     OnButtonDrag(eventData);
    //   }
    // }
    // protected virtual void OnButtonDrag(PointerEventData eventData){
    //   Debug.Log("OnDrag triggered but not implemented by child class");           
    // }

    // public void OnEndDrag(PointerEventData eventData){
    //   if (CanUserInteract()) {
    //     base.OnEndDrag(eventData);
    //     OnButtonEndDrag(eventData);
    //   }
    // }
    // protected virtual void OnButtonEndDrag(PointerEventData eventData){
    //   Debug.Log("OnEndDrag triggered but not implemented by child class");           
    // }
    // #endregion
  }
}
