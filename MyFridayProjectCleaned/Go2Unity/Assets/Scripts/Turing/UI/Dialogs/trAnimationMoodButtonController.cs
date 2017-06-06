using UnityEngine;
using System.Collections;
using WW.UGUI;
using UnityEngine.UI;

namespace Turing{
  public class trAnimationMoodButtonController : uGUISegment {
   
    public Image Icon;
    public GameObject Cover;
    public Text DescriptionText;
    public string valueText;
    
    public override void Init (){
      base.Init ();

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
      base.OnClickSegment ();
    }

  }
}
