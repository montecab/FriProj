using UnityEngine;
using System.Collections;
using System;
using Turing;
using WW.UGUI;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class trAnimationConfigurePanelController : uGUISegmentedController {

  public delegate void EditingFinishedDelegate(trState state, bool success);
  public EditingFinishedDelegate OnEditingFinished;
  public bool IsNewBehavior = false;
  public trProtoController protoController{get; set;}

  [SerializeField]
  private TextMeshProUGUI _titleText;
  [SerializeField]
  private GameObject _itemPrefab;
  [SerializeField]
  private HorizontalLayoutGroup _itemsContainer;
  [SerializeField]
  private Button _closeButton;
  [SerializeField]
  private Button _background;
  [SerializeField]
  private Image _categoryImage;
  [SerializeField]
  private GameObject _animationButtonPrefab;

  private trMoodType _stateAnimMoodStarted = trMoodType.NO_CHANGE;
  private GameObject _selectedGameObject;
  private trState _stateData;
  private trStateButtonController _stateButton;
  private bool _isPanelJustOpen = true;
 
  private void Awake(){
    _closeButton.onClick.AddListener(ClosePanel);
    _background.onClick.AddListener(ClosePanel);
  }

  private void Start () {
	  SetupView();
	}

  public void SetStateData(trStateButtonController stateButtonCtrl, piRobotType robotType){
    this._stateButton = stateButtonCtrl;
    this._stateData = this._stateButton.StateData;
    _stateAnimMoodStarted = _stateData.Mood;
    SetupView();
  }

  private void SetupView(){
    trMoodyAnimation animation = null;
    animation = _stateData.Behavior.Animation;
    if(animation == null) {
      animation = trMoodyAnimations.Instance.getAnimation((uint)_stateData.BehaviorParameterValue);
    }
    _stateData.BehaviorParameterValue = animation.id;
    _titleText.text = animation.UserFacingNameLocalized;
    _categoryImage.sprite = trMoodyAnimations.Instance.GetIcon(animation);
    RemoveAllContentItems();
    AddNewItemsToList(animation);
  }

  private void RemoveAllContentItems(){
    List<GameObject> children = new List<GameObject>();
    foreach (Transform child in _itemsContainer.transform) {
      children.Add(child.gameObject);
      trListItemControl control = child.GetComponent<trListItemControl>();
      if (control != null){
        control.onItemClicked.RemoveAllListeners();
      }
    }
    children.ForEach(child => Destroy(child));
  }


  private void AddNewItemsToList(trMoodyAnimation animation){ 
    foreach(trMoodType mood in animation.AvailableMoods){
      GameObject item = CreateListItem(mood);
      item.transform.SetParent(_itemsContainer.transform);
      item.GetComponent<RectTransform>().SetDefaultScale();
      RectTransform rectTransform = item.GetComponent<RectTransform>();
      Vector3 position = rectTransform.localPosition;
      position.z = 0;
      rectTransform.localPosition = position;
    }
  }

  public override void ActivateSegment (uGUISegment seg) {
    OnItemClicked(seg as trAnimationMoodButtonController);
    base.ActivateSegment (seg);
    if(!_isPanelJustOpen){ // only play animation when it's user pressing
      ExecutOnRobot(true);
    }
    _isPanelJustOpen = false;

  }

  private GameObject CreateListItem(trMoodType moodType){
    GameObject moodButton = Instantiate(_animationButtonPrefab, _animationButtonPrefab.transform.position, Quaternion.identity) as GameObject;
    moodButton.transform.SetParent(_itemsContainer.transform, false);
    trAnimationMoodButtonController ctrl =  moodButton.GetComponent<trAnimationMoodButtonController>();
    ctrl.SegmentsController = this;
    ctrl.Icon.sprite=  trIconFactory.GetIcon(moodType);
    ctrl.valueText = ((int)moodType).ToString();
    ctrl.DescriptionText.text = trMood.MoodString(moodType);
    AddSegment(ctrl);
  
    if (_stateData.Mood == trMoodType.NO_CHANGE) {
      _stateData.Mood = moodType;
    }
    if (_stateData.Mood == moodType) {
      _isPanelJustOpen = true;
      ActivateSegment(ctrl);
    }
    return moodButton;
  }

  private void ClosePanel(){
    _stateButton.SetUpView();
    ExecutOnRobot(false);

    if (OnEditingFinished != null){
      OnEditingFinished(_stateData, true);
    }

    if(IsNewBehavior || _stateData.Mood != _stateAnimMoodStarted){
      protoController.StateEditCtrl.UpdateUndoRedoUserAction();
      trDataManager.Instance.SaveCurProgram();
    }
    gameObject.SetActive(false);

  }

  private void OnRobotStateChanged(piBotBase robot) {
    if (robot.NumberOfExecutingCommandSequences == 0) {
      protoController.CurRobot.OnState -= OnRobotStateChanged;
    }
  }

  public void ExecutOnRobot(bool toExecute){
    // always stop whatever robot is currently doing
    piBotBase currentRobot = protoController.GetRobotToExecute();
    if (currentRobot != null){
      protoController.CurRobot.OnState += OnRobotStateChanged;
      _stateData.SetActive(false, protoController.CurRobot, _stateData.Mood); 
      ((piBotBo)protoController.CurRobot).cmd_stopSingleAnim();
      if (toExecute){
        _stateData.SetActive(true, protoController.CurRobot, _stateData.Mood);   
      }
    }
  }
    

  private void OnItemClicked (trAnimationMoodButtonController item)
  {
    bool shouldClosePanel = IsDoubleClickFound (item);

    if (_selectedGameObject != item.gameObject) {
      _stateData.Mood = (trMoodType)(uint.Parse (item.valueText)); 
      _selectedGameObject = item.gameObject;
    }
    if (shouldClosePanel) {
      ClosePanel ();
    }
  }

  private bool IsDoubleClickFound (trAnimationMoodButtonController item){
    bool result = false;
    if(float.IsNaN(LastSegmentActivationTime)){
      LastSegmentActivationTime = Time.fixedTime;
    } 
    else if (item.gameObject == _selectedGameObject){
      result = (Time.fixedTime - LastSegmentActivationTime) < DOUBLE_ACTIVATION_TIME;
    } 
    if (result){
      LastSegmentActivationTime = float.NaN;
    }
    else {
      LastSegmentActivationTime = Time.fixedTime;
    }
    return result;
  }
}
