using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Turing;

public class trElementInfo {
  // only one is non-null.
  // set via constructor.
  private trTransition transition;
  private trState      state;

  public trElementInfo(trState trS) {
    state = trS;
  }

  public trElementInfo(trTransition trTrn) {
    transition = trTrn;
  }

  public trState State {
    get {
      return state;
    }
  }

  public trTransition Transition {
    get {
      return transition;
    }
  }

  public bool IsState {
    get {
      return (state != null);
    }
  }

  public bool IsTransition {
    get {
      return (transition != null);
    }
  }

  public string VideoPath{
    get{
      if(IsState){
        trBehaviorType trBT = state.Behavior.Type;
        if(trDataManager.Instance.VideoManager.behaviorVideos.ContainsKey(trBT)){
          return trDataManager.Instance.VideoManager.behaviorVideos[trBT].FileName;
        }
      }
      if(IsTransition){
        trTriggerType trT = transition.Trigger.Type;
        if(trDataManager.Instance.VideoManager.triggerVideos.ContainsKey(trT)){
          return trDataManager.Instance.VideoManager.triggerVideos[trT].FileName;
        }
      }
      return "";
    }
  }

  public string Name {
    get {
      bool includeItemType = false;

      if (IsState) {
        trBehaviorType trBT = state.Behavior.Type;
        string ret = trBT.ToString();

        if (trBehavior.TypeToUserFacingName.ContainsKey(trBT)) {
          ret = wwLoca.Format(trBehavior.TypeToUserFacingName[trBT]);
        }
        else {
          WWLog.logError("no user-facing name for type: " + trBT.ToString());
        }

        if (includeItemType) {
          ret = wwLoca.Format("@!@<b>Action:</b>@!@") + " " + ret;
        }

        return ret;
      }
      else {
        trTriggerType trTT = transition.Trigger.Type;
        string ret = trTT.ToString();
        if (trTrigger.typeToUserFacingName.ContainsKey(trTT)) {
          ret = wwLoca.Format(trTrigger.typeToUserFacingName[trTT]);
        }
        else {
          WWLog.logError("no name for trigger: " + trTT.ToString());
        }

        if (includeItemType) {
          ret = wwLoca.Format("@!@<b>Cue:</b>@!@") + " " + ret;
        }

        return ret;
      }
    }
  }

  public string DescriptionLocalized {
    get {
      if (IsState) {
        trBehaviorType trBT = state.Behavior.Type;
        string ret = trBT.ToString();

        if (trBT == trBehaviorType.MOODY_ANIMATION) {
          trMoodyAnimation trMA = trMoodyAnimations.Instance.GetAnimation(state);
          if (trMA != null) {
            ret = "<b>" + trMA.UserFacingNameLocalized + "</b>";
          }
          else {
            WWLog.logError("unknown animation: " + state.ToString());
          }
        }
        else if (state.Behavior.Type.IsSound()) {
          trRobotSound trRS = trRobotSounds.Instance.GetSound(state, trDataManager.Instance.CurrentRobotTypeSelected);
          if (trRS != null) {
            ret = "<b>" + trRS.UserFacingNameLocalized + "</b>";
          }
          else {
            WWLog.logError("unknown sound: " + state.ToString());
          }
        }
        else if (trBehavior.TypeToDescription.ContainsKey(trBT)) {
          ret = state.Behavior.DescriptionLocalized;
        }
        else {
          WWLog.logError("no user-facing description for type: " + trBT.ToString());
        }
        return ret;
      }
      else {
        trTriggerType trTT = transition.Trigger.Type;
        string ret = trTT.ToString();

        if (trTT == trTriggerType.BEHAVIOR_FINISHED) {
          trBehavior trB = transition.StateSource.Behavior;
          ret = trB.autoTransitionDescriptionLocalized(trDataManager.Instance.CurrentRobotTypeSelected);
        }
        else {
          ret = trTrigger.GetDescriptionLocalized(trTT);
        }
        return ret;
      }
    }
  }

  public string Params {
    get {
      if (IsState) {
        return trStringFactory.GetParaString(State);
      }
      else {
        return trStringFactory.GetParaString(Transition.Trigger);
      }
    }
  }
  
  public Sprite Icon {
    get {
      if (IsState) {
        trBehaviorType trBT = state.Behavior.Type;
        Sprite ret = null;
        if (trBT == trBehaviorType.MOODY_ANIMATION) {
          trMoodyAnimation trMA = trMoodyAnimations.Instance.GetAnimation(state);
          if (trMA != null) {
            ret = trMoodyAnimations.Instance.GetIcon(trMA);
          }
          else {
            WWLog.logError("no animation! " + state.ToString());
          }
        }
        else {
          ret = trIconFactory.GetIcon(trBT);
        }
        return ret;
      }
      else {
        trTriggerType trTT = transition.Trigger.Type;
        Sprite ret = trIconFactory.GetIcon(trTT);
        return ret;
      }
    }
  }

  public Sprite MoodIcon {
    get {
      if (!IsState) {
        return null;
      }

      if (State.Mood == trMoodType.NO_CHANGE) {
        return null;
      }

      return trIconFactory.GetIcon(State.Mood);
    }
  }

  public bool isEquivalentTo(trElementInfo other) {
    if (other == this) {
      return true;
    }

    return ((other.State == state) && (other.Transition == Transition));
  }
}

public class trElementInfoController : MonoBehaviour {
  public trElementInfo info;

  public Image           elIcon;
  public TextMeshProUGUI elName;
  public TextMeshProUGUI elDescription;
  public TextMeshProUGUI elParams;
  public Image           elMoodIcon;
  public Button          btnSelect;
  public Button          videoButton;
  public trProtoController ProtoCtrl;

  private bool          expanded    = true;
  private trElementInfo elementInfo = null;
  private bool          maximal     = false;

  public bool Expanded {
    get {
      return expanded;
    }
    set {
      expanded = value;
      elName       .gameObject.SetActive(expanded);
      elDescription.gameObject.SetActive(expanded);
      elParams     .gameObject.SetActive(expanded && !maximal);
      elMoodIcon   .gameObject.SetActive(expanded && !maximal);
      gameObject.GetComponent<LayoutElement>().flexibleWidth = expanded ? 1.0f : 0.0f;
    }
  }

  public bool Maximal {
    get {
      return maximal;
    }
    set {
      maximal = value;
      elParams  .gameObject.SetActive(expanded && maximal);
      elMoodIcon.gameObject.SetActive(expanded && maximal);
    }
  }

  public trElementInfo ElementInfo {
    get {
      return elementInfo;
    }
    set {
      elementInfo = value;
      elName       .text    = elementInfo.Name;
      elDescription.text    = elementInfo.DescriptionLocalized;
      elIcon       .sprite  = elementInfo.Icon;
      elParams     .text    = elementInfo.Params;
      elMoodIcon   .sprite  = elementInfo.MoodIcon;
      elMoodIcon   .enabled = (elMoodIcon.sprite != null);

     
//      bool hasVideo = elementInfo.VideoPath != "";
//
//      videoButton.gameObject.SetActive(hasVideo);
    }
  }

  void Start(){
    videoButton.onClick.AddListener(onVideoButtonClicked);
  }

  void onVideoButtonClicked(){
    ProtoCtrl.VideoPanelCtrl.SetActive(true);
    ProtoCtrl.VideoPanelCtrl.Play(elementInfo.VideoPath, elementInfo.Name);
  }
}


