using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

[RequireComponent (typeof(LineRenderer))]
public class wwCurveLine : MonoBehaviour {
  
  public Transform StartPoint;
  public Transform EndPoint;
  public Sprite StartCap = null;
  public Sprite EndCap = null;
  public Vector2 CapSize = Vector2.zero;

  // require setting the cap pivot to (0.5, 1)
  public GameObject CapObjectStart = null;
  public GameObject CapObjectEnd = null;
  
  public float Displacement;
  [Range(2, 30)]
  public int ApproximationPoints = 20;
  private LineRenderer lineRenderer = null;
  private CurveLineProperties recentCurveProperties = new CurveLineProperties();
  private float capOffsetStart;
  private float capOffsetEnd;
  
  
  void Awake(){
    lineRenderer = gameObject.GetComponent<LineRenderer>();
  }
  
  void Start () {
    if (lineRenderer.material == null){
      lineRenderer.material = new Material(Shader.Find("Particles/Additive"));
    }

    if(CapObjectStart == null ){
      capOffsetStart = CapSize.x / 2;
    }else{
      capOffsetStart = 0;
    }

    if(CapObjectEnd == null ){
      capOffsetEnd = CapSize.x / 2;
    }else{
      capOffsetEnd = 0;
    }

    generateCapsObjects();
    redrawCurveLineAndUpdateState();
  }
  
  void Update () {
    if(StartPoint == null || EndPoint == null){
      return;
    }
    if (recentCurveProperties.IsStateChanged(StartPoint.position, EndPoint.position, StartPoint.localScale, EndPoint.localScale, ApproximationPoints, Displacement)){
      redrawCurveLineAndUpdateState();
    }
  }
  
  public void ForceUpdate(){
    redrawCurveLineAndUpdateState();
  }
  
  private void generateCapsObjects(){
    if (StartCap != null && CapObjectStart == null){
      CapObjectStart = createCapGameObject(StartCap);
    }
    
    if (EndCap != null && CapObjectEnd == null){
      CapObjectEnd = createCapGameObject(EndCap);
    }
  }
  
  private GameObject createCapGameObject(Sprite sprite){
    GameObject result = new GameObject("Cap");
    Image image = result.AddComponent<Image>();
    RectTransform trans = result.GetComponent<RectTransform>();
    image.sprite = sprite;
    result.transform.SetParent(transform);
    trans.SetSize(CapSize);
    trans.SetDefaultScale();
    trans.anchorMin = Vector2.up;
    trans.anchorMax = Vector2.up;
    return result;
  }
  
  
  private void redrawCurveLineAndUpdateState(){
    this.transform.position = StartPoint.transform.position + new Vector3(0,0,5);

    if (ApproximationPoints != recentCurveProperties.ApproximationPoints){
      this.lineRenderer.SetVertexCount(ApproximationPoints);
    }
    
    List<Vector3> positions = new List<Vector3>(ApproximationPoints);

    Vector3[] worldCorners = new Vector3[4];
    StartPoint.GetComponent<RectTransform>().GetWorldCorners(worldCorners);
    float startPointWidth = (worldCorners[0] - worldCorners[1]).magnitude ;     
        
    EndPoint.GetComponent<RectTransform>().GetWorldCorners(worldCorners);
    float endPointWidth = (worldCorners[0] - worldCorners[1]).magnitude ;
    
    Vector3 offsetStart = (EndPoint.position - StartPoint.position).normalized * startPointWidth /  2f;
    Vector3 offsetEnd   = (EndPoint.position - StartPoint.position).normalized * endPointWidth   / -2f;
    
    float rot = Mathf.Min(Mathf.Abs(Displacement) * 1.1f, 90.0f);
    
    offsetStart = Quaternion.Euler(0, 0,  rot) * offsetStart;
    offsetEnd   = Quaternion.Euler(0, 0, -rot) * offsetEnd;
    
    Vector3 start = StartPoint.position + offsetStart;
    Vector3 end   = EndPoint  .position + offsetEnd  ;
    
    Vector3 distance = end - start;
    Vector3 normalizedDistance = distance.normalized;
    if(!lineRenderer.useWorldSpace){
      //make it not changing shape while scale changes
      float scale  = this.transform.InverseTransformVector(normalizedDistance).magnitude;
      normalizedDistance /= scale;
    }
    Vector3 displacePerpedicular = new Vector3(-normalizedDistance.y, normalizedDistance.x, 0);
   
    for (int num = 0; num < ApproximationPoints; num++){
      float part = (float)num / (float)(ApproximationPoints - 1);
      Vector3 partVector = part * distance;
      float value =  part * 2 - 1;
      partVector += displacePerpedicular * Displacement * (1 - value * value);
      Vector3 position = Vector3.zero ;
      if(lineRenderer.useWorldSpace){
        position = start + partVector;
      }
      else{
        position =  this.transform.InverseTransformPoint(start + partVector);
      }
      lineRenderer.SetPosition(num, position);
      positions.Add(position);
    }
    recentCurveProperties.UpdateState(StartPoint.position,EndPoint.position ,start, end, StartPoint.localScale, EndPoint.localScale, ApproximationPoints, Displacement);
    recentCurveProperties.LinePositions = positions;
    correctPositionAndRotationForCaps();


  }
  
  private void correctPositionAndRotationForCaps(){
    
    List<Vector3> points = recentCurveProperties.LinePositions;
    if (points.Count < 2) {
      return; 
    }  
    
    if (CapObjectStart != null){
      Vector3 dir;
      if(lineRenderer.useWorldSpace){
        dir = points[1]- points[0];
      }
      else{
        dir = lineRenderer.transform.TransformDirection(points[1]- points[0]);
      }
      applyPositionAndRotation(CapObjectStart.transform, recentCurveProperties.RealStartPos, recentCurveProperties.RealStartPos + dir, capOffsetStart);
    }
    
    if (CapObjectEnd != null){
      Vector3 dir;
      if(lineRenderer.useWorldSpace){
        dir = points[points.Count - 2]- points[points.Count - 1];
      }
      else{
        dir = lineRenderer.transform.TransformDirection(points[points.Count - 2]- points[points.Count - 1]);
      }
      
      applyPositionAndRotation(CapObjectEnd.transform, recentCurveProperties.RealEndPos, recentCurveProperties.RealEndPos + dir, capOffsetEnd);
    }
  }
  
  private void applyPositionAndRotation(Transform target, Vector3 start, Vector3 end, float radius){
    Vector3 diff = end - start;
    float angle = Mathf.Atan2(diff.x, diff.y) * Mathf.Rad2Deg + 90;
    target.transform.rotation = Quaternion.AngleAxis(-angle, Vector3.forward);
    
    float angleRad = (angle - 90) * Mathf.Deg2Rad;
    
    Vector3 positionOffset = Vector3.zero;
    positionOffset.x = radius * Mathf.Sin(angleRad);
    positionOffset.y = radius * Mathf.Cos(angleRad);
    
    target.position = start + positionOffset + new Vector3(0,0,-1);
  }
  
  
  private struct CurveLineProperties {
    public Vector3 StartPoint;
    public Vector3 EndPoint;
    public Vector3 RealStartPos;
    public Vector3 RealEndPos;
    public Vector3 StartScale;
    public Vector3 EndScale;
    public int ApproximationPoints;
    public float Displacement;
    public List<Vector3> LinePositions;
    
    public CurveLineProperties (Vector3 startPoint, Vector3 endPoint, Vector3 realStart, Vector3 realEnd, Vector3 startScale, Vector3 endScale, int approximationPoints, float displacement){
      this.StartPoint = startPoint;
      this.EndPoint = endPoint;
      this.RealStartPos = realStart ;
      this.RealEndPos = realEnd;
      this.StartScale = startScale;
      this.EndScale = endScale;
      this.ApproximationPoints = approximationPoints;
      this.Displacement = displacement;
      LinePositions = new List<Vector3>();
    }
    
    public bool IsStateChanged(Vector3 startPoint, Vector3 endPoint, Vector3 startScale, Vector3 endScale, int approximationPoints, float displacement){
      bool result = false;
      result = result || !piMathUtil.withinEpsilon(calculateDistanceSquared(startScale, this.StartScale));
      result = result || !piMathUtil.withinEpsilon(calculateDistanceSquared(endScale, this.EndScale));
      result = result || !piMathUtil.withinEpsilon(calculateDistanceSquared(startPoint, this.StartPoint));
      result = result || !piMathUtil.withinEpsilon(calculateDistanceSquared(endPoint, this.EndPoint));
      result = result || !piMathUtil.withinEpsilon(displacement - this.Displacement);
      result = result || !piMathUtil.withinEpsilon(approximationPoints - this.ApproximationPoints);
      return result;
    }
    
    private float calculateDistanceSquared(Vector3 a, Vector3 b){
      return (b-a).sqrMagnitude;
    }
    
    public void UpdateState(Vector3 startPoint, Vector3 endPoint, Vector3 realStart, Vector3 realEnd, Vector3 startScale, Vector3 endScale, int approximationPoints, float displacement){
      this.StartPoint = startPoint;
      this.EndPoint = endPoint;
      this.RealEndPos = realEnd;
      this.RealStartPos = realStart;
      this.ApproximationPoints = approximationPoints;
      this.Displacement = displacement;
      this.StartScale = startScale;
      this.EndScale = endScale;
    }
  }
  
  
  #region Unity Editor Utility
  #if UNITY_EDITOR
  [UnityEditor.MenuItem("GameObject/UI/WW/CurveLineObject")]  
  static void CreateDefaultCurveLineController() {
    GameObject curveObject = new GameObject("LineCurve");
    
    if (UnityEditor.Selection.activeTransform != null){
      curveObject.AddComponent<LineRenderer>();
      curveObject.AddComponent<wwCurveLine>();
      curveObject.transform.SetParent(UnityEditor.Selection.activeTransform);
      curveObject.transform.Translate(UnityEditor.Selection.activeTransform.position);
    }
  }
  #endif
  #endregion
}
