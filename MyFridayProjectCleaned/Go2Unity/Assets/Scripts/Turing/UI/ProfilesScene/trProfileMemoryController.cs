using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using TMPro;

namespace Turing{
  public class trProfileMemoryController : MonoBehaviour{

    public Image Icon;
    public Button SelectButton;    
    public string Data;
    public TextMeshProUGUI Text;

    public delegate void ProfileItemDelegate(trProfileMemoryController ctrl);
    public ProfileItemDelegate OnPressed;

    void Start(){
      SelectButton.onClick.AddListener(onPressed);
    }

    void OnDestroy(){
      if (SelectButton != null) {
        SelectButton.onClick.RemoveAllListeners();
      }
    }

    void onPressed(){
      if (OnPressed != null){
        OnPressed(this);
      }
    }
  }  
}

