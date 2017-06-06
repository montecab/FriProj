using UnityEngine;
using System.Collections;
using Turing;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class trSoundConfigurePanelController : MonoBehaviour {

  public TextMeshProUGUI TitleText;
  public GameObject ItemPrefab;
  public GridLayoutGroup ItemsContainer;
  public ScrollRect ItemsScrollRect;
  public Color SelectedItemColor = Color.green;
  public Button Background;
  public Button CloseButton;

  public Button RecordNewSoundButton;
  public Sprite RecordNewSound;
  public Sprite RecordNewPuppet;
  public TextMeshProUGUI RecordSoundText;

  public delegate void EditingFinishedDelegate(trState state, bool success);
  public EditingFinishedDelegate OnEditingFinished;

  public trProtoController protoController{get; set;}

  public bool IsNewBehavior = false; //if the config penal is opened for a new created behavior, this is used for undo/redo checking

  private float stateSoundIdStarted = float.NaN;

  private GameObject selectedGameObject;
  private trState stateData;
  private piRobotType robotType;

  private trStateButtonController stateButton;
  private float targetScrollOffset;

  protected float LastSegmentActivationTime = float.NaN;
  protected const float DOUBLE_ACTIVATION_TIME = 0.3f; //seconds



  void Awake(){
    CloseButton.onClick.AddListener(closePanel);
    Background.onClick.AddListener(closePanel);
	  RecordNewSoundButton.onClick.AddListener(recordNewSound);
  }

  private const float kGridPaddingRatio = 0.03f;
  private const float kGridContentWidthRaio = 0.94f;
  void Start () {
	  SetupView();
    //Grid layout does not have a way to automatically change the size of the cells based on resolution
    //So we are programatically setting it here based on the size of the grid
    float gridWidth = ItemsContainer.GetComponent<RectTransform>().GetWidth();
    int padding = (int)(kGridPaddingRatio*gridWidth);
    ItemsContainer.padding = new RectOffset(padding, padding, padding, padding);
    ItemsContainer.cellSize = new Vector2(0.94f*gridWidth, ItemsContainer.cellSize.y);
	}

  void OnEnable(){
    StartCoroutine(scrollToSelectedRow());
  }

  IEnumerator scrollToSelectedRow(){
    yield return 1;
    Canvas.ForceUpdateCanvases();
    ItemsScrollRect.verticalNormalizedPosition = targetScrollOffset;
  }

  public void SetStateData(trStateButtonController stateButtonCtrl, piRobotType robotType){
    this.stateButton = stateButtonCtrl;
    this.stateData = this.stateButton.StateData;
    this.robotType = robotType;
    stateSoundIdStarted = stateData.BehaviorParameterValue;
    SetupView();
  }

  private void SetupView(){
    if (stateData.Behavior.Type == trBehaviorType.PUPPET) {
      SetupViewPuppet();
    }
    else {
      SetupViewSound();
    }
  }

  private void SetupViewSound(){
    TitleText.text = trRobotSounds.Instance.getCategoryNameLocalized(stateData.Behavior.Type);    
    RecordNewSoundButton.gameObject.SetActive(stateData.Behavior.Type == trBehaviorType.SOUND_USER);
    RecordNewSoundButton.GetComponent<Image>().sprite = RecordNewSound;
    RecordSoundText.gameObject.SetActive(stateData.Behavior.Type == trBehaviorType.SOUND_USER);    
    RecordSoundText.text = wwLoca.Format("@!@Record Sounds@!@");
    removeAllContentItems();
    addNewSoundItemsToList();
  }

  private void SetupViewPuppet(){
    TitleText.text = "Custom Animations!";
    RecordNewSoundButton.gameObject.SetActive(true);
    RecordNewSoundButton.GetComponent<Image>().sprite = RecordNewPuppet;
    RecordSoundText.gameObject.SetActive(true);
    RecordSoundText.text = "Record Anims";
    removeAllContentItems();
    addNewPuppetItemsToList();    
  }

  private void removeAllContentItems(){
    List<GameObject> children = new List<GameObject>();
    foreach (Transform child in ItemsContainer.transform) {
      children.Add(child.gameObject);
      trListItemControl control = child.GetComponent<trListItemControl>();
      if (control != null){
        control.onItemClicked.RemoveAllListeners();
      }
    }
    children.ForEach(child => Destroy(child));
  }

  private bool isUserSound (trRobotSound sound) {
  	return (sound.category == trBehaviorType.SOUND_USER);
  }

  private int CompareSoundNames(trRobotSound x, trRobotSound y) {
    string sx = x.UserFacingNameLocalized;
    string sy = y.UserFacingNameLocalized;

    while ((sx.Length > 0) && (sy.Length > 0)) {
      wwLoca.Format(sx);     
    }

    return sx.CompareTo(sy);
  }
  private void addNewSoundItemsToList(){ 
    List<trRobotSound> soundForCategory = trRobotSounds.Instance.GetCategory(stateData.Behavior.Type, robotType);

    trLocalizedSorter sorter = new trLocalizedSorter();

    soundForCategory.Sort((x,y) => sorter.compare(x.UserFacingNameLocalized, y.UserFacingNameLocalized));

    if (!stateData.IsParameterValueSet() && soundForCategory.Capacity > 0){
      stateData.BehaviorParameterValue = soundForCategory[0].id;
    }

    targetScrollOffset = 1.0f;

    foreach(trRobotSound sound in soundForCategory){
      GameObject item = CreateListSoundItem(sound);
      item.transform.SetParent(ItemsContainer.transform);
      item.GetComponent<RectTransform>().SetDefaultScale();
      RectTransform rectTransform = item.GetComponent<RectTransform>();
      Vector3 position = rectTransform.localPosition;
      position.z = 0;
      rectTransform.localPosition = position;
      if (sound.id == (uint)stateData.BehaviorParameterValue){
        item.GetComponent<Image>().color = SelectedItemColor;
        selectedGameObject = item;
        targetScrollOffset -= soundForCategory.IndexOf(sound) / (float)(soundForCategory.Count - 1);
        if (targetScrollOffset > 1){
          targetScrollOffset = 1.0f;
        }
      }
    }
  }

  private GameObject CreateListSoundItem(trRobotSound sound){
    GameObject itemObject = Instantiate(ItemPrefab) as GameObject;
    trListItemControl control = itemObject.GetComponent<trListItemControl>();
    control.IndexText.text = trRobotSounds.Instance.GetUserFacingIndex(sound).ToString() + ".";
    control.NameText.text = sound.UserFacingNameLocalized;
    control.ValueText.text = sound.id.ToString();
    control.ValueText.gameObject.SetActive(false);
    control.onItemClicked.AddListener(OnItemClicked);
    return itemObject;
  }

  private void addNewPuppetItemsToList(){ 
    if (!stateData.IsParameterValueSet()) {
      stateData.BehaviorParameterValue = 0;
    }

    targetScrollOffset = 1.0f;

    for (int n = 0; n < trBehavior.NUM_PUPPET_SLOTS; ++n) {
      GameObject item = CreateListPuppetItem(n);

      item.transform.SetParent(ItemsContainer.transform);
      item.GetComponent<RectTransform>().SetDefaultScale();
      RectTransform rectTransform = item.GetComponent<RectTransform>();
      Vector3 position = rectTransform.localPosition;
      position.z = 0;
      rectTransform.localPosition = position;
      if (n == (uint)stateData.BehaviorParameterValue){
        item.GetComponent<Image>().color = SelectedItemColor;
        selectedGameObject = item;
        targetScrollOffset -= (n / trBehavior.NUM_PUPPET_SLOTS);
        if (targetScrollOffset > 1){
          targetScrollOffset = 1.0f;
        }
      }
    }
  }

  private GameObject CreateListPuppetItem(int index){
    int slot = index + 1;
    GameObject itemObject = Instantiate(ItemPrefab) as GameObject;
    trListItemControl control = itemObject.GetComponent<trListItemControl>();
    control.IndexText.text = slot.ToString(); // what is this for ??
    control.NameText .text = "Custom Animation " + slot.ToString();
    control.ValueText.text = index.ToString();
    control.ValueText.gameObject.SetActive(false);
    control.onItemClicked.AddListener(OnItemClicked);
    return itemObject;
  }

  private void closePanel(){
    stateButton.SetUpView();
    piBotBase currentRobot = protoController.GetRobotToExecute ();
    if (currentRobot != null) {
      stateData.SetActive (false, currentRobot);
    }
    if (OnEditingFinished != null){
      OnEditingFinished(stateData, true);
    }

    gameObject.SetActive(false);

    if(IsNewBehavior || stateSoundIdStarted != stateData.BehaviorParameterValue){
      protoController.StateEditCtrl.UpdateUndoRedoUserAction();
      trDataManager.Instance.SaveCurProgram();
    }   
  }

  private void recordNewSound(){
    piBotBase currentRobot = protoController.GetRobotToExecute();
    if (currentRobot != null) {
      if (stateData.Behavior.Type == trBehaviorType.PUPPET) {
        if (currentRobot.robotType == piRobotType.DASH) {
  	      PIBInterface_internal.onClickPuppetRecordButton();
        }
        else {
          WWLog.logError("unexpected: clicked record button but robot type is " + currentRobot.robotType.ToString());
        }
      }
      else {
        PIBInterface_internal.onClickSoundRecordButton();
      }
    }
  }

  private void OnItemClicked(trListItemControl item){
    bool shouldClosePanel = isDoubleClickDetected(item);

    if (selectedGameObject != item.gameObject){
      stateData.BehaviorParameterValue = (float)uint.Parse(item.ValueText.text);
      
      if (selectedGameObject != null){
        selectedGameObject.GetComponent<Image>().color = Color.clear;
      }
      selectedGameObject = item.gameObject;
      selectedGameObject.GetComponent<Image>().color = SelectedItemColor;
     
    }
    piBotBase currentRobot = protoController.GetRobotToExecute();
    if (currentRobot != null){
      stateData.SetActive(false, currentRobot);
      stateData.SetActive(true, currentRobot);
    }
    
    if (shouldClosePanel){
      closePanel();
    }
  }

  private bool isDoubleClickDetected (trListItemControl item){    
    bool result = false;    
    if(float.IsNaN(LastSegmentActivationTime)){
      LastSegmentActivationTime = Time.fixedTime;
    } 
    else if (item.gameObject == selectedGameObject){
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
