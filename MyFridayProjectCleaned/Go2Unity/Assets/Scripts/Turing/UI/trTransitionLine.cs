using UnityEngine;
using System.Collections.Generic;

namespace Turing{
  public class trTransitionLine : MonoBehaviour{

    public enum bendStyle{
      PARABOLA,
      COSINE,
    }

    private static Texture2D theTexture = null;
  

    public Color colorSource = new Color(0XFF / 255f, 0x00 / 255f, 0x79 / 255f);
    public Color colorTarget = new Color(0XFF / 255f, 0xA5 / 255f, 0x59 / 255f);

    public trStateButtonController PtA;
    public trStateButtonController PtB;
    public float PtARadius = 30f;
    public float PtBRadius = 30f;
    public int NumSegments = 15;
    public float Displacement = 0f;
    public float TriggerDuration = 0.4f;
    public bendStyle BendStyle = bendStyle.PARABOLA;

    private wwCanvasLine canvasLine = null;

    private Vector3 prevPtALocal = Vector3.zero;
    private Vector3 prevPtBLocal = Vector3.zero;
    private Vector3 prevVAB = Vector3.zero;
    private Vector3 prevDispNorm = Vector3.zero;
    private int prevNumSegs = 0;
    private float prevDisp = 0f;

    private float widthOverall = 1f;
    private float timeTrigger = float.NaN;

    private float lastEndpointDragTime = 0;
    private bool preIsTwoTransition = false;
    private bool isTwoArrows = false;

    void Start(){
      canvasLine = GetComponent<wwCanvasLine>();
      DoUpdateTexture();
    }

    void Update(){
      DoUpdate(false);
    }

    public float WidthOverall{
      get{
        return widthOverall;
      }
      set{
        widthOverall = value;
        DoUpdate(true);
      }
    }

    void OnValidate(){
      canvasLine = GetComponent<wwCanvasLine>();
      DoUpdateTexture();
    }

    void DoUpdateTexture(){
      if(canvasLine == null){
        WWLog.logError("no wwCanvasLine component.");
        return;
      }
    
      if(theTexture == null){
        theTexture = new Texture2D(2, 1);
      }
    
      // generate texture. fancy!
      theTexture.SetPixel(0, 0, colorSource);
      theTexture.SetPixel(1, 0, colorTarget);
      theTexture.filterMode = FilterMode.Bilinear;
      theTexture.wrapMode = TextureWrapMode.Clamp;
      theTexture.Apply();
      canvasLine.texture = theTexture;
    
      DoUpdate(true);
    }

    bool isTwoTransition(){
      for(int i=0; i<PtB.StateData.OutgoingTransitions.Count; i++){
        if(PtB.StateData.OutgoingTransitions[i].StateTarget == PtA.StateData){
          return true;
        }
      }
      return false;
    }

    public void DoUpdate(bool force){
      if(canvasLine == null){
        return;
      }
    
      if(PtA == null || PtB == null){
        return;
      }

      float ttn = triggerTimeNormalized;
      bool hasTriggered = (ttn > 0f && ttn < 1f);

      isTwoArrows = isTwoTransition();
      if(!force){
        if((PtA.lastPositionChangeTime < lastEndpointDragTime) && (PtB.lastPositionChangeTime < lastEndpointDragTime) &&
          !hasTriggered && (preIsTwoTransition == isTwoArrows)){
          return;
        }
        lastEndpointDragTime = Time.time;
      }
      preIsTwoTransition = isTwoArrows;

      bool stuffHasChanged = force;

      // for proper culling, we need to set the RectTransform to contain the line.
      // set recttransform to contain PtA and PtB
      float expandOffset = Displacement + 2f;  // this is to expand the RectTransform to contain the curved line.
      RectTransform rt = GetComponent<RectTransform>();
      Vector2 PtALocToPrnt = transform.parent.InverseTransformPoint(PtA.transform.position);
      Vector2 PtBLocToPrnt = transform.parent.InverseTransformPoint(PtB.transform.position);

      // create r, a rectangle in the parent space which contains pt A and pt B.
      Rect r = new Rect(Mathf.Min(PtALocToPrnt.x, PtBLocToPrnt.x), Mathf.Min(PtALocToPrnt.y, PtBLocToPrnt.y),
                 Mathf.Abs(PtBLocToPrnt.x - PtALocToPrnt.x), Mathf.Abs(PtBLocToPrnt.y - PtALocToPrnt.y));

      // expand r by a constant amount
      r.min -= Vector2.one * expandOffset;
      r.max += Vector2.one * expandOffset;

      // set the recttransform, but only if it's different enough.
      if(!piMathUtil.areaWithinSpecifiedEpsilon(r, new Rect(rt.offsetMin, rt.offsetMax - rt.offsetMin), 0.5f)){
        rt.anchorMin = Vector2.one * 0.5f;
        rt.anchorMax = Vector2.one * 0.5f;
        rt.offsetMin = r.min;
        rt.offsetMax = r.max;
      }

    
      // convert other transforms to local.
      Vector3 ptALocal = transform.InverseTransformPoint(PtA.transform.position);
      Vector3 ptBLocal = transform.InverseTransformPoint(PtB.transform.position);
    
      getOffsetPts(ref ptALocal, ref ptBLocal);
    
      stuffHasChanged = stuffHasChanged || !piMathUtil.withinEpsilon((ptALocal - prevPtALocal).sqrMagnitude);
      stuffHasChanged = stuffHasChanged || !piMathUtil.withinEpsilon((ptBLocal - prevPtBLocal).sqrMagnitude);
      stuffHasChanged = stuffHasChanged || (NumSegments != prevNumSegs);
      stuffHasChanged = stuffHasChanged || !piMathUtil.withinEpsilon(Displacement - prevDisp);
      stuffHasChanged = stuffHasChanged || hasTriggered;
    
      if(!stuffHasChanged){
        return;
      }

      prevPtALocal = ptALocal;
      prevPtBLocal = ptBLocal;
      prevNumSegs = NumSegments;
      prevDisp = Displacement;

      if(NumSegments < 1){
        canvasLine.SetPoints(new List<Vector3>());
      }
      else{
        prevVAB = (prevPtBLocal - prevPtALocal);
        prevDispNorm = new Vector3(-prevVAB.y, prevVAB.x, 0f).normalized;
      
      
        int numPts = NumSegments + 1;
      
        List<Vector3> pts = new List<Vector3>(numPts);
        List<float  > wts = new List<float>(numPts);
      
        float t = 0;
        float dt = 1.0f / (float)NumSegments;
        for(int n = 0; n < numPts; ++n){
          pts.Add(LocalPositionForNormalizedDistance(t));
        
        
          // math !
          float w = canvasLine.Width * widthOverall;
          if(ttn > 0 && ttn < 1f){
            float q = 1f - (Mathf.Cos(Mathf.PI * 2f * Mathf.Clamp(ttn * 3f - t * 2f, 0, 1)) * 0.5f + 0.5f);
            float pulseWidth = Mathf.Lerp(0f, 10f, q);
            w += pulseWidth;
          }
          wts.Add(w);
        
        
        
          t += dt;
        }
      
        canvasLine.SetPointsAndWidths(pts, wts);
      }
    }

    void getOffsetPts(ref Vector3 ptA, ref Vector3 ptB){
      Vector3 vAB = ptB - ptA;
      float vABMag = vAB.magnitude;
      if(piMathUtil.withinEpsilon(vABMag)){
        return;
      }
    
      Vector3 vABNorm = vAB / vABMag;
      // adjusting by local scale makes things too poppy when states transition.
      float ptAOffsetFac = PtARadius; // * PtA.localScale.magnitude;
      float ptBOffsetFac = PtBRadius; // * PtB.localScale.magnitude;
    
      float rot = Mathf.Min(Mathf.Abs(Displacement / 2.0f) * 1.1f, 90.0f);
    
      ptA += Quaternion.Euler(0f, 0f, rot) * vABNorm * ptAOffsetFac;
    
      // deal w/ overlapping circles.
      if(ptBOffsetFac > (vABMag - (ptAOffsetFac))){
        ptB = ptA + vABNorm * 0.001f;
      } else{
        ptB -= Quaternion.Euler(0f, 0f, -rot) * vABNorm * ptBOffsetFac;
      }
    }

  
    public Vector3 LocalPositionForNormalizedDistance(float t){
      Vector3 vPt = prevPtALocal + prevVAB * t;
      switch(BendStyle){
      case bendStyle.COSINE:
        vPt += prevDispNorm * (Mathf.Cos(t * Mathf.PI * 2f) * -0.5f + 0.5f) * Displacement;
        break;
      case bendStyle.PARABOLA:
        float f = 1f - (t * 2f);
        f = f * f;
        f = 1f - (f * 0.5f + 0.5f);
        vPt += prevDispNorm * f * Displacement;
        break;
      }
      return vPt;
    }

    public void trigger(){
      timeTrigger = Time.time;
    }

    float triggerTimeNormalized{
      get{
        if(float.IsNaN(timeTrigger)){
          return -1f;
        }
      
        float f = (Time.time - timeTrigger) / TriggerDuration;
        f = Mathf.Clamp(f, 0.0f, 1.0f);
      
        return f;
      }
    }
  
  }
}