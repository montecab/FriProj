using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace WW.UGUI{
	public class uGUIGridPanelBase : MonoBehaviour {

		public GameObject GridParent;
    public List<GameObject> Children = new List<GameObject>() ; 

    void Start(){
      GetComponentInChildren<ScrollRect>().normalizedPosition = Vector2.zero;
    }
    
		public void AddChild(GameObject obj){
			obj.transform.SetParent(GridParent.transform, false);
      Children.Add(obj);
		}

    public void Clear(){
      foreach(GameObject child in Children){
        Destroy(child);
      }
      Children.Clear();
    }
	}
}