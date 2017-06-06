using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class trEscClosePanel : MonoBehaviour {

	public List<GameObject> MatterObjs = new List<GameObject>(); //if one of obj here is active, dont close this panel
	
	// Update is called once per frame
	void Update () {
    for(int i = 0; i< MatterObjs.Count; ++i){
      if(MatterObjs[i].activeSelf){
        return;
      }
    }

    if(Input.GetKeyDown(KeyCode.Escape)){
      this.gameObject.SetActive(false);
    }
	}
}
