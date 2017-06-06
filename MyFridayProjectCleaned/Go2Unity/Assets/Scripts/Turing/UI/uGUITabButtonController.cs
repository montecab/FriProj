using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

namespace WW.UGUI{
	public class uGUITabButtonController : uGUISegment {
		public GameObject InactiveCover;

    public TextMeshProUGUI ButtonText;
    public Color ActivateTextColor;
    public Color DeactivateTextColor;

    public Sprite ActiveSprite;
    public Sprite InactiveSprite;

		public override void Activate ()
		{
      Init(); //add init here because sometimes this is called before init and init will deactivate afterwards
			base.Activate ();
			if (InactiveCover) InactiveCover.SetActive(false);
      if(ButtonText != null) {
        ButtonText.color = ActivateTextColor;
        ButtonText.fontStyle = FontStyles.Bold;
      }

      if(ActiveSprite != null){
        SegButton.image.sprite = ActiveSprite;
      }
		}

		public override void Deactivate ()
		{
      Init(); //add init here because sometimes this is called before init and init will deactivate afterwards
			base.Deactivate ();
			if (InactiveCover) InactiveCover.SetActive(true);
      if(ButtonText != null) {
        ButtonText.color = DeactivateTextColor;
        ButtonText.fontStyle = FontStyles.Normal;
      }

      if(InactiveSprite != null){
        SegButton.image.sprite = InactiveSprite;
      }
        
		}
	}
}
