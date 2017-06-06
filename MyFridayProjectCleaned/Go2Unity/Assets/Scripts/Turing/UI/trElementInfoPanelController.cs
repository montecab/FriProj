using UnityEngine;
using UnityEngine.UI;
using Turing;
using TMPro;
using DG.Tweening;

public class trElementInfoPanelController : MonoBehaviour
{

  public enum BehaviorOnPointerUp {
    CLOSE,
    STAY_OPEN,
  };

  public        HorizontalLayoutGroup   itemList;
  public        trElementInfoController exampleItem;
  public        Button                  showMultiplesButton;
  public        Button                  detailsButton;
  private const int                     numPreambleElements = 2;
  private       bool                    userShowMultiples = false;
  public        Image                   sibling1;
  public        Image                   sibling2;

  // really this is const, but we get compile warns about unreachable code that way.
  public static BehaviorOnPointerUp     behaviorOnPointerUp = BehaviorOnPointerUp.CLOSE;
  public const  float                   longPressDuration = 0.3f;

  private       float                   prefWidthShowMultiButton = 50f; // figure how to get this from scene.
  private       float                   prefWidthItem;
  private       float                   transitionDurationShowing = 0.3f;
  private       float                   transitionDurationHiding  = 0.2f;
  private       float                   transitionStartMoment     = 0f;
  private       trProtoController       _protoCtrl;

  public void SetupView(trProtoController protoCtrl) {
    _protoCtrl = protoCtrl;
    exampleItem = itemList.transform.GetChild(1).GetComponent<trElementInfoController>();
    exampleItem.gameObject.SetActive(false);

    showMultiplesButton.onClick.AddListener(onClickShowMultiples);
    detailsButton      .onClick.AddListener(onClickDetails);

    prefWidthItem            = exampleItem        .GetComponent<LayoutElement>().preferredWidth;

    for (int n = numPreambleElements; n < itemList.transform.childCount - 1; ++n) {
      Transform child = itemList.transform.GetChild(n);
      LayoutElement le = child.GetComponent<LayoutElement>();
      le.preferredWidth = 0f;
      le.preferredHeight = 0f;
    }
  }

  void Update() {
    LayoutElement le;
    float targWidth;
    float targHeight;

    float tt = transitionPercent;

    if (tt >= 0f && tt <= 2f) {

      tt = Mathf.Sqrt(tt);

      float prefHeightItem = exampleItem.GetComponent<LayoutElement>().preferredHeight;
      targWidth  = ShowMultiples ? prefWidthItem  : 0f;
      targHeight = ShowMultiples ? prefHeightItem : 0f;
      for (int n = numPreambleElements; n < itemList.transform.childCount - 1; ++n) {
        float f = (float)(n - numPreambleElements) / (float)(itemList.transform.childCount - numPreambleElements - 1);
        f = ShowMultiples ? f : (1f - f);
        Transform child = itemList.transform.GetChild(n);
        le = child.GetComponent<LayoutElement>();
        le.preferredWidth  = Mathf.Lerp(prefWidthItem  - targWidth , targWidth , tt - 0.3f * f);
        le.preferredHeight = Mathf.Lerp(prefHeightItem - targHeight, targHeight, tt - 0.3f * f);
        le.gameObject.SetActive(le.preferredWidth > 0.01f);
      }

      // this line ensures that the expanded item is full height.
      // without this it's possible for it to be a little bit short if the user
      // selects another item while this item is still expanding, and then selects this one again.
      // (this can happen easily in the case of a cue w/ one sibling)
      itemList.transform.GetChild(itemList.transform.childCount - 1).GetComponent<LayoutElement>().preferredHeight = prefHeightItem;

      bool showIt = ShowShowMultiplesButton;
      targWidth  = showIt ? prefWidthShowMultiButton : 0f;
      targHeight = showIt ? exampleItem.GetComponent<LayoutElement>().preferredHeight : 0f;
      le = showMultiplesButton.GetComponent<LayoutElement>();
      le.preferredWidth  = Mathf.Lerp(prefWidthShowMultiButton - targWidth , targWidth , tt);
      le.preferredHeight = Mathf.Lerp(prefHeightItem           - targHeight, targHeight, tt);
      le.gameObject.SetActive(le.preferredWidth > 0.01f);
    }
  }

  private bool ShowShowMultiplesButton {
    get {
      return (!ShowMultiples) && (itemList.transform.childCount > (numPreambleElements + 2));
    }
  }

  private bool ShowFirstSibling {
    get {
      return itemList.transform.childCount == (numPreambleElements + 2);
    }
  }

  float transitionPercent {
    get {
      float dt = Time.time - transitionStartMoment;
      float duration = ShowMultiples ? transitionDurationShowing : transitionDurationHiding;
      return dt / duration;
    }
  }

  void transitionStart() {
    transitionStartMoment = Time.time;
  }

  public void clear() {
    // note: being careful not to delete the first couple, which is the example item & button.
    for (int n = itemList.transform.childCount - 1; n >= numPreambleElements; --n) {
      Transform child = itemList.transform.GetChild(n);
      GameObject.Destroy(child.gameObject);
      child.SetParent(null);
    }

    userShowMultiples = false;
  }

  public void addNewItem(trElementInfo trEI) {
    userShowMultiples = false;

    trElementInfoController trEIC = GameObject.Instantiate<trElementInfoController>(exampleItem);
    trEIC.gameObject.SetActive(true);
    trEIC.transform.SetParent(itemList.transform);
    trEIC.transform.localScale    = Vector3.one;
    trEIC.transform.localPosition = Vector3.zero; // unity you make-a me crazy

    trEIC.btnSelect.onClick.AddListener(() => {
      onSelectItem(trEIC);
    });

    trEIC.Expanded = true;

    trEIC.ElementInfo = trEI;

    for (int n = numPreambleElements; n < itemList.transform.childCount - 1; ++n) {
      trElementInfoController trEIC2 = itemList.transform.GetChild(n).GetComponent<trElementInfoController>();
      trEIC2.Expanded = false;
      trEIC2.GetComponent<LayoutElement>().preferredWidth  = 0;
      trEIC2.GetComponent<LayoutElement>().preferredHeight = 0;
      trEIC2.gameObject.SetActive(false);
    }

    bool showIt = ShowShowMultiplesButton;
    showMultiplesButton.gameObject.SetActive(showIt);
    showMultiplesButton.GetComponent<LayoutElement>().preferredWidth  = showIt ? prefWidthShowMultiButton : 0;
    showMultiplesButton.GetComponent<LayoutElement>().preferredHeight = showIt ? exampleItem.GetComponent<LayoutElement>().preferredHeight : 0;

    updateDetailsVisibility();
    ShowElementInfoRing(trEI, false);

    if (ShowMultiples) {
      transitionStart();
    }
  }

  private void updateSiblingIcons() {
    if (itemList.transform.childCount > numPreambleElements + 2) {
      sibling1.sprite = itemList.transform.GetChild(numPreambleElements + 0).GetComponent<trElementInfoController>().elIcon.sprite;
      sibling2.sprite = itemList.transform.GetChild(numPreambleElements + 1).GetComponent<trElementInfoController>().elIcon.sprite;
    }
  }

  // returns true IFF is active and the expanded item is this.
  public bool isShowing(trElementInfo item) {
    if (!gameObject.activeSelf) {
      return false;
    }

    return ExpandedElement.ElementInfo.isEquivalentTo(item);
  }

  // returns true IFF the panel is Active and Item is contained
  public bool isListing(trElementInfo item) {
    if (!gameObject.activeSelf) {
      return false;
    }

    return containsItem(item);
  }

  public bool containsItem(trElementInfo item) {
    for (int n = numPreambleElements; n < itemList.transform.childCount; ++n) {
      trElementInfoController trEIC = itemList.transform.GetChild(n).GetComponent<trElementInfoController>();
      trElementInfo trEI = trEIC.ElementInfo;
      if (trEI.isEquivalentTo(item)) {
        return true;
      }
    }

    return false;
  }

  bool ShowMultiples {
    set {
      userShowMultiples = value;
      ExpandedElement.Maximal = !userShowMultiples;
      transitionStart();
    }
    get {
      bool ret = userShowMultiples || ShowFirstSibling;
      return ret;
    }
  }

  void onSelectItem(trElementInfoController trEIC) {
    Transform parent = itemList.transform;

    updateDetailsVisibility();

    if (trEIC.Expanded) {
      if (ShowMultiples) {
        ShowMultiples = false;
      }
      return;
    }

    parent.GetChild(parent.childCount - 1).GetComponent<trElementInfoController>().Expanded = false;
    trEIC.transform.SetAsLastSibling();
    trEIC.Expanded = true;

    ShowMultiples = false;

    ShowElementInfoRing(trEIC.ElementInfo);
  }

  public void selectElement(trElementInfo trEI) {
    trElementInfoController trEIC_Found = null;

    for (int n = numPreambleElements; n < itemList.transform.childCount; ++n) {
      trElementInfoController trEIC = itemList.transform.GetChild(n).GetComponent<trElementInfoController>();
      if (trEIC.ElementInfo.isEquivalentTo(trEI)) {
        trEIC_Found = trEIC;
        break;
      }
    }

    if (trEIC_Found != null) {
      if (trEIC_Found == ExpandedElement) {
        return;
      }
      if (!ShowMultiples) {
        ExpandedElement.gameObject.SetActive(false);
      }
      ExpandedElement.Expanded = false;
      trEIC_Found.transform.SetAsLastSibling();
      trEIC_Found.Expanded = true;
      trEIC_Found.gameObject.SetActive(true);
      trEIC_Found.GetComponent<LayoutElement>().preferredWidth  = prefWidthItem;
      trEIC_Found.GetComponent<LayoutElement>().preferredHeight = exampleItem.GetComponent<LayoutElement>().preferredHeight;
      trEIC_Found.GetComponent<LayoutElement>().flexibleWidth   = 1.0f;
    }
    else {
      WWLog.logError("no controller for elementInfo: " + trEI.Name);
    }

    ShowElementInfoRing(trEI, false);
  }

  private void ShowElementInfoRing(trElementInfo trEI, bool withSound = true) {
    if (withSound) {
      SoundManager.soundManager.PlaySound(SoundManager.trAppSound.UI_SOUND);
    }
    if (_protoCtrl != null) {
      if (behaviorOnPointerUp == BehaviorOnPointerUp.STAY_OPEN) {
        _protoCtrl.ShowElementInfoRing(trEI);
      }
      else {
        _protoCtrl.ShowElementInfoRing(null);
      }
    }

    updateSiblingIcons();
  }

  public trElementInfoController ExpandedElement {
    get {
      int numChildren = itemList.transform.childCount;
      if (numChildren <= numPreambleElements) {
        return null;
      }
      return itemList.transform.GetChild(numChildren - 1).GetComponent<trElementInfoController>();    
    }
  }

  void updateDetailsVisibility() {
    bool show = true;
    trElementInfo trEI = ExpandedElement.ElementInfo;
    if (trEI.IsTransition) {
      show = true;
    }
    else {
      show = trEI.State.Behavior.IsParameterized;
    }

    show = show && (behaviorOnPointerUp == BehaviorOnPointerUp.STAY_OPEN);

    detailsButton.gameObject.SetActive(show);
  }

  void onClickShowMultiples() {
    ShowMultiples = true;
    transitionStart();
  }

  void onClickDetails() {
    gameObject.SetActive(false);
    ShowElementInfoRing(null, false);

    if (_protoCtrl && _protoCtrl.StateEditCtrl) {

      SoundManager.soundManager.PlaySound(SoundManager.trAppSound.UI_SOUND);

      if (ExpandedElement.ElementInfo.IsState) {
        _protoCtrl.StateEditCtrl.ShowBehaviourConfigurationPanel(ExpandedElement.ElementInfo.State);
      }
      else {
        _protoCtrl.StateEditCtrl.ShowTriggerConfigurationPanel(ExpandedElement.ElementInfo.Transition);
      }
    }
  }
}