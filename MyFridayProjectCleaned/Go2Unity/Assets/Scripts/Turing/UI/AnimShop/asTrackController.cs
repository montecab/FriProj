// system
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// API
using Turing;

namespace AnimShop {

  public enum asTrackType {
    LIGHTS,
    SOUNDS,
    ACTION
  }

  public class asTrackController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
    public Button          buttonClearTrack;
    public TextMeshProUGUI trackLabel;
    public Transform       itemsContainer;
    public asTrackType     trackType;

    protected asAnimShopController parentController;
    public  asAnimShopController ParentController { set { parentController = value; } }

    private GameObject reticule;
    private float      reticuleNormY;    // -1 to 1 vertically across the height of the track.

    private Transform  draggingItem;
    private float      draggingItemNormY;

    private const float thresh1 = 1.1f;
    private const float thresh2 = 1.5f;
    private static Vector3 vFilterX = new Vector3(1f, 0f, 0f);

    private static Dictionary<asTrackType, string> trackLabelDict = new Dictionary<asTrackType, string> {
      {asTrackType.LIGHTS, "Lights"},
      {asTrackType.SOUNDS, "Sounds"},
      {asTrackType.ACTION, "Action!"},
    };

    void Start() {
      buttonClearTrack.onClick.AddListener(onButtonClearTrack);
      trackLabel.text = trackLabelDict[trackType];
    }

    void Update() {
      if (draggingItem != null) {
        updateDraggingItem();
      }
      else {
        updateReticule();
      }
    }


    #region pointer handler

    GameObject Reticule {
      get {
        if (reticule == null) {
          reticule = GameObject.Instantiate<GameObject>(parentController.reticule);
          reticule.transform.SetParent(transform);
          reticule.transform.localScale = Vector3.one;
          reticule.transform.localPosition = Vector3.zero;
          reticule.SetActive(false);
        }
        return reticule;
      }
    }

    private float updateAlpha(Vector3 localPos, Graphic graphic, float minAlpha) {
      float normY = localPos.y / GetComponent<RectTransform>().GetHeight() * 2f;
      float absNY = Mathf.Abs(normY);
      float a = 1f;
      if (absNY > thresh1) {
        a = Mathf.Lerp(1f, minAlpha, Mathf.InverseLerp(thresh1, thresh2, absNY));
      }
      graphic.color = new Color(1f, 1f, 1f, a);

      return normY;
    }

    private void updateReticule() {
      if (Reticule.activeSelf) {
        Reticule.transform.position = Input.mousePosition;
        Vector3 lp = Reticule.transform.localPosition;
        reticuleNormY = updateAlpha(lp, Reticule.GetComponent<Image>(), 0f);
        lp.y = lp.z = 0;
        Reticule.transform.localPosition = lp;
      }
    }

    private void updateDraggingItem() {
      draggingItem.transform.position = Input.mousePosition;
      Vector3 lp = draggingItem.transform.localPosition;
      draggingItemNormY = updateAlpha(lp, draggingItem.GetComponent<Image>(), 0.2f);
      draggingItem.GetComponent<asTrackItem>().label.gameObject.SetActive(Mathf.Abs(draggingItemNormY) < thresh2);
    }

    public void OnPointerDown(PointerEventData ped) {
      if (parentController.Playing) {
        return;
      }

      if (trackType == asTrackType.ACTION) {
        return;
      }

      for (int n = 0; n < itemsContainer.childCount; ++n) {
        RectTransform rt = itemsContainer.GetChild(n).GetComponent<RectTransform>();
        if (RectTransformUtility.RectangleContainsScreenPoint(rt, ped.position)) {
          draggingItem = rt;
          draggingItem.GetComponent<asTrackItem>().label.gameObject.SetActive(true);
        }
      }

      if (draggingItem == null) {
        Reticule.SetActive(true);
        updateReticule();
      }
    }

    public void OnPointerUp(PointerEventData ped) {
      if (parentController.Playing) {
        #if false
        if (!parentController.Recording) {
          parentController.PlayHeadTime = parentController.timeForWorldPosition(ped.position);
        }
        #endif
        return;
      }

      // todo: make this fn virtual and override it in asActionTrackCtonrller.
      if (trackType == asTrackType.ACTION) {
        parentController.showRecordPopUp();
        return;
      }

      if (draggingItem != null) {
        draggingItem.GetComponent<asTrackItem>().label.gameObject.SetActive(false);
        if (Mathf.Abs(draggingItemNormY) > thresh2) {
          GameObject.Destroy(draggingItem.gameObject);
        }
        else {
          draggingItem.localPosition = Vector3.Scale(draggingItem.localPosition, vFilterX);
        }
        draggingItem = null;
      }
      else {
        Reticule.SetActive(false);
        if (Mathf.Abs(reticuleNormY) < thresh2) {
          dropNew();
        }
      }
    }

    List<asTrackItemVO> itemsColor    = null;
    List<asTrackItemVO> itemsSoundCat = null;

    private List<asTrackItemVO> ItemsColor {
      get {
        if (itemsColor == null) {
          itemsColor = new List<asTrackItemVO>();
          itemsColor.Add(new asTrackItemVO(trBehaviorType.COLOR_OFF));
          itemsColor.Add(new asTrackItemVO(trBehaviorType.COLOR_RED));
          itemsColor.Add(new asTrackItemVO(trBehaviorType.COLOR_ORANGE));
          itemsColor.Add(new asTrackItemVO(trBehaviorType.COLOR_YELLOW));
          itemsColor.Add(new asTrackItemVO(trBehaviorType.COLOR_GREEN));
          itemsColor.Add(new asTrackItemVO(trBehaviorType.COLOR_CYAN));
          itemsColor.Add(new asTrackItemVO(trBehaviorType.COLOR_BLUE));
          itemsColor.Add(new asTrackItemVO(trBehaviorType.COLOR_MAGENTA));
          itemsColor.Add(new asTrackItemVO(trBehaviorType.COLOR_WHITE));
        }
        return itemsColor;
      }
    }

    private List<asTrackItemVO> ItemsSoundCat {
      get {
        if (itemsSoundCat == null) {
          itemsSoundCat = new List<asTrackItemVO>();

          foreach (trBehaviorType trBT in trRobotSounds.Instance.getUserFacingCategories(piRobotType.DASH)) {
            itemsSoundCat.Add(new asTrackItemVO(trBT));
          }

        }
        return itemsSoundCat;
      }
    }

    private List<asTrackItemVO> SoundItems(trBehaviorType trBT) {
      List<asTrackItemVO> ret = new List<asTrackItemVO>();
      foreach (trRobotSound trRS in trRobotSounds.Instance.GetCategory(trBT, piRobotType.DASH)) {
        ret.Add(new asTrackItemVO(trRS));
      }
      return ret;
    }

    private void dropNew() {
      parentController.menuController.onSelectionDelegate = onMenuSelection;
      if (trackType == asTrackType.LIGHTS) {
        parentController.menuController.populate(ItemsColor);
        parentController.menuController.label.text = "Robot Lights!";
        parentController.menuController.UseConfirm = true;
        parentController.menuController.OnBackDelegate = null;
      }
      else if (trackType == asTrackType.SOUNDS) {
        setupMenuSoundCats();
      }
      else {
        WWLog.logError("unhandled tracktype: " + trackType.ToString());
        return;
      }

      parentController.menuController.gameObject.SetActive(true);
    }

    private void setupMenuSoundCats() {
      parentController.menuController.populate(ItemsSoundCat);
      parentController.menuController.label.text = "Robot Sounds!";
      parentController.menuController.UseConfirm = false;
      parentController.menuController.OnBackDelegate = null;
    }

    private void onMenuSelection(asTrackItemVO asTIVO) {
      if (asTIVO.IsSoundCat) {
        parentController.menuController.populate(SoundItems(asTIVO.BehaviorType));
        parentController.menuController.label.text = trBehavior.TypeToUserFacingName[asTIVO.BehaviorType];
        parentController.menuController.gameObject.SetActive(true);
        parentController.menuController.UseConfirm = true;
        parentController.menuController.OnBackDelegate = setupMenuSoundCats;
      }
      else {
        asTrackItem asTI = GameObject.Instantiate<asTrackItem>(parentController.trackItem);
        asTI.transform.SetParent(itemsContainer);
        asTI.transform.localScale = Vector3.one;
        asTI.transform.position = Reticule.transform.position;
        asTI.VO = asTIVO;
        asTI.label.gameObject.SetActive(false);
      }
    }

    #endregion

    protected virtual void onButtonClearTrack() {
      piUnityUtils.destroyAllChildren(itemsContainer);
    }
  }
}
