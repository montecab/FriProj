using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using Turing;


public class trFTUEController : MonoBehaviour {

  private static readonly float NOTIFICATION_DELAY = 3f;

  [SerializeField]
  private Image _backgroundSkrim;
  [SerializeField]
  private RectTransform _notification;
  [SerializeField]
  private RectTransform _hitBoxRect;
  [SerializeField]
  private Button _hitBoxButton;
  [SerializeField]
  private Button _playButton;
  [SerializeField]
  private RectTransform _speechBubble;
  [SerializeField]
  private TextMeshProUGUI _description;
  [SerializeField]
  private GameObject _characterLeft;
  [SerializeField]
  private GameObject _characterRight;

  public void SetupView(FTUEType type, UnityAction callback = null){
    FTUEModel model = Resources.Load<FTUEModel>(FTUEManager.FTUE_MODELS_PATH+type.ToString());
    FromFTUEModel(model);
    if(type==FTUEType.LOBBY_SCROLLQUEST || type==FTUEType.MAIN_PLAY_BUTTON){
      string robotName = (trCurRobotController.Instance.CurRobot==null)?wwLoca.Format("@!@The robot@!@"):trCurRobotController.Instance.CurRobot.Name;
      if (TMP_TextUtilities.HasUnsupportedCharacters(robotName)) {
        WWLog.logDebug("Robot name has unsupported characters.  Replacing with Dash/Dot");
        robotName = wwLoca.Format(trCurRobotController.Instance.CurRobot.robotType == piRobotType.DASH ? "Dash" : "Dot");
      }
      _description.text = wwLoca.Format(FTUEManager.FTUE_STRING_TABLE[type], robotName);
    }
    else{
      _description.text = wwLoca.Format(FTUEManager.FTUE_STRING_TABLE[type]);
    }
    _hitBoxButton.onClick.RemoveAllListeners();
    _playButton.onClick.RemoveAllListeners();
    UnityAction action = ()=>{
      if(callback!=null){
        callback();
      }
      piConnectionManager.Instance.showChromeButton();
      FTUEManager.Instance.MoveToNextFTUE();
      this.gameObject.SetActive(false);
    };
    _hitBoxButton.onClick.AddListener(action);
    _playButton.onClick.AddListener(action);
  }

  public void FromFTUEModel(FTUEModel model){
    _backgroundSkrim.sprite = model.backgroundSkrim;
    _backgroundSkrim.color = model.backgroundColor;
    _playButton.gameObject.SetActive(model.enablePlayButton);
    _hitBoxButton.gameObject.SetActive(model.enableHitboxButton);
    _notification.gameObject.SetActive(model.enableNotification);

    if(model.enableNotification){
      StartCoroutine(DelaySetActive(_notification.gameObject, model.enableNotification, NOTIFICATION_DELAY));
    }
    if(model.enableNotification){
      _notification.anchoredPosition3D = model.notificationPos;
      _notification.pivot = model.notificationPivot;
      _notification.anchorMin = new Vector2(model.notificationAnchorsMinMax.x, model.notificationAnchorsMinMax.y);
      _notification.anchorMax = new Vector2(model.notificationAnchorsMinMax.z, model.notificationAnchorsMinMax.w);
    }
    if(model.enableHitboxButton){
      _hitBoxRect.anchoredPosition3D = model.hitboxPos;
      _hitBoxRect.pivot = model.hitboxPivot;
      _hitBoxRect.anchorMin = new Vector2(model.hitboxAnchorsMinMax.x, model.hitboxAnchorsMinMax.y);
      _hitBoxRect.anchorMax = new Vector2(model.hitboxAnchorsMinMax.z, model.hitboxAnchorsMinMax.w);
      _hitBoxRect.SetSize(model.hitboxSize);
    }
    _speechBubble.anchoredPosition3D = model.speechBubblePos;
    _speechBubble.pivot = model.speechBubblePivot;
    _speechBubble.anchorMin = new Vector2(model.speechBubbleAnchorsMinMax.x, model.speechBubbleAnchorsMinMax.y);
    _speechBubble.anchorMax = new Vector2(model.speechBubbleAnchorsMinMax.z, model.speechBubbleAnchorsMinMax.w);
    _speechBubble.SetSize(model.speechBubbleSize);

    _characterLeft.GetComponent<Image>().sprite = model.eliSprite;
    _characterRight.GetComponent<Image>().sprite = model.eliSprite;
    _characterLeft.SetActive(model.isLeft);
    _characterRight.SetActive(!model.isLeft);
  }

  private IEnumerator DelaySetActive(GameObject obj, bool active, float delayTime){
    yield return new WaitForSeconds(delayTime);
    obj.SetActive(active);
  }

  #if UNITY_EDITOR
  public FTUEModel ToFTUEModel(FTUEModel model){
    if(_characterLeft.activeSelf){
      model.eliSprite = _characterLeft.GetComponent<Image>().sprite;
    }
    else if(_characterRight.activeSelf){
      model.eliSprite = _characterRight.GetComponent<Image>().sprite;
    }
    model.enableNotification = _notification.gameObject.activeSelf;
    model.enableHitboxButton = _hitBoxButton.gameObject.activeSelf;
    model.enablePlayButton = _playButton.gameObject.activeSelf;
    model.isLeft = _characterLeft.activeSelf;
    model.backgroundSkrim = _backgroundSkrim.sprite;
    model.backgroundColor = _backgroundSkrim.color;

    model.speechBubblePos = _speechBubble.anchoredPosition3D;
    model.speechBubbleSize = _speechBubble.GetSize();
    model.speechBubblePivot = _speechBubble.pivot;
    model.speechBubbleAnchorsMinMax = new Vector4(_speechBubble.anchorMin.x, 
                                                  _speechBubble.anchorMin.y,
                                                  _speechBubble.anchorMax.x,
                                                  _speechBubble.anchorMax.y);
    if(model.enableNotification){
      model.notificationPos = _notification.anchoredPosition3D;
      model.notificationPivot = _notification.pivot;
      model.notificationAnchorsMinMax = new Vector4(_notification.anchorMin.x, 
                                                    _notification.anchorMin.y,
                                                    _notification.anchorMax.x,
                                                    _notification.anchorMax.y);
    }
    if(model.enableHitboxButton){
      model.hitboxPos = _hitBoxRect.anchoredPosition3D;
      model.hitboxSize = _hitBoxRect.GetSize();
      model.hitboxPivot = _hitBoxRect.pivot;
      model.hitboxAnchorsMinMax = new Vector4(_hitBoxRect.anchorMin.x, 
                                                    _hitBoxRect.anchorMin.y,
                                                    _hitBoxRect.anchorMax.x,
                                                    _hitBoxRect.anchorMax.y);
    }
    return model;
  }
  #endif

}
