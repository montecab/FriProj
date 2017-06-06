using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Turing;
using WW.UGUI;
using TMPro;

public class trBehaviorButtonController : uGUICreateDragDrop, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler{
  public trProtoController ProtoCtrl;

  private trBehavior behavior;
  public trBehavior BehaviorData{
    set{
      if(behavior == value){
        return;
      }
      behavior = value;
      SetUpView();
    }
    get{
      return behavior;
    }
  }

  public Image Image;
  public TextMeshProUGUI Label;
  public Image LockImage;

  public GameObject HosterGameObject;
  public GameObject LockedModal;
  private ScrollRect scrollRect;
  private bool shouldListenHorizontalSwipe = false;
  private bool isButtonEnabled = true;

  public delegate void OnClickDelegate(trBehaviorButtonController button);
  public OnClickDelegate OnClickListeners;
  public OnClickDelegate OnUsageChanged;

  public Toggle MissionShowToggle; // the toggle to control if the behavior will show in the behavior panel during a puzzle

  public bool IsActive {
    get {
      return (draggingObj != null && draggingObj.activeSelf);
    }
  }

  void Start(){
    MissionShowToggle.onValueChanged.AddListener(onToggle);
    scrollRect = HosterGameObject.GetComponentInChildren<ScrollRect>();
  }

  public void SetEnabled(bool enabled){
    isButtonEnabled = enabled;
    float alpha = (enabled ? 1 : 0.7f);
    Image.color = wwColorUtil.ColorWithAlpha(Image.color, alpha);
  }

  public void SetActiveToggle(bool active){
    MissionShowToggle.gameObject.SetActive(active);
    if(active){
      bool isOn = trDataManager.Instance.MissionMng.GetCurPuzzle().UUIDToBehaviorDic.ContainsKey(BehaviorData.UUID);
      MissionShowToggle.isOn = isOn;
    }   
  }

  void onToggle(bool value){
    if(value && !trDataManager.Instance.MissionMng.GetCurPuzzle().UUIDToBehaviorDic.ContainsKey(BehaviorData.UUID)){
      trDataManager.Instance.MissionMng.GetCurPuzzle().UUIDToBehaviorDic.Add(BehaviorData.UUID, BehaviorData);
      trDataManager.Instance.MissionMng.AuthoringMissionInfo.SaveCurMission();
    }

    if(!value && trDataManager.Instance.MissionMng.GetCurPuzzle().UUIDToBehaviorDic.ContainsKey(BehaviorData.UUID)){
      trDataManager.Instance.MissionMng.GetCurPuzzle().UUIDToBehaviorDic.Remove(BehaviorData.UUID);
      trDataManager.Instance.MissionMng.AuthoringMissionInfo.SaveCurMission();
    }
  }

  public void SetUpView(){
    
      Sprite sprite = null;
      bool nameLabelEnabled = (behavior.Type == trBehaviorType.MAPSET || behavior.IsMissionBehavior);

      if (behavior.Type == trBehaviorType.MAPSET){
        sprite = trIconFactory.GetIcon(behavior.MapSet.IconName);
      }  
      else if(behavior.IsMissionBehavior){
        Label.color = Color.white;
        sprite = trIconFactory.GetMissionIcon(behavior.MissionFileInfo.IconName);       
      }
      else if (behavior.Type == trBehaviorType.MOODY_ANIMATION) {
        //trMoodyAnimation animation = (trMoodyAnimation)behavior;        
        sprite = trMoodyAnimations.Instance.GetIcon(behavior.Animation);
      }
	    else {
        sprite = trIconFactory.GetIcon(behavior.Type);
      }

      Label.text = behavior.UserFacingNameLocalized;

      Label.gameObject.SetActive(nameLabelEnabled);

      LockImage.gameObject.SetActive(behavior.isLocked());
      Image.sprite = sprite;
      Color c = Image.color;
      c.a = behavior.isLocked() ? 0.25f : 1.0f;
      Image.color = c;
  }

  void setDraggingTextEnabled(bool flag){
    if (draggingObj != null){
      trBehaviorButtonController draggerController = draggingObj.GetComponent<trBehaviorButtonController>();
      if (draggerController.Label != null){
        draggerController.Label.gameObject.SetActive(flag);
      }
    }
  }

  #region IPointerHandler implementation
  public void OnPointerClick (PointerEventData eventData)
  {
    if(!eventData.dragging){   
      SoundManager.soundManager.PlaySound(SoundManager.trAppSound.UI_SOUND);
      if (behavior.isLocked()){
        LockedModal.SetActive(true);
      }
      else if(OnClickListeners != null){
        OnClickListeners(this);
      }

    }
  }

  public void OnPointerUp(PointerEventData eventData){
    if(shouldListenHorizontalSwipe && draggingObj != null){
      Destroy(draggingObj);
    }
  }

  public void OnPointerDown(PointerEventData eventData){
    if (!isButtonEnabled) return;

    if (behavior.isLocked()){
      LockedModal.SetActive(true);
    }
    else{
      base.OnBeginDrag (eventData);
      draggingObj.GetComponent<trButtonTween>().TweenDragged();
      shouldListenHorizontalSwipe = true;
      if (scrollRect != null){
        scrollRect.SendMessage("OnBeginDrag", eventData);
      }
      setDraggingTextEnabled(true);

      if (OnUsageChanged != null){
        OnUsageChanged(this);
      }

      // todo: it might be nice if this only happened if the behavior is being dragged out of the actions tray.
      ProtoCtrl.HideElementInfo();
    }  
  }
  #endregion

  public override void OnBeginDrag (PointerEventData eventData)
  {
    // Moved the logic to OnPointerDown
  }

  public override void OnDrag (PointerEventData data)
  {
    if (!isButtonEnabled) return;

    if (behavior.isLocked()){
      LockedModal.SetActive(true);
    }
    else{
      if (shouldListenHorizontalSwipe && scrollRect != null && RectTransformUtility.RectangleContainsScreenPoint(HosterGameObject.GetComponent<RectTransform>(), data.position, Camera.main)){
        Vector2 diff = data.position - data.pressPosition;
        if (Mathf.Abs(diff.x)/ Mathf.Abs(diff.y) > 6.0f) {   
          scrollRect.SendMessage("OnDrag", data);
          draggingObj.GetComponent<trButtonTween>().TweenDrop();
          draggingObj.SetActive(false);
          return;
        }
      } else {
        shouldListenHorizontalSwipe = false;
      }

      base.OnDrag (data);
      if(BehaviorData.Type.IsDeletable() && IsActive){
        ProtoCtrl.TrashCanCtrl.IsInTrashCanArea(draggingObj.transform.position);
      }

     // ProtoCtrl.StateEditCtrl.CheckDropOnTransition(draggingObj.transform.position);
    }
  }

  void OnDisable(){
    if(draggingObj != null){
      Destroy(draggingObj);
    }
  }
  
  public override void OnEndDrag (PointerEventData eventData)
  {
    base.OnEndDrag (eventData);
    if(draggingObj != null){
      Destroy(draggingObj);
    }
    if(IsActive && BehaviorData.Type.IsDeletable()
       &&ProtoCtrl.TrashCanCtrl.IsInTrashCanArea(draggingObj.transform.position)){
      ProtoCtrl.DeleteBehavior(this);
    }
    ProtoCtrl.TrashCanCtrl.SetAnimation(false);
    if (scrollRect != null){
      scrollRect.SendMessage("OnEndDrag", eventData);
    }
    bool nameLabelEnabled = (behavior.Type == trBehaviorType.MAPSET || behavior.isSoundBehaviour()|| behavior.IsMissionBehavior);
    setDraggingTextEnabled(nameLabelEnabled);
    draggingObj = null;
    if (OnUsageChanged != null){
      OnUsageChanged(this);
    }
  }

}
