using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;

namespace Turing{
  public class trNewMissionUnlockPanelController : MonoBehaviour {
    public CanvasGroup ShowingPanel;

    private bool isShowing = false;
    private float startPosY = float.NaN;

    public void Show(){
      if(!trDataManager.Instance.IsAllowShowNewMissionPanel){
        return;
      }
      if(isShowing){
        return;
      }
      isShowing = true;
      this.gameObject.SetActive(true);
      ShowingPanel.transform.DOKill();
     
      StartCoroutine(showAndFade());
    }

    IEnumerator showAndFade(){
      if(float.IsNaN(startPosY)){
        startPosY = ShowingPanel.transform.localPosition.y ;
      }
      ShowingPanel.transform.localPosition = new Vector3(ShowingPanel.transform.localPosition.x,
                                                         startPosY,
                                                         ShowingPanel.transform.localPosition.z);
      ShowingPanel.alpha = 1;
      ShowingPanel.transform.DOLocalMoveY(startPosY - 100, 1.0f);

      yield return new WaitForSeconds(2.0f);

      DOTween.To( x=> ShowingPanel.alpha = x,1, 0, 1);
      yield return new WaitForSeconds(1.0f);
      isShowing = false;
      this.gameObject.SetActive(false);

    }
  }
}