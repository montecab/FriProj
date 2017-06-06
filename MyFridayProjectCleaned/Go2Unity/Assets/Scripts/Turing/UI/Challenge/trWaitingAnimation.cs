using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace Turing{
  public class trWaitingAnimation : MonoBehaviour {

    public Text label;
  	
    public void SetActive(bool active){
      label.gameObject.SetActive(active);
      if(active){
        StartCoroutine(waitingAnimation());
      }else{
        StopCoroutine(waitingAnimation());
      }

    }

    IEnumerator waitingAnimation(){
      while(true){
        if(label.text == "..."){
          label.text = "";
        }
        label.text += ".";
        yield return new WaitForSeconds(0.5f);
      }

     
    }
  }
}
