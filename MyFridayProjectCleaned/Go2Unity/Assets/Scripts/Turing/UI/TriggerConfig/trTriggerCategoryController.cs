using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using WW;
using TMPro;

namespace Turing{
  public class trTriggerCategoryController : MonoBehaviour {

    public TextMeshProUGUI Name ;
    public GridLayoutGroup GridParent;


    public float GetHeight(){
      return Name.GetComponent<RectTransform>().GetHeight() + GridParent.GetHeight();
    }

  }
}
