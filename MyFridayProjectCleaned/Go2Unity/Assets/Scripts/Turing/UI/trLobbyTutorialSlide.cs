using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;

namespace Turing {
  public class trLobbyTutorialSlide : trTutorialSlide {
  
    public RawImage   Skrim;
    
    public override void fadeOut() {
      Skrim.color = new Color(1, 1, 1, 1);
      DOTween.To(()=>  Skrim.color, x=>  Skrim.color = x, new Color(1, 1, 1, 0), 0.4f);
      piUnityDelayedExecution.Instance.delayedExecution0(base.fadeOut, 0.4f);
    }
  
    public override void fadeIn() {
      gameObject.SetActive(true);
      Skrim.color = new Color(1, 1, 1, 0);
      DOTween.To(()=>  Skrim.color, x=>  Skrim.color = x, new Color(1, 1, 1, 1), 0.4f);
      base.fadeIn();
    }
  }
}
