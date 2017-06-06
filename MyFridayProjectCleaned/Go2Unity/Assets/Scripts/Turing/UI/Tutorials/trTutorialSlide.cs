using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;

namespace Turing {
  public class trTutorialSlide : MonoBehaviour {
  
    // do not show this slide if this trAppOption has value NO.
    // this can be set in the unity Scene Editor.
    public trMultivariate.trAppOption skipIfNO = trMultivariate.trAppOption.NULL_OPTION;

    public GameObject[] AnimationObjs = new GameObject[0];
  
    public virtual void fadeOut() {
      gameObject.SetActive(false);
    }
  
    public virtual void fadeIn() {
      gameObject.SetActive(true);
      for(int i = 0; i< AnimationObjs.Length; ++i){
        AnimationObjs[i].transform.DOKill();
        AnimationObjs[i].transform.localScale = Vector3.one * 0.01f;
        AnimationObjs[i].transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack);
      }
    }
  }
}
