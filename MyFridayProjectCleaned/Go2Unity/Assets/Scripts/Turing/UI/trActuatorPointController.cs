using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using WW.UGUI;

namespace Turing{
  public class trActuatorPointController :  uGUIDragDrop{

    private static float DragOutToDeleteTreshold = 40;

    private Vector2 mNormalizedValue;
    public Vector2 NormalizedValue{
      get{
        return mNormalizedValue;
      }
      set{
        setValue(value);
      }
    }

    public uGUISnapPoints SnapPointCtrl;
    public RectTransform ValidPanel;
    public trActuatorConfigPanelController ConfigCtrl;

    [System.Serializable]
    public class PointEvent : UnityEvent<Vector2, trActuatorPointController> {}
    public PointEvent OnValueChanged = new PointEvent();
    
    public bool CanDelete = true;

    public trProtoController ProtoCtrl;

    void setValue(Vector2 value){
      value.y = SnapPointCtrl.SnappedValueNormalized(value.y);
      mNormalizedValue = value;

      updateView();

      OnValueChanged.Invoke(NormalizedValue, this);
    }

    void updateView(){
      this.transform.position = ValidPanel.XYRatioToWorldPos(mNormalizedValue);
    }

    public void SetUpAnchor(){
      Vector3 localPos = this.transform.localPosition;
      this.GetComponent<RectTransform>().anchorMin = new Vector2(mNormalizedValue.x,0.5f);
      this.GetComponent<RectTransform>().anchorMax = new Vector2(mNormalizedValue.x,0.5f);
      this.transform.localPosition = localPos;
    }

    public void UpdateValue(){
      Vector2 value = ValidPanel.GetXYRatioWithWorldPos(this.transform.position);
      if(DirectionConstraint == DraggingDirectionConstraint.VERTICAL){
        value = new Vector2(NormalizedValue.x, value.y);
      }
      if(trActuatorPoints.IsPointValid(value)){
        NormalizedValue = value;
      }
  
    }

    private bool shouldRemoveSnapPoint(Vector3 position){

      if (!DraggingLimitPanel){
        return false;
      }

      float diffOutRectX = 0;
      float diffOutRectY = 0;
      if (position.x < panelCorners[0].x){
        diffOutRectX = panelCorners[0].x - position.x;
      } else if (position.x > panelCorners[2].x){
        diffOutRectX = position.x - panelCorners[2].x;
      }

      if (position.y < panelCorners[0].y){
        diffOutRectY = panelCorners[0].y - position.y;
      } else if (position.y > panelCorners[2].y){
        diffOutRectY = position.y - panelCorners[2].y;
      }

      bool result = Mathf.Max(diffOutRectX, diffOutRectY) > DragOutToDeleteTreshold;

      return result;
    }

    public override void OnBeginDrag (PointerEventData eventData)
    {
      base.OnBeginDrag (eventData);
      this.transform.SetParent(ProtoCtrl.BehaviorMakerPanelCtrl.transform);
    }
    public override void OnDrag (PointerEventData eventData)
    {
      base.OnDrag(eventData);
      UpdateValue();

      if (CanDelete && shouldRemoveSnapPoint(dragTransformedPosition)){
        ConfigCtrl.DeletePoint(this);
        eventData.Reset();
      }
    }

    public override void OnEndDrag (PointerEventData eventData)
    {
      base.OnEndDrag (eventData);

      Vector2 value = ValidPanel.GetXYRatioWithWorldPos(this.transform.position);
      value.x = Mathf.Clamp01(value.x);
      value.y = Mathf.Clamp01(value.y);
      NormalizedValue = value;

      SetUpAnchor();
      this.transform.SetParent(ValidPanel.transform);
    }
  }
}
