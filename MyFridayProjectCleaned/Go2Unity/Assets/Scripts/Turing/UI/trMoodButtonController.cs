using UnityEngine;
using System.Collections;
using WW.UGUI;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Turing{
  public class trMoodButtonController :  uGUIDragDrop, IPointerDownHandler{
    public Image Img;
    public trStateButtonController StateCtrl;
    public trProtoController ProtoCtrl;

    private Vector3 originalPosition;

    public void SetUpView(){
      if(StateCtrl.StateData.Mood != trMoodType.NO_CHANGE){
        this.gameObject.SetActive(true);
        Img.sprite = trIconFactory.GetIcon(StateCtrl.StateData.Mood);
      }else{
        this.gameObject.SetActive(false);
      }
    }

    public override void OnBeginDrag (UnityEngine.EventSystems.PointerEventData eventData)
    {
      if(ProtoCtrl == null){
        return;
      }
      base.OnBeginDrag (eventData);
      originalPosition = this.transform.localPosition; // remember original position.  This has to be done before the SetParent since we are using localPosition            
      this.gameObject.transform.SetParent(ProtoCtrl.StateEditCtrl.StateDragPanel.transform); //change parent so that state button is on top of trash can
    }

    public override void OnDrag (PointerEventData eventData)
    {
      if(ProtoCtrl == null){
        return;
      }
      base.OnDrag (eventData);
      this.gameObject.transform.localPosition = new Vector3(this.gameObject.transform.localPosition.x,
                                                            this.gameObject.transform.localPosition.y,
                                                            0);
     
      ProtoCtrl.TrashCanCtrl.IsInTrashCanArea(this.gameObject.transform.position);
    }

    public override void OnEndDrag (PointerEventData eventData)
    {
      if(ProtoCtrl == null){
        return;
      }
      if(ProtoCtrl.TrashCanCtrl.IsInTrashCanArea(this.gameObject.transform.position)){
        StateCtrl.StateData.Mood = trMoodType.NO_CHANGE;
        trDataManager.Instance.SaveCurProgram();
      }
      ProtoCtrl.TrashCanCtrl.SetAnimation(false);
      this.gameObject.transform.SetParent(StateCtrl.gameObject.transform);
      this.gameObject.transform.localPosition = new Vector3(this.gameObject.transform.localPosition.x,
                                                            this.gameObject.transform.localPosition.y,
                                                           0);
      
      base.OnEndDrag (eventData);
      SetUpView();
      this.transform.localPosition = originalPosition; // go back to its original position
    }


    #region IPointerDownHandler implementation
    public void OnPointerDown (PointerEventData eventData)
    {
      // Add this so clicking on mood button doesn't make state button tween
    }
    #endregion
  	
  }
}
