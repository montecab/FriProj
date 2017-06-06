using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using Turing;

[RequireComponent(typeof(Button))]
public class trOptionItem : MonoBehaviour {
  public Graphic                    graphicON;
  public Graphic                    graphicOFF;
  public TextMeshProUGUI            label;
  private trMultivariate.trAppOption appOptionYESNO = trMultivariate.trAppOption.NULL_OPTION;
  
  public trMultivariate.trAppOption AppOptionYESNO {
    get {
      return appOptionYESNO;
    }
    set {
      appOptionYESNO = value;
    }
  }
}
