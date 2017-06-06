using UnityEngine;
using System.Collections;
using WW.UGUI;
using UnityEngine.UI;

namespace Turing{
  public class trTriggerButtonController : uGUISegment {

    public Text Label;
    public Image Icon;
    public Image LockImage;
    public Image WarningImage;
    public GameObject LockModal;

    private trTrigger trigger;
    public trTrigger TriggerData{
      set{
        if(trigger == value){
          return;
        }

        trigger = value;
        setupView();
      }
      get{
        return trigger;
      }
    }

    public GameObject Cover;

    public override void Init (){
      base.Init ();

    }

    void setupView(){
         
      Icon.sprite = trIconFactory.GetIcon(trigger.Type);
      Color c = Icon.color;
      c.a = trigger.isLocked() ? 0.25f : 1.0f;
      LockImage.gameObject.SetActive(trigger.isLocked());   
      Icon.color = c;
    }


    public bool IsEnabled{
      set{
        Cover.SetActive(!value);
      }
      get{
        return !Cover.activeSelf;
      }
    }

    public override void OnClickSegment () {
      if(!IsEnabled){
        return;
      }
      SoundManager.soundManager.PlaySound(SoundManager.trAppSound.UI_SOUND);
      base.OnClickSegment ();
    }

  }
}
