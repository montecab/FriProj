using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using WW;

namespace WW.UGUI{
  public class uGUICreateDragDrop   : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
  {
    public bool dragOnSurfaces = true;
    public GameObject GeneratePrefab;
    
    protected GameObject draggingObj;
    private RectTransform draggingPlane;
    private Canvas canvas;

    
    public virtual void OnBeginDrag(PointerEventData eventData)
    {
      if(canvas == null){
        canvas = wwUtility.FindInParents<Canvas>(gameObject);
      }
      
      // We have clicked something that can be dragged.
      // What we want to do is create an obj for this.
      draggingObj = Instantiate(GeneratePrefab, this.gameObject.transform.position, this.gameObject.transform.rotation) as GameObject;
      
      draggingObj.transform.SetParent (canvas.transform, false);
      draggingObj.transform.SetAsLastSibling();

      //the size will be set to 0 when instantiate under gridlayout, so reset it back
      draggingObj.GetComponent<RectTransform>().sizeDelta = GeneratePrefab.GetComponent<RectTransform>().sizeDelta;

      // The icon will be under the cursor.
      // We want it to be ignored by the event system.
      CanvasGroup group = draggingObj.AddComponent<CanvasGroup>();
      group.blocksRaycasts = false;
      
      if (dragOnSurfaces)
        draggingPlane = transform as RectTransform;
      else
        draggingPlane = canvas.transform as RectTransform;
      
      SetDraggedPosition(eventData);
    }
    
    public virtual void OnDrag(PointerEventData data)
    {
      if (draggingObj != null){
        SetDraggedPosition(data);
      }
        
    }
    
    private void SetDraggedPosition(PointerEventData data)
    {
      if (dragOnSurfaces && data.pointerEnter != null && data.pointerEnter.transform as RectTransform != null)
        draggingPlane = data.pointerEnter.transform as RectTransform;
      
      var rt = draggingObj.GetComponent<RectTransform>();
      Vector3 globalMousePos;
      if (RectTransformUtility.ScreenPointToWorldPointInRectangle(draggingPlane, data.position, data.pressEventCamera, out globalMousePos))
      {
        rt.position = globalMousePos;
      }
    }
    
    public virtual void OnEndDrag(PointerEventData eventData)
    {
      if (draggingObj != null){
        Destroy(draggingObj.GetComponent<CanvasGroup>());
      }
    }
    

  }
}
