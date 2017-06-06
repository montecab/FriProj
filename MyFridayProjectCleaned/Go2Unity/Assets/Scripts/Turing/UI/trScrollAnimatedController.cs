using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;

namespace Turing{
  public class trScrollAnimatedController : MonoBehaviour, IEndDragHandler {

    public ScrollRect TargetScrollRect;
    public RectTransform ContentRectTransform;

    public bool IsSaveScrollPosition = true;

    public bool IsPointVisible(Vector2 point){

      Vector2 contentSize = ContentRectTransform.GetSize();

      Rect visibleRect = new Rect(
        new Vector2(contentSize.x * TargetScrollRect.normalizedPosition.x, contentSize.y * TargetScrollRect.normalizedPosition.y),
        TargetScrollRect.GetComponent<RectTransform>().GetSize() / ContentRectTransform.localScale.x
      );
      visibleRect.position -= visibleRect.size / 2;

      return visibleRect.Contains(point);
    }

    public void ScrollToMakeRectVisible(Vector3 position, float duration){
      Vector2 contentSize = ContentRectTransform.GetSize();
      Vector2 targetPoint = new Vector2(position.x / contentSize.x, position.y / contentSize.y);

      DOTween.To(
        () => TargetScrollRect.normalizedPosition,
        point => TargetScrollRect.normalizedPosition = point,
        targetPoint,
        duration);
    }

    #region IEndDragHandler implementation

    public void OnEndDrag (PointerEventData eventData)
    {
      if(!IsSaveScrollPosition){
        return;
      }
      trDataManager.Instance.GetCurProgram().ScrollPosition =  
        new Vector2(ContentRectTransform.transform.localPosition.x,
          ContentRectTransform.transform.localPosition.y);
      trDataManager.Instance.SaveCurProgram();


    }

    #endregion
  }

 

}
