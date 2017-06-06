using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace Turing{
  public class trIngredientInfoController : MonoBehaviour {

  	public Image Img;
    public RectTransform Outline;
    public Text Label;
    public GameObject RightImg;
    public GameObject WrongImage;
    public Image ErrorImage;

    public GameObject StateButtonPrefab;

    private GameObject stateButton;


    public void SetView(trBehavior behavior, int count){
      SetCount(count);
      Img.gameObject.SetActive(false);
      if(stateButton == null){
        stateButton = Instantiate(StateButtonPrefab) as GameObject;
        stateButton.transform.SetParent(Outline.transform, false);
        stateButton.transform.localPosition = Vector3.zero;
        stateButton.GetComponent<RectTransform>().anchorMin = new Vector2(0,0);
        stateButton.GetComponent<RectTransform>().anchorMax = new Vector2(1,1);
        stateButton.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
        AspectRatioFitter ratioFitter = stateButton.AddComponent<AspectRatioFitter>();
        ratioFitter.aspectMode = AspectRatioFitter.AspectMode.WidthControlsHeight;
        ratioFitter.aspectRatio = 1;
      }

      trState tmpState = new trState();
      if(behavior.Type == trBehaviorType.MOODY_ANIMATION){
        tmpState.BehaviorParameterValue = behavior.Animation.id;
      }
      tmpState.Behavior = behavior;
      trStateButtonController stateButtonController = stateButton.GetComponent<trStateButtonController>();

      stateButtonController.StateData = tmpState;
      stateButtonController.SetUpView();
      stateButtonController.TransitionContainer.gameObject.SetActive(false);

      stateButtonController.IndexContainer.SetActive(false);
      if (tmpState.Behavior.Type == trBehaviorType.DO_NOTHING){
        stateButton.GetComponent<RectTransform>().SetDefaultScale();
      }
    }

    public void SetView(trTriggerType triggerType, int count){
      SetCount(count);
      Outline.SetWidth(35);
      Img.gameObject.SetActive(true);
      Img.sprite = trIconFactory.GetIcon(triggerType);
    }

    public void SetCount(int count){
      if(count == 0){
        RightImg.SetActive(true);
        WrongImage.SetActive(false);
        Label.text = "";
      }else{
        RightImg.SetActive(false);
        if(count < 0){
          Label.text = "";
          WrongImage.SetActive(true);
        }
        else{
          Label.text = "+" + count.ToString();
          WrongImage.SetActive(false);
        }
      }
    }
  }
}
