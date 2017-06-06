using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using WW.UGUI;

namespace Turing{
  public class trDoubleSliderController : MonoBehaviour {
    public Slider SliderLow;
    public Slider SliderHigh;

    public float Min;
    public float Max;

    public float LowValue{
      set{
        SliderLow.value = value;
      }
      get{
        return SliderLow.value;
      }     
    }

    public float HighValue{
      set{
        SliderHigh.value = value;
      }
      get{
        return SliderHigh.value;
      }
    }

    public bool IsPushOtherHandle = false;

    public uGUISnapPoints SnapCtrl;
    //public GameObject MidSnapPoint;

    private bool isSetToOneHandle = false;
    public bool IsSetToOneHandle{
      set{
        if(isSetToOneHandle == value){
          return;
        }
        isSetToOneHandle = value;
      }
      get{
        return isSetToOneHandle;
      }
    }

    void Start(){
      SliderLow.onValueChanged.AddListener(onSliderLowValueChanged);
      SliderHigh.onValueChanged.AddListener(onSliderHighValueChanged);
    }

    public void Reset(){
      if(IsSetToOneHandle){
        float mid = (Max + Min)/2;
        SliderLow.value = mid;
        SliderHigh.value = mid;
      }
      else{
        SliderLow.value = Min;
        SliderHigh.value = Max;
      }
    }

    public void SetUp(float min, float max, float midValue, int snappoint = 0){
      Min = min;
      Max = max;

      SliderLow.minValue = min;
      SliderLow.maxValue = max;

      SliderHigh.minValue = min;
      SliderHigh.maxValue = max;

      SnapCtrl.SetUp(snappoint);

//      if(!float.IsNaN(midValue)){
//        float height = this.GetComponent<RectTransform>().rect.height;
//        float ratio = (midValue - (max - min)/2)/(max - min);
//        MidSnapPoint.transform.localPosition = new Vector3(MidSnapPoint.transform.localPosition.x, 
//                                                           ratio * height,
//                                                           MidSnapPoint.transform.localPosition.z);
//
//      }else{
//        MidSnapPoint.SetActive(false);
//      }
    }


    void onSliderLowValueChanged(float value){
      SliderLow.value = SnapCtrl.SnappedValue(Min,Max, value);
      if(IsSetToOneHandle){
        SliderHigh.value = value;
      }
      else if(IsPushOtherHandle){
        if(SliderHigh.value < value){
          SliderHigh.value = value;
        }
      }
    }

    void onSliderHighValueChanged(float value){
      SliderHigh.value = SnapCtrl.SnappedValue(Min,Max, value);
      if(IsSetToOneHandle){
        SliderLow.value = value;
      }
      else if(IsPushOtherHandle){
        if(SliderLow.value > value){
          SliderLow.value = value;
        }
      }
    }
  }
}
