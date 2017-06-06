using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Turing;

#if UNITY_5
using UnityEngine.UI;
#endif



public class wwSpriteLine : MonoBehaviour {

  public bool       IsCheckingLocalPosChange = false; // if true, only update the line when point A or B's local position changes.
  public Transform  PtA;
  public Transform  PtB;
  public float      PtARadius = 30f; // make this public
  public float      PtBRadius = 30f; // make this public
  public Graphic    SampleParticle;
  public float      Spacing = defaultSize * 3;
  public Color      TintA = Color.white;
  public Color      TintB = Color.white;
  public float      WidthA = 1f;
  public float      WidthB = 1f;
  public float      LengthOverall = 1f;
  public float      WidthOverall = 1f;
  public float      TriggerDuration = 1f;

  public float      Displacement = 0;

  public bool UpdateDisabled = false;
  
  private const float defaultSize = 10;
  public List<Graphic> Particles = new List<Graphic>();

  private List<Graphic> particlePool = new List<Graphic>();
  private float timeTrigger = float.NaN;
  private float timePrimed  = float.NaN;
  private bool  primed = false;

  public Graphic ArrowCapReference;
  
  private const float durationPrimeTransition = 0.25f;

  private Vector3 prePtAPos   = Vector3.zero;
  private Vector3 prePtBPos   = Vector3.zero;
  private float   preDisplace = 0f;

  // Update is called once per frame
  void Update () {
    if (UpdateDisabled) {
      return;
    }

    updateParticles();
  }  

  bool checkUpdate(){
    if(PtA == null || PtB == null){
      return false;
    }
    Vector3 posA = PtA.position;
    Vector3 posB = PtB.position;
    if(IsCheckingLocalPosChange){
      posA = PtA.localPosition;
      posB = PtB.localPosition;
    }
    bool result = false;
    result = result || !piMathUtil.withinEpsilon(calculateDistanceSquared(prePtAPos, posA));
    result = result || !piMathUtil.withinEpsilon(calculateDistanceSquared(prePtBPos, posB));
    result = result || !piMathUtil.withinEpsilon(preDisplace, Displacement);

    if(result){
      prePtAPos   = posA;
      prePtBPos   = posB;
      preDisplace = Displacement;
    }
    return result;
  }

  private float calculateDistanceSquared(Vector3 a, Vector3 b){
    return (b-a).sqrMagnitude;
  }
  
  void OnEnable() {
    updateParticles();
  }
  
  void getOffsetPts(out Vector3 ptA, out Vector3 ptB) {  
    Vector3 vAB = PtB.position - PtA.position;
    float vABMag = vAB.magnitude;
    if (piMathUtil.withinEpsilon(vABMag)) {
      ptA = PtA.position;
      ptB = ptA + Vector3.one * 0.0001f;
      return;
    }
    
    Vector3 vABNorm = vAB / vABMag;
    // adjusting by local scale makes things too poppy when states transition.
    float ptAOffsetFac = PtARadius; // * PtA.localScale.magnitude;
    float ptBOffsetFac = PtBRadius; // * PtB.localScale.magnitude;
    
    float rot = Mathf.Min(Mathf.Abs(Displacement) * 1.1f, 90.0f);
    
    ptA = PtA.position + (Quaternion.Euler(0f, 0f, rot) * vABNorm * ptAOffsetFac);
    
    // deal w/ overlapping circles.
    if (ptBOffsetFac > (vABMag - (ptAOffsetFac))) {
      ptB = ptA + vABNorm * 0.001f;
    }
    else {
      ptB = PtB.position - (Quaternion.Euler(0f, 0f, -rot) * vABNorm * ptBOffsetFac);
    }
  }
  
  Vector3 getVABWorld() {
    Vector3 ptA;
    Vector3 ptB;
    getOffsetPts(out ptA, out ptB);
    return ptB - ptA;
  }
  
  void updateParticles() {
    bool updateTransforms = false;
    
    if(checkUpdate()){
      updateParticleCount();
      updateParticleColors();
      updateTransforms = true;
    }
    
    float f = triggerTimeNormalized;
    updateTransforms = updateTransforms || (f > 0f && f < 1f);
    
    if (updateTransforms) {
      updateParticleTransforms();
    }
  }
  
  void updateParticleColors() {
    if (Particles.Count == 1) {
      Particles[0].color = TintB;
      return;
    }
    
    for (int n = 0; n < Particles.Count; ++n) {
      float t = (float)n / (float)(Particles.Count - 1);
      Particles[n].color = Color.Lerp(TintA, TintB, t);
    }
  }
  
  void updateParticleTransforms() {
    if (Particles.Count == 0) {
      return;
    }
    
    Vector3 ptA;
    Vector3 ptB;
    getOffsetPts(out ptA, out ptB);

    if (Particles.Count == 1) {
      Particles[0].transform.position = ptA;
      updateParticleAlignment(Particles[0].transform,ptA, ptB);
      Vector3 baseScale = ArrowCapReference == null? Vector3.one : ArrowCapReference.transform.localScale;
      updateParticleWidth    (Particles[0].transform, 0, baseScale);
      return;
    }
    
    Vector3 vAB = getVABWorld();
    Vector3 vABNorm = vAB.normalized;
    
    // calculate full displacement
    float dMax = Particles.Count > 3 ? Displacement : 0f;
    Vector3 fullDisplace = new Vector3(-vABNorm.y, vABNorm.x, 0) * dMax;    
    
    Vector3 baseScaleGeneral =  SampleParticle.transform.localScale;
    Vector3 baseScaleLast    =  (ArrowCapReference == null ? baseScaleGeneral : ArrowCapReference.transform.localScale);
    
    // positions  
    Vector3 vABGap = vABNorm * (vAB.magnitude / (Particles.Count - 1));
    for (int n = 0; n < Particles.Count; ++n) {
      float f = (float)n / (float)(Particles.Count - 1);
      
      float displaceAmount = 1f - ((4f * f * f) - (4f * f) + 1f);
      
      Transform t = Particles[n].transform;
      
      Vector3 baseScale = n < (Particles.Count - 1) ? baseScaleGeneral : baseScaleLast;
      
      updateParticleWidth(t, f, baseScale);
      t.position = ptA + (vABGap * n) + fullDisplace * displaceAmount;
    }
        
    // alignments
    // todo: often these don't need updating. eg when we're just animating the transition.
    int frst = 0;
    int last = Particles.Count - 1;
    
    for (int n = 1; n < Particles.Count - 1; ++n) {
      updateParticleAlignment(Particles[n].transform, Particles[n - 1].transform.position, Particles[n + 1].transform.position);
    }
    updateParticleAlignment(Particles[frst].transform, ptA                                   , Particles[frst + 1].transform.position);
    updateParticleAlignment(Particles[last].transform, Particles[last - 1].transform.position, Particles[last    ].transform.position);
  }
  
  static Vector3 _workerV1 = Vector3.zero;
  void updateParticleAlignment(Transform tPart, Vector3 ptFrom, Vector3 ptTo) {
    Vector3 vFromToNorm = (ptTo - ptFrom).normalized;
    float theta = Mathf.Atan2(vFromToNorm.y, vFromToNorm.x);
    _workerV1.z = theta * Mathf.Rad2Deg;
    tPart.eulerAngles = _workerV1;
  }
  
  float triggerTimeNormalized {
    get {
      if (float.IsNaN(timeTrigger)) {
        return -1f;
      }
      
      float f = (Time.time - timeTrigger) / TriggerDuration;
      f = Mathf.Clamp(f, 0.0f, 1.0f);
      
      return f;
    }
  }
  
  void updateParticleWidth(Transform tPart, float t, Vector3 baseScale) {
    float triggerScale = 1f;
    
    if (!float.IsNaN(timeTrigger) && (TriggerDuration > piMathUtil.epsilon)) {
      float f = triggerTimeNormalized;
      
      // math !      
      float q = 1f - (Mathf.Cos(Mathf.PI * 2f * Mathf.Clamp(f * 3f - t * 2f, 0, 1)) * 0.5f + 0.5f);
      
      triggerScale = Mathf.Lerp(1f, 3f, q);
    }
    
    // more math!
    float primedScale = 0.0f;
    if (!float.IsNaN(timePrimed)) {
      float dt = Time.time - timePrimed;
      float primedAmt;
      if (primed) {
        primedAmt = Mathf.InverseLerp(0, durationPrimeTransition, dt);
      }
      else {
        primedAmt = Mathf.InverseLerp(durationPrimeTransition, 0, dt);
      }
      primedAmt = Mathf.Clamp(primedAmt, 0, 1);
      primedScale = Mathf.Sin(Time.time * 4 - t * 9) * 0.5f * primedAmt;
    }
    
    Vector3 scl = baseScale;
    scl.x *= LengthOverall;
    scl.y *= Mathf.Lerp(WidthA, WidthB, t) * WidthOverall;
    scl *= triggerScale + primedScale;
    tPart.localScale = scl;
  }
  
  void updateParticleCount() {
    // update number of sprites
    int desiredParticles = NumSprites;
    int currentParticles = Particles.Count;
    int dParticles = desiredParticles - currentParticles;
    if (dParticles > 0) {
      // need to add more particles!
      //Debug.Log("adding particles - current: " + currentParticles + ", desired: " + desiredParticles);
      for (int n = 0; n < dParticles; ++n) {
        Graphic p = getFromObjectPool();
        Particles.Add(p);
        p.gameObject.SetActive(true);
        p.transform.SetParent(transform);
      }
    }
    else if (dParticles < 0){
      // need to remove particles
      //Debug.Log("removing particles - current: " + currentParticles + ", desired: " + desiredParticles);
      Graphic[] itemsToRemove = new Graphic[-dParticles];
      for (int n = desiredParticles; n < currentParticles; ++n){
        itemsToRemove[n - desiredParticles] = Particles[n];
      }
      Particles.RemoveRange(desiredParticles, -dParticles);
      foreach (Graphic g in itemsToRemove) {
        g.gameObject.SetActive(false);
      }
    }
    
    if (dParticles != 0) {
      for (int n = Particles.Count - 2; n >= 0; --n) {
        Particles[n].GetComponent<RawImage>().texture = SampleParticle.GetComponent<RawImage>().texture;
      }

      if (Particles.Count > 0 && ArrowCapReference != null) {
        Particles[Particles.Count - 1].GetComponent<RawImage>().texture = ArrowCapReference.GetComponent<RawImage>().texture;
      }
    }
  }

  Graphic getFromObjectPool(){
    Graphic p;
    if(particlePool.Count < Particles.Count){
      WWLog.logError("object pool's count should never be less than particle count");
      return null;
    }
    if(particlePool.Count == Particles.Count){
      p = GameObject.Instantiate<Graphic>(SampleParticle);
      particlePool.Add(p);
    }
    return particlePool[Particles.Count];
  }
  
  public void Reset() {
    for (int c = 0; c < gameObject.transform.childCount; ++c) {
      GameObject child = gameObject.transform.GetChild(c).gameObject;
      if (child != SampleParticle.gameObject) {
        GameObject.Destroy(child);
      }
    }
    
    Particles.Clear();
  }
  
  bool Valid {
    get {
      if (piMathUtil.withinEpsilon(Spacing)) {
        return false;
      }
      
      if ((PtA == null) || (PtB == null)) {
        return false;
      }
      
      return true;
    }
  }
  
  float ArcLength {
    get {
      return piMathUtil.parabolaLengthApproximate(getVABWorld().magnitude, Displacement);
    }
  }
  
  int NumSprites {
    get {
      if (!Valid) {
        return 0;
      }
      
      return (int)(ArcLength / Spacing) + 1;
    }
  }

  public void SetUpdateEnabled(bool value){
    UpdateDisabled = !value;
  }
  
  public void trigger() {
    timeTrigger = Time.time;
  }
  
  public bool Primed {
    get {
      return primed;
    }
    set {
      if (value != primed) {
        timePrimed = Time.time;
        primed = value;
      }
    }
  }
}
