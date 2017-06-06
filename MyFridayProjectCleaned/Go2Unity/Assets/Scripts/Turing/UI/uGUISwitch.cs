using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace WW.UGUI{
  [RequireComponent(typeof(Button))]
  public class uGUISwitch : MonoBehaviour {
    private bool isActive = false;
    public bool IsActive{
      set{
        isActive = value;
        updateView();
      }
      get{
        return isActive;
      }
    }

    public GameObject ActiveObj;
    public GameObject InavtiveObj;
  	
    void updateView(){
      ActiveObj.SetActive(isActive);
      InavtiveObj.SetActive(!isActive);
    }



    public Button.ButtonClickedEvent onClick = new Button.ButtonClickedEvent();

    void Start(){
      this.GetComponent<Button>().onClick.AddListener(()=>onButtonClicked());
    }

    void onButtonClicked(){
      IsActive = !IsActive;
      onClick.Invoke();
    }
  }
}
