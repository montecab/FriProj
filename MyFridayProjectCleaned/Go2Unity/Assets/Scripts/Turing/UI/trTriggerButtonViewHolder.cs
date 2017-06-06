using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using WW.UGUI;
using UnityEngine.EventSystems;
using Turing;
using TMPro;
using DG.Tweening;

public class trTriggerButtonViewHolder : uGUIDragDrop, IPointerDownHandler, IPointerUpHandler {

  public Transform LabelContainer;
  public Image BehaviourImage;
  public GameObject BehaviorContainer;
  public TextMeshProUGUI Label;
  public Image ErrorImage;
  public Button TriggerButton;
  public int index = 1;
  public trTransition TransitionData;
  public RectTransform TriangleBG;
  public Button PropagateButton;

  public delegate void DragAndPointerDelegate(trTriggerButtonViewHolder sender, PointerEventData eventData);

  public DragAndPointerDelegate onStartDrag;
  public DragAndPointerDelegate onDrag;
  public DragAndPointerDelegate onEndDrag;
  public DragAndPointerDelegate onPointerDown;
  public DragAndPointerDelegate onPointerUp;
  public trTransitionArrowController ParentCtrl;
  public Image ObscureImage;
  public Image WarningImage;
  public Transform Questionmark;
  public Toggle ObscureToggle;
  public bool IsShowObscure = false;

  private static Material labelMaterial = null;
  private Material LabelMaterial {
    get {
      if (labelMaterial == null) {
        labelMaterial = Resources.Load<Material>("Fonts/bariol-regular SDF 4");
      }

      return labelMaterial;
    }
  }

  public bool isDragging {
    get {
      return itemDragged != null;
    }
  }

  void Start(){
    PropagateButton.onClick.AddListener(onTriggerPropagateButtonClicked);
    ObscureToggle.onValueChanged.AddListener(onObscureToggled);
  }

  void OnDestory(){
    PropagateButton.onClick.RemoveListener(onTriggerPropagateButtonClicked);
    ObscureToggle.onValueChanged.RemoveListener(onObscureToggled);
  }

  public void SetUpView(){
    trTriggerType trigType = trTriggerType.BUTTON_MAIN;
    if (TransitionData != null && TransitionData.Trigger != null) {
      trigType = TransitionData.Trigger.Type;
    }

    BehaviourImage.sprite = trIconFactory.GetIcon(trigType);

    if(trTrigger.Parameterized(TransitionData.Trigger.Type)){
      Label.gameObject.SetActive(true);
      Label.text = trStringFactory.GetParaString(TransitionData.Trigger);
      // I couldn't figure out how to assign the material in the scene.
      Label.fontMaterial = LabelMaterial;
    }else{
      Label.gameObject.SetActive(false);
    }

    bool showObscure = TransitionData.IsObscured && IsShowObscure;
    ObscureImage.gameObject.SetActive(showObscure);
    BehaviourImage.gameObject.SetActive(!showObscure);
    BehaviorContainer.SetActive(!showObscure);

    if(trTrigger.Parameterized(TransitionData.Trigger.Type)){
      Label.gameObject.SetActive(!showObscure);
    }

    ObscureToggle.isOn = TransitionData.IsObscured;

    bool showWarning = TransitionData.Trigger.Type.IsMicrophone() && TransitionData.StateSource.Behavior.ShowMicrophoneWarning();
    WarningImage.gameObject.SetActive(showWarning);
   
  }

  public void PopLabel(){
    Label.transform.DOKill();
    Label.transform.localScale = Vector3.one;
    Label.transform.DOShakeScale(0.5f);
  }

  void onObscureToggled(bool isOn){
    TransitionData.IsObscured = isOn;
    trDataManager.Instance.SaveCurProgram();
  }

  void onTriggerPropagateButtonClicked(){
    ParentCtrl.ProtoCtrl.missionAuthoringPanel.PropgCtrl.showPropagatePanel(TransitionData);
  }


  #region Drag Handlers
  public override void OnBeginDrag (PointerEventData eventData)
  {
    if (!enabled){ 
      return; 
    }

    base.OnBeginDrag (eventData);
    if (onStartDrag != null){
      onStartDrag(this, eventData);
    }
  }
  
  public override void OnEndDrag (PointerEventData eventData)
  { 
    if (!enabled){ 
      return; 
    }

    if (onEndDrag != null){
      onEndDrag(this, eventData);
    }
    base.OnEndDrag (eventData);
    
  }
  
  public override void OnDrag (PointerEventData eventData)
  {
    if (!enabled){ 
      return; 
    }

    base.OnDrag (eventData);
    if (onDrag != null){
      onDrag(this, eventData);
    }
  }
  #endregion

  #region Pointer handlers
  public void OnPointerDown(PointerEventData ped) {
    if (!enabled) {
      // don't know what this is for, but copying OnDrag.
      return;
    }

    if (onPointerDown != null) {
      onPointerDown(this, ped);
    }
  }
  public void OnPointerUp(PointerEventData ped) {
    if (!enabled) {
      // don't know what this is for, but copying OnDrag.
      return;
    }

    if (onPointerUp != null) {
      onPointerUp(this, ped);
    }
  }
  #endregion
}
