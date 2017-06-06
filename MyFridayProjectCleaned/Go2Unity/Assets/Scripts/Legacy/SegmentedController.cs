using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class SegmentedController : MonoBehaviour {

  public List<Segment> Segments = new List<Segment>();

  protected Segment activatedSegment;

  public void Refresh(){
    if (Segments.Count == 0)
      return;
    for (int i = 0; i< Segments.Count; i++) {
      Segments[i].Deactivate();
    }
  }

  public void ActivateSegment(Segment seg){
    if (activatedSegment != null) {
        activatedSegment.Deactivate();
    }
    activatedSegment = seg;
    if (activatedSegment != null) {
        activatedSegment.Activate();
    }

  }
}
