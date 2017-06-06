using UnityEngine;
using UnityEngine.UI;
using Turing;
using WW.UGUI;
using TMPro;

public class trSimpleBehaviorController : MonoBehaviour {

  public Button AcceptButton;
  public Button Background;
  public TextMeshProUGUI Title; 

  public trTankModeEditor TankModeEditor;
  public trMoveStraightEditor MoveStraightEditor;
  public trMoveSpinDiscreteEditor MoveSpinDiscreteEditor;
  public trHeadPanEditor HeadPanEditor;
  public trHeadTiltEditor HeadTiltEditor;
  public trEyeRingEditor EyeRingEditor;
  public trLauncherFlingEditor LaunchFlingEditor;

  public delegate void SimpleBehaviorConfigDelegate(trState state, bool success);
  public SimpleBehaviorConfigDelegate OnDismiss;
  public trProtoController ProtoController{get; set;}
  public trState State{
    get{
      return state;
    }
    set{
      SetupByState(value);
    }
  }

  public bool IsNewBehavior = false;

  private trSimpleBehaviorEditor currentBehaviorEditor;
  private trState state;

  void Start () {
    AcceptButton.onClick.AddListener(dismissPanel);
    Background.onClick.AddListener(dismissPanel);
  }

//  void SetActive(bool active, bool isCreateState = false){
//    if(active)
//  }

  void Close(){
    dismissPanel();
  }

  void dismissPanel() {
    gameObject.SetActive(false);
    if (currentBehaviorEditor != null){
      if(currentBehaviorEditor.StateParaChanged || IsNewBehavior){
        ProtoController.StateEditCtrl.UpdateUndoRedoUserAction();
        trDataManager.Instance.SaveCurProgram();
      }
      currentBehaviorEditor.Reset();
    }
    if (OnDismiss != null) OnDismiss(state, true);
  }

  void SetupByState(trState newState) {
    state = newState;
    Title.text = newState.Behavior.UserFacingNameLocalized;
    if (currentBehaviorEditor != null) currentBehaviorEditor.gameObject.SetActive(false); 
    switch (newState.Behavior.Type){
      case trBehaviorType.MOVE_CONT_SPIN:
        currentBehaviorEditor = TankModeEditor;        
        break;
      case trBehaviorType.MOVE_DISC_TURN:
        currentBehaviorEditor = MoveSpinDiscreteEditor;
        break;
      case trBehaviorType.HEAD_PAN:
        currentBehaviorEditor = HeadPanEditor;
        break;
      case trBehaviorType.HEAD_TILT:
        currentBehaviorEditor = HeadTiltEditor;
        break;
      case trBehaviorType.EYERING:
        currentBehaviorEditor = EyeRingEditor;
        break;
      case trBehaviorType.MOVE_CONT_STRAIGHT:
      case trBehaviorType.MOVE_DISC_STRAIGHT:
        currentBehaviorEditor = MoveStraightEditor;
        break;
      case trBehaviorType.LAUNCH_FLING:
        currentBehaviorEditor = LaunchFlingEditor;
        break;
    }
    currentBehaviorEditor.gameObject.SetActive(true);
    currentBehaviorEditor.State = newState;
    currentBehaviorEditor.ProtoController = ProtoController;

    // if (HeadingImage != null) HeadingImage.sprite = trIconFactory.GetIcon(state);          
    // if (HintContainer != null){
    //   string[] hints = state.Behavior.Hints;
    //   // clear out previous hints first, then instantiate new ones based on this state
    //   foreach(Transform child in HintContainer.transform){        
    //     Destroy(child.gameObject);
    //   }
    //   for (int i = 0; i < hints.Length; i++){
    //     var hintItem = (GameObject) Instantiate(BulletListItem, Vector3.zero, Quaternion.identity);
    //     wwGUIBulletItemText item = hintItem.GetComponent<wwGUIBulletItemText>();
    //     item.Index = i;
    //     item.ContentText.text = hints[i];
    //     hintItem.transform.SetParent(HintContainer.transform, false);
    //   }        
    // }
  }
}
