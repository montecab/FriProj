using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SliderPanelController : MonoBehaviour {
  public SnapPointSlider ValueSlider;

  public Button PlusButton;
  public Button MinusButton;

  public int SliderSnapPointNumber;
  public int PlusMinusSnapPointNumber;

	// Use this for initialization
	void Start () {
    if(PlusButton != null){
      PlusButton.onClick.AddListener(onPlusButtonClicked);
    }

    if(MinusButton != null){
      MinusButton.onClick.AddListener(onMinusButtonClicked);
    }
	}

  void onPlusButtonClicked(){
    ValueSlider.SnapPointNumber = PlusMinusSnapPointNumber;
    ValueSlider.value += 1;
    ValueSlider.SnapPointNumber = SliderSnapPointNumber;
  }

  void onMinusButtonClicked(){
    ValueSlider.SnapPointNumber = PlusMinusSnapPointNumber;
    ValueSlider.value -= 1;
    ValueSlider.SnapPointNumber = SliderSnapPointNumber;
  }
	
}
