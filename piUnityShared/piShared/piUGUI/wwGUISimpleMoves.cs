using UnityEngine;
using System.Collections;

public class wwGUISimpleMoves : MonoBehaviour {

  public float RevolutionTime       = 1;
  public int   RevolutionIncrements = 12;
  public bool  RevolutionClockwise  = true;
  public float BobAmplitude         = 0;
  public float BobPeriod            = 1;
  public float BobPhaseVariance     = 1;
  public float BobPow               = 1;
  
  private float nextTickTime = 0;
  private int   increment    = 0;
	private Vector3 initialLocalPosition;
  private float bobPhase;
  
  // Use this for initialization
  void Start () {
    tick();
    initialLocalPosition = transform.localPosition;
    bobPhase = Random.Range(0f, BobPhaseVariance);
  }
	
  // Update is called once per frame
  void Update () {
    if (Time.time >= nextTickTime) {
      tick();
    }
    
    if (BobAmplitude > 0 && BobPeriod > 0) {
      Vector3 v = initialLocalPosition;
      v.y += (Mathf.Pow(Mathf.Sin((Time.time / BobPeriod + bobPhase) * Mathf.PI * 2f) * 0.5f + 0.5f, BobPow) * 2.0f - 1.0f) * BobAmplitude;
      transform.localPosition = v;
    }
  }
	
  float tickPeriod {
    get {
      if (RevolutionIncrements <= 0) {
        return RevolutionTime;
      }
      return RevolutionTime / RevolutionIncrements;
    }
  }
  
  void tick() {
    if (RevolutionIncrements != 0) {
      Vector3 le = transform.localEulerAngles;
      le.z = (360.0f / (float)RevolutionIncrements) * increment * (RevolutionClockwise ? -1.0f : 1.0f);
      transform.localEulerAngles = le;
      increment = (increment + 1) % RevolutionIncrements;
    }
    nextTickTime = Time.time + tickPeriod;
  }
}
