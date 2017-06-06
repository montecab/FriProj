using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using DG.Tweening;

public class wwCanvasLine : MaskableGraphic {
  [SerializeField] Texture m_Texture;
  
  public override Texture mainTexture
  {
    get
    {
      return m_Texture == null ? s_WhiteTexture : m_Texture;
    }
  }

  public List<Vector3> Points = new List<Vector3>();
  public List<float>   Widths = null;
  public float         Width;

  private Vector2 uv00 = new Vector2(0, 0);
  private Vector2 uv10 = new Vector2(1, 0);
  private Vector2 uv11 = new Vector2(1, 1);
  private Vector2 uv01 = new Vector2(0, 1);
  
  /// <summary>
  /// Texture to be used.
  /// </summary>
  public Texture texture
  {
    get
    {
      return m_Texture;
    }
    set
    {
      if (m_Texture == value)
        return;
      
      m_Texture = value;
      // note: unity example source also called SetVerticesDirty() here, but it seems not needed.
      SetMaterialDirty();
    }
  }

  public void SetPoints(List<Vector3> points){
    SetPointsAndWidths(points, null);
  }
  
  public void SetPointsAndWidths(List<Vector3> points, List<float>widths) {
    Points = points;
    if (widths == null || widths.Count != points.Count) {
      widths = new List<float>(Points.Count);
      for (int n = Points.Count - 1; n >= 0; --n) {
        widths.Add(Width);
      }
    }
    Widths = widths;
    SetVerticesDirty();
  }

  // animate material texture offset
  public void AnimateLine(){
    material.SetFloat("_OffsetX", 0);
    DOTween.To(()=>  material.GetFloat("_OffsetX"), x=>  material.SetFloat("_OffsetX", x), -1.0f, 1);
  }

  protected void _OnFillVBO (List<UIVertex> vbo)
  {
    if (Points.Count < 2){
      return;
    }
    
    // adding this to avoid "converting invalid minmaxAABB" errors on scene exit
    if ((Points[0] - Points[Points.Count - 1]).sqrMagnitude < 0.00001f) {
      return;
    }
   
    oldGenerateMesh(vbo, Points);
  }
  
  public List<Vector3> worldPointsToLocal(List<Vector3>Points) {
    List<Vector3> ret = new List<Vector3>(Points.Count);
    for(int i = 0; i< Points.Count; ++i){
      ret.Add(this.transform.InverseTransformPoint(Points[i]));
    }
    return ret;
  }

  private static Vector3[] _workVecs   = new Vector3[10];
  private static Vector3[] _workNrms   = new Vector3[10];
  private static float  [] _workLens   = new float  [10];
  private static int       _numEntries = 0;
  
  void oldGenerateMesh(List<UIVertex> vh, List<Vector3> points){
    // note: assumes only one thread at a time in here!
    // leisen: why assuming only one thread at a time?
    // oxe:    do you mean it's not a valid assumption ?
    //         making the assumption because if more than one thread is executing this routine simultaneously,
    //         then they'll both be using _workVecs[] etc at the same time.
    
    // verify that this routine is not use re-entrantly.
    _numEntries += 1;
    if (_numEntries > 1) {
      if (wwDoOncePerTypeVal<string>.doIt("not reentrant")) {
        WWLog.logError("multiple simultaneous entries!");
      }
    }
    
    // make sure our scratch arrays are large enough.
    if (_workVecs.Length < points.Count) {
      _workVecs = new Vector3[points.Count];
      _workNrms = new Vector3[points.Count];
      _workLens = new float  [points.Count];
    }
    
    // pre-allocate space in vh.
    vh.Capacity = vh.Count + ((points.Count - 1)* 4);
    
    //calculating segment vectors, lengths, normals
    _workLens[0] = 0;
    float total = 0;
    for(int i = 0; i< points.Count - 1; ++i){
      _workVecs[i] = points[i + 1] - points[i];
      _workNrms[i] = new Vector3(_workVecs[i].y, -_workVecs[i].x, 0.0f);
      float len = _workNrms[i].magnitude;
      _workNrms[i] /= len;
      _workLens[i+1] = _workLens[i] + len;
    }
    total = _workLens[points.Count - 1];
    
    float minDisplace = Width * -10f;
    float maxDisplace = Width *  10f;
    
    UIVertex newVertex = new UIVertex();
    int pcm1 = points.Count - 1;
    for(int i = 0; i < pcm1; ++i) {
      newVertex.normal = Vector3.forward;
      newVertex.color = color;

      //map the whole line to one texture uv
      uv00.x = _workLens[i  ]/total;
      uv10.x = _workLens[i+1]/total;
      uv11.x = _workLens[i+1]/total;
      uv01.x = _workLens[i  ]/total;

      float w = Widths[i] * 0.5f;
      
      if(i == 0){
        newVertex.position = points[i] - _workNrms[i] * w;
        newVertex.uv0 = uv00;
        vh.Add(newVertex);
        
        newVertex.position = points[i] + _workNrms[i] * w;
        newVertex.uv0 = uv01;
        vh.Add(newVertex);
      }
      
      
      if(i == points.Count - 2){
        newVertex.position = points[i + 1] + _workNrms[i] * Widths[i + 1] * 0.5f;
        newVertex.uv0 = uv11;
        vh.Add(newVertex);
        
        newVertex.position = points[i + 1] - _workNrms[i] * Widths[i + 1] * 0.5f;
        newVertex.uv0 = uv10;
        vh.Add(newVertex);
      }
      else{
        Vector3 v1 = _workNrms[i] + _workNrms[i + 1];
        
        float l = 1.0f / Vector3.Dot(v1, _workNrms[i]);
        l = Mathf.Clamp(l, minDisplace, maxDisplace);
        v1 *= l;
        
        
        Vector3 top    = points[i + 1] + v1 * w;
        Vector3 bottom = points[i + 1] - v1 * w;
        
        //Order matters!
        newVertex.position = top;
        newVertex.uv0 = uv11;
        vh.Add(newVertex);
        
        newVertex.position = bottom;
        newVertex.uv0 = uv10;
        vh.Add(newVertex);
        
        // add those same two points again, in reverse order.
        vh.Add(vh[vh.Count - 1]);
        vh.Add(vh[vh.Count - 3]); // -3 instead of -2 because the previous statement grew the array by 1 !
      }
    }
    
    _numEntries -= 1;
  }
  
  [System.Obsolete ("Use OnPopulateMesh(VertexHelper vh) instead, as soon as it gets documented.")]
  protected override void OnPopulateMesh(Mesh m)
  {
    var vbo = new List<UIVertex>();
    _OnFillVBO(vbo);
    
    if (vbo.Count < 4) {
      return;
    }
    
    using (var vh = new VertexHelper()) {
      var quad = new UIVertex[4];
      for (int i = 0; i < vbo.Count; i += 4) {
        vbo.CopyTo(i, quad, 0, 4);
        vh.AddUIVertexQuad(quad);
      }
      vh.FillMesh(m);
    }
  }
  

}
