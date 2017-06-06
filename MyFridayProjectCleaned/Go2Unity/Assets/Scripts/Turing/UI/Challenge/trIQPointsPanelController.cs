using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace Turing{
  public class trIQPointsPanelController : MonoBehaviour, IPointerClickHandler {
    public TextMeshProUGUI Label;

    public delegate void SecretActionDelegate();
    public SecretActionDelegate onCombinationMatch;

    private int clicks = 0;
    private float firstClickTime = float.NaN;
    private const float SECRET_ACTION_ACTIVATION_TIME = 2; //seconds
    private const int CLICKS_TO_ACTIVATE_SECRET_ACTION = 4;


  	// Update is called once per frame
  	void Update () {
      if( trDataManager.Instance.MissionMng.UserOverallProgress != null){
        Label.text = trDataManager.Instance.MissionMng.UserOverallProgress.IQPoints.ToString();
      }
  	  
  	}

    public void OnPointerClick (PointerEventData eventData) {
      if (float.IsNaN(firstClickTime) || Time.fixedTime - firstClickTime > SECRET_ACTION_ACTIVATION_TIME){
        clicks = 1;
        firstClickTime = Time.fixedTime;
      } else {
        clicks++;
      }
      if (clicks == CLICKS_TO_ACTIVATE_SECRET_ACTION){
        clicks = 0;
        firstClickTime = float.NaN;
        if (onCombinationMatch != null){
          onCombinationMatch();
        }
      }
    }
  }
}
