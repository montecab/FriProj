using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using WW.UGUI;
using WW;

namespace Turing{
  public class trActuatorConfigPanelController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler , IDragHandler{

    private float timer = float.NaN;
    private PointerEventData mouseData;
    private float saveTimer = float.NaN;

    public List<trActuatorPointController> PointsUI = new List<trActuatorPointController>();
    public List<uGUIContentScaler> Lines = new List<uGUIContentScaler>();

    public GameObject LinePrefab;
    public GameObject PointPrefab;
    public trProtoController ProtoCtrl;
    public bool IsSimpleMode = false;

    public trActuatorPoints acPoints;
    public trActuatorPoints AcPoints{
      set{
        if(wwUtility.SetClass(ref acPoints, value)){
          updateView();
        }
      }
      get{
        return acPoints;
      }
    }

    private bool isInverse = false;
    public bool IsInverse{
      set{
        if(wwUtility.SetStruct(ref isInverse, value)){
          updateView();
        }
      }
      get{
        return isInverse;
      }
    }

    public trMapperPanelController MapCtrl;

    public GameObject FakeStartPoint;
    public GameObject FakeEndPoint;
    public uGUIContentScaler FakeStartLine;
    public uGUIContentScaler FakeEndLine;

    private const float HOLD_TIME = 0.5f;

    private trActuatorPointController newPointDrag = null;

    void Start(){

      updateLines();
    }

    public void Reset(){
      throw new System.NotImplementedException (); 
    }

    void updateView(){
      updatePoints();
      updateLines();
    }

    public void DeletePoint(trActuatorPointController point){
      int id = PointsUI.IndexOf(point);
      acPoints.RemoveAt(id);
      updateView();
      trDataManager.Instance.SaveBehavior();
    }


    void updatePoints(){
      for(int i = 0; i< PointsUI.Count; ++i){
        Destroy(PointsUI[i].gameObject);
      }
      PointsUI.Clear();

      for(int i = 0; i< acPoints.Points.Count; ++i){
        Vector2 value = acPoints.Points[i];
        createPoint(value);
      }

      PointsUI[0].DirectionConstraint = uGUIDragDrop.DraggingDirectionConstraint.VERTICAL;
      PointsUI[0].DraggingLimitPanel = this.GetComponent<RectTransform>();
      PointsUI[PointsUI.Count -1].DirectionConstraint = uGUIDragDrop.DraggingDirectionConstraint.VERTICAL;
      PointsUI[PointsUI.Count -1].DraggingLimitPanel = this.GetComponent<RectTransform>();
      PointsUI[0].CanDelete = false;
      PointsUI[PointsUI.Count -1].CanDelete = false;

      //set fake points and lines view
      setFakePoints();
      setFakeLines();

    }

    void setFakePoints(){
      trActuatorPointController end = isInverse? PointsUI[0] : PointsUI[PointsUI.Count -1];
      trActuatorPointController start = isInverse?  PointsUI[PointsUI.Count -1]:PointsUI[0];
      FakeStartPoint.transform.position = new Vector3(FakeStartPoint.transform.position.x, 
                                                      start.transform.position.y,
                                                      FakeStartPoint.transform.position.z);
      FakeEndPoint.transform.position = new Vector3(FakeEndPoint.transform.position.x, 
                                                    end.transform.position.y,
                                                    FakeEndPoint.transform.position.z);

      FakeStartPoint.GetComponent<trButtonBase>().Img.color = start.GetComponent<trButtonBase>().Img.color;
      FakeEndPoint.GetComponent<trButtonBase>().Img.color = end.GetComponent<trButtonBase>().Img.color; 
    }

    void setFakeLines(){
      trActuatorPointController end = isInverse? PointsUI[0] : PointsUI[PointsUI.Count -1];
      trActuatorPointController start = isInverse?  PointsUI[PointsUI.Count -1]:PointsUI[0];
      FakeStartLine.StartPoint = FakeStartPoint.GetComponent<RectTransform>();
      FakeStartLine.EndPoint = start.gameObject.GetComponent<RectTransform>();
      FakeEndLine.StartPoint = end.gameObject.GetComponent<RectTransform>();
      FakeEndLine.EndPoint = FakeEndPoint.GetComponent<RectTransform>();
    }

    void updateLines(){
      for(int i = 0; i< Lines.Count; ++i){
        Destroy(Lines[i].gameObject);
      }
      Lines.Clear();

      for(int i = 0; PointsUI.Count >1 && i< PointsUI.Count-1; ++i){
        GameObject newLine = Instantiate(LinePrefab, LinePrefab.transform.position, LinePrefab.transform.rotation) as GameObject;
        newLine.transform.SetParent(this.transform,false);
        newLine.transform.SetAsFirstSibling();
        uGUIContentScaler lineCtrl = newLine.GetComponent<uGUIContentScaler>();
        lineCtrl.StartPoint = PointsUI[i].GetComponent<RectTransform>();
        lineCtrl.EndPoint = PointsUI[i+1].GetComponent<RectTransform>();
        Lines.Add(lineCtrl);
      }
    }

    public void SetUpSnapPoint(int num){
      this.GetComponent<uGUISnapPoints>().SetUp(num);
    }

    void FixedUpdate(){
      //press and hold to create a new point
      if(!float.IsNaN(timer)){
        if(Mathf.Abs( Input.mousePosition.magnitude - mouseData.position.magnitude) > 5.0f){
          timer = float.NaN;
          return;
        }
        timer += Time.fixedDeltaTime;
        if(timer > HOLD_TIME){
          addPoint();
          timer = float.NaN;
        }
      }

      //save
      if(!float.IsNaN(saveTimer) ){
        saveTimer += Time.fixedDeltaTime;
        if(saveTimer > 1.0f){
          trDataManager.Instance.SaveBehavior();
          saveTimer = float.NaN;
        }
      }
    }

    void createPoint(Vector2 value){
      if(isInverse){
        value.x = 1 - value.x;
      }
      GameObject newPoint = Instantiate(PointPrefab, PointPrefab.transform.position, PointPrefab.transform.rotation) as GameObject;
      newPoint.transform.SetParent(this.transform,false);
      newPoint.transform.SetAsFirstSibling();
      trActuatorPointController ctrl = newPoint.GetComponent<trActuatorPointController>();
      ctrl.ProtoCtrl = ProtoCtrl;
      ctrl.SnapPointCtrl = this.GetComponent<uGUISnapPoints>();
      ctrl.ValidPanel = this.GetComponent<RectTransform>();
      ctrl.NormalizedValue = value;
      ctrl.DraggingLimitPanel = GetComponent<RectTransform>();
      ctrl.SetUpAnchor();
      ctrl.ConfigCtrl = this;
      ctrl.OnValueChanged.AddListener(onPointValueChange);
      setPointView(ctrl);
      PointsUI.Add(ctrl);
    }

    void setPointView(trActuatorPointController point){
      if(MapCtrl.ActuatorType == trActuatorType.RGB_ALL_HUE){
        Color c = trActuator.NormalizedValueToColor(point.NormalizedValue.y, 1.0f);
        point.GetComponent<trButtonBase>().Img.color = c;
      }
    }

    void onPointValueChange(Vector2 p, trActuatorPointController ctrl){
      if(isInverse){
        p.x = 1- p.x;
      }
      int id = PointsUI.IndexOf(ctrl);

      int newId = acPoints.UpdatePoint(id, p);
      if(id != newId && newId>0){
        if(PointsUI.Contains(ctrl)){
          PointsUI.Remove(ctrl);
          PointsUI.Insert(newId, ctrl);
          updateLines();
        }       
      }    
      setPointView(ctrl);
      setFakePoints();
      saveTimer = 0;
    }

    void addPoint(){
   
      RectTransform panel = this.GetComponent<RectTransform>();
      Vector3 globalMousePos;
      if (RectTransformUtility.ScreenPointToWorldPointInRectangle(this.GetComponent<RectTransform>(), mouseData.position, mouseData.pressEventCamera, out globalMousePos))
      {
        Vector2 newP =  panel.GetXYRatioWithWorldPos(globalMousePos);
        if(isInverse){
          newP.x = 1 - newP.x;
        }

        int id = acPoints.InsertPoint(newP);
        if(id != -1){
          trDataManager.Instance.SaveBehavior();
          updateView();

          PointsUI[id].OnBeginDrag(mouseData);
          newPointDrag = PointsUI[id];
        }
      }  
     
    }

    #region IDragHandler implementation

    public void OnDrag (PointerEventData eventData)
    {
      if(newPointDrag != null){
        newPointDrag.OnDrag(eventData);
      }
    }

    #endregion

    #region IPointerDownHandler implementation
    public void OnPointerDown (PointerEventData eventData)
    {
      timer = 0;
      mouseData = eventData;
    }
    #endregion

    #region IPointerUpHandler implementation

    public void OnPointerUp (PointerEventData eventData)
    {
      timer = float.NaN;
      if(newPointDrag != null){
        newPointDrag.OnEndDrag(eventData);
        newPointDrag = null;
      }
    }

    #endregion

  }
 
}
