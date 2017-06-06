using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

namespace Turing{
  public class trRewardDurableIconController : MonoBehaviour {
    public TextMeshProUGUI Title;
    public Image DurableIcon;

    private trRewardDurable durable;
    public trRewardDurable Durable {
      get {
        return durable;
      }
      set {
        if ((value != null) && !value.Equals(durable)){
          durable = value;
          updateUIWithDurable(); 
        }
      }
    }

    void Start() {
      
    }

    void updateUIWithDurable() {
      Title.text = durable.DisplayTextLocalized();
      DurableIcon.sprite = durable.DisplayImage();
    }
  }
}