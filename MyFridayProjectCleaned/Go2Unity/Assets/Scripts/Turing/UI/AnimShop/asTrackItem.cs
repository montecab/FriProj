using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using Turing;

namespace AnimShop {
  public class asTrackItemVO {
    trBehaviorType behaviorType = trBehaviorType.DO_NOTHING;   // only one of these is set
    trRobotSound   sound        = null;                        // only one of these is set

    public asTrackItemVO(trBehaviorType color) {
      this.behaviorType = color;
    }

    public asTrackItemVO(trRobotSound sound) {
      this.sound = sound;
    }

    public trBehaviorType BehaviorType {
      get {
        return behaviorType;
      }
    }

    public trRobotSound Sound {
      get {
        return sound;
      }
    }

    public bool IsSound {
      get {
        return sound != null;
      }
    }

    public bool IsSoundCat {
      get {
        return behaviorType.IsSound();
      }
    }

    public bool IsColor {
      get {
        return (!IsSound) && (!IsSoundCat);
      }
    }

    public Sprite Icon {
      get {
        if (IsColor || IsSoundCat) {
          return trIconFactory.GetIcon(behaviorType);
        }
        else {
          return trIconFactory.GetIcon(sound.category);
        }
      }
    }

    public string UserFacingName {
      get {
        if (IsColor || IsSoundCat) {
          return trBehavior.TypeToUserFacingName[behaviorType];
        }
        else {
          return sound.UserFacingNameLocalized;
        }
      }
    }

    public void fire() {
      foreach (piBotBo bot in piConnectionManager.Instance.RobotsInState(PI.BotConnectionState.CONNECTED)) {
        if (IsColor) {
          fireColor(bot);
        }
        else if (IsSound) {
          fireSound(bot);
        }
      }
    }

    private void fireColor(piBotBo bot) {
      Color c = trBehavior.convertColorType(BehaviorType);
      bot.cmd_rgbLights(c.r, c.g, c.b);
    }

    private void fireSound(piBotBo bot) {
      bot.cmd_playSound(Sound.filename);
    }
  }

  public class asTrackItem : MonoBehaviour {
    public Image           imageIcon;
    public TextMeshProUGUI label;
    public Button          confirmButton;

    public asMenuController.selectionDelegate onSelectionDelegate;

    private asTrackItemVO   vo;

    void Start() {
      GetComponent<Button>().onClick.AddListener(onClick);
    }

    void Update() {
      transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one, 0.1f);
    }

    public asTrackItemVO VO {
      get {
        return vo;
      }
      set {
        vo = value;
        if (vo != null) {
          imageIcon.sprite = vo.Icon;
          label    .text   = vo.UserFacingName;
        }
        imageIcon.gameObject.SetActive(vo != null);
        label    .gameObject.SetActive(vo != null);
      }
    }

    private void onClick() {
      if (onSelectionDelegate != null) {
        onSelectionDelegate(vo);
      }
    }

    public void fire() {
      transform.localScale *= 1.6f;
      vo.fire();
    }
  }
}
