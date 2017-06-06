using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace Turing{
  /// <summary>
  /// Disable scroll rect when there is multiple finger touches. This is because 
  /// pinch zoom causes scroll view to jump around
  /// </summary>
  public class trDisableScrollMulFinger : MonoBehaviour {
  	
    public ScrollRect ScrollView;
    private bool isEnabled = true;

  	// Update is called once per frame
  	void Update () {
      if(isEnabled && Input.touchCount >= 2){
        isEnabled = false;
        ScrollView.horizontal = false;
        ScrollView.vertical = false;
      }
      else if(!isEnabled && Input.touchCount == 0){
        isEnabled = true;
        ScrollView.horizontal = true;
        ScrollView.vertical = true;
      }
  	}
  }
}
