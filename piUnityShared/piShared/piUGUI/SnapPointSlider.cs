using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[AddComponentMenu("UI/WW/SnapPointSlider", 33)]
[RequireComponent(typeof(RectTransform))]
public class SnapPointSlider : Slider {


  #if UNITY_EDITOR
  [UnityEditor.MenuItem("GameObject/UI/WW/SnapPointSlide")]
  static void CreateCircleSlider(){
    GameObject circleSlider = Instantiate(UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/piShared/piPrefabs/UI/SnapPointSlider.prefab", typeof(GameObject) )) as GameObject;
    if(circleSlider != null){
      circleSlider.transform.SetParent( UnityEditor.Selection.activeTransform);
      circleSlider.transform.localScale = Vector3.one;
      circleSlider.gameObject.name = "SnapPointSlider";
    }
    else{
      Debug.LogError( "Can't find the circle slider object ");    
    }
  }
  #endif

  public int SnapPointNumber = -1;
  protected override void Set (float input, bool sendCallback)
  {
    if(SnapPointNumber >0){
      float snapvalue = (maxValue - minValue)/(SnapPointNumber);
     
      int snapPointId = Mathf.RoundToInt(input/snapvalue);
      input = (float)snapPointId * snapvalue + minValue;
    }
    base.Set (input, sendCallback);
  }

  //no callback to OnValueChange
  public void UpdateValue(float input){
    Set(input, false);
  }
}
