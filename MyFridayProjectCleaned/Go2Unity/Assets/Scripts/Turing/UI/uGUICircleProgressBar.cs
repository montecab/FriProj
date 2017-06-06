using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class uGUICircleProgressBar : MonoBehaviour {

  public float MIN = 0;
  public float MAX = 1;

  public Image FillImage;

  private float mValue = 0;
  public float Value{
    set{
      mValue = Mathf.Clamp(value, MIN, MAX);
      setUpView();
    }
    get{
      return mValue;
    }
  }

  public float NormalizedValue{
    get{
      return mValue/(MAX - MIN);
    }
  }

  void setUpView(){
    FillImage.fillAmount = NormalizedValue;
  }

}
