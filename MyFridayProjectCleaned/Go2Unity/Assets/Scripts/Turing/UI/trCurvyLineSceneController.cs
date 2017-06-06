using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Turing{
  public class trCurvyLineSceneController : MonoBehaviour{
    public GameObject SampleTarget;
    public wwCurveLine CurvyLine;
    public wwSpriteLine SpriteLine;
    public trTransitionLine TransitionLine;
    public Toggle toggleCurvyLine;
    public Toggle toggleCanvasLine;
    public Toggle togglePrimed;
    public float randomScatter = 100f;
  
    private List<GameObject> randoGOs = new List<GameObject>();
    private List<GameObject> randoSLs = new List<GameObject>();

    public void onSliderDisplacement(float value){
      CurvyLine.Displacement = value;
      SpriteLine.Displacement = value;
      TransitionLine.Displacement = value;
    }

    public void onSliderNumRandoms(float value){
      NumRandos = (int)value;
    }

    public void onClickTrigger(){
      SpriteLine.trigger();
      SpriteLine.Primed = false;
      togglePrimed.isOn = false;
    
      TransitionLine.trigger();
    
      foreach(GameObject go in randoSLs){
        go.GetComponent<wwSpriteLine>().Primed = false;
      }
    }

    public void onClickOtherPrime(){
      SpriteLine.Primed = true;
      foreach(GameObject go in randoSLs){
        go.GetComponent<wwSpriteLine>().Primed = true;
      }
    }

    private int NumRandos{
      set{
        if(value == randoGOs.Count){
          return;
        }
      
        foreach(GameObject go in randoSLs){
          GameObject.Destroy(go);
        }
        foreach(GameObject go in randoGOs){
          GameObject.Destroy(go);
        }
        randoSLs.Clear();
        randoGOs.Clear();
      
        for(int n = 0; n < value; ++n){
          GameObject target = GameObject.Instantiate<GameObject>(SampleTarget);
          target.transform.SetParent(SampleTarget.transform.parent);
          target.transform.position = new Vector2(Random.Range(-randomScatter, randomScatter), Random.Range(-randomScatter, randomScatter));
          target.transform.localScale = SampleTarget.transform.localScale * 0.5f;
          randoGOs.Add(target);
        
          GameObject sl = GameObject.Instantiate<GameObject>(SpriteLine.gameObject);
          sl.transform.SetParent(SpriteLine.transform.parent);
          sl.transform.localScale = SpriteLine.transform.localScale;
          wwSpriteLine wsl = sl.GetComponent<wwSpriteLine>();
          wsl.Displacement = SpriteLine.Displacement;
          wsl.Spacing = SpriteLine.Spacing;
          wsl.PtB = target.transform;
          wsl.Reset();
          randoSLs.Add(sl);
        }
      }
    }

    public void Update(){
      CurvyLine.gameObject.SetActive(!toggleCurvyLine.isOn);
      SpriteLine.gameObject.SetActive(toggleCurvyLine.isOn);
      TransitionLine.gameObject.SetActive(toggleCanvasLine.isOn);
    
      float valTarg = togglePrimed.isOn ? 2.0f : 1.0f;
      float slur = 0.9f;
    
      SpriteLine.WidthOverall = SpriteLine.WidthOverall * slur + valTarg * (1f - slur);
    }
  }
}
