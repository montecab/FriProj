using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using WW.SimpleJSON;

public class prettyPrintController : MonoBehaviour {

  public InputField      ui_Input;
  public TextMeshProUGUI ui_Output;
  public Button          ui_DoIt1;
  public Button          ui_DoIt2;
  

	// Use this for initialization
	void Start () {
  
    ui_DoIt1.onClick.AddListener(onClick_DoIt1);
    ui_DoIt2.onClick.AddListener(onClick_DoIt2);
	
	}

  void doIt1() {
    string src = ui_Input.text;
    
    JSONNode jsn = JSON.Parse(src);
    if (jsn == null) {
      ui_Output.text = "error parsing input";
      return;
    }
    
    ui_Output.text = jsn.ToString();
  }
  
  void doIt2() {
    string src = ui_Input.text;
    
    JSONNode jsn = JSON.Parse(src);
    if (jsn == null) {
      ui_Output.text = "error parsing input";
      return;
    }
    
    ui_Output.text = jsn.ToString("");
  }
  
  
  void onClick_DoIt1() {
    doIt1();
  }
  void onClick_DoIt2() {
    doIt2();
  }
}
