using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

namespace WW.UGUI{
  public class uGUIDragDrop : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {

    public GameObject itemDragged;
    public bool dragOnSurfaces = true;
    protected Vector3 startWorldPos;
    protected Vector3 startLocalPos;
    protected RectTransform draggingPlane;

    public RectTransform DraggingLimitPanel;

    protected Vector3[] panelCorners = new Vector3[4];
    protected Vector3 dragTransformedPosition = Vector3.zero;

    public enum DraggingDirectionConstraint{
      NONE,
      HORIZONTAL,
      VERTICAL
    }

    public DraggingDirectionConstraint DirectionConstraint = DraggingDirectionConstraint.NONE;


    #region IBeginDragHandler implementation

    public virtual void OnBeginDrag (PointerEventData eventData)
    {
      itemDragged = gameObject;
      startWorldPos = transform.position;
      startLocalPos = transform.localPosition;
      itemDragged.transform.SetAsLastSibling();

      // The icon will be under the cursor.
      // We want it to be ignored by the event system.
      SetBlockRaycast(false);
      if(DraggingLimitPanel != null){
        DraggingLimitPanel.GetWorldCorners(panelCorners);
      }


    }

    #endregion

    #region IDragHandler implementation

    public virtual void OnDrag (PointerEventData eventData)
    {
      if (dragOnSurfaces && eventData.pointerEnter != null && eventData.pointerEnter.transform as RectTransform != null){
        draggingPlane = eventData.pointerEnter.transform as RectTransform;
      }       

      RectTransform rt = itemDragged.GetComponent<RectTransform>();
      Vector3 globalMousePos;
      Vector2 pos = ClampViewPosition(eventData.position);
      if (RectTransformUtility.ScreenPointToWorldPointInRectangle(draggingPlane, pos, eventData.pressEventCamera, out globalMousePos))
      {

        if(DirectionConstraint == DraggingDirectionConstraint.VERTICAL){
          globalMousePos.x = rt.position.x;
        }
        else if(DirectionConstraint == DraggingDirectionConstraint.HORIZONTAL){
          globalMousePos.y = rt.position.y;
        }

        dragTransformedPosition = globalMousePos;

        if(DraggingLimitPanel != null){
          globalMousePos = piMathUtil.clampVec(globalMousePos, panelCorners[0], panelCorners[2]);
        }

        rt.position = globalMousePos;
      }

    }


    public virtual Vector2 ClampViewPosition(Vector2 screenPos){
      float x = Mathf.Clamp(screenPos.x, 0, Screen.width);
      float y = Mathf.Clamp(screenPos.y, 0, Screen.height);
      return new Vector2(x, y);
    }

    #endregion
   
    #region IEndDragHandler implementation
    public virtual void OnEndDrag (PointerEventData eventData)
    {
      //NOTE: end dragging happens after dropping
      SetBlockRaycast(true);
      itemDragged = null;
      dragTransformedPosition = Vector3.zero;
    }
    #endregion
   
    public void GoBackToStartWorldPos(){
      itemDragged.gameObject.transform.position = startWorldPos;
    }

    public void GoBackToStartLocalPos(){
      itemDragged.gameObject.transform.localPosition = startLocalPos;
    }

    public void SetBlockRaycast(bool isBlock){
      if(itemDragged == null){
        return;
      }
      CanvasGroup group = itemDragged.gameObject.GetComponent<CanvasGroup>();
      if(group == null){
        group = itemDragged.gameObject.AddComponent<CanvasGroup>();
      }
     
      group.blocksRaycasts = isBlock;

    }
  }
}
