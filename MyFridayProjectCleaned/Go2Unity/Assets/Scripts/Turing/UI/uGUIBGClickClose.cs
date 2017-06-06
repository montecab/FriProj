using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

namespace WW.UGUI{
  public class uGUIBGClickClose : MonoBehaviour , IPointerClickHandler{

    #region IPointerClickHandler implementation
    public void OnPointerClick (PointerEventData eventData)
    {
      this.gameObject.SetActive(false);
    }
    #endregion
  }
}
