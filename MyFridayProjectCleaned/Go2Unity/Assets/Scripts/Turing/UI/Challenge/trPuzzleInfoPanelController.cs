using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;

namespace Turing{

  public class trPuzzleInfoPanelController : MonoBehaviour {

    public delegate void BehaviourCountDelegate(string uuid, int count);
    public BehaviourCountDelegate onBehaviourCountChanged;
    public trPuzzle CurPuzzle{
      set{
        curPuzzle = value;
        setView();
      }
      get{
        return curPuzzle;
      }
    }
    public bool IsIngredientsRight{get; set;}

    [SerializeField]
    private trHintPanelController _hintPanelCtrl;
    public trHintPanelController HintPanelCtrl{get{return _hintPanelCtrl;}}
    [SerializeField]
    private GameObject _instructionPrefab;
    [SerializeField]
    private GameObject _stepIndicatorsPanel;
    [SerializeField]
    private GameObject _stepIndicatorPrefab;
    [SerializeField]
    private TextMeshProUGUI _descriptionText;
    [SerializeField]
    private Button _hintButton;
    [SerializeField]
    private Animator _hintButtonAnimator;
    [SerializeField]
    private Button _hintPanelBGButton;
    [SerializeField]
    private Animator _hintPanelAnimator;
    [SerializeField]
    private trTooltipPanelController _tooltipCtrl;

    private List<trIngredientInfoController> _wrongElements = new List<trIngredientInfoController>();
    private Dictionary<string, trIngredientInfoController> _behaviorToCtrlTable = new Dictionary<string, trIngredientInfoController>();
    private Dictionary<trTriggerType, trIngredientInfoController> _triggerToCtrlTable = new Dictionary<trTriggerType, trIngredientInfoController>();
    private trProgramIngredientInfo _targetIngredients = new trProgramIngredientInfo();
    private trPuzzle curPuzzle;
    private int _stepsCount = 3;
    private int _currentStep = 1;
    private bool _isHintAvailable = false;
    private bool _isHintShown = false;
    private IEnumerator _hintPanelCoroutine = null;
    private GameObject _instruction = null;
    private TextMeshProUGUI _instructionText;

    public int stepsCount {
      get{
        return _stepsCount;
      }
    }

    public int currentStep {
      get{
        return _currentStep;
      }
    }

    //TODO:UI Chris's design decision to remove the Ingredient in Main scene
    //public GameObject IngredientPrefab;
    //public GameObject IngredientGrid;
    //public GameObject IngredientPanel;
    //public GameObject IngredientGridParent;

    public void SetupView(trProtoController protoCtrl, trPuzzle puzzle, BehaviourCountDelegate callback){
      _hintPanelCtrl.protoCtrl = protoCtrl;
      onBehaviourCountChanged += callback;
      CurPuzzle = puzzle;
    }

    private void Start(){
      trDataManager.Instance.OnSaveCurProgram += updateIngredients;
      HintPanelCtrl.onCloseButtonClicked += onHintButtonClicked;
      if(trDataManager.Instance.MissionMng.GetCurPuzzle().Hints.Count <= 2){
        _hintButton.gameObject.SetActive(false);
        _hintButtonAnimator.gameObject.SetActive(false);
      }
      else{
        _isHintAvailable = true;
        _hintButton.onClick.AddListener(onHintButtonClicked);
        _hintPanelBGButton.onClick.AddListener(onHintButtonClicked);
//        trHintManager.Instance.OnHint += DisplayHelpTooltip;
      }

      new trTelemetryEvent(trTelemetryEventType.CHAL_BEGIN_STEP, true)
        .add(trTelemetryParamType.CHALLENGE, trDataManager.Instance.MissionMng.GetCurMission().UserFacingName)
        .add(trTelemetryParamType.STEP, trDataManager.Instance.MissionMng.UserOverallProgress.CurMissionInfo.PuzzleIndex)
        .add(trTelemetryParamType.IS_REPLAY, trDataManager.Instance.MissionMng.UserOverallProgress.IsCurMissionCompleteOnceInt)
        .emit();
    }

    private void OnDestroy(){
      if(_isHintAvailable && trHintManager.Instance != null){
//        trHintManager.Instance.OnHint -= DisplayHelpTooltip;
      }
      if(trDataManager.Instance != null){
        trDataManager.Instance.OnSaveCurProgram -= updateIngredients;
      }
      if(_instruction!=null){
        Destroy(_instruction);
      }
    }
      
    private void updateIngredients(){
      trProgramIngredientInfo info = new trProgramIngredientInfo();
      info.CalculateIngredients(trDataManager.Instance.GetCurProgram());

      IsIngredientsRight = true;
      _wrongElements.Clear();
      foreach(string uuid in _targetIngredients.BehaviorTable.Keys){
        int tmp = info.BehaviorTable.ContainsKey(uuid)? info.BehaviorTable[uuid] : 0;
        int count = _targetIngredients.BehaviorTable[uuid] - tmp;
        if(count != 0){
          IsIngredientsRight = false;
          _wrongElements.Add(_behaviorToCtrlTable[uuid]);
        }
//        _behaviorToCtrlTable[uuid].SetCount(count);
        if (onBehaviourCountChanged != null){
          onBehaviourCountChanged(uuid, count);
        }
      }
      
      foreach(trTriggerType type in _targetIngredients.TriggerTable.Keys){
        int tmp = info.TriggerTable.ContainsKey(type)? info.TriggerTable[type] : 0;
        int count = _targetIngredients.TriggerTable[type] - tmp;
        if(count != 0){
          IsIngredientsRight = false;
          _wrongElements.Add(_triggerToCtrlTable[type]);
        }
//        _triggerToCtrlTable[type].SetCount(count);
      }

      //updateIngredientViewGrid();
    }

    public void DisplayErrorTooltip(){
      _tooltipCtrl.Display (wwLoca.Format("@!@Something is not right.\nView Hint@!@"));
      _hintButtonAnimator.ChangeState(1);
      _hintButtonAnimator.RestartState();
      SoundManager.soundManager.PlaySound(SoundManager.trAppSound.SM_HINT_AVAILABLE);
    }

    private void DisplayHelpTooltip(){
      _tooltipCtrl.Display (wwLoca.Format("@!@Need help?\nView Hint@!@"));
      _hintButtonAnimator.ChangeState(1);
      _hintButtonAnimator.RestartState();
      SoundManager.soundManager.PlaySound(SoundManager.trAppSound.SM_HINT_AVAILABLE);
    }

    private void onHintButtonClicked (){
      _tooltipCtrl.Close();
      if (!_hintPanelAnimator.IsAnimationPlaying()) {
        if (_isHintShown) {
          onHintButtonCloseClicked ();
        } 
        else {
            HintPanelCtrl.gameObject.SetActive(true);
            _hintPanelBGButton.gameObject.SetActive(true);
            _hintPanelAnimator.ChangeState(1);
            new trTelemetryEvent(trTelemetryEventType.CHAL_HINT, true)
              .add(trTelemetryParamType.CHALLENGE, trDataManager.Instance.MissionMng.GetCurMission().UserFacingName)
              .add(trTelemetryParamType.STEP, trDataManager.Instance.MissionMng.UserOverallProgress.CurMissionInfo.PuzzleIndex)
              .add(trTelemetryParamType.HINT, trDataManager.Instance.MissionMng.UserOverallProgress.CurMissionInfo.HintIndex)
              .add(trTelemetryParamType.IS_REPLAY, trDataManager.Instance.MissionMng.UserOverallProgress.IsCurMissionCompleteOnceInt)
              .emit();
        }
        _isHintShown = !_isHintShown;
      }
    }

    private void onHintButtonCloseClicked (){
      if (!_hintPanelAnimator.IsAnimationPlaying()) {
        _hintPanelAnimator.ChangeState(2);
        if(_hintPanelCoroutine!=null){
          StopCoroutine(_hintPanelCoroutine);
        }
        _hintPanelCoroutine = _hintPanelAnimator.WaitForAnimationEnd(()=>{
          HintPanelCtrl.gameObject.SetActive(false);
          _hintPanelBGButton.gameObject.SetActive(false);
        });
        StartCoroutine(_hintPanelCoroutine);
      }
    }

    private void setView(){
      if(trDataManager.Instance.MissionMng.UserOverallProgress.IsCurMissionCompleted){
        _stepsCount = trDataManager.Instance.MissionMng.AuthoringMissionInfo.CurMission.Puzzles.Count;
        SetMissionCompleted();
        return;
      }

      _stepsCount = trDataManager.Instance.MissionMng.AuthoringMissionInfo.CurMission.Puzzles.Count;
      _currentStep = trDataManager.Instance.MissionMng.UserOverallProgress.CurMissionInfo.PuzzleIndex ;
      _descriptionText.text = wwLoca.Format(curPuzzle.Description);
      if(_instructionText!=null){
        _instructionText.text = _descriptionText.text;
      }
      setUpIngredient();
      setupCurrentSteps();
    }

    public void DisplayInstructionPopup(){
      if(_instruction==null){
        _instruction = Instantiate(_instructionPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        _instruction.transform.SetParent(this.transform.parent, false);
        _instructionText = _instruction.GetComponentInChildren<TextMeshProUGUI>();
      }
      _instruction.SetActive(true);
      _instructionText.text = _descriptionText.text;
    }

    public void CompleteStep(){
      if (_currentStep < _stepIndicatorsPanel.transform.childCount){
        trStepIndicator indicator = _stepIndicatorsPanel.transform.GetChild(_currentStep).GetComponent<trStepIndicator>();
        indicator.SetProgressState(trStepIndicator.ProgressState.PAST);
      }
    }
    
    public void SetMissionCompleted(){
      _descriptionText.text = wwLoca.Format("@!@You've completed this challenge!@!@");
      _hintButton.gameObject.SetActive(false);
      _hintButtonAnimator.gameObject.SetActive(false);
      //IngredientPanel.SetActive(false);
      _currentStep = _stepsCount;
      setupCurrentSteps();
    }

    private void setupCurrentSteps (){
      removeAllContentItems (_stepIndicatorsPanel.transform);
      if (_stepsCount > 1) {
        for (int i = 0; i < _stepsCount; i++) {
          GameObject step = Instantiate (_stepIndicatorPrefab) as GameObject;
          step.transform.SetParent(_stepIndicatorsPanel.transform);
          step.transform.localScale = Vector3.one;
          step.transform.localPosition = Vector3.zero;
          trStepIndicator indicator = step.GetComponent<trStepIndicator> ();
          if (i < _currentStep) {
            indicator.SetProgressState (trStepIndicator.ProgressState.PAST);
          } 
          else if (i == _currentStep) {
            indicator.SetProgressState (trStepIndicator.ProgressState.CURRENT);
          }
        }
      }
    }

    private void removeAllContentItems(Transform parent){
      List<GameObject> children = new List<GameObject>();
      foreach (Transform child in parent) {
        children.Add(child.gameObject);
      }
      children.ForEach(child => Destroy(child));
    }

    private void setUpIngredient(){
      if(curPuzzle == null){
        return;
      }

      if(curPuzzle.Hints.Count <2){
        WWLog.logError("Not enough hints");
        return;
      }

      _targetIngredients =  trDataManager.Instance.MissionMng.CurPuzzleIngredientInfo;

      foreach(string uuid in _targetIngredients.BehaviorTable.Keys){
//        trIngredientInfoController ctrl = createIngredient();
//        ctrl.SetView(trBehavior.GetDefaultBehavior(uuid),targetIngredients.BehaviorTable[uuid] );
        _behaviorToCtrlTable.Add(uuid, null);
      }

      foreach(trTriggerType type in _targetIngredients.TriggerTable.Keys){
//        trIngredientInfoController ctrl = createIngredient();
//        ctrl.SetView(type,targetIngredients.TriggerTable[type] );
        _triggerToCtrlTable.Add(type, null);
      }
      updateIngredients();

    }

    /*
    private void updateIngredientViewGrid(){
      if (!IngredientPanel.activeSelf) return;
      int itemsCount = targetIngredients.getIngredientsCount();
      int effectiveItemsCount = itemsCount;

      float ingredientWidth = IngredientPrefab.GetComponent<LayoutElement>().preferredWidth;
      GridLayoutGroup ingredientGridView = IngredientGrid.GetComponent<GridLayoutGroup>();

      int numIngredientsPerRow = IngredientGrid.GetComponent<GridLayoutGroup>().constraintCount;
      if (itemsCount > numIngredientsPerRow) {
        effectiveItemsCount = numIngredientsPerRow;
      }
      float preferedWidth = ingredientGridView.padding.left + ingredientGridView.padding.right + effectiveItemsCount * ingredientWidth;

      if (itemsCount > 1){
        preferedWidth += ingredientGridView.spacing.x * (effectiveItemsCount - 1);
      }
      float ingredientPanelHeight = ingredientWidth + 5;
      if(itemsCount > numIngredientsPerRow) {
        ingredientPanelHeight = 2f * ingredientPanelHeight;
      }

      IngredientGrid.GetComponentInParent<LayoutElement>().minHeight = ingredientPanelHeight;
      IngredientGridParent.GetComponentInParent<LayoutElement>().minHeight = ingredientPanelHeight;
      DescriptionText.GetComponent<LayoutElement>().minHeight = ingredientPanelHeight;

//      if (itemsCount <= numIngredientsPerRow) {
//        float height = IngredientGrid.GetComponentInParent<RectTransform>().GetHeight();
//        float padding = (height - ingredientWidth)/2;
//        IngredientGrid.GetComponent<GridLayoutGroup>().padding.top = (int)padding;
//      }
//      else {
//        IngredientGrid.GetComponent<GridLayoutGroup>().padding.top = 2;
//      }

      IngredientGrid.GetComponentInParent<LayoutElement>().minWidth = preferedWidth;
      IngredientGrid.GetComponentInParent<LayoutElement>().GetComponentInParent<LayoutElement>().preferredWidth = preferedWidth;
      IngredientGridParent.GetComponentInParent<LayoutElement>().minWidth = preferedWidth;
      IngredientGridParent.GetComponentInParent<LayoutElement>().GetComponentInParent<LayoutElement>().preferredWidth = preferedWidth;
      IngredientGridParent.GetComponentInParent<ScrollRect>().verticalNormalizedPosition = 0f;
      IngredientGridParent.GetComponentInParent<ScrollRect>().horizontalNormalizedPosition = 0f;
    }

    private trIngredientInfoController createIngredient(){
      GameObject ingredient = Instantiate(IngredientPrefab, IngredientPrefab.transform.position, Quaternion.identity) as GameObject;
      ingredient.transform.SetParent(IngredientGrid.transform, false);
      trIngredientInfoController ctrl = ingredient.GetComponent<trIngredientInfoController>();
      return ctrl;
    }
    */

  }
}