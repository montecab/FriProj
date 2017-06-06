using UnityEngine;
using System.Collections;
using DG.Tweening;

public class uGUIPanelTween : Singleton<uGUIPanelTween> {

  public void TweenSetActive(bool active, Transform obj, float delay = 0 ){
    if(active){
      TweenOpen(obj, delay);
    }else{
      TweenClose(obj, delay);
    }
  }

  public void TweenOpen(Transform panel, float delay = 0){
    StartCoroutine(tweenActive(panel, true, delay));
    panel.DOKill();
    panel.localScale = Vector3.one * 0.01f;
    panel.DOScale(Vector3.one, 0.4f).SetDelay(delay).SetEase(Ease.OutBack);
  }

  public void TweenClose(Transform panel, float delay = 0){
    panel.DOKill();
    panel.localScale = Vector3.one;
    panel.DOScale(Vector3.one * 0.01f, 0.2f).SetDelay(delay);
    StartCoroutine(tweenActive(panel, false, delay + 0.2f));
  }

  IEnumerator tweenActive(Transform panel, bool active, float delay){
    yield return new WaitForSeconds(delay);
    panel.gameObject.SetActive(active);
  }

}
