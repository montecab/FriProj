using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace Turing{
  public class trProfileItemController : MonoBehaviour{

    private bool locked;
    public bool Locked{
      get{
        return locked;
      }
      set{
        locked = value;
        updateUI();
      }
    }
    public Image Icon;
    public Image LockImage;
    public trMomentaryButton MomentaryButton;    
    public string Data;
    public trTooltipPanelController Tooltip;
    public Transform TooltipLayer;
    public string TooltipText;

    private Vector3 stashedTooltipPosition;
    private Transform stashedTooltipParent;

    void Start(){
      MomentaryButton.OnPointerDownEvent += showTooltip;
      MomentaryButton.OnPointerUpEvent += hideTooltip;
      stashedTooltipPosition = Tooltip.transform.localPosition;
      stashedTooltipParent = Tooltip.transform.parent;
    }

    void OnDestroy(){
      if (MomentaryButton != null) {
        MomentaryButton.OnPointerDownEvent -= showTooltip;
        MomentaryButton.OnPointerUpEvent -= hideTooltip;
      }
    }

    void updateUI(){
      Color iconColor = Icon.color;
      iconColor.a = locked ? 0.25f : 1f;
      Icon.color = iconColor;
      LockImage.gameObject.SetActive(locked);
    }

    void showTooltip() {
      Tooltip.Display(TooltipText);
      Tooltip.transform.SetParent(stashedTooltipParent, true);
      Tooltip.transform.localPosition = stashedTooltipPosition;
      Tooltip.transform.SetParent(TooltipLayer, true);
    }

    void hideTooltip() {
      Tooltip.Close();
    }
  }  
}

