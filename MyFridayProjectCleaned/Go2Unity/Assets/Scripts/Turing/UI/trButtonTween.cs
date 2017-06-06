using UnityEngine;
using System.Collections;
using DG.Tweening;
using UnityEngine.EventSystems;

namespace Turing{
  public class trButtonTween : MonoBehaviour  {

  	// Use this for initialization
  	void Start () {
      // Initialize DOTween (needs to be done only once).
      // If you don't initialize DOTween yourself,
      // it will be automatically initialized with default values.
      DOTween.Init(false, true, LogBehaviour.ErrorsOnly);
  	}

    public void Shake(){
      this.transform.DOKill();
      this.transform.localScale = Vector3.one;
      this.transform.DOShakeScale(0.7f);
    }

    public void TweenDragged(){
      this.transform.DOKill();
      this.transform.DOScale(new Vector3(1.5f, 1.5f, 1.5f), 0.1f);
    }

    public void TweenDrop(){
      this.transform.DOKill();
      this.transform.DOScale(new Vector3(1.0f, 1.0f, 1.0f), 0.1f);
    }

    public void TweenSmall(){
      this.transform.DOKill();
      this.transform.DOScale(new Vector3(0.5f, 0.5f, 0.5f), 0.1f);
    }
  }
}
