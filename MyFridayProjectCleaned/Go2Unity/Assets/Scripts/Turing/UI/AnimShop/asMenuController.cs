using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnimShop {

  public class asMenuController : MonoBehaviour {

    public delegate void selectionDelegate(asTrackItemVO selected);
    public selectionDelegate onSelectionDelegate;
    public delegate void backDelegate();
    private backDelegate      onBackDelegate;

    public TextMeshProUGUI label;
    public GameObject      contextImage;
    public Button          buttonBack;
    public Button          buttonClose;
    public Button          buttonClickBlocker;
    public GameObject      spareParts;
    public asTrackItem     exampleTrackItem;
    public Image           confirmCheckmark;
    public Transform       itemList;

    private       bool           useConfirm = false;
    public        bool           UseConfirm { get {return useConfirm;} set {useConfirm = value; updateConfirmVisibility();} }

    private       float          lastClickTime          = 0;
    private const float          DOUBLE_ACTIVATION_TIME = 0.5f;

    void Start() {
      if (buttonBack != null) {
        buttonBack        .onClick.AddListener(onClickBack);
      }

      if (buttonClose != null) {
        buttonClose       .onClick.AddListener(onClickClose);
      }

      if (exampleTrackItem != null) {
        exampleTrackItem.GetComponent<Button>().onClick.AddListener(onClickConfirm);
      }

      if (buttonClickBlocker != null) {
        buttonClickBlocker.onClick.AddListener(onClickClose);
      }

      if (spareParts != null) {
        spareParts.SetActive(false);
      }


      OnBackDelegate = null;

      updateConfirmVisibility();
    }

    public void populate(List<asTrackItemVO> asTIVOs) {
      depopulate();

      foreach (asTrackItemVO asTIVO in asTIVOs) {
        addItem(asTIVO);
      }

      setSelection(null);
    }

    private void addItem(asTrackItemVO asTIVO) {
      asTrackItem asTI              = GameObject.Instantiate<asTrackItem>(exampleTrackItem);
      asTI.gameObject.SetActive(true);
      asTI.GetComponent<Button>().enabled      = true;
      asTI.GetComponent<Button>().interactable = true;
      asTI.VO                       = asTIVO;
      asTI.transform.SetParent(itemList);
      asTI.transform.localScale     = Vector3.one;
      asTI.transform.localPosition  = Vector3.zero;
      asTI.confirmButton.gameObject.SetActive(false);
      asTI.onSelectionDelegate     += myOnSelectionDelegate;
    }

    private void depopulate() {
      piUnityUtils.destroyAllChildren(itemList);
    }

    public backDelegate OnBackDelegate {
      set {
        onBackDelegate = value;
        if (buttonBack != null) {
          buttonBack.gameObject.SetActive(onBackDelegate != null);
        }
      }
    }


    void onClickBack() {
      if (onBackDelegate != null) {
        onBackDelegate();
      }
      else {
        WWLog.logError("no back delegate");
      }
    }

    void onClickClose() {
      gameObject.SetActive(false);
    }

    void updateConfirmVisibility() {
      if (exampleTrackItem != null) {
        exampleTrackItem.gameObject.SetActive(useConfirm);
      }
    }

    void setSelection(asTrackItemVO asTIVO) {
      exampleTrackItem.VO = asTIVO;

      exampleTrackItem.GetComponent<Button>().interactable = (asTIVO != null);
      confirmCheckmark.gameObject.SetActive(asTIVO != null);

      if (asTIVO != null) {
        asTIVO.fire();
      }
    }

    void myOnSelectionDelegate(asTrackItemVO asTIVO) {
      if (useConfirm) {
        // detect double-click
        float dt = Time.time - lastClickTime;
        if (dt < DOUBLE_ACTIVATION_TIME) {
          finish(asTIVO);
        }
        else {
        setSelection(asTIVO);
          lastClickTime = Time.time;
        }
      }
      else {
      finish(asTIVO);
      }
    }

    void onClickConfirm() {
      finish(exampleTrackItem.VO);
    }

    void finish(asTrackItemVO asTIVO) {
      gameObject.SetActive(false);
      if (onSelectionDelegate != null) {
        onSelectionDelegate(asTIVO);
      }
      else {
        WWLog.logError("no selection delegate");
      }
    }
  }
}
